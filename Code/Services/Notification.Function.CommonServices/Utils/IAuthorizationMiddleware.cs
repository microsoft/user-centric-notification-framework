// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.Function.CommonServices.Utils;

using System.Security.Claims;

public interface IAuthorizationMiddleware
{
    bool IsValidClaims(ClaimsPrincipal claimsPrincipal);
}