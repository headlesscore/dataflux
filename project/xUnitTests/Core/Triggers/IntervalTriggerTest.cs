using System;
using Exortech.NetReflector;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core.Triggers;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Triggers
{
	
	public class IntervalTriggerTest : IntegrationFixture
	{
		private Mock<DateTimeProvider> mockDateTime;
		private IntervalTrigger trigger;
		private DateTime initialDateTimeNow;

		// [SetUp]
		public void Setup()
		{
			Source = "IntervalTrigger";
			mockDateTime = new Mock<DateTimeProvider>();
			initialDateTimeNow = new DateTime(2002, 1, 2, 3, 0, 0, 0);
			mockDateTime.SetupGet(provider => provider.Now).Returns(this.initialDateTimeNow);
			trigger = new IntervalTrigger((DateTimeProvider) mockDateTime.Object);
		}

		public void VerifyAll()
		{
			mockDateTime.Verify();
		}

		[Fact]
		public void ShouldFullyPopulateFromReflector()
		{
			string xml = string.Format(@"<intervalTrigger name=""continuous"" seconds=""2"" initialSeconds=""1"" buildCondition=""ForceBuild"" />");
			trigger = (IntervalTrigger) NetReflector.Read(xml);
            Assert.Equal(2, trigger.IntervalSeconds, "trigger.IntervalSeconds");
            Assert.Equal(1, trigger.InitialIntervalSeconds, "trigger.InitialIntervalSeconds");
            Assert.Equal(BuildCondition.ForceBuild, trigger.BuildCondition, "trigger.BuildCondition");
            Assert.Equal("continuous", trigger.Name, "trigger.Name");
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public void ShouldDefaultPopulateFromReflector()
		{
			string xml = string.Format(@"<intervalTrigger />");
			trigger = (IntervalTrigger) NetReflector.Read(xml);
            Assert.Equal(IntervalTrigger.DefaultIntervalSeconds, trigger.IntervalSeconds, "trigger.IntervalSeconds");
            Assert.Equal(IntervalTrigger.DefaultIntervalSeconds, trigger.InitialIntervalSeconds);
            Assert.Equal(BuildCondition.IfModificationExists, trigger.BuildCondition);
            Assert.Equal("IntervalTrigger", trigger.Name);
		}

		[Fact]
		public void VerifyThatShouldRequestIntegrationAfterTenSeconds()
		{
			trigger.IntervalSeconds = 10;
			trigger.BuildCondition = BuildCondition.IfModificationExists;

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 1, 1, 1, 0, 0, 0));
            Assert.Equal(ModificationExistRequest(), trigger.Fire());
			trigger.IntegrationCompleted();

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 1, 1, 1, 0, 5, 0)); // 5 seconds later
            Assert.Null(trigger.Fire());

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 1, 1, 1, 0, 9, 0)); // 4 seconds later

			// still before 1sec
            Assert.Null(trigger.Fire());

			// sleep beyond the 1sec mark
			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 1, 1, 1, 0, 14, 0)); // 5 seconds later

			Assert.Equal(ModificationExistRequest(), trigger.Fire());
			trigger.IntegrationCompleted();
            Assert.Null(trigger.Fire());
			VerifyAll();
		}

		[Fact]
		public void VerifyThatShouldRequestInitialIntegrationAfterTwoSecondsAndSubsequentIntegrationsAfterTenSeconds()
		{
			trigger.InitialIntervalSeconds = 2;
			trigger.IntervalSeconds = 10;
			trigger.BuildCondition = BuildCondition.IfModificationExists;

			//initialDateTimeNow = new DateTime(2002, 1, 2, 3, 0, 0, 0);

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2002, 1, 2, 3, 0, 0, 0));		// now
            Assert.Null(trigger.Fire());

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2002, 1, 2, 3, 0, 1, 0));		// 1 second later
            Assert.Null(trigger.Fire());

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2002, 1, 2, 3, 0, 2, 0));		// 2 seconds later
            Assert.Equal(ModificationExistRequest(), trigger.Fire());
			trigger.IntegrationCompleted();

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2002, 1, 2, 3, 0, 3, 0));		// 1 second later
            Assert.Null(trigger.Fire());

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2002, 1, 2, 3, 0, 11, 0));		// 9 seconds later

			// still before 1sec
            Assert.Null(trigger.Fire());

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2002, 1, 2, 3, 0, 14, 0)); // 2 seconds after trigger
            Assert.Equal(ModificationExistRequest(), trigger.Fire());
			trigger.IntegrationCompleted();
            Assert.Null(trigger.Fire());
			VerifyAll();
		}


		[Fact]
		public void ProcessTrigger()
		{
			trigger.IntervalSeconds = 0.5;
			trigger.BuildCondition = BuildCondition.IfModificationExists;

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 1, 1, 1, 0, 0, 0));
            Assert.Equal(ModificationExistRequest(), trigger.Fire(), "trigger.Fire()");

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 1, 1, 1, 0, 0, 550));

            Assert.Equal(ModificationExistRequest(), trigger.Fire(), "trigger.Fire()");
			trigger.IntegrationCompleted();
			Assert.Null(trigger.Fire());

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 1, 1, 1, 0, 1, 50));

            Assert.Equal(ModificationExistRequest(), trigger.Fire(), "trigger.Fire()");
			trigger.IntegrationCompleted();
            Assert.Equal(null, trigger.Fire(), "trigger.Fire()");

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 1, 1, 1, 0, 1, 550));

            Assert.Equal(ModificationExistRequest(), trigger.Fire(), "trigger.Fire()");
			VerifyAll();
		}

		[Fact]
		public void ShouldReturnForceBuildRequestIfSpecified()
		{
			trigger.IntervalSeconds = 10;
			trigger.BuildCondition = BuildCondition.ForceBuild;
			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 1, 1, 1, 0, 0, 0));
            Assert.Equal(ForceBuildRequest(), trigger.Fire(), "trigger.Fire()");
			VerifyAll();			
		}

		[Fact]
		public void ShouldReturnInitialIntervalTimeForNextBuildOnServerStart()
		{
			trigger.InitialIntervalSeconds = 10;
			trigger.IntervalSeconds = 30;
            Assert.Equal(initialDateTimeNow.AddSeconds(10), trigger.NextBuild, "trigger.NextBuild");
			VerifyAll();
		}

		[Fact]
		public void ShouldReturnIntervalTimeIfLastBuildJustOccured()
		{
			trigger.IntervalSeconds = 10;
			DateTime stubNow = new DateTime(2004, 1, 1, 1, 0, 0, 0);
			mockDateTime.SetupGet(provider => provider.Now).Returns(stubNow);
			trigger.IntegrationCompleted();
            Assert.Equal(stubNow.AddSeconds(10), trigger.NextBuild, "trigger.NextBuild");
		}

		[Fact]
		public void ShouldReturnIntervalTimeIfInitialBuildJustOccured()
		{
			trigger.InitialIntervalSeconds = 10;
			trigger.IntervalSeconds = 30;
			DateTime stubNow = new DateTime(2004, 1, 1, 1, 0, 0, 0);
			mockDateTime.SetupGet(provider => provider.Now).Returns(stubNow);
			trigger.IntegrationCompleted();
            Assert.Equal(stubNow.AddSeconds(30), trigger.NextBuild, "trigger.NextBuild");
		}

	}
}
