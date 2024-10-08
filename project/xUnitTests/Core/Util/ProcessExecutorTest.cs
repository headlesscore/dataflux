using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Xunit;

using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
	
	public class ProcessExecutorTest : CustomAssertion
	{
		private ProcessExecutor executor;
		private volatile bool runnerThreadCompletedNormally;
		private volatile bool runnerThreadWasAborted;
		private const string PROJECT_NAME = "testing";

		// [SetUp]
		protected void CreateExecutor()
		{
			executor = new ProcessExecutor();
			if (Thread.CurrentThread.Name == null)
			{
				Thread.CurrentThread.Name = PROJECT_NAME;
			}
			runnerThreadCompletedNormally = false;
			runnerThreadWasAborted = false;
		}

		[Fact]
		public void ExecuteProcessAndEchoResultsBackThroughStandardOut()
		{
			ProcessResult result = executor.Execute(Platform.IsWindows ? new ProcessInfo("cmd.exe", "/C @echo Hello World") : new ProcessInfo("echo", "Hello World"));
			Assert.Equal("Hello World", result.StandardOutput.Trim());
            Assert.True(true);
            Assert.True(true);
            AssertProcessExitsSuccessfully(result);
		}

		[Fact]
		public void ExecuteProcessAndEchoResultsBackThroughStandardOutWhereALargeAmountOfOutputIsProduced()
		{
			ProcessResult result = executor.Execute(Platform.IsWindows ? new ProcessInfo("cmd.exe", "/C @dir " + Environment.SystemDirectory) :  new ProcessInfo("ls", Environment.SystemDirectory));
			Assert.True(! result.TimedOut);
			AssertProcessExitsSuccessfully(result);
		}

		[Fact]
		public void ShouldNotUseATimeoutIfTimeoutSetToInfiniteOnProcessInfo()
		{
			ProcessInfo processInfo = Platform.IsWindows ? new ProcessInfo("cmd.exe", "/C @echo Hello World") : new ProcessInfo("bash", "-c \"echo Hello World\"");
			processInfo.TimeOut = ProcessInfo.InfiniteTimeout;
			ProcessResult result = executor.Execute(processInfo);
			AssertProcessExitsSuccessfully(result);
			Assert.Equal("Hello World", result.StandardOutput.Trim());			
		}

		[Fact]
		public void StartProcessRunningCmdExeCallingNonExistentFile()
		{
			ProcessResult result = executor.Execute(Platform.IsWindows ? new ProcessInfo("cmd.exe", "/C @zerk.exe foo") : new ProcessInfo("bash", "-c \"zerk.exe foo\""));

			AssertProcessExitsWithFailure(result);
			AssertContains("zerk.exe", result.StandardError);
			Assert.Equal(string.Empty, result.StandardOutput);
			Assert.True(! result.TimedOut);
		}

		[Fact]
		public void SetEnvironmentVariables()
		{
			ProcessInfo processInfo = Platform.IsWindows ? new ProcessInfo("cmd.exe", "/C set foo", null) : new ProcessInfo("bash", "-c \"echo foo=$foo\"", null);
			processInfo.EnvironmentVariables["foo"] = "bar";
			ProcessResult result = executor.Execute(processInfo);

			AssertProcessExitsSuccessfully(result);
			Assert.Equal("foo=bar" + Environment.NewLine, result.StandardOutput);
		}

		[Fact]
		public void ForceProcessTimeoutBecauseTargetIsNonTerminating()
		{
			ProcessInfo processInfo = new ProcessInfo("sleeper.exe");
			processInfo.TimeOut = 100;
			ProcessResult result = executor.Execute(processInfo);

			Assert.True(result.TimedOut, "process did not time out, but it should have.");
			Assert.NotNull(result.StandardOutput);
			AssertProcessExitsWithFailure(result);
		}

		[Fact]
		public void SupplyInvalidFilenameAndVerifyException()
		{
            Assert.Throws<IOException>(delegate { executor.Execute(new ProcessInfo("foodaddy.bat")); });
		}

		[Fact]
		public void ShouldThrowMeaningfulExceptionIfWorkingDirectoryDoesNotExist()
		{
            Assert.Throws<DirectoryNotFoundException>(delegate { executor.Execute(new ProcessInfo("myExecutable", "", @"c:\invalid_path\that_is_invalid")); });
		}

		[Fact]
		public void StartNonTerminatingProcessAndAbortThreadShouldKillProcessAndAbortThread()
		{
			// ARRANGE
			Thread thread = new Thread(StartSleeperProcess);
			thread.Name = "sleeper thread";
			thread.Start();
			WaitForProcessToStart();

			// ACT
			thread.Abort();

			// ASSERT
			thread.Join();
			Assert.True(runnerThreadWasAborted, "Runner thread should be aborted.");
			// Ensure the external process was killed
			try
			{
				Assert.Equal(0, Process.GetProcessesByName("sleeper").Length);
			}
			catch (Exception)
			{
				Process.GetProcessesByName("sleeper")[0].Kill();
				Assert.Fail("Process was not killed.");
			}
		}

		[Fact]
		public void StartNonTerminatingProcessAndInterruptCurrentProcessShouldKillProcessButLeaveThreadRunning()
		{
			// ARRANGE
			Thread thread = new Thread(StartSleeperProcess);
			thread.Name = "sleeper thread";
			thread.Start();
			WaitForProcessToStart();

			// ACT
			ProcessExecutor.KillProcessCurrentlyRunningForProject("sleeper thread");

			// ASSERT
			// Sleeper runs for 60 seconds. We need to give up early and fail the test if it takes longer than 50.
			// If it runs for the full 60 seconds it will look the same as being interrupted, and the test will pass
			// incorrectly.
			Assert.True(thread.Join(TimeSpan.FromSeconds(50)), "Thread did not exit in reasonable time."); 
			Assert.True(runnerThreadCompletedNormally, "Runner thread should have exited through normally.");
			// Ensure the external process was killed
			try
			{
				Assert.Equal(0, Process.GetProcessesByName("sleeper").Length);
			}
			catch (Exception)
			{
				Process.GetProcessesByName("sleeper")[0].Kill();
				Assert.Fail("Process was not killed.");
			}
		}

		[Fact]
		public void ReadUnicodeFile()
		{
			SystemPath tempDirectory = SystemPath.UniqueTempPath().CreateDirectory();
			try
			{
				const string content = "yooo ��";
				SystemPath tempFile = tempDirectory.CreateTextFile("test.txt", content);
				ProcessInfo processInfo = Platform.IsWindows ? new ProcessInfo("cmd.exe", "/C type \"" + tempFile + "\"") : new ProcessInfo("cat", "\"" + tempFile + "\"");
				processInfo.StreamEncoding = Encoding.UTF8;
				ProcessResult result = executor.Execute(processInfo);
				Assert.True(!result.Failed);
				Assert.Equal(content + Environment.NewLine, result.StandardOutput);
			}
			finally
			{
				tempDirectory.DeleteDirectory();
			}
		}

		[Fact]
		public void ProcessInfoDeterminesSuccessOfProcess()
		{
			int[] successExitCodes = { 1, 3, 5 };

			ProcessInfo processInfo1 = Platform.IsWindows ? new ProcessInfo("cmd.exe", "/C @echo Hello World & exit 1", null, ProcessPriorityClass.AboveNormal, successExitCodes) : new ProcessInfo("bash", "-c \"echo Hello World ; exit 1\"", null, ProcessPriorityClass.AboveNormal, successExitCodes);

			ProcessResult result1 = executor.Execute(processInfo1);
			Assert.Equal("Hello World", result1.StandardOutput.Trim());
			Assert.Equal(1, result1.ExitCode);
			AssertFalse("process should not return an error", result1.Failed);

            ProcessInfo processInfo2 = Platform.IsWindows ? new ProcessInfo("cmd.exe", "/C @echo Hello World & exit 3", null, ProcessPriorityClass.AboveNormal, successExitCodes) : new ProcessInfo("bash", "-c \"echo Hello World ; exit 3\"", null, ProcessPriorityClass.AboveNormal, successExitCodes);

			ProcessResult result2 = executor.Execute(processInfo2);
			Assert.Equal("Hello World", result2.StandardOutput.Trim());
			Assert.Equal(3, result2.ExitCode);
			AssertFalse("process should not return an error", result2.Failed);

            ProcessInfo processInfo3 = Platform.IsWindows ? new ProcessInfo("cmd.exe", "/C @echo Hello World & exit 5", null, ProcessPriorityClass.AboveNormal, successExitCodes) : new ProcessInfo("bash", "-c \"echo Hello World ; exit 5\"", null, ProcessPriorityClass.AboveNormal, successExitCodes);

			ProcessResult result3 = executor.Execute(processInfo3);
			Assert.Equal("Hello World", result3.StandardOutput.Trim());
			Assert.Equal(5, result3.ExitCode);
			AssertFalse("process should not return an error", result3.Failed);

            ProcessInfo processInfo4 = Platform.IsWindows ? new ProcessInfo("cmd.exe", "/C @echo Hello World", null, ProcessPriorityClass.AboveNormal, successExitCodes) : new ProcessInfo("bash", "-c \"echo Hello World\"", null, ProcessPriorityClass.AboveNormal, successExitCodes);

			ProcessResult result4 = executor.Execute(processInfo4);
			Assert.Equal("Hello World", result4.StandardOutput.Trim());
			Assert.Equal(ProcessResult.SUCCESSFUL_EXIT_CODE, result4.ExitCode);
			Assert.True(result4.Failed, "process should return an error");
		}

		private static void AssertProcessExitsSuccessfully(ProcessResult result)
		{
			Assert.Equal(ProcessResult.SUCCESSFUL_EXIT_CODE, result.ExitCode);
			AssertFalse("process should not return an error", result.Failed);
		}

		private static void AssertProcessExitsWithFailure(ProcessResult result)
		{
			Assert.NotEqual(ProcessResult.SUCCESSFUL_EXIT_CODE, result.ExitCode);
			Assert.True(result.Failed, "process should return an error");
		}
        
        private static bool SleeperProcessExists()
        {
            if (Platform.IsMono)
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "bash";
                    process.StartInfo.Arguments = "-c \"ps -Aef\"";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.Start();
        
                    StreamReader reader = process.StandardOutput;
                    string output = reader.ReadToEnd();
                    
                    process.WaitForExit();
                    
                    return output.Contains("sleeper");
                }
            }
            else
            {
                return Process.GetProcessesByName("sleeper").Length != 0;
            }
        }

		private static void WaitForProcessToStart()
		{
			int count = 0;
            
			while (!SleeperProcessExists() && count < 1000)
			{
				Thread.Sleep(50);
				count++;
			}
			Thread.Sleep(2000);
			if (count == 1000) Assert.Fail("sleeper process did not start.");
		}

		private void StartSleeperProcess()
		{
			try
			{
				ProcessInfo processInfo = new ProcessInfo("sleeper.exe");
				executor.Execute(processInfo);
				runnerThreadCompletedNormally = true;
			}
			catch (ThreadAbortException)
			{
				runnerThreadWasAborted = true;
				Thread.ResetAbort();
			}
		}
	}
}
