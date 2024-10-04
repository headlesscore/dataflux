using System;
using Exortech.NetReflector;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core.Triggers;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Triggers
{
	
	public class MultipleTriggerTest : IntegrationFixture
	{
		private Mock<ITrigger> subTrigger1Mock;
		private Mock<ITrigger> subTrigger2Mock;
		private ITrigger subTrigger1;
		private ITrigger subTrigger2;
		private MultipleTrigger trigger;

		// [SetUp]
		public void Setup()
		{
			subTrigger1Mock = new Mock<ITrigger>();
			subTrigger2Mock = new Mock<ITrigger>();
			subTrigger1 = (ITrigger) subTrigger1Mock.Object;
			subTrigger2 = (ITrigger) subTrigger2Mock.Object;
			trigger = new MultipleTrigger();
			trigger.Triggers = new ITrigger[] {subTrigger1, subTrigger2};
		}

		private void VerifyAll()
		{
			subTrigger1Mock.Verify();
			subTrigger2Mock.Verify();
		}

		[Fact]
		public void ShouldReturnNoBuildWhenNoTriggers()
		{
			trigger = new MultipleTrigger();
			Assert.Null(trigger.Fire());
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public void ShouldNotFailWhenNoTriggersAndIntegrationCompletedCalled()
		{
			trigger = new MultipleTrigger();
			trigger.IntegrationCompleted();
		}

		[Fact]
		public void ShouldPassThroughIntegrationCompletedCallToAllSubTriggers()
		{
			subTrigger1Mock.Setup(_trigger => _trigger.IntegrationCompleted()).Verifiable();
			subTrigger2Mock.Setup(_trigger => _trigger.IntegrationCompleted()).Verifiable();
			trigger.IntegrationCompleted();
			VerifyAll();
		}

		[Fact]
		public void ShouldReturnNoBuildIfAllNoBuild()
		{
			subTrigger1Mock.Setup(_trigger => _trigger.Fire()).Returns(() => null).Verifiable();
			subTrigger2Mock.Setup(_trigger => _trigger.Fire()).Returns(() => null).Verifiable();
			Assert.Null(trigger.Fire());
			VerifyAll();
		}

		[Fact]
		public void ShouldReturnIfModificationExistsNoForceBuild()
		{
			subTrigger1Mock.Setup(_trigger => _trigger.Fire()).Returns(() => null).Verifiable();
			subTrigger2Mock.Setup(_trigger => _trigger.Fire()).Returns(ModificationExistRequest()).Verifiable();
			Assert.Equal(ModificationExistRequest(), trigger.Fire());
			VerifyAll();
		}

		[Fact]
		public void ShouldNotCareAboutOrderingForChecking()
		{
			subTrigger1Mock.Setup(_trigger => _trigger.Fire()).Returns(() => null).Verifiable();
			subTrigger2Mock.Setup(_trigger => _trigger.Fire()).Returns(ModificationExistRequest()).Verifiable();
			Assert.Equal(ModificationExistRequest(), trigger.Fire());
			VerifyAll();
		}

		[Fact]
		public void ShouldReturnForceBuildIfOneForceBuildAndOneNoBuild()
		{
			subTrigger1Mock.Setup(_trigger => _trigger.Fire()).Returns(() => null).Verifiable();
			subTrigger2Mock.Setup(_trigger => _trigger.Fire()).Returns(ForceBuildRequest()).Verifiable();
			Assert.Equal(ForceBuildRequest(), trigger.Fire());
			VerifyAll();
		}

		[Fact]
		public void ShouldReturnForceBuildIfOneForceBuildAndOneIfModifications()
		{
			subTrigger1Mock.Setup(_trigger => _trigger.Fire()).Returns(ModificationExistRequest()).Verifiable();
			subTrigger2Mock.Setup(_trigger => _trigger.Fire()).Returns(ForceBuildRequest()).Verifiable();
			Assert.Equal(ForceBuildRequest(), trigger.Fire());
			VerifyAll();
		}

		[Fact]
		public void ShouldNotCareAboutOrderingForCheckingForceBuild()
		{
			subTrigger1Mock.Setup(_trigger => _trigger.Fire()).Returns(ForceBuildRequest()).Verifiable();
			subTrigger2Mock.Setup(_trigger => _trigger.Fire()).Returns(ModificationExistRequest()).Verifiable();
			Assert.Equal(ForceBuildRequest(), trigger.Fire());
			VerifyAll();
		}

		[Fact]
		public void ShouldReturnForceBuildIfAllForceBuild()
		{
			subTrigger1Mock.Setup(_trigger => _trigger.Fire()).Returns(ForceBuildRequest()).Verifiable();
			subTrigger2Mock.Setup(_trigger => _trigger.Fire()).Returns(ForceBuildRequest()).Verifiable();
			Assert.Equal(ForceBuildRequest(), trigger.Fire());
			VerifyAll();
		}

		[Fact]
		public void ShouldReturnNeverIfNoTriggerExists()
		{
			trigger = new MultipleTrigger();
			Assert.Equal(DateTime.MaxValue, trigger.NextBuild);
		}

		[Fact]
		public void ShouldReturnEarliestTriggerTimeForNextBuild()
		{
			DateTime earlierDate = new DateTime(2005, 1, 1);
			subTrigger1Mock.SetupGet(_trigger => _trigger.NextBuild).Returns(earlierDate).Verifiable();
			DateTime laterDate = new DateTime(2005, 1, 2);
			subTrigger2Mock.SetupGet(_trigger => _trigger.NextBuild).Returns(laterDate).Verifiable();
			Assert.Equal(earlierDate, trigger.NextBuild);
		}

		[Fact]
		public void ShouldPopulateFromConfiguration()
		{
			string xml = @"<multiTrigger operator=""And""><triggers><intervalTrigger /></triggers></multiTrigger>";
			trigger = (MultipleTrigger) NetReflector.Read(xml);
			Assert.Equal(1, trigger.Triggers.Length);
			Assert.Equal(Operator.And, trigger.Operator);
		}

		[Fact]
		public void ShouldPopulateFromConfigurationWithComment()
		{
			string xml = @"<multiTrigger><!-- foo --><triggers><intervalTrigger /></triggers></multiTrigger>";
			trigger = (MultipleTrigger) NetReflector.Read(xml);
			Assert.Equal(1, trigger.Triggers.Length);
			Assert.Equal(typeof(IntervalTrigger), trigger.Triggers[0].GetType());
		}

		[Fact]
		public void ShouldPopulateFromMinimalConfiguration()
		{
			string xml = @"<multiTrigger />";
			trigger = (MultipleTrigger) NetReflector.Read(xml);
			Assert.Equal(0, trigger.Triggers.Length);
			Assert.Equal(Operator.Or, trigger.Operator);			
		}

		[Fact]
		public void UsingAndConditionOnlyTriggersBuildIfBothTriggersShouldBuild()
		{
			trigger.Operator = Operator.And;
			subTrigger1Mock.Setup(_trigger => _trigger.Fire()).Returns(() => null).Verifiable();
			subTrigger2Mock.Setup(_trigger => _trigger.Fire()).Returns(ForceBuildRequest()).Verifiable();
			Assert.Null(trigger.Fire());
		}
	}
}
