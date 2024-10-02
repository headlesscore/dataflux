using System;
using System.Globalization;
using Exortech.NetReflector;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	[TestFixture]
	public class ClearCaseTest
	{
		public const string EXECUTABLE = "cleartool.exe";
		public const string VIEWPATH = @"C:\DOES_NOT_EXIST";
		public const string VIEWNAME = @"VIEWNAME_DOES_NOT_EXIST";
		public const string PROJECT_VOB_NAME = "ProjectVobName";
		public const string USE_BASELINE = "false";
		public static readonly string USE_LABEL = "true";
		const string BRANCH = "my branch";

		public static readonly string XML_STUB =
			@"<sourceControl type=""clearCase"">
    <executable>{0}</executable>
	<viewPath>{1}</viewPath>
    <useBaseline>{2}</useBaseline>
    <useLabel>{3}</useLabel>
    <projectVobName>{4}</projectVobName>
	<viewName>{5}</viewName>
	<branch>{6}</branch>
</sourceControl>";

		public static readonly string CLEARCASE_XML = string.Format(XML_STUB,
		                                                            EXECUTABLE,
		                                                            VIEWPATH,
		                                                            USE_BASELINE,
		                                                            USE_LABEL,
		                                                            PROJECT_VOB_NAME,
		                                                            VIEWNAME, BRANCH);

		private ClearCase clearCase;

		[SetUp]
		protected void Setup()
		{
			clearCase = new ClearCase();
			NetReflector.Read(CLEARCASE_XML, clearCase);
		}

		[Test]
		public void CanCreateTemporaryBaselineProcessInfo()
		{
			const string name = "baselinename";
			ProcessInfo info = clearCase.CreateTempBaselineProcessInfo(name);
			ClassicAssert.AreEqual(string.Format(System.Globalization.CultureInfo.CurrentCulture,"{0} mkbl -view {1} -identical {2}",
			                              EXECUTABLE,
			                              VIEWNAME,
			                              name),
			                info.FileName + " " + info.Arguments);
            ClassicAssert.IsTrue(true);
        }

		[Test]
		public void CanCreateRemoveBaselineProcessInfo()
		{
			ProcessInfo info = clearCase.CreateRemoveBaselineProcessInfo();
			ClassicAssert.AreEqual(string.Format(System.Globalization.CultureInfo.CurrentCulture,"{0} rmbl -force {1}@\\{2}",
			                              EXECUTABLE,
			                              clearCase.TempBaseline,
			                              clearCase.ProjectVobName),
			                info.FileName + " " + info.Arguments);
		}

		[Test]
		public void CanCreateRenameBaselineProcesInfo()
		{
			const string newName = "HiImANewBaselineName";
			ProcessInfo info = clearCase.CreateRenameBaselineProcessInfo(newName);
			ClassicAssert.AreEqual(string.Format(System.Globalization.CultureInfo.CurrentCulture,"{0} rename baseline:{1}@\\{2} \"{3}\"",
			                              EXECUTABLE,
			                              clearCase.TempBaseline,
			                              clearCase.ProjectVobName,
			                              newName),
			                info.FileName + " " + info.Arguments);
		}


		[Test]
		public void ShouldPopulateCorrectlyFromXml()
		{
			ClassicAssert.AreEqual(EXECUTABLE, clearCase.Executable);
			ClassicAssert.AreEqual(VIEWPATH, clearCase.ViewPath);
			ClassicAssert.AreEqual(VIEWNAME, clearCase.ViewName);
			ClassicAssert.AreEqual(Convert.ToBoolean(USE_BASELINE), clearCase.UseBaseline);
			ClassicAssert.AreEqual(Convert.ToBoolean(USE_LABEL), clearCase.UseLabel);
			ClassicAssert.AreEqual(PROJECT_VOB_NAME, clearCase.ProjectVobName);
			ClassicAssert.AreEqual(BRANCH, clearCase.Branch);
		}

		[Test]
		public void CanCatchInvalidBaselineConfiguration()
		{
			ClearCase clearCase = new ClearCase();
			const string invalidXml = "<sourcecontrol type=\"ClearCase\"><useBaseline>NOT_A_BOOLEAN</useBaseline></sourcecontrol>";
			ClassicAssert.That(delegate { NetReflector.Read(invalidXml, clearCase); },
                        Throws.TypeOf<NetReflectorConverterException>());
		}

        [Test]
		public void CanCatchInvalidLabelConfiguration()
		{
			ClearCase clearCase = new ClearCase();
			const string invalidXml = "<sourcecontrol type=\"ClearCase\"><useLabel>NOT_A_BOOLEAN</useLabel></sourcecontrol>";
			ClassicAssert.That(delegate { NetReflector.Read(invalidXml, clearCase); },
                        Throws.TypeOf<NetReflectorConverterException>());
		}

		[Test]
		public void ValidateBaselineNameFailsForEmptyString()
		{
			ClassicAssert.That(delegate { clearCase.ValidateBaselineName(""); },
                        Throws.TypeOf<CruiseControlException>());
		}

		[Test]
		public void ValidateBaselineNameFailsForNull()
		{
			ClassicAssert.That(delegate { clearCase.ValidateBaselineName(null); },
                        Throws.TypeOf<CruiseControlException>());
		}

		[Test]
		public void ValidateBaselineNameFailsForNameWithSpaces()
		{
			ClassicAssert.That(delegate { clearCase.ValidateBaselineName("name with spaces"); },
                        Throws.TypeOf<CruiseControlException>());
		}

		[Test]
		public void CanEnforceProjectVobSetIfBaselineTrue()
		{
			clearCase.UseBaseline = true;
			clearCase.ProjectVobName = null;
            ClassicAssert.That(delegate { clearCase.LabelSourceControl(IntegrationResultMother.CreateSuccessful()); },
                        Throws.TypeOf<CruiseControlException>());
		}

		[Test]
		public void CanCreateHistoryProcess()
		{
			DateTime expectedStartDate = DateTime.Parse("02-Feb-2002.05:00:00", CultureInfo.InvariantCulture);

			clearCase.Branch = null;

			string expectedArguments = "lshist -r -nco -since " + expectedStartDate.ToString(ClearCase.DATETIME_FORMAT) + " -fmt \"%u"
				+ ClearCaseHistoryParser.DELIMITER + "%Vd" + ClearCaseHistoryParser.DELIMITER
				+ "%En" + ClearCaseHistoryParser.DELIMITER
				+ "%Vn" + ClearCaseHistoryParser.DELIMITER + "%o" + ClearCaseHistoryParser.DELIMITER
				+ "!%l" + ClearCaseHistoryParser.DELIMITER + "!%a" + ClearCaseHistoryParser.DELIMITER
				+ "%Nc" + ClearCaseHistoryParser.END_OF_LINE_DELIMITER + "\\n\" \"" + VIEWPATH + "\"";
			ProcessInfo processInfo = clearCase.CreateHistoryProcessInfo(expectedStartDate, DateTime.Now);
			ClassicAssert.AreEqual("cleartool.exe", processInfo.FileName);
			ClassicAssert.AreEqual(expectedArguments, processInfo.Arguments);
		}

		[Test]
		public void BranchDetailsAreAppliedToHistroyProcessIfSet()
		{
			DateTime expectedStartDate = DateTime.Parse("02-Feb-2002.05:00:00", CultureInfo.InvariantCulture);

			string expectedArguments = "lshist -r -nco -branch \"" + BRANCH + "\" -since " + expectedStartDate.ToString(ClearCase.DATETIME_FORMAT) + " -fmt \"%u"
				+ ClearCaseHistoryParser.DELIMITER + "%Vd" + ClearCaseHistoryParser.DELIMITER
				+ "%En" + ClearCaseHistoryParser.DELIMITER
				+ "%Vn" + ClearCaseHistoryParser.DELIMITER + "%o" + ClearCaseHistoryParser.DELIMITER
				+ "!%l" + ClearCaseHistoryParser.DELIMITER + "!%a" + ClearCaseHistoryParser.DELIMITER
				+ "%Nc" + ClearCaseHistoryParser.END_OF_LINE_DELIMITER + "\\n\" \"" + VIEWPATH + "\"";
			ProcessInfo processInfo = clearCase.CreateHistoryProcessInfo(expectedStartDate, DateTime.Now);
			ClassicAssert.AreEqual("cleartool.exe", processInfo.FileName);
			ClassicAssert.AreEqual(expectedArguments, processInfo.Arguments);
		}

		[Test]
		public void CanCreateLabelType()
		{
			const string label = "This-is-a-test";
			ProcessInfo labelTypeProcess = clearCase.CreateLabelTypeProcessInfo(label);
			ClassicAssert.AreEqual(" mklbtype -c \"CRUISECONTROL Comment\" \"" + label + "\"", labelTypeProcess.Arguments);
			ClassicAssert.AreEqual("cleartool.exe", labelTypeProcess.FileName);
		}

		[Test]
		public void CanCreateLabelProcess()
		{
			const string label = "This-is-a-test";
			ProcessInfo labelProcess = clearCase.CreateMakeLabelProcessInfo(label);
			ClassicAssert.AreEqual(@" mklabel -recurse """ + label + "\" \"" + VIEWPATH + "\"", labelProcess.Arguments);
			ClassicAssert.AreEqual("cleartool.exe", labelProcess.FileName);
		}

		[Test]
		public void CanIgnoreVobError()
		{
			ClassicAssert.IsFalse(clearCase.HasFatalError(ClearCaseMother.VOB_ERROR_ONLY));
		}

		[Test]
		public void CanDetectError()
		{
			ClassicAssert.IsTrue(clearCase.HasFatalError(ClearCaseMother.REAL_ERROR_WITH_VOB));
		}

		[Test]
		public void ShouldGetSourceIfAutoGetSourceTrue()
		{
			var executor = new Mock<ProcessExecutor>();
			ClearCase clearCase = new ClearCase((ProcessExecutor) executor.Object);
			clearCase.Executable = EXECUTABLE;
			clearCase.ViewPath = VIEWPATH;
			clearCase.AutoGetSource = true;

			ProcessInfo expectedProcessRequest = new ProcessInfo(EXECUTABLE, @"update -force -overwrite """ + VIEWPATH + @"""");
			expectedProcessRequest.TimeOut = Timeout.DefaultTimeout.Millis;

			executor.Setup(e => e.Execute(expectedProcessRequest)).Returns(new ProcessResult("foo", null, 0, false)).Verifiable();
			clearCase.GetSource(new IntegrationResult());
			executor.Verify();
		}

		[Test]
		public void ShouldNotGetSourceIfAutoGetSourceFalse()
		{
			var executor = new Mock<ProcessExecutor>();
			ClearCase clearCase = new ClearCase((ProcessExecutor) executor.Object);
			clearCase.Executable = EXECUTABLE;
			clearCase.ViewPath = VIEWPATH;
			clearCase.AutoGetSource = false;

			clearCase.GetSource(new IntegrationResult());
			executor.Verify();
			executor.VerifyNoOtherCalls();
		}
	}
}
