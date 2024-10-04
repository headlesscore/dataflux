using Xunit;

using ThoughtWorks.CruiseControl.Core.Publishers;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Publishers
{
	
	public class HtmlDetailsMessageBuilderTest : CustomAssertion
	{
	    [Fact]
		public void ShouldCreateStyleElementsInTheMailMessage()
	    {
	    	HtmlDetailsMessageBuilder builder = new HtmlDetailsMessageBuilder();
	        string message = builder.BuildMessage(IntegrationResultMother.CreateSuccessful());
	        int styleBegin = message.IndexOf("<style>");
	        int styleEnd = message.IndexOf("</style>");

	        Assert.True(styleBegin != -1);
            Assert.True(styleBegin != -1);
            Assert.True(styleEnd != -1);
			Assert.True(styleEnd - styleBegin > 8, "There must be some styles from the loaded file");
	    }
	}
}
