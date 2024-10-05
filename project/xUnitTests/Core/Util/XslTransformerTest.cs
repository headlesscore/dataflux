using System;
using System.Collections;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
	
	public class XslTransformerTest : CustomAssertion
	{
		private static readonly string TestFolder = "logTransformerTest";

		// [SetUp]
		public void Setup()
		{
			TempFileUtil.CreateTempDir(TestFolder);
		}

		// [TearDown]
		public void Teardown()
		{
			TempFileUtil.DeleteTempDir(TestFolder);
		}

		// Todo - more thorough testing here
		[Fact]
		public void ShouldTransformData()
		{
			string input = TestData.LogFileContents;
			string xslfile = TempFileUtil.CreateTempXmlFile(TestFolder, "samplestylesheet.xsl", TestData.StyleSheetContents);

			string output = new XslTransformer().Transform(input, xslfile, null);
			Assert.NotNull(output);
			Assert.True(! String.Empty.Equals(output), "Transform returned no data");
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public void ShouldPassThroughXSLTArgs()
		{
			string input = TestData.LogFileContents;
			string xslfile = TempFileUtil.CreateTempXmlFile(TestFolder, "samplestylesheet.xsl", TestData.StyleSheetContentsWithParam);

			Hashtable xsltArgs = new Hashtable();
			xsltArgs["myParam"] = "myValue";
			string output = new XslTransformer().Transform(input, xslfile, xsltArgs);
			Assert.True(output.IndexOf("myValue") > 0);
		}

		[Fact]
		public void ShouldFailWhenInputInvalid()
		{
			string input = @"<This is some invalid xml";
			string xslfile = TempFileUtil.CreateTempXmlFile(TestFolder, "samplestylesheet.xsl", TestData.StyleSheetContents);

			Assert.Throws<CruiseControlException>(delegate { new XslTransformer().Transform(input, xslfile, null); });
		}

		[Fact]
		public void ShouldFailWhenXslFileMissing()
		{
			string logfile = TestData.LogFileContents;
			string xslfile = "nosuchstylefile";

			Assert.Throws<CruiseControlException>(delegate { new XslTransformer().Transform(logfile, xslfile, null); });
		}

		[Fact]
		public void ShouldFailWhenXslInvalid()
		{
			string logfile = TestData.LogFileContents;
			string xslfile = XslFileBadFormat;
            Assert.Throws<CruiseControlException>(delegate { new XslTransformer().Transform(logfile, xslfile, null); });
		}

		private static string XslFileBadFormat
		{
			get
			{
				return TempFileUtil.CreateTempXmlFile(
					TestFolder, "samplestylesheet.xsl", @"<xsl:this is some bad xsl");
			}
		}
	}
}
