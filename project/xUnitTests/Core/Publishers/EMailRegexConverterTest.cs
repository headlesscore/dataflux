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
            Assert.True(delegate { NetReflector.Read(@"<regexConverter/>"); },
                        Throws.TypeOf<NetReflectorException>());
        }

        [Fact]
        public void ShouldFailToReadOmittedFindAttribute()
        {
            Assert.True(delegate { NetReflector.Read(@"<regexConverter replace=""asdf"" />"); },
                        Throws.TypeOf<NetReflectorException>());
        }

        [Fact]
        public void ShouldFailToReadOmittedReplaceAttribute()
        {
            Assert.True(delegate { NetReflector.Read(@"<regexConverter find=""asdf""/>"); },
                        Throws.TypeOf<NetReflectorException>());
        }
	}
}
