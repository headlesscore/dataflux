using System;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.CCTrayLib.Presentation;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Presentation
{
	public class MainFormControllerTest
	{
		private int eventCount = 0;
		private Mock<IProjectMonitor> mockProjectMonitor;
		private IProjectMonitor projectMonitor;
		private Mock<ICCTrayMultiConfiguration> mockConfiguration;
		private ICCTrayMultiConfiguration configuration;
		private MainFormController controller;

		//[SetUp]
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

		[Fact]
		public void WhenTheSelectedProjectChangesTheIsProjectSelectedPropertyChangesAndEventFires()
		{
			Assert.False(controller.IsProjectSelected);
            Assert.False(controller.IsProjectSelected);
            controller.IsProjectSelectedChanged += new EventHandler(Controller_IsProjectSelectedChanged);
			controller.SelectedProject = projectMonitor;

			Assert.True(controller.IsProjectSelected);
			Assert.Equal(1, eventCount);

		}

		private void Controller_IsProjectSelectedChanged(object sender, EventArgs e)
		{
			eventCount++;
		}

		[Fact]
		public void ForceBuildInvokesForceBuildOnTheSelectedProject()
		{
			mockProjectMonitor.SetupGet(_monitor => _monitor.ProjectState).Returns(ProjectState.Success).Verifiable();
            mockProjectMonitor.Setup(_monitor => _monitor.ListBuildParameters()).Returns(() => null).Verifiable();
			controller.SelectedProject = projectMonitor;

			mockProjectMonitor.Setup(_monitor => _monitor.ForceBuild(null, "John")).Verifiable();
			controller.ForceBuild();

			mockProjectMonitor.Verify();
		}

		[Fact]
		public void ForceBuildDoesNothingIfProjectIsNotConnected()
		{
			mockProjectMonitor.SetupGet(_monitor => _monitor.ProjectState).Returns(ProjectState.NotConnected).Verifiable();
            mockProjectMonitor.Setup(_monitor => _monitor.ListBuildParameters()).Returns(() => null).Verifiable();
			controller.SelectedProject = projectMonitor;

			controller.ForceBuild();

			mockProjectMonitor.Verify();
			mockProjectMonitor.VerifyNoOtherCalls();
		}

		[Fact]
		public void ForceBuildDoesNothingIfNoProjectSelected()
		{
			Assert.Null(controller.SelectedProject);
			controller.ForceBuild();
			mockProjectMonitor.Verify();
		}

		[Fact]
		public void CanFixBuildIfBuildIsBroken()
		{
			mockProjectMonitor.SetupGet(_monitor => _monitor.ProjectState).Returns(ProjectState.Broken).Verifiable();
			controller.SelectedProject = projectMonitor;
			Assert.True(controller.CanFixBuild());
			mockProjectMonitor.Verify();
		}

		[Fact]
		public void CanFixBuildIfBuildIsBrokenAndBuilding()
		{
			mockProjectMonitor.SetupGet(_monitor => _monitor.ProjectState).Returns(ProjectState.BrokenAndBuilding).Verifiable();
			controller.SelectedProject = projectMonitor;
			Assert.True(controller.CanFixBuild());
			mockProjectMonitor.Verify();
		}

		[Fact]
		public void CannotFixBuildIfBuildIsWorking()
		{
			mockProjectMonitor.SetupGet(_monitor => _monitor.ProjectState).Returns(ProjectState.Success).Verifiable();
			controller.SelectedProject = projectMonitor;
			Assert.False(controller.CanFixBuild());
			mockProjectMonitor.Verify();
		}

		[Fact]
		public void CannotFixBuildIfNoProjectIsSelected()
		{
			Assert.Null(controller.SelectedProject);
			Assert.False(controller.CanFixBuild());
			mockProjectMonitor.Verify();
		}

		[Fact]
		public void VolunteeringToFixBuildShouldInvokeServer()
		{
			controller.SelectedProject = projectMonitor;
			mockProjectMonitor.Setup(_monitor => _monitor.FixBuild("John")).Verifiable();
			controller.VolunteerToFixBuild();
			mockProjectMonitor.Verify();
		}

		[Fact]
		public void VolunteeringToFixBuildShouldDoNothingIfNoProjectIsSelected()
		{
			Assert.Null(controller.SelectedProject);
			controller.VolunteerToFixBuild();
			mockProjectMonitor.Verify();
		}
		
		[Fact]
		public void CanCancelPendingIfBuildIsPending()
		{
			mockProjectMonitor.SetupGet(_monitor => _monitor.IsPending).Returns(true).Verifiable();
			controller.SelectedProject = projectMonitor;
			Assert.True(controller.CanCancelPending());
			mockProjectMonitor.Verify();
		}

		[Fact]
		public void CannotCancelPendingIfBuildIsNotPending()
		{
			mockProjectMonitor.SetupGet(_monitor => _monitor.IsPending).Returns(false).Verifiable();
			controller.SelectedProject = projectMonitor;
			Assert.False(controller.CanCancelPending());
			mockProjectMonitor.Verify();
		}

		[Fact]
		public void CannotCancelPendingIfNoProjectIsSelected()
		{
			Assert.Null(controller.SelectedProject);
			Assert.False(controller.CanCancelPending());
			mockProjectMonitor.Verify();
		}

		[Fact]
		public void CancelPendingShouldInvokeServer()
		{
			controller.SelectedProject = projectMonitor;
			mockProjectMonitor.Setup(_monitor => _monitor.CancelPending()).Verifiable();
			controller.CancelPending();
			mockProjectMonitor.Verify();
		}

		[Fact]
		public void CancelPendingShouldDoNothingIfNoProjectIsSelected()
		{
			Assert.Null(controller.SelectedProject);
			controller.CancelPending();
			mockProjectMonitor.Verify();
		}
	}
}
