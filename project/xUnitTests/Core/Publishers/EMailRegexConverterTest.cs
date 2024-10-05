using Exortech.NetReflector;
using Xunit;

using ThoughtWorks.CruiseControl.Core.Publishers;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Publishers
{
	
    public class EMailRegexConverterTest
    {
        [Fact]
        public void ShouldPopulateFromSimpleXml()
        {
            EmailRegexConverter regexConverter = (EmailRegexConverter) NetReflector.Read(
                @"<regexConverter find=""$"" replace=""@Example.com""/>"
                );
            Assert.Equal("$", regexConverter.Find);
            Assert.Equal("$", regexConverter.Find);
            Assert.Equal("@Example.com", regexConverter.Replace);
        }

        [Fact]
        public void ShouldPopulateFromComplexXml()
        {
            EmailRegexConverter regexConverter = (EmailRegexConverter)NetReflector.Read(
@"<regexConverter>
    <find>$</find>
    <replace>@Example.com</replace>
</regexConverter>"
                );
            Assert.Equal("$", regexConverter.Find);
            Assert.Equal("@Example.com", regexConverter.Replace);
        }

        [Fact]
        public void ShouldFailToReadEmptyConverter()
        {
            Assert.Throws<NetReflectorException>(delegate { NetReflector.Read(@"<regexConverter/>"); });
        }

        [Fact]
        public void ShouldFailToReadOmittedFindAttribute()
        {
            Assert.Throws<NetReflectorException>(delegate { NetReflector.Read(@"<regexConverter replace=""asdf"" />"); });
        }

        [Fact]
        public void ShouldFailToReadOmittedReplaceAttribute()
        {
            Assert.Throws<NetReflectorException>(delegate { NetReflector.Read(@"<regexConverter find=""asdf""/>"); });
        }
	}
}
