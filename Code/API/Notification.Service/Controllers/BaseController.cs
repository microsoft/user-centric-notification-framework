// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.Services.Controllers;

using System;
using System.Linq;
using System.Net.Mail;
using BL.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public class BaseController : ControllerBase
{
    protected string Alias { get; set; }
    protected string Xcv { get; set; }
    protected string MessageId { get; set; }

    public BaseController(IHttpContextAccessor httpContextAccessor)
    {
        var userPrincipalName = httpContextAccessor.HttpContext.Request.Headers[Constant.UserPrincipalName].FirstOrDefault();
        if (!string.IsNullOrEmpty(userPrincipalName))
        {
            Alias = new MailAddress(userPrincipalName).User;
        }

        Xcv = httpContextAccessor.HttpContext.Request.Headers[Constant.Xcv].FirstOrDefault();
        if (!string.IsNullOrEmpty(Xcv))
        {
            Xcv = Guid.NewGuid().ToString();
        }

        MessageId = httpContextAccessor.HttpContext.Request.Headers[Constant.MessageId].FirstOrDefault();
        if (!string.IsNullOrEmpty(MessageId))
        {
            MessageId = Guid.NewGuid().ToString();
        }
    }
}