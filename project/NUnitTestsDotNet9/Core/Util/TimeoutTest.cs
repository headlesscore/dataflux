using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Util;
using Timeout = ThoughtWorks.CruiseControl.Core.Util.Timeout;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
	[TestFixture]
	public class TimeoutTest
	{
		[Test]
		public void DefaultTimeoutIsInMillis()
		{
			Timeout timeout = new Timeout(100);
			ClassicAssert.AreEqual(100, timeout.Millis);
			ClassicAssert.AreEqual(new Timeout(100, TimeUnits.MILLIS), timeout);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

		[Test]
		public void CanSpecifyTimeoutInSeconds()
		{
			Timeout period = new Timeout(1, TimeUnits.SECONDS);
			ClassicAssert.AreEqual(1000, period.Millis);
			ClassicAssert.AreEqual(new Timeout(1000, TimeUnits.MILLIS), period);
		}

		[Test]
		public void CanSpecifyTimeoutInMinutes()
		{
			Timeout period = new Timeout(1, TimeUnits.MINUTES);
			ClassicAssert.AreEqual(60*1000, period.Millis);
			ClassicAssert.AreEqual(new Timeout(60*1000, TimeUnits.MILLIS), period);
		}

		[Test]
		public void CanSpecifyTimeoutInHours()
		{
			Timeout period = new Timeout(1, TimeUnits.HOURS);
			ClassicAssert.AreEqual(60*60*1000, period.Millis);
			ClassicAssert.AreEqual(new Timeout(60*60*1000, TimeUnits.MILLIS), period);
		}
	}
}
