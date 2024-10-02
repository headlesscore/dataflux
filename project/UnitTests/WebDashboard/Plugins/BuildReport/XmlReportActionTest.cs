using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using Moq;
using NUnit.Framework;
using ThoughtWorks.CruiseControl.Remote;
using ThoughtWorks.CruiseControl.Remote.Parameters;
using ThoughtWorks.CruiseControl.UnitTests.UnitTestUtils;
using ThoughtWorks.CruiseControl.WebDashboard.MVC;
using ThoughtWorks.CruiseControl.WebDashboard.Plugins.FarmReport;
using ThoughtWorks.CruiseControl.WebDashboard.ServerConnection;
using ThoughtWorks.CruiseControl.WebDashboard.Configuration;
using NUnit.Framework.Legacy;

namespace ThoughtWorks.CruiseControl.UnitTests.WebDashboard.Plugins.BuildReport
{
	[TestFixture]
	public class XmlReportActionTest
	{
		private Mock<IFarmService> mockFarmService;
		private XmlReportAction reportAction;

		private readonly DateTime LastBuildTime = new DateTime(2005, 7, 1, 12, 12, 12);
		private readonly DateTime NextBuildTime = new DateTime(2005, 7, 2, 13, 13, 13);

		[SetUp]
		public void SetUp()
		{
			mockFarmService = new Mock<IFarmService>();
			reportAction = new XmlReportAction((IFarmService) mockFarmService.Object);
		}

		[Test]
		public void ReturnsAXmlResponse()
		{
			mockFarmService.Setup(service => service.GetProjectStatusListAndCaptureExceptions(null)).
				Returns(new ProjectStatusListAndExceptions(new ProjectStatusOnServer[0], new CruiseServerException[0])).
				Verifiable();
			IResponse response = reportAction.Execute(null);
			ClassicAssert.IsNotNull(response);
			ClassicAssert.AreEqual(typeof (XmlFragmentResponse), response.GetType());
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
            mockFarmService.Verify();
		}

		[Test]
		public void WhenNoProjectStatusEntriesAreReturnedByTheFarmServiceTheXmlContainsJustASingleRootNode()
		{
			mockFarmService.Setup(service => service.GetProjectStatusListAndCaptureExceptions(null)).
				Returns(new ProjectStatusListAndExceptions(new ProjectStatusOnServer[0], new CruiseServerException[0])).
				Verifiable();
			XmlFragmentResponse response = (XmlFragmentResponse) reportAction.Execute(null);
			string xml = response.ResponseFragment;

			ClassicAssert.AreEqual("<Projects CCType=\"CCNet\" />", xml);

			mockFarmService.Verify();
		}

		[Test]
		public void WhenOneProjectStatusIsReturnedThisIsContainedInTheReturnedXml()
		{
			ProjectStatus projectStatus = CreateProjectStatus();
            ServerLocation ServerSpecifier = new ServerLocation();
            ServerSpecifier.ServerName = "localhost";

            ProjectStatusOnServer projectStatusOnServer = new ProjectStatusOnServer(projectStatus, ServerSpecifier);
			mockFarmService.Setup(service => service.GetProjectStatusListAndCaptureExceptions(null)).
				Returns(new ProjectStatusListAndExceptions(new ProjectStatusOnServer[] { projectStatusOnServer }, new CruiseServerException[0])).
				Verifiable();

			XmlFragmentResponse response = (XmlFragmentResponse) reportAction.Execute(null);
			string xml = response.ResponseFragment;
			XmlDocument doc = XPathAssert.LoadAsDocument(xml);

            XPathAssert.Matches(doc, "/Projects/Project/@name", "HelloWorld");
            XPathAssert.Matches(doc, "/Projects/Project/@activity", "Sleeping");
            XPathAssert.Matches(doc, "/Projects/Project/@lastBuildStatus", "Success");
            XPathAssert.Matches(doc, "/Projects/Project/@lastBuildLabel", "build_7");
            XPathAssert.Matches(doc, "/Projects/Project/@lastBuildTime", LastBuildTime);
            XPathAssert.Matches(doc, "/Projects/Project/@nextBuildTime", NextBuildTime);
            XPathAssert.Matches(doc, "/Projects/Project/@webUrl", "http://blah");
            XPathAssert.Matches(doc, "/Projects/Project/@category", "category");

			mockFarmService.Verify();
		}

		[Test]
		public void ReturnedXmlValidatesAgainstSchema()
		{
			ProjectStatus projectStatus = CreateProjectStatus();
            ServerLocation ServerSpecifier = new ServerLocation();
            ServerSpecifier.ServerName = "localhost";


            ProjectStatusOnServer projectStatusOnServer = new ProjectStatusOnServer(projectStatus, ServerSpecifier);
			mockFarmService.Setup(service => service.GetProjectStatusListAndCaptureExceptions(null)).
				Returns(new ProjectStatusListAndExceptions(new ProjectStatusOnServer[] { projectStatusOnServer }, new CruiseServerException[0])).
				Verifiable();

			XmlFragmentResponse response = (XmlFragmentResponse) reportAction.Execute(null);
			string xml = response.ResponseFragment;

		    XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
		    xmlReaderSettings.Schemas.Add(ReadSchemaFromResources("XmlReportActionSchema.xsd"));
		    XmlReader rdr = XmlReader.Create(new StringReader(xml), xmlReaderSettings);
			while (rdr.Read())
			{
			}

			mockFarmService.Verify();
		}

		private ProjectStatus CreateProjectStatus()
		{
			return
				new ProjectStatus("HelloWorld", "category", ProjectActivity.Sleeping, IntegrationStatus.Success, ProjectIntegratorState.Running,
				                  "http://blah", LastBuildTime, "build_8", "build_7",
                                  NextBuildTime, "", "", 0, new List<ParameterBase>());
		}

		private XmlSchema ReadSchemaFromResources(string filename)
		{
			using (Stream s = ResourceUtil.LoadResource(GetType(), filename))
			{
				return XmlSchema.Read(s, null);
			}
		}
	}
}
