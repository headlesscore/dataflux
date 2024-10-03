using Moq;
using Xunit;
using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Monitoring
{
	public class CachingCruiseServerManagerTest
	{
		private Mock<ICruiseServerManager> wrappedManagerMock;
		private ICruiseServerManager cachingManager;

		//[SetUp]
		public void SetUp()
		{
			wrappedManagerMock = new Mock<ICruiseServerManager>();
			cachingManager = new CachingCruiseServerManager((ICruiseServerManager) wrappedManagerMock.Object);
		}

		[Fact]
		public void ShouldDelegateMostMethodsToWrappedInstance()
		{
            MockSequence sequence = new MockSequence();
            wrappedManagerMock.InSequence(sequence).SetupGet(_manager => _manager.Configuration).
                Returns(new BuildServer("tcp://testUrl")).Verifiable();
            wrappedManagerMock.InSequence(sequence).SetupGet(_manager => _manager.Configuration).
                Returns(new BuildServer("tcp://testUrl")).Verifiable();
			wrappedManagerMock.SetupGet(_manager => _manager.DisplayName).Returns("testDisplayName").Verifiable();
			wrappedManagerMock.Setup(_manager => _manager.CancelPendingRequest("testProjectName")).Verifiable();

            //Assert.Equal("tcp://testUrl", cachingManager.Configuration.Url);
            Assert.Equal("tcp://testUrl", cachingManager.Configuration.Url);
			Assert.Equal("testDisplayName", cachingManager.DisplayName);
			Assert.Equal(BuildServerTransport.Remoting, cachingManager.Configuration.Transport);
			cachingManager.CancelPendingRequest("testProjectName");

		
			wrappedManagerMock.Verify();
		}

		[Fact]
		public void ShouldDelegateFirstSnapshotGet()
		{
			CruiseServerSnapshot snapshot = new CruiseServerSnapshot();
			wrappedManagerMock.Setup(_manager => _manager.GetCruiseServerSnapshot()).Returns(snapshot).Verifiable();

			Assert.Same(snapshot, cachingManager.GetCruiseServerSnapshot());

			wrappedManagerMock.Verify();
		}

		[Fact]
		public void ShouldReturnSecondSnapshotGetWithoutDelegating()
		{
			CruiseServerSnapshot snapshot = new CruiseServerSnapshot();
			wrappedManagerMock.Setup(_manager => _manager.GetCruiseServerSnapshot()).Returns(snapshot).Verifiable();

			Assert.Same(snapshot, cachingManager.GetCruiseServerSnapshot());
			Assert.Same(snapshot, cachingManager.GetCruiseServerSnapshot());

			wrappedManagerMock.Verify();
		}

		[Fact]
		public void ShouldDelegateSnapshotGetAfterCacheCleared()
		{
			CruiseServerSnapshot snapshot1 = new CruiseServerSnapshot();
			CruiseServerSnapshot snapshot2 = new CruiseServerSnapshot();
			wrappedManagerMock.SetupSequence(_manager => _manager.GetCruiseServerSnapshot())
				.Returns(snapshot1)
				.Returns(snapshot2);

			Assert.Same(snapshot1, cachingManager.GetCruiseServerSnapshot());
			Assert.Same(snapshot1, cachingManager.GetCruiseServerSnapshot());
			((ICache) cachingManager).InvalidateCache();
			Assert.Same(snapshot2, cachingManager.GetCruiseServerSnapshot());

			wrappedManagerMock.Verify(_manager => _manager.GetCruiseServerSnapshot(), Times.Exactly(2));
		}	
	}
}
