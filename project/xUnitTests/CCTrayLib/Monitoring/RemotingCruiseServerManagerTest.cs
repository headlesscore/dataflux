using System;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.Remote;
using ThoughtWorks.CruiseControl.Remote.Messages;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Monitoring
{
	public class RemotingCruiseServerManagerTest
	{
		private const string ServerUrl = "tcp://blah:1000/";
        private MockRepository mocks = new MockRepository(MockBehavior.Default);
        private CruiseServerClientBase cruiseManagerMock;
		BuildServer buildServer;
		RemotingCruiseServerManager manager;

		//// [SetUp]
		public void SetUp()
		{
			cruiseManagerMock = mocks.Create<CruiseServerClientBase>(MockBehavior.Strict).Object;

			buildServer = new BuildServer(ServerUrl);
			manager = new RemotingCruiseServerManager(cruiseManagerMock, buildServer);
		}

		[Fact]
		public void InitialisingReturnsCorrectServerProperties()
		{
			Assert.Equal(ServerUrl, manager.Configuration.Url);
            //Assert.Equal(ServerUrl, manager.Configuration.Url);
            Assert.Equal(@"blah:1000", manager.DisplayName);
			Assert.Equal(BuildServerTransport.Remoting, manager.Configuration.Transport);
		}

		[Fact]
		public void RetrieveSnapshotFromManager()
		{
            var snapshot= new CruiseServerSnapshot();
            Mock.Get(cruiseManagerMock).Setup(_cruiseManagerMock => _cruiseManagerMock.GetCruiseServerSnapshot())
                .Returns(snapshot);

            CruiseServerSnapshot result = manager.GetCruiseServerSnapshot();
			Assert.Equal(snapshot, result);
		}
	}
}
