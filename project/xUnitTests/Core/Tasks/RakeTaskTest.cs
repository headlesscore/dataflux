using System;
using System.IO;
using Exortech.NetReflector;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
	
	public class RakeTaskTest : ProcessExecutorTestFixtureBase
	{
		private RakeTask builder;
		private IIntegrationResult result;

		// [SetUp]
		public void SetUp()
		{
			CreateProcessExecutorMock(RakeTask.DefaultExecutable);
			builder = new RakeTask((ProcessExecutor) mockProcessExecutor.Object);
			result = IntegrationResult();
			result.Label = "1.0";
		}

		// [TearDown]
		public void TearDown()
		{
			Verify();
		}

		[Fact]
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
			Assert.Equal(@"C:\", builder.BaseDirectory);
			Assert.Equal("Rakefile", builder.Rakefile);
			Assert.Equal(@"C:\ruby\bin\rake.bat", builder.Executable);
			Assert.Equal(2, builder.Targets.Length);
			Assert.Equal("foo", builder.Targets[0]);
			Assert.Equal("bar", builder.Targets[1]);
			Assert.Equal(123, builder.BuildTimeoutSeconds);
			Assert.Equal(true, builder.Quiet);
			Assert.Equal(true, builder.Silent);
			Assert.Equal(true, builder.Trace);
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public void PopulateFromConfigurationUsingOnlyRequiredElementsAndCheckDefaultValues()
		{
			const string xml = @"<rake />";

			NetReflector.Read(xml, builder);
			Assert.Equal("", builder.BaseDirectory);
			Assert.Equal(RakeTask.DefaultExecutable, builder.Executable);
			Assert.Equal(0, builder.Targets.Length);
			Assert.Equal(RakeTask.DefaultBuildTimeout, builder.BuildTimeoutSeconds);
		}

		[Fact]
		public void ShouldSetSuccessfulStatusAndBuildOutputAsAResultOfASuccessfulBuild()
		{
			ExpectToExecuteAndReturn(SuccessfulProcessResult());
			
			builder.Run(result);
			
			Assert.True(result.Succeeded);
			Assert.Equal(IntegrationStatus.Success, result.Status);
			Assert.Equal(StringUtil.MakeBuildResult(SuccessfulProcessResult().StandardOutput, ""), result.TaskOutput);
		}

		[Fact]
		public void ShouldSetFailedStatusAndBuildOutputAsAResultOfFailedBuild()
		{
			ExpectToExecuteAndReturn(FailedProcessResult());
			
			builder.Run(result);
			
			Assert.True(result.Failed);
			Assert.Equal(IntegrationStatus.Failure, result.Status);
			Assert.Equal(StringUtil.MakeBuildResult(FailedProcessResult().StandardOutput, ""), result.TaskOutput);
		}

		[Fact]
		public void TimedOutExecutionShouldFailBuild()
		{
			ExpectToExecuteAndReturn(TimedOutProcessResult());
			builder.Run(result);

			Assert.True(result.Status, Is.EqualTo(IntegrationStatus.Failure));
			Assert.True(result.TaskOutput, Does.Match("Command line '.*' timed out after \\d+ seconds"));
		}
		
		[Fact]
		public void ShouldThrowBuilderExceptionIfProcessThrowsException()
		{
			ExpectToExecuteAndThrow();
            Assert.True(delegate { builder.Run(result); },
                        Throws.TypeOf<BuilderException>());
		}

		[Fact]
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

			Assert.Equal("rake", info.FileName);
			Assert.Equal(222000, info.TimeOut);
			Assert.Equal("myargs", info.Arguments);
			Assert.Equal("1.0", info.EnvironmentVariables["CCNetLabel"]);
			Assert.Equal("ForceBuild", info.EnvironmentVariables["CCNetBuildCondition"]);
			Assert.Equal(DefaultWorkingDirectory, info.EnvironmentVariables["CCNetWorkingDirectory"]);
			Assert.Equal(DefaultWorkingDirectory, info.EnvironmentVariables["CCNetArtifactDirectory"]);
		}

		[Fact]
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

		[Fact]
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

		[Fact]
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
		
		[Fact]
		public void ShouldRunWithMultipleTargetsSpecified()
		{
			ProcessInfo info = null;
			mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).
				Callback<ProcessInfo>(processInfo => info = processInfo).Returns(SuccessfulProcessResult()).Verifiable();

			builder.Targets = new string[] { "targeta", "targetb", "targetc" };
			builder.Run(result);
			
			Assert.Equal("targeta targetb targetc", info.Arguments);
		}

		[Fact]
		public void IfConfiguredBaseDirectoryIsNotSetUseProjectWorkingDirectoryAsBaseDirectory()
		{
			builder.BaseDirectory = null;
			CheckBaseDirectoryIsProjectDirectoryWithGivenRelativePart("");
		}

		[Fact]
		public void IfConfiguredBaseDirectoryIsEmptyUseProjectWorkingDirectoryAsBaseDirectory()
		{
			builder.BaseDirectory = "";
			CheckBaseDirectoryIsProjectDirectoryWithGivenRelativePart("");
		}

		[Fact]
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

		[Fact]
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
			Assert.Equal(expectedBaseDirectory, info.WorkingDirectory);
		}

		[Fact]
		public void ShouldGiveAPresentationValueForTargetsThatIsANewLineSeparatedEquivalentOfAllTargets()
		{
			builder.Targets = new string[] { "target1", "target2" };
			Assert.Equal("target1" + Environment.NewLine + "target2", builder.TargetsForPresentation);
		}

		[Fact]
		public void ShouldWorkForSingleTargetWhenSettingThroughPresentationValue()
		{
			builder.TargetsForPresentation = "target1";
			Assert.Equal("target1", builder.Targets[0]);
			Assert.Equal(1, builder.Targets.Length);
		}

		[Fact]
		public void ShouldSplitAtNewLineWhenSettingThroughPresentationValue()
		{
			builder.TargetsForPresentation = "target1" + Environment.NewLine + "target2";
			Assert.Equal("target1", builder.Targets[0]);
			Assert.Equal("target2", builder.Targets[1]);
			Assert.Equal(2, builder.Targets.Length);
		}

		[Fact]
		public void ShouldWorkForEmptyAndNullStringsWhenSettingThroughPresentationValue()
		{
			builder.TargetsForPresentation = "";
			Assert.Equal(0, builder.Targets.Length);
			builder.TargetsForPresentation = null;
			Assert.Equal(0, builder.Targets.Length);
		}
		
		[Fact]
		public void SilentOptionShouldAddSilentArgument()
		{
			ProcessInfo info = null;
			mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).
				Callback<ProcessInfo>(processInfo => info = processInfo).Returns(SuccessfulProcessResult()).Verifiable();
			builder.Silent = true;
			builder.Run(result);
			Assert.Equal("--silent", info.Arguments);
		}
		
		[Fact]
		public void SilentAndTraceOptionShouldAddSilentAndTraceArgument()
		{
			ProcessInfo info = null;
			mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).
				Callback<ProcessInfo>(processInfo => info = processInfo).Returns(SuccessfulProcessResult()).Verifiable();
			builder.Silent = true;
			builder.Trace = true;
			builder.Run(result);
			Assert.Equal("--silent --trace", info.Arguments);
		}
		
		[Fact]
		public void QuietOptionShouldAddQuietArgument()
		{
			ProcessInfo info = null;
			mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).
				Callback<ProcessInfo>(processInfo => info = processInfo).Returns(SuccessfulProcessResult()).Verifiable();
			builder.Quiet = true;
			builder.Run(result);
			Assert.Equal("--quiet", info.Arguments);
		}
		
		[Fact]
		public void QuietAndTraceOptionShouldAddQuietAndTraceArgument()
		{
			ProcessInfo info = null;
			mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).
				Callback<ProcessInfo>(processInfo => info = processInfo).Returns(SuccessfulProcessResult()).Verifiable();
			builder.Quiet = true;
			builder.Trace = true;
			builder.Run(result);
			Assert.Equal("--quiet --trace", info.Arguments);
		}
		
		[Fact]
		public void TraceOptionShouldAddTraceArgument()
		{
			ProcessInfo info = null;
			mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).
				Callback<ProcessInfo>(processInfo => info = processInfo).Returns(SuccessfulProcessResult()).Verifiable();
			builder.Trace = true;
			builder.Run(result);
			Assert.Equal("--trace", info.Arguments);
		}
		
		[Fact]
		public void SilentAndQuietOptionShouldOnlyAddSilentArgument()
		{
			ProcessInfo info = null;
			mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).
				Callback<ProcessInfo>(processInfo => info = processInfo).Returns(SuccessfulProcessResult()).Verifiable();
			builder.Silent = true;
			builder.Quiet = true;
			builder.Run(result);
			Assert.Equal("--silent", info.Arguments);
		}
		
		[Fact]
		public void ConstructorShouldNotThrowException()
		{
			new RakeTask();
		}
	}
}
