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
	public class DevenvTaskTest : ProcessExecutorTestFixtureBase
	{
		private const string DEVENV_PATH = @"C:\Program Files\Microsoft Visual Studio .NET 2003\Common7\IDE\devenv.com";

        private const string DEVENV_2013_PATH = @"C:\Program Files\Microsoft Visual Studio 12\Common7\IDE\";
        private const string DEVENV_2012_PATH = @"C:\Program Files\Microsoft Visual Studio 11\Common7\IDE\";
        private const string DEVENV_2010_PATH = @"C:\Program Files\Microsoft Visual Studio 10\Common7\IDE\";
        private const string DEVENV_2008_PATH = @"C:\Program Files\Microsoft Visual Studio 9\Common7\IDE\";
		private const string DEVENV_2005_PATH = @"C:\Program Files\Microsoft Visual Studio 8\Common7\IDE\";
		private const string DEVENV_2003_PATH = @"C:\Program Files\Microsoft Visual Studio .NET 2003\Common7\IDE\";
		private const string DEVENV_2002_PATH = @"C:\Program Files\Microsoft Visual Studio .NET\Common7\IDE\";

		private const string SOLUTION_FILE = @"D:\dev\ccnet\ccnet\project\ccnet.sln";
		private const string CONFIGURATION = "Debug";

		private DevenvTask task;
		private Mock<IRegistry> mockRegistry;

		[SetUp]
		public void Setup()
		{
			mockRegistry = new Mock<IRegistry>();
			CreateProcessExecutorMock(DEVENV_PATH);
			task = new DevenvTask((IRegistry) mockRegistry.Object, (ProcessExecutor) mockProcessExecutor.Object);
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
			const string xml = @"
<devenv>
	<executable>c:\vs.net\devenv.com</executable>
	<version>9.0</version>
	<solutionfile>mySolution.sln</solutionfile>
	<configuration>Debug</configuration>
	<buildTimeoutSeconds>4</buildTimeoutSeconds>
	<project>MyProject</project>
	<buildtype>Clean</buildtype>
</devenv>";

			DevenvTask task2 = (DevenvTask) NetReflector.Read(xml);
			ClassicAssert.AreEqual(@"c:\vs.net\devenv.com", task2.Executable);
			ClassicAssert.AreEqual(@"mySolution.sln", task2.SolutionFile);
			ClassicAssert.AreEqual(@"Debug", task2.Configuration);
			ClassicAssert.AreEqual(4, task2.BuildTimeoutSeconds);
			ClassicAssert.AreEqual(@"Clean", task2.BuildType);
			ClassicAssert.AreEqual(@"MyProject", task2.Project);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

		[Test]
		public void ShouldLoadMinimalValuesFromConfiguration()
		{
			const string xml = @"<devenv solutionfile=""mySolution.sln"" configuration=""Release"" />";
			DevenvTask task2 = (DevenvTask) NetReflector.Read(xml);
			ClassicAssert.AreEqual(@"mySolution.sln", task2.SolutionFile);
			ClassicAssert.AreEqual(@"Release", task2.Configuration);
			ClassicAssert.AreEqual(DevenvTask.DEFAULT_BUILD_TIMEOUT, task2.BuildTimeoutSeconds);
			ClassicAssert.AreEqual(@"rebuild", task2.BuildType);
			ClassicAssert.AreEqual(@"", task2.Project);
		}

		[Test]
		public void ShouldFailToLoadInvalidVersionFromConfiguration()
		{
			const string xml = @"<devenv solutionfile=""mySolution.sln"" configuration=""Release"" version=""VSBAD""/>";
            ClassicAssert.That(delegate { NetReflector.Read(xml); },
                        Throws.TypeOf<NetReflectorException>());
		}

        [Test]
        public void DefaultVisualStudioShouldBe2010IfNothingNewerInstalled()
        {
            var mockRegistry2 = new Mock<IRegistry>();

            DevenvTask task2 = new DevenvTask((IRegistry)mockRegistry2.Object, (ProcessExecutor)mockProcessExecutor.Object);
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(DevenvTask.VS2013_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(() => null).Verifiable();
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(DevenvTask.VS2012_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(() => null).Verifiable();
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(DevenvTask.VS2010_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(DEVENV_2010_PATH).Verifiable();

            ClassicAssert.AreEqual(System.IO.Path.Combine(DEVENV_2010_PATH, "devenv.com"), task2.Executable);
            mockRegistry2.Verify();
            mockProcessExecutor.Verify();
        }

		[Test]
        public void DefaultVisualStudioShouldBe2008IfNothingNewerInstalled()
        {
            var mockRegistry2 = new Mock<IRegistry>();
           
            DevenvTask task2 = new DevenvTask((IRegistry)mockRegistry2.Object, (ProcessExecutor)mockProcessExecutor.Object);
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(DevenvTask.VS2013_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(() => null).Verifiable();
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(DevenvTask.VS2012_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(() => null).Verifiable();
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(DevenvTask.VS2010_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(() => null).Verifiable();
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(DevenvTask.VS2008_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(DEVENV_2008_PATH).Verifiable();
            ClassicAssert.AreEqual(System.IO.Path.Combine(DEVENV_2008_PATH, "devenv.com"), task2.Executable);
            mockRegistry2.Verify();
            mockProcessExecutor.Verify();
        }

        [Test]
        public void SelectVisualStudio2010ExplicitlyUsingVersionNumberWhenEverythingInstalled()
        {
            var mockRegistry2 = new Mock<IRegistry>();

            DevenvTask task2 = new DevenvTask((IRegistry)mockRegistry2.Object, (ProcessExecutor)mockProcessExecutor.Object);
            mockRegistry2.Setup(registry => registry.GetExpectedLocalMachineSubKeyValue(DevenvTask.VS2010_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(DEVENV_2010_PATH).Verifiable();

            task2.Version = "10.0";

            ClassicAssert.AreEqual(System.IO.Path.Combine(DEVENV_2010_PATH, "devenv.com"), task2.Executable);
            mockRegistry2.Verify();
            mockProcessExecutor.Verify();
        }

		[Test]
		public void SelectVisualStudio2008ExplicitlyUsingVersionNumberWhenEverythingInstalled()
		{
			var mockRegistry2 = new Mock<IRegistry>();

			DevenvTask task2 = new DevenvTask((IRegistry)mockRegistry2.Object, (ProcessExecutor)mockProcessExecutor.Object);
			mockRegistry2.Setup(registry => registry.GetExpectedLocalMachineSubKeyValue(DevenvTask.VS2008_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(DEVENV_2008_PATH).Verifiable();

			task2.Version = "9.0";

			ClassicAssert.AreEqual(System.IO.Path.Combine(DEVENV_2008_PATH, "devenv.com"), task2.Executable);
			mockRegistry2.Verify();
			mockProcessExecutor.Verify();
		}

        [Test]
        public void SelectVisualStudio2010ExplicitlyUsingVersionNameWhenEverythingInstalled()
        {
            var mockRegistry2 = new Mock<IRegistry>();

            DevenvTask task2 = new DevenvTask((IRegistry)mockRegistry2.Object, (ProcessExecutor)mockProcessExecutor.Object);
            mockRegistry2.Setup(registry => registry.GetExpectedLocalMachineSubKeyValue(DevenvTask.VS2010_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(DEVENV_2010_PATH).Verifiable();

            task2.Version = "VS2010";

            ClassicAssert.AreEqual(System.IO.Path.Combine(DEVENV_2010_PATH, "devenv.com"), task2.Executable);
            mockRegistry2.Verify();
            mockProcessExecutor.Verify();
        }

        [Test]
        public void SelectVisualStudio2008ExplicitlyUsingVersionNameWhenEverythingInstalled()
        {
            var mockRegistry2 = new Mock<IRegistry>();

            DevenvTask task2 = new DevenvTask((IRegistry)mockRegistry2.Object, (ProcessExecutor)mockProcessExecutor.Object);
            mockRegistry2.Setup(registry => registry.GetExpectedLocalMachineSubKeyValue(DevenvTask.VS2008_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(DEVENV_2008_PATH).Verifiable();

            task2.Version = "VS2008";

            ClassicAssert.AreEqual(System.IO.Path.Combine(DEVENV_2008_PATH, "devenv.com"), task2.Executable);
            mockRegistry2.Verify();
            mockProcessExecutor.Verify();
        }

        [Test]
        public void DefaultVisualStudioShouldBe2005IfNothingNewerInstalled()
        {
            var mockRegistry2 = new Mock<IRegistry>();

            DevenvTask task2 = new DevenvTask((IRegistry)mockRegistry2.Object, (ProcessExecutor)mockProcessExecutor.Object);
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(DevenvTask.VS2013_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(() => null).Verifiable();
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(DevenvTask.VS2012_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(() => null).Verifiable();
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(DevenvTask.VS2010_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(() => null).Verifiable();
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(DevenvTask.VS2008_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(() => null).Verifiable();
			mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(DevenvTask.VS2005_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(DEVENV_2005_PATH).Verifiable();
			ClassicAssert.AreEqual(System.IO.Path.Combine(DEVENV_2005_PATH, "devenv.com"), task2.Executable);
			mockRegistry2.Verify();
            mockProcessExecutor.Verify();
        }

		[Test]
		public void SelectVisualStudio2005ExplicitlyUsingVersionNumberWhenEverythingInstalled()
		{
			var mockRegistry2 = new Mock<IRegistry>();

			DevenvTask task2 = new DevenvTask((IRegistry)mockRegistry2.Object, (ProcessExecutor)mockProcessExecutor.Object);
			mockRegistry2.Setup(registry => registry.GetExpectedLocalMachineSubKeyValue(DevenvTask.VS2005_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(DEVENV_2005_PATH).Verifiable();

			task2.Version = "8.0";

			ClassicAssert.AreEqual(System.IO.Path.Combine(DEVENV_2005_PATH, "devenv.com"), task2.Executable);
			mockRegistry2.Verify();
			mockProcessExecutor.Verify();
		}

		[Test]
		public void SelectVisualStudio2005ExplicitlyUsingVersionNameWhenEverythingInstalled()
		{
			var mockRegistry2 = new Mock<IRegistry>();

			DevenvTask task2 = new DevenvTask((IRegistry)mockRegistry2.Object, (ProcessExecutor)mockProcessExecutor.Object);
			mockRegistry2.Setup(registry => registry.GetExpectedLocalMachineSubKeyValue(DevenvTask.VS2005_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(DEVENV_2005_PATH).Verifiable();

			task2.Version = "VS2005";

			ClassicAssert.AreEqual(System.IO.Path.Combine(DEVENV_2005_PATH, "devenv.com"), task2.Executable);
			mockRegistry2.Verify();
			mockProcessExecutor.Verify();
		}

        [Test]
        public void DefaultVisualStudioShouldBe2003IfNothingNewerInstalled()
		{
            var mockRegistry2 = new Mock<IRegistry>();
            
            DevenvTask task2 = new DevenvTask((IRegistry)mockRegistry2.Object, (ProcessExecutor)mockProcessExecutor.Object);
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(DevenvTask.VS2013_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(() => null).Verifiable();
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(DevenvTask.VS2012_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(() => null).Verifiable();
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(DevenvTask.VS2010_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(() => null).Verifiable();
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(DevenvTask.VS2008_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(() => null).Verifiable();
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(DevenvTask.VS2005_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(() => null).Verifiable();
			mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(DevenvTask.VS2003_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(DEVENV_2003_PATH).Verifiable();
			ClassicAssert.AreEqual(System.IO.Path.Combine(DEVENV_2003_PATH, "devenv.com"), task2.Executable);
			mockRegistry2.Verify();
            mockProcessExecutor.Verify();
        }

		[Test]
		public void SelectVisualStudio2003ExplicitlyUsingVersionNumberWhenEverythingInstalled()
		{
			var mockRegistry2 = new Mock<IRegistry>();

			DevenvTask task2 = new DevenvTask((IRegistry)mockRegistry2.Object, (ProcessExecutor)mockProcessExecutor.Object);
			mockRegistry2.Setup(registry => registry.GetExpectedLocalMachineSubKeyValue(DevenvTask.VS2003_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(DEVENV_2003_PATH).Verifiable();

			task2.Version = "7.1";

			ClassicAssert.AreEqual(System.IO.Path.Combine(DEVENV_2003_PATH, "devenv.com"), task2.Executable);
			mockRegistry2.Verify();
			mockProcessExecutor.Verify();
		}

		[Test]
		public void SelectVisualStudio2003ExplicitlyUsingVersionNameWhenEverythingInstalled()
		{
			var mockRegistry2 = new Mock<IRegistry>();

			DevenvTask task2 = new DevenvTask((IRegistry)mockRegistry2.Object, (ProcessExecutor)mockProcessExecutor.Object);
			mockRegistry2.Setup(registry => registry.GetExpectedLocalMachineSubKeyValue(DevenvTask.VS2003_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(DEVENV_2003_PATH).Verifiable();

			task2.Version = "VS2003";

			ClassicAssert.AreEqual(System.IO.Path.Combine(DEVENV_2003_PATH, "devenv.com"), task2.Executable);
			mockRegistry2.Verify();
			mockProcessExecutor.Verify();
		}

		[Test]
        public void DefaultVisualStudioShouldBe2002IfNothingNewerInstalled()
		{
            var mockRegistry2 = new Mock<IRegistry>();
           
            DevenvTask task2 = new DevenvTask((IRegistry)mockRegistry2.Object, (ProcessExecutor)mockProcessExecutor.Object);
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(DevenvTask.VS2013_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(() => null).Verifiable();
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(DevenvTask.VS2012_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(() => null).Verifiable();
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(DevenvTask.VS2010_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(() => null).Verifiable();
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(DevenvTask.VS2008_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(() => null).Verifiable();
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(DevenvTask.VS2005_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(() => null).Verifiable();
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(DevenvTask.VS2003_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(() => null).Verifiable();
            mockRegistry2.Setup(registry => registry.GetLocalMachineSubKeyValue(DevenvTask.VS2002_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(DEVENV_2002_PATH).Verifiable();

			ClassicAssert.AreEqual(System.IO.Path.Combine(DEVENV_2002_PATH, "devenv.com"), task2.Executable);
			mockRegistry2.Verify();
            mockProcessExecutor.Verify();
        }

		[Test]
		public void SelectVisualStudio2002ExplicitlyUsingVersionNumberWhenEverythingInstalled()
		{
			var mockRegistry2 = new Mock<IRegistry>();

			DevenvTask task2 = new DevenvTask((IRegistry)mockRegistry2.Object, (ProcessExecutor)mockProcessExecutor.Object);
			mockRegistry2.Setup(registry => registry.GetExpectedLocalMachineSubKeyValue(DevenvTask.VS2002_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(DEVENV_2002_PATH).Verifiable();

			task2.Version = "7.0";

			ClassicAssert.AreEqual(System.IO.Path.Combine(DEVENV_2002_PATH, "devenv.com"), task2.Executable);
			mockRegistry2.Verify();
			mockProcessExecutor.Verify();
		}

		[Test]
		public void SelectVisualStudio2002ExplicitlyUsingVersionNameWhenEverythingInstalled()
		{
			var mockRegistry2 = new Mock<IRegistry>();

			DevenvTask task2 = new DevenvTask((IRegistry)mockRegistry2.Object, (ProcessExecutor)mockProcessExecutor.Object);
			mockRegistry2.Setup(registry => registry.GetExpectedLocalMachineSubKeyValue(DevenvTask.VS2002_REGISTRY_PATH, DevenvTask.VS_REGISTRY_KEY)).Returns(DEVENV_2002_PATH).Verifiable();

			task2.Version = "VS2002";

			ClassicAssert.AreEqual(System.IO.Path.Combine(DEVENV_2002_PATH, "devenv.com"), task2.Executable);
			mockRegistry2.Verify();
			mockProcessExecutor.Verify();
		}

		[Test]
		public void VerifyDevenvProcessInfo()
		{
			ProcessInfo info = null;
			ProcessResult processResult = new ProcessResult("output", "error", 0, false);
			mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).
				Callback<ProcessInfo>(processInfo => info = processInfo).Returns(processResult).Verifiable();
			task.Executable = DEVENV_PATH;
			task.SolutionFile = "\"mySolution.sln\"";
			task.Configuration = "Debug";

			task.Run(IntegrationResult());

			ClassicAssert.AreEqual(DEVENV_PATH, info.FileName);
			ClassicAssert.AreEqual(DevenvTask.DEFAULT_BUILD_TIMEOUT*1000, info.TimeOut);
            AssertStartsWith("\"mySolution.sln\" /rebuild \"Debug\"", info.Arguments);
		}

		[Test]
		public void VerifyDevenvProcessInfoWithProjectDefined()
		{
			ProcessInfo info = null;
			ProcessResult processResult = new ProcessResult("output", "error", 0, false);
			mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).
				Callback<ProcessInfo>(processInfo => info = processInfo).Returns(processResult).Verifiable();
			task.Executable = DEVENV_PATH;
			task.SolutionFile = "mySolution.sln";
			task.Configuration = "\"Debug\"";
			task.Project = "myProject";

			task.Run(IntegrationResult());

			ClassicAssert.AreEqual(DEVENV_PATH, info.FileName);
			ClassicAssert.AreEqual(DevenvTask.DEFAULT_BUILD_TIMEOUT*1000, info.TimeOut);
            AssertStartsWith("\"mySolution.sln\" /rebuild \"Debug\" /project \"myProject\"", info.Arguments);
		}

		[Test]
		public void ShouldSetOutputAndIntegrationStatusToSuccessOnSuccessfulBuild()
		{
			ProcessResult processResult = new ProcessResult(@"Rebuild All: 10 succeeded, 0 failed, 0 skipped", string.Empty, ProcessResult.SUCCESSFUL_EXIT_CODE, false);
			mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).Returns(processResult).Verifiable();
			task.Executable = DEVENV_PATH;
			task.SolutionFile = SOLUTION_FILE;
			task.Configuration = CONFIGURATION;

			IntegrationResult result = (IntegrationResult)IntegrationResult();
			task.Run(result);

			ClassicAssert.AreEqual(IntegrationStatus.Success, result.Status);
			AssertMatches(@"Rebuild All: \d+ succeeded, \d+ failed, \d+ skipped", result.TaskOutput);
		}

		[Test]
		public void ShouldSetOutputAndIntegrationStatusToFailedOnFailedBuild()
		{
			ProcessResult processResult = new ProcessResult(@"D:\dev\ccnet\ccnet\project\nosolution.sln could not be found and will not be loaded", string.Empty, 1, false);
			ProcessInfo info = null;
			mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).
				Callback<ProcessInfo>(processInfo => info = processInfo).Returns(processResult).Verifiable();

			task.Executable = DEVENV_PATH;
			task.SolutionFile = @"D:\dev\ccnet\ccnet\project\nosolution.sln";
			task.Configuration = CONFIGURATION;

			IIntegrationResult result = Integration("myProject", "myWorkingDirectory", "myArtifactDirectory");
			task.Run(result);

			ClassicAssert.AreEqual("myWorkingDirectory", info.WorkingDirectory);

			ClassicAssert.AreEqual(IntegrationStatus.Failure, result.Status);
			AssertMatches(@"(\.|\n)*could not be found and will not be loaded", result.TaskOutput);
		}

		[Test]
		public void ShouldThrowBuilderExceptionIfProcessExecutorThrowsAnException()
		{
			mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).Throws(new IOException()).Verifiable();
			task.Executable = DEVENV_PATH + ".some.extra.ext.exe"; // file should not exist
			task.SolutionFile = @"D:\dev\ccnet\ccnet\project\nosolution.sln";
			task.Configuration = "Debug";

            ClassicAssert.That(delegate { task.Run(IntegrationResult()); },
                        Throws.TypeOf<BuilderException>());
		}

		[Test]
		public void ShouldThrowBuilderExceptionIfProcessExecutorThrowsAnExceptionUsingUnkownProject()
		{
			mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).Throws(new IOException()).Verifiable();
			task.Executable = DEVENV_PATH;
			task.SolutionFile = SOLUTION_FILE;
			task.Configuration = CONFIGURATION;
			task.Project = "unknownproject";

            ClassicAssert.That(delegate { task.Run(IntegrationResult()); },
                        Throws.TypeOf<BuilderException>());
		}

		[Test]
		public void ShouldThrowBuilderExceptionIfProcessTimesOut()
		{
			ProcessResult processResult = new ProcessResult(string.Empty, string.Empty, ProcessResult.TIMED_OUT_EXIT_CODE, true);
			mockProcessExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).Returns(processResult).Verifiable();
			task.BuildTimeoutSeconds = 2;
			task.Executable = DEVENV_PATH;
			task.SolutionFile = SOLUTION_FILE;
			task.Configuration = CONFIGURATION;

            ClassicAssert.That(delegate { task.Run(IntegrationResult()); },
                        Throws.TypeOf<BuilderException>());
		}
	}
}
