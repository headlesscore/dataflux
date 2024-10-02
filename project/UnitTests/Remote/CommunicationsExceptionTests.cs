namespace ThoughtWorks.CruiseControl.UnitTests.Remote
{
    using System;
    using NUnit.Framework;
    using ThoughtWorks.CruiseControl.Remote;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.IO;
    using NUnit.Framework.Legacy;

    [TestFixture]
    public class CommunicationsExceptionTests
    {
        #region Test methods
        #region Constructors()
        [Test]
        public void NewWithNoParametersSetsDefaultMessage()
        {
            CommunicationsException exception = new CommunicationsException();
            ClassicAssert.AreEqual("A communications error has occurred.", exception.Message);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void NewWithMessageSetsMessage()
        {
            CommunicationsException exception = new CommunicationsException("Message");
            ClassicAssert.AreEqual("Message", exception.Message);
        }

        [Test]
        public void FullNewSetsAllProperties()
        {
            Exception error = new Exception();
            CommunicationsException exception = new CommunicationsException("Message", error);
            ClassicAssert.AreEqual("Message", exception.Message);
            ClassicAssert.AreEqual(error, exception.InnerException);
        }

        [Test]
        public void ConstructorSetsType()
        {
            var message = "Test Message";
            var exception = new Exception();
            var type = "Some type";
            var error = new CommunicationsException(message, exception, type);
            ClassicAssert.AreEqual(message, error.Message);
            ClassicAssert.AreSame(exception, error.InnerException);
            ClassicAssert.AreEqual(type, error.ErrorType);
        }

        [Test]
        public void ExceptionCanBeSerialised()
        {
            var message = "Test Message";
            var exception = new Exception("Inner message");
            var type = "Some type";
            var original = new CommunicationsException(message, exception, type);
            var formatter = new BinaryFormatter();
            var stream = new MemoryStream();
            formatter.Serialize(stream, original);
            stream.Position = 0;  
            var error = formatter.Deserialize(stream);
            ClassicAssert.IsInstanceOf<CommunicationsException>(error);
            var deserialised = error as CommunicationsException;
            ClassicAssert.AreEqual(message, deserialised.Message);
            ClassicAssert.IsNotNull(deserialised.InnerException);
            ClassicAssert.AreEqual(exception.Message, deserialised.InnerException.Message);
            ClassicAssert.AreEqual(type, deserialised.ErrorType);
        }
        #endregion
        #endregion
    }
}
