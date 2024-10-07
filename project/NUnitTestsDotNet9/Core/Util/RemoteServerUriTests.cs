namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
    using System;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using ThoughtWorks.CruiseControl.Core.Util;

    [TestFixture]
    public class RemoteServerUriTests
    {
        [Test]
        public void IsLocalReturnsTrueForLocal()
        {
            var uri = "tcp://localhost:21234/CruiseManager.rem";
            var actual = RemoteServerUri.IsLocal(uri);
            ClassicAssert.IsTrue(actual);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void IsLocalReturnsTrueFor127001()
        {
            var uri = "tcp://127.0.0.1:21234/CruiseManager.rem";
            var actual = RemoteServerUri.IsLocal(uri);
            ClassicAssert.IsTrue(actual);
        }

        [Test]
        public void IsLocalReturnsTrueForSameMachineName()
        {
            var uri = "tcp://" + Environment.MachineName + ":21234/CruiseManager.rem";
            var actual = RemoteServerUri.IsLocal(uri);
            ClassicAssert.IsTrue(actual);
        }

        [Test]
        public void IsLocalReturnsFalseForDifferentMachineName()
        {
            var uri = "tcp://d" + Environment.MachineName + "d:21234/CruiseManager.rem";
            var actual = RemoteServerUri.IsLocal(uri);
            ClassicAssert.IsFalse(actual);
        }
    }
}
