using System;
using System.Reflection;
using Xunit;

namespace ThoughtWorks.CruiseControl.UnitTests
{
    public class GlobalSetup : IDisposable
    {
        public GlobalSetup() {
            Environment.CurrentDirectory = Assembly.GetExecutingAssembly().Location;
        }
        public void Dispose() { }
    }
}

