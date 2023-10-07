// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.BL.Common.Interface
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Azure.Data.Tables;
    using Contract;

    public interface INotificationHelper
    {
        /// <summary>
        /// Sends Notification message to different queues based on the notification type
        /// </summary>
        /// <param name="item">notification item</param>
        /// <returns>Returns Notification response with details about message delivery</returns>
        Task<NotificationResponse> BroadcastNotificationsToProviders(NotificationItem item);

        /// <summary>
        /// Replaces the placeholders in template content with template data
        /// </summary>
        /// <param name="notificationTypes">list of notificationtype</param>
        /// <param name="templateData">template data</param>
        /// <param name="templateContent">template content</param>
        /// <returns>Updated template content (string) without placeholders</returns>
        string Replace(List<NotificationType> notificationTypes, dynamic templateData, dynamic templateContent);

        /// <summary>
        /// Log Notification Status.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="tableName"></param>
        /// <returns>Table operation result.</returns>
        Task<bool> LogNotificationStatus<T>(T status, string tableName) where T : class, ITableEntity, new();
    }
}