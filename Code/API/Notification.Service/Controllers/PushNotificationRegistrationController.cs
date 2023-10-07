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
using Microsoft.Azure.NotificationHubs;
using Microsoft.Extensions.Logging;
using Model.Model;

[Route("api/PushNotificationRegistration")]
[ApiController]
public class PushNotificationRegistrationController : BaseController
{
    private readonly IPushNotificationRegistration _pushNotificationRegistration = null;
    private readonly ILogger _logger;

    public PushNotificationRegistrationController(
        IHttpContextAccessor httpContextAccessor,
        IPushNotificationRegistration pushNotificationRegistration,
        ILogger<PushNotificationRegistrationController> logger) : base(httpContextAccessor)
    {
        _pushNotificationRegistration = pushNotificationRegistration;
        _logger = logger;
    }

    /// <summary>
    /// API controller to Get all Push Registration info from notification hub
    /// </summary>
    /// <returns>returns a task</returns>
    [HttpGet]
    public async Task<ActionResult<List<RegistrationDescription>>> Get()
    {
        LogData logData = new LogData()
        {
            EventDetails = new Dictionary<string, object>() {
                { Constant.BusinessProcessName, "Notification - Registration - Device Push - GET" },
                { Constant.ActionUri, "GET api/PushNotificationRegistration" },
                { Constant.ComponentType, ComponentType.WebService },
                { Constant.UserAlias, Alias },
                { Constant.Xcv, Xcv },
                { Constant.MessageId, MessageId }
            }
        };

        try
        {
            logData.EventDetails.Modify(Constant.AppAction, "Notification - Registration - Device Push - GET - Initiated");
            using (_logger.BeginScope(logData.EventDetails))
            {
                _logger.LogInformation(new EventId((int)EventIds.DevicePushRegistrationInitiated),
                    "Notification - Registration - Device Push - GET - Initiated");
            }

            // Get registration
            var registrations = await _pushNotificationRegistration.GetRegistrationInfo();

            // Log Success
            logData.EventDetails.Modify(Constant.AppAction, "Notification - Registration - Device Push - GET - Success");
            using (_logger.BeginScope(logData.EventDetails))
            {
                _logger.LogInformation(new EventId((int)EventIds.DevicePushRegistrationSuccess),
                    "Notification - Registration - Device Push - GET - Success");
            }
            return Ok(registrations);
        }
        catch (Exception ex)
        {
            // Log Error
            logData.EventDetails.Modify(Constant.AppAction, "Notification - Registration - Device Push - GET - Failed - Exception");
            using (_logger.BeginScope(logData.EventDetails))
            {
                _logger.LogError(new EventId((int)EventIds.DevicePushRegistrationError),
                    ex,
                    "Notification - Registration - Device Push - Failed - GET - Exception");
            }
            return BadRequest();
        }
    }

    /// <summary>
    /// API controller to Create Push Registration ID in notification hub
    /// </summary>
    /// <returns>returns a task of string</returns>
    [HttpPost]
    public async Task<ActionResult<string>> Post()
    {
        LogData logData = new LogData()
        {
            EventDetails = new Dictionary<string, object>() {
                { Constant.BusinessProcessName, "Notification - Registration - Device Push - POST" },
                { Constant.ActionUri, "POST api/PushNotificationRegistration" },
                { Constant.ComponentType, ComponentType.WebService },
                { Constant.UserAlias, Alias },
                { Constant.Xcv, Xcv },
                { Constant.MessageId, MessageId }
            }
        };

        try
        {
            logData.EventDetails.Modify(Constant.AppAction, "Notification - Registration - Device Push - POST - Initiated");
            using (_logger.BeginScope(logData.EventDetails))
            {
                _logger.LogInformation(new EventId((int)EventIds.DevicePushRegistrationInitiated),
                    "Notification - Registration - Device Push - POST - Initiated");
            }

            string registrationId;
            using (var reader = new StreamReader(Request.Body))
            {
                var body = reader.ReadToEnd();

                // Create registration
                registrationId = await _pushNotificationRegistration.CreateRegistrationId(body);
            }

            // Log Success
            logData.EventDetails.Modify(Constant.AppAction, "Notification - Registration - Device Push - POST - Success");
            using (_logger.BeginScope(logData.EventDetails))
            {
                _logger.LogInformation(new EventId((int)EventIds.DevicePushRegistrationSuccess),
                    "Notification - Registration - Device Push - POST - Success");
            }

            return Ok(registrationId);
        }
        catch (Exception ex)
        {
            // Log Error
            logData.EventDetails.Modify(Constant.AppAction, "Notification - Registration - Device Push - POST - Failed - Exception");
            using (_logger.BeginScope(logData.EventDetails))
            {
                _logger.LogError(new EventId((int)EventIds.DevicePushRegistrationError),
                    ex,
                    "Notification - Registration - Device Push - Failed - POST - Exception");
            }
            return BadRequest();
        }
    }

