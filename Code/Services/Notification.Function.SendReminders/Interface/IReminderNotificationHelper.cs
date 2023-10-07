// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.Function.SendReminders.Interface;

using System.Threading.Tasks;
using Contract;

public interface IReminderNotificationHelper
{
    /// <summary>
    /// Send Reminder Notifications using the Notification payload
    /// </summary>
    /// <param name="notificationItem"></param>
    /// <returns></returns>
    Task<bool> ProcessReminderNotificationRequestsAsync(NotificationItem notificationItem);
}