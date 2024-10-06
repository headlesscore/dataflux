using System.Collections;
using System.Diagnostics;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core
{
	public class TraceListenerBackup
	{
		private ArrayList backupListenerCollection;

		public TraceListenerBackup()
		{
			backupListenerCollection = new ArrayList();
			backupListenerCollection.AddRange(Trace.Listeners);
			System.Diagnostics.Trace.Listeners.Clear();			
		}

		public void Reset()
		{
			System.Diagnostics.Trace.Listeners.Clear();
			foreach (TraceListener listener in backupListenerCollection)
			{
				System.Diagnostics.Trace.Listeners.Add(listener);				
			}			
		}

		public TestTraceListener AddTestTraceListener()
		{
			TestTraceListener listener = new TestTraceListener();
			System.Diagnostics.Trace.Listeners.Add(listener);
			return listener;
		}

		public TraceListener AddTraceListener(TraceListener listener)
		{
			System.Diagnostics.Trace.Listeners.Add(listener);
			return listener;
		}
	}
}
