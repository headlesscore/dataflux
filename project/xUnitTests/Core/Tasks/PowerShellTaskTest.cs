using System.IO;
using Exortech.NetReflector;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.Remote;
using System;


namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{

    public class PowerShellTaskTest : ProcessExecutorTestFixtureBase, IClassFixture<PowerShellTaskTest.TestFixture>
    {
        private const string POWERSHELL_PATH = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe";

        private const string POWERSHELL2_PATH = @"C:\Windows\System32\WindowsPowerShell\v1.0";
        private string SCRIPTS_PATH = System.Environment.GetEnvironmentVariable("USERPROFILE") + @"\Documents\WindowsPowerShell\";



        public class TestFixture : ProcessExecutorTestFixtureBase, IDisposable
        {
            internal const string POWERSHELL1_PATH = @"C:\Windows\System32\WindowsPowerShell\v1.0";
            private Mock<IRegistry> mockRegistry;
            public PowerShellTask mytask;
            public TestFixture()
            {
                mockRegistry = new Mock<IRegistry>();
                CreateProcessExecutorMock(POWERSHELL1_PATH);
                mytask = new PowerShellTask((IRegistry)mockRegistry.Object, (ProcessExecutor)mockProcessExecutor.Object);
            }

            public void Dispose()
            {
                mockRegistry.Verify();
                mockProcessExecutor.Verify();
            }
        }
        private TestFixture fixture;

        public PowerShellTaskTest(TestFixture fixture)
        {
            this.fixture = fixture;
            this.mockProcessExecutor = this.fixture.mockProcessExecutor;
        }

        [Fact]
        public void ShouldLoadAllValuesFromConfiguration()
        {
            string xml = @"
<powershell>
    <script>testscript.ps1</script>    
	<executable>c:\powershell\powershell.exe</executable>
	<scriptsDirectory>D:\CruiseControl</scriptsDirectory>
	<buildArgs>-noprofile</buildArgs>
	<buildTimeoutSeconds>4</buildTimeoutSeconds>
	<successExitCodes>1</successExitCodes>
	<environment>
        <variable name=""Env1"" value=""value1""/>
        <variable name=""Env2"" value=""value2""/>
   </environment>
</powershell>";

            PowerShellTask task = (PowerShellTask)NetReflector.Read(xml);
            Assert.Equal(@"c:\powershell\powershell.exe", task.Executable);
            Assert.Equal(@"testscript.ps1", task.Script);
            Assert.Equal(@"D:\CruiseControl", task.ConfiguredScriptsDirectory);
            Assert.Equal(@"-noprofile", task.BuildArgs);
            Assert.Equal(4, task.BuildTimeoutSeconds);
            Assert.Equal("1", task.SuccessExitCodes);
            Assert.Equal("value1", task.EnvironmentVariables[0].value);
            Assert.Equal("Env1", task.EnvironmentVariables[0].name);
            Assert.Equal("value2", task.EnvironmentVariables[1].value);
            Assert.Equal("Env2", task.EnvironmentVariables[1].name);
            Assert.True(true);
            Assert.True(true);
        }

        [Fact]
        public void ShouldLoadMinimalValuesFromConfiguration()
        {
            string xml = @"<powershell script=""myScript.ps1"" />";
            PowerShellTask task = (PowerShellTask)NetReflector.Read(xml);

            // Need to override the default registry search, otherwise this fails on a machine that does not have PowerShell installed
            var registryMock2 = new Mock<IRegistry>();
            task.Registry = (IRegistry)registryMock2.Object;
            registryMock2.Setup(registry => registry.GetLocalMachineSubKeyValue(
                @"SOFTWARE\Microsoft\PowerShell\2\PowerShellEngine",
                @"ApplicationBase")).Returns(@"C:\Windows\System32\WindowsPowerShell\v1.0").Verifiable();

            Assert.Equal(System.IO.Path.Combine(@"C:\Windows\System32\WindowsPowerShell\v1.0", "powershell.exe"), task.Executable);
            Assert.Equal(@"myScript.ps1", task.Script);
            Assert.Equal(PowerShellTask.DefaultScriptsDirectory, task.ConfiguredScriptsDirectory);
            Assert.Equal(PowerShellTask.DefaultBuildTimeOut, task.BuildTimeoutSeconds);
            Assert.Equal(string.Empty, task.SuccessExitCodes);
            Assert.Equal(0, task.EnvironmentVariables.Length);
        }

        [Fact]
        public void DefaultPowerShellShouldBe1IfNothingNewerInstalled()
        {
            var mockRegistry2 = new Mock<IRegistry>();

            PowerShellTask task = new PowerShellTask((IRegistry)mockRegistry2.Object, (ProcessExecutor)mockProcessExecutor.Object);
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(PowerShellTask.regkeypowershell2, PowerShellTask.regkeyholder)).Returns(() => null).Verifiable();
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(PowerShellTask.regkeypowershell1, PowerShellTask.regkeyholder)).Returns(TestFixture.POWERSHELL1_PATH).Verifiable();
            Assert.Equal(System.IO.Path.Combine(TestFixture.POWERSHELL1_PATH, "powershell.exe"), task.Executable);
            mockRegistry2.Verify();
            mockProcessExecutor.Verify();
        }

        [Fact(Skip = "Skipped in original codebase")]
        public void ShouldThrowAnExceptionIfPowerShellNotInstalled()
        {
            var mockRegistry2 = new Mock<IRegistry>();

            PowerShellTask task = new PowerShellTask((IRegistry)mockRegistry2.Object, (ProcessExecutor)mockProcessExecutor.Object);
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(PowerShellTask.regkeypowershell2, PowerShellTask.regkeyholder)).Returns(() => null).Verifiable();
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(PowerShellTask.regkeypowershell1, PowerShellTask.regkeyholder)).Returns(() => null).Verifiable();
            Assert.Equal(System.IO.Path.Combine(TestFixture.POWERSHELL1_PATH, "powershell.exe"), task.Executable);
            mockRegistry2.Verify();
            mockProcessExecutor.Verify();
        }

        [Fact]
        public void DefaultPowerShellShouldBe2IfInstalled()
        {
            var mockRegistry2 = new Mock<IRegistry>();

            PowerShellTask task = new PowerShellTask((IRegistry)mockRegistry2.Object, (ProcessExecutor)mockProcessExecutor.Object);
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(PowerShellTask.regkeypowershell2, PowerShellTask.regkeyholder)).Returns(POWERSHELL2_PATH).Verifiable();
            Assert.Equal(System.IO.Path.Combine(POWERSHELL2_PATH, "powershell.exe"), task.Executable);
            mockRegistry2.Verify();
            mockProcessExecutor.Verify();
        }

        [Fact]
        public void VerifyPowerShellProcessInfoBasic()
        {
            ProcessInfo info = null;
            ProcessResult processResult = new ProcessResult("output", "error", 0, false);
            mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).
                Callback<ProcessInfo>(processInfo => info = processInfo).Returns(processResult).Verifiable();
            fixture.mytask.Executable = POWERSHELL_PATH;
            fixture.mytask.Script = "MyScipt.ps1";

            fixture.mytask.Run(IntegrationResult());

            Assert.Equal(POWERSHELL_PATH, info.FileName);
            Assert.Equal(PowerShellTask.DefaultBuildTimeOut * 1000, info.TimeOut);
            CustomAssertion.AssertContains(fixture.mytask.Script, info.Arguments);
        }

        [Fact]
        public void VerifyPowerShellProcessInfoWithScriptsDirectoryConfigured()
        {
            ProcessInfo info = null;
            ProcessResult processResult = new ProcessResult("output", "error", 0, false);
            mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).
                Callback<ProcessInfo>(processInfo => info = processInfo).Returns(processResult).Verifiable();
            fixture.mytask.Executable = POWERSHELL_PATH;
            fixture.mytask.Script = "MyScript.ps1";
            fixture.mytask.ConfiguredScriptsDirectory = @"D:\CruiseControl";

            fixture.mytask.Run(IntegrationResult());

            Assert.Equal(POWERSHELL_PATH, info.FileName);
            Assert.Equal(PowerShellTask.DefaultBuildTimeOut * 1000, info.TimeOut);
            CustomAssertion.AssertStartsWith(@"-nologo -NoProfile -NonInteractive -file ""D:\CruiseControl\MyScript.ps1""", info.Arguments);
        }

        [Fact]
        public void ShouldSetOutputAndIntegrationStatusToSuccessOnSuccessfulBuild()
        {
            ProcessResult processResult = new ProcessResult(" ", string.Empty, ProcessResult.SUCCESSFUL_EXIT_CODE, false);
            mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).Returns(processResult).Verifiable();
            fixture.mytask.Executable = POWERSHELL_PATH;
            fixture.mytask.Script = "MyScript.ps1";
            fixture.mytask.BuildArgs = "an arg";
            fixture.mytask.EnvironmentVariables = new EnvironmentVariable[]
            {
                new EnvironmentVariable
                {
                    name = "test",
                    value = "value"
                }
            };

            IntegrationResult result = (IntegrationResult)IntegrationResult();
            fixture.mytask.Run(result);

            Assert.Equal(IntegrationStatus.Success, result.Status);
            CustomAssertion.AssertMatches(" ", result.TaskOutput);
        }

        [Fact]
        public void ShouldSetOutputAndIntegrationStatusToFailedOnFailedBuild()
        {
            ProcessResult processResult = new ProcessResult(@"Documents\WindowsPowerShell\MyScript.ps1' is not recognized as a cmdlet", string.Empty, 1, false);
            ProcessInfo info = null;
            mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).
                Callback<ProcessInfo>(processInfo => info = processInfo).Returns(processResult).Verifiable();

            string path = Platform.IsWindows ? @"D:\CruiseControl" : @"/CruiseControl";

            fixture.mytask.Executable = POWERSHELL_PATH;
            fixture.mytask.Script = "MyScript.ps1";
            fixture.mytask.ConfiguredScriptsDirectory = path;

            IIntegrationResult result = Integration("myProject", path, "myArtifactDirectory");
            fixture.mytask.Run(result);

            Assert.Equal(path, info.WorkingDirectory);

            Assert.Equal(IntegrationStatus.Failure, result.Status);
            CustomAssertion.AssertMatches(@"(\.|\n)*is not recognized as a cmdlet", result.TaskOutput);
        }

        [Fact]
        public void ShouldThrowBuilderExceptionIfProcessExecutorThrowsAnException()
        {
            mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).Throws(new IOException()).Verifiable();
            fixture.mytask.Executable = POWERSHELL_PATH;
            fixture.mytask.Script = "MyScript.ps1";

            Assert.Throws<BuilderException>(delegate { fixture.mytask.Run(IntegrationResult()); });
        }

        [Fact]
        public void ShouldThrowBuilderExceptionIfProcessExecutorThrowsAnExceptionUsingUnkownProject()
        {
            mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).Throws(new IOException()).Verifiable();
            fixture.mytask.Executable = POWERSHELL_PATH;
            fixture.mytask.Script = "MyScript.ps1";

            Assert.Throws<BuilderException>(delegate { fixture.mytask.Run(IntegrationResult()); });
        }

        [Fact]
        public void ShouldFailBuildIfProcessTimesOut()
        {
            ProcessResult processResult = ProcessResultFixture.CreateTimedOutResult();
            mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).Returns(processResult).Verifiable();
            fixture.mytask.BuildTimeoutSeconds = 2;
            fixture.mytask.Executable = POWERSHELL_PATH;
            fixture.mytask.Script = "MyScript.ps1";

            var result = IntegrationResult();
            fixture.mytask.Run(result);

            Assert.Equal(IntegrationStatus.Failure, result.Status);
            Assert.Matches("Command line '.*' timed out after \\d+ seconds", result.TaskOutput);
        }

        [Fact]
        public void SuccessExitCodesIsSuccessfullySplitAndRebuilt()
        {
            var task = new PowerShellTask();
            task.SuccessExitCodes = "1,2,3";
            Assert.Equal("1,2,3", task.SuccessExitCodes);
        }

        [Fact]
        public void SuccessExitCodesHandlesNull()
        {
            var task = new PowerShellTask();
            task.SuccessExitCodes = null;
            Assert.Equal(string.Empty, task.SuccessExitCodes);
        }

        [Fact]
        public void SuccessExitCodesHandlesEmpty()
        {
            var task = new PowerShellTask();
            task.SuccessExitCodes = string.Empty;
            Assert.Equal(string.Empty, task.SuccessExitCodes);
        }

        [Fact]
        public void ToStringReturnsTheBaseDirectoryAndExe()
        {
            var task = new PowerShellTask
            {
                ConfiguredScriptsDirectory = "testDir"
            };
            var actual = task.ToString();
            var expected = " BaseDirectory: testDir, PowerShell: powershell.exe";
            Assert.Equal(expected, actual);
        }
    }
}
