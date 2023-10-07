// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.Function.SendEmail;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using BL.Common;
using BL.Common.Extension;
using Contract;
using Data.Azure.Storage.Interface;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Model.Model;
using Newtonsoft.Json;
using Notification.BL.Common.Interface;
using Notification.BL.SendEmail.Interfaces;

public class SendEmailNotification
{
    private readonly IEmailNotificationHelper _emailNotificationHelper;
    private readonly IUtilityHelper _utilityHelper;
    private readonly IBlobStorageHelper _blobStorageHelper;
    private static readonly SemaphoreSlim _syncObject = new SemaphoreSlim(1);

    /// <summary>
    /// SendEmailNotification Initialization
    /// </summary>
    /// <param name="emailNotificationHelper">Email Notification Helper</param>
    /// <param name="utilityHelper">Utility Helper</param>
    /// <param name="blobStorageHelper">BlobStorage Helper</param>
    public SendEmailNotification(IEmailNotificationHelper emailNotificationHelper, IUtilityHelper utilityHelper, IBlobStorageHelper blobStorageHelper)
    {
        _emailNotificationHelper = emailNotificationHelper;
        _utilityHelper = utilityHelper;
        _blobStorageHelper = blobStorageHelper;
    }

    [FunctionName("SendEmailNotification")]
    public async Task Run([ServiceBusTrigger("%MailQueueName%", Connection = "ServiceBusNamespace")] ServiceBusReceivedMessage message, ILogger logger)
    {
        LogData logData = new LogData()
        {
            EventDetails = new Dictionary<string, object>()
            {
                { Constant.BusinessProcessName, "Notification - Send Email Notification" },
                { Constant.ActionUri, "SendEmailNotification" },
                { Constant.ComponentType, ComponentType.BackgroundProcess }
            }
        };

        try
        {
            await _syncObject.WaitAsync();

            await SendEmailStartup.refresher.RefreshAsync();

            logData.EventDetails.Modify(Constant.AppAction, "Notification - Send Email Notification - Initiated");
            using (logger.BeginScope(logData.EventDetails))
            {
                logger.LogInformation(new EventId((int)EventIds.SendEmailNotificationInitiated),
                    "Notification - Send Email Notification - Initiated");
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

            await _emailNotificationHelper.ProcessEmailNotificationRequestsAsync(notificationPayload);

            logData.EventDetails.Modify(Constant.AppAction, "Notification - Send Email Notification - Complete");
            using (logger.BeginScope(logData.EventDetails))
            {
                logger.LogInformation(new EventId((int)EventIds.SendEmailNotificationComplete),
                    "Notification - Send Email Notification - Complete");
            }
        }
        catch (Exception ex)
        {
            logData.EventDetails.Modify(Constant.AppAction, "Notification - Send Email Notification - Failed - Exception");
            using (logger.BeginScope(logData.EventDetails))
            {
                logger.LogError(new EventId((int)EventIds.SendEmailNotificationError),
                    ex,
                    "Notification - Send Email Notification - Failed - Exception");
            }
            throw;
        }
        finally
        {
            _syncObject.Release();
        }
    }
}