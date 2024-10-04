using Xunit;

using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
	
	public class TimeoutTest
	{
		[Fact]
		public void DefaultTimeoutIsInMillis()
		{
			Timeout timeout = new Timeout(100);
			Assert.Equal(100, timeout.Millis);
			Assert.Equal(new Timeout(100, TimeUnits.MILLIS), timeout);
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public void CanSpecifyTimeoutInSeconds()
		{
			Timeout period = new Timeout(1, TimeUnits.SECONDS);
			Assert.Equal(1000, period.Millis);
			Assert.Equal(new Timeout(1000, TimeUnits.MILLIS), period);
		}

		[Fact]
		public void CanSpecifyTimeoutInMinutes()
		{
			Timeout period = new Timeout(1, TimeUnits.MINUTES);
			Assert.Equal(60*1000, period.Millis);
			Assert.Equal(new Timeout(60*1000, TimeUnits.MILLIS), period);
		}

		[Fact]
		public void CanSpecifyTimeoutInHours()
		{
			Timeout period = new Timeout(1, TimeUnits.HOURS);
			Assert.Equal(60*60*1000, period.Millis);
			Assert.Equal(new Timeout(60*60*1000, TimeUnits.MILLIS), period);
		}
	}
}
