using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Reporting.Dashboard.Navigation;

namespace ThoughtWorks.CruiseControl.UnitTests.WebDashboard.Dashboard
{
	[TestFixture]
	public class DefaultUrlBuilderTest
	{
		private DefaultUrlBuilder urlBuilder;

		[SetUp]
		public void Setup()
		{
			urlBuilder = new DefaultUrlBuilder();
		}

		[Test]
		public void ShouldBuildUrlAddingCorrectlyFormattedAction()
		{
			ClassicAssert.AreEqual("myAction.aspx", urlBuilder.BuildUrl("myAction"));
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

		[Test]
		public void ShouldBuildUrlWithActionAndQueryString()
		{
			ClassicAssert.AreEqual("myAction.aspx?myparam=myvalue", urlBuilder.BuildUrl("myAction", "myparam=myvalue"));
		}

		[Test]
		public void ShouldBuildUrlWithActionAndQueryStringAndPath()
		{
			ClassicAssert.AreEqual("myPath/myAction.aspx?myparam=myvalue", urlBuilder.BuildUrl("myAction", "myparam=myvalue", "myPath/"));
		}

		[Test]
		public void ShouldAddTrailingSlashToPathIfItDoesntAlreadyHaveOne()
		{
			ClassicAssert.AreEqual("myPath/myAction.aspx?myparam=myvalue", urlBuilder.BuildUrl("myAction", "myparam=myvalue", "myPath"));
		}

		[Test]
		public void ShouldHandlePathsWithMoreThanOneLevel()
		{
			ClassicAssert.AreEqual("myPath/mySubPath/myAction.aspx?myparam=myvalue", urlBuilder.BuildUrl("myAction", "myparam=myvalue", "myPath/mySubPath"));
		}

		[Test]
		public void ShouldUseSpecifiedExtension()
		{
			urlBuilder.Extension = "foo";
			ClassicAssert.AreEqual("myAction.foo", urlBuilder.BuildUrl("myAction"));
			ClassicAssert.AreEqual("myAction.foo?myparam=myvalue", urlBuilder.BuildUrl("myAction", "myparam=myvalue"));
		}
	}
}
