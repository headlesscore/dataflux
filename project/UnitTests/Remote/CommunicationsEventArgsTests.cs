namespace ThoughtWorks.CruiseControl.UnitTests.Remote
{
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using ThoughtWorks.CruiseControl.Remote;
    using ThoughtWorks.CruiseControl.Remote.Messages;

    [TestFixture]
    public class CommunicationsEventArgsTests
    {
        #region Tests
        [Test]
        public void ConstructorSetsProperties()
        {
            var message = new Response();
            var args = new CommunicationsEventArgs("action", message);
            ClassicAssert.AreEqual("action", args.Action);
            ClassicAssert.AreSame(message, args.Message);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }
        #endregion
    }
}
