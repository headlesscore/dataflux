using System;
using Exortech.NetReflector;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core.Triggers;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Triggers
{
	
	public class FilterTriggerTest : IntegrationFixture
	{
		private Mock<ITrigger> mockTrigger;
		private Mock<DateTimeProvider> mockDateTime;
		private FilterTrigger trigger;

		// [SetUp]
		protected void CreateMocksAndInitialiseObjectUnderTest()
		{
			mockTrigger = new Mock<ITrigger>();
			mockDateTime = new Mock<DateTimeProvider>();

			trigger = new FilterTrigger((DateTimeProvider) mockDateTime.Object);
			trigger.InnerTrigger = (ITrigger) mockTrigger.Object;
			trigger.StartTime = "10:00";
			trigger.EndTime = "11:00";
			trigger.WeekDays = new DayOfWeek[] {DayOfWeek.Wednesday};
			trigger.BuildCondition = BuildCondition.NoBuild;
		}

		// [TearDown]
		protected void VerifyMocks()
		{
			mockDateTime.Verify();
			mockTrigger.Verify();
		}

		[Fact]
		public void ShouldNotInvokeDecoratedTriggerWhenTimeAndWeekDayMatch()
		{
			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 12, 1, 10, 30, 0, 0)).Verifiable();

            Assert.Null(trigger.Fire());
            mockTrigger.VerifyNoOtherCalls();
		}

		[Fact]
		public void ShouldNotInvokeDecoratedTriggerWhenWeekDaysNotSpecified()
		{
			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 12, 1, 10, 30, 0, 0)).Verifiable();
			trigger.WeekDays = new DayOfWeek[] {};

            Assert.Null(trigger.Fire());

			mockTrigger.VerifyNoOtherCalls();
		}

		[Fact]
		public void ShouldInvokeDecoratedTriggerWhenTimeIsOutsideOfRange()
		{
			mockTrigger.Setup(_trigger => _trigger.Fire()).Returns(ModificationExistRequest()).Verifiable();
			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 12, 1, 11, 30, 0, 0)).Verifiable();

            Assert.Equal(ModificationExistRequest(), trigger.Fire());
		}

		[Fact]
		public void ShouldNotInvokeOverMidnightTriggerWhenCurrentTimeIsBeforeMidnight()
		{
			trigger.StartTime = "23:00";
			trigger.EndTime = "7:00";

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 12, 1, 23, 30, 0, 0)).Verifiable();

            Assert.Null(trigger.Fire());

			mockTrigger.VerifyNoOtherCalls();
		}

		[Fact]
		public void ShouldNotInvokeOverMidnightTriggerWhenCurrentTimeIsAfterMidnight()
		{
			trigger.StartTime = "23:00";
			trigger.EndTime = "7:00";

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 12, 1, 00, 30, 0, 0)).Verifiable();

            Assert.Null(trigger.Fire());

			mockTrigger.VerifyNoOtherCalls();
		}

		[Fact]
		public void ShouldInvokeOverMidnightTriggerWhenCurrentTimeIsOutsideOfRange()
		{
			trigger.StartTime = "23:00";
			trigger.EndTime = "7:00";

			mockTrigger.Setup(_trigger => _trigger.Fire()).Returns(ModificationExistRequest()).Verifiable();
			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 12, 1, 11, 30, 0, 0)).Verifiable();

            Assert.Equal(ModificationExistRequest(), trigger.Fire());
		}

		[Fact]
		public void ShouldNotInvokeDecoratedTriggerWhenTimeIsEqualToStartTimeOrEndTime()
		{
			MockSequence sequence = new MockSequence();
			mockDateTime.InSequence(sequence).SetupGet(provider => provider.Now).Returns(new DateTime(2004, 12, 1, 10, 00, 0, 0)).Verifiable();
			mockDateTime.InSequence(sequence).SetupGet(provider => provider.Now).Returns(new DateTime(2004, 12, 1, 11, 00, 0, 0)).Verifiable();

            Assert.Null(trigger.Fire());
            Assert.Null(trigger.Fire());

			mockTrigger.VerifyNoOtherCalls();
		}

		[Fact]
		public void ShouldNotInvokeDecoratedTriggerWhenTodayIsOneOfSpecifiedWeekdays()
		{
			mockTrigger.Setup(_trigger => _trigger.Fire()).Returns(ModificationExistRequest()).Verifiable();
			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 12, 2, 10, 30, 0, 0)).Verifiable();

            Assert.Equal(ModificationExistRequest(), trigger.Fire());
		}

		[Fact]
		public void ShouldDelegateIntegrationCompletedCallToInnerTrigger()
		{
			mockTrigger.Setup(_trigger => _trigger.IntegrationCompleted()).Verifiable();
			trigger.IntegrationCompleted();
		}

		[Fact]
		public void ShouldUseFilterEndTimeIfTriggerBuildTimeIsInFilter()
		{
			DateTime triggerNextBuildTime = new DateTime(2004, 12, 1, 10, 30, 00);
			mockTrigger.SetupGet(_trigger => _trigger.NextBuild).Returns(triggerNextBuildTime).Verifiable();
			Assert.Equal(new DateTime(2004, 12, 1, 11, 00, 00), trigger.NextBuild);
		}

		[Fact]
		public void ShouldNotFilterIfTriggerBuildDayIsNotInFilter()
		{
			DateTime triggerNextBuildTime = new DateTime(2004, 12, 4, 10, 00, 00);
			mockTrigger.SetupGet(_trigger => _trigger.NextBuild).Returns(triggerNextBuildTime).Verifiable();
            Assert.Equal(triggerNextBuildTime, trigger.NextBuild);
		}

		[Fact]
		public void ShouldNotFilterIfTriggerBuildTimeIsNotInFilter()
		{
			DateTime nextBuildTime = new DateTime(2004, 12, 1, 13, 30, 00);
			mockTrigger.SetupGet(_trigger => _trigger.NextBuild).Returns(nextBuildTime).Verifiable();
            Assert.Equal(nextBuildTime, trigger.NextBuild);
		}

		[Fact]
		public void ShouldFullyPopulateFromReflector()
		{
			string xml =
				string.Format(
					@"<filterTrigger startTime=""8:30:30"" endTime=""22:30:30"" buildCondition=""ForceBuild"">
											<trigger type=""scheduleTrigger"" time=""12:00:00""/>
											<weekDays>
												<weekDay>Monday</weekDay>
												<weekDay>Tuesday</weekDay>
											</weekDays>
										</filterTrigger>");
			trigger = (FilterTrigger) NetReflector.Read(xml);
            Assert.Equal("08:30:30", trigger.StartTime);
            Assert.Equal("22:30:30", trigger.EndTime);
			Assert.Equal(typeof (ScheduleTrigger), trigger.InnerTrigger.GetType());
			Assert.Equal(DayOfWeek.Monday, trigger.WeekDays[0]);
			Assert.Equal(DayOfWeek.Tuesday, trigger.WeekDays[1]);
			Assert.Equal(BuildCondition.ForceBuild, trigger.BuildCondition);
		}

		[Fact]
		public void ShouldMinimallyPopulateFromReflector()
		{
			string xml =
				string.Format(
					@"<filterTrigger>
											<trigger type=""scheduleTrigger"" time=""12:00:00"" />
										</filterTrigger>");
			trigger = (FilterTrigger) NetReflector.Read(xml);
            Assert.Equal("00:00:00", trigger.StartTime);
            Assert.Equal("23:59:59", trigger.EndTime);
            Assert.Equal(typeof(ScheduleTrigger), trigger.InnerTrigger.GetType());
            Assert.Equal(7, trigger.WeekDays.Length);
            Assert.Equal(BuildCondition.NoBuild, trigger.BuildCondition);
		}

		[Fact]
		public void ShouldHandleNestedFilterTriggers()
		{
			string xml =
				@"<filterTrigger startTime=""19:00"" endTime=""07:00"">
                    <trigger type=""filterTrigger"" startTime=""0:00"" endTime=""23:59:59"">
                        <trigger type=""intervalTrigger"" name=""continuous"" seconds=""900"" buildCondition=""ForceBuild""/>
                        <weekDays>
                            <weekDay>Saturday</weekDay>
                            <weekDay>Sunday</weekDay>
                        </weekDays>
                    </trigger>
				  </filterTrigger>";
			trigger = (FilterTrigger) NetReflector.Read(xml);
            Assert.Equal(typeof(FilterTrigger), trigger.InnerTrigger.GetType());
            Assert.Equal(typeof(IntervalTrigger), ((FilterTrigger)trigger.InnerTrigger).InnerTrigger.GetType());
		}

		[Fact]
		public void ShouldOnlyBuildBetween7AMAnd7PMOnWeekdays()
		{
			FilterTrigger outerTrigger = new FilterTrigger((DateTimeProvider) mockDateTime.Object);
			outerTrigger.StartTime = "19:00";
			outerTrigger.EndTime = "7:00";
			outerTrigger.InnerTrigger = trigger;
			
			trigger.StartTime = "0:00";
			trigger.EndTime = "23:59:59";
			trigger.WeekDays = new DayOfWeek[] { DayOfWeek.Saturday, DayOfWeek.Sunday };
			IntegrationRequest request = ModificationExistRequest();
			mockTrigger.Setup(_trigger => _trigger.Fire()).Returns(request);
			
			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2006, 8, 10, 11, 30, 0, 0)); // Thurs midday
            Assert.Equal(request, outerTrigger.Fire());
			
			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2006, 8, 10, 19, 30, 0, 0)); // Thurs evening
            Assert.Null(outerTrigger.Fire());			

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2006, 8, 12, 11, 30, 0, 0)); // Sat midday
            Assert.Null(outerTrigger.Fire());			

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2006, 8, 12, 19, 30, 0, 0)); // Sat evening
            Assert.Null(outerTrigger.Fire());			
		}
	}
}
