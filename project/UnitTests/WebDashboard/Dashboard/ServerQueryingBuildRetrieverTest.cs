using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Reporting.Dashboard.Navigation;
using ThoughtWorks.CruiseControl.WebDashboard.Dashboard;
using ThoughtWorks.CruiseControl.WebDashboard.ServerConnection;

namespace ThoughtWorks.CruiseControl.UnitTests.WebDashboard.Dashboard
{
	[TestFixture]
	public class ServerQueryingBuildRetrieverTest
	{
		private Mock<ICruiseManagerWrapper> cruiseManagerWrapperMock;
		private ServerQueryingBuildRetriever serverQueryingBuildRetriever;
		private string logContent;
		private DefaultBuildSpecifier buildSpecifier;

		[SetUp]
		public void Setup()
		{
			cruiseManagerWrapperMock = new Mock<ICruiseManagerWrapper>();

			serverQueryingBuildRetriever = new ServerQueryingBuildRetriever(((ICruiseManagerWrapper) cruiseManagerWrapperMock.Object));

			buildSpecifier = new DefaultBuildSpecifier(new DefaultProjectSpecifier(new DefaultServerSpecifier("s"), "p"), "myBuild");
			logContent = "log Content";
		}

		private void VerifyAll()
		{
			cruiseManagerWrapperMock.Verify();
		}

		[Test]
		public void ReturnsBuildUsingLogFromServer()
		{
			cruiseManagerWrapperMock.Setup(_manager => _manager.GetLog(buildSpecifier, null)).Returns(logContent).Verifiable();

			Build returnedBuild = serverQueryingBuildRetriever.GetBuild(buildSpecifier, null);

			ClassicAssert.AreEqual(buildSpecifier, returnedBuild.BuildSpecifier);
			ClassicAssert.AreEqual(logContent, returnedBuild.Log);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
            VerifyAll();
		}
	}
}
