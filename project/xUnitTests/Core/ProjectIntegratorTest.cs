using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Moq;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Config;
using ThoughtWorks.CruiseControl.Core.Queues;
using ThoughtWorks.CruiseControl.Remote;
using ThoughtWorks.CruiseControl.Remote.Events;
using ThoughtWorks.CruiseControl.UnitTests.UnitTestUtils;
using Xunit;

namespace ThoughtWorks.CruiseControl.UnitTests.Core
{
	public class ProjectIntegratorTest : IntegrationFixture
	{
		private const string TestQueueName = "projectQueue";
	    private static readonly string tempDir = Path.GetTempPath() + Assembly.GetExecutingAssembly().FullName + "\\";
		private Mock<ITrigger> integrationTriggerMock;
		private Mock<IProject> projectMock;
		private ProjectIntegrator integrator;
		private IntegrationQueueSet integrationQueues;
		private IIntegrationQueue integrationQueue;

        private readonly string tempWorkingDir1 = tempDir + "tempWorkingDir1";
        private readonly string tempArtifactDir1 = tempDir + "tempArtifactDir1";

		//// [SetUp]
		public void SetUp()
		{
			integrationTriggerMock = new Mock<ITrigger>(MockBehavior.Strict);
			projectMock = new Mock<IProject>(MockBehavior.Strict);
			projectMock.SetupGet(_project => _project.Name).Returns("project");
			projectMock.SetupGet(_project => _project.QueueName).Returns(TestQueueName);
			projectMock.SetupGet(_project => _project.QueuePriority).Returns(0);
			projectMock.SetupGet(_project => _project.Triggers).Returns(integrationTriggerMock.Object);
            projectMock.SetupGet(_project => _project.WorkingDirectory).Returns(tempWorkingDir1);
            projectMock.SetupGet(_project => _project.ArtifactDirectory).Returns(tempArtifactDir1);

			integrationQueues = new IntegrationQueueSet();
            integrationQueues.Add(TestQueueName, new DefaultQueueConfiguration(TestQueueName));
			integrationQueue = integrationQueues[TestQueueName];
			integrator = new ProjectIntegrator((IProject) projectMock.Object, integrationQueue);
		}

		//// [TearDown]
		public void TearDown()
		{
			if (integrator != null)
			{
				integrator.Stop(false);
				integrator.WaitForExit();
                (integrator as IDisposable).Dispose();
            }
            if (Directory.Exists(tempWorkingDir1))
                Directory.Delete(tempWorkingDir1, true);
            if (Directory.Exists(tempArtifactDir1))
                Directory.Delete(tempArtifactDir1, true);
        }

		private void VerifyAll()
		{
			integrationTriggerMock.Verify();
			projectMock.Verify();
		}

		[Fact]
		public void ShouldContinueRunningIfNotToldToStop()
		{
			LatchHelper latchHelper = new LatchHelper();
			integrationTriggerMock.Setup(_trigger => _trigger.Fire()).Callback(() => latchHelper.SetLatch()).Returns(() => null);

			integrator.Start();
			latchHelper.WaitForSignal();
			Assert.Equal(ProjectIntegratorState.Running, integrator.State);
            Assert.Equal(ProjectIntegratorState.Running, integrator.State);
            projectMock.Verify(project => project.Integrate(It.IsAny<IntegrationRequest>()), Times.Never);
			integrationTriggerMock.Verify(_trigger => _trigger.IntegrationCompleted(), Times.Never);
			VerifyAll();
		}

		[Fact]
		public void ShouldStopWhenStoppedExternally()
		{
			LatchHelper latchHelper = new LatchHelper();
			integrationTriggerMock.Setup(_trigger => _trigger.Fire()).Callback(() => latchHelper.SetLatch()).Returns(() => null);

			integrator.Start();
			latchHelper.WaitForSignal();
			Assert.Equal(ProjectIntegratorState.Running, integrator.State);

			integrator.Stop(false);
			integrator.WaitForExit();
			Assert.Equal(ProjectIntegratorState.Stopped, integrator.State);
			projectMock.Verify(project => project.NotifyPendingState(), Times.Never);
			projectMock.Verify(project => project.Integrate(It.IsAny<IntegrationRequest>()), Times.Never);
			projectMock.Verify(project => project.NotifySleepingState(), Times.Never);
			integrationTriggerMock.Verify(_trigger => _trigger.IntegrationCompleted(), Times.Never);
			VerifyAll();
		}

