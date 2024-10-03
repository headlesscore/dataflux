using System;
using Xunit;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Monitoring
{
	public class BuildDurationTrackerTest
	{
		private BuildDurationTracker tracker;
		private StubCurrentTimeProvider currentTimeProvider;

		//[SetUp]
		public void SetUp()
		{
			currentTimeProvider = new StubCurrentTimeProvider();
			tracker = new BuildDurationTracker(currentTimeProvider);
		}

		[Fact]
		public void WhenNoBuildsHaveOccurredPropertiesReturnValuesThatIndicateThis()
		{
            //Assert.Equal(TimeSpan.MaxValue, tracker.LastBuildDuration);
            Assert.Equal(TimeSpan.MaxValue, tracker.LastBuildDuration);
			Assert.Equal(TimeSpan.MaxValue, tracker.EstimatedTimeRemainingOnCurrentBuild);
			Assert.False(tracker.IsBuildInProgress);
		}

		[Fact]
		public void WhenABuildStartsWithNoHistoryTheDurationAndEstimatedTimeAreStillNotCalculated()
		{
			tracker.OnBuildStart();
			Assert.Equal(TimeSpan.MaxValue, tracker.LastBuildDuration);
			Assert.Equal(TimeSpan.MaxValue, tracker.EstimatedTimeRemainingOnCurrentBuild);
			Assert.True(tracker.IsBuildInProgress);
		}

		[Fact]
		public void IfANewBuildStartsBeforeOnCompletesTheDurationAndEstimatedTimeAreStillNotCalculated()
		{
			tracker.OnBuildStart();

			tracker.OnBuildStart();
			Assert.Equal(TimeSpan.MaxValue, tracker.LastBuildDuration);
			Assert.Equal(TimeSpan.MaxValue, tracker.EstimatedTimeRemainingOnCurrentBuild);
			Assert.True(tracker.IsBuildInProgress);
		}

		[Fact]
		public void AfterASuccessfulBuildTheLastBuildTimeIsCalculated()
		{
			DateTime startTime = new DateTime(2005, 7, 20, 10, 15, 02);

			currentTimeProvider.SetNow(startTime);
			tracker.OnBuildStart();
			currentTimeProvider.SetNow(startTime.AddHours(2));
			tracker.OnSuccessfulBuild();

			Assert.Equal(TimeSpan.FromHours(2), tracker.LastBuildDuration);

			currentTimeProvider.SetNow(startTime);
			tracker.OnBuildStart();
			currentTimeProvider.SetNow(startTime.AddMinutes(4));

			Assert.Equal(TimeSpan.FromHours(2), tracker.LastBuildDuration);
			tracker.OnSuccessfulBuild();

			Assert.Equal(TimeSpan.FromMinutes(4), tracker.LastBuildDuration);

		}
		
		[Fact]
		public void TheEstimatedTimeForThisBuildIsBasedOnTheDuratuionOfTheLastBuild()
		{
			DateTime startTime = new DateTime(2005, 7, 20, 10, 15, 02);

			currentTimeProvider.SetNow(startTime);
			tracker.OnBuildStart();
			currentTimeProvider.SetNow(startTime.AddHours(3));
			tracker.OnSuccessfulBuild();

			Assert.Equal(TimeSpan.FromHours(3), tracker.LastBuildDuration);

			startTime = startTime.AddHours(5);
			currentTimeProvider.SetNow(startTime);
			
			tracker.OnBuildStart();
			currentTimeProvider.SetNow(startTime.AddHours(1));

			Assert.Equal(TimeSpan.FromHours(2), tracker.EstimatedTimeRemainingOnCurrentBuild);
			tracker.OnSuccessfulBuild();			
		}

		[Fact]
		public void WhenTheCurrentBuildTakesLongerTheEstimatedTimeRemainingIsNegative()
		{
			DateTime startTime = new DateTime(2005, 7, 20, 10, 15, 02);

			currentTimeProvider.SetNow(startTime);
			tracker.OnBuildStart();
			currentTimeProvider.SetNow(startTime.AddHours(3));
			tracker.OnSuccessfulBuild();

			Assert.Equal(TimeSpan.FromHours(3), tracker.LastBuildDuration);

			startTime = startTime.AddHours(5);
			currentTimeProvider.SetNow(startTime);
			
			tracker.OnBuildStart();
			currentTimeProvider.SetNow(startTime.AddHours(4));

			Assert.Equal(TimeSpan.FromHours(-1), tracker.EstimatedTimeRemainingOnCurrentBuild);
			tracker.OnSuccessfulBuild();			
		}

	}
}
