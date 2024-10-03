using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Presentation;
using System;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Monitoring
{
	public class AggregatingServerMonitorTest : IDisposable
	{
		private Mock<IServerMonitor> monitor1;
		private Mock<IServerMonitor> monitor2;
		private Mock<IServerMonitor> monitor3;
		private IServerMonitor[] monitors;
		private AggregatingServerMonitor aggregator;

		//[SetUp]
		public void SetUp()
		{
			monitor1 = new Mock<IServerMonitor>();
			monitor2 = new Mock<IServerMonitor>();
			monitor3 = new Mock<IServerMonitor>();

			monitors = new IServerMonitor[]
				{
					(IServerMonitor) monitor1.Object,
					(IServerMonitor) monitor2.Object,
					(IServerMonitor) monitor3.Object,
				};

			aggregator = new AggregatingServerMonitor(monitors);
		}

		
		public void Dispose()
		{
			monitor1.Verify();
			monitor2.Verify();
			monitor3.Verify();
		}

		[Fact]
		public void PollInvokesPollOnAllContainedServers()
		{
			monitor1.Setup(_monitor => _monitor.Poll()).Verifiable();
			monitor2.Setup(_monitor => _monitor.Poll()).Verifiable();
			monitor3.Setup(_monitor => _monitor.Poll()).Verifiable();
			aggregator.Poll();
		}

		private int queueChangedCount;
		private MonitorServerQueueChangedEventArgs lastQueueChangedEventArgs;

		[Fact]
		public void QueueChangedIsFiredWheneverAnyContainedServerFiresIt()
		{
			queueChangedCount = 0;
			lastQueueChangedEventArgs = null;

			StubServerMonitor stubServerMonitor1 = new StubServerMonitor("tcp://somehost1/");
			StubServerMonitor stubServerMonitor2 = new StubServerMonitor("tcp://somehost2/");

			aggregator = new AggregatingServerMonitor(stubServerMonitor1, stubServerMonitor2);
			aggregator.QueueChanged += new MonitorServerQueueChangedEventHandler(Aggregator_QueueChanged);
            // ClassicAssert.AreEqual(0, queueChangedCount);
            Assert.Equal(0, queueChangedCount);
			stubServerMonitor1.OnQueueChanged(new MonitorServerQueueChangedEventArgs(stubServerMonitor1));

			Assert.Equal(1, queueChangedCount);
			Assert.Same(stubServerMonitor1, lastQueueChangedEventArgs.ServerMonitor);
		}

		private void Aggregator_QueueChanged(object sauce, MonitorServerQueueChangedEventArgs e)
		{
			queueChangedCount++;
			lastQueueChangedEventArgs = e;
		}

		private int pollCount;
		private object lastPolledSource;
		private MonitorServerPolledEventArgs lastPolledArgs;

		[Fact]
		public void PolledIsFiredWheneverAnyContainedServerFiresIt()
		{
			pollCount = 0;

			StubServerMonitor stubServerMonitor1 = new StubServerMonitor("tcp://1.2.3.4/");
			StubServerMonitor stubServerMonitor2 = new StubServerMonitor("tcp://1.2.3.5/");

			aggregator = new AggregatingServerMonitor(stubServerMonitor1, stubServerMonitor2);
			aggregator.Polled += new MonitorServerPolledEventHandler(Aggregator_Polled);

			Assert.Equal(0, pollCount);
			stubServerMonitor1.Poll();

			Assert.Equal(1, pollCount);
		}

		private void Aggregator_Polled(object source, MonitorServerPolledEventArgs args)
		{
			pollCount++;
			lastPolledSource = source;
			lastPolledArgs = args;
		}

		[Fact]
		public void WhenPolledIsFiredTheSourcePointToTheAggregatorNotTheFiringServer()
		{
			StubServerMonitor stubServerMonitor1 = new StubServerMonitor("tcp://1.2.3.4/");

			aggregator = new AggregatingServerMonitor(stubServerMonitor1);
			aggregator.Polled += new MonitorServerPolledEventHandler(Aggregator_Polled);

			aggregator.Poll();

			Assert.Same(lastPolledSource, aggregator);
			Assert.Same(lastPolledArgs.ServerMonitor, stubServerMonitor1);
		}
	}
}
