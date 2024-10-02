using System;
using Exortech.NetReflector;
using Moq;
using NUnit.Framework;
using ThoughtWorks.CruiseControl.Core.Triggers;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.Remote;
using ThoughtWorks.CruiseControl.Core.Config;
using NUnit.Framework.Legacy;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Triggers
{
	[TestFixture]
	public class ScheduleTriggerTest : IntegrationFixture
	{
		private Mock<DateTimeProvider> mockDateTime;
		private ScheduleTrigger trigger;

		[SetUp]
		public void Setup()
		{
			Source = "ScheduleTrigger";
			mockDateTime = new Mock<DateTimeProvider>();
			trigger = new ScheduleTrigger((DateTimeProvider) mockDateTime.Object);
		}

		[TearDown]
		public void VerifyAll()
		{
			mockDateTime.Verify();
		}

		[Test]
		public void ShouldRunIntegrationIfCalendarTimeIsAfterIntegrationTime()
		{
			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 1, 1, 23, 25, 0, 0));
			trigger.Time = "23:30";
			trigger.BuildCondition = BuildCondition.IfModificationExists;
			ClassicAssert.IsNull(trigger.Fire());
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
            mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 1, 1, 23, 31, 0, 0));
			ClassicAssert.AreEqual(ModificationExistRequest(), trigger.Fire());
		}

		[Test]
		public void ShouldRunIntegrationOnTheNextDay()
		{
			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 1, 1, 23, 25, 0, 0));
			trigger.Time = "23:30";
			trigger.BuildCondition = BuildCondition.IfModificationExists;
			ClassicAssert.IsNull(trigger.Fire());

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 1, 2, 1, 1, 0, 0));
			ClassicAssert.AreEqual(ModificationExistRequest(), trigger.Fire());
		}

		[Test]
		public void ShouldIncrementTheIntegrationTimeToTheNextDayAfterIntegrationIsCompleted()
		{
			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 6, 27, 13, 00, 0, 0));
			trigger.Time = "14:30";
			trigger.BuildCondition = BuildCondition.IfModificationExists;
			ClassicAssert.IsNull(trigger.Fire());

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 6, 27, 15, 00, 0, 0));
			ClassicAssert.AreEqual(ModificationExistRequest(), trigger.Fire());
			trigger.IntegrationCompleted();
			ClassicAssert.IsNull(trigger.Fire());

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 6, 28, 15, 00, 0, 0));
			ClassicAssert.AreEqual(ModificationExistRequest(), trigger.Fire());
		}

		[Test]
		public void ShouldReturnSpecifiedBuildConditionWhenShouldRunIntegration()
		{
			foreach (BuildCondition expectedCondition in Enum.GetValues(typeof (BuildCondition)))
			{
				mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 1, 1, 23, 25, 0, 0));
				trigger.Time = "23:30";
				trigger.BuildCondition = expectedCondition;
				ClassicAssert.IsNull(trigger.Fire());

				mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 1, 1, 23, 31, 0, 0));
				ClassicAssert.AreEqual(Request(expectedCondition), trigger.Fire());
			}
		}

		[Test]
		public void ShouldOnlyRunOnSpecifiedDays()
		{
			trigger.WeekDays = new DayOfWeek[] {DayOfWeek.Monday, DayOfWeek.Wednesday};
			trigger.BuildCondition = BuildCondition.ForceBuild;
			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 11, 30));
			ClassicAssert.AreEqual(new DateTime(2004, 12, 1), trigger.NextBuild);
			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 12, 1, 0, 0, 1));
			ClassicAssert.AreEqual(ForceBuildRequest(), trigger.Fire());

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 12, 2));
			ClassicAssert.IsNull(trigger.Fire());
		}

		[Test]
		public void ShouldFullyPopulateFromReflector()
		{
			string xml = string.Format(@"<scheduleTrigger name=""nightly"" time=""12:00:00"" buildCondition=""ForceBuild"">
<weekDays>
	<weekDay>Monday</weekDay>
	<weekDay>Tuesday</weekDay>
</weekDays>
</scheduleTrigger>");
			trigger = (ScheduleTrigger) NetReflector.Read(xml);
			ClassicAssert.AreEqual("12:00:00", trigger.Time);
			ClassicAssert.AreEqual(DayOfWeek.Monday, trigger.WeekDays[0]);
			ClassicAssert.AreEqual(DayOfWeek.Tuesday, trigger.WeekDays[1]);
			ClassicAssert.AreEqual(BuildCondition.ForceBuild, trigger.BuildCondition);
			ClassicAssert.AreEqual("nightly", trigger.Name);
		}

		[Test]
		public void ShouldMinimallyPopulateFromReflector()
		{
			string xml = string.Format(@"<scheduleTrigger time=""10:00:00"" />");
			trigger = (ScheduleTrigger) NetReflector.Read(xml);
			ClassicAssert.AreEqual("10:00:00", trigger.Time);
			ClassicAssert.AreEqual(7, trigger.WeekDays.Length);
			ClassicAssert.AreEqual(BuildCondition.IfModificationExists, trigger.BuildCondition);
			ClassicAssert.AreEqual("ScheduleTrigger", trigger.Name);
		}

		[Test]
		public void NextBuildTimeShouldBeSameTimeNextDay()
		{
			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2005, 2, 4, 13, 13, 0));
			trigger.Time = "10:00";
			trigger.WeekDays = new DayOfWeek[] {DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday};
			trigger.IntegrationCompleted();
			DateTime expectedDate = new DateTime(2005, 2, 5, 10, 0, 0);
			ClassicAssert.AreEqual(expectedDate, trigger.NextBuild);
		}

		[Test]
		public void NextBuildTimeShouldBeTheNextSpecifiedDay()
		{
			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2005, 2, 4, 13, 13, 0));		// Friday
			trigger.Time = "10:00";
			trigger.WeekDays = new DayOfWeek[] {DayOfWeek.Friday, DayOfWeek.Sunday};
			ClassicAssert.AreEqual(new DateTime(2005, 2, 6, 10, 0, 0), trigger.NextBuild);		// Sunday

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2005, 2, 6, 13, 13, 0));		// Sunday
			ClassicAssert.AreEqual(ModificationExistRequest(), trigger.Fire());
			trigger.IntegrationCompleted();
			ClassicAssert.AreEqual(new DateTime(2005, 2, 11, 10, 0, 0), trigger.NextBuild);	// Friday
		}

		[Test]
		public void NextBuildTimeShouldBeTheNextSpecifiedDayWithTheNextDayFarAway()
		{
			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2005, 2, 4, 13, 13, 0));
			trigger.Time = "10:00";
			trigger.WeekDays = new DayOfWeek[] {DayOfWeek.Friday, DayOfWeek.Thursday};
			ClassicAssert.AreEqual(new DateTime(2005, 2, 10, 10, 0, 0), trigger.NextBuild);

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2005, 2, 10, 13, 13, 0));
			ClassicAssert.AreEqual(ModificationExistRequest(), trigger.Fire());
			trigger.IntegrationCompleted();
			ClassicAssert.AreEqual(new DateTime(2005, 2, 11, 10, 0, 0), trigger.NextBuild);
		}

		[Test]
		public void ShouldNotUpdateNextBuildTimeUnlessScheduledBuildHasRun()
		{
			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2005, 2, 4, 9, 0, 1));
			trigger.Time = "10:00";
			ClassicAssert.AreEqual(new DateTime(2005, 2, 4, 10, 0, 0), trigger.NextBuild);			

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2005, 2, 4, 10, 0, 1));
			trigger.IntegrationCompleted();
			ClassicAssert.AreEqual(new DateTime(2005, 2, 4, 10, 0, 0), trigger.NextBuild);			
		}


        [Test]
        public void RandomOffSetInMinutesFromTimeShouldBePositive()
        {
            ClassicAssert.That(delegate { trigger.RandomOffSetInMinutesFromTime = -10; },
                        Throws.TypeOf<ThoughtWorks.CruiseControl.Core.Config.ConfigurationException>());

        }

        [Test]
        public void RandomOffSetInMinutesFromTimeMayNotExceedMidnight()
        {
            mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2005, 2, 4, 9, 0, 1));
         //whatever random time is choosen, the resulted time will still be after midnight
            trigger.Time = "23:59";
            trigger.RandomOffSetInMinutesFromTime = 1;
            ClassicAssert.That(delegate { DateTime x = trigger.NextBuild; },
                        Throws.TypeOf<ThoughtWorks.CruiseControl.Core.Config.ConfigurationException>());

        }

        [Test]
        public void TimeFailsWithInvalidDate()
        {
            var trigger = new ScheduleTrigger();
            ClassicAssert.Throws<ConfigurationException>(() => trigger.Time = "plain wrong!");
        }
	}
}
