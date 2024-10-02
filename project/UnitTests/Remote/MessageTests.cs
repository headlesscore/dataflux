using System;
using System.Collections.Generic;
using System.Text;
using ThoughtWorks.CruiseControl.Remote;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace ThoughtWorks.CruiseControl.UnitTests.Remote
{
    [TestFixture]
    public class MessageTests
    {
        #region Test methods
        [Test]
        public void StartANewBlankMessage()
        {
            Message value = new Message();
            ClassicAssert.IsNull(value.Text);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void StartANewMessageWithText()
        {
            string expected = "Testing";
            Message value = new Message(expected);
            ClassicAssert.AreEqual(expected, value.Text);
        }

        [Test]
        public void TextPropertyCanBeSet()
        {
            string expected = "Testing";
            Message value = new Message();
            ClassicAssert.IsNull(value.Text);
            value.Text = expected;
            ClassicAssert.AreEqual(expected, value.Text);
        }

        [Test]
        public void ToStringReturnsMessage()
        {
            string expected = "Testing";
            Message value = new Message(expected);
            ClassicAssert.AreEqual(expected, value.ToString());
        }

        [Test]
        public void GetHashCodeReturnsStringHashCode()
        {
            var msg = new Message("A message");
            var hashCode = msg.GetHashCode();
            ClassicAssert.AreEqual(msg.ToString().GetHashCode(), msg.GetHashCode());
        }

        [Test]
        public void GetHashCodeWithNullMessage()
        {
            var msg = new Message(null, Message.MessageKind.BuildAbortedBy);
            ClassicAssert.AreEqual(msg.ToString().GetHashCode(), msg.GetHashCode());
        }

        [Test]
        public void EqualsReturnsTrueWhenBothMessageAndKindAreSame()
        {
            var msg1 = new Message("The message", Message.MessageKind.NotDefined);
            var msg2 = new Message("The message", Message.MessageKind.NotDefined);
            ClassicAssert.IsTrue(msg1.Equals(msg2));
        }

        [Test]
        public void EqualsReturnsFalseIfMessageIsDifferent()
        {
            var msg1 = new Message("The message1", Message.MessageKind.NotDefined);
            var msg2 = new Message("The message2", Message.MessageKind.NotDefined);
            ClassicAssert.IsFalse(msg1.Equals(msg2));
        }

        [Test]
        public void EqualsReturnsFalseIfTypeIsDifferent()
        {
            var msg1 = new Message("The message", Message.MessageKind.NotDefined);
            var msg2 = new Message("The message", Message.MessageKind.Fixer);
            ClassicAssert.IsFalse(msg1.Equals(msg2));
        }

        [Test]
        public void EqualsReturnsFalseIfArgumentIsNotAMessage()
        {
            var msg1 = new Message("The message", Message.MessageKind.NotDefined);
            var msg2 = "A message";
            ClassicAssert.IsFalse(msg1.Equals(msg2));
        }
        #endregion
    }
}
