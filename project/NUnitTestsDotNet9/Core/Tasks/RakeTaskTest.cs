using System;
using System.IO;
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
	public class RakeTaskTest : ProcessExecutorTestFixtureBase
	{
		private RakeTask builder;
		private IIntegrationResult result;

		[SetUp]
		public void SetUp()
		{
			CreateProcessExecutorMock(RakeTask.DefaultExecutable);
			builder = new RakeTask((ProcessExecutor) mockProcessExecutor.Object);
			result = IntegrationResult();
			result.Label = "1.0";
		}

		[TearDown]
		public void TearDown()
		{
			Verify();
		}

		[Test]
		public void PopulateFromReflector()
		{
			const string xml = @"
    <rake>
    	<executable>C:\ruby\bin\rake.bat</executable>
    	<baseDirectory>C:\</baseDirectory>
    	<rakefile>Rakefile</rakefile>
		<targetList>
			<target>foo</target>
			<target>bar</target>
		</targetList>
		<buildTimeoutSeconds>123</buildTimeoutSeconds>
		<quiet>true</quiet>
		<silent>true</silent>
		<trace>true</trace>
    </rake>";

			NetReflector.Read(xml, builder);
			ClassicAssert.AreEqual(@"C:\", builder.BaseDirectory);
			ClassicAssert.AreEqual("Rakefile", builder.Rakefile);
			ClassicAssert.AreEqual(@"C:\ruby\bin\rake.bat", builder.Executable);
			ClassicAssert.AreEqual(2, builder.Targets.Length);
			ClassicAssert.AreEqual("foo", builder.Targets[0]);
			ClassicAssert.AreEqual("bar", builder.Targets[1]);
			ClassicAssert.AreEqual(123, builder.BuildTimeoutSeconds);
			ClassicAssert.AreEqual(true, builder.Quiet);
			ClassicAssert.AreEqual(true, builder.Silent);
			ClassicAssert.AreEqual(true, builder.Trace);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

		[Test]
		public void PopulateFromConfigurationUsingOnlyRequiredElementsAndCheckDefaultValues()
		{
			const string xml = @"<rake />";

			NetReflector.Read(xml, builder);
			ClassicAssert.AreEqual("", builder.BaseDirectory);
			ClassicAssert.AreEqual(RakeTask.DefaultExecutable, builder.Executable);
			ClassicAssert.AreEqual(0, builder.Targets.Length);
			ClassicAssert.AreEqual(RakeTask.DefaultBuildTimeout, builder.BuildTimeoutSeconds);
		}

		[Test]
		public void ShouldSetSuccessfulStatusAndBuildOutputAsAResultOfASuccessfulBuild()
		{
			ExpectToExecuteAndReturn(SuccessfulProcessResult());
			
			builder.Run(result);
			
			ClassicAssert.IsTrue(result.Succeeded);
			ClassicAssert.AreEqual(IntegrationStatus.Success, result.Status);
			ClassicAssert.AreEqual(StringUtil.MakeBuildResult(SuccessfulProcessResult().StandardOutput, ""), result.TaskOutput);
		}

		[Test]
		public void ShouldSetFailedStatusAndBuildOutputAsAResultOfFailedBuild()
		{
			ExpectToExecuteAndReturn(FailedProcessResult());
			
			builder.Run(result);
			
			ClassicAssert.IsTrue(result.Failed);
			ClassicAssert.AreEqual(IntegrationStatus.Failure, result.Status);
			ClassicAssert.AreEqual(StringUtil.MakeBuildResult(FailedProcessResult().StandardOutput, ""), result.TaskOutput);
		}

		[Test]
		public void TimedOutExecutionShouldFailBuild()
		{
			ExpectToExecuteAndReturn(TimedOutProcessResult());
			builder.Run(result);

			ClassicAssert.That(result.Status, Is.EqualTo(IntegrationStatus.Failure));
			ClassicAssert.That(result.TaskOutput, Does.Match("Command line '.*' timed out after \\d+ seconds"));
		}
		
		[Test]
		public void ShouldThrowBuilderExceptionIfProcessThrowsException()
		{
			ExpectToExecuteAndThrow();
            ClassicAssert.That(delegate { builder.Run(result); },
                        Throws.TypeOf<BuilderException>());
		}

		[Test]
		public void ShouldPassSpecifiedPropertiesAsProcessInfoArgumentsToProcessExecutor()
		{
			ProcessInfo info = null;
			mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).
				Callback<ProcessInfo>(processInfo => info = processInfo).Returns(SuccessfulProcessResult()).Verifiable();

			IntegrationResult integrationResult = (IntegrationResult)IntegrationResult();
			integrationResult.ProjectName = "test";
			integrationResult.Label = "1.0";
			integrationResult.BuildCondition = BuildCondition.ForceBuild;
			integrationResult.WorkingDirectory = DefaultWorkingDirectory;
			integrationResult.ArtifactDirectory = DefaultWorkingDirectory;

			builder.Executable = "rake";
			builder.BuildArgs = "myargs";
			builder.BuildTimeoutSeconds = 222;
			builder.Run(integrationResult);

			ClassicAssert.AreEqual("rake", info.FileName);
			ClassicAssert.AreEqual(222000, info.TimeOut);
			ClassicAssert.AreEqual("myargs", info.Arguments);
			ClassicAssert.AreEqual("1.0", info.EnvironmentVariables["CCNetLabel"]);
			ClassicAssert.AreEqual("ForceBuild", info.EnvironmentVariables["CCNetBuildCondition"]);
			ClassicAssert.AreEqual(DefaultWorkingDirectory, info.EnvironmentVariables["CCNetWorkingDirectory"]);
			ClassicAssert.AreEqual(DefaultWorkingDirectory, info.EnvironmentVariables["CCNetArtifactDirectory"]);
		}

		[Test]
		public void ShouldPassAppropriateDefaultPropertiesAsProcessInfoArgumentsToProcessExecutor()
		{
			ExpectToExecuteArguments("");

			builder.Rakefile = "";
			builder.BuildArgs = "";
			builder.BaseDirectory = DefaultWorkingDirectory;
			result.ArtifactDirectory = DefaultWorkingDirectory;
			result.WorkingDirectory = DefaultWorkingDirectory;
			builder.Run(result);
		}

		[Test]
		public void ShouldPutQuotesAroundBuildFileIfItContainsASpace()
		{
			ExpectToExecuteArguments(@"--rakefile ""my project.rake""");

			builder.Rakefile = "my project.rake";
			builder.BuildArgs = "";
			builder.BaseDirectory = DefaultWorkingDirectory;
			result.ArtifactDirectory = DefaultWorkingDirectory;
			result.WorkingDirectory = DefaultWorkingDirectory;
			builder.Run(result);
		}

		[Test]
		public void ShouldEncloseDirectoriesInQuotesIfTheyContainSpaces()
		{
			ExpectToExecuteArguments("", DefaultWorkingDirectoryWithSpaces);

			builder.Rakefile = "";
			builder.BuildArgs = "";
			builder.BaseDirectory = DefaultWorkingDirectoryWithSpaces;
			result.ArtifactDirectory = DefaultWorkingDirectoryWithSpaces;
			result.WorkingDirectory = DefaultWorkingDirectoryWithSpaces;
			builder.Run(result);
		}
		
		[Test]
		public void ShouldRunWithMultipleTargetsSpecified()
		{
			ProcessInfo info = null;
			mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).
				Callback<ProcessInfo>(processInfo => info = processInfo).Returns(SuccessfulProcessResult()).Verifiable();

			builder.Targets = new string[] { "targeta", "targetb", "targetc" };
			builder.Run(result);
			
			ClassicAssert.AreEqual("targeta targetb targetc", info.Arguments);
		}

		[Test]
		public void IfConfiguredBaseDirectoryIsNotSetUseProjectWorkingDirectoryAsBaseDirectory()
		{
			builder.BaseDirectory = null;
			CheckBaseDirectoryIsProjectDirectoryWithGivenRelativePart("");
		}

		[Test]
		public void IfConfiguredBaseDirectoryIsEmptyUseProjectWorkingDirectoryAsBaseDirectory()
		{
			builder.BaseDirectory = "";
			CheckBaseDirectoryIsProjectDirectoryWithGivenRelativePart("");
		}

		[Test]
		public void IfConfiguredBaseDirectoryIsNotAbsoluteUseProjectWorkingDirectoryAsFirstPartOfBaseDirectory()
		{
			builder.BaseDirectory = "relativeBaseDirectory";
			CheckBaseDirectoryIsProjectDirectoryWithGivenRelativePart("relativeBaseDirectory");
		}

		private void CheckBaseDirectoryIsProjectDirectoryWithGivenRelativePart(string relativeDirectory)
		{
			string expectedBaseDirectory = "projectWorkingDirectory";
			if (relativeDirectory.Length > 0)
			{
				expectedBaseDirectory = Path.Combine(expectedBaseDirectory, relativeDirectory);
			}
			CheckBaseDirectory(IntegrationResultForWorkingDirectoryTest(), expectedBaseDirectory);
		}

		[Test]
		public void IfConfiguredBaseDirectoryIsAbsoluteUseItAtBaseDirectory()
		{
			builder.BaseDirectory = DefaultWorkingDirectory;
			CheckBaseDirectory(IntegrationResultForWorkingDirectoryTest(), DefaultWorkingDirectory);
		}

		private void CheckBaseDirectory(IIntegrationResult integrationResult, string expectedBaseDirectory)
		{
			ProcessInfo info = null;
			mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).
				Callback<ProcessInfo>(processInfo => info = processInfo).Returns(SuccessfulProcessResult()).Verifiable();
			builder.Run(integrationResult);
			ClassicAssert.AreEqual(expectedBaseDirectory, info.WorkingDirectory);
		}

		[Test]
		public void ShouldGiveAPresentationValueForTargetsThatIsANewLineSeparatedEquivalentOfAllTargets()
		{
			builder.Targets = new string[] { "target1", "target2" };
			ClassicAssert.AreEqual("target1" + Environment.NewLine + "target2", builder.TargetsForPresentation);
		}

		[Test]
		public void ShouldWorkForSingleTargetWhenSettingThroughPresentationValue()
		{
			builder.TargetsForPresentation = "target1";
			ClassicAssert.AreEqual("target1", builder.Targets[0]);
			ClassicAssert.AreEqual(1, builder.Targets.Length);
		}

		[Test]
		public void ShouldSplitAtNewLineWhenSettingThroughPresentationValue()
		{
			builder.TargetsForPresentation = "target1" + Environment.NewLine + "target2";
			ClassicAssert.AreEqual("target1", builder.Targets[0]);
			ClassicAssert.AreEqual("target2", builder.Targets[1]);
			ClassicAssert.AreEqual(2, builder.Targets.Length);
		}

		[Test]
		public void ShouldWorkForEmptyAndNullStringsWhenSettingThroughPresentationValue()
		{
			builder.TargetsForPresentation = "";
			ClassicAssert.AreEqual(0, builder.Targets.Length);
			builder.TargetsForPresentation = null;
			ClassicAssert.AreEqual(0, builder.Targets.Length);
		}
		
		[Test]
		public void SilentOptionShouldAddSilentArgument()
		{
			ProcessInfo info = null;
			mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).
				Callback<ProcessInfo>(processInfo => info = processInfo).Returns(SuccessfulProcessResult()).Verifiable();
			builder.Silent = true;
			builder.Run(result);
			ClassicAssert.AreEqual("--silent", info.Arguments);
		}
		
		[Test]
		public void SilentAndTraceOptionShouldAddSilentAndTraceArgument()
		{
			ProcessInfo info = null;
			mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).
				Callback<ProcessInfo>(processInfo => info = processInfo).Returns(SuccessfulProcessResult()).Verifiable();
			builder.Silent = true;
			builder.Trace = true;
			builder.Run(result);
			ClassicAssert.AreEqual("--silent --trace", info.Arguments);
		}
		
		[Test]
		public void QuietOptionShouldAddQuietArgument()
		{
			ProcessInfo info = null;
			mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).
				Callback<ProcessInfo>(processInfo => info = processInfo).Returns(SuccessfulProcessResult()).Verifiable();
			builder.Quiet = true;
			builder.Run(result);
			ClassicAssert.AreEqual("--quiet", info.Arguments);
		}
		
		[Test]
		public void QuietAndTraceOptionShouldAddQuietAndTraceArgument()
		{
			ProcessInfo info = null;
			mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).
				Callback<ProcessInfo>(processInfo => info = processInfo).Returns(SuccessfulProcessResult()).Verifiable();
			builder.Quiet = true;
			builder.Trace = true;
			builder.Run(result);
			ClassicAssert.AreEqual("--quiet --trace", info.Arguments);
		}
		
		[Test]
		public void TraceOptionShouldAddTraceArgument()
		{
			ProcessInfo info = null;
			mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).
				Callback<ProcessInfo>(processInfo => info = processInfo).Returns(SuccessfulProcessResult()).Verifiable();
			builder.Trace = true;
			builder.Run(result);
			ClassicAssert.AreEqual("--trace", info.Arguments);
		}
		
		[Test]
		public void SilentAndQuietOptionShouldOnlyAddSilentArgument()
		{
			ProcessInfo info = null;
			mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).
				Callback<ProcessInfo>(processInfo => info = processInfo).Returns(SuccessfulProcessResult()).Verifiable();
			builder.Silent = true;
			builder.Quiet = true;
			builder.Run(result);
			ClassicAssert.AreEqual("--silent", info.Arguments);
		}
		
		[Test]
		public void ConstructorShouldNotThrowException()
		{
			new RakeTask();
		}
	}
}
