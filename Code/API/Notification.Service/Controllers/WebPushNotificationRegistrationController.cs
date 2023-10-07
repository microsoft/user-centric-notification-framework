// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.Services.Controllers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BL.Common;
using BL.Common.Extension;
using Contract;
using Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Model.Model;

[Route("api/WebPushNotificationRegistration")]
[ApiController]
public class WebPushNotificationRegistrationController : BaseController
{
    private readonly IWebPushNotificationRegistration _webPushNotificationRegistration = null;
    private readonly ILogger _logger;

    public WebPushNotificationRegistrationController(
        IHttpContextAccessor httpContextAccessor,
        IWebPushNotificationRegistration webPushNotificationRegistration,
        ILogger<WebPushNotificationRegistrationController> logger) : base(httpContextAccessor)
    {
        _webPushNotificationRegistration = webPushNotificationRegistration;
        _logger = logger;
    }

    /// <summary>
    /// API controller to get Web Push Registration from database
    /// </summary>
    /// <returns>returns a JSON array</returns>
    [HttpGet()]
    public async Task<ActionResult<List<WebPushNotificationRegistrationEntity>>> Get()
    {
        LogData logData = new LogData()
        {
            EventDetails = new Dictionary<string, object>() {
                { Constant.BusinessProcessName, "Notification - Registration - Web Push - GET" },
                { Constant.ActionUri, "GET api/WebPushNotificationRegistration" },
                { Constant.ComponentType, ComponentType.WebService },
                { Constant.UserAlias, Alias },
                { Constant.Xcv, Xcv },
                { Constant.MessageId, MessageId }
            }
        };

        try
        {
            logData.EventDetails.Modify(Constant.AppAction, "Notification - Registration - Web Push - GET - Initiated");
            using (_logger.BeginScope(logData.EventDetails))
            {
                _logger.LogInformation(new EventId((int)EventIds.WebPushRegistrationInitiated),
                    "Notification - Registration - Web Push - GET - Initiated");
            }

            // Get registrations
            var data = _webPushNotificationRegistration.GetRegistration(Alias);

            // Log Success
            logData.EventDetails.Modify(Constant.AppAction, "Notification - Registration - Web Push - GET - Success");
            using (_logger.BeginScope(logData.EventDetails))
            {
                _logger.LogInformation(new EventId((int)EventIds.WebPushRegistrationSuccess),
                     "Notification - Registration - Web Push - GET - Success");
            }
            return Ok(data);
        }
        catch (Exception ex)
        {
            // Log Error
            logData.EventDetails.Modify(Constant.AppAction, "Notification - Registration - Web Push - GET - Failed - Exception");
            using (_logger.BeginScope(logData.EventDetails))
            {
                _logger.LogError(new EventId((int)EventIds.WebPushRegistrationError),
                    ex,
                    "Notification - Registration - Web Push - Failed - GET - Exception");
            }
            return BadRequest();
        }
    }

    /// <summary>
    /// API controller to Create Web Push Registration in database
    /// </summary>
    /// <returns>returns a task</returns>
    [HttpPost()]
    public async Task<ActionResult> Post()
    {
        LogData logData = new LogData()
        {
            EventDetails = new Dictionary<string, object>() {
                { Constant.BusinessProcessName, "Notification - Registration - Web Push - POST" },
                { Constant.ActionUri, "POST api/WebPushNotificationRegistration" },
                { Constant.ComponentType, ComponentType.WebService },
                { Constant.UserAlias, Alias },
                { Constant.Xcv, Xcv },
                { Constant.MessageId, MessageId }
            }
        };

        try
        {
            logData.EventDetails.Modify(Constant.AppAction, "Notification - Registration - Web Push - POST - Initiated");
            using (_logger.BeginScope(logData.EventDetails))
            {
                _logger.LogInformation(new EventId((int)EventIds.WebPushRegistrationInitiated),
                    "Notification - Registration - Web Push - POST - Initiated");
            }

            using (var reader = new StreamReader(Request.Body))
            {
                var body = reader.ReadToEnd();

                // Create or update registration
                _webPushNotificationRegistration.CreateRegistration(Alias, body);
            }

            // Log Success
            logData.EventDetails.Modify(Constant.AppAction, "Notification - Registration - Web Push - POST - Success");
            using (_logger.BeginScope(logData.EventDetails))
            {
                _logger.LogInformation(new EventId((int)EventIds.WebPushRegistrationSuccess),
                    "Notification - Registration - Web Push - POST - Success");
            }

            return Ok();
        }
        catch (Exception ex)
        {
            // Log Error
            logData.EventDetails.Modify(Constant.AppAction, "Notification - Registration - Web Push - POST - Failed - Exception");
            using (_logger.BeginScope(logData.EventDetails))
            {
                _logger.LogError(new EventId((int)EventIds.WebPushRegistrationError),
                    ex,
                    "Notification - Registration - Web Push - Failed - POST - Exception");
            }
            return BadRequest();
        }
    }

    /// <summary>
    /// API controller to Delete Web Push Registration in database
    /// </summary>
    /// <returns>returns a task</returns>
    [HttpDelete]
    public async Task<ActionResult> Delete()
    {
        LogData logData = new LogData()
        {
            EventDetails = new Dictionary<string, object>() {
                { Constant.BusinessProcessName, "Notification - Registration - Web Push - DELETE" },
                { Constant.ActionUri, "DELETE api/WebPushNotificationRegistration" },
                { Constant.ComponentType, ComponentType.WebService },
                { Constant.UserAlias, Alias },
                { Constant.Xcv, Xcv },
                { Constant.MessageId, MessageId }
            }
        };
        try
        {
            logData.EventDetails.Modify(Constant.AppAction, "Notification - Registration - Web Push - DELETE - Initiated");
            using (_logger.BeginScope(logData.EventDetails))
            {
                _logger.LogInformation(new EventId((int)EventIds.WebPushRegistrationInitiated),
                    "Notification - Registration - Web Push - DELETE - Initiated");
            }

            using (var reader = new StreamReader(Request.Body))
            {
                var body = reader.ReadToEnd();

                // Delete registration
                _webPushNotificationRegistration.DeleteRegistration(Alias, body);
            }

            // Log Success
            logData.EventDetails.Modify(Constant.AppAction, "Notification - Registration - Web Push - DELETE - Success");
            using (_logger.BeginScope(logData.EventDetails))
            {
                _logger.LogInformation(new EventId((int)EventIds.WebPushRegistrationSuccess),
                    "Notification - Registration - Web Push - DELETE - Success");
            }

            return Ok();
        }
        catch (Exception ex)
        {
            // Log Error
            logData.EventDetails.Modify(Constant.AppAction, "Notification - Registration - Web Push - DELETE - Failed - Exception");
            using (_logger.BeginScope(logData.EventDetails))
            {
                _logger.LogError(new EventId((int)EventIds.WebPushRegistrationError),
                    ex,
                    "Notification - Registration - Web Push - Failed - DELETE - Exception");
            }
            return BadRequest();
        }
    }
}