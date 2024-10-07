namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using ThoughtWorks.CruiseControl.Core.Util;

    [TestFixture]
    public class PrivateArgumentsTests
    {
        [Test]
        public void ConstructorWithNoArgumentsInitialises()
        {
            var args = new PrivateArguments();
            ClassicAssert.AreEqual(0, args.Count);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void ConstructorWithOneArgumentInitialises()
        {
            var args = new PrivateArguments("test");
            ClassicAssert.AreEqual(1, args.Count);
            ClassicAssert.AreEqual("test", args.ToString());
        }

        [Test]
        public void ConstructorWithTwoArgumentsInitialises()
        {
            var args = new PrivateArguments("first", "second");
            ClassicAssert.AreEqual(2, args.Count);
            ClassicAssert.AreEqual("first second", args.ToString());
        }

        [Test]
        public void ToStringGeneratesPublicString()
        {
            PrivateString hidden = "private";
            var args = new PrivateArguments("public", hidden);
            ClassicAssert.AreEqual("public " + hidden.PublicValue, args.ToString());
        }

        [Test]
        public void ToStringPublicGeneratesPublicString()
        {
            PrivateString hidden = "private";
            var args = new PrivateArguments("public", hidden);
            ClassicAssert.AreEqual("public ********", args.ToString(SecureDataMode.Public));
        }

        [Test]
        public void ToStringPrivateGeneratesPublicString()
        {
            PrivateString hidden = "private";
            var args = new PrivateArguments("public", hidden);
            ClassicAssert.AreEqual("public private", args.ToString(SecureDataMode.Private));
        }

        [Test]
        public void AddWithValueAdds()
        {
            var args = new PrivateArguments();
            args.Add("testValue");
            ClassicAssert.AreEqual(1, args.Count);
            ClassicAssert.AreEqual("testValue", args.ToString());
        }

        [Test]
        public void AddWithPrefixedValueAdds()
        {
            var args = new PrivateArguments();
            args.Add("pre=", "test Value");
            ClassicAssert.AreEqual(1, args.Count);
            ClassicAssert.AreEqual("pre=test Value", args.ToString());
        }

        [Test]
        public void AddQuoteWithValueAdds()
        {
            var args = new PrivateArguments();
            args.AddQuote("testValue");
            ClassicAssert.AreEqual(1, args.Count);
            ClassicAssert.AreEqual("\"testValue\"", args.ToString());
        }

        [Test]
        public void AddQuoteWithPrefixedValueAdds()
        {
            var args = new PrivateArguments();
            args.AddQuote("pre=", "test Value");
            ClassicAssert.AreEqual(1, args.Count);
            ClassicAssert.AreEqual("pre=\"test Value\"", args.ToString());
        }

        [Test]
        public void AddWithAutoQuoteValueAdds()
        {
            var args = new PrivateArguments();
            args.Add("pre=", "test Value", true);
            ClassicAssert.AreEqual(1, args.Count);
            ClassicAssert.AreEqual("pre=\"test Value\"", args.ToString());
        }

        [Test]
        public void AddIfWithValueAddsOnTrue()
        {
            var args = new PrivateArguments();
            args.AddIf(true, "testValue");
            ClassicAssert.AreEqual(1, args.Count);
            ClassicAssert.AreEqual("testValue", args.ToString());
        }

        [Test]
        public void AddIfWithPrefixedValueAddsOnTrue()
        {
            var args = new PrivateArguments();
            args.AddIf(true, "pre=", "test Value");
            ClassicAssert.AreEqual(1, args.Count);
            ClassicAssert.AreEqual("pre=test Value", args.ToString());
        }

        [Test]
        public void AddIfWithAutoQuoteValueAddsOnTrue()
        {
            var args = new PrivateArguments();
            args.AddIf(true, "pre=", "test Value", true);
            ClassicAssert.AreEqual(1, args.Count);
            ClassicAssert.AreEqual("pre=\"test Value\"", args.ToString());
        }

        [Test]
        public void AddIfWithValueDoesNotAddOnFalse()
        {
            var args = new PrivateArguments();
            args.AddIf(false, "testValue");
            ClassicAssert.AreEqual(0, args.Count);
            ClassicAssert.AreEqual(string.Empty, args.ToString());
        }

        [Test]
        public void AddIfWithPrefixedValueDoesNotAddOnFalse()
        {
            var args = new PrivateArguments();
            args.AddIf(false, "pre=", "test Value");
            ClassicAssert.AreEqual(0, args.Count);
            ClassicAssert.AreEqual(string.Empty, args.ToString());
        }

        [Test]
        public void AddIfWithAutoQuoteValueDoesNotAddOnFalse()
        {
            var args = new PrivateArguments();
            args.AddIf(false, "pre=", "test Value", true);
            ClassicAssert.AreEqual(0, args.Count);
            ClassicAssert.AreEqual(string.Empty, args.ToString());
        }

        [Test]
        public void ImplicitOperatorGeneratesInstance()
        {
            PrivateArguments args = "test args";
            ClassicAssert.AreEqual(1, args.Count);
            ClassicAssert.AreEqual("test args", args.ToString());
        }

        [Test]
        public void PlusOperatorAddsPublicValue()
        {
            PrivateArguments args = "test args";
            args += "value";
            ClassicAssert.AreEqual(2, args.Count);
            ClassicAssert.AreEqual("test args value", args.ToString());
        }

        [Test]
        public void PlusOperatorAddsPrivateValue()
        {
            PrivateArguments args = "test args";
            args += new PrivateString
            {
                PrivateValue = "value"
            };
            ClassicAssert.AreEqual(2, args.Count);
            ClassicAssert.AreEqual("test args ********", args.ToString());
        }
    }
}
