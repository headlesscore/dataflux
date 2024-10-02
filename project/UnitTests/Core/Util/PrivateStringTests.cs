namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using ThoughtWorks.CruiseControl.Core.Util;

    [TestFixture]
    public class PrivateStringTests
    {
        [Test]
        public void ImplicitConvertsFromString()
        {
            PrivateString theString = "testing";
            ClassicAssert.AreEqual("testing", theString.PrivateValue);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void PublicIsHiddenPrivateIsSeen()
        {
            var testValue = "testValue";
            var theString = new PrivateString
            {
                PrivateValue = testValue
            };
            ClassicAssert.AreEqual(testValue, theString.PrivateValue);
            ClassicAssert.AreNotEqual(testValue, theString.PublicValue);
        }

        [Test]
        public void ToStringReturnsPublicValue()
        {
            var testString = new PrivateString
            {
                PrivateValue = "hidden"
            };
            ClassicAssert.AreEqual(testString.PublicValue, testString.ToString());
        }

        [Test]
        public void ToStringWithPublicReturnsPublicValue()
        {
            var testString = new PrivateString
            {
                PrivateValue = "hidden"
            };
            ClassicAssert.AreEqual(testString.PublicValue, testString.ToString(SecureDataMode.Public));
        }

        [Test]
        public void ToStringWithPrivateReturnsPrivateValue()
        {
            var testString = new PrivateString
            {
                PrivateValue = "hidden"
            };
            ClassicAssert.AreEqual(testString.PrivateValue, testString.ToString(SecureDataMode.Private));
        }
    }
}
