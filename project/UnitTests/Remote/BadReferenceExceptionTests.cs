using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using ThoughtWorks.CruiseControl.Core;

namespace ThoughtWorks.CruiseControl.UnitTests.Remote
{
    /// <summary>
    /// Unit tests for BadReferenceException. 
    /// </summary>
    [TestFixture]
    public class BadReferenceExceptionTests
    {
        [Test]
        public void CreateWithReference()
        {
            TestHelpers.EnsureLanguageIsValid();
            string reference = "Something";
            BadReferenceException exception = new BadReferenceException(reference);
            ClassicAssert.AreEqual("Reference 'Something' is either incorrect or missing.", exception.Message);
            ClassicAssert.AreEqual(reference, exception.Reference);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void CreateWithReferenceAndMessage()
        {
            TestHelpers.EnsureLanguageIsValid();
            string reference = "Something";
            string message = "An error has occured";
            BadReferenceException exception = new BadReferenceException(reference, message);
            ClassicAssert.AreEqual(message, exception.Message);
            ClassicAssert.AreEqual(reference, exception.Reference);
        }

        [Test]
        public void CreateWithReferenceMessageAndException()
        {
            TestHelpers.EnsureLanguageIsValid();
            string reference = "Something";
            string message = "An error has occured";
            Exception innerException = new Exception("An inner exception");
            BadReferenceException exception = new BadReferenceException(reference, message, innerException);
            ClassicAssert.AreEqual(message, exception.Message);
            ClassicAssert.AreEqual(reference, exception.Reference);
            ClassicAssert.AreEqual(innerException, exception.InnerException);
        }

        [Test]
        public void PassThroughSerialisation()
        {
            TestHelpers.EnsureLanguageIsValid();
            string reference = "Something";
            BadReferenceException exception = new BadReferenceException(reference);
            object result = TestHelpers.RunSerialisationTest(exception);
            ClassicAssert.IsNotNull(result);
            ClassicAssert.IsInstanceOf<BadReferenceException>(result);
            ClassicAssert.AreEqual("Reference 'Something' is either incorrect or missing.", (result as BadReferenceException).Message);
            ClassicAssert.AreEqual(reference, (result as BadReferenceException).Reference);
        }
    }
}
