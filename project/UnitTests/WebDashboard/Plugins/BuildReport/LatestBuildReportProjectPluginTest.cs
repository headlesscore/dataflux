using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Reporting.Dashboard.Navigation;
using ThoughtWorks.CruiseControl.WebDashboard.Dashboard;
using ThoughtWorks.CruiseControl.WebDashboard.IO;
using ThoughtWorks.CruiseControl.WebDashboard.MVC;
using ThoughtWorks.CruiseControl.WebDashboard.Plugins.BuildReport;
using ThoughtWorks.CruiseControl.WebDashboard.ServerConnection;

namespace ThoughtWorks.CruiseControl.UnitTests.WebDashboard.Plugins.BuildReport
{
	[TestFixture]
	public class LatestBuildReportProjectPluginTest
	{
		private Mock<IFarmService> farmServiceMock;
		private Mock<ILinkFactory> linkFactoryMock;
		private LatestBuildReportProjectPlugin plugin;
		private Mock<ICruiseRequest> cruiseRequestMock;
		private ICruiseRequest cruiseRequest;

		[SetUp]
		public void Setup()
		{
			farmServiceMock = new Mock<IFarmService>();
			linkFactoryMock = new Mock<ILinkFactory>();
			plugin = new LatestBuildReportProjectPlugin((IFarmService) farmServiceMock.Object,
			                                            (ILinkFactory) linkFactoryMock.Object);

			cruiseRequestMock = new Mock<ICruiseRequest>();
			cruiseRequest = (ICruiseRequest) cruiseRequestMock.Object;
		}
		
		[Test]
		public void ShouldReturnWarningMessageIfNoBuildsAvailable()
		{
			IProjectSpecifier projectSpecifier = new DefaultProjectSpecifier(new DefaultServerSpecifier("myServer"), "myProject");
			cruiseRequestMock.SetupGet(_request => _request.ProjectSpecifier).Returns(projectSpecifier).Verifiable();
			farmServiceMock.Setup(services => services.GetMostRecentBuildSpecifiers(projectSpecifier, 1, null)).Returns(new IBuildSpecifier[0]).Verifiable();
			
			IResponse returnedReponse = plugin.Execute(cruiseRequest);
			ClassicAssert.IsTrue(returnedReponse is HtmlFragmentResponse);
			ClassicAssert.AreEqual("There are no complete builds for this project", ((HtmlFragmentResponse) returnedReponse).ResponseFragment);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }
		
		[Test]
		public void ShouldReturnRedirectToActualBuildReportPageIfBuildAvailable()
		{
			IProjectSpecifier projectSpecifier = new DefaultProjectSpecifier(new DefaultServerSpecifier("myServer"), "myProject");
			IBuildSpecifier buildSpecifier = new DefaultBuildSpecifier(projectSpecifier, "myBuild");
			cruiseRequestMock.SetupGet(_request => _request.ProjectSpecifier).Returns(projectSpecifier).Verifiable();
			farmServiceMock.Setup(services => services.GetMostRecentBuildSpecifiers(projectSpecifier, 1, null)).Returns(new IBuildSpecifier[] { buildSpecifier }).Verifiable();
			linkFactoryMock.Setup(factory => factory.CreateBuildLink(buildSpecifier, BuildReportBuildPlugin.ACTION_NAME)).Returns(new GeneralAbsoluteLink("foo", "buildUrl")).Verifiable();

			IResponse returnedReponse = plugin.Execute(cruiseRequest);
			ClassicAssert.IsTrue(returnedReponse is RedirectResponse);
			ClassicAssert.AreEqual("buildUrl", ((RedirectResponse) returnedReponse).Url);
			
		}
	}
}