		[Fact]
		public void StartMultipleTimes()
		{
			LatchHelper latchHelper = new LatchHelper();
			integrationTriggerMock.Setup(_trigger => _trigger.Fire()).Callback(() => latchHelper.SetLatch()).Returns(() => null);

			integrator.Start();
			integrator.Start();
			integrator.Start();
			latchHelper.WaitForSignal();
			Assert.Equal(ProjectIntegratorState.Running, integrator.State);
			integrator.Stop(false);
			integrator.WaitForExit();
			Assert.Equal(ProjectIntegratorState.Stopped, integrator.State);
			projectMock.Verify(project => project.NotifyPendingState(), Times.Never);
			projectMock.Verify(project => project.Integrate(It.IsAny<IntegrationRequest>()), Times.Never);
			projectMock.Verify(project => project.NotifySleepingState(), Times.Never);
			integrationTriggerMock.Verify(_trigger => _trigger.IntegrationCompleted(), Times.Never);
			VerifyAll();
		}

		[Fact]
		public void RestartIntegrator()
		{
			LatchHelper latchHelper = new LatchHelper();
			integrationTriggerMock.Setup(_trigger => _trigger.Fire()).Callback(() => latchHelper.SetLatch()).Returns(() => null);

			integrator.Start();
			latchHelper.WaitForSignal();
			integrator.Stop(false);
			integrator.WaitForExit();

			latchHelper.ResetLatch();
			integrator.Start();
			latchHelper.WaitForSignal();
			integrator.Stop(false);
			integrator.WaitForExit();
			projectMock.Verify(project => project.NotifyPendingState(), Times.Never);
			projectMock.Verify(project => project.Integrate(It.IsAny<IntegrationRequest>()), Times.Never);
			projectMock.Verify(project => project.NotifySleepingState(), Times.Never);
			integrationTriggerMock.Verify(_trigger => _trigger.IntegrationCompleted(), Times.Never);
			VerifyAll();
		}

		[Fact]
		public void StopUnstartedIntegrator()
		{
			integrator.Stop(false);
			Assert.Equal(ProjectIntegratorState.Stopping, integrator.State);

			projectMock.Verify(project => project.NotifyPendingState(), Times.Never);
			projectMock.Verify(project => project.Integrate(It.IsAny<IntegrationRequest>()), Times.Never);
			projectMock.Verify(project => project.NotifySleepingState(), Times.Never);
			integrationTriggerMock.Verify(_trigger => _trigger.Fire(), Times.Never);
			integrationTriggerMock.Verify(_trigger => _trigger.IntegrationCompleted(), Times.Never);
			VerifyAll();
		}

		[Fact]
		public void VerifyStateAfterException()
		{
			string exceptionMessage = "Intentional exception";

			integrationTriggerMock.Setup(_trigger => _trigger.Fire()).Returns(ForceBuildRequest()).Verifiable();
			projectMock.Setup(project => project.NotifyPendingState()).Verifiable();
			projectMock.Setup(project => project.Integrate(It.Is<IntegrationRequest>(r => r.BuildCondition == BuildCondition.ForceBuild))).Throws(new CruiseControlException(exceptionMessage)).Verifiable();
			LatchHelper latchHelper = new LatchHelper();
			projectMock.Setup(project => project.NotifySleepingState()).Callback(() => latchHelper.SetLatch()).Verifiable();
            projectMock.SetupGet(project => project.MaxSourceControlRetries).Returns(5);
            projectMock.SetupGet(project => project.SourceControlErrorHandling).Returns(ThoughtWorks.CruiseControl.Core.Sourcecontrol.Common.SourceControlErrorHandlingPolicy.ReportEveryFailure);
            integrationTriggerMock.Setup(_trigger => _trigger.IntegrationCompleted()).Verifiable();

			integrator.Start();
			latchHelper.WaitForSignal();
			Assert.Equal(ProjectIntegratorState.Running, integrator.State);
			integrator.Stop(false);
			integrator.WaitForExit();
			Assert.Equal(ProjectIntegratorState.Stopped, integrator.State);
			VerifyAll();
		}

