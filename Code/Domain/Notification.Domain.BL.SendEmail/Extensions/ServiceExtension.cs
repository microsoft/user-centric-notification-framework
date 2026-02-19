// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.Domain.BL.SendEmail.Extensions
{
    using Microsoft.Extensions.DependencyInjection;
    using Notification.BL.SendEmail.Interfaces;
    using Notification.Domain.BL.SendEmail.Internal.Helpers;
    using Notification.Domain.BL.SendEmail.Internal.Interfaces;
    using Notification.Domain.BL.SendEmail.Internal.Models;

    public static class ServiceExtension
    {
        public static void ConfigureEnterpriseEmail(this IServiceCollection services)
        {
            services.AddSingleton<Configuration>();
            services.AddSingleton<ITokenFactory, DefaultTokenFactory>();
            services.AddSingleton<IEnterpriseEmailClient, EnterpriseEmailClient>();
            services.AddSingleton<IStatusTableHelper, StatusTableHelper>();
            services.AddScoped<IEmailNotificationHelper, EnterpriseEmailNotificationHelper>();
        }
    }
}