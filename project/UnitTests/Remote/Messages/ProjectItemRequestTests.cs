namespace ThoughtWorks.CruiseControl.UnitTests.Remote.Messages
{
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using ThoughtWorks.CruiseControl.Remote.Messages;

    [TestFixture]
    public class ProjectItemRequestTests
    {
        #region Tests
        #region Constructor tests
        [Test]
        public void SessionConstructorInitialisesTheValues()
        {
            var sessionId = "MyNewSession";
            var request = new ProjectItemRequest(sessionId);
            ClassicAssert.AreEqual(sessionId, request.SessionToken);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void FullConstructorInitialisesTheValues()
        {
            var sessionId = "MyNewSession";
            var projectName = "TheNameOfTheProject";
            var request = new ProjectItemRequest(sessionId, projectName);
            ClassicAssert.AreEqual(sessionId, request.SessionToken);
            ClassicAssert.AreEqual(projectName, request.ProjectName);
        }
        #endregion
        #endregion
    }
}
