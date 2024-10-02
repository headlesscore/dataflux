using System;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;

namespace ThoughtWorks.CruiseControl.UnitTests.Remote
{
    /// <summary>
    /// Unit tests for PermissionDeniedException. 
    /// </summary>
    [TestFixture]
    public class PermissionDeniedExceptionTests
    {
        [Test]
        public void CreateWithpermission()
        {
            TestHelpers.EnsureLanguageIsValid();
            string permission = "Something";
            PermissionDeniedException exception = new PermissionDeniedException(permission);
            ClassicAssert.AreEqual("Permission to execute 'Something' has been denied.", exception.Message);
            ClassicAssert.AreEqual(permission, exception.Permission);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void CreateWithpermissionAndMessage()
        {
            TestHelpers.EnsureLanguageIsValid();
            string permission = "Something";
            string message = "An error has occured";
            PermissionDeniedException exception = new PermissionDeniedException(permission, message);
            ClassicAssert.AreEqual(message, exception.Message);
            ClassicAssert.AreEqual(permission, exception.Permission);
        }

        [Test]
        public void CreateWithpermissionMessageAndException()
        {
            TestHelpers.EnsureLanguageIsValid();
            string permission = "Something";
            string message = "An error has occured";
            Exception innerException = new Exception("An inner exception");
            PermissionDeniedException exception = new PermissionDeniedException(permission, message, innerException);
            ClassicAssert.AreEqual(message, exception.Message);
            ClassicAssert.AreEqual(permission, exception.Permission);
            ClassicAssert.AreEqual(innerException, exception.InnerException);
        }

        [Test]
        public void PassThroughSerialisation()
        {
            TestHelpers.EnsureLanguageIsValid();
            string permission = "Something";
            PermissionDeniedException exception = new PermissionDeniedException(permission);
            object result = TestHelpers.RunSerialisationTest(exception);
            ClassicAssert.IsNotNull(result);
            ClassicAssert.That(result, Is.InstanceOf<PermissionDeniedException>());
            ClassicAssert.AreEqual("Permission to execute 'Something' has been denied.", exception.Message);
            ClassicAssert.AreEqual(permission, (result as PermissionDeniedException).Permission);
        }
    }
}
