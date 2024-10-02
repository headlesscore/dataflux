using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Reporting.Dashboard.Navigation;
using ThoughtWorks.CruiseControl.WebDashboard.IO;
using ThoughtWorks.CruiseControl.WebDashboard.MVC;
using ThoughtWorks.CruiseControl.WebDashboard.Plugins.FarmReport;
using ThoughtWorks.CruiseControl.WebDashboard.ServerConnection;

namespace ThoughtWorks.CruiseControl.UnitTests.WebDashboard.Plugins.ForceBuild
{
	[TestFixture]
	public class ForceBuildXmlActionTest
	{
		private Mock<IFarmService> mockFarmService;
		private ForceBuildXmlAction reportAction;
		private Mock<ICruiseRequest> cruiseRequestMock;
		private ICruiseRequest cruiseRequest;

		[SetUp]
		public void SetUp()
		{
			mockFarmService = new Mock<IFarmService>();
			reportAction = new ForceBuildXmlAction((IFarmService) mockFarmService.Object);
			cruiseRequestMock = new Mock<ICruiseRequest>();
			cruiseRequest = (ICruiseRequest) cruiseRequestMock.Object;
		}

		public void VerifyAll()
		{
			mockFarmService.Verify();
			cruiseRequestMock.Verify();
		}

		[Test]
		public void ShouldReturnCorrectMessageIfBuildForcedSuccessfully()
		{
			DefaultProjectSpecifier projectSpecifier = new DefaultProjectSpecifier(
				new DefaultServerSpecifier("myServer"), "myProject");
			cruiseRequestMock.SetupGet(_request => _request.ProjectSpecifier).Returns(projectSpecifier);
			cruiseRequestMock.SetupGet(_request => _request.ProjectName).Returns("myProject");

            mockFarmService.Setup(service => service.ForceBuild(projectSpecifier, (string)null)).Verifiable();

			IResponse response = reportAction.Execute(cruiseRequest);
			ClassicAssert.IsTrue(response is XmlFragmentResponse);
			ClassicAssert.AreEqual("<ForceBuildResult>Build Forced for myProject</ForceBuildResult>",
			                ((XmlFragmentResponse) response).ResponseFragment);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }
	}
}
