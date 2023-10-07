// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.Function.CommonServices;

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using BL.Common;
using BL.Common.Extension;
using BL.Common.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Model.Model;
using Newtonsoft.Json;
using Notification.Contract;
using Notification.Function.CommonServices.Utils;

public class NotificationDataMapperFunction
{
    private readonly INotificationHelper _notificationHelper;
    private readonly IAuthorizationMiddleware _authService;

    /// <summary>
    /// NotificationDataMapperFunction Initialization
    /// </summary>
    /// <param name="notificationHelper">Notification Helper</param>
    /// <param name="authService">Authorization Service to validate claims</param>
    public NotificationDataMapperFunction(INotificationHelper notificationHelper, IAuthorizationMiddleware authService)
    {
        _notificationHelper = notificationHelper;
        _authService = authService;
    }

    [FunctionName("NotificationDataMapper")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
        ILogger logger, ExecutionContext context, ClaimsPrincipal claimsPrincipal)
    {
        LogData logData = new LogData()
        {
            EventDetails = new Dictionary<string, object>()
            {
                { Constant.BusinessProcessName, "Notification - Data Mapper" },
                { Constant.ActionUri, "NotificationDataMapper" },
                { Constant.ComponentType, ComponentType.BackgroundProcess }
            }
        };

        try
        {
            // Authorization using claims
            if (!_authService.IsValidClaims(claimsPrincipal))
            {
                return new UnauthorizedResult();
            }

            await CommonServicesStartup.refresher.RefreshAsync();

            logData.EventDetails.Modify(Constant.AppAction, "Notification - Data Mapper - Initiated");
            using (logger.BeginScope(logData.EventDetails))
            {
                logger.LogInformation(new EventId((int)EventIds.DataMapperProcessInitiated),
                    "Notification - Data Mapper - Initiated");
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            var notificationTypes = JsonConvert.DeserializeObject<List<NotificationType>>(Convert.ToString(data.notificationTypes));

            var output = _notificationHelper.Replace(notificationTypes, data.templateData, data.templateContent);

            logData.EventDetails.Modify(Constant.AppAction, "Notification - Data Mapper - Success");
            using (logger.BeginScope(logData.EventDetails))
            {
                logger.LogInformation(new EventId((int)EventIds.DataMapperProcessSuccess),
                    "Notification - Data Mapper - Success");
            }

            return new OkObjectResult(output);
        }
        catch (Exception ex)
        {
            logData.EventDetails.Modify(Constant.AppAction, "Notification - Data Mapper - Failed - Exception");
            using (logger.BeginScope(logData.EventDetails))
            {
                logger.LogError(new EventId((int)EventIds.DataMapperProcessError),
                    ex,
                    "Notification - Data Mapper - Failed - Exception");
            }
            return new BadRequestObjectResult(ex.Message);
        }
    }
}