		[Fact]
		public void Abort()
		{
			LatchHelper latchHelper = new LatchHelper();
			integrationTriggerMock.Setup(_trigger => _trigger.Fire()).Callback(() => latchHelper.SetLatch()).Returns(() => null);

			integrator.Start();
			latchHelper.WaitForSignal();
			Assert.Equal(ProjectIntegratorState.Running, integrator.State);
			integrator.Abort();
			integrator.WaitForExit();
			Assert.Equal(ProjectIntegratorState.Stopped, integrator.State);
			projectMock.Verify(project => project.NotifyPendingState(), Times.Never);
			projectMock.Verify(project => project.Integrate(It.IsAny<IntegrationRequest>()), Times.Never);
			projectMock.Verify(project => project.NotifySleepingState(), Times.Never);
			integrationTriggerMock.Verify(_trigger => _trigger.IntegrationCompleted(), Times.Never);
			VerifyAll();
		}

		[Fact]
		public void TerminateWhenProjectIsntStarted()
		{
			LatchHelper latchHelper = new LatchHelper();
			integrationTriggerMock.Setup(_trigger => _trigger.Fire()).Callback(() => latchHelper.SetLatch()).Returns(() => null);

			integrator.Abort();
			Assert.Equal(ProjectIntegratorState.Unknown, integrator.State);
			projectMock.Verify(project => project.NotifyPendingState(), Times.Never);
			projectMock.Verify(project => project.Integrate(It.IsAny<IntegrationRequest>()), Times.Never);
			projectMock.Verify(project => project.NotifySleepingState(), Times.Never);
			integrationTriggerMock.Verify(_trigger => _trigger.IntegrationCompleted(), Times.Never);
			VerifyAll();
		}

		[Fact]
		public void TerminateCalledTwice()
		{
			LatchHelper latchHelper = new LatchHelper();
			integrationTriggerMock.Setup(_trigger => _trigger.Fire()).Callback(() => latchHelper.SetLatch()).Returns(() => null);

			integrator.Start();
			latchHelper.WaitForSignal();
			Assert.Equal(ProjectIntegratorState.Running, integrator.State);
			integrator.Abort();
			integrator.Abort();
			projectMock.Verify(project => project.NotifyPendingState(), Times.Never);
			projectMock.Verify(project => project.Integrate(It.IsAny<IntegrationRequest>()), Times.Never);
			projectMock.Verify(project => project.NotifySleepingState(), Times.Never);
			integrationTriggerMock.Verify(_trigger => _trigger.IntegrationCompleted(), Times.Never);
			VerifyAll();
		}

		[Fact(Skip = "can not get to work consistently, is handled in integrationtests")]
		public void ForceBuild()
		{
			LatchHelper projectLatchHelper = new LatchHelper();
			projectMock.Setup(project => project.Integrate(It.Is<IntegrationRequest>(r => r.BuildCondition == BuildCondition.ForceBuild))).Verifiable();
			projectMock.Setup(project => project.NotifyPendingState()).Verifiable();
			projectMock.Setup(project => project.NotifySleepingState()).Callback(() => projectLatchHelper.SetLatch()).Verifiable();
            projectMock.SetupGet(project => project.MaxSourceControlRetries).Returns(5);
            projectMock.SetupGet(project => project.SourceControlErrorHandling).Returns(ThoughtWorks.CruiseControl.Core.Sourcecontrol.Common.SourceControlErrorHandlingPolicy.ReportEveryFailure);
		    integrator.Start();
            
            LatchHelper integrationTriggerLatchHelper = new LatchHelper();
            integrationTriggerMock.Setup(_trigger => _trigger.IntegrationCompleted()).Callback(() => integrationTriggerLatchHelper.SetLatch()).Verifiable();
            var parameters = new Dictionary<string, string>();
            integrator.ForceBuild("BuildForcer", parameters);
			integrationTriggerLatchHelper.WaitForSignal();
			projectLatchHelper.WaitForSignal();
			VerifyAll();
			projectMock.VerifyNoOtherCalls();
			integrationTriggerMock.VerifyNoOtherCalls();
		}

