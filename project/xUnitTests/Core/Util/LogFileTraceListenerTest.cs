using System.Diagnostics;
using System.IO;
using Xunit;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
	
	public class LogFileTraceListenerTest : CustomAssertion
	{
		private const string TempDir = "LogFileTraceListenerTest";
		private LogFileTraceListener listener;
		private TraceListenerBackup backup;
		private string tempfile;

		// [SetUp]
		protected void AddTraceListener()
		{
			tempfile = TempFileUtil.CreateTempFile(TempDir, "ccnet.log");
			listener = new LogFileTraceListener(tempfile);
			backup = new TraceListenerBackup();
			backup.AddTraceListener(listener);
		}

		// [TearDown]
		protected void RemoveTraceListenerAndCleanUp()
		{
			backup.Reset();
			listener.Close();
			TempFileUtil.DeleteTempDir(TempDir);
		}

		[Fact]
		public void LoggingAnEntryShouldFlushLogFileIfAutoFlushIsEnabled()
		{
			Trace.AutoFlush = true;
			Trace.WriteLine("doh!");

			using (TextReader stream = new StreamReader(new FileStream(tempfile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
			{
				AssertContains("doh!", stream.ReadToEnd());
			}
		}
	}
}