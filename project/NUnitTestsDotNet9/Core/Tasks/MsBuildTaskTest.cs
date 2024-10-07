using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Exortech.NetReflector;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
	[TestFixture]
	public class MsBuildTaskTest : ProcessExecutorTestFixtureBase
	{
		private const string defaultLogger = "ThoughtWorks.CruiseControl.MsBuild.dll";
		private string logfile;
		private IIntegrationResult result;
		private MsBuildTask task;
        private MockRepository mocks = new MockRepository(MockBehavior.Default);

		[SetUp]
		protected void SetUp()
		{
			var shadowCopier = mocks.Create<IShadowCopier>().Object;
			Mock.Get(shadowCopier).Setup(_shadowCopier => _shadowCopier.RetrieveFilePath(defaultLogger)).Returns(defaultLogger);

			var executionEnvironment = mocks.Create<IExecutionEnvironment>().Object;
			Mock.Get(executionEnvironment).SetupGet(_executionEnvironment => _executionEnvironment.IsRunningOnWindows).Returns(true);
			Mock.Get(executionEnvironment).SetupGet(_executionEnvironment => _executionEnvironment.RuntimeDirectory).Returns(RuntimeEnvironment.GetRuntimeDirectory());

			CreateProcessExecutorMock(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "MSBuild.exe"));
			task = new MsBuildTask((ProcessExecutor) mockProcessExecutor.Object, executionEnvironment, shadowCopier);

			result = IntegrationResult();
			result.Label = "1.0";
			result.ArtifactDirectory = DefaultWorkingDirectory;
            
			logfile = Path.Combine(result.ArtifactDirectory, MsBuildTask.LogFilename);
			TempFileUtil.DeleteTempFile(logfile);
		}

		[TearDown]
		protected void TearDown()
		{
			Verify();
		}

		[Test]
		public void ExecuteSpecifiedProject()
		{
            string args = "/nologo /t:target1;target2 " + IntegrationProperties() + " /p:Configuration=Release myproject.sln" + GetLoggerArgument();
			ExpectToExecuteArguments(args, DefaultWorkingDirectory);

			task.ProjectFile = "myproject.sln";
			task.Targets = "target1;target2";
			task.BuildArgs = "/p:Configuration=Release";
			task.Timeout = 600;

			task.Run(result);

			ClassicAssert.AreEqual(1, result.TaskResults.Count);
			ClassicAssert.AreEqual(IntegrationStatus.Success, result.Status);
		    ClassicAssert.That(result.TaskOutput, Is.Empty);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

		[Test]
		public void AddQuotesAroundProjectsWithSpacesAndHandleNoSpecifiedTargets()
		{
			ExpectToExecuteArguments(@"/nologo " + IntegrationProperties() + @" ""my project.proj""" + GetLoggerArgument());
			task.ProjectFile = "my project.proj";

            task.Run(result);
		}

		[Test]
		public void AddQuotesAroundTargetsWithSpaces()
		{
            ExpectToExecuteArguments(@"/nologo /t:first;""next task"" " + IntegrationProperties() + GetLoggerArgument());
			task.Targets = "first;next task";

			task.Run(result);
		}

		[Test]
		public void AddQuotesAroundPropertiesWithSpaces()
		{
			// NOTE: Property names are sorted alphabetically when passed as process arguments
			// Tests that look for the correct arguments will fail if the following properties
			// are not sorted alphabetically.
            string expectedProperties = string.Format(@"/p:CCNetArtifactDirectory={2};CCNetBuildCondition=IfModificationExists;CCNetBuildDate={0};CCNetBuildId={5};CCNetBuildTime={1};CCNetFailureTasks=;CCNetFailureUsers=;CCNetIntegrationStatus=Success;CCNetLabel=""My Label"";CCNetLastIntegrationStatus=Success;CCNetListenerFile={3};CCNetModifyingUsers=;CCNetNumericLabel=0;CCNetProject=test;CCNetRequestSource=foo;CCNetUser=;CCNetWorkingDirectory={4}", testDateString, testTimeString, StringUtil.AutoDoubleQuoteString(result.ArtifactDirectory), StringUtil.AutoDoubleQuoteString(Path.GetTempPath() + "test_ListenFile.xml"), StringUtil.AutoDoubleQuoteString(DefaultWorkingDirectoryWithSpaces), IntegrationResultMother.DefaultBuildId);
            ExpectToExecuteArguments(@"/nologo " + expectedProperties + GetLoggerArgument(), DefaultWorkingDirectoryWithSpaces);
			result.Label = @"My Label";
			result.WorkingDirectory = DefaultWorkingDirectoryWithSpaces;

			task.Run(result);
		}

		[Test]
		public void DoNotAddQuotesAroundBuildArgs()
		{
            ExpectToExecuteArguments(@"/nologo " + IntegrationProperties() + @" /noconsolelogger /p:Configuration=Debug" + GetLoggerArgument());
			task.BuildArgs = "/noconsolelogger /p:Configuration=Debug";
			task.Run(result);			
		}

		[Test]
		public void RebaseFromWorkingDirectory()
		{
            ProcessInfo info = NewProcessInfo("/nologo " + IntegrationProperties() + GetLoggerArgument(), Path.Combine(DefaultWorkingDirectory, "src"));
			info.WorkingDirectory = Path.Combine(DefaultWorkingDirectory, "src");
			ExpectToExecute(info);
			task.WorkingDirectory = "src";

			task.Run(result);
		}

		[Test]
		public void TimedOutExecutionShouldFailBuild()
		{
			ExpectToExecuteAndReturn(TimedOutProcessResult());
			task.Run(result);

			ClassicAssert.That(result.Status, Is.EqualTo(IntegrationStatus.Failure));
			ClassicAssert.That(result.TaskOutput, Does.Match("Command line '.*' timed out after \\d+ seconds"));
		}

		[Test]
		public void ShouldAutomaticallyMergeTheBuildOutputFile()
		{
            TempFileUtil.CreateTempXmlFile(string.Format(logfile, task.LogFileId), "<output/>");
			ExpectToExecuteAndReturn(SuccessfulProcessResult());
			task.Run(result);
			ClassicAssert.AreEqual(2, result.TaskResults.Count);
		    ClassicAssert.That(result.TaskOutput, Is.EqualTo("<output/>"));
			ClassicAssert.IsTrue(result.Succeeded);
		}

		[Test]
		public void ShouldFailOnFailedProcessResult()
		{
            TempFileUtil.CreateTempXmlFile(string.Format(logfile, task.LogFileId), "<output/>");
			ExpectToExecuteAndReturn(FailedProcessResult());
			task.Run(result);
			ClassicAssert.AreEqual(2, result.TaskResults.Count);
			ClassicAssert.AreEqual("<output/>" + ProcessResultOutput, result.TaskOutput);
			ClassicAssert.IsTrue(result.Failed);			
		}

		[Test]
		public void PopulateFromConfiguration()
		{
			const string xml = @"<msbuild>
	<executable>C:\WINDOWS\Microsoft.NET\Framework\v2.0.50215\MSBuild.exe</executable>
	<workingDirectory>C:\dev\ccnet</workingDirectory>
	<projectFile>CCNet.sln</projectFile>
	<buildArgs>/p:Configuration=Debug /v:diag</buildArgs>
	<targets>Build;Test</targets>
	<timeout>15</timeout>
	<logger>Kobush.Build.Logging.XmlLogger,Kobush.MSBuild.dll</logger>
    <loggerParameters>
        <loggerParameter>buildresult.xml</loggerParameter>
        <loggerParameter>someField=true</loggerParameter>
    </loggerParameters>
    <priority>BelowNormal</priority>
</msbuild>";
			task = (MsBuildTask) NetReflector.Read(xml);
			ClassicAssert.AreEqual(@"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50215\MSBuild.exe", task.Executable);
			ClassicAssert.AreEqual(@"C:\dev\ccnet", task.WorkingDirectory);
			ClassicAssert.AreEqual("CCNet.sln", task.ProjectFile);
			ClassicAssert.AreEqual("Build;Test", task.Targets);
			ClassicAssert.AreEqual("/p:Configuration=Debug /v:diag", task.BuildArgs);
			ClassicAssert.AreEqual(15, task.Timeout);
			ClassicAssert.AreEqual("Kobush.Build.Logging.XmlLogger,Kobush.MSBuild.dll", task.Logger);
            ClassicAssert.AreEqual(2, task.LoggerParameters.Length);
            ClassicAssert.AreEqual("buildresult.xml", task.LoggerParameters[0]);
            ClassicAssert.AreEqual("someField=true", task.LoggerParameters[1]);
            ClassicAssert.AreEqual(ProcessPriorityClass.BelowNormal, task.Priority);
		}

		[Test]
		public void PopulateFromMinimalConfiguration()
		{
			task = (MsBuildTask) NetReflector.Read("<msbuild />");
			ClassicAssert.AreEqual(defaultExecutable, task.Executable);
			ClassicAssert.AreEqual(MsBuildTask.DefaultTimeout, task.Timeout);
			ClassicAssert.AreEqual(null, task.Logger);
		}

        [Test]
        public void PopulateFromConfigurationBogusLoggerParameters()
        {
            const string xml = @"<msbuild>
	<executable>C:\WINDOWS\Microsoft.NET\Framework\v2.0.50215\MSBuild.exe</executable>
	<workingDirectory>C:\dev\ccnet</workingDirectory>
	<projectFile>CCNet.sln</projectFile>
	<buildArgs>/p:Configuration=Debug /v:diag</buildArgs>
	<targets>Build;Test</targets>
	<timeout>15</timeout>
	<logger>Kobush.Build.Logging.XmlLogger,Kobush.MSBuild.dll;buildresult.xml</logger>
    <priority>BelowNormal</priority>
</msbuild>";
            ClassicAssert.That(delegate { task = (MsBuildTask)NetReflector.Read(xml); },
                        Throws.TypeOf<NetReflectorException>());
            ;
        }

        private string GetLoggerArgument()
		{
			var logger = string.IsNullOrEmpty(task.Logger) ? defaultLogger : task.Logger;
            return string.Format(@" /l:{0};{1}", StringUtil.AutoDoubleQuoteString(logger), StringUtil.AutoDoubleQuoteString(string.Format(logfile, task.LogFileId)));
		}

		private string IntegrationProperties()
		{
			// NOTE: Property names are sorted alphabetically when passed as process arguments
			// Tests that look for the correct arguments will fail if the following properties
			// are not sorted alphabetically.			
            return string.Format(@"/p:CCNetArtifactDirectory={3};CCNetBuildCondition=IfModificationExists;CCNetBuildDate={1};CCNetBuildId={5};CCNetBuildTime={2};CCNetFailureTasks=;CCNetFailureUsers=;CCNetIntegrationStatus=Success;CCNetLabel=1.0;CCNetLastIntegrationStatus=Success;CCNetListenerFile={4};CCNetModifyingUsers=;CCNetNumericLabel=0;CCNetProject=test;CCNetRequestSource=foo;CCNetUser=;CCNetWorkingDirectory={0}", StringUtil.AutoDoubleQuoteString(DefaultWorkingDirectory), testDateString, testTimeString, StringUtil.AutoDoubleQuoteString(result.ArtifactDirectory), StringUtil.AutoDoubleQuoteString(Path.GetTempPath() + "test_ListenFile.xml"), IntegrationResultMother.DefaultBuildId);
		}
	}
}