		[Fact(Skip ="can not get to work consistently")]
		public void RequestIntegration()
		{
            IntegrationRequest request = new IntegrationRequest(BuildCondition.IfModificationExists, "intervalTrigger", null);
			
			LatchHelper projectLatchHelper = new LatchHelper();
            projectMock.Setup(project => project.NotifyPendingState()).Verifiable();
			projectMock.Setup(project => project.Integrate(request)).Verifiable();
			projectMock.Setup(project => project.NotifySleepingState()).Callback(() => projectLatchHelper.SetLatch()).Verifiable();
            projectMock.SetupGet(project => project.MaxSourceControlRetries).Returns(5);
            projectMock.SetupGet(project => project.SourceControlErrorHandling).Returns(ThoughtWorks.CruiseControl.Core.Sourcecontrol.Common.SourceControlErrorHandlingPolicy.ReportEveryFailure);
            LatchHelper integrationTriggerLatchHelper = new LatchHelper();
            integrationTriggerMock.Setup(_trigger => _trigger.IntegrationCompleted()).Callback(() => integrationTriggerLatchHelper.SetLatch()).Verifiable();
		    integrator.Start();
            integrator.Request(request);
			integrationTriggerLatchHelper.WaitForSignal();
			projectLatchHelper.WaitForSignal();
			Assert.Equal(ProjectIntegratorState.Running, integrator.State);
			VerifyAll();
		}

		[Fact(Skip ="Works on devPc, fails on ccnetlive, works on DNa buildserver, anybody want to take a shot?")]
		public void ShouldClearRequestQueueAsSoonAsRequestIsProcessed()
		{
            IntegrationRequest request = new IntegrationRequest(BuildCondition.IfModificationExists, "intervalTrigger", null);
			LatchHelper projectLatchHelper = new LatchHelper();
			projectMock.Setup(project => project.NotifyPendingState()).Verifiable();
			projectMock.Setup(project => project.Integrate(request)).Verifiable();
			projectMock.Setup(project => project.NotifySleepingState()).Callback(() => projectLatchHelper.SetLatch()).Verifiable();
            projectMock.SetupGet(project => project.MaxSourceControlRetries).Returns(5);
            projectMock.SetupGet(project => project.SourceControlErrorHandling).Returns(ThoughtWorks.CruiseControl.Core.Sourcecontrol.Common.SourceControlErrorHandlingPolicy.ReportEveryFailure);
            LatchHelper integrationTriggerLatchHelper = new LatchHelper();
            integrationTriggerMock.Setup(_trigger => _trigger.IntegrationCompleted()).Verifiable();
			integrationTriggerMock.Setup(_trigger => _trigger.Fire()).Callback(() => integrationTriggerLatchHelper.SetLatch()).Returns(() => null).Verifiable();
		    integrator.Start();
			integrator.Request(request);
			projectLatchHelper.WaitForSignal();
			integrationTriggerLatchHelper.WaitForSignal();
			VerifyAll();
		}

		[Fact]
		public void CancelPendingRequestDoesNothingForNoPendingItems()
		{
			int queuedItemCount = integrationQueue.GetQueuedIntegrations().Length;
			integrator.CancelPendingRequest();
			Assert.Equal(queuedItemCount, integrationQueue.GetQueuedIntegrations().Length);

			VerifyAll();
		}

		[Fact]
		public void CancelPendingRequestRemovesPendingItems()
		{
			IProject project = (IProject) projectMock.Object;

            IntegrationRequest request1 = new IntegrationRequest(BuildCondition.IfModificationExists, "intervalTrigger", null);
			projectMock.Setup(_project => _project.NotifyPendingState()).Verifiable();
			integrationQueue.Enqueue(new IntegrationQueueItem(project, request1, integrator));

            IntegrationRequest request2 = new IntegrationRequest(BuildCondition.IfModificationExists, "intervalTrigger", null);
			integrationQueue.Enqueue(new IntegrationQueueItem(project, request2, integrator));

			int queuedItemCount = integrationQueue.GetQueuedIntegrations().Length;
			Assert.Equal(2, queuedItemCount);
			integrationTriggerMock.Setup(_trigger => _trigger.IntegrationCompleted()).Verifiable();

			integrator.CancelPendingRequest();

			queuedItemCount = integrationQueue.GetQueuedIntegrations().Length;
			Assert.Equal(1, queuedItemCount);

			VerifyAll();
			projectMock.Verify(_project => _project.NotifyPendingState(), Times.Once);
		}

