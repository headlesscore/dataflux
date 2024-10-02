using System;
using System.Net;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Monitoring
{
	[TestFixture]
	public class HttpCruiseServerManagerTest
	{
		private const string SERVER_URL = @"http://localhost/ccnet/XmlServerReport.aspx";

        private MockRepository mocks = new MockRepository(MockBehavior.Default);
        private CruiseServerClientBase serverClient;
        private BuildServer buildServer;
        private HttpCruiseServerManager manager;

		[SetUp]
		public void SetUp()
		{
            serverClient = mocks.Create<CruiseServerClientBase>().Object;

			buildServer = new BuildServer(SERVER_URL);
            manager = new HttpCruiseServerManager(serverClient, buildServer);
		}

		[Test]
		public void InitialisingReturnsCorrectServerProperties()
		{
			ClassicAssert.AreEqual(SERVER_URL, manager.Configuration.Url);
            //ClassicAssert.AreEqual(SERVER_URL, manager.Configuration.Url);
            ClassicAssert.AreEqual(@"localhost", manager.DisplayName);
			ClassicAssert.AreEqual(BuildServerTransport.HTTP, manager.Configuration.Transport);
		}

		[Test]
		public void RetrieveSnapshotFromManager()
		{
			CruiseServerSnapshot snapshot = new CruiseServerSnapshot();

            Mock.Get(serverClient).Setup(_serverClient => _serverClient.GetCruiseServerSnapshot()).Returns(snapshot);
			CruiseServerSnapshot actual = manager.GetCruiseServerSnapshot();
			
			ClassicAssert.AreSame(snapshot, actual);
		}

		[Test]
		public void CanHandleTimeouts(){
            Mock.Get(serverClient).Setup(_serverClient => _serverClient.GetCruiseServerSnapshot()).Throws(new WebException("The operation has timed out"));

			CruiseServerSnapshot actual = manager.GetCruiseServerSnapshot();
			
			ClassicAssert.IsNotNull(actual);
			// mainly want to make sure that the exception is caught, and return is not null. 
		}

        [Test]
        public void CancelPendingRequestThrowsAnNotImplementedException()
        {
            ClassicAssert.That(delegate { manager.CancelPendingRequest("myproject"); },
                        Throws.TypeOf<NotImplementedException>().With.Message.EqualTo("Cancel pending not currently supported on servers monitored via HTTP"));
        }
	}
}
