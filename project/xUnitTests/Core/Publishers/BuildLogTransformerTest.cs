using System.IO;
using System.Xml.XPath;
using Xunit;

using ThoughtWorks.CruiseControl.Core.Publishers;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Publishers
{
	
	public class BuildLogTransformerTest
	{
		[Fact]
		public void TransformingDocumentWithEmptyXSLFilesReturnsEmptyString()
		{
			BuildLogTransformer xformer = new BuildLogTransformer();
			string result = xformer.TransformResults(null, new XPathDocument(new StringReader("<foo />")));
			Assert.Equal("", result);
            Assert.Equal("", result);
        }
	}
}
