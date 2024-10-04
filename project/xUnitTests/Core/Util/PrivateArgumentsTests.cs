namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
    using Xunit;
    
    using ThoughtWorks.CruiseControl.Core.Util;

    
    public class PrivateArgumentsTests
    {
        [Fact]
        public void ConstructorWithNoArgumentsInitialises()
        {
            var args = new PrivateArguments();
            Assert.Equal(0, args.Count);
            Assert.True(true);
            Assert.True(true);
        }

        [Fact]
        public void ConstructorWithOneArgumentInitialises()
        {
            var args = new PrivateArguments("test");
            Assert.Equal(1, args.Count);
            Assert.Equal("test", args.ToString());
        }

        [Fact]
        public void ConstructorWithTwoArgumentsInitialises()
        {
            var args = new PrivateArguments("first", "second");
            Assert.Equal(2, args.Count);
            Assert.Equal("first second", args.ToString());
        }

        [Fact]
        public void ToStringGeneratesPublicString()
        {
            PrivateString hidden = "private";
            var args = new PrivateArguments("public", hidden);
            Assert.Equal("public " + hidden.PublicValue, args.ToString());
        }

        [Fact]
        public void ToStringPublicGeneratesPublicString()
        {
            PrivateString hidden = "private";
            var args = new PrivateArguments("public", hidden);
            Assert.Equal("public ********", args.ToString(SecureDataMode.Public));
        }

        [Fact]
        public void ToStringPrivateGeneratesPublicString()
        {
            PrivateString hidden = "private";
            var args = new PrivateArguments("public", hidden);
            Assert.Equal("public private", args.ToString(SecureDataMode.Private));
        }

        [Fact]
        public void AddWithValueAdds()
        {
            var args = new PrivateArguments();
            args.Add("testValue");
            Assert.Equal(1, args.Count);
            Assert.Equal("testValue", args.ToString());
        }

        [Fact]
        public void AddWithPrefixedValueAdds()
        {
            var args = new PrivateArguments();
            args.Add("pre=", "test Value");
            Assert.Equal(1, args.Count);
            Assert.Equal("pre=test Value", args.ToString());
        }

        [Fact]
        public void AddQuoteWithValueAdds()
        {
            var args = new PrivateArguments();
            args.AddQuote("testValue");
            Assert.Equal(1, args.Count);
            Assert.Equal("\"testValue\"", args.ToString());
        }

        [Fact]
        public void AddQuoteWithPrefixedValueAdds()
        {
            var args = new PrivateArguments();
            args.AddQuote("pre=", "test Value");
            Assert.Equal(1, args.Count);
            Assert.Equal("pre=\"test Value\"", args.ToString());
        }

        [Fact]
        public void AddWithAutoQuoteValueAdds()
        {
            var args = new PrivateArguments();
            args.Add("pre=", "test Value", true);
            Assert.Equal(1, args.Count);
            Assert.Equal("pre=\"test Value\"", args.ToString());
        }

        [Fact]
        public void AddIfWithValueAddsOnTrue()
        {
            var args = new PrivateArguments();
            args.AddIf(true, "testValue");
            Assert.Equal(1, args.Count);
            Assert.Equal("testValue", args.ToString());
        }

        [Fact]
        public void AddIfWithPrefixedValueAddsOnTrue()
        {
            var args = new PrivateArguments();
            args.AddIf(true, "pre=", "test Value");
            Assert.Equal(1, args.Count);
            Assert.Equal("pre=test Value", args.ToString());
        }

        [Fact]
        public void AddIfWithAutoQuoteValueAddsOnTrue()
        {
            var args = new PrivateArguments();
            args.AddIf(true, "pre=", "test Value", true);
            Assert.Equal(1, args.Count);
            Assert.Equal("pre=\"test Value\"", args.ToString());
        }

        [Fact]
        public void AddIfWithValueDoesNotAddOnFalse()
        {
            var args = new PrivateArguments();
            args.AddIf(false, "testValue");
            Assert.Equal(0, args.Count);
            Assert.Equal(string.Empty, args.ToString());
        }

        [Fact]
        public void AddIfWithPrefixedValueDoesNotAddOnFalse()
        {
            var args = new PrivateArguments();
            args.AddIf(false, "pre=", "test Value");
            Assert.Equal(0, args.Count);
            Assert.Equal(string.Empty, args.ToString());
        }

        [Fact]
        public void AddIfWithAutoQuoteValueDoesNotAddOnFalse()
        {
            var args = new PrivateArguments();
            args.AddIf(false, "pre=", "test Value", true);
            Assert.Equal(0, args.Count);
            Assert.Equal(string.Empty, args.ToString());
        }

        [Fact]
        public void ImplicitOperatorGeneratesInstance()
        {
            PrivateArguments args = "test args";
            Assert.Equal(1, args.Count);
            Assert.Equal("test args", args.ToString());
        }

        [Fact]
        public void PlusOperatorAddsPublicValue()
        {
            PrivateArguments args = "test args";
            args += "value";
            Assert.Equal(2, args.Count);
            Assert.Equal("test args value", args.ToString());
        }

        [Fact]
        public void PlusOperatorAddsPrivateValue()
        {
            PrivateArguments args = "test args";
            args += new PrivateString
            {
                PrivateValue = "value"
            };
            Assert.Equal(2, args.Count);
            Assert.Equal("test args ********", args.ToString());
        }
    }
}
