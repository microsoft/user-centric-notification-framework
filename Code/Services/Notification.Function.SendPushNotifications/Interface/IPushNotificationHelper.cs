// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.Function.SendPushNotifications.Interface;

using System.Threading.Tasks;
using Contract;

public interface IPushNotificationHelper
{
    Task<bool> ProcessWebPushNotificationRequestsAsync(NotificationItem notificationItem);

    Task ProcessDevicePushNotificationRequestsAsync(NotificationItem notificationItem);
}