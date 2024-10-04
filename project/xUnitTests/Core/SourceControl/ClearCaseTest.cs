using System;
using System.Globalization;
using Exortech.NetReflector;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	
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

		// [SetUp]
		protected void Setup()
		{
			clearCase = new ClearCase();
			NetReflector.Read(CLEARCASE_XML, clearCase);
		}

		[Fact]
		public void CanCreateTemporaryBaselineProcessInfo()
		{
			const string name = "baselinename";
			ProcessInfo info = clearCase.CreateTempBaselineProcessInfo(name);
			Assert.Equal(string.Format(System.Globalization.CultureInfo.CurrentCulture,"{0} mkbl -view {1} -identical {2}",
			                              EXECUTABLE,
			                              VIEWNAME,
			                              name),
			                info.FileName + " " + info.Arguments);
            Assert.True(true);
        }

		[Fact]
		public void CanCreateRemoveBaselineProcessInfo()
		{
			ProcessInfo info = clearCase.CreateRemoveBaselineProcessInfo();
			Assert.Equal(string.Format(System.Globalization.CultureInfo.CurrentCulture,"{0} rmbl -force {1}@\\{2}",
			                              EXECUTABLE,
			                              clearCase.TempBaseline,
			                              clearCase.ProjectVobName),
			                info.FileName + " " + info.Arguments);
		}

		[Fact]
		public void CanCreateRenameBaselineProcesInfo()
		{
			const string newName = "HiImANewBaselineName";
			ProcessInfo info = clearCase.CreateRenameBaselineProcessInfo(newName);
			Assert.Equal(string.Format(System.Globalization.CultureInfo.CurrentCulture,"{0} rename baseline:{1}@\\{2} \"{3}\"",
			                              EXECUTABLE,
			                              clearCase.TempBaseline,
			                              clearCase.ProjectVobName,
			                              newName),
			                info.FileName + " " + info.Arguments);
		}


		[Fact]
		public void ShouldPopulateCorrectlyFromXml()
		{
			Assert.Equal(EXECUTABLE, clearCase.Executable);
			Assert.Equal(VIEWPATH, clearCase.ViewPath);
			Assert.Equal(VIEWNAME, clearCase.ViewName);
			Assert.Equal(Convert.ToBoolean(USE_BASELINE), clearCase.UseBaseline);
			Assert.Equal(Convert.ToBoolean(USE_LABEL), clearCase.UseLabel);
			Assert.Equal(PROJECT_VOB_NAME, clearCase.ProjectVobName);
			Assert.Equal(BRANCH, clearCase.Branch);
		}

		[Fact]
		public void CanCatchInvalidBaselineConfiguration()
		{
			ClearCase clearCase = new ClearCase();
			const string invalidXml = "<sourcecontrol type=\"ClearCase\"><useBaseline>NOT_A_BOOLEAN</useBaseline></sourcecontrol>";
			Assert.True(delegate { NetReflector.Read(invalidXml, clearCase); },
                        Throws.TypeOf<NetReflectorConverterException>());
		}

        [Fact]
		public void CanCatchInvalidLabelConfiguration()
		{
			ClearCase clearCase = new ClearCase();
			const string invalidXml = "<sourcecontrol type=\"ClearCase\"><useLabel>NOT_A_BOOLEAN</useLabel></sourcecontrol>";
			Assert.True(delegate { NetReflector.Read(invalidXml, clearCase); },
                        Throws.TypeOf<NetReflectorConverterException>());
		}

		[Fact]
		public void ValidateBaselineNameFailsForEmptyString()
		{
			Assert.True(delegate { clearCase.ValidateBaselineName(""); },
                        Throws.TypeOf<CruiseControlException>());
		}

		[Fact]
		public void ValidateBaselineNameFailsForNull()
		{
			Assert.True(delegate { clearCase.ValidateBaselineName(null); },
                        Throws.TypeOf<CruiseControlException>());
		}

		[Fact]
		public void ValidateBaselineNameFailsForNameWithSpaces()
		{
			Assert.True(delegate { clearCase.ValidateBaselineName("name with spaces"); },
                        Throws.TypeOf<CruiseControlException>());
		}

		[Fact]
		public void CanEnforceProjectVobSetIfBaselineTrue()
		{
			clearCase.UseBaseline = true;
			clearCase.ProjectVobName = null;
            Assert.True(delegate { clearCase.LabelSourceControl(IntegrationResultMother.CreateSuccessful()); },
                        Throws.TypeOf<CruiseControlException>());
		}

		[Fact]
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
			Assert.Equal("cleartool.exe", processInfo.FileName);
			Assert.Equal(expectedArguments, processInfo.Arguments);
		}

		[Fact]
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
			Assert.Equal("cleartool.exe", processInfo.FileName);
			Assert.Equal(expectedArguments, processInfo.Arguments);
		}

		[Fact]
		public void CanCreateLabelType()
		{
			const string label = "This-is-a-test";
			ProcessInfo labelTypeProcess = clearCase.CreateLabelTypeProcessInfo(label);
			Assert.Equal(" mklbtype -c \"CRUISECONTROL Comment\" \"" + label + "\"", labelTypeProcess.Arguments);
			Assert.Equal("cleartool.exe", labelTypeProcess.FileName);
		}

		[Fact]
		public void CanCreateLabelProcess()
		{
			const string label = "This-is-a-test";
			ProcessInfo labelProcess = clearCase.CreateMakeLabelProcessInfo(label);
			Assert.Equal(@" mklabel -recurse """ + label + "\" \"" + VIEWPATH + "\"", labelProcess.Arguments);
			Assert.Equal("cleartool.exe", labelProcess.FileName);
		}

		[Fact]
		public void CanIgnoreVobError()
		{
			Assert.False(clearCase.HasFatalError(ClearCaseMother.VOB_ERROR_ONLY));
		}

		[Fact]
		public void CanDetectError()
		{
			Assert.True(clearCase.HasFatalError(ClearCaseMother.REAL_ERROR_WITH_VOB));
		}

		[Fact]
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

		[Fact]
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
