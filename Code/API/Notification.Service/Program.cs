// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.Services;

using System;
using Azure.Identity;
using BL.Common;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

public class Program
{
    public static void Main(string[] args)
    {
        CreateWebHostBuilder(args).Build().Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                var settings = config.Build();
                // Add the Azure App Configuration to the configuration builder
                config.AddAzureAppConfiguration(options =>
                {
                    options.Connect(new Uri(settings?[Constant.AzureAppConfigurationUrl]), new DefaultAzureCredential())
                        // Load configuration values with no label
                        .Select(KeyFilter.Any, LabelFilter.Null)
                        // Load configurations for current feature
                        .Select(KeyFilter.Any, settings?[Constant.FeatureName])
                        .ConfigureKeyVault(kv =>
                        {
                            kv.SetCredential(new DefaultAzureCredential());
                        })
                        .ConfigureRefresh(refreshOptions =>
                        {
                            refreshOptions.Register(Constant.MustUpdateConfig, true)
                                .SetCacheExpiration(TimeSpan.FromMinutes(5));
                        });
                });
                settings = config.Build();
            })
            .UseStartup<Startup>();
}