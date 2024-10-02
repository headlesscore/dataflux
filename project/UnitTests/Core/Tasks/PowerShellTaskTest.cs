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
    public class PowerShellTaskTest : ProcessExecutorTestFixtureBase
    {
        private const string POWERSHELL_PATH = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe";
        private const string POWERSHELL1_PATH = @"C:\Windows\System32\WindowsPowerShell\v1.0";
        private const string POWERSHELL2_PATH = @"C:\Windows\System32\WindowsPowerShell\v1.0";
        private string SCRIPTS_PATH = System.Environment.GetEnvironmentVariable("USERPROFILE") + @"\Documents\WindowsPowerShell\";
       
        private PowerShellTask mytask;
        private Mock<IRegistry> mockRegistry;

        [SetUp]
        public void Setup()
        {
            mockRegistry = new Mock<IRegistry>();
            CreateProcessExecutorMock(POWERSHELL1_PATH);
            mytask = new PowerShellTask((IRegistry)mockRegistry.Object, (ProcessExecutor)mockProcessExecutor.Object);
        }

        [TearDown]
        public void VerifyMocks()
        {
            mockRegistry.Verify();
            mockProcessExecutor.Verify();
        }

        [Test]
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
            ClassicAssert.AreEqual(@"c:\powershell\powershell.exe", task.Executable);
            ClassicAssert.AreEqual(@"testscript.ps1", task.Script);
            ClassicAssert.AreEqual(@"D:\CruiseControl", task.ConfiguredScriptsDirectory);
            ClassicAssert.AreEqual(@"-noprofile", task.BuildArgs);
            ClassicAssert.AreEqual(4, task.BuildTimeoutSeconds);
            ClassicAssert.AreEqual("1", task.SuccessExitCodes);
            ClassicAssert.AreEqual("value1", task.EnvironmentVariables[0].value);
            ClassicAssert.AreEqual("Env1", task.EnvironmentVariables[0].name);
            ClassicAssert.AreEqual("value2", task.EnvironmentVariables[1].value);
            ClassicAssert.AreEqual("Env2", task.EnvironmentVariables[1].name);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
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

            ClassicAssert.AreEqual(System.IO.Path.Combine(@"C:\Windows\System32\WindowsPowerShell\v1.0", "powershell.exe"), task.Executable);
            ClassicAssert.AreEqual(@"myScript.ps1", task.Script);
            ClassicAssert.AreEqual(PowerShellTask.DefaultScriptsDirectory, task.ConfiguredScriptsDirectory);
            ClassicAssert.AreEqual(PowerShellTask.DefaultBuildTimeOut, task.BuildTimeoutSeconds);
            ClassicAssert.AreEqual(string.Empty, task.SuccessExitCodes);
            ClassicAssert.AreEqual(0, task.EnvironmentVariables.Length, "Checking environment variable array size.");
        }

        [Test]
        public void DefaultPowerShellShouldBe1IfNothingNewerInstalled()
        {
            var mockRegistry2 = new Mock<IRegistry>();

            PowerShellTask task = new PowerShellTask((IRegistry)mockRegistry2.Object, (ProcessExecutor)mockProcessExecutor.Object);
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(PowerShellTask.regkeypowershell2, PowerShellTask.regkeyholder)).Returns(() => null).Verifiable();
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(PowerShellTask.regkeypowershell1, PowerShellTask.regkeyholder)).Returns(POWERSHELL1_PATH).Verifiable();
            ClassicAssert.AreEqual(System.IO.Path.Combine(POWERSHELL1_PATH, "powershell.exe"), task.Executable);
            mockRegistry2.Verify();
            mockProcessExecutor.Verify();
        }

        [Test]
        [Ignore("In which place is the BuilderException expected? => Use ClassicAssert.That(..., Throws.TypeOf<BuilderException>())")]
        public void ShouldThrowAnExceptionIfPowerShellNotInstalled()
        {
            var mockRegistry2 = new Mock<IRegistry>();

            PowerShellTask task = new PowerShellTask((IRegistry)mockRegistry2.Object, (ProcessExecutor)mockProcessExecutor.Object);
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(PowerShellTask.regkeypowershell2, PowerShellTask.regkeyholder)).Returns(() => null).Verifiable();
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(PowerShellTask.regkeypowershell1, PowerShellTask.regkeyholder)).Returns(() => null).Verifiable();
            ClassicAssert.AreEqual(System.IO.Path.Combine(POWERSHELL1_PATH, "powershell.exe"), task.Executable);
            mockRegistry2.Verify();
            mockProcessExecutor.Verify();
        }  

        [Test]
        public void DefaultPowerShellShouldBe2IfInstalled()
        {
            var mockRegistry2 = new Mock<IRegistry>();

            PowerShellTask task = new PowerShellTask((IRegistry)mockRegistry2.Object, (ProcessExecutor)mockProcessExecutor.Object);
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(PowerShellTask.regkeypowershell2,PowerShellTask.regkeyholder)).Returns(POWERSHELL2_PATH).Verifiable();
            ClassicAssert.AreEqual(System.IO.Path.Combine(POWERSHELL2_PATH, "powershell.exe"), task.Executable);
            mockRegistry2.Verify();
            mockProcessExecutor.Verify();
        }           

        [Test]
        public void VerifyPowerShellProcessInfoBasic()
        {
            ProcessInfo info = null;
            ProcessResult processResult = new ProcessResult("output", "error", 0, false);
            mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).
                Callback<ProcessInfo>(processInfo => info = processInfo).Returns(processResult).Verifiable();
            mytask.Executable = POWERSHELL_PATH;
            mytask.Script = "MyScipt.ps1";
           
            mytask.Run(IntegrationResult());

            ClassicAssert.AreEqual(POWERSHELL_PATH, info.FileName);
            ClassicAssert.AreEqual(PowerShellTask.DefaultBuildTimeOut * 1000, info.TimeOut);
            CustomAssertion.AssertContains(mytask.Script, info.Arguments);
        }

        [Test]
        public void VerifyPowerShellProcessInfoWithScriptsDirectoryConfigured()
        {
            ProcessInfo info = null;
            ProcessResult processResult = new ProcessResult("output", "error", 0, false);
            mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).
                Callback<ProcessInfo>(processInfo => info = processInfo).Returns(processResult).Verifiable();
            mytask.Executable = POWERSHELL_PATH;
            mytask.Script = "MyScript.ps1";
            mytask.ConfiguredScriptsDirectory = @"D:\CruiseControl";

            mytask.Run(IntegrationResult());

            ClassicAssert.AreEqual(POWERSHELL_PATH, info.FileName);
            ClassicAssert.AreEqual(PowerShellTask.DefaultBuildTimeOut * 1000, info.TimeOut);
            CustomAssertion.AssertStartsWith(@"-nologo -NoProfile -NonInteractive -file ""D:\CruiseControl\MyScript.ps1""", info.Arguments);
        }

        [Test]
        public void ShouldSetOutputAndIntegrationStatusToSuccessOnSuccessfulBuild()
        {
            ProcessResult processResult = new ProcessResult(" ", string.Empty, ProcessResult.SUCCESSFUL_EXIT_CODE, false);
            mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).Returns(processResult).Verifiable();
            mytask.Executable = POWERSHELL_PATH;
            mytask.Script = "MyScript.ps1";
            mytask.BuildArgs = "an arg";
            mytask.EnvironmentVariables = new EnvironmentVariable[]
            {
                new EnvironmentVariable
                {
                    name = "test",
                    value = "value"
                }
            };

            IntegrationResult result = (IntegrationResult)IntegrationResult();
            mytask.Run(result);

            ClassicAssert.AreEqual(IntegrationStatus.Success, result.Status);
            CustomAssertion.AssertMatches(" ", result.TaskOutput);
        }

        [Test]
        public void ShouldSetOutputAndIntegrationStatusToFailedOnFailedBuild()
        {
            ProcessResult processResult = new ProcessResult(@"Documents\WindowsPowerShell\MyScript.ps1' is not recognized as a cmdlet", string.Empty, 1, false);
            ProcessInfo info = null;
            mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).
                Callback<ProcessInfo>(processInfo => info = processInfo).Returns(processResult).Verifiable();

            string path = Platform.IsWindows ? @"D:\CruiseControl" : @"/CruiseControl";
            
            mytask.Executable = POWERSHELL_PATH;
            mytask.Script = "MyScript.ps1";
            mytask.ConfiguredScriptsDirectory = path;

            IIntegrationResult result = Integration("myProject", path, "myArtifactDirectory");
            mytask.Run(result);

            ClassicAssert.AreEqual(path, info.WorkingDirectory);

            ClassicAssert.AreEqual(IntegrationStatus.Failure, result.Status);
            CustomAssertion.AssertMatches(@"(\.|\n)*is not recognized as a cmdlet", result.TaskOutput);
        }

        [Test]
        public void ShouldThrowBuilderExceptionIfProcessExecutorThrowsAnException()
        {
            mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).Throws(new IOException()).Verifiable();
            mytask.Executable = POWERSHELL_PATH;
            mytask.Script = "MyScript.ps1";

            ClassicAssert.That(delegate { mytask.Run(IntegrationResult()); },
                        Throws.TypeOf<BuilderException>());
        }

        [Test]
        public void ShouldThrowBuilderExceptionIfProcessExecutorThrowsAnExceptionUsingUnkownProject()
        {
            mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).Throws(new IOException()).Verifiable();
            mytask.Executable = POWERSHELL_PATH;
            mytask.Script = "MyScript.ps1";

            ClassicAssert.That(delegate { mytask.Run(IntegrationResult()); },
                        Throws.TypeOf<BuilderException>());
        }

        [Test]
        public void ShouldFailBuildIfProcessTimesOut()
        {
						ProcessResult processResult = ProcessResultFixture.CreateTimedOutResult();
            mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).Returns(processResult).Verifiable();
            mytask.BuildTimeoutSeconds = 2;
            mytask.Executable = POWERSHELL_PATH;
            mytask.Script = "MyScript.ps1";

						var result = IntegrationResult();
						mytask.Run(result);

						ClassicAssert.That(result.Status, Is.EqualTo(IntegrationStatus.Failure));
						ClassicAssert.That(result.TaskOutput, Does.Match("Command line '.*' timed out after \\d+ seconds"));
        }

        [Test]
        public void SuccessExitCodesIsSuccessfullySplitAndRebuilt()
        {
            var task = new PowerShellTask();
            task.SuccessExitCodes = "1,2,3";
            ClassicAssert.AreEqual("1,2,3", task.SuccessExitCodes);
        }

        [Test]
        public void SuccessExitCodesHandlesNull()
        {
            var task = new PowerShellTask();
            task.SuccessExitCodes = null;
            ClassicAssert.AreEqual(string.Empty, task.SuccessExitCodes);
        }

        [Test]
        public void SuccessExitCodesHandlesEmpty()
        {
            var task = new PowerShellTask();
            task.SuccessExitCodes = string.Empty;
            ClassicAssert.AreEqual(string.Empty, task.SuccessExitCodes);
        }

        [Test]
        public void ToStringReturnsTheBaseDirectoryAndExe()
        {
            var task = new PowerShellTask
            {
                ConfiguredScriptsDirectory = "testDir"
            };
            var actual = task.ToString();
            var expected = " BaseDirectory: testDir, PowerShell: powershell.exe";
            ClassicAssert.AreEqual(expected, actual);
        }
    }
}
