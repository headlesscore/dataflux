using System.Diagnostics;
using System.IO;
using System.Xml;
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
	public class ExecutableTaskTest : ProcessExecutorTestFixtureBase
	{
		private const string DefaultExecutable = "run.bat";
		private const string DefaultArgs = "out.txt";
		public const int SUCCESSFUL_EXIT_CODE = 0;
		public const int FAILED_EXIT_CODE = -1;

		private ExecutableTask task;

		[SetUp]
		public void SetUp()
		{
			CreateProcessExecutorMock(DefaultExecutable);
			task = new ExecutableTask((ProcessExecutor) mockProcessExecutor.Object);
			task.Executable = DefaultExecutable;
			task.BuildArgs = DefaultArgs;
		}

		[Test]
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
			ClassicAssert.AreEqual(@"C:\", task.ConfiguredBaseDirectory, "Checking ConfiguredBaseDirectory property.");
			ClassicAssert.AreEqual("mybatchfile.bat", task.Executable, "Checking property.");
			ClassicAssert.AreEqual(123, task.BuildTimeoutSeconds, "Checking BuildTimeoutSeconds property.");
			ClassicAssert.AreEqual("myarg1 myarg2", task.BuildArgs, "Checking BuildArgs property.");
			ClassicAssert.AreEqual(3, task.EnvironmentVariables.Length, "Checking environment variable array size.");
			ClassicAssert.AreEqual("name1", task.EnvironmentVariables[0].name, "Checking name1 environment variable.");
			ClassicAssert.AreEqual("value1", task.EnvironmentVariables[0].value, "Checking name1 environment value.");
			ClassicAssert.AreEqual("name2", task.EnvironmentVariables[1].name, "Checking name2 environment variable.");
			ClassicAssert.AreEqual("", task.EnvironmentVariables[1].value, "Checking name2 environment value.");
			ClassicAssert.AreEqual("name3", task.EnvironmentVariables[2].name, "Checking name3 environment variable.");
			ClassicAssert.AreEqual("value3", task.EnvironmentVariables[2].value, "Checking name3 environment value.");
			ClassicAssert.AreEqual("0,1,3,5", task.SuccessExitCodes);
            ClassicAssert.AreEqual(ProcessPriorityClass.BelowNormal, task.Priority);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
            Verify();
		}

		[Test]
		public void PopulateFromConfigurationUsingOnlyRequiredElementsAndCheckDefaultValues()
		{
			const string xml = @"
    <exec>
    	<executable>mybatchfile.bat</executable>
    </exec>";

			task = (ExecutableTask) NetReflector.Read(xml);
            ClassicAssert.AreEqual("mybatchfile.bat", task.Executable, "Checking property.");
            ClassicAssert.AreEqual(600, task.BuildTimeoutSeconds, "Checking BuildTimeoutSeconds property.");
            ClassicAssert.AreEqual("", task.BuildArgs, "Checking BuildArgs property.");
            ClassicAssert.AreEqual(0, task.EnvironmentVariables.Length, "Checking environment variable array size.");
			ClassicAssert.AreEqual("", task.SuccessExitCodes);
            ClassicAssert.AreEqual(ProcessPriorityClass.Normal, task.Priority);
			Verify();
		}

		[Test]
		public void ShouldSetSuccessfulStatusAndBuildOutputAsAResultOfASuccessfulBuild()
		{
			ExpectToExecuteArguments(DefaultArgs);

			IIntegrationResult result = IntegrationResult();
			task.Run(result);

            StringWriter buffer = new StringWriter();
            new ReflectorTypeAttribute("task").Write(new XmlTextWriter(buffer), task);

            ClassicAssert.IsTrue(result.Succeeded);
			ClassicAssert.AreEqual(IntegrationStatus.Success, result.Status);
            ClassicAssert.AreEqual(System.Environment.NewLine + "<buildresults>" + System.Environment.NewLine
                + buffer.ToString() + System.Environment.NewLine + "  <message>" 
                + ProcessResultOutput + "</message>" + System.Environment.NewLine + "</buildresults>" 
                + System.Environment.NewLine, result.TaskOutput);
            Verify();
		}

		[Test]
		public void ShouldSetFailedStatusAndBuildOutputAsAResultOfFailedBuild()
		{
			ExpectToExecuteAndReturn(FailedProcessResult());

			IIntegrationResult result = IntegrationResult();
			task.Run(result);

            StringWriter buffer = new StringWriter();
            new ReflectorTypeAttribute("task").Write(new XmlTextWriter(buffer), task);

			ClassicAssert.IsTrue(result.Failed);
			ClassicAssert.AreEqual(IntegrationStatus.Failure, result.Status);
            ClassicAssert.AreEqual(System.Environment.NewLine + "<buildresults>" + System.Environment.NewLine
                + buffer.ToString() + System.Environment.NewLine + "  <message>" 
                + ProcessResultOutput + "</message>" + System.Environment.NewLine + "</buildresults>" 
                + System.Environment.NewLine, result.TaskOutput);

			Verify();
		}

		// TODO - Timeout?
		[Test]
		public void ShouldThrowBuilderExceptionIfProcessThrowsException()
		{
			ExpectToExecuteAndThrow();

            ClassicAssert.That(delegate { task.Run(IntegrationResult()); },
                        Throws.TypeOf<BuilderException>());
			Verify();
		}

		[Test]
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

			ClassicAssert.AreEqual("test-exe", info.FileName);
			ClassicAssert.AreEqual(222000, info.TimeOut);
			ClassicAssert.AreEqual("test-args", info.Arguments);
			ClassicAssert.AreEqual("1.0", info.EnvironmentVariables["CCNetLabel"]);
			ClassicAssert.AreEqual("ForceBuild", info.EnvironmentVariables["CCNetBuildCondition"]);
			ClassicAssert.AreEqual(@"c:\workingdir\", info.EnvironmentVariables["CCNetWorkingDirectory"]);
			ClassicAssert.AreEqual(@"c:\artifactdir\", info.EnvironmentVariables["CCNetArtifactDirectory"]);
			Verify();
		}

		[Test]
		public void IfConfiguredBaseDirectoryIsNotSetUseProjectWorkingDirectoryAsBaseDirectory()
		{
			task.ConfiguredBaseDirectory = null;
			CheckBaseDirectoryIsProjectDirectoryWithGivenRelativePart("");
		}

		[Test]
		public void IfConfiguredBaseDirectoryIsEmptyUseProjectWorkingDirectoryAsBaseDirectory()
		{
			task.ConfiguredBaseDirectory = "";
			CheckBaseDirectoryIsProjectDirectoryWithGivenRelativePart("");
		}

		[Test]
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

		[Test]
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

			ClassicAssert.AreEqual(expectedBaseDirectory, info.WorkingDirectory);
			Verify();
		}

        [Test]
        public void ExecutableOutputShouldBeBuildResults()
        {
            ExecutableTask xmlTestTask = new ExecutableTask((ProcessExecutor)mockProcessExecutor.Object);
            xmlTestTask.Executable = DefaultExecutable;
            xmlTestTask.BuildArgs = DefaultArgs;
            ExpectToExecuteArguments(DefaultArgs);

            IIntegrationResult result = IntegrationResult();
            xmlTestTask.Run(result);

            ClassicAssert.IsTrue(result.Succeeded);
            ClassicAssert.AreEqual(IntegrationStatus.Success, result.Status);

            StringWriter buffer = new StringWriter();
            new ReflectorTypeAttribute("task").Write(new XmlTextWriter(buffer), xmlTestTask);
            
            // TODO: The following only works correctly when ProcessResultOutput is a single non-empty line.
            // That is always the case, courtesy of our superclass' initialization.  If that should ever
            // change, this test needs to be adjusted accordingly.
            ClassicAssert.AreEqual(System.Environment.NewLine + "<buildresults>" + System.Environment.NewLine
                + buffer.ToString() + System.Environment.NewLine + "  <message>" 
                + ProcessResultOutput + "</message>" + System.Environment.NewLine + "</buildresults>"
                + System.Environment.NewLine, result.TaskOutput);
            Verify();
        }

		[Test]
		public void ShouldParseValidSuccessExitCodes()
		{
			task.SuccessExitCodes = "0,1,3,5";

			task.SuccessExitCodes = "300,500,-1";
		}

		[Test]
		public void ShouldThrowExceptionOnInvalidSuccessExitCodes()
		{
			ClassicAssert.That(delegate { task.SuccessExitCodes = "0, 1, GOOD"; },
                        Throws.TypeOf<System.FormatException>());
		}

		[Test]
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

			ClassicAssert.IsTrue(info.ProcessSuccessful(0));
			ClassicAssert.IsTrue(info.ProcessSuccessful(1));
			ClassicAssert.IsFalse(info.ProcessSuccessful(2));
			ClassicAssert.IsTrue(info.ProcessSuccessful(3));
			ClassicAssert.IsFalse(info.ProcessSuccessful(4));
			ClassicAssert.IsTrue(info.ProcessSuccessful(5));
			ClassicAssert.IsFalse(info.ProcessSuccessful(6));

			Verify();
		}

		[Test]
		public void ShouldFailIfProcessTimesOut()
		{
			ExpectToExecuteAndReturn(TimedOutProcessResult());

			IIntegrationResult result = IntegrationResult();
			task.Run(result);

			ClassicAssert.That(result.Status, Is.EqualTo(IntegrationStatus.Failure));
			ClassicAssert.That(result.TaskOutput, Does.Match("Command line '.*' timed out after \\d+ seconds"));

			Verify();
		}
	}
}
