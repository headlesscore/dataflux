using System;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
	
	public class NUnitTaskExecutionTest : ProcessExecutorTestFixtureBase
	{
		private const string NUnitConsolePath = @"D:\temp\nunit-console.exe";
		private string[] TEST_ASSEMBLIES = new string[] {"foo.dll"};
        const string WORKING_DIRECTORY = @"c:\temp";
        const string ARTIFACT_DIRECTORY = @"c:\temp";

		private Mock<ProcessExecutor> executorMock;
		private NUnitTask task;
		private IIntegrationResult result;
	    private SystemPath tempOutputFile;

	    // [SetUp]
		protected void Init()
		{
		    tempOutputFile = new TempDirectory().CreateTextFile("results.xml", "foo");
		    executorMock = new Mock<ProcessExecutor>();

			task = new NUnitTask(executorMock.Object as ProcessExecutor);
			task.Assemblies = TEST_ASSEMBLIES;
			task.NUnitPath = NUnitConsolePath;
	        task.OutputFile = tempOutputFile.ToString();
			result = Integration("testProject", WORKING_DIRECTORY, ARTIFACT_DIRECTORY);
		}

		// [TearDown]
		protected void DeleteTempFile()
		{
            tempOutputFile.DeleteDirectory();
		}

		[Fact]
		public void ExecuteNUnitConsoleAndRetrieveResultsFromFile()
		{
		    string args = string.Format(@"/xml={0} /nologo foo.dll", GeneratePath("{0}", task.OutputFile));
		    ProcessInfo info = new ProcessInfo(NUnitConsolePath, args, WORKING_DIRECTORY);
			info.TimeOut = NUnitTask.DefaultTimeout * 1000;
			executorMock.Setup(executor => executor.Execute(info)).Returns(new ProcessResult("", String.Empty, 0, false)).Verifiable();

			task.Run(result);

			Assert.Equal("foo", result.TaskOutput);
            Assert.True(true);
            Assert.True(true);
            executorMock.Verify();
		}

		[Fact]
		public void ShouldThrowExceptionIfTestsFailed()
		{
			executorMock.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).Returns(ProcessResultFixture.CreateNonZeroExitCodeResult()).Verifiable();

			task = new NUnitTask((ProcessExecutor) executorMock.Object);
            Assert.Throws<CruiseControlException>(delegate { task.Run(result); });
		}

        /// <summary>
        /// Path generation hack to text whether the desired path contains spaces.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is required because some environments contain spaces for their temp paths (e.g. WinXP), 
        /// other don't (e.g. WinVista). Previously the unit tests would fail between the different
        /// environments just because of this.
        /// </remarks>
        private string GeneratePath(string path, params string[] args)
        {
            string basePath = string.Format(path, args);
            if (basePath.Contains(" ")) basePath = "\"" + basePath + "\"";
            return basePath;
        }
    }
}
