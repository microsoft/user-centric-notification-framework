# Notification Guide

This guide explains how tenants can use the Notification Framework to send emails, schedule them for later, set up recurring reminders, and cancel pending notifications.

## Sending an Email

To send an email, POST to the `NotificationBroadcaster` endpoint with `notificationTypes` set to `"0"` (Mail) or `"1"` (Actionable Email).

```json
POST /api/NotificationBroadcaster
{
  "notificationTypes": ["0"],
  "to": "recipient@domain.com",
  "from": "sender@domain.com",
  "subject": "Your expense has been submitted",
  "body": "...",
  "tenantIdentifier": "1",
  "telemetry": {
    "xcv": "some-correlation-id",
    "messageId": "some-message-id"
  }
}
```

The email is sent immediately. The response confirms whether the request was accepted:

```json
{
  "actionResult": true,
  "displayMessage": "Message sent successfully",
  "sequenceNumber": 0,
  "telemetry": {
    "xcv": "some-correlation-id",
    "messageId": "some-message-id"
  }
}
```

## Scheduling an Email for Later

To schedule an email to be sent at a specific time, include the `sendOnUtcDate` field. The email will be held and delivered at the specified UTC time.

```json
POST /api/NotificationBroadcaster
{
  "notificationTypes": ["0"],
  "to": "recipient@domain.com",
  "subject": "Monthly report is ready",
  "sendOnUtcDate": "2025-02-15T09:00:00Z",
  "tenantIdentifier": "1",
  "telemetry": {
    "xcv": "some-correlation-id",
    "messageId": "some-message-id"
  }
}
```

> **Note:** Cancellation of scheduled emails is not currently supported. Once a scheduled email is queued, it will be delivered at the specified time.

## Setting Up Reminder Emails

Reminders allow you to send the same notification repeatedly at a set interval until an expiration date. This is useful for scenarios like pending approvals where the recipient needs periodic nudges.

To set up reminders, include a `reminder` object in the payload:

```json
POST /api/NotificationBroadcaster
{
  "notificationTypes": ["0"],
  "to": "approver@domain.com",
  "subject": "Expense ER-123 needs your approval",
  "tenantIdentifier": "1",
  "reminder": {
    "notificationTypes": ["0"],
    "frequency": 24,
    "expirationDate": "2025-03-01T00:00:00Z"
  },
  "telemetry": {
    "xcv": "some-correlation-id",
    "messageId": "some-message-id"
  }
}
```

### Reminder fields

| Field | Description |
|---|---|
| `notificationTypes` | Which notification channels to use for reminders (e.g., `"0"` for email). This can differ from the initial notification. |
| `frequency` | How often to send reminders, in **hours**. For example, `24` means once a day. |
| `expression` | Alternatively, a CRON expression for more complex schedules (e.g., `"0 12 */10 * *"` for every 10 days at noon). Use either `frequency` or `expression`, not both. If `frequency` is greater than 0, it takes precedence. |
| `expirationDate` | When to stop sending reminders (UTC). No reminders will be sent after this date. |

### How it works

1. The **first email** is sent immediately
2. The **first reminder** is scheduled based on the `frequency` or `expression`
3. When a reminder fires, a new email is sent with `"Reminder: "` prefixed to the subject
4. The next reminder is automatically scheduled, repeating until the `expirationDate` is reached

### Response

The response includes a `sequenceNumber` and confirms whether the request was accepted:

```json
{
  "actionResult": true,
  "displayMessage": "Message sent successfully",
  "sequenceNumber": 12345
}
```

## Cancelling Reminders

To stop a reminder chain, send a cancel request with the `id` from your original notification:

```json
POST /api/NotificationBroadcaster
{
  "notificationTypes": ["8"],
  "id": "D7A224A3-0555-41CA-AB3F-FBA9A1EDBFF9",
  "tenantIdentifier": "1",
  "telemetry": {
    "xcv": "some-correlation-id",
    "messageId": "some-message-id"
  }
}
```

The system looks up the latest scheduled reminder internally using the `id`, so cancellation works regardless of how many reminder cycles have already passed.

> **Note:** Emails and reminders that have already been delivered cannot be recalled.

## Notification Types Reference

