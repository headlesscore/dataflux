using System;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;

namespace ThoughtWorks.CruiseControl.UnitTests.Remote
{
    /// <summary>
    /// Unit tests for CruiseControlException. 
    /// </summary>
    [TestFixture]
    public class CruiseControlExceptionTests
    {
        [Test]
        public void CreateDefault()
        {
            TestHelpers.EnsureLanguageIsValid();
            CruiseControlException exception = new CruiseControlException();
            ClassicAssert.AreEqual(string.Empty, exception.Message);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void CreateWithMessage()
        {
            TestHelpers.EnsureLanguageIsValid();
            string message = "An error has occured";
            CruiseControlException exception = new CruiseControlException(message);
            ClassicAssert.AreEqual(message, exception.Message);
        }

        [Test]
        public void CreateWithMessageAndException()
        {
            TestHelpers.EnsureLanguageIsValid();
            string message = "An error has occured";
            Exception innerException = new Exception("An inner exception");
            CruiseControlException exception = new CruiseControlException(message, innerException);
            ClassicAssert.AreEqual(message, exception.Message);
            ClassicAssert.AreEqual(innerException, exception.InnerException);
        }

        [Test]
        public void PassThroughSerialisation()
        {
            TestHelpers.EnsureLanguageIsValid();
            string message = "An error has occured";
            CruiseControlException exception = new CruiseControlException(message);
            object result = TestHelpers.RunSerialisationTest(exception);
            ClassicAssert.IsNotNull(result);
            ClassicAssert.That(result, Is.InstanceOf<CruiseControlException>());
            ClassicAssert.AreEqual(message, (result as CruiseControlException).Message);
        }
    }
}
