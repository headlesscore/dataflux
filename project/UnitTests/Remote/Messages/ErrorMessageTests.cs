namespace ThoughtWorks.CruiseControl.UnitTests.Remote.Messages
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using ThoughtWorks.CruiseControl.Remote.Messages;

    [TestFixture]
    public class ErrorMessageTests
    {
        #region Tests
        #region Constructor tests
        [Test]
        public void MessageConstructorInitialisesTheValues()
        {
            var message = "MyNewSession";
            var request = new ErrorMessage(message);
            ClassicAssert.AreEqual(message, request.Message);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void FullConstructorInitialisesTheValues()
        {
            var message = "MyNewSession";
            var type = "TheErrorType";
            var request = new ErrorMessage(message, type);
            ClassicAssert.AreEqual(message, request.Message);
            ClassicAssert.AreEqual(type, request.Type);
        }
        #endregion

        #region Getter/setter tests
        [Test]
        public void TypeCanBeSetAndRetrieved()
        {
            var request = new ErrorMessage();
            var type = "TheErrorType";
            request.Type = type;
            ClassicAssert.AreEqual(type, request.Type);
        }
        #endregion
        #endregion
    }
}
