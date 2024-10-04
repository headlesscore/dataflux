using System;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.State;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Core
{
	
	public class ProjectExceptionHandlingTest
	{
		[Fact]
		public void ShouldHandleIncrementingLabelAfterInitialBuildFailsWithException()
		{
			var mockSourceControl = new Mock<ISourceControl>();
			MockSequence sequence = new MockSequence();
			mockSourceControl.InSequence(sequence).Setup(sourceControl => sourceControl.GetModifications(It.IsAny<IIntegrationResult>(), It.IsAny<IIntegrationResult>())).Throws(new Exception("doh!")).Verifiable();
			mockSourceControl.InSequence(sequence).Setup(sourceControl => sourceControl.GetModifications(It.IsAny<IIntegrationResult>(), It.IsAny<IIntegrationResult>())).Returns(new Modification[] {new Modification()}).Verifiable();

			Project project = new Project();
			project.Name = "test";
			project.SourceControl = (ISourceControl) mockSourceControl.Object;
			project.StateManager = new StateManagerStub();
			try { project.Integrate(new IntegrationRequest(BuildCondition.ForceBuild, "test", null));}
			catch (Exception) { }

			project.Integrate(new IntegrationRequest(BuildCondition.ForceBuild, "test", null));
			Assert.Equal(IntegrationStatus.Success, project.CurrentResult.Status);
            Assert.Equal(IntegrationStatus.Success, project.CurrentResult.Status);
            Assert.Equal("1", project.CurrentResult.Label);
		}

		[Fact]
		public void ShouldNotResetLabelIfGetModificationsThrowsException()
		{
			var mockSourceControl = new Mock<ISourceControl>();
			MockSequence sequence = new MockSequence();
			mockSourceControl.InSequence(sequence).Setup(sourceControl => sourceControl.GetModifications(It.IsAny<IIntegrationResult>(), It.IsAny<IIntegrationResult>())).Throws(new Exception("doh!")).Verifiable();
			mockSourceControl.InSequence(sequence).Setup(sourceControl => sourceControl.GetModifications(It.IsAny<IIntegrationResult>(), It.IsAny<IIntegrationResult>())).Returns(new Modification[] { new Modification() }).Verifiable();

			StateManagerStub stateManagerStub = new StateManagerStub();
			stateManagerStub.SaveState(IntegrationResultMother.CreateSuccessful("10"));
			
			Project project = new Project();
			project.Name = "test";
			project.SourceControl = (ISourceControl) mockSourceControl.Object;
			project.StateManager = stateManagerStub;
			try { project.Integrate(new IntegrationRequest(BuildCondition.ForceBuild, "test", null));}
			catch (Exception) { }

            project.Integrate(new IntegrationRequest(BuildCondition.ForceBuild, "test", null));
			Assert.Equal(IntegrationStatus.Success, project.CurrentResult.Status);
			Assert.Equal("11", project.CurrentResult.Label);			
		}
	}

	internal class StateManagerStub : IStateManager
	{
        private IIntegrationResult savedResult = IntegrationResult.CreateInitialIntegrationResult("test", @"c:\temp", @"c:\temp");

		public IIntegrationResult LoadState(string project)
		{
			return savedResult;
		}

		public void SaveState(IIntegrationResult result)
		{
			savedResult = result;
		}

		public bool HasPreviousState(string project)
		{
			return true;
		}
	}
}
