using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.WebDashboard.Dashboard;
using ThoughtWorks.CruiseControl.WebDashboard.Dashboard.Actions;
using ThoughtWorks.CruiseControl.WebDashboard.Dashboard.GenericPlugins;
using ThoughtWorks.CruiseControl.WebDashboard.Plugins.BuildReport;

namespace ThoughtWorks.CruiseControl.UnitTests.WebDashboard.Dashboard.Actions
{
	[TestFixture]
	public class XslMultiGenericReportActionTest
	{
		private Mock<IActionInstantiator> actionInstantiatorMock;
		private XslMultiReportBuildPlugin buildPlugin;
        private BuildReportXslFilename[] xslFiles = new BuildReportXslFilename[] {
            new BuildReportXslFilename(@"xsl\myxsl.xsl"),
            new BuildReportXslFilename(@"xsl\myxsl2.xsl")
        };

		[SetUp]
		public void Setup()
		{
			actionInstantiatorMock = new Mock<IActionInstantiator>();
			buildPlugin = new XslMultiReportBuildPlugin((IActionInstantiator) actionInstantiatorMock.Object);
		}

		private void VerifyAll()
		{
			actionInstantiatorMock.Verify();
		}

		[Test]
		public void ShouldUseConfigurableProperties()
		{
			buildPlugin.ActionName = "MyAction";
			buildPlugin.ConfiguredLinkDescription = "My Plugin";
			buildPlugin.XslFileNames = xslFiles;

			ClassicAssert.AreEqual("MyAction", buildPlugin.ActionName);
			ClassicAssert.AreEqual("My Plugin", buildPlugin.LinkDescription);
			ClassicAssert.AreEqual(xslFiles, buildPlugin.XslFileNames);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
            VerifyAll();
		}

		[Test]
		public void ShouldCreateAnMultipleXslReportBuildActionWithCorrectNameXslFileName()
		{
			buildPlugin.ActionName = "MyAction";
			buildPlugin.ConfiguredLinkDescription = "My Plugin";
			buildPlugin.XslFileNames = xslFiles;

			MultipleXslReportBuildAction xslReportAction = new MultipleXslReportBuildAction(null, null);
			actionInstantiatorMock.Setup(instantiator => instantiator.InstantiateAction(typeof(MultipleXslReportBuildAction))).Returns(xslReportAction).Verifiable();

			INamedAction[] namedActions = buildPlugin.NamedActions;

			ClassicAssert.AreEqual(1, namedActions.Length);
			ClassicAssert.AreEqual("MyAction", namedActions[0].ActionName);
			ClassicAssert.AreEqual(xslReportAction, namedActions[0].Action);
			ClassicAssert.AreEqual(xslFiles, ((MultipleXslReportBuildAction) namedActions[0].Action).XslFileNames);

			VerifyAll();
		}
	}
}
