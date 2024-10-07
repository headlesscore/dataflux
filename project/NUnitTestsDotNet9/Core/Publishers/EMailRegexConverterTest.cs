using Exortech.NetReflector;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Publishers;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Publishers
{
	[TestFixture]
    public class EMailRegexConverterTest
    {
        [Test]
        public void ShouldPopulateFromSimpleXml()
        {
            EmailRegexConverter regexConverter = (EmailRegexConverter) NetReflector.Read(
                @"<regexConverter find=""$"" replace=""@Example.com""/>"
                );
            ClassicAssert.AreEqual("$", regexConverter.Find);
            ClassicAssert.AreEqual("$", regexConverter.Find);
            ClassicAssert.AreEqual("@Example.com", regexConverter.Replace);
        }

        [Test]
        public void ShouldPopulateFromComplexXml()
        {
            EmailRegexConverter regexConverter = (EmailRegexConverter)NetReflector.Read(
@"<regexConverter>
    <find>$</find>
    <replace>@Example.com</replace>
</regexConverter>"
                );
            ClassicAssert.AreEqual("$", regexConverter.Find);
            ClassicAssert.AreEqual("@Example.com", regexConverter.Replace);
        }

        [Test]
        public void ShouldFailToReadEmptyConverter()
        {
            ClassicAssert.That(delegate { NetReflector.Read(@"<regexConverter/>"); },
                        Throws.TypeOf<NetReflectorException>());
        }

        [Test]
        public void ShouldFailToReadOmittedFindAttribute()
        {
            ClassicAssert.That(delegate { NetReflector.Read(@"<regexConverter replace=""asdf"" />"); },
                        Throws.TypeOf<NetReflectorException>());
        }

        [Test]
        public void ShouldFailToReadOmittedReplaceAttribute()
        {
            ClassicAssert.That(delegate { NetReflector.Read(@"<regexConverter find=""asdf""/>"); },
                        Throws.TypeOf<NetReflectorException>());
        }
	}
}
