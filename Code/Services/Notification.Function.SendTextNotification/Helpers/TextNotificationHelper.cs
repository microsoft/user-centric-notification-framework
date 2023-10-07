// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.Function.SendTextNotification.Helpers;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BL.Common;
using BL.Common.Extension;
using BL.Common.Interface;
using Contract;
using Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Model.Model;
using Newtonsoft.Json;

public class TextNotificationHelper : ITextNotificationHelper
{
    private readonly IConfiguration _config;
    private readonly IHttpHelper _httpHelper;
    private readonly ILogger _logger;
    private readonly string _functionUrl;

    public TextNotificationHelper(IConfiguration config, IHttpHelper httpHelper, ILogger<TextNotificationHelper> logger)
    {
        _config = config;
        _httpHelper = httpHelper;
        _logger = logger;
        _functionUrl = _config[Constant.SendTextWebhookEndpoint];
    }

    public async Task<bool> ProcessTextNotificationRequestsAsync(NotificationItem notificationItem)
    {
        LogData logData = new LogData()
        {
            EventDetails = new Dictionary<string, object>()
            {
                { Constant.BusinessProcessName, "Notification - Send Text Notification" },
                { Constant.ActionUri, "ProcessTextNotificationRequestsAsync" },
                { Constant.ComponentType, ComponentType.BackgroundProcess },
                { Constant.ApplicationName, $"{notificationItem.ApplicationName}" },
                { Constant.TenantIdentifier, $"{notificationItem.TenantIdentifier}" },
                { Constant.Xcv, $"{notificationItem.Telemetry?.Xcv}" },
                { Constant.MessageId, $"{notificationItem.Telemetry?.MessageId}" },
                { Constant.Id, $"{notificationItem.Id}" },
                { Constant.AppAction, "Notification - ProcessTextNotificationRequestsAsync - Initiated"}
            }
        };

        try
        {
            using (_logger.BeginScope(logData.EventDetails))
            {
                _logger.LogInformation(new EventId((int)EventIds.SendTextNotificationProcessing),
                    "Notification - ProcessTextNotificationRequestsAsync - Initiated");
            }

            // Set default response
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);

            // Send request
            response = await _httpHelper.SendRequestAsync(HttpMethod.Post, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, _functionUrl, JsonConvert.SerializeObject(notificationItem));

            // Get response content
            string content = await response.Content.ReadAsStringAsync();
            logData.EventDetails.Modify(Constant.ResponseStatusCode, response.StatusCode);
            logData.EventDetails.Modify(Constant.ResponseContent, content);

            if (response.IsSuccessStatusCode)
            {
                // Log success
                logData.EventDetails.Modify(Constant.AppAction, "Notification - ProcessTextNotificationRequestsAsync - Success");
                using (_logger.BeginScope(logData.EventDetails))
                {
                    _logger.LogInformation(new EventId((int)EventIds.SendTextNotificationSuccess),
                        "Notification - ProcessTextNotificationRequestsAsync - Success");
                }
            }
            else
            {
                // Log error
                logData.EventDetails.Modify(Constant.AppAction, "Notification - ProcessTextNotificationRequestsAsync - Failed");
                using (_logger.BeginScope(logData.EventDetails))
                {
                    _logger.LogInformation(new EventId((int)EventIds.SendTextNotificationFailed),
                        "Notification - ProcessTextNotificationRequestsAsync - Failed");
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            // Log error
            logData.EventDetails.Modify(Constant.AppAction, "Notification - ProcessTextNotificationRequestsAsync - Failed");
            using (_logger.BeginScope(logData.EventDetails))
            {
                _logger.LogError(new EventId((int)EventIds.SendTextNotificationError),
                    ex,
                    "Notification - ProcessTextNotificationRequestsAsync - Failed");
            }
            return false;
        }
    }
}