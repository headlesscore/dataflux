using System;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.Remote;
using ThoughtWorks.CruiseControl.Remote.Messages;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Monitoring
{
	[TestFixture]
	public class RemotingCruiseServerManagerTest
	{
		private const string ServerUrl = "tcp://blah:1000/";
        private MockRepository mocks = new MockRepository(MockBehavior.Default);
        private CruiseServerClientBase cruiseManagerMock;
		BuildServer buildServer;
		RemotingCruiseServerManager manager;

		[SetUp]
		public void SetUp()
		{
			cruiseManagerMock = mocks.Create<CruiseServerClientBase>(MockBehavior.Strict).Object;

			buildServer = new BuildServer(ServerUrl);
			manager = new RemotingCruiseServerManager(cruiseManagerMock, buildServer);
		}

		[Test]
		public void InitialisingReturnsCorrectServerProperties()
		{
			ClassicAssert.AreEqual(ServerUrl, manager.Configuration.Url);
            //ClassicAssert.AreEqual(ServerUrl, manager.Configuration.Url);
            ClassicAssert.AreEqual(@"blah:1000", manager.DisplayName);
			ClassicAssert.AreEqual(BuildServerTransport.Remoting, manager.Configuration.Transport);
		}

		[Test]
		public void RetrieveSnapshotFromManager()
		{
            var snapshot= new CruiseServerSnapshot();
            Mock.Get(cruiseManagerMock).Setup(_cruiseManagerMock => _cruiseManagerMock.GetCruiseServerSnapshot())
                .Returns(snapshot);

            CruiseServerSnapshot result = manager.GetCruiseServerSnapshot();
			ClassicAssert.AreEqual(snapshot, result);
		}
	}
}
