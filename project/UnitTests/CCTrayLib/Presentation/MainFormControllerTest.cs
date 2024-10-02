using System;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.CCTrayLib.Presentation;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Presentation
{
	[TestFixture]
	public class MainFormControllerTest
	{
		private int eventCount = 0;
		private Mock<IProjectMonitor> mockProjectMonitor;
		private IProjectMonitor projectMonitor;
		private Mock<ICCTrayMultiConfiguration> mockConfiguration;
		private ICCTrayMultiConfiguration configuration;
		private MainFormController controller;

		[SetUp]
		public void SetUp()
		{
			mockProjectMonitor = new Mock<IProjectMonitor>(MockBehavior.Strict);
			projectMonitor = (IProjectMonitor) mockProjectMonitor.Object;

			mockConfiguration = new Mock<ICCTrayMultiConfiguration>();
			configuration = (ICCTrayMultiConfiguration) mockConfiguration.Object;
            
            ISingleServerMonitor[] serverMonitors = new ISingleServerMonitor[0];
            mockConfiguration.Setup(_configuration => _configuration.GetServerMonitors()).Returns(serverMonitors);
            mockConfiguration.Setup(_configuration => _configuration.GetProjectStatusMonitors(It.IsAny<ISingleServerMonitor[]>())).Returns(new IProjectMonitor[0]);
			mockConfiguration.SetupGet(_configuration => _configuration.Icons).Returns(new Icons());
            mockConfiguration.SetupGet(_configuration => _configuration.FixUserName).Returns("John");
            GrowlConfiguration growlConfig = new GrowlConfiguration();
            mockConfiguration.SetupGet(_configuration => _configuration.Growl).Returns(growlConfig);

			eventCount = 0;

			controller = new MainFormController(configuration, null, null);
		}

		[Test]
		public void WhenTheSelectedProjectChangesTheIsProjectSelectedPropertyChangesAndEventFires()
		{
			ClassicAssert.IsFalse(controller.IsProjectSelected);
            ClassicAssert.IsFalse(controller.IsProjectSelected);
            controller.IsProjectSelectedChanged += new EventHandler(Controller_IsProjectSelectedChanged);
			controller.SelectedProject = projectMonitor;

			ClassicAssert.IsTrue(controller.IsProjectSelected);
			ClassicAssert.AreEqual(1, eventCount);

		}

		private void Controller_IsProjectSelectedChanged(object sender, EventArgs e)
		{
			eventCount++;
		}

		[Test]
		public void ForceBuildInvokesForceBuildOnTheSelectedProject()
		{
			mockProjectMonitor.SetupGet(_monitor => _monitor.ProjectState).Returns(ProjectState.Success).Verifiable();
            mockProjectMonitor.Setup(_monitor => _monitor.ListBuildParameters()).Returns(() => null).Verifiable();
			controller.SelectedProject = projectMonitor;

			mockProjectMonitor.Setup(_monitor => _monitor.ForceBuild(null, "John")).Verifiable();
			controller.ForceBuild();

			mockProjectMonitor.Verify();
		}

		[Test]
		public void ForceBuildDoesNothingIfProjectIsNotConnected()
		{
			mockProjectMonitor.SetupGet(_monitor => _monitor.ProjectState).Returns(ProjectState.NotConnected).Verifiable();
            mockProjectMonitor.Setup(_monitor => _monitor.ListBuildParameters()).Returns(() => null).Verifiable();
			controller.SelectedProject = projectMonitor;

			controller.ForceBuild();

			mockProjectMonitor.Verify();
			mockProjectMonitor.VerifyNoOtherCalls();
		}

		[Test]
		public void ForceBuildDoesNothingIfNoProjectSelected()
		{
			ClassicAssert.IsNull(controller.SelectedProject);
			controller.ForceBuild();
			mockProjectMonitor.Verify();
		}

		[Test]
		public void CanFixBuildIfBuildIsBroken()
		{
			mockProjectMonitor.SetupGet(_monitor => _monitor.ProjectState).Returns(ProjectState.Broken).Verifiable();
			controller.SelectedProject = projectMonitor;
			ClassicAssert.IsTrue(controller.CanFixBuild());
			mockProjectMonitor.Verify();
		}

		[Test]
		public void CanFixBuildIfBuildIsBrokenAndBuilding()
		{
			mockProjectMonitor.SetupGet(_monitor => _monitor.ProjectState).Returns(ProjectState.BrokenAndBuilding).Verifiable();
			controller.SelectedProject = projectMonitor;
			ClassicAssert.IsTrue(controller.CanFixBuild());
			mockProjectMonitor.Verify();
		}

		[Test]
		public void CannotFixBuildIfBuildIsWorking()
		{
			mockProjectMonitor.SetupGet(_monitor => _monitor.ProjectState).Returns(ProjectState.Success).Verifiable();
			controller.SelectedProject = projectMonitor;
			ClassicAssert.IsFalse(controller.CanFixBuild());
			mockProjectMonitor.Verify();
		}

		[Test]
		public void CannotFixBuildIfNoProjectIsSelected()
		{
			ClassicAssert.IsNull(controller.SelectedProject);
			ClassicAssert.IsFalse(controller.CanFixBuild());
			mockProjectMonitor.Verify();
		}

		[Test]
		public void VolunteeringToFixBuildShouldInvokeServer()
		{
			controller.SelectedProject = projectMonitor;
			mockProjectMonitor.Setup(_monitor => _monitor.FixBuild("John")).Verifiable();
			controller.VolunteerToFixBuild();
			mockProjectMonitor.Verify();
		}

		[Test]
		public void VolunteeringToFixBuildShouldDoNothingIfNoProjectIsSelected()
		{
			ClassicAssert.IsNull(controller.SelectedProject);
			controller.VolunteerToFixBuild();
			mockProjectMonitor.Verify();
		}
		
		[Test]
		public void CanCancelPendingIfBuildIsPending()
		{
			mockProjectMonitor.SetupGet(_monitor => _monitor.IsPending).Returns(true).Verifiable();
			controller.SelectedProject = projectMonitor;
			ClassicAssert.IsTrue(controller.CanCancelPending());
			mockProjectMonitor.Verify();
		}

		[Test]
		public void CannotCancelPendingIfBuildIsNotPending()
		{
			mockProjectMonitor.SetupGet(_monitor => _monitor.IsPending).Returns(false).Verifiable();
			controller.SelectedProject = projectMonitor;
			ClassicAssert.IsFalse(controller.CanCancelPending());
			mockProjectMonitor.Verify();
		}

		[Test]
		public void CannotCancelPendingIfNoProjectIsSelected()
		{
			ClassicAssert.IsNull(controller.SelectedProject);
			ClassicAssert.IsFalse(controller.CanCancelPending());
			mockProjectMonitor.Verify();
		}

		[Test]
		public void CancelPendingShouldInvokeServer()
		{
			controller.SelectedProject = projectMonitor;
			mockProjectMonitor.Setup(_monitor => _monitor.CancelPending()).Verifiable();
			controller.CancelPending();
			mockProjectMonitor.Verify();
		}

		[Test]
		public void CancelPendingShouldDoNothingIfNoProjectIsSelected()
		{
			ClassicAssert.IsNull(controller.SelectedProject);
			controller.CancelPending();
			mockProjectMonitor.Verify();
		}
	}
}
