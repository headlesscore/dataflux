using System;
using System.Collections.Generic;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.CCTrayLib;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.Remote;
//using ThoughtWorks.CruiseControl.UnitTests.Core;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Monitoring
{
#if false
    public class ProjectMonitorTest : IDisposable
	{
		private StubCurrentTimeProvider dateTimeProvider;
        private Mock<ISingleServerMonitor> mockServerMonitor;
        private Mock<ICruiseProjectManager> mockProjectManager;
		private ProjectMonitor monitor;
		private int pollCount;
		private int buildOccurredCount;
		private MonitorBuildOccurredEventArgs lastBuildOccurredArgs;
		private Message actualMessage;

	    private const string PROJECT_NAME = "Project1";

		//[SetUp]
		public void SetUp()
		{
			buildOccurredCount = pollCount = 0;
            mockServerMonitor = new Mock<ISingleServerMonitor>(MockBehavior.Strict);
            mockProjectManager = new Mock<ICruiseProjectManager>(MockBehavior.Strict);
			dateTimeProvider = new StubCurrentTimeProvider();
            monitor = new ProjectMonitor(null, (ICruiseProjectManager)mockProjectManager.Object, (ISingleServerMonitor)mockServerMonitor.Object, dateTimeProvider);
			monitor.Polled += new MonitorPolledEventHandler(Monitor_Polled);
			monitor.BuildOccurred += new MonitorBuildOccurredEventHandler(Monitor_BuildOccurred);
		}

		public void Dispose()
		{
			mockProjectManager.Verify();
			actualMessage = null;
		}

		[Fact]
		public void WhenPollIsCalledRetrievesANewCopyOfTheProjectStatus()
		{
			ProjectStatus status = new ProjectStatus();
            mockProjectManager.SetupGet(_manager => _manager.ProjectName).Returns(PROJECT_NAME).Verifiable();
            mockServerMonitor.Setup(_monitor => _monitor.GetProjectStatus(PROJECT_NAME)).Returns(status).Verifiable();

			monitor.Poll();

			// deliberately called twice: should not go back to server on 2nd
			// call
			Assert.Same(status, monitor.ProjectStatus);
            //Assert.Same(status, monitor.ProjectStatus);
            Assert.Same(status, monitor.ProjectStatus);
		}

		[Fact]
		public void ThePollEventIsFiredWhenPollIsInvoked()
		{
			Assert.Equal(0, pollCount);

			ProjectStatus status = new ProjectStatus();
            mockProjectManager.SetupGet(_manager => _manager.ProjectName).Returns(PROJECT_NAME).Verifiable();
            mockServerMonitor.Setup(_monitor => _monitor.GetProjectStatus(PROJECT_NAME)).Returns(status).Verifiable();
            monitor.Poll();
			Assert.Equal(1, pollCount);

            mockProjectManager.SetupGet(_manager => _manager.ProjectName).Returns(PROJECT_NAME).Verifiable();
            mockServerMonitor.Setup(_monitor => _monitor.GetProjectStatus(PROJECT_NAME)).Returns(status).Verifiable();
            monitor.Poll();
			Assert.Equal(2, pollCount);
		}

		[Fact]
		public void WhenPollingEncountersAnExceptionThePolledEventIsStillFired()
		{
			Assert.Equal(0, pollCount);

            mockProjectManager.SetupGet(_manager => _manager.ProjectName).Throws(new Exception("should be caught")).Verifiable();
			monitor.Poll();
			Assert.Equal(1, pollCount);
		}

		[Fact]
		public void IfTheLastBuildDateHasChangedABuildOccuredEventIsFired()
		{
			Assert.Equal(0, buildOccurredCount);

		    ProjectStatus status = CreateProjectStatus(IntegrationStatus.Success, new DateTime(2004, 1, 1));
            mockProjectManager.SetupGet(_manager => _manager.ProjectName).Returns(PROJECT_NAME).Verifiable();
            mockServerMonitor.Setup(_monitor => _monitor.GetProjectStatus(PROJECT_NAME)).Returns(status).Verifiable();
			monitor.Poll();

			Assert.Equal(0, buildOccurredCount);

            status = CreateProjectStatus(IntegrationStatus.Success, new DateTime(2004, 1, 1));
            mockProjectManager.SetupGet(_manager => _manager.ProjectName).Returns(PROJECT_NAME).Verifiable();
            mockServerMonitor.Setup(_monitor => _monitor.GetProjectStatus(PROJECT_NAME)).Returns(status).Verifiable();
            monitor.Poll();

			Assert.Equal(0, buildOccurredCount);

            status = CreateProjectStatus(IntegrationStatus.Success, new DateTime(2004, 1, 2));
            mockProjectManager.SetupGet(_manager => _manager.ProjectName).Returns(PROJECT_NAME).Verifiable();
            mockServerMonitor.Setup(_monitor => _monitor.GetProjectStatus(PROJECT_NAME)).Returns(status).Verifiable();
            monitor.Poll();

			Assert.Equal(1, buildOccurredCount);

            status = CreateProjectStatus(IntegrationStatus.Success, new DateTime(2004, 1, 3));
            mockProjectManager.SetupGet(_manager => _manager.ProjectName).Returns(PROJECT_NAME).Verifiable();
            mockServerMonitor.Setup(_monitor => _monitor.GetProjectStatus(PROJECT_NAME)).Returns(status).Verifiable();
            monitor.Poll();

			Assert.Equal(2, buildOccurredCount);
		}

		[Fact]
		public void ShouldCorrectlyReportEstimatedTimeWhenANewBuildStartsDuringThePollInterval()
		{
			ProjectStatus firstBuildStatus =
				ProjectStatusFixture.New(IntegrationStatus.Success, ProjectActivity.Building, new DateTime(2007, 1, 1, 0, 0, 0));
            mockProjectManager.SetupGet(_manager => _manager.ProjectName).Returns(PROJECT_NAME).Verifiable();
            mockServerMonitor.Setup(_monitor => _monitor.GetProjectStatus(PROJECT_NAME)).Returns(firstBuildStatus).Verifiable();
            dateTimeProvider.SetNow(new DateTime(2007, 1, 1, 1, 0, 0));
			monitor.Poll();

			ProjectStatus secondBuildStatus =
				ProjectStatusFixture.New(IntegrationStatus.Success, ProjectActivity.Building, new DateTime(2007, 1, 1, 2, 0, 0));
            mockProjectManager.SetupGet(_manager => _manager.ProjectName).Returns(PROJECT_NAME).Verifiable();
            mockServerMonitor.Setup(_monitor => _monitor.GetProjectStatus(PROJECT_NAME)).Returns(secondBuildStatus).Verifiable();
            dateTimeProvider.SetNow(new DateTime(2007, 1, 1, 3, 0, 0));
			monitor.Poll();

			ProjectStatus thirdBuildStatus =
				ProjectStatusFixture.New(IntegrationStatus.Success, ProjectActivity.Building, new DateTime(2007, 1, 1, 4, 0, 0));
            mockProjectManager.SetupGet(_manager => _manager.ProjectName).Returns(PROJECT_NAME).Verifiable();
            mockServerMonitor.Setup(_monitor => _monitor.GetProjectStatus(PROJECT_NAME)).Returns(thirdBuildStatus).Verifiable();
            dateTimeProvider.SetNow(new DateTime(2007, 1, 1, 5, 0, 0));
			monitor.Poll();

			Assert.Equal(new TimeSpan(2, 0, 0), monitor.EstimatedTimeRemainingOnCurrentBuild);
		}

		[Fact]
		public void NotifiesCorrectlyForStillSuccessfulBuild()
		{
			AssertTransition(IntegrationStatus.Success, IntegrationStatus.Success, BuildTransition.StillSuccessful);
		}

		[Fact]
		public void NotifiesCorrectlyForBrokenBuild()
		{
			AssertTransition(IntegrationStatus.Success, IntegrationStatus.Failure, BuildTransition.Broken);
		}

		[Fact]
		public void NotifiesCorrectlyForStillFailingBuild()
		{
			AssertTransition(IntegrationStatus.Failure, IntegrationStatus.Failure, BuildTransition.StillFailing);
		}

		[Fact]
		public void NotifiesCorrectlyForFixedBuild()
		{
			AssertTransition(IntegrationStatus.Failure, IntegrationStatus.Success, BuildTransition.Fixed);
		}

		private void AssertTransition(
			IntegrationStatus initialIntegrationStatus,
			IntegrationStatus nextBuildIntegrationStatus,
			BuildTransition expectedBuildTransition)
		{
			// initial connection
            ProjectStatus status = CreateProjectStatus(initialIntegrationStatus, new DateTime(2004, 1, 1));
            mockProjectManager.SetupGet(_manager => _manager.ProjectName).Returns(PROJECT_NAME).Verifiable();
            mockServerMonitor.Setup(_monitor => _monitor.GetProjectStatus(PROJECT_NAME)).Returns(status).Verifiable();
			monitor.Poll();

			// then the build
            status = CreateProjectStatus(nextBuildIntegrationStatus, new DateTime(2004, 1, 2));
            mockProjectManager.SetupGet(_manager => _manager.ProjectName).Returns(PROJECT_NAME).Verifiable();
            mockServerMonitor.Setup(_monitor => _monitor.GetProjectStatus(PROJECT_NAME)).Returns(status).Verifiable();
			monitor.Poll();

			Assert.Equal(1, buildOccurredCount);
			Assert.Equal(expectedBuildTransition, lastBuildOccurredArgs.BuildTransition);

			buildOccurredCount = 0;
		}

		private void Monitor_Polled(object sauce, MonitorPolledEventArgs args)
		{
			pollCount++;
		}

		private void Monitor_BuildOccurred(object sauce, MonitorBuildOccurredEventArgs e)
		{
			buildOccurredCount++;
			lastBuildOccurredArgs = e;
		}

		private ProjectStatus CreateProjectStatus(IntegrationStatus integrationStatus, DateTime lastBuildDate)
		{
			return ProjectStatusFixture.New(integrationStatus, lastBuildDate);
		}

		private ProjectStatus CreateProjectStatus(IntegrationStatus integrationStatus, ProjectActivity activity)
		{
			return ProjectStatusFixture.New(integrationStatus, activity);
		}

		[Fact]
		public void CorrectlyDeterminesProjectState()
		{
			Assert.Equal(ProjectState.NotConnected, monitor.ProjectState);

            mockProjectManager.SetupGet(_manager => _manager.ProjectName).Returns(PROJECT_NAME).Verifiable();
            mockServerMonitor.Setup(_monitor => _monitor.GetProjectStatus(PROJECT_NAME))
                .Returns(CreateProjectStatus(IntegrationStatus.Success, ProjectActivity.Sleeping)).Verifiable();
			monitor.Poll();
			Assert.Equal(ProjectState.Success, monitor.ProjectState);

            mockProjectManager.SetupGet(_manager => _manager.ProjectName).Returns(PROJECT_NAME).Verifiable();
            mockServerMonitor.Setup(_monitor => _monitor.GetProjectStatus(PROJECT_NAME))
                .Returns(CreateProjectStatus(IntegrationStatus.Exception, ProjectActivity.Sleeping)).Verifiable();
			monitor.Poll();
			Assert.Equal(ProjectState.Broken, monitor.ProjectState);

            mockProjectManager.SetupGet(_manager => _manager.ProjectName).Returns(PROJECT_NAME).Verifiable();
            mockServerMonitor.Setup(_monitor => _monitor.GetProjectStatus(PROJECT_NAME))
                .Returns(CreateProjectStatus(IntegrationStatus.Failure, ProjectActivity.Sleeping)).Verifiable();
			monitor.Poll();
			Assert.Equal(ProjectState.Broken, monitor.ProjectState);

            mockProjectManager.SetupGet(_manager => _manager.ProjectName).Returns(PROJECT_NAME).Verifiable();
            mockServerMonitor.Setup(_monitor => _monitor.GetProjectStatus(PROJECT_NAME))
                .Returns(CreateProjectStatus(IntegrationStatus.Unknown, ProjectActivity.Sleeping)).Verifiable();
			monitor.Poll();
			Assert.Equal(ProjectState.Broken, monitor.ProjectState);

            mockProjectManager.SetupGet(_manager => _manager.ProjectName).Returns(PROJECT_NAME).Verifiable();
            mockServerMonitor.Setup(_monitor => _monitor.GetProjectStatus(PROJECT_NAME))
                .Returns(CreateProjectStatus(IntegrationStatus.Success, ProjectActivity.Building)).Verifiable();
			monitor.Poll();
			Assert.Equal(ProjectState.Building, monitor.ProjectState);

            mockProjectManager.SetupGet(_manager => _manager.ProjectName).Returns(PROJECT_NAME).Verifiable();
            mockServerMonitor.Setup(_monitor => _monitor.GetProjectStatus(PROJECT_NAME))
                .Returns(CreateProjectStatus(IntegrationStatus.Success, ProjectActivity.Building)).Verifiable();
			monitor.Poll();
			Assert.Equal(ProjectState.Building, monitor.ProjectState);

            mockProjectManager.SetupGet(_manager => _manager.ProjectName).Returns(PROJECT_NAME).Verifiable();
            mockServerMonitor.Setup(_monitor => _monitor.GetProjectStatus(PROJECT_NAME))
                .Returns(CreateProjectStatus(IntegrationStatus.Success, ProjectActivity.CheckingModifications)).Verifiable();
			monitor.Poll();
			Assert.Equal(ProjectState.Success, monitor.ProjectState);

            mockProjectManager.SetupGet(_manager => _manager.ProjectName).Returns(PROJECT_NAME).Verifiable();
            mockServerMonitor.Setup(_monitor => _monitor.GetProjectStatus(PROJECT_NAME))
                .Returns(() => null).Verifiable();
			monitor.Poll();
			Assert.Equal(ProjectState.NotConnected, monitor.ProjectState);

            mockProjectManager.SetupGet(_manager => _manager.ProjectName).Returns(PROJECT_NAME).Verifiable();
            mockServerMonitor.Setup(_monitor => _monitor.GetProjectStatus(PROJECT_NAME))
                .Returns(CreateProjectStatus(IntegrationStatus.Failure, ProjectActivity.Building)).Verifiable();
			monitor.Poll();
			Assert.Equal(ProjectState.BrokenAndBuilding, monitor.ProjectState);

            mockProjectManager.SetupGet(_manager => _manager.ProjectName).Returns(PROJECT_NAME).Verifiable();
            mockServerMonitor.Setup(_monitor => _monitor.GetProjectStatus(PROJECT_NAME))
                .Returns(CreateProjectStatus(IntegrationStatus.Exception, ProjectActivity.Building)).Verifiable();
			monitor.Poll();
			Assert.Equal(ProjectState.BrokenAndBuilding, monitor.ProjectState);
		}

		[Fact]
		public void DoNotTransitionProjectStateForNewInstanceOfSameProjectActivity()
		{
            mockProjectManager.SetupGet(_manager => _manager.ProjectName).Returns(PROJECT_NAME).Verifiable();
            mockServerMonitor.Setup(_monitor => _monitor.GetProjectStatus(PROJECT_NAME))
                .Returns(CreateProjectStatus(IntegrationStatus.Success, ProjectActivity.Building)).Verifiable();
			monitor.Poll();
			Assert.Equal(ProjectState.Building, monitor.ProjectState);
            mockProjectManager.SetupGet(_manager => _manager.ProjectName).Returns(PROJECT_NAME).Verifiable();
            mockServerMonitor.Setup(_monitor => _monitor.GetProjectStatus(PROJECT_NAME))
                .Returns(CreateProjectStatus(IntegrationStatus.Success, new ProjectActivity(ProjectActivity.Building.ToString()))).Verifiable();
			monitor.Poll();
			Assert.Equal(ProjectState.Building, monitor.ProjectState);
		}

		[Fact]
		public void ForceBuildIsForwardedOn()
		{
            mockServerMonitor.SetupGet(_monitor => _monitor.SessionToken).Returns(() => null).Verifiable();
            var parameters = new Dictionary<string, string>();
            mockProjectManager.Setup(_manager => _manager.ForceBuild(null, parameters, null)).Verifiable();
            monitor.ForceBuild(parameters, null);
		}

		[Fact]
		public void SummaryStatusStringReturnsASummaryStatusStringWhenTheStateNotSuccess()
		{
			ProjectStatus status = ProjectStatusFixture.New(IntegrationStatus.Failure, ProjectActivity.Sleeping);
            mockProjectManager.SetupGet(_manager => _manager.ProjectName).Returns(PROJECT_NAME).Verifiable();
            mockServerMonitor.Setup(_monitor => _monitor.GetProjectStatus(PROJECT_NAME)).Returns(status).Verifiable();

			monitor.Poll();

			Assert.Equal(PROJECT_NAME + ": Broken", monitor.SummaryStatusString);
		}

		[Fact]
		public void SummaryStatusStringReturnsEmptyStringWhenTheStateIsSuccess()
		{
			ProjectStatus status = ProjectStatusFixture.New(IntegrationStatus.Success, ProjectActivity.Sleeping);
            mockProjectManager.SetupGet(_manager => _manager.ProjectName).Returns(PROJECT_NAME).Verifiable();
            mockServerMonitor.Setup(_monitor => _monitor.GetProjectStatus(PROJECT_NAME)).Returns(status).Verifiable();

			monitor.Poll();

			Assert.Equal(string.Empty, monitor.SummaryStatusString);
		}

		[Fact]
		public void ExposesTheIntegrationStatusOfTheContainedProject()
		{
			AssertIntegrationStateReturned(IntegrationStatus.Failure);
			AssertIntegrationStateReturned(IntegrationStatus.Exception);
			AssertIntegrationStateReturned(IntegrationStatus.Success);
			AssertIntegrationStateReturned(IntegrationStatus.Unknown);
		}

		private void AssertIntegrationStateReturned(IntegrationStatus integrationStatus)
		{
			ProjectStatus status = ProjectStatusFixture.New(integrationStatus, ProjectActivity.CheckingModifications);
            mockProjectManager.SetupGet(_manager => _manager.ProjectName).Returns(PROJECT_NAME).Verifiable();
            mockServerMonitor.Setup(_monitor => _monitor.GetProjectStatus(PROJECT_NAME)).Returns(status).Verifiable();

			monitor.Poll();

			Assert.Equal(integrationStatus, monitor.IntegrationStatus);
		}

		[Fact]
		public void WhenNoConnectionHasBeenMadeToTheBuildServerTheIntegrationStatusIsUnknown()
		{
			Assert.Equal(IntegrationStatus.Unknown, monitor.IntegrationStatus);
		}

		[Fact]
		public void InvokeServerWhenVolunteeringToFixBuild()
		{
            mockServerMonitor.SetupGet(_monitor => _monitor.SessionToken).Returns(() => null).Verifiable();
            mockProjectManager.Setup(_manager => _manager.FixBuild(null, "John")).Verifiable();
			monitor.FixBuild("John");
			mockProjectManager.Verify();
		}

		[Fact]
		public void DisplayBalloonMessageWhenNewMessageIsReceived()
		{
            monitor.MessageReceived += new MessageEventHandler(OnMessageReceived);
			ProjectStatus initial = ProjectStatusFixture.New(IntegrationStatus.Success, ProjectActivity.Sleeping);
            mockProjectManager.SetupGet(_manager => _manager.ProjectName).Returns(PROJECT_NAME).Verifiable();
            mockServerMonitor.Setup(_monitor => _monitor.GetProjectStatus(PROJECT_NAME)).Returns(initial).Verifiable();

			monitor.Poll();
			Assert.Equal(actualMessage, null);

			Message expectedMessage = new Message("foo");
			ProjectStatus newStatus = ProjectStatusFixture.New(IntegrationStatus.Success, ProjectActivity.Sleeping);
			newStatus.Messages = new Message[] {expectedMessage};
            mockProjectManager.SetupGet(_manager => _manager.ProjectName).Returns(PROJECT_NAME).Verifiable();
            mockServerMonitor.Setup(_monitor => _monitor.GetProjectStatus(PROJECT_NAME)).Returns(newStatus).Verifiable();

			monitor.Poll();
			Assert.Equal(actualMessage, expectedMessage);
		}

		[Fact]
		public void InvokeServerWhenCancelPendingRequest()
		{
            mockServerMonitor.SetupGet(_monitor => _monitor.SessionToken).Returns(() => null).Verifiable();
            mockProjectManager.Setup(_manager => _manager.CancelPendingRequest(null)).Verifiable();
			monitor.CancelPending();
			mockProjectManager.Verify();
		}

		[Fact]
		public void IsNotPendingIfThereIsNoProjectStatus()
		{
			ClassicAssert.IsFalse(monitor.IsPending);
		}

		private void OnMessageReceived(string projectName, Message message)
		{
			actualMessage = message;
		}
	}
#endif
}
