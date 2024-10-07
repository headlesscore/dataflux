using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core
{
	[TestFixture]
	public class ConsoleRunnerArgumentsTest : CustomAssertion
	{
		private TraceListenerBackup backup;
		private TestTraceListener listener;

		[SetUp]
		protected void AddListener()
		{
			backup = new TraceListenerBackup();
			backup.Reset();
			listener = backup.AddTestTraceListener();
		}

		[TearDown]
		protected void RemoveListener()
		{
			backup.Reset();
		}

		[Test]
		public void TestDefaultArguments()
		{
			ConsoleRunnerArguments consoleArgs = new ConsoleRunnerArguments();
			ClassicAssert.AreEqual(true, consoleArgs.UseRemoting);
            ClassicAssert.AreEqual(true, consoleArgs.UseRemoting);
            ClassicAssert.IsNull(consoleArgs.Project);
			ClassicAssert.AreEqual(ConsoleRunnerArguments.DEFAULT_CONFIG_PATH, consoleArgs.ConfigFile);
            ClassicAssert.AreEqual(false, consoleArgs.ValidateConfigOnly);
            ClassicAssert.AreEqual(true, consoleArgs.Logging);
            ClassicAssert.AreEqual(true, consoleArgs.PauseOnError);
            ClassicAssert.AreEqual(false, consoleArgs.ShowHelp);
		}
	}
}
