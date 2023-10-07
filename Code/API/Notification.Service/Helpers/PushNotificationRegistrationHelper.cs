// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.Services.Helpers;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BL.Common;
using Contract;
using Interface;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.NotificationHubs.Messaging;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Helper class to Create/Update/Delete push registration entry in notificationn hub
/// </summary>
public class PushNotificationRegistrationHelper : IPushNotificationRegistration
{
    private readonly IConfiguration _config;
    private readonly NotificationHubClient _hub;

    public PushNotificationRegistrationHelper(IConfiguration config)
    {
        _config = config;
        _hub = NotificationHubClient.CreateClientFromConnectionString(_config[Constant.NotificationHubDefaultFullSharedAccessSignature], _config[Constant.HubName]);
    }

    #region Implemented methods

    /// <summary>
    /// Gets the push registration info
    /// </summary>
    /// <returns>Returns the registration info if for the given user</returns>
    public async Task<List<RegistrationDescription>> GetRegistrationInfo()
    {
        return (await _hub.GetAllRegistrationsAsync(0)).ToList();
    }

    /// <summary>
    /// Creates the push registration id
    /// </summary>
    /// <param name="handle">the push handle</param>
    /// <returns>Returns the registration id if created successfully else returns null</returns>
    public async Task<string> CreateRegistrationId(string handle = null)
    {
        string newRegistrationId = null;

        // make sure there are no existing registrations for this push handle (used for iOS and Android)
        if (handle != null)
        {
            var registrations = await _hub.GetRegistrationsByChannelAsync(handle, 100);

            foreach (var registration in registrations)
            {
                if (newRegistrationId == null)
                {
                    newRegistrationId = registration.RegistrationId;
                }
                else
                {
                    await _hub.DeleteRegistrationAsync(registration);
                }
            }
        }

        return newRegistrationId ?? (await _hub.CreateRegistrationIdAsync());
    }

    /// <summary>
    /// This creates or updates a registration (with provided PNS handle) at the specified id
    /// </summary>
    /// <param name="alias">Alias of the user</param>
    /// <param name="registrationInfo"></param>
    /// <returns>Returns all active registrations for the user</returns>
    public async Task CreateOrUpdateRegistration(string alias, DeviceRegistration registrationInfo)
    {
        RegistrationDescription registration = null;
        switch (registrationInfo.Platform)
        {
            case "mpns":
                registration = new MpnsRegistrationDescription(registrationInfo.Handle);
                break;

            case "wns":
                registration = new WindowsRegistrationDescription(registrationInfo.Handle);
                break;

            case "apns":
                registration = new AppleRegistrationDescription(registrationInfo.Handle);
                break;

            case "fcm":
                registration = new FcmRegistrationDescription(registrationInfo.Handle);
                break;

            default:
                throw new InvalidDataException();
        }

        // This insures that this registration is only for the currently signed in user's alias
        // The tag could contain anything else but that doesn't matter. The important thing is that it is of the form
        // [currentUsersAlias]_[type] and not [someOtherAlias]_[type] and this is an easy flexable way to test that
        if (registrationInfo.Tags.Any(tag => !tag.Contains(alias)))
        {
            throw new InvalidDataException();
        }

        registration.RegistrationId = registrationInfo.Id;
        registration.Tags = new HashSet<string>(registrationInfo.Tags);

        try
        {
            await _hub.CreateOrUpdateRegistrationAsync(registration);
        }
        catch (MessagingException e)
        {
            ReturnGoneIfHubResponseIsGone(e);
        }
    }

    /// <summary>
    /// Deletes the push registration entry in notification hub for the user/device combination
    /// </summary>
    /// <param name="id">the registration id</param>
    /// <returns></returns>
    public async Task DeleteRegistration(string id)
    {
        await _hub.DeleteRegistrationAsync(id);
    }

    #endregion Implemented methods

    /// <summary>
    /// Checks if the exception has an internal response code of HttpStatusCode.Gone then throw a new HttpRequestException with HttpStatusCode as Gone
    /// </summary>
    /// <param name="e"></param>
    private static void ReturnGoneIfHubResponseIsGone(MessagingException e)
    {
        if (e.InnerException is WebException webex && webex.Status == WebExceptionStatus.ProtocolError)
        {
            var response = (HttpWebResponse)webex.Response;
            if (response.StatusCode == HttpStatusCode.Gone)
            {
                throw new HttpRequestException(HttpStatusCode.Gone.ToString());
            }
        }
    }
}