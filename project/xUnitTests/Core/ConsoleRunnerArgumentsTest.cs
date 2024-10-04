using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core
{
	
	public class ConsoleRunnerArgumentsTest : CustomAssertion
	{
		private TraceListenerBackup backup;
		private TestTraceListener listener;

		// [SetUp]
		protected void AddListener()
		{
			backup = new TraceListenerBackup();
			backup.Reset();
			listener = backup.AddTestTraceListener();
		}

		// [TearDown]
		protected void RemoveListener()
		{
			backup.Reset();
		}

		[Fact]
		public void TestDefaultArguments()
		{
			ConsoleRunnerArguments consoleArgs = new ConsoleRunnerArguments();
			Assert.Equal(true, consoleArgs.UseRemoting);
            Assert.Equal(true, consoleArgs.UseRemoting);
            Assert.Null(consoleArgs.Project);
			Assert.Equal(ConsoleRunnerArguments.DEFAULT_CONFIG_PATH, consoleArgs.ConfigFile);
            Assert.Equal(false, consoleArgs.ValidateConfigOnly);
            Assert.Equal(true, consoleArgs.Logging);
            Assert.Equal(true, consoleArgs.PauseOnError);
            Assert.Equal(false, consoleArgs.ShowHelp);
		}
	}
}
