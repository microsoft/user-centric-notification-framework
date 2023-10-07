// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.Function.SendPushNotifications.Helpers;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BL.Common;
using BL.Common.Extension;
using BL.Common.Interface;
using Contract;
using Interface;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Model.Model;
using Newtonsoft.Json;
using Notification.Data.Azure.Storage.Interface;
using WebPush;

public class PushNotificationHelper : IPushNotificationHelper
{
    private readonly IConfiguration _config;
    private readonly ILogger _logger;
    private readonly string _vapidPublicKey;
    private readonly string _vapidPrivateKey;
    private readonly ITableHelper _storageTableHelper;
    private readonly INotificationHelper _notificationHelper;
    private readonly NotificationHubClient _hub;

    public PushNotificationHelper(IConfiguration config, INotificationHelper notificationHelper, ILogger<PushNotificationHelper> logger, ITableHelper storageTableHelper)
    {
        _config = config;
        _logger = logger;
        _vapidPublicKey = _config[Constant.VapidPublicKey];
        _vapidPrivateKey = _config[Constant.VapidPrivateKey];
        _storageTableHelper = storageTableHelper;
        _hub = NotificationHubClient.CreateClientFromConnectionString(_config[Constant.NotificationHubDefaultFullSharedAccessSignature], _config[Constant.HubName]);
        _notificationHelper = notificationHelper;
    }

    /// <summary>
    /// Send Web Push Notification using the Notification payload
    /// </summary>
    /// <param name="notificationItem"></param>
    /// <returns></returns>
    public async Task<bool> ProcessWebPushNotificationRequestsAsync(NotificationItem notificationItem)
    {
        LogData logData = new LogData()
        {
            EventDetails = new Dictionary<string, object>()
            {
                { Constant.BusinessProcessName, "Notification - Send Web Push Notification" },
                { Constant.ActionUri, "ProcessWebPushNotificationRequestsAsync" },
                { Constant.ComponentType, ComponentType.BackgroundProcess },
                { Constant.ApplicationName, $"{notificationItem.ApplicationName}" },
                { Constant.TenantIdentifier, $"{notificationItem.TenantIdentifier}" },
                { Constant.Xcv, $"{notificationItem.Telemetry?.Xcv}" },
                { Constant.MessageId, $"{notificationItem.Telemetry?.MessageId}" },
                { Constant.Id, $"{notificationItem.Id}" },
                { Constant.AppAction, "Notification - ProcessWebPushNotificationRequestsAsync - Initiated" }
            }
        };
        try
        {
            using (_logger.BeginScope(logData.EventDetails))
            {
                _logger.LogInformation(new EventId((int)EventIds.SendWebPushNotificationInitiated),
                    "Notification - ProcessWebPushNotificationRequestsAsync - Initiated");
            }
            // Get keys from configuration table
            var subject = "mailto:" + notificationItem.To;
            var publicKey = _vapidPublicKey;
            var privateKey = _vapidPrivateKey;

            var vapidDetails = new VapidDetails(subject, publicKey, privateKey);

            // Construct the notification payload
            var payLoad = new
            {
                tenant = notificationItem.ApplicationName,
                body = notificationItem.Body,
                title = notificationItem.Subject,
                url = notificationItem.DeeplinkUrl,
                tag = notificationItem.WebPushNotificationTag,
                renotify = true
            };

            var options = new Dictionary<string, object>
                {
                    { Constant.VapidDetails, vapidDetails },
                    { Constant.TTL, 2419200 } // Default TTL is 28 days (2419200)
                };

            // Create the Web Push client
            var webPushClient = new WebPushClient();

            // Get the endpoints configured for the user
            var registrations = _storageTableHelper.GetTableEntityListByPartitionKey<WebPushNotificationRegistrationEntity>(_config[Constant.WebPushNotificationRegistrationTableName], notificationItem.To);

            // Iterate over the endpoints and send notification only to the valid ones
            foreach (var registration in registrations)
            {
                // Get the endpoints for the given approver from storage
                var pushEndpoint = registration.EndPoint;
                var p256dh = registration.P256DH;
                var auth = registration.Auth;

                var subscription = new PushSubscription(pushEndpoint, p256dh, auth);
                try
                {
                    // Send notifications using Web Push Client
                    webPushClient.SendNotification(subscription, JsonConvert.SerializeObject(payLoad), options);
                }
                catch (WebPushException exception)
                {
                    if (exception.StatusCode.Equals(System.Net.HttpStatusCode.Gone))
                    {
                        // Remove entry from db
                        await _storageTableHelper.DeleteRow(
                            _config[Constant.WebPushNotificationRegistrationTableName], registration);
                    }

                    // Log error
                    logData.EventDetails.Modify(Constant.AppAction,
                        "Notification - ProcessWebPushNotificationRequestsAsync - Failed");
                    using (_logger.BeginScope(logData.EventDetails))
                    {
                        _logger.LogInformation(new EventId((int)EventIds.SendWebPushNotificationFailed),
                            "Notification - ProcessWebPushNotificationRequestsAsync - Failed");
                    }
                }
                catch (Exception)
                {
                    // Log error
                    logData.EventDetails.Modify(Constant.AppAction,
                        "Notification - ProcessWebPushNotificationRequestsAsync - Failed");
                    using (_logger.BeginScope(logData.EventDetails))
                    {
                        _logger.LogInformation(new EventId((int)EventIds.SendWebPushNotificationFailed),
                            "Notification - ProcessWebPushNotificationRequestsAsync - Failed");
                    }
                }
            }
            // Log success
            logData.EventDetails.Modify(Constant.AppAction, "Notification - ProcessWebPushNotificationRequestsAsync - Success");
            using (_logger.BeginScope(logData.EventDetails))
            {
                _logger.LogInformation(new EventId((int)EventIds.SendWebPushNotificationSuccess),
                    "Notification - ProcessWebPushNotificationRequestsAsync - Success");
            }
            return true;
        }
        catch (Exception ex)
        {
            // Log error
            logData.EventDetails.Modify(Constant.AppAction, "Notification - ProcessWebPushNotificationRequestsAsync - Failed");
            using (_logger.BeginScope(logData.EventDetails))
            {
                _logger.LogError(new EventId((int)EventIds.SendWebPushNotificationError),
                    ex,
                "Notification - ProcessWebPushNotificationRequestsAsync - Failed");
            }
            return false;
        }
    }

