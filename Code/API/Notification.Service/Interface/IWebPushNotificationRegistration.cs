// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.Services.Interface;

using System.Collections.Generic;
using System.Threading.Tasks;
using Contract;

public interface IWebPushNotificationRegistration
{
    /// <summary>
    /// Gets the web push registration entries in the database for the user
    /// </summary>
    /// <param name="alias">Alias of the user</param>
    /// <returns>Returns all active registrations for the user</returns>
    List<WebPushNotificationRegistrationEntity> GetRegistration(string alias);

    /// <summary>
    /// Creates the web push registration entry in the database for the user/device combination
    /// </summary>
    /// <param name="alias">Alias of the user</param>
    /// <param name="registrationInfo">Registration details like browser/endpoint configuration</param>
    /// <returns></returns>
    void CreateRegistration(string alias, string registrationInfo);

    /// <summary>
    /// Deletes the web push registration entry in the database for the user/device combination
    /// </summary>
    /// <param name="alias">Alias of the user</param>
    /// <param name="registrationInfo">Registration details like browser/endpoint configuration</param>
    /// <returns></returns>
    void DeleteRegistration(string alias, string registrationInfo);
}