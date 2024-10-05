using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThoughtWorks.CruiseControl.UnitTests.Xsl
{
	[TestClass]
	public class MsBuildSummaryStylesheetTest : StylesheetTestFixture
	{
		protected override string Stylesheet
		{
			get { return @"xsl\compile-msbuild.xsl"; }
		}

		[TestMethod]
		public void ShouldNotRenderAnyOutputIfRootNodeIsMissing()
		{
			string xml = WrapInBuildResultsElement("<foo>bar</foo>");
			string actualXml = LoadStylesheetAndTransformInput(xml);
			Assert.AreEqual("", actualXml);
        }

		[TestMethod]
		public void ShouldRenderWarnings()
		{
			string xml = WrapInBuildResultsElement(@"<msbuild><warning file=""c:\temp\foo.txt"" line=""10"" column=""23"">Invalid behaviour.</warning></msbuild>");
			string actualXml = LoadStylesheetAndTransformInput(xml);
			CustomAssertion.AssertContains(@"c:\temp\foo.txt", actualXml);
			CustomAssertion.AssertContains("(10,23)", actualXml);
			CustomAssertion.AssertContains("Invalid behaviour.", actualXml);
		}

		[TestMethod]
		public void ShouldRenderErrors()
		{
			string xml = WrapInBuildResultsElement(@"<msbuild><error file=""c:\temp\foo.txt"" line=""10"" column=""23"">Invalid behaviour.</error></msbuild>");
			string actualXml = LoadStylesheetAndTransformInput(xml);
			CustomAssertion.AssertContains(@"c:\temp\foo.txt", actualXml);
			CustomAssertion.AssertContains("(10,23)", actualXml);
			CustomAssertion.AssertContains("Invalid behaviour.", actualXml);
		}

	}
}
