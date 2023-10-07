// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Notification.BL.Common;
using Notification.BL.Common.Extension;
using Notification.BL.Common.Helpers;
using Notification.BL.Common.Interface;
using Notification.BL.PerformanceLogger.Helpers;
using Notification.BL.PerformanceLogger.Interfaces;
using Notification.Data.Azure.Storage.Helpers;
using Notification.Data.Azure.Storage.Interface;
using Notification.Function.SendCustomNotifications;
using Notification.Function.SendCustomNotifications.Helpers;
using Notification.Function.SendCustomNotifications.Interface;

[assembly: FunctionsStartup(typeof(SendCustomNotificationStartup))]

namespace Notification.Function.SendCustomNotifications;

public class SendCustomNotificationStartup : FunctionsStartup
{
    public static IConfigurationRefresher refresher;

    /// <summary>
    /// Configure Builder
    /// </summary>
    /// <param name="builder">FunctionsHost Builder</param>
    public override void Configure(IFunctionsHostBuilder builder)
    {
        // Create the new ConfigurationBuilder
        var configurationBuilder = new ConfigurationBuilder();

        // Build the config in order to access the appsettings for getting the Azure App Configuration connection settings
        var config = configurationBuilder.Build();

        // Add the Azure App Configuration to the configuration builder
        configurationBuilder.AddAzureAppConfiguration(options =>
        {
            options.Connect(new Uri(Environment.GetEnvironmentVariable(Constant.AzureAppConfigurationUrl)), new DefaultAzureCredential())
                // Load configuration values with no label
                .Select(KeyFilter.Any, LabelFilter.Null)
                // Load configurations for current feature
                .Select(KeyFilter.Any, Environment.GetEnvironmentVariable(Constant.FeatureName))
                .ConfigureKeyVault(kv =>
                {
                    kv.SetCredential(new DefaultAzureCredential());
                })
                .ConfigureRefresh(refreshOptions =>
                {
                    refreshOptions.Register(Constant.MustUpdateConfig, true)
                    .SetCacheExpiration(TimeSpan.FromMinutes(5));
                    refresher = options.GetRefresher();
                });
        });

        // Build the config again so it has the Azure App Configuration provider
        config = configurationBuilder.Build();

        // Replace the existing config with the new one
        builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));

        builder.Services.AddLogging(configure =>
        {
            configure.AddApplicationInsights(
                configureTelemetryConfiguration: (config) => config.ConnectionString = Environment.GetEnvironmentVariable(Constant.ApplicationInsightsConnectionString),
                configureApplicationInsightsLoggerOptions: (options) => { }
            );
            configure.AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Information)
                .AddFilter<ApplicationInsightsLoggerProvider>(Constant.CompanyName, LogLevel.Error);
        });

        var client = new BlobServiceClient(
                        new Uri($"https://" + config?[Constant.StorageAccountName] + ".blob.core.windows.net/"),
                        new DefaultAzureCredential());
        builder.Services.AddSingleton<IBlobStorageHelper, BlobStorageHelper>(x => new BlobStorageHelper(client));

        builder.Services.AddScoped<ICustomNotificationHelper, CustomNotificationHelper>();
        builder.Services.AddScoped<IUtilityHelper, UtilityHelper>();
        builder.Services.AddSingleton<IPerformanceLogger, PerformanceLogger>();
        builder.Services.AddSingleton<HttpClientHandler>();
        builder.Services.AddScoped<IAuthenticationHelper, AuthenticationHelper>();
        builder.Services.AddHttpClient<IHttpHelper, HttpHelper>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5)) // Set lifetime to five minutes
            .AddPolicyHandler(Extension.GetRetryPolicy(6));
    }
}