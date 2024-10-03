using System;
using Xunit;
using ThoughtWorks.CruiseControl.CCTrayLib;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib
{
	public class CCTimeFormatterTest
	{
		[Fact]
		public void ShouldDisplayInDDHHMMFormat()
		{
			CCTimeFormatter formatter = new CCTimeFormatter(new TimeSpan(2, 2, 6, 30));
            Assert.True("2 days 2 hours 6 minutes" == formatter.ToString());
		}

		[Fact]
		public void ShouldDisplayInDDHHMMFormatIgnoringPluralsIfNumberIsOne()
		{
			CCTimeFormatter formatter = new CCTimeFormatter(new TimeSpan(1, 1, 1, 30));
            Assert.True("1 day 1 hour 1 minute" == formatter.ToString());
		}

		[Fact]
		public void ShouldNotDisplayMinutesIfZero()
		{
			CCTimeFormatter formatter = new CCTimeFormatter(new TimeSpan(1, 1, 0, 30));
            Assert.True("1 day 1 hour" == formatter.ToString());
		}

		[Fact]
		public void ShouldDisplayInSecondsIfLessThanOneMinute()
		{
			CCTimeFormatter formatter = new CCTimeFormatter(new TimeSpan(0, 0, 0, 30));
            Assert.True("30 seconds" == formatter.ToString());
		}
	}
}
