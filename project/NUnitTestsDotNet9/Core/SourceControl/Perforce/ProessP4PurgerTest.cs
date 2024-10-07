using System.IO;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol.Perforce;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol.Perforce
{
	[TestFixture]
	public class ProessP4PurgerTest
	{
		private Mock<ProcessExecutor> processExecutorMock;
		private Mock<IP4ProcessInfoCreator> processInfoCreatorMock;
		private ProcessP4Purger p4Purger;
		private string tempDirPath;

		[SetUp]
		public void Setup()
		{
			processExecutorMock = new Mock<ProcessExecutor>();
			processInfoCreatorMock = new Mock<IP4ProcessInfoCreator>();
			p4Purger = new ProcessP4Purger((ProcessExecutor) processExecutorMock.Object,  (IP4ProcessInfoCreator) processInfoCreatorMock.Object);

			tempDirPath = TempFileUtil.CreateTempDir("tempDir");
		}

		[TearDown]
		public void TearDown()
		{
			TempFileUtil.DeleteTempDir("tempDir");
		}
		
		private void VerifyAll()
		{
			processExecutorMock.Verify();
			processInfoCreatorMock.Verify();
		}

		[Test]
		public void ShouldDeleteClientSpecAndWorkingDirectoryOnPurge()
		{
			// Setup
			P4 p4 = new P4();
			p4.Client = "myClient";

			ProcessInfo processInfo = new ProcessInfo("deleteclient");
			processInfoCreatorMock.Setup(creator => creator.CreateProcessInfo(p4, "client -d myClient")).Returns(processInfo).Verifiable();
			processExecutorMock.Setup(executor => executor.Execute(processInfo)).Returns(new ProcessResult("", "", 0, false)).Verifiable();

			ClassicAssert.IsTrue(Directory.Exists(tempDirPath));
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);

            // Execute
            p4Purger.Purge(p4, tempDirPath);

			// Verify
			ClassicAssert.IsFalse(Directory.Exists(tempDirPath));
			VerifyAll();
		}

		[Test]
		public void ShouldNotTryAndDeleteClientSpecIfClientSpecNotSet()
		{
			// Setup
			P4 p4 = new P4();
			p4.Client = null;

			ClassicAssert.IsTrue(Directory.Exists(tempDirPath));

			// Execute
			p4Purger.Purge(p4, tempDirPath);

			// Verify
			ClassicAssert.IsFalse(Directory.Exists(tempDirPath));
			VerifyAll();
			processInfoCreatorMock.VerifyNoOtherCalls();
			processExecutorMock.VerifyNoOtherCalls();
		}

		[Test]
		public void ShouldThrowAnExceptionIfProcessFails()
		{
			// Setup
			P4 p4 = new P4();
			p4.Client = "myClient";

			ProcessInfo processInfo = new ProcessInfo("deleteclient");
			processInfoCreatorMock.Setup(creator => creator.CreateProcessInfo(p4, "client -d myClient")).Returns(processInfo).Verifiable();
			processExecutorMock.Setup(executor => executor.Execute(processInfo)).Returns(new ProcessResult("This is standard out", "This is standard error", 1, false)).Verifiable();

			// Execute
			try
			{
				p4Purger.Purge(p4, tempDirPath);
				ClassicAssert.Fail("Should throw an exception since process result has a non zero exit code");
			}
			catch (CruiseControlException e)
			{
				ClassicAssert.IsTrue(e.Message.IndexOf("This is standard out") > -1);
				ClassicAssert.IsTrue(e.Message.IndexOf("This is standard error") > -1);
			}

			VerifyAll();
			// Don't delete wd since the client may still exist
			ClassicAssert.IsTrue(Directory.Exists(tempDirPath));
		}
	}
}