		[Fact]
		public void FirstBuildOfProjectShouldSetToPending()
		{
			IProject project = (IProject) projectMock.Object;

            IntegrationRequest request1 = new IntegrationRequest(BuildCondition.IfModificationExists, "intervalTrigger", null);
			projectMock.Setup(_project => _project.NotifyPendingState()).Verifiable();

			integrationQueue.Enqueue(new IntegrationQueueItem(project, request1, integrator));
			VerifyAll();
		}

		[Fact]
		public void SecondBuildOfProjectShouldNotSetToPendingWhenQueued()
		{
			IProject project = (IProject) projectMock.Object;

            IntegrationRequest request1 = new IntegrationRequest(BuildCondition.IfModificationExists, "intervalTrigger", null);
			projectMock.Setup(_project => _project.NotifyPendingState()).Verifiable();

            IntegrationRequest request2 = new IntegrationRequest(BuildCondition.IfModificationExists, "intervalTrigger", null);

			integrationQueue.Enqueue(new IntegrationQueueItem(project, request1, integrator));
			integrationQueue.Enqueue(new IntegrationQueueItem(project, request2, integrator));
			VerifyAll();
			projectMock.Verify(_project => _project.NotifyPendingState(), Times.Once);
		}

		[Fact]
		public void CompletingOnlyQueueBuildGoesToSleepingState()
		{
			IProject project = (IProject) projectMock.Object;

            IntegrationRequest request1 = new IntegrationRequest(BuildCondition.IfModificationExists, "intervalTrigger", null);
			projectMock.Setup(_project => _project.NotifyPendingState()).Verifiable();
			integrationTriggerMock.Setup(_trigger => _trigger.IntegrationCompleted()).Verifiable();
			projectMock.Setup(_project => _project.NotifySleepingState()).Verifiable();

			integrationQueue.Enqueue(new IntegrationQueueItem(project, request1, integrator));
			// Simulate first build completed by dequeuing it to invoke notifcation.
			integrationQueue.Dequeue();
			VerifyAll();
		}

		[Fact]
		public void CompletingWithPendingQueueBuildGoesToPendingState()
		{
			IProject project = (IProject) projectMock.Object;

            IntegrationRequest request1 = new IntegrationRequest(BuildCondition.IfModificationExists, "intervalTrigger", null);
			projectMock.Setup(_project => _project.NotifyPendingState()).Verifiable();

            IntegrationRequest request2 = new IntegrationRequest(BuildCondition.IfModificationExists, "intervalTrigger", null);

			// As first build completes we go to pending as still another build on queue
			integrationTriggerMock.Setup(_trigger => _trigger.IntegrationCompleted()).Verifiable();
			projectMock.Setup(_project => _project.NotifyPendingState()).Verifiable();

			integrationQueue.Enqueue(new IntegrationQueueItem(project, request1, integrator));
			integrationQueue.Enqueue(new IntegrationQueueItem(project, request2, integrator));
			// Simulate first build completed by dequeuing it to invoke notifcation.
			integrationQueue.Dequeue();

			VerifyAll();
		}

