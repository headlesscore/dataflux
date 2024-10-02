using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.WebDashboard.MVC;

namespace ThoughtWorks.CruiseControl.UnitTests.WebDashboard.MVC
{
	[TestFixture]
	public class HtmlViewTest
	{
		[Test]
		public void ShouldGiveHtmlFragmentIfStringConstructorUsed()
		{
			HtmlFragmentResponse responseFragment = new HtmlFragmentResponse("Some HTML");

			ClassicAssert.AreEqual("Some HTML", responseFragment.ResponseFragment );
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }
	}
}
