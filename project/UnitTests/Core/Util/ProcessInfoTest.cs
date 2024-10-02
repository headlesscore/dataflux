using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
	[TestFixture]
	public class ProcessInfoTest : CustomAssertion
	{
		[Test]
		public void IfStandardInputContentIsSetThenStandardInputIsRedirected()
		{
			ProcessInfo info = new ProcessInfo("temp");
			info.StandardInputContent = "Some content";

			Process process = info.CreateProcess();
			ClassicAssert.IsTrue(process.StartInfo.RedirectStandardInput);
			ClassicAssert.IsTrue(!process.StartInfo.UseShellExecute);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

		[Test]
		public void IfStandardInputContentIsNotSetThenStandardInputIsNotRedirected()
		{
			ProcessInfo info = new ProcessInfo("temp");
			Process process = info.CreateProcess();
			ClassicAssert.IsTrue(!process.StartInfo.RedirectStandardInput);
		}

		[Test]
		public void IfExecutableIsFoundInWorkingDirectoryThenUseCombinedPathAsExecutablePath()
		{
			string workingDir = TempFileUtil.CreateTempDir("working");
			string executablePath = TempFileUtil.CreateTempFile("working", "myExecutable");

			ProcessInfo infoWithoutPathQualifiedExecutable = new ProcessInfo("myExecutable", "", workingDir);
			ProcessInfo infoWithPreQualifiedExecutable = new ProcessInfo(executablePath, "", workingDir);

			ClassicAssert.AreEqual(infoWithPreQualifiedExecutable, infoWithoutPathQualifiedExecutable);
		}

		[Test]
		public void StripQuotesFromQuotedExecutablePath()
		{
			ProcessInfo info = new ProcessInfo(@"""c:\nant\nant.exe""", null, string.Format(@"""{0}""", Path.GetTempPath()));
			ClassicAssert.AreEqual(@"c:\nant\nant.exe", info.FileName);
			ClassicAssert.AreEqual(Path.GetTempPath(), info.WorkingDirectory);
		}

		[Test]
		public void ProcessSuccessIsDeterminedBySuccessExitCodes()
		{
			int[] successExitCodes = { 1, 3, 5 };

			ProcessInfo info = new ProcessInfo(@"""c:\nant\nant.exe""", null, string.Format(@"""{0}""", Path.GetTempPath()), ProcessPriorityClass.Normal, successExitCodes);

			ClassicAssert.IsFalse(info.ProcessSuccessful(0));
			ClassicAssert.IsTrue(info.ProcessSuccessful(1));
			ClassicAssert.IsFalse(info.ProcessSuccessful(2));
			ClassicAssert.IsTrue(info.ProcessSuccessful(3));
			ClassicAssert.IsFalse(info.ProcessSuccessful(4));
			ClassicAssert.IsTrue(info.ProcessSuccessful(5));
			ClassicAssert.IsFalse(info.ProcessSuccessful(6));
		}

		[Test]
		public void ProcessSuccessRequiresZeroExitCode()
		{
			int[] successExitCodes = { 1, 3, 5 };

			ProcessInfo info = new ProcessInfo(@"""c:\nant\nant.exe""", null, string.Format(@"""{0}""", Path.GetTempPath()));

			ClassicAssert.IsTrue(info.ProcessSuccessful(0));
			ClassicAssert.IsFalse(info.ProcessSuccessful(1));
			ClassicAssert.IsFalse(info.ProcessSuccessful(2));
			ClassicAssert.IsFalse(info.ProcessSuccessful(-1));
		}

	}
}