		[Fact]
		public void CompletingAllPendingQueueBuildsGoesToPendingState()
		{
			IProject project = (IProject) projectMock.Object;

            IntegrationRequest request1 = new IntegrationRequest(BuildCondition.IfModificationExists, "intervalTrigger", null);
			projectMock.Setup(_project => _project.NotifyPendingState()).Verifiable();

            IntegrationRequest request2 = new IntegrationRequest(BuildCondition.IfModificationExists, "intervalTrigger", null);
			// As first build completes we go to pending as still another build on queue
			integrationTriggerMock.Setup(_trigger => _trigger.IntegrationCompleted()).Verifiable();
			projectMock.Setup(_project => _project.NotifyPendingState()).Verifiable();
			// As second build completes, we can go to sleeping state
			integrationTriggerMock.Setup(_trigger => _trigger.IntegrationCompleted()).Verifiable();
			projectMock.Setup(_project => _project.NotifySleepingState()).Verifiable();

			integrationQueue.Enqueue(new IntegrationQueueItem(project, request1, integrator));
			integrationQueue.Enqueue(new IntegrationQueueItem(project, request2, integrator));
			// Simulate first build completed by dequeuing it to invoke notifcation.
			integrationQueue.Dequeue();
			// Simulate second build completed by dequeuing it to invoke notifcation.
			integrationQueue.Dequeue();
			
			VerifyAll();
		}

		[Fact]
		public void CancellingAPendingRequestWhileBuildingIgnoresState()
		{
			IProject project = (IProject) projectMock.Object;

            IntegrationRequest request1 = new IntegrationRequest(BuildCondition.IfModificationExists, "intervalTrigger", null);
			projectMock.Setup(_project => _project.NotifyPendingState()).Verifiable();

            IntegrationRequest request2 = new IntegrationRequest(BuildCondition.IfModificationExists, "intervalTrigger", null);
			// As pending build is cancelled we should not alter state
			integrationTriggerMock.Setup(_trigger => _trigger.IntegrationCompleted()).Verifiable();

			integrationQueue.Enqueue(new IntegrationQueueItem(project, request1, integrator));
			integrationQueue.Enqueue(new IntegrationQueueItem(project, request2, integrator));
			// Cancel second build project on queue
			integrator.CancelPendingRequest();

			projectMock.Verify(_project => _project.NotifyPendingState(), Times.Once);
			projectMock.Verify(_project => _project.NotifySleepingState(), Times.Never);
			VerifyAll();
		}

		[Fact]
		public void CancellingAPendingRequestWhileNotBuildingGoesToSleeping()
		{
			var otherProjectMock = new Mock<IProject>(MockBehavior.Strict);
			otherProjectMock.SetupGet(_project => _project.Name).Returns("otherProject");
			otherProjectMock.SetupGet(_project => _project.QueueName).Returns(TestQueueName);
			otherProjectMock.SetupGet(_project => _project.QueuePriority).Returns(0);
			otherProjectMock.SetupGet(_project => _project.Triggers).Returns(integrationTriggerMock.Object);
            otherProjectMock.SetupGet(_project => _project.WorkingDirectory).Returns(tempDir + "tempWorkingDir2");
            otherProjectMock.SetupGet(_project => _project.ArtifactDirectory).Returns(tempDir + "tempArtifactDir2");

			IProject otherProject = (IProject) otherProjectMock.Object;
			IProject project = (IProject) projectMock.Object;

			ProjectIntegrator otherIntegrator = new ProjectIntegrator(otherProject, integrationQueue);
			// Queue up the "otherProject" in the first queue position to build
            IntegrationRequest otherProjectRequest = new IntegrationRequest(BuildCondition.IfModificationExists, "intervalTrigger", null);
			otherProjectMock.Setup(_project => _project.NotifyPendingState()).Verifiable();

			// Queue up our test project on the same queue as so it goes to pending
            IntegrationRequest request2 = new IntegrationRequest(BuildCondition.IfModificationExists, "intervalTrigger", null);
			projectMock.Setup(_project => _project.NotifyPendingState()).Verifiable();
			// Cancelling the pending request should revert status to sleeping
			projectMock.Setup(_project => _project.NotifySleepingState()).Verifiable();
			integrationTriggerMock.Setup(_trigger => _trigger.IntegrationCompleted()).Verifiable();

			integrationQueue.Enqueue(new IntegrationQueueItem(otherProject, otherProjectRequest, otherIntegrator));
			integrationQueue.Enqueue(new IntegrationQueueItem(project, request2, integrator));
			// Cancel second build project on queue
			integrator.CancelPendingRequest();
			
			otherProjectMock.Verify();
			VerifyAll();
		}

