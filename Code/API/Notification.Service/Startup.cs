// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.Services;

using System;
using System.IO;
using Azure.Identity;
using BL.Common;
using BL.PerformanceLogger.Helpers;
using BL.PerformanceLogger.Interfaces;
using Data.Azure.Storage.Helpers;
using Data.Azure.Storage.Interface;
using Helpers;
using Interface;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.OpenApi.Models;
using Notification.Services.Utils;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        // Add IHttpContextAccessor if it's not yet added
        services.AddHttpContextAccessor();

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddTransient<ITableHelper, TableHelper>((provider) => { return new TableHelper(Configuration[Constant.StorageAccountName], new DefaultAzureCredential()); });
        services.AddTransient<IWebPushNotificationRegistration, WebPushNotificationRegistrationHelper>();
        services.AddTransient<IPushNotificationRegistration, PushNotificationRegistrationHelper>();

        services.AddSingleton<IPerformanceLogger, PerformanceLogger>();
        services.AddLogging(configure =>
        {
            configure.AddApplicationInsights(
                configureTelemetryConfiguration: (config) => config.ConnectionString = Environment.GetEnvironmentVariable(Constant.ApplicationInsightsConnectionString),
                configureApplicationInsightsLoggerOptions: (options) => { }
            );
            configure.AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Information)
                .AddFilter<ApplicationInsightsLoggerProvider>(Constant.CompanyName, LogLevel.Error);
        });

        services.AddTransient<AuthorizationMiddleware, AuthorizationMiddleware>();

        // Register the Swagger generator, defining one or more Swagger documents
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Notification Services",
                Version = "v1",
            });

            var xmlPath = Path.ChangeExtension(typeof(Startup).Assembly.Location, ".xml");
            c.IncludeXmlComments(xmlPath);
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.RoutePrefix = "";
            c.SwaggerEndpoint("../swagger/v1/swagger.json", "Notification Services");
        });

        app.UseHttpsRedirection();
        app.UseAzureAppConfiguration();
        app.UseMiddleware<AuthorizationMiddleware>();
    }
}