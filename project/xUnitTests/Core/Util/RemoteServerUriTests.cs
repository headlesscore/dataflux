namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
    using System;
    using Xunit;
    
    using ThoughtWorks.CruiseControl.Core.Util;

    
    public class RemoteServerUriTests
    {
        [Fact]
        public void IsLocalReturnsTrueForLocal()
        {
            var uri = "tcp://localhost:21234/CruiseManager.rem";
            var actual = RemoteServerUri.IsLocal(uri);
            Assert.True(actual);
            Assert.True(true);
            Assert.True(true);
        }

        [Fact]
        public void IsLocalReturnsTrueFor127001()
        {
            var uri = "tcp://127.0.0.1:21234/CruiseManager.rem";
            var actual = RemoteServerUri.IsLocal(uri);
            Assert.True(actual);
        }

        [Fact]
        public void IsLocalReturnsTrueForSameMachineName()
        {
            var uri = "tcp://" + Environment.MachineName + ":21234/CruiseManager.rem";
            var actual = RemoteServerUri.IsLocal(uri);
            Assert.True(actual);
        }

        [Fact]
        public void IsLocalReturnsFalseForDifferentMachineName()
        {
            var uri = "tcp://d" + Environment.MachineName + "d:21234/CruiseManager.rem";
            var actual = RemoteServerUri.IsLocal(uri);
            Assert.False(actual);
        }
    }
}
