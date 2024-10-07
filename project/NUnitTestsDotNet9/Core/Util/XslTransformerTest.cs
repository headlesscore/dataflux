using System;
using System.Collections;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
	[TestFixture]
	public class XslTransformerTest : CustomAssertion
	{
		private static readonly string TestFolder = "logTransformerTest";

		[SetUp]
		public void Setup()
		{
			TempFileUtil.CreateTempDir(TestFolder);
		}

		[TearDown]
		public void Teardown()
		{
			TempFileUtil.DeleteTempDir(TestFolder);
		}

		// Todo - more thorough testing here
		[Test]
		public void ShouldTransformData()
		{
			string input = TestData.LogFileContents;
			string xslfile = TempFileUtil.CreateTempXmlFile(TestFolder, "samplestylesheet.xsl", TestData.StyleSheetContents);

			string output = new XslTransformer().Transform(input, xslfile, null);
			ClassicAssert.IsNotNull(output);
			ClassicAssert.IsTrue(! String.Empty.Equals(output), "Transform returned no data");
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

		[Test]
		public void ShouldPassThroughXSLTArgs()
		{
			string input = TestData.LogFileContents;
			string xslfile = TempFileUtil.CreateTempXmlFile(TestFolder, "samplestylesheet.xsl", TestData.StyleSheetContentsWithParam);

			Hashtable xsltArgs = new Hashtable();
			xsltArgs["myParam"] = "myValue";
			string output = new XslTransformer().Transform(input, xslfile, xsltArgs);
			ClassicAssert.IsTrue(output.IndexOf("myValue") > 0);
		}

		[Test]
		public void ShouldFailWhenInputInvalid()
		{
			string input = @"<This is some invalid xml";
			string xslfile = TempFileUtil.CreateTempXmlFile(TestFolder, "samplestylesheet.xsl", TestData.StyleSheetContents);

			ClassicAssert.That(delegate { new XslTransformer().Transform(input, xslfile, null); },
                        Throws.TypeOf<CruiseControlException>());
		}

		[Test]
		public void ShouldFailWhenXslFileMissing()
		{
			string logfile = TestData.LogFileContents;
			string xslfile = "nosuchstylefile";

			ClassicAssert.That(delegate { new XslTransformer().Transform(logfile, xslfile, null); },
                        Throws.TypeOf<CruiseControlException>());
		}

		[Test]
		public void ShouldFailWhenXslInvalid()
		{
			string logfile = TestData.LogFileContents;
			string xslfile = XslFileBadFormat;
            ClassicAssert.That(delegate { new XslTransformer().Transform(logfile, xslfile, null); },
                        Throws.TypeOf<CruiseControlException>());
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
