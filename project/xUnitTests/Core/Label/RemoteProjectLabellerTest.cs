using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Label;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Label
{
	
	public class RemoteProjectLabellerTest
	{
		private Mock<ICruiseManager> mockCruiseManager;
		private Mock<IRemotingService> mockRemotingService;
		private RemoteProjectLabeller labeller;

		// [SetUp]
		protected void SetUp()
		{
			mockCruiseManager = new Mock<ICruiseManager>();
			mockCruiseManager.Setup(_manager => _manager.GetProjectStatus()).Returns(new ProjectStatus[1] {NewProjectStatus("foo", "1")}).Verifiable();

			mockRemotingService = new Mock<IRemotingService>();
			mockRemotingService.Setup(service => service.Connect(typeof(ICruiseManager), RemoteCruiseServer.DefaultManagerUri)).Returns(mockCruiseManager.Object).Verifiable();

			labeller = new RemoteProjectLabeller((IRemotingService) mockRemotingService.Object);
		}

		[Fact]
		public void ShouldConnectToRemoteServerAndRetrieveLabel()
		{
			labeller.ProjectName = "foo";
			Assert.Equal("1", labeller.Generate(IntegrationResultMother.CreateSuccessful()));
            Assert.Equal("1", labeller.Generate(IntegrationResultMother.CreateSuccessful()));
        }

		[Fact]
		public void ShouldThrowExceptionIfProjectNameIsInvalid()
		{
			labeller.ProjectName = "invalid";
            Assert.Throws<NoSuchProjectException>(delegate { labeller.Generate(IntegrationResultMother.CreateSuccessful()); });
		}

		private ProjectStatus NewProjectStatus(string projectName, string label)
		{
			return ProjectStatusFixture.New(projectName, label);
		}
	}
}
