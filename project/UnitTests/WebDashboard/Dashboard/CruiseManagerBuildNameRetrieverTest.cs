using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Reporting.Dashboard.Navigation;
using ThoughtWorks.CruiseControl.WebDashboard.Dashboard;
using ThoughtWorks.CruiseControl.WebDashboard.ServerConnection;

namespace ThoughtWorks.CruiseControl.UnitTests.WebDashboard.Dashboard
{
	[TestFixture]
	public class CruiseManagerBuildNameRetrieverTest
	{
		private Mock<ICruiseManagerWrapper> cruiseManagerWrapperMock;
		private CruiseManagerBuildNameRetriever nameBuildRetriever;
		private string serverName;
		private string projectName;
		private string buildName;
		private IBuildSpecifier[] buildSpecifiers;
		private DefaultBuildSpecifier buildSpecifier;
		private DefaultProjectSpecifier projectSpecifier;

		[SetUp]
		public void Setup()
		{
			cruiseManagerWrapperMock = new Mock<ICruiseManagerWrapper>();

			nameBuildRetriever = new CruiseManagerBuildNameRetriever((ICruiseManagerWrapper) cruiseManagerWrapperMock.Object);

			serverName = "my Server";
			projectName = "my Project";
			buildName = "myLogfile.xml";
			projectSpecifier = new DefaultProjectSpecifier(new DefaultServerSpecifier(serverName), projectName);
			buildSpecifier = new DefaultBuildSpecifier(this.projectSpecifier, buildName);
			buildSpecifiers = new IBuildSpecifier[] {CreateBuildSpecifier("log3"), CreateBuildSpecifier("log2"), CreateBuildSpecifier("log1")};
		}

		private IBuildSpecifier CreateBuildSpecifier(string buildName)
		{
			return new DefaultBuildSpecifier(projectSpecifier, buildName);
		}

		private void VerifyAll()
		{
			cruiseManagerWrapperMock.Verify();
		}

		[Test]
		public void ReturnsNameOfLatestLog()
		{
			cruiseManagerWrapperMock.Setup(_manager => _manager.GetLatestBuildSpecifier(projectSpecifier, null)).Returns(buildSpecifier).Verifiable();

			ClassicAssert.AreEqual(buildSpecifier, nameBuildRetriever.GetLatestBuildSpecifier(projectSpecifier, null));
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
            VerifyAll();
		}

		[Test]
		public void NextBuildIsRequestedBuildIfNoneNewer()
		{
			cruiseManagerWrapperMock.Setup(_manager => _manager.GetBuildSpecifiers(projectSpecifier, null)).Returns(buildSpecifiers).Verifiable();

            ClassicAssert.AreEqual("log3", nameBuildRetriever.GetNextBuildSpecifier(new DefaultBuildSpecifier(projectSpecifier, "log3"), null).BuildName);

			VerifyAll();
		}

		[Test]
		public void NextBuildIsNextMostRecentBuildIfOneExists()
		{
			cruiseManagerWrapperMock.Setup(_manager => _manager.GetBuildSpecifiers(projectSpecifier, null)).Returns(buildSpecifiers).Verifiable();

            ClassicAssert.AreEqual("log2", nameBuildRetriever.GetNextBuildSpecifier(new DefaultBuildSpecifier(projectSpecifier, "log1"), null).BuildName);
			VerifyAll();
		}

		[Test]
		public void ThrowsAnExceptionForNextBuildIfBuildIsUnknown()
		{
			cruiseManagerWrapperMock.Setup(_manager => _manager.GetBuildSpecifiers(projectSpecifier, null)).Returns(buildSpecifiers).Verifiable();
			try
			{
                nameBuildRetriever.GetNextBuildSpecifier(new DefaultBuildSpecifier(projectSpecifier, "not a real build"), null);
				ClassicAssert.Fail("Should throw the right exception");
			}
			catch (UnknownBuildException) { }
			VerifyAll();
		}

		[Test]
		public void PreviousBuildIsRequestedBuildIfNoneOlder()
		{
			cruiseManagerWrapperMock.Setup(_manager => _manager.GetBuildSpecifiers(projectSpecifier, null)).Returns(buildSpecifiers).Verifiable();

            ClassicAssert.AreEqual("log1", nameBuildRetriever.GetPreviousBuildSpecifier(new DefaultBuildSpecifier(projectSpecifier, "log1"), null).BuildName);

			VerifyAll();
		}

		[Test]
		public void PreviousBuildIsNextOldestIfOneExists()
		{
			cruiseManagerWrapperMock.Setup(_manager => _manager.GetBuildSpecifiers(projectSpecifier, null)).Returns(buildSpecifiers).Verifiable();

            ClassicAssert.AreEqual("log2", nameBuildRetriever.GetPreviousBuildSpecifier(new DefaultBuildSpecifier(projectSpecifier, "log3"), null).BuildName);

			VerifyAll();
		}

		[Test]
		public void ThrowsAnExceptionForPreviousBuildIfBuildIsUnknown()
		{
			cruiseManagerWrapperMock.Setup(_manager => _manager.GetBuildSpecifiers(projectSpecifier, null)).Returns(buildSpecifiers).Verifiable();
			try
			{
                nameBuildRetriever.GetPreviousBuildSpecifier(new DefaultBuildSpecifier(projectSpecifier, "not a real build"), null);
				ClassicAssert.Fail("Should throw the right exception");
			}
			catch (UnknownBuildException) { }
			VerifyAll();
		}
	}
}
