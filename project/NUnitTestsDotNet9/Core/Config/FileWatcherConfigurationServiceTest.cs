using System;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Config;
using ThoughtWorks.CruiseControl.UnitTests.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Config
{
	[TestFixture]
	public class FileWatcherConfigurationServiceTest
	{
		private FileWatcherConfigurationService fileService;
		private MockFileWatcher fileWatcher;

        [TearDown]
        public void TearDown()
        {
            (fileWatcher as IDisposable)?.Dispose();
            fileService = null;
        }

		[SetUp]
		public void Setup()
		{
			fileWatcher = new MockFileWatcher();
			var mockService = new Mock<IConfigurationService>();
			fileService = new FileWatcherConfigurationService((IConfigurationService) mockService.Object, fileWatcher);
		}

		[Test]
		public void CallsUpdateHandlersWhenFileWatcherChanges()
		{
			// Setup
			fileService.AddConfigurationUpdateHandler(new ConfigurationUpdateHandler(OnUpdate));
			updateCalled = false;

			// Execute
			fileWatcher.RaiseEvent();

			// Verify
			ClassicAssert.IsTrue(updateCalled);
            ClassicAssert.IsTrue(updateCalled);
        }

		private bool updateCalled = false;

		public void OnUpdate()
		{
			updateCalled = true;
		}
	}
}
