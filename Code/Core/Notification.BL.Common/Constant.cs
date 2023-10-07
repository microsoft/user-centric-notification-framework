// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.BL.Common
{
    public static class Constant
    {
        // App Configuration Constants

        public const string AzureAppConfigurationUrl = "AzureAppConfigurationUrl";
        public const string ServiceBusNamespace = "ServiceBusNamespace:fullyQualifiedNamespace";
        public const string StorageAccountName = "StorageAccountName";
        public const string ApplicationInsightsConnectionString = "APPLICATIONINSIGHTS_CONNECTION_STRING";
        public const string SendEmailWebhookEndpoint = "SendEmailWebhookEndpoint";
        public const string SendTextWebhookEndpoint = "SendTextWebhookEndpoint";
        public const string SendCustomWebhookEndpoint = "SendCustomWebhookEndpoint";
        public const string SendNotificationUrl = "SendNotification_Url";
        public const string VapidPublicKey = "VAPIDPublicKey";
        public const string VapidPrivateKey = "VAPIDPrivateKey";
        public const string NotificationHubDefaultFullSharedAccessSignature = "NotificationHubDefaultFullSharedAccessSignature";
        public const string HubName = "HubName";
        public const string MustUpdateConfig = "MustUpdateConfig";
        public const string IdentityProviderClientId = "IdentityProviderClientId";
        public const string IdentityProviderAppKey = "IdentityProviderAppKey";
        public const string IdentityProviderAuthority = "IdentityProviderAuthority";
        public const string IdentityProviderResource = "IdentityProviderResource";
        public const string WebPushNotificationRegistrationTableName = "WebPushNotificationRegistration";
        public const string DeviceNotificationTemplatesTableName = "DeviceNotificationTemplates";
        public const string MailQueueName = "MailQueueName";
        public const string DevicePushQueueName = "DevicePushQueueName";
        public const string WebPushQueueName = "WebPushQueueName";
        public const string TextQueueName = "TextQueueName";
        public const string CustomQueueName = "CustomQueueName";
        public const string QueueName = "QueueName";
        public const string ReminderQueueName = "reminders";
        public const string IsEnterpriseEmailEnabled = "IsEnterpriseEmailEnabled";
        public const string EnterpriseEmailApiUrl = "EESApiUrl";
        public const string EnterpriseEmailAuthority = "EESAuthority";
        public const string EnterpriseEmailClientId = "EESClientId";
        public const string EnterpriseEmailAppKey = "EESAppKey";
        public const string EnterpriseEmailResource = "EESResource";

        public const string CompanyName = "Microsoft";
        public const string Bearer = "Bearer";
        public const string ApplicationJson = "application/json";
        public const string UserPrincipalName = "UserPrincipalName";
        public const string VapidDetails = "vapidDetails";
        public const string TTL = "TTL";
        public const string EndPoint = "EndPoint";
        public const string TemplateContent = "TemplateContent";

        public const string FeatureName = "FeatureName";
        public const string EmailNotificationStatusTableName = "EmailNotificationStatus";
        public const string EESNotificationStatusTableName = "EESNotificationStatus";
        public const string NotificationStatusTableName = "NotificationStatus";

        // Logging

        public const string ApplicationName = "ApplicationName";
        public const string TenantIdentifier = "TenantIdentifier";
        public const string BusinessProcessName = "BusinessProcessName";
        public const string ActionUri = "ActionUri";
        public const string ComponentType = "ComponentType";
        public const string AppAction = "AppAction";
        public const string Request = "Request";
        public const string ResponseStatusCode = "ResponseStatusCode";
        public const string ResponseContent = "ResponseContent";
        public const string Xcv = "Xcv";
        public const string MessageId = "MessageId";
        public const string UserAlias = "UserAlias";
        public const string Id = "Id";
        public const string NotificationTypes = "NotificationTypes";
        public const string To = "To";
        public const string BatchId = "BatchId";

        // Message

        public const string AttachmentsBlobContainer = "attachments";
    }
}