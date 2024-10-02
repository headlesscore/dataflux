using System.Collections.Generic;
using System.ComponentModel;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.CCTrayLib;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.CCTrayLib.Presentation;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Presentation
{
	[TestFixture]
	public class SynchronizedProjectMonitorTest
	{
		[Test]
		public void MethodsAndPropertiesDoSimpleDelagationOntoInjectedMonitor()
		{
			var mockProjectMonitor = new Mock<IProjectMonitor>();

			SynchronizedProjectMonitor monitor = new SynchronizedProjectMonitor(
				(IProjectMonitor) mockProjectMonitor.Object, null);

			mockProjectMonitor.SetupGet(_monitor => _monitor.ProjectState).Returns(() => null).Verifiable();
			ClassicAssert.IsNull(monitor.ProjectState);
            ClassicAssert.IsNull(monitor.ProjectState);

            Dictionary<string, string> parameters = new Dictionary<string, string>();
			mockProjectMonitor.Setup(_monitor => _monitor.ForceBuild(parameters, null)).Verifiable();
			monitor.ForceBuild(parameters, null);

			mockProjectMonitor.Setup(_monitor => _monitor.Poll()).Verifiable();
			monitor.Poll();

			mockProjectMonitor.Verify();
		}

		[Test]
		public void WhenPolledIsFiredTheDelegateIsInvokedThroughISynchronisedInvoke()
		{
			var mockSynchronizeInvoke = new Mock<ISynchronizeInvoke>();
			StubProjectMonitor containedMonitor = new StubProjectMonitor("test");

			SynchronizedProjectMonitor monitor = new SynchronizedProjectMonitor(
				containedMonitor,
				(ISynchronizeInvoke) mockSynchronizeInvoke.Object);

			MonitorPolledEventHandler delegateToPolledMethod = new MonitorPolledEventHandler(Monitor_Polled);
			monitor.Polled += delegateToPolledMethod;

			mockSynchronizeInvoke.Setup(invoke => invoke.BeginInvoke(delegateToPolledMethod, It.IsAny<object[]>())).Verifiable();
			containedMonitor.OnPolled(new MonitorPolledEventArgs(containedMonitor));

			mockSynchronizeInvoke.Verify();
		}

		[Test]
		public void WhenBuildOccurredIsFiredTheDelegateIsInvokedThroughISynchronisedInvoke()
		{
			var mockSynchronizeInvoke = new Mock<ISynchronizeInvoke>();
			StubProjectMonitor containedMonitor = new StubProjectMonitor("test");

			SynchronizedProjectMonitor monitor = new SynchronizedProjectMonitor(
				containedMonitor,
				(ISynchronizeInvoke) mockSynchronizeInvoke.Object);

			MonitorBuildOccurredEventHandler delegateToBuildOccurred = new MonitorBuildOccurredEventHandler(Monitor_BuildOccurred);
			monitor.BuildOccurred += delegateToBuildOccurred;

			mockSynchronizeInvoke.Setup(invoke => invoke.BeginInvoke(delegateToBuildOccurred, It.IsAny<object[]>())).Verifiable();
			containedMonitor.OnBuildOccurred(new MonitorBuildOccurredEventArgs(null, BuildTransition.StillFailing));

			mockSynchronizeInvoke.Verify();
		}

		private void Monitor_Polled(object sender, MonitorPolledEventArgs args)
		{
			ClassicAssert.Fail("Do not expect this method to actually get called as using mcoked synchronised invoke");
		}

		private void Monitor_BuildOccurred(object sender, MonitorBuildOccurredEventArgs e)
		{
			ClassicAssert.Fail("Do not expect this method to actually get called as using mcoked synchronised invoke");
		}
	}
}
