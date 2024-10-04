using System;
using System.Net;
using Moq;
using Xunit;
using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Monitoring
{
	public class HttpCruiseServerManagerTest
	{
		private const string SERVER_URL = @"http://localhost/ccnet/XmlServerReport.aspx";

        private MockRepository mocks = new MockRepository(MockBehavior.Default);
        private CruiseServerClientBase serverClient;
        private BuildServer buildServer;
        private HttpCruiseServerManager manager;

		//// [SetUp]
		public void SetUp()
		{
            serverClient = mocks.Create<CruiseServerClientBase>().Object;

			buildServer = new BuildServer(SERVER_URL);
            manager = new HttpCruiseServerManager(serverClient, buildServer);
		}

		[Fact]
		public void InitialisingReturnsCorrectServerProperties()
		{
			Assert.Equal(SERVER_URL, manager.Configuration.Url);
            //Assert.Equal(SERVER_URL, manager.Configuration.Url);
            Assert.Equal(@"localhost", manager.DisplayName);
			Assert.Equal(BuildServerTransport.HTTP, manager.Configuration.Transport);
		}

		[Fact]
		public void RetrieveSnapshotFromManager()
		{
			CruiseServerSnapshot snapshot = new CruiseServerSnapshot();

            Mock.Get(serverClient).Setup(_serverClient => _serverClient.GetCruiseServerSnapshot()).Returns(snapshot);
			CruiseServerSnapshot actual = manager.GetCruiseServerSnapshot();
			
			Assert.Same(snapshot, actual);
		}

		[Fact]
		public void CanHandleTimeouts(){
            Mock.Get(serverClient).Setup(_serverClient => _serverClient.GetCruiseServerSnapshot()).Throws(new WebException("The operation has timed out"));

			CruiseServerSnapshot actual = manager.GetCruiseServerSnapshot();
			
			Assert.NotNull(actual);
			// mainly want to make sure that the exception is caught, and return is not null. 
		}

        [Fact]
        public void CancelPendingRequestThrowsAnNotImplementedException()
        {
            Assert.Equal("Cancel pending not currently supported on servers monitored via HTTP", Assert.Throws<NotImplementedException>(delegate { manager.CancelPendingRequest("myproject"); }).Message);                        
        }
	}
}
