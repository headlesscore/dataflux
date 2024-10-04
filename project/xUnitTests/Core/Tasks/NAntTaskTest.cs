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
	
	public class NAntTaskTest : ProcessExecutorTestFixtureBase
	{
		private NAntTask builder;
		private IIntegrationResult result;

		// [SetUp]
		public void SetUp()
		{
			CreateProcessExecutorMock(NAntTask.defaultExecutable);
			builder = new NAntTask((ProcessExecutor) mockProcessExecutor.Object);
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
    <nant>
    	<executable>NAnt.exe</executable>
    	<baseDirectory>C:\</baseDirectory>
    	<buildFile>mybuild.build</buildFile>
		<targetList>
      		<target>foo</target>
    	</targetList>
		<logger>SourceForge.NAnt.XmlLogger</logger>
		<listener>CCNetListener, CCNetListener</listener>
		<buildTimeoutSeconds>123</buildTimeoutSeconds>
		<nologo>FALSE</nologo>
    </nant>";

			NetReflector.Read(xml, builder);
			Assert.Equal(@"C:\", builder.ConfiguredBaseDirectory);
			Assert.Equal("mybuild.build", builder.BuildFile);
			Assert.Equal("NAnt.exe", builder.Executable);
			Assert.Equal(1, builder.Targets.Length);
			Assert.Equal(123, builder.BuildTimeoutSeconds);
			Assert.Equal("SourceForge.NAnt.XmlLogger", builder.Logger);
			Assert.Equal("CCNetListener, CCNetListener", builder.Listener);
			Assert.Equal("foo", builder.Targets[0]);
			Assert.Equal(false, builder.NoLogo);
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public void PopulateFromConfigurationUsingOnlyRequiredElementsAndCheckDefaultValues()
		{
			const string xml = @"<nant />";

			NetReflector.Read(xml, builder);
			Assert.Equal("", builder.ConfiguredBaseDirectory);
			Assert.Equal(NAntTask.defaultExecutable, builder.Executable);
			Assert.Equal(0, builder.Targets.Length);
			Assert.Equal(NAntTask.DefaultBuildTimeout, builder.BuildTimeoutSeconds);
			Assert.Equal(NAntTask.DefaultLogger, builder.Logger);
			Assert.Equal(NAntTask.DefaultListener, builder.Listener);
			Assert.Equal(NAntTask.DefaultNoLogo, builder.NoLogo);
		}

		[Fact]
		public void ShouldSetSuccessfulStatusAndBuildOutputAsAResultOfASuccessfulBuild()
		{
			ExpectToExecuteAndReturn(SuccessfulProcessResult());
			
			builder.Run(result);
			
			Assert.True(result.Succeeded);
			Assert.Equal(IntegrationStatus.Success, result.Status);
		    Assert.True(result.TaskOutput, Is.Empty);
		}

		[Fact]
		public void ShouldSetFailedStatusAndBuildOutputAsAResultOfFailedBuild()
		{
			ExpectToExecuteAndReturn(FailedProcessResult());
			
			builder.Run(result);
			
			Assert.True(result.Failed);
			Assert.Equal(IntegrationStatus.Failure, result.Status);
			Assert.Equal(FailedProcessResult().StandardOutput, result.TaskOutput);
		}

		[Fact]
		public void ShouldFailBuildIfProcessTimesOut()
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
			result.Label = "1.0";
			result.WorkingDirectory = DefaultWorkingDirectory;
			result.ArtifactDirectory = DefaultWorkingDirectory;

            string args = @"-nologo -buildfile:mybuild.build -logger:NAnt.Core.XmlLogger -logfile:" + StringUtil.AutoDoubleQuoteString(Path.Combine(result.ArtifactDirectory, string.Format(NAntTask.logFilename, builder.LogFileId))) + " -listener:NAnt.Core.DefaultLogger myArgs " + IntegrationProperties(DefaultWorkingDirectory, DefaultWorkingDirectory) + " target1 target2";
			ProcessInfo info = NewProcessInfo(args, DefaultWorkingDirectory);
			info.TimeOut = 2000;
			ExpectToExecute(info);

			builder.ConfiguredBaseDirectory = DefaultWorkingDirectory;
			builder.BuildFile = "mybuild.build";
			builder.BuildArgs = "myArgs";
			builder.Targets = new string[] {"target1", "target2"};
			builder.BuildTimeoutSeconds = 2;
			builder.Run(result);
		}

		[Fact]
		public void ShouldPassAppropriateDefaultPropertiesAsProcessInfoArgumentsToProcessExecutor()
		{
			result.ArtifactDirectory = DefaultWorkingDirectory;
			result.WorkingDirectory = DefaultWorkingDirectory;
            ExpectToExecuteArguments(@"-nologo -logger:NAnt.Core.XmlLogger -logfile:" + StringUtil.AutoDoubleQuoteString(Path.Combine(result.ArtifactDirectory, string.Format(NAntTask.logFilename, builder.LogFileId))) + " -listener:NAnt.Core.DefaultLogger " + IntegrationProperties(DefaultWorkingDirectory, DefaultWorkingDirectory));
			builder.ConfiguredBaseDirectory = DefaultWorkingDirectory;
			builder.Run(result);
		}

		[Fact]
		public void ShouldPutQuotesAroundBuildFileIfItContainsASpace()
		{
			result.ArtifactDirectory = DefaultWorkingDirectory;
			result.WorkingDirectory = DefaultWorkingDirectory;
            ExpectToExecuteArguments(@"-nologo -buildfile:""my project.build"" -logger:NAnt.Core.XmlLogger -logfile:" + StringUtil.AutoDoubleQuoteString(Path.Combine(result.ArtifactDirectory, string.Format(NAntTask.logFilename, builder.LogFileId))) + " -listener:NAnt.Core.DefaultLogger " + IntegrationProperties(DefaultWorkingDirectory, DefaultWorkingDirectory));

			builder.BuildFile = "my project.build";
			builder.ConfiguredBaseDirectory = DefaultWorkingDirectory;
			builder.Run(result);
		}

		[Fact]
		public void ShouldEncloseDirectoriesInQuotesIfTheyContainSpaces()
		{
			result.ArtifactDirectory = DefaultWorkingDirectoryWithSpaces;
			result.WorkingDirectory = DefaultWorkingDirectoryWithSpaces;

            ExpectToExecuteArguments(@"-nologo -logger:NAnt.Core.XmlLogger -logfile:" + StringUtil.AutoDoubleQuoteString(Path.Combine(result.ArtifactDirectory, string.Format(NAntTask.logFilename, builder.LogFileId))) + " -listener:NAnt.Core.DefaultLogger " + IntegrationProperties(DefaultWorkingDirectoryWithSpaces, DefaultWorkingDirectoryWithSpaces), DefaultWorkingDirectoryWithSpaces);

			builder.ConfiguredBaseDirectory = DefaultWorkingDirectoryWithSpaces;
			builder.Run(result);
		}

		[Fact]
		public void IfConfiguredBaseDirectoryIsNotSetUseProjectWorkingDirectoryAsBaseDirectory()
		{
			builder.ConfiguredBaseDirectory = null;
			CheckBaseDirectoryIsProjectDirectoryWithGivenRelativePart("");
		}

		[Fact]
		public void IfConfiguredBaseDirectoryIsEmptyUseProjectWorkingDirectoryAsBaseDirectory()
		{
			builder.ConfiguredBaseDirectory = "";
			CheckBaseDirectoryIsProjectDirectoryWithGivenRelativePart("");
		}

		[Fact]
		public void IfConfiguredBaseDirectoryIsNotAbsoluteUseProjectWorkingDirectoryAsFirstPartOfBaseDirectory()
		{
			builder.ConfiguredBaseDirectory = "relativeBaseDirectory";
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
			builder.ConfiguredBaseDirectory = DefaultWorkingDirectory;
			CheckBaseDirectory(IntegrationResultForWorkingDirectoryTest(), DefaultWorkingDirectory);
		}
		
		private void CheckBaseDirectory(IIntegrationResult integrationResult, string expectedBaseDirectory)
		{
			ProcessResult returnVal = SuccessfulProcessResult();
			ProcessInfo info = null;
			mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).
				Callback<ProcessInfo>(processInfo => info = processInfo).Returns(returnVal).Verifiable();
			builder.Run(integrationResult);
			Assert.Equal(expectedBaseDirectory, info.WorkingDirectory);
		}
		
		[Fact]
		public void ShouldGiveAPresentationValueForTargetsThatIsANewLineSeparatedEquivalentOfAllTargets()
		{
			builder.Targets = new string[] {"target1", "target2"};
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

		private string IntegrationProperties(string workingDirectory, string artifactDirectory)
		{
			// NOTE: Property names are sorted alphabetically when passed as process arguments
			// Tests that look for the correct arguments will fail if the following properties
			// are not sorted alphabetically.
            return string.Format(@"-D:CCNetArtifactDirectory={1} -D:CCNetBuildCondition=IfModificationExists -D:CCNetBuildDate={2} -D:CCNetBuildId={5} -D:CCNetBuildTime={3} -D:CCNetFailureTasks= -D:CCNetFailureUsers= -D:CCNetIntegrationStatus=Success -D:CCNetLabel=1.0 -D:CCNetLastIntegrationStatus=Success -D:CCNetListenerFile={4} -D:CCNetModifyingUsers= -D:CCNetNumericLabel=0 -D:CCNetProject=test -D:CCNetRequestSource=foo -D:CCNetWorkingDirectory={0}", StringUtil.AutoDoubleQuoteString(workingDirectory), StringUtil.AutoDoubleQuoteString(artifactDirectory), testDateString, testTimeString, StringUtil.AutoDoubleQuoteString(Path.GetTempPath() + "test_ListenFile.xml"), IntegrationResultMother.DefaultBuildId);
        }
	}
}
