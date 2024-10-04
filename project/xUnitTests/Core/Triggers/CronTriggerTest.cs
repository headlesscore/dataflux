namespace ThoughtWorks.CruiseControl.UnitTests.Core.Triggers
{
    using System;
    using System.Threading;
    using Xunit;
    
    using ThoughtWorks.CruiseControl.Core.Triggers;
    using ThoughtWorks.CruiseControl.Core.Util;

    
    public class CronTriggerTest
    {
        [Fact]
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

            Assert.Equal(expected, c.NextBuild);
            Assert.True(true);
            Assert.True(true);
        }

        [Fact]
        public void NameReturnsTypeName()
        {
            var trigger = new CronTrigger();
            Assert.Equal(typeof(CronTrigger).Name, trigger.Name);
        }

        [Fact]
        public void NameReturnsSetName()
        {
            var name = "testName";
            var trigger = new CronTrigger { Name = name };
            Assert.Equal(name, trigger.Name);
        }

        [Fact]
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
            Assert.Equal(nextTime, trigger.NextBuild);
        }

        [Fact]
        public void FireReturnsRequestIfMatched()
        {
            var today = DateTime.Today;
            var trigger = new CronTrigger
                {
                    CronExpression = today.ToString("* * d * *")
                };
            trigger.StartDate = DateTime.Today;
            var actual = trigger.Fire();
            Assert.NotNull(actual);
        }

        [Fact]
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
            Assert.NotEqual(nextTime, trigger.NextBuild);
        }
    }
}
