// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.Function.SendTextNotification.Interface;

using System.Threading.Tasks;
using Contract;

public interface ITextNotificationHelper
{
    Task<bool> ProcessTextNotificationRequestsAsync(NotificationItem notificatioItem);
}