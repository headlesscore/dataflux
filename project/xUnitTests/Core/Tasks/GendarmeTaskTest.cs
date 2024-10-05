using System.IO;	
using Exortech.NetReflector;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
	
	public class GendarmeTaskTest : ProcessExecutorTestFixtureBase
	{
		private string logfile;
		private IIntegrationResult result;
		private GendarmeTask task;

		// [SetUp]
		protected void SetUp()
		{
			CreateProcessExecutorMock(GendarmeTask.defaultExecutable);
			result = IntegrationResult();
			result.Label = "1.0";
			result.ArtifactDirectory = Path.GetTempPath();
			logfile = Path.Combine(result.ArtifactDirectory, GendarmeTask.logFilename);
			TempFileUtil.DeleteTempFile(logfile);
			task = new GendarmeTask((ProcessExecutor)mockProcessExecutor.Object);
		}

		// [TearDown]
		protected void TearDown()
		{
			Verify();
		}

		protected static void AddDefaultAssemblyToCheck(GendarmeTask task)
		{
            AssemblyMatch match1 = new AssemblyMatch();
            match1.Expression = "*.dll";
			AssemblyMatch match2 = new AssemblyMatch();
			match2.Expression = "*.exe";
			task.Assemblies = new AssemblyMatch[] { match1, match2 };
		}

		[Fact]
		public void PopulateFromReflector()
		{
			const string xml = @"
    <gendarme>
    	<executable>gendarme.exe</executable>
    	<baseDirectory>C:\</baseDirectory>
		<configFile>rules.xml</configFile>
		<ruleSet>*</ruleSet>
		<ignoreFile>C:\gendarme.ignore.list.txt</ignoreFile>
		<limit>200</limit>
		<severity>medium+</severity>
		<confidence>normal+</confidence>
		<quiet>FALSE</quiet>
		<verbose>TRUE</verbose>
		<failBuildOnFoundDefects>TRUE</failBuildOnFoundDefects>
		<verifyTimeoutSeconds>600</verifyTimeoutSeconds>
		<assemblies>
      		<assemblyMatch expr='*.dll' />
			<assemblyMatch expr='*.exe' />
    	</assemblies>
		<assemblyListFile>C:\gendarme.assembly.list.txt</assemblyListFile>
		<description>Test description</description>
    </gendarme>";

			NetReflector.Read(xml, task);
			Assert.Equal("gendarme.exe", task.Executable);
			Assert.Equal(@"C:\", task.ConfiguredBaseDirectory);
			Assert.Equal("rules.xml", task.ConfigFile);
			Assert.Equal("*", task.RuleSet);
			Assert.Equal(@"C:\gendarme.ignore.list.txt", task.IgnoreFile);
			Assert.Equal(200, task.Limit);
			Assert.Equal("medium+", task.Severity);
			Assert.Equal("normal+", task.Confidence);
			Assert.Equal(false, task.Quiet);
			Assert.Equal(true, task.Verbose);
			Assert.Equal(true, task.FailBuildOnFoundDefects);
			Assert.Equal(600, task.VerifyTimeoutSeconds);
			Assert.Equal("Test description", task.Description);

			Assert.Equal(2, task.Assemblies.Length);
			Assert.Equal("*.dll", task.Assemblies[0].Expression);
			Assert.Equal("*.exe", task.Assemblies[1].Expression);
			Assert.Equal(@"C:\gendarme.assembly.list.txt", task.AssemblyListFile);
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public void PopulateFromConfigurationUsingOnlyRequiredElementsAndCheckDefaultValues()
		{
			const string xml = @"<gendarme />";

			NetReflector.Read(xml, task);
			Assert.Equal(GendarmeTask.defaultExecutable, task.Executable);
			Assert.Equal(string.Empty, task.ConfiguredBaseDirectory);
			Assert.Equal(string.Empty, task.ConfigFile);
			Assert.Equal(string.Empty, task.RuleSet);
			Assert.Equal(string.Empty, task.IgnoreFile);
			Assert.Equal(GendarmeTask.defaultLimit, task.Limit);
			Assert.Equal(string.Empty, task.Severity);
			Assert.Equal(string.Empty, task.Confidence);
			Assert.Equal(GendarmeTask.defaultQuiet, task.Quiet);
			Assert.Equal(GendarmeTask.defaultVerbose, task.Verbose);
			Assert.Equal(GendarmeTask.defaultFailBuildOnFoundDefects, task.FailBuildOnFoundDefects);
			Assert.Equal(GendarmeTask.defaultVerifyTimeout, task.VerifyTimeoutSeconds);
			Assert.Equal(null, task.Description);
			Assert.Equal(0, task.Assemblies.Length);
			Assert.Equal(string.Empty, task.AssemblyListFile);
		}

		[Fact]
		public void ShouldEncloseDirectoriesInQuotesIfTheyContainSpaces()
		{
			result.ArtifactDirectory = DefaultWorkingDirectoryWithSpaces;
			result.WorkingDirectory = DefaultWorkingDirectoryWithSpaces;

			task.AssemblyListFile = Path.Combine(DefaultWorkingDirectoryWithSpaces, "gendarme assembly file.txt");
			task.ConfigFile = Path.Combine(DefaultWorkingDirectoryWithSpaces, "gendarme rules.xml");
			task.IgnoreFile = Path.Combine(DefaultWorkingDirectoryWithSpaces, "gendarme ignore file.txt");

			ExpectToExecuteArguments(@"--config " + StringUtil.AutoDoubleQuoteString(task.ConfigFile) + " --ignore " +
			                         StringUtil.AutoDoubleQuoteString(task.IgnoreFile) + " --xml " +
			                         StringUtil.AutoDoubleQuoteString(Path.Combine(result.ArtifactDirectory, "gendarme-results.xml")) + " @" +
									 StringUtil.AutoDoubleQuoteString(task.AssemblyListFile), DefaultWorkingDirectoryWithSpaces);

			task.ConfiguredBaseDirectory = DefaultWorkingDirectoryWithSpaces;
			task.VerifyTimeoutSeconds = 600;
			task.Run(result);
		}

		[Fact]
		public void ShouldThrowConfigurationExceptionIfAssemblyListNotSet()
		{
			//DO NOT SET: AddDefaultAssemblyToCheck(task);
            Assert.Throws<CruiseControl.Core.Config.ConfigurationException>(delegate { task.Run(result); });
		}

		[Fact]
		public void ShouldThrowBuilderExceptionIfProcessThrowsException()
		{
			AddDefaultAssemblyToCheck(task);
			ExpectToExecuteAndThrow();
            Assert.Throws<BuilderException>(delegate { task.Run(result); });
		}

		[Fact]
		public void RebaseFromWorkingDirectory()
		{
			AddDefaultAssemblyToCheck(task);
			ProcessInfo info =
				NewProcessInfo(
					string.Format(System.Globalization.CultureInfo.CurrentCulture,"--xml {0} {1} {2}",
					              StringUtil.AutoDoubleQuoteString(Path.Combine(result.ArtifactDirectory, "gendarme-results.xml")),
					              StringUtil.AutoDoubleQuoteString("*.dll"), StringUtil.AutoDoubleQuoteString("*.exe")),
					Path.Combine(DefaultWorkingDirectory, "src"));

			info.WorkingDirectory = Path.Combine(DefaultWorkingDirectory, "src");
			ExpectToExecute(info);
			task.ConfiguredBaseDirectory = "src";
			task.VerifyTimeoutSeconds = 600;
			task.Run(result);
		}

		[Fact]
		public void UseAssemblyCollectionAndAssemblyListFile()
		{
			AddDefaultAssemblyToCheck(task);
			task.AssemblyListFile = Path.Combine(DefaultWorkingDirectoryWithSpaces, "gendarme assembly file.txt");

			ProcessInfo info =
				NewProcessInfo(
					string.Format(System.Globalization.CultureInfo.CurrentCulture,"--xml {0} @{1} {2} {3}",
					              StringUtil.AutoDoubleQuoteString(Path.Combine(result.ArtifactDirectory, "gendarme-results.xml")),
					              StringUtil.AutoDoubleQuoteString(task.AssemblyListFile),
					              StringUtil.AutoDoubleQuoteString("*.dll"), StringUtil.AutoDoubleQuoteString("*.exe")),
					Path.Combine(DefaultWorkingDirectory, "src"));

			info.WorkingDirectory = Path.Combine(DefaultWorkingDirectory, "src");
			ExpectToExecute(info);
			task.ConfiguredBaseDirectory = "src";
			task.VerifyTimeoutSeconds = 600;
			task.Run(result);
		}

		[Fact]
		public void TimedOutExecutionShouldFailBuild()
		{
			AddDefaultAssemblyToCheck(task);
			ExpectToExecuteAndReturn(TimedOutProcessResult());
			task.Run(result);

			Assert.True(result.Status == IntegrationStatus.Failure);
			Assert.Matches(result.TaskOutput, "Command line '.*' timed out after \\d+ seconds");
		}

		[Fact]
		public void ShouldAutomaticallyMergeTheBuildOutputFile()
		{
			AddDefaultAssemblyToCheck(task);
			TempFileUtil.CreateTempXmlFile(logfile, "<output/>");
			ExpectToExecuteAndReturn(SuccessfulProcessResult());
			task.Run(result);
			Assert.Single(result.TaskResults);
		    Assert.Empty(result.TaskOutput);
			Assert.True(result.Succeeded);
		}

		[Fact]
		public void ShouldFailOnFailedProcessResult()
		{
			AddDefaultAssemblyToCheck(task);
			TempFileUtil.CreateTempXmlFile(logfile, "<output/>");
			ExpectToExecuteAndReturn(FailedProcessResult());
			task.Run(result);
			Assert.Equal(1, result.TaskResults.Count);
		    Assert.True(result.TaskOutput == ProcessResultOutput);
			Assert.True(result.Failed);
		}
	}
}