        [Fact]
        public void FiresIntegrationEvents()
        {
            string enforcer = "BuildForcer";
            IntegrationResult result = new IntegrationResult();
            result.ProjectName = (projectMock.Object as IProject).Name;
            result.Status = IntegrationStatus.Success;
            // The following latch is needed to ensure the end assertions are not called before
            // the events have been fired. Because of the multi-threaded nature of the integrators
            // this can happen without any of the other latches being affected.
            ManualResetEvent latch = new ManualResetEvent(false);

            bool eventIntegrationStartedFired = false;
            bool eventIntegrationCompletedFired = false;
            IntegrationStatus status = IntegrationStatus.Unknown;
            integrator.IntegrationStarted += delegate(object o, IntegrationStartedEventArgs a)
            {
                eventIntegrationStartedFired = true;
            };
            integrator.IntegrationCompleted += delegate(object o, IntegrationCompletedEventArgs a)
            {
                eventIntegrationCompletedFired = true;
                status = a.Status;
                latch.Set();
            };

            LatchHelper integrationTriggerLatchHelper = new LatchHelper();
            integrationTriggerMock.Setup(_trigger => _trigger.Fire()).Verifiable();
            LatchHelper projectLatchHelper = new LatchHelper();
            projectMock.Setup(project => project.Integrate(It.Is<IntegrationRequest>(r => r.BuildCondition == BuildCondition.ForceBuild))).Returns(result).Verifiable();
            projectMock.Setup(project => project.NotifyPendingState()).Verifiable();
            projectMock.Setup(project => project.NotifySleepingState()).Callback(() => projectLatchHelper.SetLatch()).Verifiable();
            projectMock.SetupGet(project => project.MaxSourceControlRetries).Returns(5);
            projectMock.SetupGet(project => project.SourceControlErrorHandling).Returns(ThoughtWorks.CruiseControl.Core.Sourcecontrol.Common.SourceControlErrorHandlingPolicy.ReportEveryFailure);
            integrationTriggerMock.Setup(_trigger => _trigger.IntegrationCompleted()).Callback(() => integrationTriggerLatchHelper.SetLatch()).Verifiable();
            var parameters = new Dictionary<string, string>();
            integrator.Start();
            integrator.ForceBuild(enforcer, parameters);
            integrationTriggerLatchHelper.WaitForSignal();
            projectLatchHelper.WaitForSignal();
            projectMock.Verify(project => project.Integrate(It.IsAny<IntegrationRequest>()), Times.Once);
            VerifyAll();

            latch.WaitOne(2000, false);
            Assert.True(eventIntegrationStartedFired);
            Assert.True(eventIntegrationCompletedFired);
            Assert.Equal(IntegrationStatus.Success, status);
        }

