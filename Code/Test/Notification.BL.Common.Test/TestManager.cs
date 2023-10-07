// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.BL.Common.Test
{
    using System.Threading;
    using Azure.Messaging.ServiceBus;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Newtonsoft.Json;
    using Notification.BL.Common.Test.Properties;
    using Notification.Contract;

    public abstract class TestManager
    {
        public Mock<IConfiguration> MockConfig { get; set; }
        public Mock<ServiceBusSender> MockMessageSender { get; set; }
        public NotificationItem NotificationItem { get; set; }

        public void TestManagerInitializer()
        {
            MockConfig = new Mock<IConfiguration>();
            MockMessageSender = new Mock<ServiceBusSender>();

            MockMessageSender.Setup(x => x.SendMessageAsync(It.IsAny<ServiceBusMessage>(), CancellationToken.None)).Verifiable();

            NotificationItem = JsonConvert.DeserializeObject<NotificationItem>(Resources.NotificationItem);
        }
    }
}