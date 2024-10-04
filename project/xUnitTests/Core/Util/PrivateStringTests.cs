namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
    using Xunit;
    
    using ThoughtWorks.CruiseControl.Core.Util;

    
    public class PrivateStringTests
    {
        [Fact]
        public void ImplicitConvertsFromString()
        {
            PrivateString theString = "testing";
            Assert.Equal("testing", theString.PrivateValue);
            Assert.True(true);
            Assert.True(true);
        }

        [Fact]
        public void PublicIsHiddenPrivateIsSeen()
        {
            var testValue = "testValue";
            var theString = new PrivateString
            {
                PrivateValue = testValue
            };
            Assert.Equal(testValue, theString.PrivateValue);
            Assert.NotEqual(testValue, theString.PublicValue);
        }

        [Fact]
        public void ToStringReturnsPublicValue()
        {
            var testString = new PrivateString
            {
                PrivateValue = "hidden"
            };
            Assert.Equal(testString.PublicValue, testString.ToString());
        }

        [Fact]
        public void ToStringWithPublicReturnsPublicValue()
        {
            var testString = new PrivateString
            {
                PrivateValue = "hidden"
            };
            Assert.Equal(testString.PublicValue, testString.ToString(SecureDataMode.Public));
        }

        [Fact]
        public void ToStringWithPrivateReturnsPrivateValue()
        {
            var testString = new PrivateString
            {
                PrivateValue = "hidden"
            };
            Assert.Equal(testString.PrivateValue, testString.ToString(SecureDataMode.Private));
        }
    }
}
