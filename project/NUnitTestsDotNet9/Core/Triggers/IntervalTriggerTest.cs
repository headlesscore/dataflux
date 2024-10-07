using System;
using Exortech.NetReflector;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Triggers;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Triggers
{
	[TestFixture]
	public class IntervalTriggerTest : IntegrationFixture
	{
		private Mock<DateTimeProvider> mockDateTime;
		private IntervalTrigger trigger;
		private DateTime initialDateTimeNow;

		[SetUp]
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

		[Test]
		public void ShouldFullyPopulateFromReflector()
		{
			string xml = string.Format(@"<intervalTrigger name=""continuous"" seconds=""2"" initialSeconds=""1"" buildCondition=""ForceBuild"" />");
			trigger = (IntervalTrigger) NetReflector.Read(xml);
            ClassicAssert.AreEqual(2, trigger.IntervalSeconds, "trigger.IntervalSeconds");
            ClassicAssert.AreEqual(1, trigger.InitialIntervalSeconds, "trigger.InitialIntervalSeconds");
            ClassicAssert.AreEqual(BuildCondition.ForceBuild, trigger.BuildCondition, "trigger.BuildCondition");
            ClassicAssert.AreEqual("continuous", trigger.Name, "trigger.Name");
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

		[Test]
		public void ShouldDefaultPopulateFromReflector()
		{
			string xml = string.Format(@"<intervalTrigger />");
			trigger = (IntervalTrigger) NetReflector.Read(xml);
            ClassicAssert.AreEqual(IntervalTrigger.DefaultIntervalSeconds, trigger.IntervalSeconds, "trigger.IntervalSeconds");
            ClassicAssert.AreEqual(IntervalTrigger.DefaultIntervalSeconds, trigger.InitialIntervalSeconds, "trigger.InitialIntervalSeconds");
            ClassicAssert.AreEqual(BuildCondition.IfModificationExists, trigger.BuildCondition, "trigger.BuildCondition");
            ClassicAssert.AreEqual("IntervalTrigger", trigger.Name, "trigger.Name");
		}

		[Test]
		public void VerifyThatShouldRequestIntegrationAfterTenSeconds()
		{
			trigger.IntervalSeconds = 10;
			trigger.BuildCondition = BuildCondition.IfModificationExists;

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 1, 1, 1, 0, 0, 0));
            ClassicAssert.AreEqual(ModificationExistRequest(), trigger.Fire(), "trigger.Fire()");
			trigger.IntegrationCompleted();

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 1, 1, 1, 0, 5, 0)); // 5 seconds later
            ClassicAssert.IsNull(trigger.Fire(), "trigger.Fire()");

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 1, 1, 1, 0, 9, 0)); // 4 seconds later

			// still before 1sec
            ClassicAssert.IsNull(trigger.Fire(), "trigger.Fire()");

			// sleep beyond the 1sec mark
			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 1, 1, 1, 0, 14, 0)); // 5 seconds later

			ClassicAssert.AreEqual(ModificationExistRequest(), trigger.Fire());
			trigger.IntegrationCompleted();
            ClassicAssert.IsNull(trigger.Fire(), "trigger.Fire()");
			VerifyAll();
		}

		[Test]
		public void VerifyThatShouldRequestInitialIntegrationAfterTwoSecondsAndSubsequentIntegrationsAfterTenSeconds()
		{
			trigger.InitialIntervalSeconds = 2;
			trigger.IntervalSeconds = 10;
			trigger.BuildCondition = BuildCondition.IfModificationExists;

			//initialDateTimeNow = new DateTime(2002, 1, 2, 3, 0, 0, 0);

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2002, 1, 2, 3, 0, 0, 0));		// now
            ClassicAssert.IsNull(trigger.Fire(), "trigger.Fire()"); ;

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2002, 1, 2, 3, 0, 1, 0));		// 1 second later
            ClassicAssert.IsNull(trigger.Fire(), "trigger.Fire()");

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2002, 1, 2, 3, 0, 2, 0));		// 2 seconds later
            ClassicAssert.AreEqual(ModificationExistRequest(), trigger.Fire(), "trigger.Fire()");
			trigger.IntegrationCompleted();

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2002, 1, 2, 3, 0, 3, 0));		// 1 second later
            ClassicAssert.IsNull(trigger.Fire(), "trigger.Fire()");

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2002, 1, 2, 3, 0, 11, 0));		// 9 seconds later

			// still before 1sec
            ClassicAssert.IsNull(trigger.Fire(), "trigger.Fire()");

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2002, 1, 2, 3, 0, 14, 0)); // 2 seconds after trigger
            ClassicAssert.AreEqual(ModificationExistRequest(), trigger.Fire(), "trigger.Fire()");
			trigger.IntegrationCompleted();
            ClassicAssert.IsNull(trigger.Fire(), "trigger.Fire()");
			VerifyAll();
		}


		[Test]
		public void ProcessTrigger()
		{
			trigger.IntervalSeconds = 0.5;
			trigger.BuildCondition = BuildCondition.IfModificationExists;

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 1, 1, 1, 0, 0, 0));
            ClassicAssert.AreEqual(ModificationExistRequest(), trigger.Fire(), "trigger.Fire()");

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 1, 1, 1, 0, 0, 550));

            ClassicAssert.AreEqual(ModificationExistRequest(), trigger.Fire(), "trigger.Fire()");
			trigger.IntegrationCompleted();
			ClassicAssert.IsNull(trigger.Fire());

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 1, 1, 1, 0, 1, 50));

            ClassicAssert.AreEqual(ModificationExistRequest(), trigger.Fire(), "trigger.Fire()");
			trigger.IntegrationCompleted();
            ClassicAssert.AreEqual(null, trigger.Fire(), "trigger.Fire()");

			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 1, 1, 1, 0, 1, 550));

            ClassicAssert.AreEqual(ModificationExistRequest(), trigger.Fire(), "trigger.Fire()");
			VerifyAll();
		}

		[Test]
		public void ShouldReturnForceBuildRequestIfSpecified()
		{
			trigger.IntervalSeconds = 10;
			trigger.BuildCondition = BuildCondition.ForceBuild;
			mockDateTime.SetupGet(provider => provider.Now).Returns(new DateTime(2004, 1, 1, 1, 0, 0, 0));
            ClassicAssert.AreEqual(ForceBuildRequest(), trigger.Fire(), "trigger.Fire()");
			VerifyAll();			
		}

		[Test]
		public void ShouldReturnInitialIntervalTimeForNextBuildOnServerStart()
		{
			trigger.InitialIntervalSeconds = 10;
			trigger.IntervalSeconds = 30;
            ClassicAssert.AreEqual(initialDateTimeNow.AddSeconds(10), trigger.NextBuild, "trigger.NextBuild");
			VerifyAll();
		}

		[Test]
		public void ShouldReturnIntervalTimeIfLastBuildJustOccured()
		{
			trigger.IntervalSeconds = 10;
			DateTime stubNow = new DateTime(2004, 1, 1, 1, 0, 0, 0);
			mockDateTime.SetupGet(provider => provider.Now).Returns(stubNow);
			trigger.IntegrationCompleted();
            ClassicAssert.AreEqual(stubNow.AddSeconds(10), trigger.NextBuild, "trigger.NextBuild");
		}

		[Test]
		public void ShouldReturnIntervalTimeIfInitialBuildJustOccured()
		{
			trigger.InitialIntervalSeconds = 10;
			trigger.IntervalSeconds = 30;
			DateTime stubNow = new DateTime(2004, 1, 1, 1, 0, 0, 0);
			mockDateTime.SetupGet(provider => provider.Now).Returns(stubNow);
			trigger.IntegrationCompleted();
            ClassicAssert.AreEqual(stubNow.AddSeconds(30), trigger.NextBuild, "trigger.NextBuild");
		}

	}
}
