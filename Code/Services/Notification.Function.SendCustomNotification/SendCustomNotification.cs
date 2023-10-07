// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.Function.SendCustomNotifications;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using BL.Common;
using BL.Common.Extension;
using Contract;
using Interface;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Model.Model;
using Newtonsoft.Json;
using Notification.BL.Common.Interface;
using Notification.Data.Azure.Storage.Interface;

public class SendCustomNotification
{
    private readonly IConfiguration _config;
    private readonly ICustomNotificationHelper _customNotificationHelper;
    private readonly IUtilityHelper _utilityHelper;
    private readonly IBlobStorageHelper _blobStorageHelper;

    /// <summary>
    /// SendCustomNotification Initialization
    /// </summary>
    /// <param name="config">Configuration object</param>
    /// <param name="CustomNotificationHelper">Custom Notification Helper</param>
    /// <param name="utilityHelper">Utility Helper</param>
    /// <param name="blobStorageHelper">BlobStorage Helper</param>
    public SendCustomNotification(IConfiguration config, ICustomNotificationHelper customNotificationHelper, IUtilityHelper utilityHelper, IBlobStorageHelper blobStorageHelper)
    {
        _config = config;
        _customNotificationHelper = customNotificationHelper;
        _utilityHelper = utilityHelper;
        _blobStorageHelper = blobStorageHelper;
    }

    [FunctionName("SendCustomNotifications")]
    public async Task Run([ServiceBusTrigger("%CustomQueueName%", Connection = "ServiceBusNamespace")] ServiceBusMessage message, ILogger logger)
    {
        LogData logData = new LogData()
        {
            EventDetails = new Dictionary<string, object>()
            {
                { Constant.BusinessProcessName, "Notification - Send Custom Notification" },
                { Constant.ActionUri, "SendCustomNotification" },
                { Constant.ComponentType, ComponentType.BackgroundProcess }
            }
        };

        try
        {
            await SendCustomNotificationStartup.refresher.RefreshAsync();

            logData.EventDetails.Modify(Constant.AppAction, "Notification - Send Custom Notification - Initiated");
            using (logger.BeginScope(logData.EventDetails))
            {
                logger.LogInformation(new EventId((int)EventIds.SendCustomNotificationInitiated),
                    "Notification - Send Custom Notification - Initiated");
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

            await _customNotificationHelper.ProcessCustomNotificationRequestsAsync(notificationPayload);

            logData.EventDetails.Modify(Constant.AppAction, "Notification - Send Custom Notification - Complete");
            using (logger.BeginScope(logData.EventDetails))
            {
                logger.LogInformation(new EventId((int)EventIds.SendCustomNotificationComplete),
                    "Notification - Send Custom Notification - Complete");
            }
        }
        catch (Exception ex)
        {
            logData.EventDetails.Modify(Constant.AppAction, "Notification - Send Custom Notification - Failed - Exception");
            using (logger.BeginScope(logData.EventDetails))
            {
                logger.LogError(new EventId((int)EventIds.SendCustomNotificationError),
                    ex,
                    "Notification - Send Custom Notification - Failed - Exception");
            }
            throw;
        }
    }
}