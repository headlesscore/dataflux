using System.Diagnostics;
using System.IO;
using System.Xml;
using Exortech.NetReflector;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
	
	public class ExecutableTaskTest : ProcessExecutorTestFixtureBase
	{
		private const string DefaultExecutable = "run.bat";
		private const string DefaultArgs = "out.txt";
		public const int SUCCESSFUL_EXIT_CODE = 0;
		public const int FAILED_EXIT_CODE = -1;

		private ExecutableTask task;

		// [SetUp]
		public void SetUp()
		{
			CreateProcessExecutorMock(DefaultExecutable);
			task = new ExecutableTask((ProcessExecutor) mockProcessExecutor.Object);
			task.Executable = DefaultExecutable;
			task.BuildArgs = DefaultArgs;
		}

		[Fact]
		public void PopulateFromReflector()
		{
			const string xml = @"
    <exec>
    	<executable>mybatchfile.bat</executable>
    	<baseDirectory>C:\</baseDirectory>
	<buildArgs>myarg1 myarg2</buildArgs>
	<buildTimeoutSeconds>123</buildTimeoutSeconds>
        <environment>
		<variable name=""name1"" value=""value1""/>
		<variable><name>name2</name></variable>
		<variable name=""name3""><value>value3</value></variable>
	</environment>
    <priority>BelowNormal</priority>
	<successExitCodes>0,1,3,5</successExitCodes>
    </exec>";

			task = (ExecutableTask) NetReflector.Read(xml);
			Assert.Equal(@"C:\", task.ConfiguredBaseDirectory, "Checking ConfiguredBaseDirectory property.");
			Assert.Equal("mybatchfile.bat", task.Executable, "Checking property.");
			Assert.Equal(123, task.BuildTimeoutSeconds, "Checking BuildTimeoutSeconds property.");
			Assert.Equal("myarg1 myarg2", task.BuildArgs, "Checking BuildArgs property.");
			Assert.Equal(3, task.EnvironmentVariables.Length, "Checking environment variable array size.");
			Assert.Equal("name1", task.EnvironmentVariables[0].name, "Checking name1 environment variable.");
			Assert.Equal("value1", task.EnvironmentVariables[0].value, "Checking name1 environment value.");
			Assert.Equal("name2", task.EnvironmentVariables[1].name, "Checking name2 environment variable.");
			Assert.Equal("", task.EnvironmentVariables[1].value, "Checking name2 environment value.");
			Assert.Equal("name3", task.EnvironmentVariables[2].name, "Checking name3 environment variable.");
			Assert.Equal("value3", task.EnvironmentVariables[2].value, "Checking name3 environment value.");
			Assert.Equal("0,1,3,5", task.SuccessExitCodes);
            Assert.Equal(ProcessPriorityClass.BelowNormal, task.Priority);
            Assert.True(true);
            Assert.True(true);
            Verify();
		}

		[Fact]
		public void PopulateFromConfigurationUsingOnlyRequiredElementsAndCheckDefaultValues()
		{
			const string xml = @"
    <exec>
    	<executable>mybatchfile.bat</executable>
    </exec>";

			task = (ExecutableTask) NetReflector.Read(xml);
            Assert.Equal("mybatchfile.bat", task.Executable, "Checking property.");
            Assert.Equal(600, task.BuildTimeoutSeconds, "Checking BuildTimeoutSeconds property.");
            Assert.Equal("", task.BuildArgs, "Checking BuildArgs property.");
            Assert.Equal(0, task.EnvironmentVariables.Length, "Checking environment variable array size.");
			Assert.Equal("", task.SuccessExitCodes);
            Assert.Equal(ProcessPriorityClass.Normal, task.Priority);
			Verify();
		}

		[Fact]
		public void ShouldSetSuccessfulStatusAndBuildOutputAsAResultOfASuccessfulBuild()
		{
			ExpectToExecuteArguments(DefaultArgs);

			IIntegrationResult result = IntegrationResult();
			task.Run(result);

            StringWriter buffer = new StringWriter();
            new ReflectorTypeAttribute("task").Write(new XmlTextWriter(buffer), task);

            Assert.True(result.Succeeded);
			Assert.Equal(IntegrationStatus.Success, result.Status);
            Assert.Equal(System.Environment.NewLine + "<buildresults>" + System.Environment.NewLine
                + buffer.ToString() + System.Environment.NewLine + "  <message>" 
                + ProcessResultOutput + "</message>" + System.Environment.NewLine + "</buildresults>" 
                + System.Environment.NewLine, result.TaskOutput);
            Verify();
		}

		[Fact]
		public void ShouldSetFailedStatusAndBuildOutputAsAResultOfFailedBuild()
		{
			ExpectToExecuteAndReturn(FailedProcessResult());

			IIntegrationResult result = IntegrationResult();
			task.Run(result);

            StringWriter buffer = new StringWriter();
            new ReflectorTypeAttribute("task").Write(new XmlTextWriter(buffer), task);

			Assert.True(result.Failed);
			Assert.Equal(IntegrationStatus.Failure, result.Status);
            Assert.Equal(System.Environment.NewLine + "<buildresults>" + System.Environment.NewLine
                + buffer.ToString() + System.Environment.NewLine + "  <message>" 
                + ProcessResultOutput + "</message>" + System.Environment.NewLine + "</buildresults>" 
                + System.Environment.NewLine, result.TaskOutput);

			Verify();
		}

		// TODO - Timeout?
		[Fact]
		public void ShouldThrowBuilderExceptionIfProcessThrowsException()
		{
			ExpectToExecuteAndThrow();

            Assert.True(delegate { task.Run(IntegrationResult()); },
                        Throws.TypeOf<BuilderException>());
			Verify();
		}

		[Fact]
		public void ShouldPassSpecifiedPropertiesAsProcessInfoArgumentsToProcessExecutor()
		{
			ProcessInfo info = null;
			mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).
				Callback<ProcessInfo>(processInfo => info = processInfo).Returns(SuccessfulProcessResult()).Verifiable();

			IntegrationResult result = (IntegrationResult) IntegrationResult();
			result.Label = "1.0";
			result.BuildCondition = BuildCondition.ForceBuild;
			result.WorkingDirectory = @"c:\workingdir\";
			result.ArtifactDirectory = @"c:\artifactdir\";

			task.Executable = "test-exe";
			task.BuildArgs = "test-args";
			task.BuildTimeoutSeconds = 222;
			task.Run(result);

			Assert.Equal("test-exe", info.FileName);
			Assert.Equal(222000, info.TimeOut);
			Assert.Equal("test-args", info.Arguments);
			Assert.Equal("1.0", info.EnvironmentVariables["CCNetLabel"]);
			Assert.Equal("ForceBuild", info.EnvironmentVariables["CCNetBuildCondition"]);
			Assert.Equal(@"c:\workingdir\", info.EnvironmentVariables["CCNetWorkingDirectory"]);
			Assert.Equal(@"c:\artifactdir\", info.EnvironmentVariables["CCNetArtifactDirectory"]);
			Verify();
		}

		[Fact]
		public void IfConfiguredBaseDirectoryIsNotSetUseProjectWorkingDirectoryAsBaseDirectory()
		{
			task.ConfiguredBaseDirectory = null;
			CheckBaseDirectoryIsProjectDirectoryWithGivenRelativePart("");
		}

		[Fact]
		public void IfConfiguredBaseDirectoryIsEmptyUseProjectWorkingDirectoryAsBaseDirectory()
		{
			task.ConfiguredBaseDirectory = "";
			CheckBaseDirectoryIsProjectDirectoryWithGivenRelativePart("");
		}

		[Fact]
		public void IfConfiguredBaseDirectoryIsNotAbsoluteUseProjectWorkingDirectoryAsFirstPartOfBaseDirectory()
		{
			task.ConfiguredBaseDirectory = "relativeBaseDirectory";
			CheckBaseDirectoryIsProjectDirectoryWithGivenRelativePart("relativeBaseDirectory");
		}

		private void CheckBaseDirectoryIsProjectDirectoryWithGivenRelativePart(string relativeDirectory)
		{
			string expectedBaseDirectory = "projectWorkingDirectory";
			if (relativeDirectory != "")
			{
				expectedBaseDirectory = Path.Combine(expectedBaseDirectory, relativeDirectory);
			}
			CheckBaseDirectory(IntegrationResultForWorkingDirectoryTest(), expectedBaseDirectory);
		}

		[Fact]
		public void IfConfiguredBaseDirectoryIsAbsoluteUseItAtBaseDirectory()
		{
            string path = Platform.IsWindows ? @"c:\my\base\directory" : @"/my/base/directory";
        
			task.ConfiguredBaseDirectory = path;
			CheckBaseDirectory(IntegrationResultForWorkingDirectoryTest(), path);
		}

		private void CheckBaseDirectory(IIntegrationResult result, string expectedBaseDirectory)
		{
			ProcessInfo info = null;
			mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).
				Callback<ProcessInfo>(processInfo => info = processInfo).Returns(SuccessfulProcessResult()).Verifiable();

			task.Run(result);

			Assert.Equal(expectedBaseDirectory, info.WorkingDirectory);
			Verify();
		}

        [Fact]
        public void ExecutableOutputShouldBeBuildResults()
        {
            ExecutableTask xmlTestTask = new ExecutableTask((ProcessExecutor)mockProcessExecutor.Object);
            xmlTestTask.Executable = DefaultExecutable;
            xmlTestTask.BuildArgs = DefaultArgs;
            ExpectToExecuteArguments(DefaultArgs);

            IIntegrationResult result = IntegrationResult();
            xmlTestTask.Run(result);

            Assert.True(result.Succeeded);
            Assert.Equal(IntegrationStatus.Success, result.Status);

            StringWriter buffer = new StringWriter();
            new ReflectorTypeAttribute("task").Write(new XmlTextWriter(buffer), xmlTestTask);
            
            // TODO: The following only works correctly when ProcessResultOutput is a single non-empty line.
            // That is always the case, courtesy of our superclass' initialization.  If that should ever
            // change, this test needs to be adjusted accordingly.
            Assert.Equal(System.Environment.NewLine + "<buildresults>" + System.Environment.NewLine
                + buffer.ToString() + System.Environment.NewLine + "  <message>" 
                + ProcessResultOutput + "</message>" + System.Environment.NewLine + "</buildresults>"
                + System.Environment.NewLine, result.TaskOutput);
            Verify();
        }

		[Fact]
		public void ShouldParseValidSuccessExitCodes()
		{
			task.SuccessExitCodes = "0,1,3,5";

			task.SuccessExitCodes = "300,500,-1";
		}

		[Fact]
		public void ShouldThrowExceptionOnInvalidSuccessExitCodes()
		{
			Assert.True(delegate { task.SuccessExitCodes = "0, 1, GOOD"; },
                        Throws.TypeOf<System.FormatException>());
		}

		[Fact]
		public void ShouldPassSuccessExitCodesToProcessExecutor()
		{
			ProcessInfo info = null;
			mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).
				Callback<ProcessInfo>(processInfo => info = processInfo).Returns(SuccessfulProcessResult()).Verifiable();

			IntegrationResult result = (IntegrationResult)IntegrationResult();
			result.Label = "1.0";
			result.BuildCondition = BuildCondition.ForceBuild;
			result.WorkingDirectory = @"c:\workingdir\";
			result.ArtifactDirectory = @"c:\artifactdir\";

			task.SuccessExitCodes = "0,1,3,5";
			task.Run(result);

			Assert.True(info.ProcessSuccessful(0));
			Assert.True(info.ProcessSuccessful(1));
			Assert.False(info.ProcessSuccessful(2));
			Assert.True(info.ProcessSuccessful(3));
			Assert.False(info.ProcessSuccessful(4));
			Assert.True(info.ProcessSuccessful(5));
			Assert.False(info.ProcessSuccessful(6));

			Verify();
		}

		[Fact]
		public void ShouldFailIfProcessTimesOut()
		{
			ExpectToExecuteAndReturn(TimedOutProcessResult());

			IIntegrationResult result = IntegrationResult();
			task.Run(result);

			Assert.True(result.Status, Is.EqualTo(IntegrationStatus.Failure));
			Assert.True(result.TaskOutput, Does.Match("Command line '.*' timed out after \\d+ seconds"));

			Verify();
		}
	}
}
