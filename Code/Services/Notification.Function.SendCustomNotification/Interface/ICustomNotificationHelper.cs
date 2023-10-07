// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.Function.SendCustomNotifications.Interface;

using System.Threading.Tasks;
using Contract;

public interface ICustomNotificationHelper
{
    Task<bool> ProcessCustomNotificationRequestsAsync(NotificationItem notificationItem);
}