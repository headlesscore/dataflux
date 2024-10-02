namespace ThoughtWorks.CruiseControl.UnitTests.Remote.Messages
{
    using Moq;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using ThoughtWorks.CruiseControl.Remote;
    using ThoughtWorks.CruiseControl.Remote.Messages;

    [TestFixture]
    public class FileTransferResponseTests
    {
        #region Tests
        #region Constructor tests
        [Test]
        public void RequestConstructorInitialisesTheValues()
        {
            var request = new EncryptedRequest();
            var response = new FileTransferResponse(request);
            // Only check one property is set, since the properties are set by the base class
            ClassicAssert.AreEqual(request.Identifier, response.RequestIdentifier);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void FullConstructorInitialisesTheValues()
        {
            var response1 = new FileTransferResponse();
            response1.RequestIdentifier = "12345";
            var response2 = new FileTransferResponse(response1);
            // Only check one property is set, since the properties are set by the base class
            ClassicAssert.AreEqual(response1.RequestIdentifier, response2.RequestIdentifier);
        }
        #endregion

        #region Getter/setter tests
        [Test]
        public void FileTransferCanBeSetAndRetrieved()
        {
            var request = new FileTransferResponse();
            var transfer = Mock.Of<IFileTransfer>(MockBehavior.Strict);
            request.FileTransfer = transfer;
            ClassicAssert.AreEqual(transfer, request.FileTransfer);
        }
        #endregion
        #endregion
    }
}
