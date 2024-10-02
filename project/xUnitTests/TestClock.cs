using System;
using ThoughtWorks.CruiseControl.Core;

namespace ThoughtWorks.CruiseControl.xUnitTests
{
    public class TestClock : IClock
    {
        public DateTime Now { get; set; }

        public void TimePasses(TimeSpan timeSpan)
        {
            Now = Now.Add(timeSpan);
        }
    }
}