    /// <summary>
    /// Send Device Push Notification using the Notification payload
    /// </summary>
    /// <param name="notificationItem"></param>
    /// <returns></returns>
    public async Task ProcessDevicePushNotificationRequestsAsync(NotificationItem notificationItem)
    {
        LogData logData = new LogData()
        {
            EventDetails = new Dictionary<string, object>()
            {
                { Constant.BusinessProcessName, "Notification - Send Email Notification" },
                { Constant.ActionUri, "ProcessDevicePushNotificationRequestsAsync" },
                { Constant.ComponentType, ComponentType.BackgroundProcess }
            }
        };

        foreach (var notificationType in notificationItem.NotificationTypes)
        {
            try
            {
                // Get the templates configured for the device type
                var templateContent = _storageTableHelper.GetTableEntityListByPartitionKey<DeviceNotificationTemplates>(_config[Constant.DeviceNotificationTemplatesTableName], notificationType.ToString());
                foreach (var item in templateContent)
                {
                    var payload = _notificationHelper.Replace(null, notificationItem.TemplateData, item.TemplateContent);
                    switch (item.RowKey)
                    {
                        case "wns":
                            // Windows 8.1 / Windows Phone 8.1 / Windows 10
                            NotificationOutcome wnsOutcome = await _hub.SendWindowsNativeNotificationAsync(payload, notificationItem.To + notificationType);

                            // Log success
                            logData.EventDetails.Modify(Constant.AppAction, "Notification - ProcessDevicePushNotificationRequestsAsync - wns - Success");
                            using (_logger.BeginScope(logData.EventDetails))
                            {
                                _logger.LogInformation(new EventId((int)EventIds.SendDevicePushNotificationSuccess),
                                    "Notification - ProcessDevicePushNotificationRequestsAsync - wns - Success");
                            }
                            break;

                        case "apns":
                            // iOS
                            NotificationOutcome apnsOutcome = await _hub.SendAppleNativeNotificationAsync(payload, notificationItem.To + notificationType);

                            // Log success
                            logData.EventDetails.Modify(Constant.AppAction, "Notification - ProcessDevicePushNotificationRequestsAsync - apns - Success");
                            using (_logger.BeginScope(logData.EventDetails))
                            {
                                _logger.LogInformation(new EventId((int)EventIds.SendDevicePushNotificationSuccess),
                                    "Notification - ProcessDevicePushNotificationRequestsAsync - apns - Success");
                            }
                            break;

                        case "fcm":
                            // Android
                            NotificationOutcome fcmOutcome = await _hub.SendFcmNativeNotificationAsync(payload, notificationItem.To + notificationType);

                            // Log success
                            logData.EventDetails.Modify(Constant.AppAction, "Notification - ProcessDevicePushNotificationRequestsAsync - fcm - Success");
                            using (_logger.BeginScope(logData.EventDetails))
                            {
                                _logger.LogInformation(new EventId((int)EventIds.SendDevicePushNotificationSuccess),
                                    "Notification - ProcessDevicePushNotificationRequestsAsync - fcm - Success");
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error
                logData.EventDetails.Modify(Constant.AppAction, "Notification - ProcessDevicePushNotificationRequestsAsync - Failed");
                using (_logger.BeginScope(logData.EventDetails))
                {
                    _logger.LogError(new EventId((int)EventIds.SendDevicePushNotificationError),
                        ex,
                        "Notification - ProcessDevicePushNotificationRequestsAsync - Failed");
                }
            }
        }

        // Log success
        logData.EventDetails.Modify(Constant.AppAction, "Notification - ProcessDevicePushNotificationRequestsAsync - Complete");
        using (_logger.BeginScope(logData.EventDetails))
        {
            _logger.LogInformation(new EventId((int)EventIds.SendDevicePushNotificationCompleted),
                "Notification - ProcessDevicePushNotificationRequestsAsync - Complete");
        }
    }
}