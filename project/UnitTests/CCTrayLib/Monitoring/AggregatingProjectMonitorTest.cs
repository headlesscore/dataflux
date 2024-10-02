using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.CCTrayLib;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.CCTrayLib.Presentation;
using ThoughtWorks.CruiseControl.Remote;
using ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Presentation;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Monitoring
{
	[TestFixture]
	public class AggregatingProjectMonitorTest
	{
		private Mock<IProjectMonitor> monitor1;
		private Mock<IProjectMonitor> monitor2;
		private Mock<IProjectMonitor> monitor3;
		private IProjectMonitor[] monitors;
		private AggregatingProjectMonitor aggregator;

		[SetUp]
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

		[TearDown]
		public void TearDown()
		{
			monitor1.Verify();
			monitor2.Verify();
			monitor3.Verify();
		}

		[Test]
		public void PollInvokesPollOnAllContainedProjects()
		{
			monitor1.Setup(_monitor => _monitor.Poll()).Verifiable();
			monitor2.Setup(_monitor => _monitor.Poll()).Verifiable();
			monitor3.Setup(_monitor => _monitor.Poll()).Verifiable();
			aggregator.Poll();
		}

		[Test]
		public void ThrowsWhenAttemptingToRetrieveSingleProjectDetail()
		{
		    ClassicAssert.That(delegate { ISingleProjectDetail detail = aggregator.Detail; },
		                Throws.TypeOf<InvalidOperationException>());
		}

		private int buildOccurredCount;
		private MonitorBuildOccurredEventArgs lastBuildOccurredEventArgs;

		[Test]
		public void BuildOccuredIsFiredWheneverAnyContainedProjectStatusFiresIt()
		{
			buildOccurredCount = 0;
			lastBuildOccurredEventArgs = null;

			StubProjectMonitor stubProjectMonitor1 = new StubProjectMonitor("project1");
			StubProjectMonitor stubProjectMonitor2 = new StubProjectMonitor("project2");

			aggregator = new AggregatingProjectMonitor(stubProjectMonitor1, stubProjectMonitor2);
			aggregator.BuildOccurred += new MonitorBuildOccurredEventHandler(Aggregator_BuildOccurred);
            
            ClassicAssert.AreEqual(0, buildOccurredCount);
			stubProjectMonitor1.OnBuildOccurred(new MonitorBuildOccurredEventArgs(stubProjectMonitor1, BuildTransition.Fixed));

			ClassicAssert.AreEqual(1, buildOccurredCount);
			ClassicAssert.AreSame(stubProjectMonitor1, lastBuildOccurredEventArgs.ProjectMonitor);
			ClassicAssert.AreEqual(BuildTransition.Fixed, lastBuildOccurredEventArgs.BuildTransition);
		}

		private void Aggregator_BuildOccurred(object sauce, MonitorBuildOccurredEventArgs e)
		{
			buildOccurredCount++;
			lastBuildOccurredEventArgs = e;
		}

		private int pollCount;
		private object lastPolledSource;
		private MonitorPolledEventArgs lastPolledArgs;

		[Test]
		public void PolledIsFiredWheneverAnyContainedProjectStatusFiresIt()
		{
			pollCount = 0;

			StubProjectMonitor stubProjectMonitor1 = new StubProjectMonitor("project1");
			StubProjectMonitor stubProjectMonitor2 = new StubProjectMonitor("project2");

			aggregator = new AggregatingProjectMonitor(stubProjectMonitor1, stubProjectMonitor2);
			aggregator.Polled += new MonitorPolledEventHandler(Aggregator_Polled);

			ClassicAssert.AreEqual(0, pollCount);
			stubProjectMonitor1.OnPolled(new MonitorPolledEventArgs(stubProjectMonitor1));

			ClassicAssert.AreEqual(1, pollCount);
		}

		private void Aggregator_Polled(object source, MonitorPolledEventArgs args)
		{
			pollCount++;
			lastPolledSource = source;
			lastPolledArgs = args;
		}


		[Test]
		public void WhenPolledIsFiredTheSourcePointToTheAggregatorNotTheFiringProject()
		{
			StubProjectMonitor stubProjectMonitor1 = new StubProjectMonitor("project1");

			aggregator = new AggregatingProjectMonitor(stubProjectMonitor1);
			aggregator.Polled += new MonitorPolledEventHandler(Aggregator_Polled);

			stubProjectMonitor1.OnPolled(new MonitorPolledEventArgs(stubProjectMonitor1));

			ClassicAssert.AreSame(lastPolledSource, aggregator);
			ClassicAssert.AreSame(lastPolledArgs.ProjectMonitor, stubProjectMonitor1);
		}


		[Test]
		public void ProjectStateReturnsTheWorstStateOfAllMonitors()
		{
			// so the states, most significant first, are:
			//  Broken
			//  Building
			//  NotConnected
			//  Success

			ClassicAssert.AreEqual(ProjectState.Success, CombinedState(ProjectState.Success, ProjectState.Success, ProjectState.Success));
			ClassicAssert.AreEqual(ProjectState.Building,
			                CombinedState(ProjectState.Success, ProjectState.Building, ProjectState.Success));
			ClassicAssert.AreEqual(ProjectState.Building,
			                CombinedState(ProjectState.Building, ProjectState.Success, ProjectState.NotConnected));
			ClassicAssert.AreEqual(ProjectState.NotConnected,
			                CombinedState(ProjectState.Success, ProjectState.Success, ProjectState.NotConnected));
			ClassicAssert.AreEqual(ProjectState.Broken,
			                CombinedState(ProjectState.NotConnected, ProjectState.Success, ProjectState.Broken));
			ClassicAssert.AreEqual(ProjectState.Broken, CombinedState(ProjectState.Broken, ProjectState.Building, ProjectState.Success));
			ClassicAssert.AreEqual(ProjectState.Broken,
			                CombinedState(ProjectState.Success, ProjectState.Broken, ProjectState.NotConnected));
		}

		private ProjectState CombinedState(ProjectState state1, ProjectState state2, ProjectState state3)
		{
			monitor1.SetupGet(_monitor => _monitor.ProjectState).Returns(state1).Verifiable();
			monitor2.SetupGet(_monitor => _monitor.ProjectState).Returns(state2).Verifiable();
			monitor3.SetupGet(_monitor => _monitor.ProjectState).Returns(state3).Verifiable();

			return aggregator.ProjectState;
		}

		[Test]
		public void ProjectSummaryStringCombinesAllStringsWithNewLinesBetween()
		{
			monitor1.SetupGet(_monitor => _monitor.SummaryStatusString).Returns("hello from monitor1").Verifiable();
			monitor2.SetupGet(_monitor => _monitor.SummaryStatusString).Returns("and from monitor2").Verifiable();
			monitor3.SetupGet(_monitor => _monitor.SummaryStatusString).Returns("goodbye from monitor3").Verifiable();
			string statusString = aggregator.SummaryStatusString;

			ClassicAssert.AreEqual("hello from monitor1\nand from monitor2\ngoodbye from monitor3", statusString);
		}

		[Test]
		public void ProjectSummaryStringDoesNotIncludeBlankLinesWhenAProjectReturnsNothing()
		{
			monitor1.SetupGet(_monitor => _monitor.SummaryStatusString).Returns("hello from monitor1").Verifiable();
			monitor2.SetupGet(_monitor => _monitor.SummaryStatusString).Returns(string.Empty).Verifiable();
			monitor3.SetupGet(_monitor => _monitor.SummaryStatusString).Returns("goodbye from monitor3").Verifiable();
			string statusString = aggregator.SummaryStatusString;

			ClassicAssert.AreEqual("hello from monitor1\ngoodbye from monitor3", statusString);
		}

		[Test]
		public void ProjectSummaryStringReturnsADefaultMessageIfAllProjectsReturnEmptyString()
		{
			monitor1.SetupGet(_monitor => _monitor.SummaryStatusString).Returns(string.Empty).Verifiable();
			monitor2.SetupGet(_monitor => _monitor.SummaryStatusString).Returns(string.Empty).Verifiable();
			monitor3.SetupGet(_monitor => _monitor.SummaryStatusString).Returns(string.Empty).Verifiable();
			string statusString = aggregator.SummaryStatusString;

			ClassicAssert.AreEqual("All builds are good", statusString);
		}

		[Test]
		public void IntegrationResultReturnsTheWorstResultOfAllMonitors()
		{
			// so the states, most significant first, are:
			//  Failure
			//  Exception
			//  Unknown
			//  Success

			ClassicAssert.AreEqual(IntegrationStatus.Success,
			                CombinedIntegrationStatus(IntegrationStatus.Success, IntegrationStatus.Success, IntegrationStatus.Success));
			ClassicAssert.AreEqual(IntegrationStatus.Unknown,
			                CombinedIntegrationStatus(IntegrationStatus.Success, IntegrationStatus.Success, IntegrationStatus.Unknown));
			ClassicAssert.AreEqual(IntegrationStatus.Exception,
			                CombinedIntegrationStatus(IntegrationStatus.Success, IntegrationStatus.Exception, IntegrationStatus.Success));
			ClassicAssert.AreEqual(IntegrationStatus.Exception,
			                CombinedIntegrationStatus(IntegrationStatus.Success, IntegrationStatus.Exception, IntegrationStatus.Unknown));
			ClassicAssert.AreEqual(IntegrationStatus.Failure,
			                CombinedIntegrationStatus(IntegrationStatus.Failure, IntegrationStatus.Exception, IntegrationStatus.Success));
			ClassicAssert.AreEqual(IntegrationStatus.Failure,
			                CombinedIntegrationStatus(IntegrationStatus.Failure, IntegrationStatus.Success, IntegrationStatus.Success));
		}

		private IntegrationStatus CombinedIntegrationStatus(IntegrationStatus state1, IntegrationStatus state2, IntegrationStatus state3)
		{
			monitor1.SetupGet(_monitor => _monitor.IntegrationStatus).Returns(state1).Verifiable();
			monitor2.SetupGet(_monitor => _monitor.IntegrationStatus).Returns(state2).Verifiable();
			monitor3.SetupGet(_monitor => _monitor.IntegrationStatus).Returns(state3).Verifiable();

			return aggregator.IntegrationStatus;
		}

        //[Test]
        //[ExpectedException(typeof(NotImplementedException))]
        //public void ForceBuildThrowsAnNotImplementedException()
        //{
        //    Dictionary<string, string> parameters = new Dictionary<string, string>();
        //    aggregator.ForceBuild(parameters);
        //}

        //[Test]
        //[ExpectedException(typeof(NotImplementedException))]
        //public void FixBuildThrowsAnNotImplementedException()
        //{
        //    aggregator.FixBuild("JoeSmith");
        //}

        //[Test]
        //[ExpectedException(typeof(NotImplementedException))]
        //public void CancelPendingThrowsAnNotImplementedException()
        //{
        //    aggregator.CancelPending();
        //}        
    }
}
