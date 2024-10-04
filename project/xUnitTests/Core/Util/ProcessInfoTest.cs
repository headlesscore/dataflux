using System.Diagnostics;
using System.IO;
using Xunit;

using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
	
	public class ProcessInfoTest : CustomAssertion
	{
		[Fact]
		public void IfStandardInputContentIsSetThenStandardInputIsRedirected()
		{
			ProcessInfo info = new ProcessInfo("temp");
			info.StandardInputContent = "Some content";

			Process process = info.CreateProcess();
			Assert.True(process.StartInfo.RedirectStandardInput);
			Assert.True(!process.StartInfo.UseShellExecute);
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public void IfStandardInputContentIsNotSetThenStandardInputIsNotRedirected()
		{
			ProcessInfo info = new ProcessInfo("temp");
			Process process = info.CreateProcess();
			Assert.True(!process.StartInfo.RedirectStandardInput);
		}

		[Fact]
		public void IfExecutableIsFoundInWorkingDirectoryThenUseCombinedPathAsExecutablePath()
		{
			string workingDir = TempFileUtil.CreateTempDir("working");
			string executablePath = TempFileUtil.CreateTempFile("working", "myExecutable");

			ProcessInfo infoWithoutPathQualifiedExecutable = new ProcessInfo("myExecutable", "", workingDir);
			ProcessInfo infoWithPreQualifiedExecutable = new ProcessInfo(executablePath, "", workingDir);

			Assert.Equal(infoWithPreQualifiedExecutable, infoWithoutPathQualifiedExecutable);
		}

		[Fact]
		public void StripQuotesFromQuotedExecutablePath()
		{
			ProcessInfo info = new ProcessInfo(@"""c:\nant\nant.exe""", null, string.Format(@"""{0}""", Path.GetTempPath()));
			Assert.Equal(@"c:\nant\nant.exe", info.FileName);
			Assert.Equal(Path.GetTempPath(), info.WorkingDirectory);
		}

		[Fact]
		public void ProcessSuccessIsDeterminedBySuccessExitCodes()
		{
			int[] successExitCodes = { 1, 3, 5 };

			ProcessInfo info = new ProcessInfo(@"""c:\nant\nant.exe""", null, string.Format(@"""{0}""", Path.GetTempPath()), ProcessPriorityClass.Normal, successExitCodes);

			Assert.False(info.ProcessSuccessful(0));
			Assert.True(info.ProcessSuccessful(1));
			Assert.False(info.ProcessSuccessful(2));
			Assert.True(info.ProcessSuccessful(3));
			Assert.False(info.ProcessSuccessful(4));
			Assert.True(info.ProcessSuccessful(5));
			Assert.False(info.ProcessSuccessful(6));
		}

		[Fact]
		public void ProcessSuccessRequiresZeroExitCode()
		{
			int[] successExitCodes = { 1, 3, 5 };

			ProcessInfo info = new ProcessInfo(@"""c:\nant\nant.exe""", null, string.Format(@"""{0}""", Path.GetTempPath()));

			Assert.True(info.ProcessSuccessful(0));
			Assert.False(info.ProcessSuccessful(1));
			Assert.False(info.ProcessSuccessful(2));
			Assert.False(info.ProcessSuccessful(-1));
		}

	}
}
