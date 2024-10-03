using System;
using Xunit;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Monitoring
{
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

		[Fact]
		public void ReturnsCorrectProjectDetailsFromProjectsOnlyXml()
		{
			DashboardXmlParser parser = new DashboardXmlParser();
			
			CruiseServerSnapshot snapshot = parser.ExtractAsCruiseServerSnapshot(PROJECTS_XML);
			Assert.NotNull(snapshot);
            //ClassicAssert.IsNotNull(snapshot);

            Assert.Equal(2, snapshot.ProjectStatuses.Length);
		    AssertProjectsSerializedCorrectly(snapshot);
		}

	    private static void AssertProjectsSerializedCorrectly(CruiseServerSnapshot snapshot)
	    {
	        ProjectStatus projectStatus1 = snapshot.ProjectStatuses[0];
	        Assert.Equal("SvnTest", projectStatus1.Name);
	        Assert.Equal(ProjectActivity.Sleeping, projectStatus1.Activity);
	        Assert.Equal(IntegrationStatus.Exception, projectStatus1.BuildStatus);
	        Assert.Equal("8", projectStatus1.LastBuildLabel);
	        Assert.Equal("http://mrtickle/ccnet/", projectStatus1.WebURL);

	        ProjectStatus projectStatus2 = snapshot.ProjectStatuses[1];
	        Assert.Equal("projectName", projectStatus2.Name);
	        Assert.Equal(ProjectActivity.Sleeping, projectStatus2.Activity);
	        Assert.Equal(IntegrationStatus.Success, projectStatus2.BuildStatus);
	        Assert.Equal("13", projectStatus2.LastBuildLabel);
	        Assert.Equal("http://mrtickle/ccnet/", projectStatus2.WebURL);
	    }

	    [Fact]
		public void ReturnsListOfProjectsFromProjectsXml()
		{
			DashboardXmlParser parser = new DashboardXmlParser();

			string[] names = parser.ExtractProjectNames(PROJECTS_XML);
			Assert.Equal(2, names.Length);
			Assert.Equal("SvnTest", names[0]);
			Assert.Equal("projectName", names[1]);
		}

        [Fact]
        public void ReturnsListOfProjectsFromProjectsAndQueuesXml()
        {
            DashboardXmlParser parser = new DashboardXmlParser();

            string[] names = parser.ExtractProjectNames(CRUISE_SERVER_XML);
            Assert.Equal(2, names.Length);
            Assert.Equal("SvnTest", names[0]);
            Assert.Equal("projectName", names[1]);
        }

        [Fact]
        public void ReturnsCorrectProjectDetailsFromProjectsAndQueuesXml()
        {
            DashboardXmlParser parser = new DashboardXmlParser();

            CruiseServerSnapshot snapshot = parser.ExtractAsCruiseServerSnapshot(CRUISE_SERVER_XML);
            Assert.NotNull(snapshot);

            Assert.Equal(2, snapshot.ProjectStatuses.Length);
            AssertProjectsSerializedCorrectly(snapshot);

            Assert.Equal(2, snapshot.QueueSetSnapshot.Queues.Count);
            QueueSnapshot queueSnapshot1 = snapshot.QueueSetSnapshot.Queues[0];
            Assert.Equal("Queue1", queueSnapshot1.QueueName);
            Assert.Equal("projectName", queueSnapshot1.Requests[0].ProjectName);
            Assert.Equal(ProjectActivity.CheckingModifications, queueSnapshot1.Requests[0].Activity);
            Assert.Equal("SVNTest", queueSnapshot1.Requests[1].ProjectName);
            Assert.Equal(ProjectActivity.Pending, queueSnapshot1.Requests[1].Activity);

            QueueSnapshot queueSnapshot2 = snapshot.QueueSetSnapshot.Queues[1];
            Assert.Equal("Queue2", queueSnapshot2.QueueName);
            Assert.Equal("Missing", queueSnapshot2.Requests[0].ProjectName);
            Assert.Equal(ProjectActivity.Building, queueSnapshot2.Requests[0].Activity);
        }
    }
}