| Value | Type | Description |
|---|---|---|
| `0` | Mail | Standard email |
| `1` | ActionableEmail | Email with adaptive card content |
| `2` | Tile | Device push (tile) |
| `3` | Toast | Device push (toast) |
| `4` | Badge | Device push (badge) |
| `5` | Raw | Device push (raw) |
| `6` | WebPush | Browser push notification |
| `7` | Text | SMS/text notification |
| `8` | Cancel | Cancel a scheduled reminder |

## Payload Reference

All requests are sent as a JSON POST to the `NotificationBroadcaster` endpoint. Below is the complete payload with all fields, followed by a description of each field and which notification types it applies to.

### Full payload

```json
{
  "notificationTypes": ["0"],
  "id": "D7A224A3-0555-41CA-AB3F-FBA9A1EDBFF9",
  "applicationName": "Expense",
  "to": "recipient@domain.com",
  "from": "sender@domain.com",
  "cc": "cc-recipient@domain.com",
  "bcc": "bcc-recipient@domain.com",
  "subject": "Your expense has been submitted",
  "body": "<html>...</html>",
  "deeplinkUrl": "https://expense.contoso.com/details/123",
  "webPushnotificationTag": "expense-tag-123",
  "sendOnUtcDate": "2025-02-15T09:00:00Z",
  "emailAccountNumberToUse": "0",
  "culture": "en-US",
  "tenantIdentifier": "1",
  "templateId": "ExpenseSubmitted|None",
  "templateData": {
    "DocumentNumber": "ER-0000098473",
    "TotalAmount": "123",
    "TransactionCurrency": "USD"
  },
  "attachments": [
    {
      "fileName": "receipt.pdf",
      "fileBase64": "base64-encoded-content",
      "fileType": "application/pdf",
      "fileUrl": "https://storageaccount.blob.core.windows.net/container/receipt.pdf",
      "cid": "receipt-image-1"
    }
  ],
  "reminder": {
    "notificationTypes": ["0"],
    "frequency": 24,
    "expression": "0 12 */10 * *",
    "expirationDate": "2025-03-01T00:00:00Z"
  },
  "sequenceNumber": 12345,
  "telemetry": {
    "xcv": "D7A224A3-0555-41CA-AB3F-FBA9A1EDBFF9",
    "messageId": "562F3BAE-79F4-47E1-B21D-44B22673DBC8"
  }
}
```

### Field descriptions

#### Common fields (all notification types)