    /// <summary>
    /// API controller to Create/Update Push Registration in notification hub
    /// </summary>
    /// <param name="registrationInfo">Device registration info</param>
    /// <returns>returns a Task</returns>
    [HttpPut]
    public async Task<ActionResult> Put(DeviceRegistration registrationInfo)
    {
        LogData logData = new LogData()
        {
            EventDetails = new Dictionary<string, object>() {
                { Constant.BusinessProcessName, "Notification - Registration - Device Push - PUT" },
                { Constant.ActionUri, "PUT api/PushNotificationRegistration" },
                { Constant.ComponentType, ComponentType.WebService },
                { Constant.UserAlias, Alias },
                { Constant.Xcv, Xcv },
                { Constant.MessageId, MessageId }
            }
        };

        try
        {
            logData.EventDetails.Modify(Constant.AppAction, "Notification - Registration - Device Push - PUT - Initiated");
            using (_logger.BeginScope(logData.EventDetails))
            {
                _logger.LogInformation(new EventId((int)EventIds.DevicePushRegistrationInitiated),
                    "Notification - Registration - Device Push - PUT - Initiated");
            }

            // Update registration
            await _pushNotificationRegistration.CreateOrUpdateRegistration(Alias, registrationInfo);

            // Log Success
            logData.EventDetails.Modify(Constant.AppAction, "Notification - Registration - Device Push - PUT - Success");
            using (_logger.BeginScope(logData.EventDetails))
            {
                _logger.LogInformation(new EventId((int)EventIds.DevicePushRegistrationSuccess),
                    "Notification - Registration - Device Push - PUT - Success");
            }
            return Ok();
        }
        catch (Exception ex)
        {
            // Log Error
            logData.EventDetails.Modify(Constant.AppAction, "Notification - Registration - Device Push - PUT - Failed - Exception");
            using (_logger.BeginScope(logData.EventDetails))
            {
                _logger.LogError(new EventId((int)EventIds.DevicePushRegistrationError),
                    ex,
                    "Notification - Registration - Device Push - Failed - PUT - Exception");
            }
            return BadRequest();
        }
    }

    /// <summary>
    /// API controller to Delete Push Registration from notification hub
    /// </summary>
    /// <param name="id">registration id</param>
    /// <returns>returns a task</returns>
    [HttpDelete]
    public async Task<ActionResult> Delete(string id)
    {
        LogData logData = new LogData()
        {
            EventDetails = new Dictionary<string, object>() {
                { Constant.BusinessProcessName, "Notification - Registration - Device Push - DELETE" },
                { Constant.ActionUri, "DELETE api/PushNotificationRegistration" },
                { Constant.ComponentType, ComponentType.WebService },
                { Constant.UserAlias, Alias },
                { Constant.Xcv, Xcv },
                { Constant.MessageId, MessageId }
            }
        };

        try
        {
            logData.EventDetails.Modify(Constant.AppAction, "Notification - Registration - Device Push - DELETE - Initiated");
            using (_logger.BeginScope(logData.EventDetails))
            {
                _logger.LogInformation(new EventId((int)EventIds.DevicePushRegistrationInitiated),
                    "Notification - Registration - Device Push - DELETE - Initiated");
            }

            // Delete registration
            await _pushNotificationRegistration.DeleteRegistration(id);

            // Log Success
            logData.EventDetails.Modify(Constant.AppAction, "Notification - Registration - Device Push - DELETE - Success");
            using (_logger.BeginScope(logData.EventDetails))
            {
                _logger.LogInformation(new EventId((int)EventIds.DevicePushRegistrationSuccess),
                    "Notification - Registration - Device Push - DELETE - Success");
            }
            return Ok();
        }
        catch (Exception ex)
        {
            // Log Error
            logData.EventDetails.Modify(Constant.AppAction, "Notification - Registration - Device Push - DELETE - Failed - Exception");
            using (_logger.BeginScope(logData.EventDetails))
            {
                _logger.LogError(new EventId((int)EventIds.DevicePushRegistrationError),
                    ex,
                    "Notification - Registration - Device Push - Failed - DELETE - Exception");
            }
            return BadRequest();
        }
    }
}