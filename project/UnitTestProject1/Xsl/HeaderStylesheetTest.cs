using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThoughtWorks.CruiseControl.UnitTests.Xsl
{
	[TestClass]
	public class HeaderStylesheetTest : StylesheetTestFixture
	{
		protected override string Stylesheet
		{
			get { return @"xsl\header.xsl"; }
		}

		[TestMethod]
		public void ShouldOutputIntegrationRequest()
		{
			string input = @"<cruisecontrol><request>foobar</request></cruisecontrol>";

			string actualXml = LoadStylesheetAndTransformInput(input);
			CustomAssertion.AssertContains("foobar", actualXml);
		}
	}
}
