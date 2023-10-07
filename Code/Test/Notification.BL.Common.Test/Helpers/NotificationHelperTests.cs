// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Notification.BL.Common.Interface;
using Notification.BL.Common.Test;
using Notification.Data.Azure.Storage.Interface;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]

namespace Notification.BL.Common.Helpers.Tests
{
    [TestClass()]
    public class NotificationHelperTests : TestManager
    {
        private NotificationHelper _notificationHelper;
        private Mock<IUtilityHelper> _mockUtilityHelper;
        private Mock<ITableHelper> _mockTableHelper;
        private Mock<IBlobStorageHelper> _mockBlobStorageHelper;
        private Mock<ILogger<NotificationHelper>> _mockNotificationLogger;

        private readonly List<string> queueNames = new List<string>()
        {
            "queue1",
            "queue2",
        };

        [TestInitialize]
        public void Initialize()
        {
            base.TestManagerInitializer();
            _mockNotificationLogger = new Mock<ILogger<NotificationHelper>>();
            _mockUtilityHelper = new Mock<IUtilityHelper>();
            _mockTableHelper = new Mock<ITableHelper>();
            _mockBlobStorageHelper = new Mock<IBlobStorageHelper>();
            _notificationHelper = new NotificationHelper(MockConfig.Object, _mockUtilityHelper.Object, _mockTableHelper.Object, _mockBlobStorageHelper.Object, _mockNotificationLogger.Object);

            SetupMocks();
        }

        [TestMethod()]
        public async Task BroadcastNotificationsToProvidersTest()
        {
            await _notificationHelper.BroadcastNotificationsToProviders(NotificationItem);
        }

        [TestMethod()]
        public void ReplaceTest()
        {
            //await _notificationHelper.Replace(notificationTypes, templateData, templateContent);
        }

        private void SetupMocks()
        {
        }
    }
}