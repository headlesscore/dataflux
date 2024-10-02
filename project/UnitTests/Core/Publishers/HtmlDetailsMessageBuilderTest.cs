using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Publishers;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Publishers
{
	[TestFixture]
	public class HtmlDetailsMessageBuilderTest : CustomAssertion
	{
	    [Test]
		public void ShouldCreateStyleElementsInTheMailMessage()
	    {
	    	HtmlDetailsMessageBuilder builder = new HtmlDetailsMessageBuilder();
	        string message = builder.BuildMessage(IntegrationResultMother.CreateSuccessful());
	        int styleBegin = message.IndexOf("<style>");
	        int styleEnd = message.IndexOf("</style>");

	        ClassicAssert.IsTrue(styleBegin != -1);
            ClassicAssert.IsTrue(styleBegin != -1);
            ClassicAssert.IsTrue(styleEnd != -1);
			ClassicAssert.IsTrue(styleEnd - styleBegin > 8, "There must be some styles from the loaded file");
	    }
	}
}
