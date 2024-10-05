using System;
using ThoughtWorks.CruiseControl.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThoughtWorks.CruiseControl.UnitTests
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
