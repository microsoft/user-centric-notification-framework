// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.Function.SendReminders.Helpers;

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

public class ReminderNotificationHelper : IReminderNotificationHelper
{
    private readonly IHttpHelper _httpHelper;
    private readonly ILogger _logger;
    private readonly string _functionUrl;
    private readonly INotificationHelper _notificationHelper;

    public ReminderNotificationHelper(IConfiguration config, IHttpHelper httpHelper, ILogger<NotificationHelper> logger, INotificationHelper notificationHelper)
    {
        _httpHelper = httpHelper;
        _logger = logger;
        _functionUrl = config[Constant.SendNotificationUrl];
        _notificationHelper = notificationHelper;
    }

    /// <summary>
    /// Send Reminder Notifications using the Notification payload
    /// </summary>
    /// <param name="notificationItem"></param>
    /// <returns></returns>
    public async Task<bool> ProcessReminderNotificationRequestsAsync(NotificationItem notificationItem)
    {
        LogData logData = new LogData()
        {
            EventDetails = new Dictionary<string, object>()
            {
                { Constant.BusinessProcessName, "Notification - Send Reminder Notification" },
                { Constant.ActionUri, "ProcessReminderNotificationRequestsAsync" },
                { Constant.ComponentType, ComponentType.BackgroundProcess },
                { Constant.ApplicationName, $"{notificationItem.ApplicationName}" },
                { Constant.TenantIdentifier, $"{notificationItem.TenantIdentifier}" },
                { Constant.Xcv, $"{notificationItem.Telemetry?.Xcv}" },
                { Constant.MessageId, $"{notificationItem.Telemetry?.MessageId}" },
                { Constant.Id, $"{notificationItem.Id}" }
            }
        };

        try
        {
            logData.EventDetails.Modify(Constant.AppAction, "Notification - ProcessReminderNotificationRequestsAsync - Initiated");
            using (_logger.BeginScope(logData.EventDetails))
            {
                _logger.LogInformation(new EventId((int)EventIds.SendReminderNotificationProcessing),
                    "Notification - ProcessReminderNotificationRequestsAsync - Initiated");
            }

            // Set default response
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);

            // Update payload to send only specific notifications for reminders
            notificationItem.NotificationTypes = notificationItem.Reminder.NotificationTypes;
            notificationItem.Subject = !notificationItem.Subject.StartsWith("Reminder") ? $"Reminder: {notificationItem.Subject}" : notificationItem.Subject;

            // Send request
            response = await _httpHelper.SendRequestAsync(HttpMethod.Post, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, _functionUrl, JsonConvert.SerializeObject(notificationItem));

            // Get response content
            string content = await response.Content.ReadAsStringAsync();
            logData.EventDetails.Modify(Constant.ResponseStatusCode, response.StatusCode);
            logData.EventDetails.Modify(Constant.ResponseContent, content);

            if (response.IsSuccessStatusCode)
            {
                var notificationResponse = JsonConvert.DeserializeObject<NotificationResponse>(content);

                // Insert notification response in NotificationStatus table
                var notificationStatus = new NotificationStatus
                {
                    PartitionKey = notificationItem.Id,
                    RowKey = notificationItem.Telemetry.MessageId,
                    ActionResult = notificationResponse.ActionResult,
                    TenantIdentifier = notificationResponse.TenantIdentifier,
                    MessageId = notificationItem.Telemetry.MessageId,
                    Xcv = notificationResponse.Telemetry.Xcv,
                    SequenceNumber = notificationResponse.SequenceNumber
                };

                await _notificationHelper.LogNotificationStatus<NotificationStatus>(notificationStatus, Constant.NotificationStatusTableName);

                logData.EventDetails.Modify(Constant.AppAction, "Notification - ProcessReminderNotificationRequestsAsync - Success");
                using (_logger.BeginScope(logData.EventDetails))
                {
                    _logger.LogInformation(new EventId((int)EventIds.SendReminderNotificationSuccess),
                        "Notification - ProcessReminderNotificationRequestsAsync - Success");
                }
                return true;
            }
            else
            {
                // Log error
                logData.EventDetails.Modify(Constant.AppAction, "Notification - ProcessReminderNotificationRequestsAsync - Failed");
                using (_logger.BeginScope(logData.EventDetails))
                {
                    _logger.LogError(new EventId((int)EventIds.SendReminderNotificationFailed),
                        new InvalidOperationException(content),
                        "Notification - ProcessReminderNotificationRequestsAsync - Failed");
                }
                return false;
            }
        }
        catch (Exception ex)
        {
            // Log error
            logData.EventDetails.Modify(Constant.AppAction, "Notification - ProcessReminderNotificationRequestsAsync - Failed");
            using (_logger.BeginScope(logData.EventDetails))
            {
                _logger.LogError(new EventId((int)EventIds.SendReminderNotificationError),
                    ex,
                    "Notification - ProcessReminderNotificationRequestsAsync - Failed");
            }
            return false;
        }
    }
}