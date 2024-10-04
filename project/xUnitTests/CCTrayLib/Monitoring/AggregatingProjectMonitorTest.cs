using System;
using System.Collections.Generic;
using Moq;
using Xunit;
using ThoughtWorks.CruiseControl.CCTrayLib;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.CCTrayLib.Presentation;
using ThoughtWorks.CruiseControl.Remote;
using ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Presentation;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Monitoring
{
    public class TestsFixture : IDisposable
    {
        public TestsFixture() {
        }
        public void Dispose() {

        }
    }
    public class AggregatingProjectMonitorTest : IClassFixture<TestsFixture>, IDisposable
    {
        private Mock<IProjectMonitor> monitor1;
        private Mock<IProjectMonitor> monitor2;
        private Mock<IProjectMonitor> monitor3;
        private IProjectMonitor[] monitors;
        private AggregatingProjectMonitor aggregator;

        //// [SetUp]
        public void SetUp()
        {
            monitor1 = new Mock<IProjectMonitor>();
            monitor2 = new Mock<IProjectMonitor>();
            monitor3 = new Mock<IProjectMonitor>();

            monitors = new IProjectMonitor[]
                {
                    (IProjectMonitor) monitor1.Object,
                    (IProjectMonitor) monitor2.Object,
                    (IProjectMonitor) monitor3.Object,
                };

            aggregator = new AggregatingProjectMonitor(monitors);
        }
        public AggregatingProjectMonitorTest(TestsFixture fixture)
        {

        }

        [Fact]
        public void PollInvokesPollOnAllContainedProjects()
        {
            monitor1.Setup(_monitor => _monitor.Poll()).Verifiable();
            monitor2.Setup(_monitor => _monitor.Poll()).Verifiable();
            monitor3.Setup(_monitor => _monitor.Poll()).Verifiable();
            aggregator.Poll();
        }

        [Fact]
        public void ThrowsWhenAttemptingToRetrieveSingleProjectDetail()
        {
            Assert.Throws<InvalidOperationException>(delegate { ISingleProjectDetail detail = aggregator.Detail; });
        }

        private int buildOccurredCount;
        private MonitorBuildOccurredEventArgs lastBuildOccurredEventArgs;

        [Fact]
        public void BuildOccuredIsFiredWheneverAnyContainedProjectStatusFiresIt()
        {
            buildOccurredCount = 0;
            lastBuildOccurredEventArgs = null;

            StubProjectMonitor stubProjectMonitor1 = new StubProjectMonitor("project1");
            StubProjectMonitor stubProjectMonitor2 = new StubProjectMonitor("project2");

            aggregator = new AggregatingProjectMonitor(stubProjectMonitor1, stubProjectMonitor2);
            aggregator.BuildOccurred += new MonitorBuildOccurredEventHandler(Aggregator_BuildOccurred);

            Assert.Equal(0, buildOccurredCount);
            stubProjectMonitor1.OnBuildOccurred(new MonitorBuildOccurredEventArgs(stubProjectMonitor1, BuildTransition.Fixed));

            Assert.Equal(1, buildOccurredCount);
            Assert.Same(stubProjectMonitor1, lastBuildOccurredEventArgs.ProjectMonitor);
            Assert.Equal(BuildTransition.Fixed, lastBuildOccurredEventArgs.BuildTransition);
        }

        private void Aggregator_BuildOccurred(object sauce, MonitorBuildOccurredEventArgs e)
        {
            buildOccurredCount++;
            lastBuildOccurredEventArgs = e;
        }

        private int pollCount;
        private object lastPolledSource;
        private MonitorPolledEventArgs lastPolledArgs;

        [Fact]
        public void PolledIsFiredWheneverAnyContainedProjectStatusFiresIt()
        {
            pollCount = 0;

            StubProjectMonitor stubProjectMonitor1 = new StubProjectMonitor("project1");
            StubProjectMonitor stubProjectMonitor2 = new StubProjectMonitor("project2");

            aggregator = new AggregatingProjectMonitor(stubProjectMonitor1, stubProjectMonitor2);
            aggregator.Polled += new MonitorPolledEventHandler(Aggregator_Polled);

            Assert.Equal(0, pollCount);
            stubProjectMonitor1.OnPolled(new MonitorPolledEventArgs(stubProjectMonitor1));

            Assert.Equal(1, pollCount);
        }

        private void Aggregator_Polled(object source, MonitorPolledEventArgs args)
        {
            pollCount++;
            lastPolledSource = source;
            lastPolledArgs = args;
        }


        [Fact]
        public void WhenPolledIsFiredTheSourcePointToTheAggregatorNotTheFiringProject()
        {
            StubProjectMonitor stubProjectMonitor1 = new StubProjectMonitor("project1");

            aggregator = new AggregatingProjectMonitor(stubProjectMonitor1);
            aggregator.Polled += new MonitorPolledEventHandler(Aggregator_Polled);

            stubProjectMonitor1.OnPolled(new MonitorPolledEventArgs(stubProjectMonitor1));

            Assert.Same(lastPolledSource, aggregator);
            Assert.Same(lastPolledArgs.ProjectMonitor, stubProjectMonitor1);
        }


        [Fact]
        public void ProjectStateReturnsTheWorstStateOfAllMonitors()
        {
            // so the states, most significant first, are:
            //  Broken
            //  Building
            //  NotConnected
            //  Success

            Assert.Equal(ProjectState.Success, CombinedState(ProjectState.Success, ProjectState.Success, ProjectState.Success));
            Assert.Equal(ProjectState.Building,
                            CombinedState(ProjectState.Success, ProjectState.Building, ProjectState.Success));
            Assert.Equal(ProjectState.Building,
                            CombinedState(ProjectState.Building, ProjectState.Success, ProjectState.NotConnected));
            Assert.Equal(ProjectState.NotConnected,
                            CombinedState(ProjectState.Success, ProjectState.Success, ProjectState.NotConnected));
            Assert.Equal(ProjectState.Broken,
                            CombinedState(ProjectState.NotConnected, ProjectState.Success, ProjectState.Broken));
            Assert.Equal(ProjectState.Broken, CombinedState(ProjectState.Broken, ProjectState.Building, ProjectState.Success));
            Assert.Equal(ProjectState.Broken,
                            CombinedState(ProjectState.Success, ProjectState.Broken, ProjectState.NotConnected));
        }

        private ProjectState CombinedState(ProjectState state1, ProjectState state2, ProjectState state3)
        {
            monitor1.SetupGet(_monitor => _monitor.ProjectState).Returns(state1).Verifiable();
            monitor2.SetupGet(_monitor => _monitor.ProjectState).Returns(state2).Verifiable();
            monitor3.SetupGet(_monitor => _monitor.ProjectState).Returns(state3).Verifiable();

            return aggregator.ProjectState;
        }

        [Fact]
        public void ProjectSummaryStringCombinesAllStringsWithNewLinesBetween()
        {
            monitor1.SetupGet(_monitor => _monitor.SummaryStatusString).Returns("hello from monitor1").Verifiable();
            monitor2.SetupGet(_monitor => _monitor.SummaryStatusString).Returns("and from monitor2").Verifiable();
            monitor3.SetupGet(_monitor => _monitor.SummaryStatusString).Returns("goodbye from monitor3").Verifiable();
            string statusString = aggregator.SummaryStatusString;

            Assert.Equal("hello from monitor1\nand from monitor2\ngoodbye from monitor3", statusString);
        }

        [Fact]
        public void ProjectSummaryStringDoesNotIncludeBlankLinesWhenAProjectReturnsNothing()
        {
            monitor1.SetupGet(_monitor => _monitor.SummaryStatusString).Returns("hello from monitor1").Verifiable();
            monitor2.SetupGet(_monitor => _monitor.SummaryStatusString).Returns(string.Empty).Verifiable();
            monitor3.SetupGet(_monitor => _monitor.SummaryStatusString).Returns("goodbye from monitor3").Verifiable();
            string statusString = aggregator.SummaryStatusString;

            Assert.Equal("hello from monitor1\ngoodbye from monitor3", statusString);
        }

        [Fact]
        public void ProjectSummaryStringReturnsADefaultMessageIfAllProjectsReturnEmptyString()
        {
            monitor1.SetupGet(_monitor => _monitor.SummaryStatusString).Returns(string.Empty).Verifiable();
            monitor2.SetupGet(_monitor => _monitor.SummaryStatusString).Returns(string.Empty).Verifiable();
            monitor3.SetupGet(_monitor => _monitor.SummaryStatusString).Returns(string.Empty).Verifiable();
            string statusString = aggregator.SummaryStatusString;

            Assert.Equal("All builds are good", statusString);
        }

        [Fact]
        public void IntegrationResultReturnsTheWorstResultOfAllMonitors()
        {
            // so the states, most significant first, are:
            //  Failure
            //  Exception
            //  Unknown
            //  Success

            Assert.Equal(IntegrationStatus.Success,
                            CombinedIntegrationStatus(IntegrationStatus.Success, IntegrationStatus.Success, IntegrationStatus.Success));
            Assert.Equal(IntegrationStatus.Unknown,
                            CombinedIntegrationStatus(IntegrationStatus.Success, IntegrationStatus.Success, IntegrationStatus.Unknown));
            Assert.Equal(IntegrationStatus.Exception,
                            CombinedIntegrationStatus(IntegrationStatus.Success, IntegrationStatus.Exception, IntegrationStatus.Success));
            Assert.Equal(IntegrationStatus.Exception,
                            CombinedIntegrationStatus(IntegrationStatus.Success, IntegrationStatus.Exception, IntegrationStatus.Unknown));
            Assert.Equal(IntegrationStatus.Failure,
                            CombinedIntegrationStatus(IntegrationStatus.Failure, IntegrationStatus.Exception, IntegrationStatus.Success));
            Assert.Equal(IntegrationStatus.Failure,
                            CombinedIntegrationStatus(IntegrationStatus.Failure, IntegrationStatus.Success, IntegrationStatus.Success));
        }

        private IntegrationStatus CombinedIntegrationStatus(IntegrationStatus state1, IntegrationStatus state2, IntegrationStatus state3)
        {
            monitor1.SetupGet(_monitor => _monitor.IntegrationStatus).Returns(state1).Verifiable();
            monitor2.SetupGet(_monitor => _monitor.IntegrationStatus).Returns(state2).Verifiable();
            monitor3.SetupGet(_monitor => _monitor.IntegrationStatus).Returns(state3).Verifiable();

            return aggregator.IntegrationStatus;
        }

        public void Dispose() {
            monitor1.Verify();
            monitor2.Verify();
            monitor3.Verify();
        }

        //[Fact]
        //[ExpectedException(typeof(NotImplementedException))]
        //public void ForceBuildThrowsAnNotImplementedException()
        //{
        //    Dictionary<string, string> parameters = new Dictionary<string, string>();
        //    aggregator.ForceBuild(parameters);
        //}

        //[Fact]
        //[ExpectedException(typeof(NotImplementedException))]
        //public void FixBuildThrowsAnNotImplementedException()
        //{
        //    aggregator.FixBuild("JoeSmith");
        //}

        //[Fact]
        //[ExpectedException(typeof(NotImplementedException))]
        //public void CancelPendingThrowsAnNotImplementedException()
        //{
        //    aggregator.CancelPending();
        //}        
    }
}
