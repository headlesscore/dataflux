﻿namespace ThoughtWorks.CruiseControl.UnitTests.Core.Triggers
{
    using System;
    using System.Threading;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using ThoughtWorks.CruiseControl.Core.Triggers;
    using ThoughtWorks.CruiseControl.Core.Util;

    [TestFixture]
    public class CronTriggerTest
    {
        [Test]
        public void TestX()
        {
            var c = new CronTrigger();

            c.CronExpression = "* * 1 1 *"; // first januari of each year

            c.Fire();

            DateTime expected;
            if (DateTime.Now.DayOfYear == 1)
            {
                expected = new DateTime(DateTime.Now.Year, 1, 1);
            }
            else
            {
                expected = new DateTime(DateTime.Now.Year + 1, 1, 1);
            }

            ClassicAssert.AreEqual(expected, c.NextBuild);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void NameReturnsTypeName()
        {
            var trigger = new CronTrigger();
            ClassicAssert.AreEqual(typeof(CronTrigger).Name, trigger.Name);
        }

        [Test]
        public void NameReturnsSetName()
        {
            var name = "testName";
            var trigger = new CronTrigger { Name = name };
            ClassicAssert.AreEqual(name, trigger.Name);
        }

        [Test]
        public void IntegrationCompletedDoesNothingIfNotTriggered()
        {
            var trigger = new CronTrigger
                {
                    CronExpression = "* * 1 1 *"
                };
            trigger.Fire();

            var nextTime = trigger.NextBuild;
            trigger.StartDate = DateTime.Now.AddHours(2);
            trigger.IntegrationCompleted();
            ClassicAssert.AreEqual(nextTime, trigger.NextBuild);
        }

        [Test]
        public void FireReturnsRequestIfMatched()
        {
            var today = DateTime.Today;
            var trigger = new CronTrigger
                {
                    CronExpression = today.ToString("* * d * *")
                };
            trigger.StartDate = DateTime.Today;
            var actual = trigger.Fire();
            ClassicAssert.IsNotNull(actual);
        }

        [Test]
        public void IntegrationCompletedUpdatesNextBuildIfTriggered()
        {
            var today = DateTime.Today;
            var trigger = new CronTrigger
                {
                    CronExpression = today.ToString("* * d * *")
                };
            trigger.StartDate = DateTime.Today;
            trigger.Fire();
            var nextTime = trigger.NextBuild;
            trigger.StartDate = DateTime.Now.AddHours(2);
            trigger.Fire();
            trigger.IntegrationCompleted();
            ClassicAssert.AreNotEqual(nextTime, trigger.NextBuild);
        }
    }
}
