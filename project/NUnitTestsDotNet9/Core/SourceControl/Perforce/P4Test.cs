using System;
using Exortech.NetReflector;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol.Perforce;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol.Perforce
{
	// ToDo - tidy up these tests by using mocks for process Executor, and make appropriate methods on P4 private
	// This already performed for 'Label', 'Get Source', 'Initialize'
	[TestFixture]
	public class P4Test : ProcessExecutorTestFixtureBase
	{
		private Mock<IP4Initializer> p4InitializerMock;
		private Mock<IP4ProcessInfoCreator> processInfoCreatorMock;
		private Mock<IProject> projectMock;
		private IProject project;
		private Mock<IP4Purger> p4PurgerMock;

		[SetUp]
		public void Setup()
		{
			CreateProcessExecutorMock("p4");
			p4InitializerMock = new Mock<IP4Initializer>();
			p4PurgerMock = new Mock<IP4Purger>();
			processInfoCreatorMock = new Mock<IP4ProcessInfoCreator>();
			projectMock = new Mock<IProject>();
			project = (IProject) projectMock.Object;
		}

		private void VerifyAll()
		{
			Verify();
			p4InitializerMock.Verify();
			p4PurgerMock.Verify();
			processInfoCreatorMock.Verify();
			projectMock.Verify();
		}

		[TearDown]
		public void TearDown()
		{
			TempFileUtil.DeleteTempDir("ccnet");
		}

		[Test]
		public void ReadConfig()
		{
			string xml = string.Format(@"
<sourceControl type=""p4"">
  <executable>c:\bin\p4.exe</executable>
  <view>//depot/myproject/...</view>
  <client>myclient</client>
  <user>me</user>
  <password>mypassword</password>
  <port>anotherserver:2666</port>
  <workingDirectory>myWorkingDirectory</workingDirectory>
  <p4WebURLFormat>http://perforceWebServer:8080/@md=d&amp;cd=//&amp;c=3IB@/{{0}}?ac=10</p4WebURLFormat>
  <timeZoneOffset>{0}</timeZoneOffset>
  <useExitCode>true</useExitCode>
  <errorPattern>Error: (.*)</errorPattern>
  <acceptableErrors>
    <acceptableError>(.*)\.accept1</acceptableError>
    <acceptableError>(.*)\.accept2</acceptableError>
  </acceptableErrors>
</sourceControl>
", -5.5);
			P4 p4 = CreateP4WithNoArgContructor(xml);
			ClassicAssert.AreEqual(@"c:\bin\p4.exe", p4.Executable);
			ClassicAssert.AreEqual("//depot/myproject/...", p4.View);
			ClassicAssert.AreEqual("myclient", p4.Client);
			ClassicAssert.AreEqual("me", p4.User);
			ClassicAssert.AreEqual("mypassword", p4.Password);
			ClassicAssert.AreEqual("anotherserver:2666", p4.Port);
			ClassicAssert.AreEqual("myWorkingDirectory", p4.WorkingDirectory);
			ClassicAssert.AreEqual("http://perforceWebServer:8080/@md=d&cd=//&c=3IB@/{0}?ac=10", p4.P4WebURLFormat);
			ClassicAssert.AreEqual(-5.5, p4.TimeZoneOffset);
            ClassicAssert.AreEqual(true, p4.UseExitCode);
            ClassicAssert.AreEqual("Error: (.*)", p4.ErrorPattern);
            ClassicAssert.AreEqual(@"(.*)\.accept1", p4.AcceptableErrors[0]);
            ClassicAssert.AreEqual(@"(.*)\.accept2", p4.AcceptableErrors[1]);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void ReadConfigWithEmptyErrorsArguments()
        {
            string xml = @"
<sourceControl type=""p4"">
  <executable>c:\bin\p4.exe</executable>
  <view>//depot/myproject/...</view>
  <errorPattern/>
  <acceptableErrors/>
</sourceControl>
";
            P4 p4 = CreateP4WithNoArgContructor(xml);
            ClassicAssert.AreEqual("", p4.ErrorPattern);
            ClassicAssert.AreEqual(0, p4.AcceptableErrors.Length);
        }

		private P4 CreateP4WithNoArgContructor(string p4root)
		{
			P4 perforce = new P4();
			NetReflector.Read(p4root, perforce);
			return perforce;
		}

		private P4 CreateP4()
		{
			return new P4((ProcessExecutor) mockProcessExecutor.Object,
			              (IP4Initializer) p4InitializerMock.Object,
			              (IP4Purger) p4PurgerMock.Object,
			              (IP4ProcessInfoCreator) processInfoCreatorMock.Object);
		}

		[Test]
		public void ReadConfigDefaults()
		{
			string xml = @"
<sourceControl name=""p4"">
  <view>//depot/anotherproject/...</view>
</sourceControl>
";
			P4 p4 = CreateP4WithNoArgContructor(xml);
			ClassicAssert.AreEqual("p4", p4.Executable);
			ClassicAssert.AreEqual("//depot/anotherproject/...", p4.View);
			ClassicAssert.AreEqual("", p4.Client);
			ClassicAssert.AreEqual("", p4.User);
			ClassicAssert.AreEqual("", p4.Password);
			ClassicAssert.AreEqual("", p4.Port);
            ClassicAssert.AreEqual(false, p4.UseExitCode);
            ClassicAssert.AreEqual(@"^error: .*", p4.ErrorPattern);
            ClassicAssert.AreEqual(@"file\(s\) up-to-date\.", p4.AcceptableErrors[0]);
		}

		[Test]
		public void ReadConfigBarfsWhenViewIsExcluded()
		{
			string xml = @"
<sourceControl name=""p4"">
</sourceControl>
";
            ClassicAssert.That(delegate { CreateP4WithNoArgContructor(xml); },
                        Throws.TypeOf<NetReflectorException>());
		}

		[Test]
		public void CreateGetChangeListsProcess()
		{
			P4 p4 = new P4();
			p4.View = "//depot/myproj/...";
			DateTime from = new DateTime(2002, 10, 20, 2, 0, 0);
			DateTime to = new DateTime(2002, 10, 31, 5, 5, 0);

			ProcessInfo process = p4.CreateChangeListProcess(from, to);

			string expectedArgs = "-s changes -s submitted //depot/myproj/...@2002/10/20:02:00:00,@2002/10/31:05:05:00";

			ClassicAssert.AreEqual("p4", process.FileName);
			ClassicAssert.AreEqual(expectedArgs, process.Arguments);
		}

		[Test]
		public void CreateGetChangeListsProcessInDifferentTimeZone()
		{
			P4 p4 = new P4();
			p4.View = "//depot/myproj/...";
			DateTime from = new DateTime(2002, 10, 20, 2, 0, 0);
			DateTime to = new DateTime(2002, 10, 31, 5, 5, 0);
			p4.TimeZoneOffset = -4.5;

			ProcessInfo process = p4.CreateChangeListProcess(from, to);

			string expectedArgs = "-s changes -s submitted //depot/myproj/...@2002/10/19:21:30:00,@2002/10/31:00:35:00";

			ClassicAssert.AreEqual("p4", process.FileName);
			ClassicAssert.AreEqual(expectedArgs, process.Arguments);
		}

		[Test]
		public void CreateGetChangeListsProcessWithMultiLineView()
		{
			P4 p4 = new P4();
			p4.View = "//depot/myproj/...,//myotherdepot/proj/...";
			DateTime from = new DateTime(2002, 10, 20, 2, 0, 0);
			DateTime to = new DateTime(2002, 10, 31, 5, 5, 0);

			ProcessInfo process = p4.CreateChangeListProcess(from, to);

			string expectedArgs = "-s changes -s submitted //depot/myproj/...@2002/10/20:02:00:00,@2002/10/31:05:05:00 //myotherdepot/proj/...@2002/10/20:02:00:00,@2002/10/31:05:05:00";

			ClassicAssert.AreEqual("p4", process.FileName);
			ClassicAssert.AreEqual(expectedArgs, process.Arguments);
		}

		[Test]
		public void CreateGetChangeListsProcessWithDifferentArgs()
		{
			string xml = @"
<sourceControl name=""p4"">
  <executable>c:\bin\p4.exe</executable>
  <view>//depot/myproject/...</view>
  <client>myclient</client>
  <user>me</user>
  <password>mypassword</password>
  <port>anotherserver:2666</port>
</sourceControl>
";

			DateTime from = new DateTime(2003, 11, 20, 2, 10, 32);
			DateTime to = new DateTime(2004, 10, 31, 5, 5, 1);

			string expectedArgs = "-s -c myclient -p anotherserver:2666 -u me -P mypassword"
				+ " changes -s submitted //depot/myproject/...@2003/11/20:02:10:32,@2004/10/31:05:05:01";

			P4 p4 = CreateP4WithNoArgContructor(xml);
			ProcessInfo process = p4.CreateChangeListProcess(from, to);

			ClassicAssert.AreEqual("c:\\bin\\p4.exe", process.FileName);
			ClassicAssert.AreEqual(expectedArgs, process.Arguments);
		}

		[Test]
		public void CreateGetDescribeProcess()
		{
			string changes = "3327 3328 332";
			ProcessInfo process = new P4().CreateDescribeProcess(changes);

			string expectedArgs = "-s describe -s " + changes;
			ClassicAssert.AreEqual("p4", process.FileName);
			ClassicAssert.AreEqual(expectedArgs, process.Arguments);
		}

		[Test]
		public void CreateGetDescribeProcessWithSpecifiedArgs()
		{
			string xml = @"
<sourceControl name=""p4"">
  <executable>c:\bin\p4.exe</executable>
  <view>//depot/myproject/...</view>
  <client>myclient</client>
  <user>me</user>
  <password>mypassword</password>
  <port>anotherserver:2666</port>
</sourceControl>
";
			string changes = "3327 3328 332";

			string expectedArgs = "-s -c myclient -p anotherserver:2666 -u me -P mypassword"
				+ " describe -s " + changes;

			P4 p4 = CreateP4WithNoArgContructor(xml);
			ProcessInfo process = p4.CreateDescribeProcess(changes);

			ClassicAssert.AreEqual("c:\\bin\\p4.exe", process.FileName);
			ClassicAssert.AreEqual(expectedArgs, process.Arguments);
		}

		[Test]
		public void CreateGetDescribeProcessWithEvilCode()
		{
			string changes = "3327 3328 332; echo 'rm -rf /'";

            ClassicAssert.That(delegate { new P4().CreateDescribeProcess(changes); },
                        Throws.TypeOf<CruiseControlException>());
		}

		[Test]
		public void CreateGetDescribeProcessWithNoChanges()
		{
            ClassicAssert.That(delegate { new P4().CreateDescribeProcess(""); },
                        Throws.TypeOf<CruiseControlException>());
		}

		[Test]
		public void GetModifications()
		{
			string changes = @"
info: Change 3328 on 2002/10/31 by someone@somewhere 'Something important '
info: Change 3327 on 2002/10/31 by someone@somewhere 'Joe's test '
info: Change 332 on 2002/10/31 by someone@somewhere 'thingy'
exit: 0
";

			P4 p4 = CreateP4();

			ProcessInfo info = new ProcessInfo("p4.exe");
			MockSequence sequence = new MockSequence();
			processInfoCreatorMock.Setup(creator => creator.CreateProcessInfo(p4, "changes -s submitted ViewData@0001/01/01:00:00:00")).Returns(info).Verifiable();
			mockProcessExecutor.InSequence(sequence).Setup(executor => executor.Execute(info)).Returns(new ProcessResult(changes, "", 0, false)).Verifiable();
			processInfoCreatorMock.Setup(creator => creator.CreateProcessInfo(p4, "describe -s 3328 3327 332")).Returns(info).Verifiable();
			mockProcessExecutor.InSequence(sequence).Setup(executor => executor.Execute(info)).Returns(new ProcessResult(P4Mother.P4_LOGFILE_CONTENT, "", 0, false)).Verifiable();

			p4.View = "ViewData";
			p4.P4WebURLFormat = "http://perforceWebServer:8080/@md=d&amp;cd=//&amp;c=3IB@/{0}?ac=10";
			Modification[] result = p4.GetModifications(new IntegrationResult(), new IntegrationResult());

			VerifyAll();
			ClassicAssert.AreEqual(7, result.Length);
			ClassicAssert.AreEqual("http://perforceWebServer:8080/@md=d&amp;cd=//&amp;c=3IB@/3328?ac=10", result[0].Url);
			ClassicAssert.AreEqual("http://perforceWebServer:8080/@md=d&amp;cd=//&amp;c=3IB@/3327?ac=10", result[3].Url);
		}

		[Test]
		public void LabelSourceControlIfApplyLabelTrue()
		{
			P4 p4 = CreateP4();
			p4.View = "//depot/myproject/...";
			p4.ApplyLabel = true;

			ProcessInfo labelSpecProcess = new ProcessInfo("spec");
			ProcessInfo labelSpecProcessWithStdInContent = new ProcessInfo("spec");
			labelSpecProcessWithStdInContent.StandardInputContent = "Label:	foo-123\n\nDescription:\n	Created by CCNet\n\nOptions:	unlocked\n\nView:\n //depot/myproject/...\n";
			ProcessInfo labelSyncProcess = new ProcessInfo("sync");

			processInfoCreatorMock.Setup(creator => creator.CreateProcessInfo(p4, "label -i")).Returns(labelSpecProcess).Verifiable();
			mockProcessExecutor.Setup(executor => executor.Execute(labelSpecProcessWithStdInContent)).Returns(new ProcessResult("", "", 0, false)).Verifiable();
			processInfoCreatorMock.Setup(creator => creator.CreateProcessInfo(p4, "labelsync -l foo-123")).Returns(labelSyncProcess).Verifiable();
			mockProcessExecutor.Setup(executor => executor.Execute(labelSyncProcess)).Returns(new ProcessResult("", "", 0, false)).Verifiable();

			// Execute
			p4.LabelSourceControl(IntegrationResultMother.CreateSuccessful("foo-123"));

			// Verify
			VerifyAll();
		}

		[Test]
		public void LabelSourceControlIfApplyLabelTrueWithMultiLineViews()
		{
			P4 p4 = CreateP4();
			p4.View = "//depot/myproj/...,//myotherdepot/proj/...";
			p4.ApplyLabel = true;

			ProcessInfo labelSpecProcess = new ProcessInfo("spec");
			ProcessInfo labelSpecProcessWithStdInContent = new ProcessInfo("spec");
			labelSpecProcessWithStdInContent.StandardInputContent = "Label:	foo-123\n\nDescription:\n	Created by CCNet\n\nOptions:	unlocked\n\nView:\n //depot/myproj/...\n //myotherdepot/proj/...\n";
			ProcessInfo labelSyncProcess = new ProcessInfo("sync");

			processInfoCreatorMock.Setup(creator => creator.CreateProcessInfo(p4, "label -i")).Returns(labelSpecProcess).Verifiable();
			mockProcessExecutor.Setup(executor => executor.Execute(labelSpecProcessWithStdInContent)).Returns(new ProcessResult("", "", 0, false)).Verifiable();
			processInfoCreatorMock.Setup(creator => creator.CreateProcessInfo(p4, "labelsync -l foo-123")).Returns(labelSyncProcess).Verifiable();
			mockProcessExecutor.Setup(executor => executor.Execute(labelSyncProcess)).Returns(new ProcessResult("", "", 0, false)).Verifiable();

			// Execute
			p4.LabelSourceControl(IntegrationResultMother.CreateSuccessful("foo-123"));

			// Verify
			VerifyAll();
		}

		[Test]
		public void ViewForSpecificationsSupportsSingleLineView()
		{
			P4 p4 = CreateP4();
			p4.View = "//depot/myproj/...";

			ClassicAssert.AreEqual(1, p4.ViewForSpecifications.Length);
			ClassicAssert.AreEqual("//depot/myproj/...", p4.ViewForSpecifications[0]);
		}

		[Test]
		public void ViewForSpecificationsSupportsMultiLineView()
		{
			P4 p4 = CreateP4();
			p4.View = "//depot/myproj/...,//myotherdepot/proj/...";

			ClassicAssert.AreEqual(2, p4.ViewForSpecifications.Length);
			ClassicAssert.AreEqual("//depot/myproj/...", p4.ViewForSpecifications[0]);
			ClassicAssert.AreEqual("//myotherdepot/proj/...", p4.ViewForSpecifications[1]);
		}

		[Test]
		public void LabelSourceControlFailsIfLabelIsOnlyNumeric()
		{
			P4 p4 = CreateP4();
			p4.View = "//depot/myproject/...";
			p4.ApplyLabel = true;

			try
			{
				p4.LabelSourceControl(IntegrationResultMother.CreateSuccessful("123"));
				ClassicAssert.Fail("Perforce labelling should fail if a purely numeric label is attempted to be applied");
			}
			catch (CruiseControlException)
			{}

			VerifyAll();
		}

		[Test]
		public void DontLabelSourceControlIfApplyLabelFalse()
		{
			P4 p4 = CreateP4();
			p4.View = "//depot/myproject/...";
			p4.ApplyLabel = false;

			p4.LabelSourceControl(IntegrationResultMother.CreateSuccessful("foo-123"));

			processInfoCreatorMock.VerifyNoOtherCalls();
			mockProcessExecutor.VerifyNoOtherCalls();
			VerifyAll();
		}

		[Test]
		public void DontLabelSourceControlIfIntegrationFailed()
		{
			P4 p4 = CreateP4();
			p4.View = "//depot/myproject/...";
			p4.ApplyLabel = true;

			p4.LabelSourceControl(IntegrationResultMother.CreateFailed("foo-123"));

			processInfoCreatorMock.VerifyNoOtherCalls();
			mockProcessExecutor.VerifyNoOtherCalls();
			VerifyAll();
		}

		[Test]
		public void DontLabelSourceControlIfApplyLabelNotSetEvenIfInvalidLabel()
		{
			P4 p4 = CreateP4();
			p4.View = "//depot/myproject/...";

			p4.LabelSourceControl(IntegrationResultMother.CreateSuccessful("123"));

			processInfoCreatorMock.VerifyNoOtherCalls();
			mockProcessExecutor.VerifyNoOtherCalls();
			VerifyAll();
		}

        [Test]
        public void GetSourceIfGetSourceTrue()
        {
            P4 p4 = CreateP4();
            p4.View = "//depot/myproject/...";
            p4.AutoGetSource = true;

            DateTime modificationsToDate = new DateTime(2002, 10, 31, 5, 5, 0);
            ProcessInfo processInfo = new ProcessInfo("getSource");
            processInfoCreatorMock.Setup(creator => creator.CreateProcessInfo(p4, "sync //depot/myproject/...@2002/10/31:05:05:00")).Returns(processInfo).Verifiable();
            mockProcessExecutor.Setup(executor => executor.Execute(processInfo)).Returns(new ProcessResult("", "", 0, false)).Verifiable();
            p4.GetSource(IntegrationResultMother.CreateSuccessful(modificationsToDate));

            VerifyAll();
        }

        [Test]
        public void GetForceSourceIfGetSourceTrue()
        {
            P4 p4 = CreateP4();
            p4.View = "//depot/myproject/...";
            p4.AutoGetSource = true;
            p4.ForceSync = true;

            DateTime modificationsToDate = new DateTime(2002, 10, 31, 5, 5, 0);
            ProcessInfo processInfo = new ProcessInfo("getSource");
            processInfoCreatorMock.Setup(creator => creator.CreateProcessInfo(p4, "sync -f //depot/myproject/...@2002/10/31:05:05:00")).Returns(processInfo).Verifiable();
            mockProcessExecutor.Setup(executor => executor.Execute(processInfo)).Returns(new ProcessResult("", "", 0, false)).Verifiable();
            p4.GetSource(IntegrationResultMother.CreateSuccessful(modificationsToDate));

            VerifyAll();
        }

		[Test]
		public void DontGetSourceIfGetSourceFalse()
		{
			P4 p4 = CreateP4();
			p4.View = "//depot/myproject/...";
			p4.AutoGetSource = false;

			p4.GetSource(new IntegrationResult());
			processInfoCreatorMock.VerifyNoOtherCalls();
			mockProcessExecutor.VerifyNoOtherCalls();
			VerifyAll();
		}

		[Test]
		public void ShouldCallInitializerWithGivenWorkingDirectoryIfAlternativeNotSet()
		{
			// Setup
			P4 p4 = CreateP4();
			p4.View = "//depot/myproject/...";
			p4InitializerMock.Setup(initializer => initializer.Initialize(p4, "myProject", "workingDirFromProject")).Verifiable();
			projectMock.SetupGet(_project => _project.Name).Returns("myProject").Verifiable();
			projectMock.SetupGet(_project => _project.WorkingDirectory).Returns("workingDirFromProject").Verifiable();

			// Execute
			p4.Initialize(project);

			// Verify
			VerifyAll();
		}

		[Test]
		public void ShouldCallInitializerWithGivenWorkingDirectoryIfAlternativeSetToEmpty()
		{
			// Setup
			P4 p4 = CreateP4();
			p4.View = "//depot/myproject/...";
			p4.WorkingDirectory = "";
			p4InitializerMock.Setup(initializer => initializer.Initialize(p4, "myProject", "workingDirFromProject")).Verifiable();
			projectMock.SetupGet(_project => _project.Name).Returns("myProject").Verifiable();
			projectMock.SetupGet(_project => _project.WorkingDirectory).Returns("workingDirFromProject").Verifiable();

			// Execute
			p4.Initialize(project);

			// Verify
			VerifyAll();
		}

		[Test]
		public void ShouldCallInitializerWithConfiguredWorkingDirectoryIfAlternativeIsConfigured()
		{
			// Setup
			P4 p4 = CreateP4();
			p4.View = "//depot/myproject/...";
			p4.WorkingDirectory = "p4sOwnWorkingDirectory";
			p4InitializerMock.Setup(initializer => initializer.Initialize(p4, "myProject", "p4sOwnWorkingDirectory")).Verifiable();
			projectMock.SetupGet(_project => _project.Name).Returns("myProject").Verifiable();

			// Execute
			p4.Initialize(project);

			// Verify
			VerifyAll();
			projectMock.VerifyNoOtherCalls();
		}

		[Test]
		public void ShouldCallPurgerWithGivenWorkingDirectoryIfAlternativeNotSet()
		{
			// Setup
			P4 p4 = CreateP4();
			p4.View = "//depot/myproject/...";
			p4PurgerMock.Setup(purger => purger.Purge(p4, "workingDirFromProject")).Verifiable();
			projectMock.SetupGet(_project => _project.WorkingDirectory).Returns("workingDirFromProject").Verifiable();

			// Execute
			p4.Purge(project);

			// Verify
			VerifyAll();
		}

		[Test]
		public void ShouldCallPurgerWithGivenWorkingDirectoryIfAlternativeSetToEmpty()
		{
			// Setup
			P4 p4 = CreateP4();
			p4.View = "//depot/myproject/...";
			p4.WorkingDirectory = "";
			p4PurgerMock.Setup(purger => purger.Purge(p4, "workingDirFromProject")).Verifiable();
			projectMock.SetupGet(_project => _project.WorkingDirectory).Returns("workingDirFromProject").Verifiable();

			// Execute
			p4.Purge(project);

			// Verify
			VerifyAll();
		}

		[Test]
		public void ShouldCallPurgerWithConfiguredWorkingDirectoryIfAlternativeIsConfigured()
		{
			// Setup
			P4 p4 = CreateP4();
			p4.View = "//depot/myproject/...";
			p4.WorkingDirectory = "p4sOwnWorkingDirectory";
			p4PurgerMock.Setup(purger => purger.Purge(p4, "p4sOwnWorkingDirectory")).Verifiable();

			// Execute
			p4.Purge(project);

			// Verify
			projectMock.VerifyNoOtherCalls();
			VerifyAll();
		}
	}
}
