using System.Collections.Generic;
using Exortech.NetReflector;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core.Publishers;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Publishers
{
	
	public class ForceBuildPublisherTest
	{
        private MockRepository mocks;

        // [SetUp]
        public void Setup()
        {
            mocks = new MockRepository(MockBehavior.Default);
        }

		[Fact]
		public void PopulateFromConfigurationXml()
		{
			string xml = @"<forcebuild project=""proj"" serverUri=""http://localhost"" integrationStatus=""Failure"" />";
			ForceBuildPublisher publisher = NetReflector.Read(xml) as ForceBuildPublisher;
			Assert.Equal("proj", publisher.Project);
            Assert.Equal("proj", publisher.Project);
            Assert.Equal("http://localhost", publisher.ServerUri);
			Assert.Equal(IntegrationStatus.Failure, publisher.IntegrationStatus);
		}

		[Fact]
		public void PopulateFromMinimalXml()
		{
			string xml = @"<forcebuild project=""proj"" />";
			ForceBuildPublisher publisher = NetReflector.Read(xml) as ForceBuildPublisher;
			Assert.Equal("proj", publisher.Project);
			Assert.Equal("tcp://localhost:21234/CruiseManager.rem", publisher.ServerUri);
			Assert.Equal(IntegrationStatus.Success, publisher.IntegrationStatus);
		}

		[Fact]
		public void ShouldReqestForceBuildOnRemoteCruiseServer()
		{
            var factory = mocks.Create<ICruiseServerClientFactory>(MockBehavior.Strict).Object;
            var client = mocks.Create<CruiseServerClientBase>(MockBehavior.Strict).Object;
            Mock.Get(factory).Setup(_factory => _factory.GenerateClient("tcp://localhost:21234/CruiseManager.rem"))
                .Returns(client);
            Mock.Get(client).Setup(_client => _client.ForceBuild("project", It.IsNotNull<List<NameValuePair>>()))
                .Verifiable();
            Mock.Get(client).SetupSet(_client => _client.SessionToken = It.IsAny<string>()).Verifiable();

			ForceBuildPublisher publisher = new ForceBuildPublisher(factory);
			publisher.Project = "project";
			publisher.ServerUri = "tcp://localhost:21234/CruiseManager.rem";
			publisher.Run(IntegrationResultMother.CreateSuccessful());

            mocks.VerifyAll();
		}

		[Fact]
		public void ShouldOnlyForceBuildIfIntegrationStatusMatches()
		{
            var factory = mocks.Create<ICruiseServerClientFactory>(MockBehavior.Strict).Object;

			ForceBuildPublisher publisher = new ForceBuildPublisher(factory);
			publisher.IntegrationStatus = IntegrationStatus.Exception;
			publisher.Run(IntegrationResultMother.CreateFailed());

            mocks.VerifyAll();
        }
	}
}
