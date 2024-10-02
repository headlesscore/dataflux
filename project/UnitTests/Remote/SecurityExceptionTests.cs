using System;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;

namespace ThoughtWorks.CruiseControl.UnitTests.Remote
{
    /// <summary>
    /// Unit tests for SecurityException. 
    /// </summary>
    [TestFixture]
    public class SecurityExceptionTests
    {
        [Test]
        public void CreateDefault()
        {
            TestHelpers.EnsureLanguageIsValid();
            SecurityException exception = new SecurityException();
            ClassicAssert.AreEqual("A security failure has occurred.", exception.Message);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void CreateWithMessage()
        {
            TestHelpers.EnsureLanguageIsValid();
            string message = "An error has occured";
            SecurityException exception = new SecurityException(message);
            ClassicAssert.AreEqual(message, exception.Message);
        }

        [Test]
        public void CreateWithMessageAndException()
        {
            TestHelpers.EnsureLanguageIsValid();
            string message = "An error has occured";
            Exception innerException = new Exception("An inner exception");
            SecurityException exception = new SecurityException(message, innerException);
            ClassicAssert.AreEqual(message, exception.Message);
            ClassicAssert.AreEqual(innerException, exception.InnerException);
        }

        [Test]
        public void PassThroughSerialisation()
        {
            TestHelpers.EnsureLanguageIsValid();
            SecurityException exception = new SecurityException();
            object result = TestHelpers.RunSerialisationTest(exception);
            ClassicAssert.IsNotNull(result);
            ClassicAssert.That(result, Is.InstanceOf<SecurityException>());
            ClassicAssert.AreEqual("A security failure has occurred.", (result as SecurityException).Message);
        }
    }
}
