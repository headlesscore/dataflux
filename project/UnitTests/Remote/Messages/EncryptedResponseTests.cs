namespace ThoughtWorks.CruiseControl.UnitTests.Remote.Messages
{
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using ThoughtWorks.CruiseControl.Remote.Messages;

    [TestFixture]
    public class EncryptedResponseTests
    {
        #region Tests
        #region Constructor tests
        [Test]
        public void RequestConstructorInitialisesTheValues()
        {
            var request = new EncryptedRequest();
            var response = new EncryptedResponse(request);
            // Only check one property is set, since the properties are set by the base class
            ClassicAssert.AreEqual(request.Identifier, response.RequestIdentifier);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void FullConstructorInitialisesTheValues()
        {
            var response1 = new EncryptedResponse();
            response1.RequestIdentifier = "12345";
            var response2 = new EncryptedResponse(response1);
            // Only check one property is set, since the properties are set by the base class
            ClassicAssert.AreEqual(response1.RequestIdentifier, response2.RequestIdentifier);
        }
        #endregion

        #region Getter/setter tests
        [Test]
        public void EncryptedDataCanBeSetAndRetrieved()
        {
            var request = new EncryptedResponse();
            var data = "SomeEncryptedData";
            request.EncryptedData = data;
            ClassicAssert.AreEqual(data, request.EncryptedData);
        }
        #endregion
        #endregion
    }
}
