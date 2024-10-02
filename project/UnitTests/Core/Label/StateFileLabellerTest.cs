using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Label;
using ThoughtWorks.CruiseControl.Core.State;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Label
{
	[TestFixture]
	public class StateFileLabellerTest : IntegrationFixture
	{
		private StateFileLabeller labeller;
		private Mock<IStateManager> mockStateManager;

		[SetUp]
		public void SetUp()
		{
			mockStateManager = new Mock<IStateManager>();
			labeller = new StateFileLabeller((IStateManager) mockStateManager.Object);
		}

		[Test]
		public void ShouldLoadIntegrationResultFromStateManagerAndReturnLastSuccessfulBuildLabel()
		{
			mockStateManager.Setup(_manager => _manager.LoadState("Project1")).Returns(SuccessfulResult("success")).Verifiable();
			labeller.Project = "Project1";

			ClassicAssert.AreEqual("success", labeller.Generate(new IntegrationResult()));
            ClassicAssert.AreEqual("success", labeller.Generate(new IntegrationResult()));
            mockStateManager.Verify();
		}
	}
}