| Field | Type | Required | Description |
|---|---|---|---|
| `notificationTypes` | `string[]` | Yes | Array of notification types to send. See [Notification Types Reference](#notification-types-reference). Multiple types can be specified to send the same notification across different channels. |
| `id` | `string` | No | A unique identifier for the notification. Used for tracking, logging, and **required for cancelling reminders**. If you plan to cancel reminders, always include this. |
| `applicationName` | `string` | No | Name of the calling application (e.g., `"Expense"`). Used for logging and as a label in push notifications. |
| `tenantIdentifier` | `string` | Yes | Identifies the tenant. Used to partition data in blob storage and table storage. |
| `sendOnUtcDate` | `datetime` | No | UTC date/time to schedule the notification for later delivery. If omitted or empty, the notification is sent immediately. |
| `culture` | `string` | No | Culture/locale string (e.g., `"en-US"`). |
| `telemetry` | `object` | Yes | Tracking identifiers for the notification. |
| `telemetry.xcv` | `string` | Yes | Correlation vector for end-to-end tracing across services. |
| `telemetry.messageId` | `string` | Yes | Unique message identifier for tracking individual notifications. |

#### Email fields (`Mail`, `ActionableEmail`)

| Field | Type | Required | Description |
|---|---|---|---|
| `from` | `string` | No | Sender email address. |
| `to` | `string` | Yes | Recipient email address. |
| `cc` | `string` | No | CC recipient email address. |
| `bcc` | `string` | No | BCC recipient email address. |
| `subject` | `string` | Yes | Email subject line. |
| `body` | `string` | No | Email body content (HTML supported). If using templates, this can be left empty. |
| `templateId` | `string` | No | Identifier of the email template stored in Azure Table Storage (format: `TemplateName\|Variant`). |
| `templateData` | `object` | No | Key-value pairs used to replace `#placeholders#` in the template content. |
| `emailAccountNumberToUse` | `string` | No | Which email account to use for sending (default `"0"`). Useful when multiple accounts are configured to handle sending limits. |
| `attachments` | `object[]` | No | Array of file attachments (see below). |

#### Attachment fields (Email only)

| Field | Type | Required | Description |
|---|---|---|---|
| `fileName` | `string` | Yes | Name of the file (e.g., `"receipt.pdf"`). |
| `fileBase64` | `string` | No | Base64-encoded file content. Use this **or** `fileUrl`, not both. |
| `fileUrl` | `string` | No | Blob storage URL of the file. The framework will download and attach it automatically. Use this **or** `fileBase64`, not both. |
| `fileType` | `string` | No | MIME type of the file (e.g., `"application/pdf"`). |
| `cid` | `string` | No | Content ID for inline/embedded images in HTML email bodies. |

#### Web Push fields (`WebPush`)

| Field | Type | Required | Description |
|---|---|---|---|
| `to` | `string` | Yes | User alias/email. Used to look up the user's registered push subscription endpoints. |
| `subject` | `string` | Yes | Notification title displayed in the browser. |
| `body` | `string` | Yes | Notification body text. |
| `deeplinkUrl` | `string` | No | URL to open when the user clicks the notification. |
| `webPushnotificationTag` | `string` | No | Tag for the notification. Notifications with the same tag replace each other in the browser. |

#### Device Push fields (`Tile`, `Toast`, `Badge`, `Raw`)

| Field | Type | Required | Description |
|---|---|---|---|
| `to` | `string` | Yes | User alias/email. Used as a tag to target specific device registrations in Azure Notification Hub. |
| `templateData` | `object` | Yes | Key-value pairs used to fill `#placeholders#` in the device notification templates stored in Azure Table Storage. |

#### Text/SMS fields (`Text`)

| Field | Type | Required | Description |
|---|---|---|---|
| `to` | `string` | Yes | User alias/email. Used to look up the user's phone number from Azure Table Storage. |
| `body` | `string` | Yes | Text message content. |

#### Reminder fields (optional, works with any notification type)

| Field | Type | Required | Description |
|---|---|---|---|
| `reminder.notificationTypes` | `string[]` | Yes | Which channels to use for the reminder notifications. Can differ from the initial notification types. |
| `reminder.frequency` | `number` | No | How often to send reminders, in hours (e.g., `24` = once a day). Takes precedence over `expression` if greater than 0. |
| `reminder.expression` | `string` | No | CRON expression for complex schedules (e.g., `"0 12 */10 * *"`). Used only if `frequency` is 0 or not set. |
| `reminder.expirationDate` | `datetime` | Yes | UTC date/time after which no more reminders will be sent. |

#### Cancel fields (`Cancel`)

| Field | Type | Required | Description |
|---|---|---|---|
| `id` | `string` | Yes | The notification `id` from the original request. The system looks up the latest scheduled reminder sequence number from this. |
| `sequenceNumber` | `long` | No | Optional fallback. Only used if the `id` lookup fails (e.g., table unavailable). Not needed under normal circumstances. |

### Response

All requests return the following response:

```json
{
  "tenantIdentifier": "1",
  "actionResult": true,
  "displayMessage": "Message sent successfully",
  "sequenceNumber": 12345,
  "telemetry": {
    "xcv": "D7A224A3-0555-41CA-AB3F-FBA9A1EDBFF9",
    "messageId": "562F3BAE-79F4-47E1-B21D-44B22673DBC8"
  },
  "e2EErrorInformation": null
}
```

| Field | Type | Description |
|---|---|---|
| `actionResult` | `boolean` | `true` if the notification was successfully queued, `false` otherwise. |
| `displayMessage` | `string` | Human-readable status message. |
| `sequenceNumber` | `long` | Service Bus sequence number of the last scheduled message. Save this if you need to cancel a reminder later. For immediate (non-scheduled) notifications, this will be `0`. |
| `telemetry` | `object` | Echo of the correlation identifiers from the request. |
| `e2EErrorInformation` | `object` | Error details if `actionResult` is `false`. Contains `errorMessages` (list of failure descriptions), `errorType`, and `retryInterval`. |
