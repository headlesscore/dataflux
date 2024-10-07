using System.IO;	
using Exortech.NetReflector;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
	[TestFixture]
	public class GendarmeTaskTest : ProcessExecutorTestFixtureBase
	{
		private string logfile;
		private IIntegrationResult result;
		private GendarmeTask task;

		[SetUp]
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

		[TearDown]
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

		[Test]
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
			ClassicAssert.AreEqual("gendarme.exe", task.Executable);
			ClassicAssert.AreEqual(@"C:\", task.ConfiguredBaseDirectory);
			ClassicAssert.AreEqual("rules.xml", task.ConfigFile);
			ClassicAssert.AreEqual("*", task.RuleSet);
			ClassicAssert.AreEqual(@"C:\gendarme.ignore.list.txt", task.IgnoreFile);
			ClassicAssert.AreEqual(200, task.Limit);
			ClassicAssert.AreEqual("medium+", task.Severity);
			ClassicAssert.AreEqual("normal+", task.Confidence);
			ClassicAssert.AreEqual(false, task.Quiet);
			ClassicAssert.AreEqual(true, task.Verbose);
			ClassicAssert.AreEqual(true, task.FailBuildOnFoundDefects);
			ClassicAssert.AreEqual(600, task.VerifyTimeoutSeconds);
			ClassicAssert.AreEqual("Test description", task.Description);

			ClassicAssert.AreEqual(2, task.Assemblies.Length);
			ClassicAssert.AreEqual("*.dll", task.Assemblies[0].Expression);
			ClassicAssert.AreEqual("*.exe", task.Assemblies[1].Expression);
			ClassicAssert.AreEqual(@"C:\gendarme.assembly.list.txt", task.AssemblyListFile);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

		[Test]
		public void PopulateFromConfigurationUsingOnlyRequiredElementsAndCheckDefaultValues()
		{
			const string xml = @"<gendarme />";

			NetReflector.Read(xml, task);
			ClassicAssert.AreEqual(GendarmeTask.defaultExecutable, task.Executable);
			ClassicAssert.AreEqual(string.Empty, task.ConfiguredBaseDirectory);
			ClassicAssert.AreEqual(string.Empty, task.ConfigFile);
			ClassicAssert.AreEqual(string.Empty, task.RuleSet);
			ClassicAssert.AreEqual(string.Empty, task.IgnoreFile);
			ClassicAssert.AreEqual(GendarmeTask.defaultLimit, task.Limit);
			ClassicAssert.AreEqual(string.Empty, task.Severity);
			ClassicAssert.AreEqual(string.Empty, task.Confidence);
			ClassicAssert.AreEqual(GendarmeTask.defaultQuiet, task.Quiet);
			ClassicAssert.AreEqual(GendarmeTask.defaultVerbose, task.Verbose);
			ClassicAssert.AreEqual(GendarmeTask.defaultFailBuildOnFoundDefects, task.FailBuildOnFoundDefects);
			ClassicAssert.AreEqual(GendarmeTask.defaultVerifyTimeout, task.VerifyTimeoutSeconds);
			ClassicAssert.AreEqual(null, task.Description);
			ClassicAssert.AreEqual(0, task.Assemblies.Length);
			ClassicAssert.AreEqual(string.Empty, task.AssemblyListFile);
		}

		[Test]
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

		[Test]
		public void ShouldThrowConfigurationExceptionIfAssemblyListNotSet()
		{
			//DO NOT SET: AddDefaultAssemblyToCheck(task);
            ClassicAssert.That(delegate { task.Run(result); },
                        Throws.TypeOf<ThoughtWorks.CruiseControl.Core.Config.ConfigurationException>());
		}

		[Test]
		public void ShouldThrowBuilderExceptionIfProcessThrowsException()
		{
			AddDefaultAssemblyToCheck(task);
			ExpectToExecuteAndThrow();
            ClassicAssert.That(delegate { task.Run(result); },
                        Throws.TypeOf<BuilderException>());
		}

		[Test]
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

		[Test]
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

		[Test]
		public void TimedOutExecutionShouldFailBuild()
		{
			AddDefaultAssemblyToCheck(task);
			ExpectToExecuteAndReturn(TimedOutProcessResult());
			task.Run(result);

			ClassicAssert.That(result.Status, Is.EqualTo(IntegrationStatus.Failure));
			ClassicAssert.That(result.TaskOutput, Does.Match("Command line '.*' timed out after \\d+ seconds"));
		}

		[Test]
		public void ShouldAutomaticallyMergeTheBuildOutputFile()
		{
			AddDefaultAssemblyToCheck(task);
			TempFileUtil.CreateTempXmlFile(logfile, "<output/>");
			ExpectToExecuteAndReturn(SuccessfulProcessResult());
			task.Run(result);
			ClassicAssert.AreEqual(1, result.TaskResults.Count);
		    ClassicAssert.That(result.TaskOutput, Is.Empty);
			ClassicAssert.IsTrue(result.Succeeded);
		}

		[Test]
		public void ShouldFailOnFailedProcessResult()
		{
			AddDefaultAssemblyToCheck(task);
			TempFileUtil.CreateTempXmlFile(logfile, "<output/>");
			ExpectToExecuteAndReturn(FailedProcessResult());
			task.Run(result);
			ClassicAssert.AreEqual(1, result.TaskResults.Count);
		    ClassicAssert.That(result.TaskOutput, Is.EqualTo(ProcessResultOutput));
			ClassicAssert.IsTrue(result.Failed);
		}
	}
}
