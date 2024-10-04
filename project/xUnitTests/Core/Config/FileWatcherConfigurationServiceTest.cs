using System;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core.Config;
using ThoughtWorks.CruiseControl.UnitTests.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Config
{
	
	public class FileWatcherConfigurationServiceTest
	{
		private FileWatcherConfigurationService fileService;
		private MockFileWatcher fileWatcher;

        // [TearDown]
        public void TearDown()
        {
            (fileWatcher as IDisposable)?.Dispose();
            fileService = null;
        }

		// [SetUp]
		public void Setup()
		{
			fileWatcher = new MockFileWatcher();
			var mockService = new Mock<IConfigurationService>();
			fileService = new FileWatcherConfigurationService((IConfigurationService) mockService.Object, fileWatcher);
		}

		[Fact]
		public void CallsUpdateHandlersWhenFileWatcherChanges()
		{
			// Setup
			fileService.AddConfigurationUpdateHandler(new ConfigurationUpdateHandler(OnUpdate));
			updateCalled = false;

			// Execute
			fileWatcher.RaiseEvent();

			// Verify
			Assert.True(updateCalled);
            Assert.True(updateCalled);
        }

		private bool updateCalled = false;

		public void OnUpdate()
		{
			updateCalled = true;
		}
	}
}
