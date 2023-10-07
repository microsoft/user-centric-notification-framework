// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.BL.Common.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using AdaptiveCards.Templating;
    using Azure.Data.Tables;
    using Azure.Identity;
    using Azure.Messaging.ServiceBus;
    using Contract;
    using Data.Azure.Storage.Interface;
    using Extension;
    using Interface;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Model.Model;
    using Newtonsoft.Json;

    public class NotificationHelper : INotificationHelper
    {
        #region Variables

        private readonly IConfiguration _config;
        private readonly IUtilityHelper _utilityHelper;
        private readonly ILogger _logger;
        private readonly ITableHelper _tableStorageHelper;
        private readonly IBlobStorageHelper _blobStorageHelper;
        private static ServiceBusClient _serviceBusClient;
        private static readonly SemaphoreSlim _syncObject = new SemaphoreSlim(1);

        #endregion Variables

        #region Constructor

        public NotificationHelper(IConfiguration config, IUtilityHelper utilityHelper, ITableHelper tableStorageHelper, IBlobStorageHelper blobStorageHelper, ILogger<NotificationHelper> logger)
        {
            _config = config;
            _utilityHelper = utilityHelper;
            _logger = logger;
            _tableStorageHelper = tableStorageHelper;
            _blobStorageHelper = blobStorageHelper;
            _serviceBusClient = new ServiceBusClient(_config[Constant.ServiceBusNamespace], new DefaultAzureCredential());
        }

        #endregion Constructor

        #region Public Methods

        /// <summary>
        /// Sends Notification message to different queues based on the notification type
        /// </summary>
        /// <param name="item">notification item</param>
        /// <returns>Returns true if broadcast is successful</returns>
        public async Task<NotificationResponse> BroadcastNotificationsToProviders(NotificationItem item)
        {
            bool isSucccessfullySent;
            Dictionary<string, string> failedQueuesInformation;
            List<string> queueNames = new List<string>();

            foreach (var notificationItem in item.NotificationTypes)
            {
                switch (notificationItem)
                {
                    case NotificationType.ActionableEmail:
                    case NotificationType.Mail:
                        queueNames.Add(_config[Constant.MailQueueName]);
                        break;

                    case NotificationType.Badge:
                    case NotificationType.Raw:
                    case NotificationType.Toast:
                    case NotificationType.Tile:
                        queueNames.Add(_config[Constant.DevicePushQueueName]);
                        break;

                    case NotificationType.WebPush:
                        queueNames.Add(_config[Constant.WebPushQueueName]);
                        break;

                    case NotificationType.Text:
                        queueNames.Add(_config[Constant.TextQueueName]);
                        break;

                    case NotificationType.Cancel:
                        (isSucccessfullySent, failedQueuesInformation) = await CancelScheduledMessageAsync(Constant.ReminderQueueName, item);
                        break;
                }
            }

            (isSucccessfullySent, failedQueuesInformation) = await PushMessageToQueueAsync(queueNames, item);

            return FormulateResponse(item, isSucccessfullySent, failedQueuesInformation);
        }

        /// <summary>
        /// Replaces the placeholders in template content with template data
        /// </summary>
        /// <param name="notificationTypes">list of notificationtype</param>
        /// <param name="templateData">template data</param>
        /// <param name="templateContent">template content</param>
        /// <returns>Updated template content (string) without placeholders</returns>
        public string Replace(List<NotificationType> notificationTypes, dynamic templateData, dynamic templateContent)
        {
            string output = templateContent;
            string data = Convert.ToString(templateData);
            if (null != notificationTypes)
            {
                foreach (NotificationType notificationType in notificationTypes)
                {
                    if (notificationType.Equals(NotificationType.ActionableEmail))
                    {
                        if (data != null && output != null)
                        {
                            data = data.Replace("null", "\"\"");
                            AdaptiveCardTemplate transformer = new AdaptiveCardTemplate(templateContent);
                            var ctx = new EvaluationContext
                            {
                                Root = data
                            };
                            string jsonstr = transformer.Expand(ctx);
                            output = Regex.Unescape(jsonstr).Trim('"');
                        }
                    }
                }
            }

            if (data != null && output != null)
            {
                foreach (KeyValuePair<string, string> replaceable in JsonConvert.DeserializeObject<Dictionary<string, string>>(data))
                {
                    output = output.Replace("#" + replaceable.Key + "#", replaceable.Value);
                }
            }
            return output;
        }

        /// <summary>
        /// Log Notification Status.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="tableName"></param>
        /// <returns>Table operation result.</returns>
        public async Task<bool> LogNotificationStatus<T>(T status, string tableName) where T : class, ITableEntity, new()
        {
            LogData logData = new LogData()
            {
                EventDetails = new Dictionary<string, object>()
                {
                    { Constant.BusinessProcessName, "Notification - Send Notification" },
                    { Constant.ActionUri, "LogNotificationStatus" },
                    { Constant.ComponentType, ComponentType.BackgroundProcess },
                    { Constant.NotificationStatusTableName, tableName }
                }
            };

            try
            {
                logData.EventDetails.Modify(Constant.Xcv, $"{status.PartitionKey}");

                return await _tableStorageHelper.Insert(tableName, status);
            }
            catch (Exception ex)
            {
                logData.EventDetails.Modify(Constant.AppAction, "Notification - LogNotificationStatus - Failed");
                using (_logger.BeginScope(logData.EventDetails))
                {
                    _logger.LogError(new EventId((int)EventIds.NotificationStatusUpdateFailure),
                        ex,
                        "Notification - LogNotificationStatus - Failed");
                }
                return false;
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Pushes the message to each of the service bus queue
        /// </summary>
        /// <param name="queueNames">queue name</param>
        /// <param name="item">notification item</param>
        /// <returns>a task</returns>
        private async Task<(bool IsSuccess, Dictionary<string, string> FailedQueuesInformation)> PushMessageToQueueAsync(List<string> queueNames, NotificationItem item)
        {
            bool isSucccessfullySent = true;
            Dictionary<string, string> failedQueuesInformation = new Dictionary<string, string>();

            LogData logData = new LogData()
            {
                EventDetails = new Dictionary<string, object>()
                    {
                        { Constant.BusinessProcessName, "Notification - Broadcaster" },
                        { Constant.ActionUri, "PushMessageToQueueAsync" },
                        { Constant.ComponentType, ComponentType.BackgroundProcess },
                        { Constant.ApplicationName, $"{item.ApplicationName}" },
                        { Constant.TenantIdentifier, $"{item.TenantIdentifier}" },
                        { Constant.Id, $"{item.Id}" },
                        { Constant.Xcv, $"{item.Telemetry?.Xcv}" },
                        { Constant.MessageId, $"{item.Telemetry?.MessageId}" }
                    }
            };
            try
            {
                if (queueNames.Count > 0)
                {
                    // Create messages
                    ServiceBusMessage message = new ServiceBusMessage
                    {
                        MessageId = string.IsNullOrWhiteSpace(item.Telemetry?.MessageId) ? Guid.NewGuid().ToString() : $"{item.Telemetry?.MessageId}_{Guid.NewGuid()}",
                        SessionId = string.IsNullOrWhiteSpace(item.Telemetry?.Xcv) ? Guid.NewGuid().ToString() : item.Telemetry?.Xcv
                    };

                    if (item.SendOnUtcDate != null)
                    {
                        message.ScheduledEnqueueTime = (DateTime)item.SendOnUtcDate;
                    }
                    message.ApplicationProperties["tenantId"] = string.IsNullOrWhiteSpace(item.TenantIdentifier) ? "root" : item.TenantIdentifier;
                    message.ApplicationProperties["IsDataInBlob"] = true;

                    var tenantId = item.TenantIdentifier;
                    item.AttachmentBlobName = $"{tenantId}/{message.MessageId}";

                    var compressedBody = _utilityHelper.CompressMessage(JsonConvert.SerializeObject(item));
                    await _blobStorageHelper.UploadByteArray(compressedBody, Constant.AttachmentsBlobContainer, item.AttachmentBlobName);

                    foreach (var queue in queueNames)
                    {
                        (isSucccessfullySent, item.SequenceNumber) = await SendMessage(failedQueuesInformation, logData, message, queue);
                    }

                    // Add to reminder queue if reminder notifications are applicable
                    if (item.Reminder != null && item.Reminder.NextReminderDate > DateTime.MinValue && item.Reminder.NextReminderDate <= item.Reminder.ExpirationDate)
                    {
                        message.ScheduledEnqueueTime = item.Reminder.NextReminderDate;
                        (isSucccessfullySent, item.SequenceNumber) = await SendMessage(failedQueuesInformation, logData, message, Constant.ReminderQueueName);
                    }
                    else
                    {
                        logData.EventDetails.Modify(Constant.AppAction, "Notification - Broadcaster - PushMessageToQueueAsync - Reminder - Ignore");
                        logData.EventDetails.Modify(Constant.Request, JsonConvert.SerializeObject(item.Reminder));
                        using (_logger.BeginScope(logData.EventDetails))
                        {
                            _logger.LogInformation(new EventId((int)EventIds.NotificationBroadcasterIgnore),
                                "Notification - Broadcaster - PushMessageToQueueAsync - Reminder - Ignore");
                        }
                    }
                }
                else
                {
                    logData.EventDetails.Modify(Constant.AppAction, "Notification - Broadcaster - PushMessageToQueueAsync - Ignore");
                    using (_logger.BeginScope(logData.EventDetails))
                    {
                        _logger.LogInformation(new EventId((int)EventIds.NotificationBroadcasterIgnore),
                            "Notification - Broadcaster - PushMessageToQueueAsync - Ignore");
                    }
                }
            }
            catch (Exception ex)
            {
                logData.EventDetails.Modify(Constant.AppAction,
                           "Notification - Broadcaster - PushMessageToQueueAsync - Failed - Exception");
                using (_logger.BeginScope(logData.EventDetails))
                {
                    _logger.LogError(new EventId((int)EventIds.NotificationBroadcasterError),
                        ex,
                        "Notification - Broadcaster - PushMessageToQueueAsync - Failed - Exception");
                }
                isSucccessfullySent = false;
            }

            return (isSucccessfullySent, failedQueuesInformation);
        }

        /// <summary>
        /// Cancel sending the scheduled message to the ServiceBus queue
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private async Task<(bool IsSuccess, Dictionary<string, string> FailedQueuesInformation)> CancelScheduledMessageAsync(string queue, NotificationItem item)
        {
            bool isSucccessfullySent = true;
            Dictionary<string, string> failedQueuesInformation = new Dictionary<string, string>();
            LogData logData = new LogData()
            {
                EventDetails = new Dictionary<string, object>()
                    {
                        { Constant.BusinessProcessName, "Notification - Broadcaster" },
                        { Constant.ActionUri, "CancelScheduledMessageAsync" },
                        { Constant.ComponentType, ComponentType.BackgroundProcess },
                        { Constant.ApplicationName, $"{item.ApplicationName}" },
                        { Constant.TenantIdentifier, $"{item.TenantIdentifier}" },
                        { Constant.Id, $"{item.Id}" },
                        { Constant.Xcv, $"{item.Telemetry?.Xcv}" },
                        { Constant.MessageId, $"{item.Telemetry?.MessageId}" }
                    }
            };

            try
            {
                await _syncObject.WaitAsync();

                if (item.SequenceNumber > 0)
                {
                    // Cancel sending the scheduled message to the queue
                    await _serviceBusClient.CreateSender(queue).CancelScheduledMessageAsync(item.SequenceNumber);
                }
            }
            catch (Exception ex)
            {
                logData.EventDetails.Modify(Constant.AppAction,
                    "Notification - Broadcaster - PushMessageToQueueAsync - Failed - Exception");
                using (_logger.BeginScope(logData.EventDetails))
                {
                    _logger.LogError(new EventId((int)EventIds.NotificationBroadcasterError),
                        ex,
                        "Notification - Broadcaster - PushMessageToQueueAsync - Failed - Exception");
                }
                failedQueuesInformation.Add(queue, ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                isSucccessfullySent = false;
            }
            finally
            {
                _syncObject.Release();
            }

            return (isSucccessfullySent, failedQueuesInformation);
        }

        /// <summary>
        /// Sends the message to the ServiceBus queue
        /// </summary>
        /// <param name="failedQueuesInformation"></param>
        /// <param name="logData"></param>
        /// <param name="message"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        private async Task<(bool IsSuccess, long SequenceNumber)> SendMessage(Dictionary<string, string> failedQueuesInformation, LogData logData, ServiceBusMessage message, string queue)
        {
            bool isSucccessfullySent = true;
            long sequenceNumber = 0;
            try
            {
                await _syncObject.WaitAsync();
                var serviceBusSender = _serviceBusClient.CreateSender(queue);

                logData.EventDetails.Modify(Constant.QueueName, $"{queue}");

                if (message != null && message.ScheduledEnqueueTime > DateTime.MinValue)
                {
                    // Send the message to the queue at the scheduled time
                    sequenceNumber = await serviceBusSender.ScheduleMessageAsync(message, message.ScheduledEnqueueTime);
                }
                else
                {
                    // Send the message to the queue
                    await serviceBusSender.SendMessageAsync(message);
                }
            }
            catch (Exception ex)
            {
                logData.EventDetails.Modify(Constant.AppAction,
                    "Notification - Broadcaster - PushMessageToQueueAsync - Failed - Exception");
                using (_logger.BeginScope(logData.EventDetails))
                {
                    _logger.LogError(new EventId((int)EventIds.NotificationBroadcasterError),
                        ex,
                        "Notification - Broadcaster - PushMessageToQueueAsync - Failed - Exception");
                }
                failedQueuesInformation.Add(queue, ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                isSucccessfullySent = false;
            }
            finally
            {
                _syncObject.Release();
            }

            return (isSucccessfullySent, sequenceNumber);
        }

        /// <summary>
        /// Formulates the response
        /// </summary>
        /// <param name="item">notification item</param>
        /// <param name="isSucccessfullySent">true if message successfully sent</param>
        /// <param name="failedQueueNames">list of queue names where message sending failed</param>
        /// <returns>NotificationResponse object</returns>
        private NotificationResponse FormulateResponse(NotificationItem item, bool isSucccessfullySent, Dictionary<string, string> failedQueueNames)
        {
            var response = new NotificationResponse
            {
                TenantIdentifier = item.TenantIdentifier,
                ActionResult = isSucccessfullySent,
                Telemetry = new Telemetry()
                {
                    MessageId = item.Telemetry?.MessageId,
                    Xcv = item.Telemetry?.Xcv
                },
                DisplayMessage = isSucccessfullySent
                    ? "Message sent successfully"
                    : "Failed to send the message. Please check details retry after some time.",
                SequenceNumber = item.SequenceNumber,
                E2EErrorInformation = isSucccessfullySent
                    ? null
                    : new E2EErrorInformation()
                    {
                        ErrorMessages = failedQueueNames.Count > 0
                        ? new List<string>
                        {
                            $"Failed to send message to: {JsonConvert.SerializeObject(failedQueueNames)}"
                        }
                        : new List<string>
                        {
                            $"Failed to send message to all queues"
                        }
                    }
            };

            return response;
        }

        #endregion Private Methods
    }
}