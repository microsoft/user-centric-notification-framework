// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.Function.CommonServices;

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BL.Common;
using BL.Common.Extension;
using BL.Common.Interface;
using Contract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Notification.Function.CommonServices.Utils;
using Notification.Model.Model;

/// <summary>
/// This will convert the notification payload and broadcast it to various providers (email/text etc.)
/// </summary>
public class NotificationBroadcasterFunction
{
    private static readonly Regex AliasRegex =
        new("^[A-Za-z0-9][A-Za-z0-9._-]{0,63}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly INotificationHelper _notificationHelper;
    private readonly IAuthorizationMiddleware _authService;

    /// <summary>
    /// NotificationBroadcasterFunction Initialization
    /// </summary>
    /// <param name="notificationHelper">Notification Helper</param>
    /// <param name="authService">Authorization Service to validate claims</param>
    public NotificationBroadcasterFunction(INotificationHelper notificationHelper, IAuthorizationMiddleware authService)
    {
        _notificationHelper = notificationHelper;
        _authService = authService;
    }

    [FunctionName("NotificationBroadcaster")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
        ILogger logger, ExecutionContext context, ClaimsPrincipal claimsPrincipal)
    {
        LogData logData = new LogData()
        {
            EventDetails = new Dictionary<string, object>()
            {
                { Constant.BusinessProcessName, "Notification - Broadcaster" },
                { Constant.ActionUri, "NotificationBroadcaster" },
                { Constant.ComponentType, ComponentType.BackgroundProcess }
            }
        };

        try
        {
            await CommonServicesStartup.refresher.RefreshAsync();

            // Authorization using claims
            if (!_authService.IsValidClaims(claimsPrincipal))
            {
                return new UnauthorizedResult();
            }

            logData.EventDetails.Modify(Constant.AppAction, "Notification - Broadcaster - Initiated");
            using (logger.BeginScope(logData.EventDetails))
            {
                logger.LogInformation(new EventId((int)EventIds.NotificationBroadcasterInitiated),
                    "Notification - Broadcaster - Initiated");
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<NotificationItem>(requestBody);

            if (data?.NotificationTypes?.Contains(NotificationType.Text) == true && !IsValidAlias(data.To))
            {
                logData.EventDetails.Modify(Constant.AppAction, "Notification - Broadcaster - Failed - Invalid Text Alias");
                logData.EventDetails.Modify(Constant.To, $"{data.To}");
                using (logger.BeginScope(logData.EventDetails))
                {
                    logger.LogWarning(new EventId((int)EventIds.NotificationBroadcasterFailed),
                        "Notification - Broadcaster - Failed - Invalid Text Alias");
                }

                return new BadRequestObjectResult("Invalid 'to' alias for Text notification.");
            }

            logData.EventDetails.Modify(Constant.ApplicationName, $"{data.ApplicationName}");
            logData.EventDetails.Modify(Constant.TenantIdentifier, $"{data.TenantIdentifier}");
            logData.EventDetails.Modify(Constant.Xcv, $"{data.Telemetry?.Xcv}");
            logData.EventDetails.Modify(Constant.MessageId, $"{data.Telemetry?.MessageId}");
            logData.EventDetails.Modify(Constant.Id, $"{data.Id}");
            logData.EventDetails.Modify(Constant.To, $"{data.To}");
            logData.EventDetails.Modify(Constant.NotificationTypes, $"{JsonConvert.SerializeObject(data.NotificationTypes)}");

            var notificationResponse = await _notificationHelper.BroadcastNotificationsToProviders(data);
            logData.EventDetails.Modify(Constant.ResponseContent, JsonConvert.SerializeObject(notificationResponse));

            if (notificationResponse.ActionResult)
            {
                logData.EventDetails.Modify(Constant.AppAction, "Notification - Broadcaster - Success");
                using (logger.BeginScope(logData.EventDetails))
                {
                    logger.LogInformation(new EventId((int)EventIds.NotificationBroadcasterSuccess),
                        "Notification - Broadcaster - Success");
                }
                return new OkObjectResult(notificationResponse);
            }

            logData.EventDetails.Modify(Constant.AppAction, "Notification - Broadcaster - Failed");
            using (logger.BeginScope(logData.EventDetails))
            {
                logger.LogInformation(new EventId((int)EventIds.NotificationBroadcasterFailed),
                    "Notification - Broadcaster - Failed");
            }
            return new BadRequestObjectResult(notificationResponse);
        }
        catch (Exception ex)
        {
            logData.EventDetails.Modify(Constant.AppAction, "Notification - Broadcaster - Failed - Exception");
            using (logger.BeginScope(logData.EventDetails))
            {
                logger.LogError(new EventId((int)EventIds.NotificationBroadcasterError),
                    ex,
                    "Notification - Broadcaster - Failed - Exception");
            }
            return new BadRequestObjectResult(ex.Message);
        }
    }

    private static bool IsValidAlias(string alias) =>
        !string.IsNullOrWhiteSpace(alias) && AliasRegex.IsMatch(alias);
}