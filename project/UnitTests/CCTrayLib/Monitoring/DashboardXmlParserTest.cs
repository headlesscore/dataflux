using System;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Monitoring
{
	[TestFixture]
	public class DashboardXmlParserTest
	{

		const string PROJECTS_XML = @"<Projects xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:noNamespaceSchemaLocation='C:\SF\ccnet\dashboard.xsd'>
	<Project name='SvnTest' activity='Sleeping' lastBuildStatus='Exception' lastBuildLabel='8' lastBuildTime='2005-09-28T10:30:34.6362160+01:00' nextBuildTime='2005-10-04T14:31:52.4509248+01:00' webUrl='http://mrtickle/ccnet/'/>
	<Project name='projectName' activity='Sleeping' lastBuildStatus='Success' lastBuildLabel='13' lastBuildTime='2005-09-15T17:33:07.6447696+01:00' nextBuildTime='2005-10-04T14:31:51.7799600+01:00' webUrl='http://mrtickle/ccnet/'/>
</Projects>";

        const string CRUISE_SERVER_XML = @"<CruiseControl xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'>
    <Projects>
	    <Project name='SvnTest' activity='Sleeping' lastBuildStatus='Exception' lastBuildLabel='8' lastBuildTime='2005-09-28T10:30:34.6362160+01:00' nextBuildTime='2005-10-04T14:31:52.4509248+01:00' webUrl='http://mrtickle/ccnet/'/>
	    <Project name='projectName' activity='Sleeping' lastBuildStatus='Success' lastBuildLabel='13' lastBuildTime='2005-09-15T17:33:07.6447696+01:00' nextBuildTime='2005-10-04T14:31:51.7799600+01:00' webUrl='http://mrtickle/ccnet/'/>
    </Projects>
    <Queues>
        <Queue name='Queue1'>
            <Request projectName='projectName' activity='CheckingModifications' />
            <Request projectName='SVNTest' activity='Pending' />
        </Queue>
        <Queue name='Queue2'>
            <Request projectName='Missing' activity='Building' />
        </Queue>
    </Queues>
</CruiseControl>";

		[Test]
		public void ReturnsCorrectProjectDetailsFromProjectsOnlyXml()
		{
			DashboardXmlParser parser = new DashboardXmlParser();
			
			CruiseServerSnapshot snapshot = parser.ExtractAsCruiseServerSnapshot(PROJECTS_XML);
			ClassicAssert.IsNotNull(snapshot);
            //ClassicAssert.IsNotNull(snapshot);

            ClassicAssert.AreEqual(2, snapshot.ProjectStatuses.Length);
		    AssertProjectsSerializedCorrectly(snapshot);
		}

	    private static void AssertProjectsSerializedCorrectly(CruiseServerSnapshot snapshot)
	    {
	        ProjectStatus projectStatus1 = snapshot.ProjectStatuses[0];
	        ClassicAssert.AreEqual("SvnTest", projectStatus1.Name);
	        ClassicAssert.AreEqual(ProjectActivity.Sleeping, projectStatus1.Activity);
	        ClassicAssert.AreEqual(IntegrationStatus.Exception, projectStatus1.BuildStatus);
	        ClassicAssert.AreEqual("8", projectStatus1.LastBuildLabel);
	        ClassicAssert.AreEqual("http://mrtickle/ccnet/", projectStatus1.WebURL);

	        ProjectStatus projectStatus2 = snapshot.ProjectStatuses[1];
	        ClassicAssert.AreEqual("projectName", projectStatus2.Name);
	        ClassicAssert.AreEqual(ProjectActivity.Sleeping, projectStatus2.Activity);
	        ClassicAssert.AreEqual(IntegrationStatus.Success, projectStatus2.BuildStatus);
	        ClassicAssert.AreEqual("13", projectStatus2.LastBuildLabel);
	        ClassicAssert.AreEqual("http://mrtickle/ccnet/", projectStatus2.WebURL);
	    }

	    [Test]
		public void ReturnsListOfProjectsFromProjectsXml()
		{
			DashboardXmlParser parser = new DashboardXmlParser();

			string[] names = parser.ExtractProjectNames(PROJECTS_XML);
			ClassicAssert.AreEqual(2, names.Length);
			ClassicAssert.AreEqual("SvnTest", names[0]);
			ClassicAssert.AreEqual("projectName", names[1]);
		}

        [Test]
        public void ReturnsListOfProjectsFromProjectsAndQueuesXml()
        {
            DashboardXmlParser parser = new DashboardXmlParser();

            string[] names = parser.ExtractProjectNames(CRUISE_SERVER_XML);
            ClassicAssert.AreEqual(2, names.Length);
            ClassicAssert.AreEqual("SvnTest", names[0]);
            ClassicAssert.AreEqual("projectName", names[1]);
        }

        [Test]
        public void ReturnsCorrectProjectDetailsFromProjectsAndQueuesXml()
        {
            DashboardXmlParser parser = new DashboardXmlParser();

            CruiseServerSnapshot snapshot = parser.ExtractAsCruiseServerSnapshot(CRUISE_SERVER_XML);
            ClassicAssert.IsNotNull(snapshot);

            ClassicAssert.AreEqual(2, snapshot.ProjectStatuses.Length);
            AssertProjectsSerializedCorrectly(snapshot);

            ClassicAssert.AreEqual(2, snapshot.QueueSetSnapshot.Queues.Count);
            QueueSnapshot queueSnapshot1 = snapshot.QueueSetSnapshot.Queues[0];
            ClassicAssert.AreEqual("Queue1", queueSnapshot1.QueueName);
            ClassicAssert.AreEqual("projectName", queueSnapshot1.Requests[0].ProjectName);
            ClassicAssert.AreEqual(ProjectActivity.CheckingModifications, queueSnapshot1.Requests[0].Activity);
            ClassicAssert.AreEqual("SVNTest", queueSnapshot1.Requests[1].ProjectName);
            ClassicAssert.AreEqual(ProjectActivity.Pending, queueSnapshot1.Requests[1].Activity);

            QueueSnapshot queueSnapshot2 = snapshot.QueueSetSnapshot.Queues[1];
            ClassicAssert.AreEqual("Queue2", queueSnapshot2.QueueName);
            ClassicAssert.AreEqual("Missing", queueSnapshot2.Requests[0].ProjectName);
            ClassicAssert.AreEqual(ProjectActivity.Building, queueSnapshot2.Requests[0].Activity);
        }
    }
}
