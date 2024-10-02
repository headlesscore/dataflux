namespace ThoughtWorks.CruiseControl.UnitTests.Remote.Messages
{
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using ThoughtWorks.CruiseControl.Remote.Messages;

    [TestFixture]
    public class EncryptedRequestTests
    {
        #region Tests
        #region Constructor tests
        [Test]
        public void SessionConstructorInitialisesTheValues()
        {
            var sessionId = "MyNewSession";
            var request = new EncryptedRequest(sessionId);
            ClassicAssert.AreEqual(sessionId, request.SessionToken);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void FullConstructorInitialisesTheValues()
        {
            var sessionId = "MyNewSession";
            var data = "SomeEncryptedData";
            var request = new EncryptedRequest(sessionId, data);
            ClassicAssert.AreEqual(sessionId, request.SessionToken);
            ClassicAssert.AreEqual(data, request.EncryptedData);
        }
        #endregion

        #region Getter/setter tests
        [Test]
        public void EncryptedDataCanBeSetAndRetrieved()
        {
            var request = new EncryptedRequest();
            var data = "SomeEncryptedData";
            request.EncryptedData = data;
            ClassicAssert.AreEqual(data, request.EncryptedData);
        }
        #endregion
        #endregion
    }
}
