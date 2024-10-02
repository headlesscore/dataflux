namespace ThoughtWorks.CruiseControl.UnitTests.Remote.Messages
{
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using ThoughtWorks.CruiseControl.Remote.Messages;

    [TestFixture]
    public class FileTransferRequestTests
    {
        #region Tests
        #region Constructor tests
        [Test]
        public void SessionConstructorInitialisesTheValues()
        {
            var sessionId = "MyNewSession";
            var request = new FileTransferRequest(sessionId);
            ClassicAssert.AreEqual(sessionId, request.SessionToken);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void FullConstructorInitialisesTheValues()
        {
            var sessionId = "MyNewSession";
            var projectName = "projectName";
            var fileName = "fileName";
            var request = new FileTransferRequest(sessionId, projectName, fileName);
            ClassicAssert.AreEqual(sessionId, request.SessionToken);
            ClassicAssert.AreEqual(projectName, request.ProjectName);
            ClassicAssert.AreEqual(fileName, request.FileName);
        }
        #endregion

        #region Getter/setter tests
        [Test]
        public void FileNameCanBeSetAndRetrieved()
        {
            var request = new FileTransferRequest();
            var projectName = "projectName";
            request.FileName = projectName;
            ClassicAssert.AreEqual(projectName, request.FileName);
        }
        #endregion
        #endregion
    }
}
