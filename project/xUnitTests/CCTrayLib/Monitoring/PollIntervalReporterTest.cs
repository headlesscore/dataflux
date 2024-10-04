using System;
using Xunit;

using ThoughtWorks.CruiseControl.CCTrayLib;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.Remote;


namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Monitoring
{
	public class PollIntervalReporterTest
	{
		[Fact]
		public void BuildStartedIfLastBuildDateHasChangedAndStatusRemainedBuilding()
		{
#if false
            ProjectStatus oldProjectStatus =
				ProjectStatusFixture.New(IntegrationStatus.Success, ProjectActivity.Building, new DateTime(2007, 1, 1));
			ProjectStatus newProjectStatus =
				ProjectStatusFixture.New(IntegrationStatus.Success, ProjectActivity.Building, new DateTime(2007, 1, 2));

			bool result = new PollIntervalReporter(oldProjectStatus, newProjectStatus).HasNewBuildStarted;

			Assert.True(result);
#endif
            //Assert.True(result);
        }

		[Fact]
		public void BuildStartedIfStatusChangedToBuilding()
		{
#if false
            ProjectStatus oldProjectStatus = ProjectStatusFixture.New(IntegrationStatus.Success, ProjectActivity.Sleeping);
			ProjectStatus newProjectStatus = ProjectStatusFixture.New(IntegrationStatus.Success, ProjectActivity.Building);

			bool result = new PollIntervalReporter(oldProjectStatus, newProjectStatus).HasNewBuildStarted;

			Assert.True(result);
#endif
		}

		[Fact]
		public void NoBuildIfLastBuildDateIsSameAndStatusIsSame()
		{
			DateTime lastBuildDate = new DateTime(2007, 1, 1);
#if false
            ProjectStatus oldProjectStatus =
				ProjectStatusFixture.New(IntegrationStatus.Success, ProjectActivity.Building, lastBuildDate);
			ProjectStatus newProjectStatus =
				ProjectStatusFixture.New(IntegrationStatus.Success, ProjectActivity.Building, lastBuildDate);
			bool result = new PollIntervalReporter(oldProjectStatus, newProjectStatus).HasNewBuildStarted;

			Assert.False(result);

			oldProjectStatus = ProjectStatusFixture.New(IntegrationStatus.Success, ProjectActivity.Sleeping, lastBuildDate);
			newProjectStatus = ProjectStatusFixture.New(IntegrationStatus.Success, ProjectActivity.Sleeping, lastBuildDate);
			result = new PollIntervalReporter(oldProjectStatus, newProjectStatus).HasNewBuildStarted;
			Assert.False(result);
#endif
		}

		[Fact]
		public void MessagesUpdatedIfNewStatusHasMoreMessagesThanOld()
		{
#if false
            ProjectStatus oldProjectStatus = ProjectStatusFixture.New("test project");
			oldProjectStatus.Messages = new Message[] {new Message("message")};
			ProjectStatus newProjectStatus = ProjectStatusFixture.New("test project");
			newProjectStatus.Messages = new Message[] {new Message("message"), new Message("another message")};

			Assert.True(new PollIntervalReporter(oldProjectStatus, newProjectStatus).WasNewStatusMessagesReceived);
#endif
		}

		[Fact]
        public void AllStatusMessagesReturnsMustReturnThemAll()
		{
			Message latestMessage = new Message("latest message");
            Message firstMessage = new Message("message");

#if false
            ProjectStatus oldProjectStatus = ProjectStatusFixture.New("test project");
			oldProjectStatus.Messages = new Message[] {};
			ProjectStatus newProjectStatus = ProjectStatusFixture.New("test project");
            newProjectStatus.Messages = new Message[] { firstMessage, latestMessage };
			PollIntervalReporter pollIntervalReporter = new PollIntervalReporter(newProjectStatus, newProjectStatus);

            System.Text.StringBuilder expected = new System.Text.StringBuilder();
            expected.AppendLine(firstMessage.Text);
            expected.Append(latestMessage.Text);


			Assert.Equal(new Message(expected.ToString()), pollIntervalReporter.AllStatusMessages);
#endif
        }

		[Fact]
		public void CallingLatestStatusMessageWhenThereAreNoneIsSafe()
		{
#if false
            ProjectStatus newProjectStatus = ProjectStatusFixture.New("test project");
			newProjectStatus.Messages = new Message[] {};
			PollIntervalReporter pollIntervalReporter = new PollIntervalReporter(newProjectStatus, newProjectStatus);

			Assert.Equal(new Message("").ToString(), pollIntervalReporter.AllStatusMessages.ToString());
#endif
		}

		[Fact]
		public void TwoSuccessesMeansBuildIsStillSuccessful()
		{
#if false
            ProjectStatus lastProjectStatus = ProjectStatusFixture.New("last successful", IntegrationStatus.Success);
			ProjectStatus newProjectStatus = ProjectStatusFixture.New("new successful", IntegrationStatus.Success);
			PollIntervalReporter pollIntervalReporter = new PollIntervalReporter(lastProjectStatus, newProjectStatus);

			Assert.Equal(BuildTransition.StillSuccessful, pollIntervalReporter.BuildTransition);
#endif
		}

		[Fact]
		public void TwoFailuresMeansBuildIsStillFailing()
		{
#if false
            ProjectStatus lastProjectStatus = ProjectStatusFixture.New("last failed", IntegrationStatus.Failure);
			ProjectStatus newProjectStatus = ProjectStatusFixture.New("new failed", IntegrationStatus.Failure);
			PollIntervalReporter pollIntervalReporter = new PollIntervalReporter(lastProjectStatus, newProjectStatus);

			Assert.Equal(BuildTransition.StillFailing, pollIntervalReporter.BuildTransition);
#endif
		}

		[Fact]
		public void FailureThenSuccessMeansBuildIsFixed()
		{
#if false
            ProjectStatus lastProjectStatus = ProjectStatusFixture.New("last failed", IntegrationStatus.Failure);
			ProjectStatus newProjectStatus = ProjectStatusFixture.New("new success", IntegrationStatus.Success);
			PollIntervalReporter pollIntervalReporter = new PollIntervalReporter(lastProjectStatus, newProjectStatus);

			Assert.Equal(BuildTransition.Fixed, pollIntervalReporter.BuildTransition);
#endif
		}
		
		[Fact]
		public void SuccessThenFailureMeansBuildIsBroken()
		{
#if false
            ProjectStatus lastProjectStatus = ProjectStatusFixture.New("last success", IntegrationStatus.Success);
			ProjectStatus newProjectStatus = ProjectStatusFixture.New("new failed", IntegrationStatus.Failure);
			PollIntervalReporter pollIntervalReporter = new PollIntervalReporter(lastProjectStatus, newProjectStatus);

			Assert.Equal(BuildTransition.Broken, pollIntervalReporter.BuildTransition);
#endif
		}		
		
		[Fact]
		public void BuildCompletedDuringPollIntervalIfLastBuildDateChanged()
		{
#if false
            ProjectStatus lastProjectStatus = ProjectStatusFixture.New(IntegrationStatus.Success, new DateTime(2007, 1, 1));
			ProjectStatus newProjectStatus = ProjectStatusFixture.New(IntegrationStatus.Success, new DateTime(2007, 1, 2));
			PollIntervalReporter pollIntervalReporter = new PollIntervalReporter(lastProjectStatus, newProjectStatus);

			Assert.True(pollIntervalReporter.IsAnotherBuildComplete);
#endif
		}
		
		[Fact]
		public void LatestBuildWasSuccessfulIfNewProjectStatusIsSuccess()
		{
#if false
            ProjectStatus lastProjectStatus = ProjectStatusFixture.New(IntegrationStatus.Failure, new DateTime(2007, 1, 1));
			ProjectStatus newProjectStatus = ProjectStatusFixture.New(IntegrationStatus.Success, new DateTime(2007, 1, 2));
			PollIntervalReporter pollIntervalReporter = new PollIntervalReporter(lastProjectStatus, newProjectStatus);

			Assert.True(pollIntervalReporter.WasLatestBuildSuccessful);
#endif
		}
	}
}
