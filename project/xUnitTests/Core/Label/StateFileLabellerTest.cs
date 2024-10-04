using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Label;
using ThoughtWorks.CruiseControl.Core.State;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Label
{
	
	public class StateFileLabellerTest : IntegrationFixture
	{
		private StateFileLabeller labeller;
		private Mock<IStateManager> mockStateManager;

		// [SetUp]
		public void SetUp()
		{
			mockStateManager = new Mock<IStateManager>();
			labeller = new StateFileLabeller((IStateManager) mockStateManager.Object);
		}

		[Fact]
		public void ShouldLoadIntegrationResultFromStateManagerAndReturnLastSuccessfulBuildLabel()
		{
			mockStateManager.Setup(_manager => _manager.LoadState("Project1")).Returns(SuccessfulResult("success")).Verifiable();
			labeller.Project = "Project1";

			Assert.Equal("success", labeller.Generate(new IntegrationResult()));
            Assert.Equal("success", labeller.Generate(new IntegrationResult()));
            mockStateManager.Verify();
		}
	}
}
