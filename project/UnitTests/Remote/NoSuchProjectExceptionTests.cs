using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Remote
{
    /// <summary>
    /// Unit tests for NoSuchProjectException. 
    /// </summary>
    [TestFixture]
    public class NoSuchProjectExceptionTests
    {
        [Test]
        public void CreateDefault()
        {
            TestHelpers.EnsureLanguageIsValid();
            NoSuchProjectException exception = new NoSuchProjectException();
            ClassicAssert.AreEqual("The project '' does not exist on the CCNet server.", exception.Message);
            ClassicAssert.IsNull(exception.RequestedProject);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void CreateWithRequestedProject()
        {
            TestHelpers.EnsureLanguageIsValid();
            string requestedProject = "Something";
            NoSuchProjectException exception = new NoSuchProjectException(requestedProject);
            ClassicAssert.AreEqual("The project 'Something' does not exist on the CCNet server.", exception.Message);
            ClassicAssert.AreEqual(requestedProject, exception.RequestedProject);
        }

        [Test]
        public void CreateWithRequestedProjectAndException()
        {
            TestHelpers.EnsureLanguageIsValid();
            string requestedProject = "Something";
            Exception innerException = new Exception("An inner exception");
            NoSuchProjectException exception = new NoSuchProjectException(requestedProject, innerException);
            ClassicAssert.AreEqual("The project 'Something' does not exist on the CCNet server.", exception.Message);
            ClassicAssert.AreEqual(requestedProject, exception.RequestedProject);
            ClassicAssert.AreEqual(innerException, exception.InnerException);
        }

        [Test]
        public void PassThroughSerialisation()
        {
            TestHelpers.EnsureLanguageIsValid();
            string requestedProject = "Something";
            NoSuchProjectException exception = new NoSuchProjectException(requestedProject);
            object result = TestHelpers.RunSerialisationTest(exception);
            ClassicAssert.IsNotNull(result);
            ClassicAssert.That(result, Is.InstanceOf<NoSuchProjectException>());
            ClassicAssert.AreEqual("The project 'Something' does not exist on the CCNet server.", (result as NoSuchProjectException).Message);
            ClassicAssert.AreEqual(requestedProject, (result as NoSuchProjectException).RequestedProject);
        }
    }
}
