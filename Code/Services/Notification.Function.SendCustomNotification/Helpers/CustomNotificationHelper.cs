// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.Function.SendCustomNotifications.Helpers;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BL.Common;
using BL.Common.Extension;
using BL.Common.Helpers;
using BL.Common.Interface;
using Contract;
using Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Model.Model;
using Newtonsoft.Json;

public class CustomNotificationHelper : ICustomNotificationHelper
{
    private readonly IHttpHelper _httpHelper;
    private readonly ILogger _logger;
    private readonly string[] _functionUrls;

    public CustomNotificationHelper(IConfiguration config, IHttpHelper httpHelper, ILogger<NotificationHelper> logger)
    {
        _httpHelper = httpHelper;
        _logger = logger;
        _functionUrls = config[Constant.SendCustomWebhookEndpoint].Split(';');
    }

    /// <summary>
    /// Send Custom Notifications using the Notification payload
    /// </summary>
    /// <param name="notificationItem"></param>
    /// <returns></returns>
    public async Task<bool> ProcessCustomNotificationRequestsAsync(NotificationItem notificationItem)
    {
        LogData logData = new LogData()
        {
            EventDetails = new Dictionary<string, object>()
            {
                { Constant.BusinessProcessName, "Notification - Send Custom Notification" },
                { Constant.ActionUri, "ProcessCustomNotificationRequestsAsync" },
                { Constant.ComponentType, ComponentType.BackgroundProcess },
                { Constant.ApplicationName, $"{notificationItem.ApplicationName}" },
                { Constant.TenantIdentifier, $"{notificationItem.TenantIdentifier}" },
                { Constant.Xcv, $"{notificationItem.Telemetry?.Xcv}" },
                { Constant.MessageId, $"{notificationItem.Telemetry?.MessageId}" },
                { Constant.Id, $"{notificationItem.Id}" },
                { Constant.AppAction, "Notification - ProcessCustomNotificationRequestsAsync - Initiated"}
            }
        };

        try
        {
            using (_logger.BeginScope(logData.EventDetails))
            {
                _logger.LogInformation(new EventId((int)EventIds.SendCustomNotificationProcessing),
                    "Notification - ProcessCustomNotificationRequestsAsync - Initiated");
            }

            // Set default response
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);

            // Send request
            foreach (var endPoint in _functionUrls)
            {
                try
                {
                    response = await _httpHelper.SendRequestAsync(HttpMethod.Post, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, endPoint, JsonConvert.SerializeObject(notificationItem), null, false);

                    // Get response content
                    string content = await response.Content.ReadAsStringAsync();
                    logData.EventDetails.Modify(Constant.ResponseStatusCode, response.StatusCode);
                    logData.EventDetails.Modify(Constant.ResponseContent, content);

                    if (response.IsSuccessStatusCode)
                    {
                        // Log success
                        logData.EventDetails.Modify(Constant.AppAction, "Notification - ProcessCustomNotificationRequestsAsync - Success");
                        using (_logger.BeginScope(logData.EventDetails))
                        {
                            _logger.LogInformation(new EventId((int)EventIds.SendCustomNotificationSuccess),
                                "Notification - ProcessCustomNotificationRequestsAsync - Success");
                        }
                    }
                    else
                    {
                        // Log error
                        logData.EventDetails.Modify(Constant.AppAction, "Notification - ProcessCustomNotificationRequestsAsync - Failed");
                        using (_logger.BeginScope(logData.EventDetails))
                        {
                            _logger.LogInformation(new EventId((int)EventIds.SendCustomNotificationFailed),
                                "Notification - ProcessCustomNotificationRequestsAsync - Failed");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log error
                    logData.EventDetails.Modify(Constant.AppAction, "Notification - ProcessCustomNotificationRequestsAsync - Failed");
                    using (_logger.BeginScope(logData.EventDetails))
                    {
                        _logger.LogError(new EventId((int)EventIds.SendCustomNotificationError),
                            ex,
                            "Notification - ProcessCustomNotificationRequestsAsync - Failed");
                    }
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            // Log error
            logData.EventDetails.Modify(Constant.AppAction, "Notification - ProcessCustomNotificationRequestsAsync - Failed");
            using (_logger.BeginScope(logData.EventDetails))
            {
                _logger.LogError(new EventId((int)EventIds.SendCustomNotificationError),
                    ex,
                    "Notification - ProcessCustomNotificationRequestsAsync - Failed");
            }
            return false;
        }
    }
}