// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.Function.SendReminders;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using BL.Common;
using BL.Common.Extension;
using BL.Common.Interface;
using Contract;
using Data.Azure.Storage.Interface;
using Interface;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Model.Model;
using Newtonsoft.Json;

public class SendRemindersNotification
{
    private readonly IReminderNotificationHelper _reminderNotificationHelper;
    private readonly IUtilityHelper _utilityHelper;
    private readonly IBlobStorageHelper _blobStorageHelper;
    private static readonly SemaphoreSlim _syncObject = new SemaphoreSlim(1);

    /// <summary>
    /// SendRemindersNotification Initialization
    /// </summary>
    /// <param name="reminderNotificationHelper">Reminder Notification Helper</param>
    /// <param name="utilityHelper">Utility Helper</param>
    public SendRemindersNotification(IReminderNotificationHelper reminderNotificationHelper, IUtilityHelper utilityHelper, IBlobStorageHelper blobStorageHelper)
    {
        _reminderNotificationHelper = reminderNotificationHelper;
        _utilityHelper = utilityHelper;
        _blobStorageHelper = blobStorageHelper;
    }

    [FunctionName("SendReminderNotification")]
    public async Task Run([ServiceBusTrigger("%ReminderQueueName%", Connection = "ServiceBusNamespace")] ServiceBusMessage message, ILogger logger)
    {
        LogData logData = new LogData()
        {
            EventDetails = new Dictionary<string, object>()
            {
                { Constant.BusinessProcessName, "Notification - Send Reminder Notification" },
                { Constant.ActionUri, "SendReminderNotification" },
                { Constant.ComponentType, ComponentType.BackgroundProcess }
            }
        };

        try
        {
            await _syncObject.WaitAsync();

            await SendRemindersStartup.refresher.RefreshAsync();

            logData.EventDetails.Modify(Constant.AppAction, "Notification - Send Reminder Notification - Initiated");
            using (logger.BeginScope(logData.EventDetails))
            {
                logger.LogInformation(new EventId((int)EventIds.SendReminderNotificationInitiated),
                    "Notification - Send Reminder Notification - Initiated");
            }

            var tenantId = message.ApplicationProperties["tenantId"].ToString();
            var blobName = $"{tenantId}/{message.MessageId}";

            byte[] payload;
            bool isDataInBlob = (bool)message.ApplicationProperties["IsDataInBlob"];
            if (isDataInBlob)
            {
                payload = await _blobStorageHelper.DownloadByteArray(Constant.AttachmentsBlobContainer, blobName);
            }
            else
            {
                payload = message.Body.ToArray();
            }

            // Decompress message before deserializing
            string decompressedPayload = await _utilityHelper.DecompressMessage(payload);

            var notificationPayload = JsonConvert.DeserializeObject<NotificationItem>(decompressedPayload);

            logData.EventDetails.Modify(Constant.ApplicationName, $"{notificationPayload.ApplicationName}");
            logData.EventDetails.Modify(Constant.TenantIdentifier, $"{notificationPayload.TenantIdentifier}");
            logData.EventDetails.Modify(Constant.Xcv, $"{notificationPayload.Telemetry?.Xcv}");
            logData.EventDetails.Modify(Constant.MessageId, $"{notificationPayload.Telemetry?.MessageId}");
            logData.EventDetails.Modify(Constant.Id, $"{notificationPayload.Id}");

            await _reminderNotificationHelper.ProcessReminderNotificationRequestsAsync(notificationPayload);

            logData.EventDetails.Modify(Constant.AppAction, "Notification - Send Reminder Notification - Complete");
            using (logger.BeginScope(logData.EventDetails))
            {
                logger.LogInformation(new EventId((int)EventIds.SendReminderNotificationComplete),
                    "Notification - Send Reminder Notification - Complete");
            }
        }
        catch (Exception ex)
        {
            logData.EventDetails.Modify(Constant.AppAction, "Notification - Send Reminder Notification - Failed - Exception");
            using (logger.BeginScope(logData.EventDetails))
            {
                logger.LogError(new EventId((int)EventIds.SendReminderNotificationError),
                    ex,
                    "Notification - Send Reminder Notification - Failed - Exception");
            }
            throw;
        }
        finally
        {
            _syncObject.Release();
        }
    }
}