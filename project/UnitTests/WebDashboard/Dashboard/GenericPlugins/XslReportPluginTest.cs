using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.WebDashboard.Dashboard;
using ThoughtWorks.CruiseControl.WebDashboard.Dashboard.Actions;
using ThoughtWorks.CruiseControl.WebDashboard.Dashboard.GenericPlugins;

namespace ThoughtWorks.CruiseControl.UnitTests.WebDashboard.Dashboard.GenericPlugins
{
	[TestFixture]
	public class XslReportPluginTest
	{
		private Mock<IActionInstantiator> actionInstantiatorMock;
		private XslReportBuildPlugin buildPlugin;

		[SetUp]
		public void Setup()
		{
			actionInstantiatorMock = new Mock<IActionInstantiator>();
			buildPlugin = new XslReportBuildPlugin((IActionInstantiator) actionInstantiatorMock.Object);
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
			buildPlugin.XslFileName = @"xsl\myxsl.xsl";

			ClassicAssert.AreEqual("MyAction", buildPlugin.ActionName);
            //ClassicAssert.AreEqual("MyAction", buildPlugin.ActionName);
            ClassicAssert.AreEqual("My Plugin", buildPlugin.LinkDescription);
			ClassicAssert.AreEqual(@"xsl\myxsl.xsl", buildPlugin.XslFileName);

			VerifyAll();
		}

		[Test]
		public void ShouldCreateAnXslReportActionWithCorrectNameXslFileName()
		{
			buildPlugin.ActionName = "MyAction";
			buildPlugin.ConfiguredLinkDescription = "My Plugin";
			buildPlugin.XslFileName = @"xsl\myxsl.xsl";

			XslReportBuildAction xslReportAction = new XslReportBuildAction(null, null);
			actionInstantiatorMock.Setup(instantiator => instantiator.InstantiateAction(typeof(XslReportBuildAction))).Returns(xslReportAction).Verifiable();

			INamedAction[] namedActions = buildPlugin.NamedActions;

			ClassicAssert.AreEqual(1, namedActions.Length);
			ClassicAssert.AreEqual("MyAction", namedActions[0].ActionName);
			ClassicAssert.AreEqual(xslReportAction, namedActions[0].Action);
			ClassicAssert.AreEqual(@"xsl\myxsl.xsl", ((XslReportBuildAction) namedActions[0].Action).XslFileName);

			VerifyAll();
		}
	}
}
