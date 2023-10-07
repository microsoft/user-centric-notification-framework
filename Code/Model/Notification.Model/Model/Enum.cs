// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.Model.Model
{
    public enum EventIds
    {
        NotificationBroadcasterInitiated = 1000,
        NotificationBroadcasterProcessing = 1001,
        NotificationBroadcasterSuccess = 1002,
        NotificationBroadcasterIgnore = 1003,
        NotificationBroadcasterFailed = 1004,
        NotificationBroadcasterError = 1005,
        NotificationBroadcasterComplete = 1006,
        NotificationBroadcasterPerfomance = 1007,

        DataMapperProcessInitiated = 2000,
        DataMapperProcessProcessing = 2001,
        DataMapperProcessSuccess = 2002,
        DataMapperProcessIgnore = 2003,
        DataMapperProcessFailed = 2004,
        DataMapperProcessError = 2005,
        DataMapperProcessComplete = 2006,
        DataMapperProcessPerfomance = 2007,

        SendEmailNotificationInitiated = 3000,
        SendEmailNotificationProcessing = 3001,
        SendEmailNotificationSuccess = 3002,
        SendEmailNotificationIgnore = 3003,
        SendEmailNotificationFailed = 3004,
        SendEmailNotificationError = 3005,
        SendEmailNotificationComplete = 3006,
        SendEmailNotificationPerfomance = 3007,

        SendDevicePushNotificationInitiated = 4000,
        SendDevicePushNotificationProcessing = 4001,
        SendDevicePushNotificationSuccess = 4002,
        SendDevicePushNotificationIgnore = 4003,
        SendDevicePushNotificationFailed = 4004,
        SendDevicePushNotificationCompleted = 4005,
        SendDevicePushNotificationError = 4006,
        SendDevicePushNotificationPerformance = 4007,

        SendWebPushNotificationInitiated = 5000,
        SendWebPushNotificationProcessing = 5001,
        SendWebPushNotificationSuccess = 5002,
        SendWebPushNotificationIgnore = 5003,
        SendWebPushNotificationFailed = 5004,
        SendWebPushNotificationCompleted = 5005,
        SendWebPushNotificationError = 5006,
        SendWebPushNotificationPerformance = 5007,

        SendTextNotificationInitiated = 6000,
        SendTextNotificationProcessing = 6001,
        SendTextNotificationSuccess = 6002,
        SendTextNotificationIgnore = 6003,
        SendTextNotificationFailed = 6004,
        SendTextNotificationCompleted = 6005,
        SendTextNotificationError = 6006,
        SendTextNotificationPerformance = 6007,

        WebPushRegistrationInitiated = 7000,
        WebPushRegistrationProcessing = 7001,
        WebPushRegistrationSuccess = 7002,
        WebPushRegistrationIgnore = 7003,
        WebPushRegistrationFailed = 7004,
        WebPushRegistrationCompleted = 7005,
        WebPushRegistrationError = 7006,
        WebPushRegistrationPerfomance = 7007,

        DevicePushRegistrationInitiated = 8000,
        DevicePushRegistrationProcessing = 8001,
        DevicePushRegistrationSuccess = 8002,
        DevicePushRegistrationIgnore = 8003,
        DevicePushRegistrationFailed = 8004,
        DevicePushRegistrationCompleted = 8005,
        DevicePushRegistrationError = 8006,
        DevicePushRegistrationPerfomance = 8007,

        SendCustomNotificationInitiated = 9000,
        SendCustomNotificationProcessing = 9001,
        SendCustomNotificationSuccess = 9002,
        SendCustomNotificationIgnore = 9003,
        SendCustomNotificationFailed = 9004,
        SendCustomNotificationError = 9005,
        SendCustomNotificationComplete = 9006,
        SendCustomNotificationPerfomance = 9007,

        SendReminderNotificationInitiated = 10000,
        SendReminderNotificationProcessing = 10001,
        SendReminderNotificationSuccess = 10002,
        SendReminderNotificationIgnore = 10003,
        SendReminderNotificationFailed = 10004,
        SendReminderNotificationError = 10005,
        SendReminderNotificationComplete = 10006,
        SendReminderNotificationPerfomance = 10007,
        SendReminderNotificationStatusUpdateFailure = 10008,

        SendEnterpriseEmailNotificationInitiated = 11000,
        SendEnterpriseEmailNotificationProcessing = 11001,
        SendEnterpriseEmailNotificationSuccess = 11002,
        SendEnterpriseEmailNotificationIgnore = 11003,
        SendEnterpriseEmailNotificationFailed = 11004,
        SendEnterpriseEmailNotificationError = 11005,
        SendEnterpriseEmailNotificationComplete = 11006,
        SendEnterpriseEmailNotificationPerfomance = 11007,
        SendEnterpriseEmailNotificationStatusUpdateFailure = 11008,

        GetEmailStatusInitiated = 12000,
        GetEmailBatchStatusSuccess = 12002,
        GetEmailBatchStatusFailed = 12004,
        GetEmailStatusFailure = 12008,

        NotificationStatusUpdateFailure = 13008,

        GenericError = 9999
    }

    public enum LogType
    {
        Information = 1,
        Error = 2,
        Warning = 3,
    }

    public enum ComponentType
    {
        Web,
        SmartApp,
        Device,
        BackgroundProcess,
        WebService,
        Executable,
        DynamicLinkLibrary,
        JobService,
        WorkflowComponent,
        DataStore,
        Other
    }
}