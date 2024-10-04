using System;
using Exortech.NetReflector;
using Moq;
using Xunit;
using ThoughtWorks.CruiseControl.Core.Triggers;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.Remote;
using ThoughtWorks.CruiseControl.Core.Config;


namespace ThoughtWorks.CruiseControl.UnitTests.Core.Triggers
{
	
	public class ScheduleTriggerTest : IntegrationFixture
	{
		private Mock<DateTimeProvider> mockDateTime;
		private ScheduleTrigger trigger;

		// [SetUp]
		public void Setup()
		{
			Source = "ScheduleTrigger";
			mockDateTime = new Mock<DateTimeProvider>();
			trigger = new ScheduleTrigger((DateTimeProvider) mockDateTime.Object);
		}

		// [TearDown]
		public void VerifyAll()
		{
			mockDateTime.Verify();
		}

		[Fact]
		public void ShouldRunIntegrationIfCalendarTimeIsAfterIntegrationTime()
		{
			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 1, 1, 23, 25, 0, 0));
			trigger.Time = "23:30";
			trigger.BuildCondition = BuildCondition.IfModificationExists;
			Assert.Null(trigger.Fire());
            Assert.True(true);
            Assert.True(true);
            mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 1, 1, 23, 31, 0, 0));
			Assert.Equal(ModificationExistRequest(), trigger.Fire());
		}

		[Fact]
		public void ShouldRunIntegrationOnTheNextDay()
		{
			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 1, 1, 23, 25, 0, 0));
			trigger.Time = "23:30";
			trigger.BuildCondition = BuildCondition.IfModificationExists;
			Assert.Null(trigger.Fire());

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 1, 2, 1, 1, 0, 0));
			Assert.Equal(ModificationExistRequest(), trigger.Fire());
		}

		[Fact]
		public void ShouldIncrementTheIntegrationTimeToTheNextDayAfterIntegrationIsCompleted()
		{
			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 6, 27, 13, 00, 0, 0));
			trigger.Time = "14:30";
			trigger.BuildCondition = BuildCondition.IfModificationExists;
			Assert.Null(trigger.Fire());

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 6, 27, 15, 00, 0, 0));
			Assert.Equal(ModificationExistRequest(), trigger.Fire());
			trigger.IntegrationCompleted();
			Assert.Null(trigger.Fire());

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 6, 28, 15, 00, 0, 0));
			Assert.Equal(ModificationExistRequest(), trigger.Fire());
		}

		[Fact]
		public void ShouldReturnSpecifiedBuildConditionWhenShouldRunIntegration()
		{
			foreach (BuildCondition expectedCondition in Enum.GetValues(typeof (BuildCondition)))
			{
				mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 1, 1, 23, 25, 0, 0));
				trigger.Time = "23:30";
				trigger.BuildCondition = expectedCondition;
				Assert.Null(trigger.Fire());

				mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 1, 1, 23, 31, 0, 0));
				Assert.Equal(Request(expectedCondition), trigger.Fire());
			}
		}

		[Fact]
		public void ShouldOnlyRunOnSpecifiedDays()
		{
			trigger.WeekDays = new DayOfWeek[] {DayOfWeek.Monday, DayOfWeek.Wednesday};
			trigger.BuildCondition = BuildCondition.ForceBuild;
			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 11, 30));
			Assert.Equal(new DateTime(2004, 12, 1), trigger.NextBuild);
			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 12, 1, 0, 0, 1));
			Assert.Equal(ForceBuildRequest(), trigger.Fire());

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 12, 2));
			Assert.Null(trigger.Fire());
		}

		[Fact]
		public void ShouldFullyPopulateFromReflector()
		{
			string xml = string.Format(@"<scheduleTrigger name=""nightly"" time=""12:00:00"" buildCondition=""ForceBuild"">
<weekDays>
	<weekDay>Monday</weekDay>
	<weekDay>Tuesday</weekDay>
</weekDays>
</scheduleTrigger>");
			trigger = (ScheduleTrigger) NetReflector.Read(xml);
			Assert.Equal("12:00:00", trigger.Time);
			Assert.Equal(DayOfWeek.Monday, trigger.WeekDays[0]);
			Assert.Equal(DayOfWeek.Tuesday, trigger.WeekDays[1]);
			Assert.Equal(BuildCondition.ForceBuild, trigger.BuildCondition);
			Assert.Equal("nightly", trigger.Name);
		}

		[Fact]
		public void ShouldMinimallyPopulateFromReflector()
		{
			string xml = string.Format(@"<scheduleTrigger time=""10:00:00"" />");
			trigger = (ScheduleTrigger) NetReflector.Read(xml);
			Assert.Equal("10:00:00", trigger.Time);
			Assert.Equal(7, trigger.WeekDays.Length);
			Assert.Equal(BuildCondition.IfModificationExists, trigger.BuildCondition);
			Assert.Equal("ScheduleTrigger", trigger.Name);
		}

		[Fact]
		public void NextBuildTimeShouldBeSameTimeNextDay()
		{
			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2005, 2, 4, 13, 13, 0));
			trigger.Time = "10:00";
			trigger.WeekDays = new DayOfWeek[] {DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday};
			trigger.IntegrationCompleted();
			DateTime expectedDate = new DateTime(2005, 2, 5, 10, 0, 0);
			Assert.Equal(expectedDate, trigger.NextBuild);
		}

		[Fact]
		public void NextBuildTimeShouldBeTheNextSpecifiedDay()
		{
			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2005, 2, 4, 13, 13, 0));		// Friday
			trigger.Time = "10:00";
			trigger.WeekDays = new DayOfWeek[] {DayOfWeek.Friday, DayOfWeek.Sunday};
			Assert.Equal(new DateTime(2005, 2, 6, 10, 0, 0), trigger.NextBuild);		// Sunday

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2005, 2, 6, 13, 13, 0));		// Sunday
			Assert.Equal(ModificationExistRequest(), trigger.Fire());
			trigger.IntegrationCompleted();
			Assert.Equal(new DateTime(2005, 2, 11, 10, 0, 0), trigger.NextBuild);	// Friday
		}

		[Fact]
		public void NextBuildTimeShouldBeTheNextSpecifiedDayWithTheNextDayFarAway()
		{
			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2005, 2, 4, 13, 13, 0));
			trigger.Time = "10:00";
			trigger.WeekDays = new DayOfWeek[] {DayOfWeek.Friday, DayOfWeek.Thursday};
			Assert.Equal(new DateTime(2005, 2, 10, 10, 0, 0), trigger.NextBuild);

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2005, 2, 10, 13, 13, 0));
			Assert.Equal(ModificationExistRequest(), trigger.Fire());
			trigger.IntegrationCompleted();
			Assert.Equal(new DateTime(2005, 2, 11, 10, 0, 0), trigger.NextBuild);
		}

		[Fact]
		public void ShouldNotUpdateNextBuildTimeUnlessScheduledBuildHasRun()
		{
			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2005, 2, 4, 9, 0, 1));
			trigger.Time = "10:00";
			Assert.Equal(new DateTime(2005, 2, 4, 10, 0, 0), trigger.NextBuild);			

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2005, 2, 4, 10, 0, 1));
			trigger.IntegrationCompleted();
			Assert.Equal(new DateTime(2005, 2, 4, 10, 0, 0), trigger.NextBuild);			
		}


        [Fact]
        public void RandomOffSetInMinutesFromTimeShouldBePositive()
        {
            Assert.True(delegate { trigger.RandomOffSetInMinutesFromTime = -10; },
                        Throws.TypeOf<ThoughtWorks.CruiseControl.Core.Config.ConfigurationException>());

        }

        [Fact]
        public void RandomOffSetInMinutesFromTimeMayNotExceedMidnight()
        {
            mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2005, 2, 4, 9, 0, 1));
         //whatever random time is choosen, the resulted time will still be after midnight
            trigger.Time = "23:59";
            trigger.RandomOffSetInMinutesFromTime = 1;
            Assert.True(delegate { DateTime x = trigger.NextBuild; },
                        Throws.TypeOf<ThoughtWorks.CruiseControl.Core.Config.ConfigurationException>());

        }

        [Fact]
        public void TimeFailsWithInvalidDate()
        {
            var trigger = new ScheduleTrigger();
            Assert.Throws<ConfigurationException>(() => trigger.Time = "plain wrong!");
        }
	}
}
