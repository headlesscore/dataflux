using System;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Monitoring
{
	[TestFixture]
	public class BuildDurationTrackerTest
	{
		private BuildDurationTracker tracker;
		private StubCurrentTimeProvider currentTimeProvider;

		[SetUp]
		public void SetUp()
		{
			currentTimeProvider = new StubCurrentTimeProvider();
			tracker = new BuildDurationTracker(currentTimeProvider);
		}

		[Test]
		public void WhenNoBuildsHaveOccurredPropertiesReturnValuesThatIndicateThis()
		{
            //ClassicAssert.AreEqual(TimeSpan.MaxValue, tracker.LastBuildDuration);
            ClassicAssert.AreEqual(TimeSpan.MaxValue, tracker.LastBuildDuration);
			ClassicAssert.AreEqual(TimeSpan.MaxValue, tracker.EstimatedTimeRemainingOnCurrentBuild);
			ClassicAssert.AreEqual(false, tracker.IsBuildInProgress);
		}

		[Test]
		public void WhenABuildStartsWithNoHistoryTheDurationAndEstimatedTimeAreStillNotCalculated()
		{
			tracker.OnBuildStart();
			ClassicAssert.AreEqual(TimeSpan.MaxValue, tracker.LastBuildDuration);
			ClassicAssert.AreEqual(TimeSpan.MaxValue, tracker.EstimatedTimeRemainingOnCurrentBuild);
			ClassicAssert.AreEqual(true, tracker.IsBuildInProgress);
		}

		[Test]
		public void IfANewBuildStartsBeforeOnCompletesTheDurationAndEstimatedTimeAreStillNotCalculated()
		{
			tracker.OnBuildStart();

			tracker.OnBuildStart();
			ClassicAssert.AreEqual(TimeSpan.MaxValue, tracker.LastBuildDuration);
			ClassicAssert.AreEqual(TimeSpan.MaxValue, tracker.EstimatedTimeRemainingOnCurrentBuild);
			ClassicAssert.AreEqual(true, tracker.IsBuildInProgress);
		}

		[Test]
		public void AfterASuccessfulBuildTheLastBuildTimeIsCalculated()
		{
			DateTime startTime = new DateTime(2005, 7, 20, 10, 15, 02);

			currentTimeProvider.SetNow(startTime);
			tracker.OnBuildStart();
			currentTimeProvider.SetNow(startTime.AddHours(2));
			tracker.OnSuccessfulBuild();

			ClassicAssert.AreEqual(TimeSpan.FromHours(2), tracker.LastBuildDuration);

			currentTimeProvider.SetNow(startTime);
			tracker.OnBuildStart();
			currentTimeProvider.SetNow(startTime.AddMinutes(4));

			ClassicAssert.AreEqual(TimeSpan.FromHours(2), tracker.LastBuildDuration);
			tracker.OnSuccessfulBuild();

			ClassicAssert.AreEqual(TimeSpan.FromMinutes(4), tracker.LastBuildDuration);

		}
		
		[Test]
		public void TheEstimatedTimeForThisBuildIsBasedOnTheDuratuionOfTheLastBuild()
		{
			DateTime startTime = new DateTime(2005, 7, 20, 10, 15, 02);

			currentTimeProvider.SetNow(startTime);
			tracker.OnBuildStart();
			currentTimeProvider.SetNow(startTime.AddHours(3));
			tracker.OnSuccessfulBuild();

			ClassicAssert.AreEqual(TimeSpan.FromHours(3), tracker.LastBuildDuration);

			startTime = startTime.AddHours(5);
			currentTimeProvider.SetNow(startTime);
			
			tracker.OnBuildStart();
			currentTimeProvider.SetNow(startTime.AddHours(1));

			ClassicAssert.AreEqual(TimeSpan.FromHours(2), tracker.EstimatedTimeRemainingOnCurrentBuild);
			tracker.OnSuccessfulBuild();			
		}

		[Test]
		public void WhenTheCurrentBuildTakesLongerTheEstimatedTimeRemainingIsNegative()
		{
			DateTime startTime = new DateTime(2005, 7, 20, 10, 15, 02);

			currentTimeProvider.SetNow(startTime);
			tracker.OnBuildStart();
			currentTimeProvider.SetNow(startTime.AddHours(3));
			tracker.OnSuccessfulBuild();

			ClassicAssert.AreEqual(TimeSpan.FromHours(3), tracker.LastBuildDuration);

			startTime = startTime.AddHours(5);
			currentTimeProvider.SetNow(startTime);
			
			tracker.OnBuildStart();
			currentTimeProvider.SetNow(startTime.AddHours(4));

			ClassicAssert.AreEqual(TimeSpan.FromHours(-1), tracker.EstimatedTimeRemainingOnCurrentBuild);
			tracker.OnSuccessfulBuild();			
		}

	}
}
