using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core.Reporting.Dashboard.Navigation;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Reporting.Dashboard.Navigation
{
	
	public class AbsolutePathUrlBuilderDecoratorTest
	{
		[Fact]
		public void ShouldDecorateUrlsToCreateAbsoluteURLs()
		{
			/// Setup
			var decoratedBuilderMock = new Mock<IUrlBuilder>();
			string baseUrl = "https://myserver:8080/myvdir";

			AbsolutePathUrlBuilderDecorator decorator = new AbsolutePathUrlBuilderDecorator((IUrlBuilder) decoratedBuilderMock.Object, baseUrl);
			string actionName = "myAction";
			decoratedBuilderMock.Setup(builder => builder.BuildUrl(actionName)).Returns("myRelativeUrl").Verifiable();
			decoratedBuilderMock.Setup(builder => builder.BuildUrl(actionName, "query")).Returns("myRelativeUrl2").Verifiable();
			decoratedBuilderMock.Setup(builder => builder.BuildUrl(actionName, "query", "myPath/")).Returns("myPath/myRelativeUrl3").Verifiable();

			/// Execute & Verify
			Assert.Equal(baseUrl + "/myRelativeUrl", decorator.BuildUrl(actionName));
			Assert.Equal(baseUrl + "/myRelativeUrl2", decorator.BuildUrl(actionName, "query"));
			Assert.Equal(baseUrl + "/myPath/myRelativeUrl3", decorator.BuildUrl(actionName, "query", "myPath/"));
            Assert.True(true);
            decoratedBuilderMock.Verify();
		}

		[Fact]
		public void ShouldHandleBaseURLsWithTrailingSlashes()
		{
			/// Setup
			var decoratedBuilderMock = new Mock<IUrlBuilder>();
			string baseUrl = "https://myserver:8080/myvdir/";

			AbsolutePathUrlBuilderDecorator decorator = new AbsolutePathUrlBuilderDecorator((IUrlBuilder) decoratedBuilderMock.Object, baseUrl);
			string actionName = "myAction";
			decoratedBuilderMock.Setup(builder => builder.BuildUrl(actionName)).Returns("myRelativeUrl").Verifiable();

			/// Execute & Verify
			Assert.Equal(baseUrl + "myRelativeUrl", decorator.BuildUrl(actionName));

			decoratedBuilderMock.Verify();
		}

		[Fact]
		public void ShouldDelegateExtensionToSubBuilder()
		{
			// Setup
			var decoratedBuilderMock = new Mock<IUrlBuilder>();
			decoratedBuilderMock.SetupSet(builder => builder.Extension = "foo").Verifiable();

			// Execute
			AbsolutePathUrlBuilderDecorator decorator = new AbsolutePathUrlBuilderDecorator((IUrlBuilder) decoratedBuilderMock.Object, null);
			decorator.Extension = "foo";

			// Verify
			decoratedBuilderMock.Verify();
		}
	}
}
