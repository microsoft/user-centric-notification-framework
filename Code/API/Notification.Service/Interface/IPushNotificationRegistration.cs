// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.Services.Interface;

using System.Collections.Generic;
using System.Threading.Tasks;
using Contract;
using Microsoft.Azure.NotificationHubs;

/// <summary>
/// Defines operations for managing push notification registrations in a notification hub.
/// </summary>
public interface IPushNotificationRegistration
{
    /// <summary>
    /// Gets the push registration info
    /// </summary>
    /// <param name="alias">Alias of the user</param>
    /// <returns>Returns the registration info if for the given user</returns>
    Task<List<RegistrationDescription>> GetRegistrationInfo(string alias);

    /// <summary>
    /// Creates the push registration id
    /// </summary>
    /// <param name="handle">the push handle</param>
    /// <returns>Returns the registration id if created successfully else returns null</returns>
    Task<string> CreateRegistrationId(string handle = null);

    /// <summary>
    /// Updates the  push registration entry in notification hub for the user/device combination
    /// </summary>
    /// <param name="alias">Alias of the user</param>
    /// <param name="registrationInfo">Registration details like endpoint configuration</param>
    /// <returns></returns>
    Task CreateOrUpdateRegistration(string alias, DeviceRegistration registrationInfo);

    /// <summary>
    /// Deletes the push registration entry in notification hub for the user/device combination
    /// </summary>
    /// <param name="alias">Alias of the user</param>
    /// <param name="id">the registration id</param>
    /// <returns></returns>
    Task DeleteRegistration(string alias, string id);
}