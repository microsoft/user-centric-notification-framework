## How to Setup to use this framework
### Email Notification

Add the following table in Azure Table Storage (name already configured in LogicApp)
> EmailNotificationTemplates

with the following schema

| Column Name | DataType | Notes |
|--------|------|--------|
| PartitionKey | String | Tenant Identifer - Integer (specified in the notification payload) |
| RowKey | String | Email template name (specified in the notification payload) |
| TemplateContent | String | Html email template with placeholders in the format #placeholders# |

Sample Email Template for Actionable Emails:
~~~~
<html>
<head>
<meta http-equiv="Content-Type" content="text/html; charset=us-ascii">
<script type="application/adaptivecard+json">#AdaptiveJSON#</script></head>
<body>
</body>
</html>
~~~~

#### Additional features (Optional)

##### Option to send more than 5k emails per day

Microsoft 365 or Outlook accounts generally have limitation on number of emails that can be sent per day, 
for personal subscription it is 5000 emails per day. To overcome this limitation this framework provides an 
option to configure more than one account.

##### Update the EmailNotification Logic app 

1. Update the Max Email Account Count
2. Add Connections
3. In the Send Email step, configure the switch case with new step for each account and update connection
4. Optionally you can pass which account to be used by default by passing emailAccountNumberToUse parameter of the email payload

### Enterprise Email Notification
Refer [ENTERPRISE_EMAIL_SETUP.md](Code/Domain/Internal/ENTERPRISE_EMAIL_SETUP.md) file for details

### Text Notification

Add the following table in Azure Table Storage (name already configured in LogicApp)
> UserPreferenceSetting

with the following schema (it will be automatically created as this is Object Oriented Storage)

| Column Name | DataType | Notes |
|--------|------|--------|
| PartitionKey | String | User UPN (e.g. alias@domain.com) which is going to be unique |
| RowKey | String | Identifier - GUID |
| PhoneNumber | String | Phone number of the user with area code. |

```
Note: Please follow the guidelines of storing and accessing user's personal information as per the governing rules & policies
```


### Web Push Notification

Add the following table in Azure Table Storage (name already added in App Configuration)
> WebPushNotificationRegistration

with the following schema (it will be automatically created as this is Object Oriented Storage)

| Column Name | DataType | Notes |
|--------|------|--------|
| PartitionKey | String | User UPN (e.g. alias@domain.com) which is going to be unique |
| RowKey | String | Identifier - GUID |
| UserAlias | String | User UPN (e.g. alias@domain.com) which is going to be unique |
| EndPoint | String | Property of the PushSubscription interface returns a [USVString](https://developer.mozilla.org/en-US/docs/Web/API/USVString) containing the endpoint associated with the push subscription. |
| Auth | String | An authentication secret, as described in [Message Encryption for Web Push](https://tools.ietf.org/html/draft-ietf-webpush-encryption-08). |
| P256DH | String | An [Elliptic curve Diffie�Hellman](https://en.wikipedia.org/wiki/Elliptic_curve_Diffie%E2%80%93Hellman) public key on the P-256 curve (that is, the NIST secp256r1 elliptic curve).  The resulting key is an uncompressed point in ANSI X9.62 format. |
| ExpirationTime | String | Property of the PushSubscription interface returns a [DOMHighResTimeStamp](https://developer.mozilla.org/en-US/docs/Web/API/DOMHighResTimeStamp) of the subscription expiration time associated with the push subscription, if there is one, or null otherwise. |


```
Note: Please follow the guidelines of storing and accessing user's personal information as per the governing rules & policies
```

### Device Push Notification

Add the following table in Azure Table Storage (name already added in App Configuration)
> DeviceNotificationTemplates

with the following schema

| Column Name | DataType | Notes |
|--------|------|--------|
| PartitionKey | String | Notification Type e.g. Badge, Raw, Tile, Toast |
| RowKey | String | Form factor's underlying service where the notifcation will be sent e.g. wns, apns, fcm |
| TemplateContent | String | XML/JSON based template with placeholders in the format #placeholders# |

```
Note: The XML/JSON notations for each provider (Android/Windows/iOS) are different. Please refer to the existing guidelines.
```

Sample Android (fcm) Template for Toast Notification:
~~~~
{"data":{"AppName":"#AppName#","DocNumber":"#DocNumber#","RequestorAlias":"#RequestorAlias#"}}
~~~~

Sample iOS (apns) Template for Toast Notification:
~~~~
{"aps": {"AppName":"#AppName#","DocNumber":"#DocNumber#","RequestorAlias":"#RequestorAlias#"}}
~~~~

Sample Windows (wns) Template for Toast Notification:
~~~~
<?xml version="1.0" encoding="utf-8"?><toast><visual version='2' lang='en-US'><binding template='ToastText01'><text id='1'>#ToastBody#</text></binding></visual></toast>
~~~~

## Test the framework

Use Postman or any other tool to send the below request to the APIM or FunctionProxy endpoint
and pass the NotificationType based on what type of notifications are needed

```
Mail = 0,
ActionableEmail = 1,
Tile = 2,
Toast = 3,
Badge = 4,
Raw = 5,
WebPush = 6,
Text = 7,
Cancel = 8
```

~~~~
curl --location --request POST '#CommonServiceFunctionUrl#/api/NotificationBroadcaster?code=#FunctionKey#' \
--header 'Content-Type: application/json' \
--header 'Authorization: Bearer token' \
--data-raw '{
  "notificationTypes": [
    "0"
  ],
  "id": "D7A224A3-0555-41CA-AB3F-FBA9A1EDBFF9",
  "applicationName": "Expense",
  "notificationTag": "F29C1D04-4546-4813-BBAD-2B16E9938BEE",
  "deeplinkUrl": "http://expense",
  "sendOnUtcDate": "",
  "from": "alias@domain.com",
  "to": "alias@domain.com",
  "cc": "alias@domain.com",
  "bcc": "alias@domain.com",
  "subject": "Your Expense has been successfully submitted!",
  "body": "",
  "attachments": [
    {
      "fileName": "testing.txt",
      "fileBase64": "VGhpcyBpcyBhIHRlc3QgZmlsZSBmb3IgYXR0YWNobWVudHMu",
      "fileUrl": ""
    },
    {
      "fileName": "testing2.txt",
      "fileBase64": "VGhpcyBpcyBhIHRlc3QgZmlsZSBmb3IgYXR0YWNobWVudHMu",
      "fileUrl": ""
    }
  ],
  "reminder": {
    "notificationTypes": [
      "0"
    ],
    "frequency":"3",
    "expression": "0 12 */10 * *",
    "expirationDate": "2022-01-01T00:00:00Z"
  },
  "emailAccountNumberToUse": "0",
  "tenantIdentifier": "1",
  "templateId": "ExpenseSubmitted|None",
  "templateData": {
    "DocumentNumber": "ER-0000098473",
    "TotalAmount": "123",
    "TransactionCurrency": "USD"
  },
  "telemetry": {
    "xcv": "D7A224A3-0555-41CA-AB3F-FBA9A1EDBFF9",
    "messageId": "562F3BAE-79F4-47E1-B21D-44B22673DBC8"
  }
}'
~~~~


