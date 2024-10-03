using System;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.Remote;
//using ThoughtWorks.CruiseControl.UnitTests.Core;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Monitoring
{
	public class ServerMonitorTest : IDisposable
	{
		private Mock<ICruiseServerManager> mockServerManager;
		private ServerMonitor monitor;
		private int pollCount;
		private int queueChangedCount;
        const string PROJECT_NAME = "projectName";

		//[SetUp]
		public void SetUp()
		{
			queueChangedCount = pollCount = 0;
			mockServerManager = new Mock<ICruiseServerManager>(MockBehavior.Strict);
			monitor = new ServerMonitor((ICruiseServerManager) mockServerManager.Object);
			monitor.Polled += new MonitorServerPolledEventHandler(Monitor_Polled);
			monitor.QueueChanged += new MonitorServerQueueChangedEventHandler(Monitor_QueueChanged);
		}

		
		public void Dispose()
		{
			mockServerManager.Verify();
		}

		[Fact]
        public void WhenPollIsCalledRetrievesANewCopyOfTheCruiseServerSnapshot()
		{
            CruiseServerSnapshot snapshot = new CruiseServerSnapshot();
            mockServerManager.Setup(_manager => _manager.GetCruiseServerSnapshot()).Returns(snapshot).Verifiable();

			monitor.Poll();

			// deliberately called twice: should not go back to server on 2nd call

            Assert.Same(snapshot, monitor.CruiseServerSnapshot);
            
            //Assert.Same(snapshot, monitor.CruiseServerSnapshot);
            Assert.Same(snapshot, monitor.CruiseServerSnapshot);
		}

		[Fact]
		public void ThePollEventIsFiredWhenPollIsInvoked()
		{
			Assert.Equal(0, pollCount);

            CruiseServerSnapshot snapshot = new CruiseServerSnapshot();
            mockServerManager.Setup(_manager => _manager.GetCruiseServerSnapshot()).Returns(snapshot).Verifiable();
			monitor.Poll();
			Assert.Equal(1, pollCount);

            mockServerManager.Setup(_manager => _manager.GetCruiseServerSnapshot()).Returns(snapshot).Verifiable();
			monitor.Poll();
			Assert.Equal(2, pollCount);
		}

		[Fact]
		public void WhenPollingEncountersAnExceptionThePolledEventIsStillFired()
		{
			Assert.Equal(0, pollCount);
			Exception ex = new Exception("should be caught");
            mockServerManager.Setup(_manager => _manager.GetCruiseServerSnapshot()).Throws(ex).Verifiable();
			monitor.Poll();
			Assert.Equal(1, pollCount);
			Assert.Equal(ex, monitor.ConnectException);
		}

		[Fact]
		public void IfTheQueueTimeStampHasChangedAQueueChangedEventIsFired()
		{
			Assert.Equal(0, queueChangedCount);
            CruiseServerSnapshot snapshot = CreateCruiseServerSnapshot();

            mockServerManager.Setup(_manager => _manager.GetCruiseServerSnapshot()).Returns(snapshot).Verifiable();
			monitor.Poll();

			Assert.Equal(1, queueChangedCount);

            mockServerManager.Setup(_manager => _manager.GetCruiseServerSnapshot()).Returns(snapshot).Verifiable();
			monitor.Poll();

			Assert.Equal(1, queueChangedCount);

            mockServerManager.Setup(_manager => _manager.GetCruiseServerSnapshot()).Returns(CreateCruiseServerSnapshot2()).Verifiable();
			monitor.Poll();

			Assert.Equal(2, queueChangedCount);

			mockServerManager.Setup(_manager => _manager.GetCruiseServerSnapshot()).Returns(CreateCruiseServerSnapshot()).Verifiable();
			monitor.Poll();

			Assert.Equal(3, queueChangedCount);
		}

		private void Monitor_Polled(object sauce, MonitorServerPolledEventArgs args)
		{
			pollCount++;
		}

		private void Monitor_QueueChanged(object sauce, MonitorServerQueueChangedEventArgs e)
		{
			queueChangedCount++;
		}

		private CruiseServerSnapshot CreateCruiseServerSnapshot()
		{
            return new CruiseServerSnapshot();
		}

        private CruiseServerSnapshot CreateCruiseServerSnapshot2()
        {
            CruiseServerSnapshot snapshot = new CruiseServerSnapshot();
            snapshot.QueueSetSnapshot.Queues.Add(new QueueSnapshot("Test"));
            snapshot.QueueSetSnapshot.Queues[0].Requests.Add(new QueuedRequestSnapshot("Project", ProjectActivity.CheckingModifications));
            return snapshot;
        }

        [Fact]
        public void ExposesTheCruiseServerSnapshotOfTheContainedServer()
		{
            CruiseServerSnapshot snapshot = new CruiseServerSnapshot();
            mockServerManager.Setup(_manager => _manager.GetCruiseServerSnapshot()).Returns(snapshot).Verifiable();

			monitor.Poll();

            Assert.Equal(snapshot, monitor.CruiseServerSnapshot);
		}

		[Fact]
        public void WhenNoConnectionHasBeenMadeToTheBuildServerTheCruiseServerSnapshotIsNull()
		{
            Assert.Equal(null, monitor.CruiseServerSnapshot);			
		}

        [Fact]
        public void ProjectStatusNullIfServerNotYetPolled()
        {
            ProjectStatus projectStatus = monitor.GetProjectStatus(PROJECT_NAME);

            Assert.Null(projectStatus);
        }

        [Fact]
        public void ProjectStatusReturnsTheStatusForTheNominatedProject()
        {
            ProjectStatus[] result = new ProjectStatus[]
				{
					CreateProjectStatus("a name"),
					CreateProjectStatus(PROJECT_NAME),
				};

            CruiseServerSnapshot snapshot = new CruiseServerSnapshot(result, null);
            mockServerManager.Setup(_manager => _manager.GetCruiseServerSnapshot()).Returns(snapshot).Verifiable();

            monitor.Poll(); // Force the snapshot to be loaded
            ProjectStatus projectStatus = monitor.GetProjectStatus(PROJECT_NAME);

            Assert.Same(result[1], projectStatus);
        }

        [Fact]
        //[ExpectedException(typeof(ApplicationException), )]
        public void ProjectStatusThrowsIfProjectNotFound()
        {
            ProjectStatus[] result = new ProjectStatus[]
                {
                    CreateProjectStatus("a name"),
                    CreateProjectStatus("another name"),
            };

            CruiseServerSnapshot snapshot = new CruiseServerSnapshot(result, null);
            mockServerManager.Setup(_manager => _manager.GetCruiseServerSnapshot()).Returns(snapshot).Verifiable();

            monitor.Poll();// Force the snapshot to be loaded 

            Assert.Equal("Project 'projectName' not found on server",Assert.Throws<ApplicationException>(delegate { monitor.GetProjectStatus(PROJECT_NAME); }).Message);
        }

        private static ProjectStatus CreateProjectStatus(string projectName)
        {
            throw new NotImplementedException();
            //return ProjectStatusFixture.New(projectName);
        }
    }
}
