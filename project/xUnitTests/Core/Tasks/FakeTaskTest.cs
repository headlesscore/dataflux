using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Exortech.NetReflector;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
    
    public class FakeTaskTest
    {
        private MockRepository mocks;
        private readonly string DefaultWorkingDirectory = Path.GetFullPath(Path.Combine(".", "source"));
        private readonly string DefaultWorkingDirectoryWithSpaces = Path.GetFullPath(Path.Combine(".", "source code"));
        private ProcessExecutor executor;

        // [SetUp]
        public void Setup()
        {
            mocks = new MockRepository(MockBehavior.Default);
            executor = mocks.Create<ProcessExecutor>(MockBehavior.Strict).Object;
        }

        [Fact]
        public void PopulateFromReflector()
        {
            var task = new FakeTask();
            const string xml = @"
    <fake>
    	<executable>C:\FAKE.exe</executable>
    	<baseDirectory>C:\</baseDirectory>
		<buildFile>mybuild.fx</buildFile>
		<description>Test description</description>
    </fake>";

            NetReflector.Read(xml, task);
            Assert.Equal(@"C:\FAKE.exe", task.Executable);
            Assert.Equal(@"C:\", task.ConfiguredBaseDirectory);
            Assert.Equal("mybuild.fx", task.BuildFile);
            Assert.Equal("Test description", task.Description);
            Assert.True(true);
            Assert.True(true);
        }

        [Fact]
        public void PopulateFromConfigurationUsingOnlyRequiredElementsAndCheckDefaultValues()
        {
            
            var task = new FakeTask();
            const string xml = "<fake />";

            NetReflector.Read(xml, task);
            Assert.Equal(FakeTask.defaultExecutable, task.Executable);
            Assert.Equal(string.Empty, task.ConfiguredBaseDirectory);
            Assert.Equal(string.Empty, task.BuildFile);
            Assert.Equal(null, task.Description);
        }

        [Fact]
        public void ExecuteRunsFakeWithDefaults()
        {
            var workingDir = Path.Combine(DefaultWorkingDirectory, "WorkingDir");
            var artefactDir = Path.Combine(DefaultWorkingDirectoryWithSpaces, "ArtifactsDir");
            var buildFile = Path.Combine(DefaultWorkingDirectory, "ccnet.fsx");

            var result = GenerateResultMock(workingDir, artefactDir);
            var task = new FakeTask(executor);
            task.BuildFile = buildFile;
            SetupExecutorMock(executor, "FAKE.exe", string.Concat(StringUtil.AutoDoubleQuoteString(buildFile), " ", "logfile=", StringUtil.AutoDoubleQuoteString(Path.Combine(artefactDir, string.Format(FakeTask.logFilename, task.LogFileId)))), workingDir, 600000);
            Mock.Get(result).SetupProperty(_result => _result.Status);

            result.Status = IntegrationStatus.Unknown;
            task.Run(result);
            mocks.Verify();
        }

				[Fact]
				public void ShouldFailIfProcessTimesOut()
				{
					ExecutorShouldTimeOut(executor);

					var task = new FakeTask(executor);

					var result = IntegrationResultMother.CreateUnknown();
					task.Run(result);
					
					mocks.VerifyAll();
					Assert.True(result.Status, Is.EqualTo(IntegrationStatus.Failure));
					Assert.True(result.TaskOutput, Does.Match("Command line '.*' timed out after \\d+ seconds"));
				}

        private IIntegrationResult GenerateResultMock(string workingDir, string artefactDir)
        {
            var buildInfo = mocks.Create<BuildProgressInformation>(string.Empty, string.Empty).Object;
            var result = mocks.Create<IIntegrationResult>(MockBehavior.Strict).Object;
            Mock.Get(result).SetupGet(_result => _result.BuildProgressInformation).Returns(buildInfo);
            Mock.Get(result).SetupGet(_result => _result.WorkingDirectory).Returns(workingDir);
            Mock.Get(result).SetupGet(_result => _result.ArtifactDirectory).Returns(artefactDir);
            Mock.Get(result).SetupGet(_result => _result.IntegrationProperties).Returns(new Dictionary<string, string>());
            Mock.Get(result).SetupGet(_result => _result.Label).Returns("1");
            Mock.Get(result).Setup(_result => _result.AddTaskResult(It.IsAny<ITaskResult>())).Verifiable();
            Mock.Get(result).Setup(_result => _result.BaseFromWorkingDirectory("")).Returns(workingDir);
            return result;
        }

        private void SetupExecutorMock(ProcessExecutor executor, string fileName, string args, string workingDir, int timeout)
        {
            Mock.Get(executor).Setup(_executor => _executor.Execute(It.IsAny<ProcessInfo>()))
                .Callback<ProcessInfo>(info =>
                {
                    Assert.Equal(fileName, info.FileName);
                    Assert.Equal(args, info.Arguments);
                    Assert.Equal(workingDir, info.WorkingDirectory);
                    Assert.Equal(timeout, info.TimeOut);
                }).Returns(new ProcessResult(string.Empty, string.Empty, 0, false)).Verifiable();
        }

				private void ExecutorShouldTimeOut(ProcessExecutor executor)
				{
					Mock.Get(executor).Setup(_executor => _executor.Execute(It.IsAny<ProcessInfo>()))
							.Returns(ProcessResultFixture.CreateTimedOutResult()).Verifiable();
				}
		}
}
