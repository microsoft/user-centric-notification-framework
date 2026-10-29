// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.BL.SendEmail.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Azure.Storage.Blobs;
    using BL.Common;
    using BL.Common.Extension;
    using BL.Common.Helpers;
    using BL.Common.Interface;
    using Contract;
    using Data.Azure.Storage.Interface;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Model.Model;
    using Newtonsoft.Json;
    using Notification.BL.SendEmail.Interfaces;

    public class EmailNotificationHelper : IEmailNotificationHelper
    {
        private readonly INotificationHelper _notificationHelper;
        private readonly IBlobStorageHelper _blobStorageHelper;
        private readonly IHttpHelper _httpHelper;
        private readonly ILogger _logger;
        private readonly string _functionUrl;
        private readonly string _allowedStorageHost;
        private readonly string _allowedBlobContainerName;

        public EmailNotificationHelper(INotificationHelper notificationHelper, IBlobStorageHelper blobStorageHelper, IConfiguration config, IHttpHelper httpHelper, ILogger<NotificationHelper> logger)
        {
            _notificationHelper = notificationHelper;
            _blobStorageHelper = blobStorageHelper;
            _httpHelper = httpHelper;
            _logger = logger;
            _functionUrl = config[Constant.SendEmailWebhookEndpoint];
            _allowedStorageHost = $"{config[Constant.StorageAccountName]}.blob.core.windows.net";
            // Falls back to the internal "attachments" container name if not overridden in App Configuration.
            var configuredContainerName = config[Constant.AttachmentsBlobContainerName];
            _allowedBlobContainerName = string.IsNullOrWhiteSpace(configuredContainerName)
                ? Constant.AttachmentsBlobContainer
                : configuredContainerName;
        }

        /// <summary>
        /// Send Email Notifications using the Notification payload
        /// </summary>
        /// <param name="notificationItem"></param>
        /// <returns></returns>
        public virtual async Task<bool> ProcessEmailNotificationRequestsAsync(NotificationItem notificationItem)
        {
            LogData logData = new LogData()
            {
                EventDetails = new Dictionary<string, object>()
                {
                    { Constant.BusinessProcessName, "Notification - Send Email Notification" },
                    { Constant.ActionUri, "ProcessEmailNotificationRequestsAsync" },
                    { Constant.ComponentType, ComponentType.BackgroundProcess },
                    { Constant.ApplicationName, $"{notificationItem.ApplicationName}" },
                    { Constant.TenantIdentifier, $"{notificationItem.TenantIdentifier}" },
                    { Constant.Xcv, $"{notificationItem.Telemetry?.Xcv}" },
                    { Constant.MessageId, $"{notificationItem.Telemetry?.MessageId}" },
                    { Constant.Id, $"{notificationItem.Id}" },
                    { Constant.AppAction, "Notification - ProcessEmailNotificationRequestsAsync - Initiated"}
                }
            };

            try
            {
                using (_logger.BeginScope(logData.EventDetails))
                {
                    _logger.LogInformation(new EventId((int)EventIds.SendEmailNotificationProcessing),
                        "Notification - ProcessEmailNotificationRequestsAsync - Initiated");
                }

                // Set default response
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);

                // Handle attachments
                foreach (var attachment in notificationItem.Attachments)
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(attachment.FileUrl))
                        {
                            // Assumption:: All the urls would be blob urls
                            BlobUriBuilder blobUriBuilder = new BlobUriBuilder(new Uri(attachment.FileUrl));
                            if (!string.Equals(blobUriBuilder.Host, _allowedStorageHost, StringComparison.OrdinalIgnoreCase)
                                || !string.Equals(blobUriBuilder.BlobContainerName, _allowedBlobContainerName, StringComparison.Ordinal)
                                || string.IsNullOrWhiteSpace(blobUriBuilder.BlobName))
                            {
                                throw new UnauthorizedAccessException("Attachment FileUrl does not resolve to an allowed blob location.");
                            }

                            attachment.FileBase64 = Convert.ToBase64String(await _blobStorageHelper.DownloadByteArray(blobUriBuilder.BlobContainerName, blobUriBuilder.BlobName));

                            // Rebuild FileUrl from validated parts only, so no caller-supplied data is forwarded downstream.
                            BlobUriBuilder canonicalUriBuilder = new BlobUriBuilder(new Uri($"https://{_allowedStorageHost}"))
                            {
                                BlobContainerName = _allowedBlobContainerName,
                                BlobName = blobUriBuilder.BlobName
                            };
                            attachment.FileUrl = canonicalUriBuilder.ToUri().ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error
                        logData.EventDetails.Modify(Constant.AppAction, "Notification - ProcessEmailNotificationRequestsAsync - Adding Attachments - Failed");
                        using (_logger.BeginScope(logData.EventDetails))
                        {
                            _logger.LogError(new EventId((int)EventIds.SendEmailNotificationError),
                                ex,
                                "Notification - ProcessEmailNotificationRequestsAsync - Adding Attachments - Failed");
                        }
                    }
                }

                // Send request
                response = await _httpHelper.SendRequestAsync(HttpMethod.Post, string.Empty, _functionUrl, JsonConvert.SerializeObject(notificationItem));

                // Get response content
                string content = await response.Content.ReadAsStringAsync();
                logData.EventDetails.Modify(Constant.ResponseStatusCode, response.StatusCode);
                logData.EventDetails.Modify(Constant.ResponseContent, content);

                if (response.IsSuccessStatusCode)
                {
                    // Log success
                    EmailNotificationStatus emailStatus = new EmailNotificationStatus()
                    {
                        PartitionKey = notificationItem.Telemetry?.Xcv,
                        RowKey = notificationItem.Telemetry?.MessageId,
                        Status = "Sending"
                    };

                    await _notificationHelper.LogNotificationStatus<EmailNotificationStatus>(emailStatus, Constant.EmailNotificationStatusTableName);
                    logData.EventDetails.Modify(Constant.AppAction, "Notification - ProcessEmailNotificationRequestsAsync - Success");
                    using (_logger.BeginScope(logData.EventDetails))
                    {
                        _logger.LogInformation(new EventId((int)EventIds.SendEmailNotificationSuccess),
                            "Notification - ProcessEmailNotificationRequestsAsync - Success");
                    }
                    return true;
                }
                else
                {
                    // Log error
                    logData.EventDetails.Modify(Constant.AppAction, "Notification - ProcessEmailNotificationRequestsAsync - Failed");
                    using (_logger.BeginScope(logData.EventDetails))
                    {
                        _logger.LogInformation(new EventId((int)EventIds.SendEmailNotificationFailed),
                            "Notification - ProcessEmailNotificationRequestsAsync - Failed");
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Log error
                logData.EventDetails.Modify(Constant.AppAction, "Notification - ProcessEmailNotificationRequestsAsync - Failed");
                using (_logger.BeginScope(logData.EventDetails))
                {
                    _logger.LogError(new EventId((int)EventIds.SendEmailNotificationError),
                        ex,
                        "Notification - ProcessEmailNotificationRequestsAsync - Failed");
                }
                return false;
            }
        }
    }
}