// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.BL.SendEmail.Interfaces
{
    using System.Threading.Tasks;
    using Contract;

    public interface IEmailNotificationHelper
    {
        /// <summary>
        /// Send Email Notifications using the Notification payload
        /// </summary>
        /// <param name="notificationItem"></param>
        /// <returns></returns>
        Task<bool> ProcessEmailNotificationRequestsAsync(NotificationItem notificationItem);
    }
}