namespace ThoughtWorks.CruiseControl.UnitTests.Remote.Monitor
{
    using System;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using ThoughtWorks.CruiseControl.Remote.Monitor;

    public class ServerUpdateArgsTests
    {
        #region Tests
        [Test]
        public void ConstructorSetsException()
        {
            var exception = new Exception();
            var args = new ServerUpdateArgs(exception);
            ClassicAssert.AreSame(exception, args.Exception);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }
        #endregion
    }
}
