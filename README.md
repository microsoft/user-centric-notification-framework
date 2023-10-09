# Notification Framework
![GitHub release (latest by date including pre-releases)](https://img.shields.io/github/v/release/microsoft/user-centric-notification-framework?include_prereleases)
[![CodeQL](https://github.com/microsoft/user-centric-notification-framework/actions/workflows/codeql.yml/badge.svg)](https://github.com/microsoft/user-centric-notification-framework/actions/workflows/codeql.yml)
[![OpenSSF Scorecard](https://api.securityscorecards.dev/projects/github.com/microsoft/user-centric-notification-framework/badge)](https://api.securityscorecards.dev/projects/github.com/microsoft/user-centric-notification-framework)

Notification framework is a ready-to-use framework which can be plugged into any application for their notification needs.
The notifications currently supported are:
* Email Notification
* Device Push Notification (iOS/Android/Windows)
* Web Push Notification
* Text Notification

## Getting Started

These instructions will get the project up and running in Azure.

### Pre-requisites

Before running the project on local machine/ or deploy the following things needs to be setup on Azure:
- Azure Subscription
- Azure Key Vault (to store secrets)
- Azure App Configuration (to store all configurations)
- Microsoft Entra ID App
- Azure Notification Hub (for device push notifications)

Apart from these keep the following items handy as it would be required during deployment:
- Microsoft Entra ID App's ClientId 
- Custom Application Name which would be used to create AppServices/Functions (resource_name_prefix)
- Custom Resource Group Name where all the resources will be deployed
- Location/code to deploy the Azure Resources (e.g. Central US/centralus. Powershell Command: Get-AzureRmLocation |Format-Table)
- Provider For Email Notifications
  > Exchange - Service Account Details 
  > 
  > Send Grid - Account Details (this is supported but right now not a part of the current solution)
- Provider for Text Notifications
  > Twilio - Account Details
- Notification Hub Connection information
  
- Key for Web Push Notification. Generate and store the following keys
  > VAPIDPublicKey
  >
  > VAPIDPrivateKey

- Enterprise Email Provider's App Registration Details
  > Microsoft Entra ID App's Client Id
  > 
  > Microsoft Entra ID App's Secret


### Installing

A step by step series of that explains how to get the components deployed in Azure

```
Step 1: Download the ARM template (azuredeploy.json) from the source (Code\ReleaseManagement\ResourceTemplates folder)
```

```
Step 2: Go to Azure Portal and search/select the service 'Deploy a custom template"
```

```
Step 3: Select 'Build your own template in the editor' and paste the content of 'template.json' in the editor
```

```
Step 4: Remove the nodes which are not required ('EmailResources', 'TextResources', 'PushResources') and update the next node's 'dependsOn' property to correctly point to the previous node.
```

```
Step 5: Save and go the next step. Select the subscription, resource group & location.
Update the settings to update any of the parameter values if required and click on purchase 

Note : If there is any failure, try re-deploying again before proceeding for any troubleshooting.
```

## Clean-up
It might have happened that some of the resources which got created may be already present in your subscription. 
In that case, you can continue to use the same and delete the newly created resources. (e.g. Storage Account, Application Insights, ServiceBus - In case of ServiceBus make sure to create the Queues in your exisiting ServiceBus namespace before deleting).

If a specific channel needs to be deployed, the following table will help in deciding which components can be cleaned-up.

| Notification Channel | Components Required |
|--------|------|
| Common | App Configuration |
|  | KeyVault |
|  | Application Insights |
|  | ServiceBus Namespace |
|  | Storage Account |
|  | Storage API Connection |
|  | Notification Common Services Function App (w/ App Service Plan) |
| Email Notification | ServiceBus Queue|
|  | Send Email Function App |
|  | Send Email Logic App |
|  | Office 365 API Connection |
| Text Notification | ServiceBus Queue|
|  | Send Text Function App |
|  | Send Text Logic App |
|  | Twilio API Connection |
| Device Push Notification | ServiceBus Queue|
|  | Send Push Function App |
|  | Notification Hub |
|  | Notification Service API  (w/ App Service Plan) |
| Web Push Notification | ServiceBus Queue|
|  | Send Push Function App |
|  | Notification Service API (w/ App Service Plan) |
| Reminder Notification | ServiceBus Queue|
|  | Send Reminder Function App |

## Setup and Configuration

Once all the components are deployed, go to the below components, copy the access keys and store that in Azure App Configuration.
**The secrets needs to be stored in KeyVault and a key-value reference of the same should be present in Azure App Configuration.**

| Key Name | Source | In KeyVault ? | Required For |
|--------|------|--------|--------|
| AzureAppConfigurationUrl | Azure App Configuration | No | Common |
| ServiceBusNamespace | ServiceBus | No | Common |
| StorageAccountName | Storage Account | No | Common |
| ApplicationInsightsConnectionString | Application Insights | No | Common |
| APPINSIGHTS_INSTRUMENTATIONKEY | Application Insights | No | Common |
| SendEmailWebhookEndpoint | LogicApp - Send Email (without SAS Token) | No | Email |
| SendTextWebhookEndpoint | LogicApp - Send Text (without SAS Token) | No | Text |
| VAPIDPublicKey | Web Push Keys | Yes | Web Push |
| VAPIDPrivateKey | Web Push Keys | Yes | Web Push |
| NotificationHubDefaultFullSharedAccessSignature | Azure Notification Hub | Yes | Device Push |
| HubName | Azure Notification Hub | No | Device Push |
| MustUpdateConfig |  | No | Common |
| IdentityProviderClientId | Microsoft Entra ID | No | Common |
| IdentityProviderAppKey | Microsoft Entra ID | Yes | Common |
| IdentityProviderAuthority | Microsoft Entra ID | No | Common |
| IdentityProviderResource | Microsoft Entra ID | No | Common |
| WebPushNotificationRegistrationTableName |  | No | Web Push |
| DeviceNotificationTemplatesTableName |  | No | Device Push |
| MailQueueName | ServiceBus Queue | No | Email |
| DevicePushQueueName | ServiceBus Queue | No | Device Push |
| WebPushQueueName | ServiceBus Queue | No | Web Push |
| TextQueueName | ServiceBus Queue | No | Text |
| ReminderQueueName | ServiceBus Queue | No | Reminders |
| SendNotification_Url | Function_Url *(#CommonServiceFunctionUrl#/api/NotificationBroadcaster?code=#FunctionKey#)* | No | Reminders |
| IsEnterpriseEmailEnabled | | No | Enterprise Email |
| EESAuthority | Microsoft Entra ID | No | Enterprise Email |
| EESClientId | Microsoft Entra ID | No | Enterprise Email |
| EESAppKey | Microsoft Entra ID | Yes | Enterprise Email |
| EESApiUrl | Enterprise Email Provider | No | Enterprise Email |
| EESResource | Enterprise Email Provider | No | Enterprise Email |


#### Update Application Settings
* Enable (if not enabled) 'Managed Identity' for all the App Services, Function Apps and Logic Apps
* Add the above created identities in the 'Access policies' section of the KeyVault
* For the Function Apps add/update the below AppSetting keys:
  > APPINSIGHTS_INSTRUMENTATIONKEY
  >
  > AzureAppConfigurationUrl
  >
  > FUNCTIONS_EXTENSION_VERSION : ~4
  >
  > FeatureName : --Label in Azure App Configuration--
  > 
  > ValidAppIds : --Microsoft Entra ID App's ClientIds which are authorized to access this component (; separated)--
* For the Azure App Services add/update the below AppSetting keys:
  > APPINSIGHTS_INSTRUMENTATIONKEY
  >
  > AzureAppConfigurationUrl
  >
  > FeatureName : --Label in Azure App Configuration--
  > 
  > ValidAppIds : --Microsoft Entra ID App's ClientIds which are authorized to access this component (; separated)--

* Authorize the API connections (for Logic Apps)
  * Outlook - with the account that will be sending emails, 
  * Twilio
  * Key Vault - with Managed Idenity of the Logic App
  * Storage - with Managed Idenity of the Logic App
    ```
    Note: For your personal outlook account, please add a new outlook (Send an Email(V2)) task instead of Office 365, 
    and fill all the parameters as if in the existing Send an Email task.
    ```
* Setup AuthN for APIs and Function Apps
  * Update the Reply Urls section of the Microsoft Entra ID App created earlier with the URLs of the App Services and FunctionApps (HttpTriggered) URLs suffixed with '/auth/login/aad/callback' 
  * In the 'Authentication' section of the AppServices / FunctionApps (HttpTriggered),
    * Add or update the Authentication values (ClientId/Secret/Issuer/Audience)
    * Select 'Return HTTP 302 Found (Redirect to identity provider)' for the option 'Unauthenticated requests'
    
* Setup AuthZ for Logic Apps
  * Creating an Microsoft Entra ID Authorization Policy
  * Add the following Claims with the appropriate values
    > Issuer
            
    > Audience
    
        Use 'IdentityProviderResource' value
    
    > appid
    
        Use 'IdentityProviderClientId' value

## Local Setup
Configure the following package sources in VS to restore NuGet packages:
- One-Engineering-System@Local
```
https://microsoftit.pkgs.visualstudio.com/_packaging/One-Engineering-System%40Local/nuget/v3/index.json
```
## Deploy
Deploy the code in these new components using AzureDevOps (Build and Release pipelines)

The deployment might fail sometimes due to locked files. Try restarting the service, before redeploying. 
If the issue persists, add the following AppSettings in the service configuration
```
    "MSDEPLOY_RENAME_LOCKED_FILES": "1"
```

- Reference Release [Pipeline](https://microsoftit.visualstudio.com/OneITVSO/_release?_a=releases&view=mine&definitionId=13611) 

## How to Setup to use this framework

See the [SETUP.md](SETUP.md) file for details

## Authors

* **Chinmaya Rath**
* **Nirav Khandhedia** 

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details
