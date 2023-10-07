// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.Services.Helpers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BL.Common;
using Contract;
using Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Notification.Data.Azure.Storage.Interface;

/// <summary>
/// Helper class to Create/Update/Delete Web push registration entry in database
/// </summary>
public class WebPushNotificationRegistrationHelper : IWebPushNotificationRegistration
{
    private readonly IConfiguration _config;
    private readonly ITableHelper _storageTableDataHelperWebPushRegistration;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="storageTableDataHelperWebPushRegistration"></param>
    /// <param name="config"></param>
    public WebPushNotificationRegistrationHelper(ITableHelper storageTableDataHelperWebPushRegistration, IConfiguration config)
    {
        _config = config;
        _storageTableDataHelperWebPushRegistration = storageTableDataHelperWebPushRegistration;
    }

    #region Implemented methods

    /// <summary>
    /// Gets the web push registration entries in the database for the user
    /// </summary>
    /// <param name="alias">Alias of the user</param>
    /// <returns>Returns all active registrations for the user</returns>
    public List<WebPushNotificationRegistrationEntity> GetRegistration(string alias)
    {
        return _storageTableDataHelperWebPushRegistration.GetTableEntityListByPartitionKey<WebPushNotificationRegistrationEntity>(_config[Constant.WebPushNotificationRegistrationTableName], alias).ToList();
    }

    /// <summary>
    /// Creates the web push registration entry in the database for the user/device combination
    /// </summary>
    /// <param name="alias">Alias of the user</param>
    /// <param name="registrationInfo">Registration details like browser/endpoint configuration</param>
    /// <returns></returns>
    public void CreateRegistration(string alias, string registrationInfo)
    {
        var registrationEntity = JsonConvert.DeserializeObject<WebPushNotificationRegistrationEntity>(registrationInfo);
        if (registrationEntity == null)
        {
            throw new InvalidDataException("Registration Entity is null");
        }

        registrationEntity.UserAlias = alias;
        registrationEntity.PartitionKey = alias;
        registrationEntity.RowKey = Guid.NewGuid().ToString();
        registrationEntity.Auth = registrationEntity.Keys["auth"];
        registrationEntity.P256DH = registrationEntity.Keys["p256dh"];

        var rows = _storageTableDataHelperWebPushRegistration.GetTableEntityListByPartitionKey<WebPushNotificationRegistrationEntity>(_config[Constant.WebPushNotificationRegistrationTableName], alias).Where(x => x.EndPoint.Equals(registrationEntity.EndPoint)).ToList();

        if (rows.Count == 0)
        {
            _storageTableDataHelperWebPushRegistration.Insert(_config[Constant.WebPushNotificationRegistrationTableName], registrationEntity);
        }
    }

    /// <summary>
    /// Deletes the web push registration entry in the database for the user/device combination
    /// </summary>
    /// <param name="alias">Alias of the user</param>
    /// <param name="registrationInfo">Registration details like browser/endpoint configuration</param>
    /// <returns></returns>
    public void DeleteRegistration(string alias, string registrationInfo)
    {
        var registrationEntity = JsonConvert.DeserializeObject<WebPushNotificationRegistrationEntity>(registrationInfo);
        if (registrationEntity == null)
        {
            throw new InvalidDataException("Registration Entity is null");
        }

        var rows = _storageTableDataHelperWebPushRegistration.GetTableEntityListByPartitionKey<WebPushNotificationRegistrationEntity>(_config[Constant.WebPushNotificationRegistrationTableName], alias).Where(x => x.EndPoint.Equals(registrationEntity.EndPoint)).ToList();

        _storageTableDataHelperWebPushRegistration.DeleteRowsAsync(_config[Constant.WebPushNotificationRegistrationTableName], rows);
    }

    #endregion Implemented methods
}