        [Fact(Skip = "can not get to work consistently")]
        public void IntegrationCanBeDelayed()
        {
            string enforcer = "BuildForcer";
            IntegrationResult result = new IntegrationResult();
            result.ProjectName = (projectMock.Object as IProject).Name;
            result.Status = IntegrationStatus.Success;
            // The following latch is needed to ensure the end assertions are not called before
            // the events have been fired. Because of the multi-threaded nature of the integrators
            // this can happen without any of the other latches being affected.
            ManualResetEvent latch = new ManualResetEvent(false);

            bool eventIntegrationStartedFired = false;
            bool eventIntegrationCompletedFired = false;
            bool delayIntegration = true;
            IntegrationStatus status = IntegrationStatus.Unknown;
            integrator.IntegrationStarted += delegate(object o, IntegrationStartedEventArgs a)
            {
                eventIntegrationStartedFired = true;
                if (delayIntegration)
                {
                    a.Result = IntegrationStartedEventArgs.EventResult.Delay;
                    delayIntegration = !delayIntegration;
                }
            };
            integrator.IntegrationCompleted += delegate(object o, IntegrationCompletedEventArgs a)
            {
                eventIntegrationCompletedFired = true;
                status = a.Status;
                latch.Set();
            };

            LatchHelper integrationTriggerLatchHelper = new LatchHelper();
            integrationTriggerMock.Setup(_trigger => _trigger.Fire()).Verifiable();
            LatchHelper projectLatchHelper = new LatchHelper();
            projectMock.Setup(project => project.Integrate(It.Is<IntegrationRequest>(r => r.BuildCondition == BuildCondition.ForceBuild))).Returns(result).Verifiable();
            projectMock.Setup(project => project.NotifyPendingState()).Verifiable();
            projectMock.Setup(project => project.NotifySleepingState()).Callback(() => projectLatchHelper.SetLatch()).Verifiable();
            projectMock.SetupGet(project => project.MaxSourceControlRetries).Returns(5);
            projectMock.SetupGet(project => project.SourceControlErrorHandling).Returns(ThoughtWorks.CruiseControl.Core.Sourcecontrol.Common.SourceControlErrorHandlingPolicy.ReportEveryFailure);
            integrationTriggerMock.Setup(_trigger => _trigger.IntegrationCompleted()).Callback(() => integrationTriggerLatchHelper.SetLatch()).Verifiable();
            var parameters = new Dictionary<string, string>();
            integrator.Start();
            integrator.ForceBuild(enforcer, parameters);
            integrationTriggerLatchHelper.WaitForSignal();
            projectLatchHelper.WaitForSignal();
            VerifyAll();
            projectMock.VerifyNoOtherCalls();

            latch.WaitOne(2000, false);
            Assert.True(eventIntegrationStartedFired);
            Assert.True(eventIntegrationCompletedFired);
            Assert.Equal(IntegrationStatus.Success, status);
        }

        [Fact(Skip ="")]
        //[Ignore("can not get to work consistently")]
        public void IntegrationCanBeCancelled()
        {
            string enforcer = "BuildForcer";
            IntegrationResult result = new IntegrationResult();
            result.ProjectName = (projectMock.Object as IProject).Name;
            result.Status = IntegrationStatus.Success;
            // The following latch is needed to ensure the end assertions are not called before
            // the events have been fired. Because of the multi-threaded nature of the integrators
            // this can happen without any of the other latches being affected.
            ManualResetEvent latch = new ManualResetEvent(false);

            bool eventIntegrationStartedFired = false;
            bool eventIntegrationCompletedFired = false;
            IntegrationStatus status = IntegrationStatus.Unknown;
            integrator.IntegrationStarted += delegate(object o, IntegrationStartedEventArgs a)
            {
                eventIntegrationStartedFired = true;
                a.Result = IntegrationStartedEventArgs.EventResult.Cancel;
            };
            integrator.IntegrationCompleted += delegate(object o, IntegrationCompletedEventArgs a)
            {
                eventIntegrationCompletedFired = true;
                status = a.Status;
                latch.Set();
            };

            LatchHelper integrationTriggerLatchHelper = new LatchHelper();
            integrationTriggerMock.Setup(_trigger => _trigger.Fire()).Verifiable();
            LatchHelper projectLatchHelper = new LatchHelper();
            projectMock.Setup(project => project.NotifyPendingState()).Verifiable();
            projectMock.Setup(project => project.NotifySleepingState()).Callback(() => projectLatchHelper.SetLatch()).Verifiable();
            projectMock.SetupGet(project => project.MaxSourceControlRetries).Returns(5);
            projectMock.SetupGet(project => project.SourceControlErrorHandling).Returns(ThoughtWorks.CruiseControl.Core.Sourcecontrol.Common.SourceControlErrorHandlingPolicy.ReportEveryFailure);
            integrationTriggerMock.Setup(_trigger => _trigger.IntegrationCompleted()).Callback(() => integrationTriggerLatchHelper.SetLatch()).Verifiable();
            var parameters = new Dictionary<string, string>();
            integrator.Start();
            integrator.ForceBuild(enforcer, parameters);
            integrationTriggerLatchHelper.WaitForSignal();
            projectLatchHelper.WaitForSignal();
            VerifyAll();
            projectMock.VerifyNoOtherCalls();

            latch.WaitOne(2000, false);
            Assert.True(eventIntegrationStartedFired);
            Assert.True(eventIntegrationCompletedFired);
            Assert.Equal(IntegrationStatus.Cancelled, status);
        }
    }
}
