using System;
using System.Globalization;
using System.IO;
using Exortech.NetReflector;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Config;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
    //[Ignore("Ignore until fixed!")]
	public class CvsTest : ProcessExecutorTestFixtureBase
	{
		private Cvs cvs;
		private Mock<IHistoryParser> mockHistoryParser;
		private DateTime from;
		private DateTime to;
		private Mock<IFileSystem> mockFileSystem;
        private IExecutionEnvironment executionEnv;

		// [SetUp]
		protected void CreateCvs()
		{
			mockHistoryParser = new Mock<IHistoryParser>();
			mockFileSystem = new Mock<IFileSystem>();
            executionEnv = new ExecutionEnvironment();
			CreateProcessExecutorMock(Cvs.DefaultCvsExecutable);
            cvs = new Cvs((IHistoryParser)mockHistoryParser.Object, (ProcessExecutor)mockProcessExecutor.Object, (IFileSystem)mockFileSystem.Object, executionEnv);
			from = new DateTime(2001, 1, 21, 20, 0, 0);
			to = from.AddDays(1);
		}

		// [TearDown]
		protected void VerifyAll()
		{
			Verify();
			mockHistoryParser.Verify();
			mockFileSystem.Verify();
		}

		private const string xml = @"<sourceControl type=""cvs"" autoGetSource=""true"">
      <executable>..\tools\cvs.exe</executable>
      <workingDirectory>..</workingDirectory>
      <cvsroot>myCvsRoot</cvsroot>
	  <branch>branch</branch>
	  <module>module</module>
	  <suppressRevisionHeader>true</suppressRevisionHeader>
    </sourceControl>";

		[Fact]
		public void PopulateFromXml()
		{
			NetReflector.Read(xml, cvs);
            Assert.True(true);
            Assert.Equal(@"..\tools\cvs.exe", cvs.Executable);
			Assert.Equal("..", cvs.WorkingDirectory);
			Assert.Equal("myCvsRoot", cvs.CvsRoot);
			Assert.Equal("branch", cvs.Branch);
			Assert.Equal(true, cvs.AutoGetSource);
			Assert.Equal(true, cvs.CleanCopy);
			Assert.Equal("module", cvs.Module);
			Assert.Equal(true, cvs.SuppressRevisionHeader);
		}

		[Fact]
		public void PopulateFromMinimalXml()
		{
			string minimalXml = @"<cvs><executable>c:\cvs\cvs.exe</executable><cvsroot>:local:C:\dev\CVSRoot</cvsroot><module>ccnet</module></cvs>";
			NetReflector.Read(minimalXml, cvs);
			Assert.Equal(@"c:\cvs\cvs.exe", cvs.Executable);
			Assert.Equal(@":local:C:\dev\CVSRoot", cvs.CvsRoot);
			Assert.Equal(@"ccnet", cvs.Module);
			Assert.False(cvs.SuppressRevisionHeader);
		}

		[Fact]
		public void SerializeToXml()
		{
			NetReflector.Read(xml, cvs);
			string y = NetReflector.Write(cvs);
			XmlUtil.VerifyXmlIsWellFormed(y);
		}

		[Fact]
		public void VerifyLogCommandArgumentsWithoutCvsRoot()
		{
			ExpectToExecuteArguments(string.Format(@"-q rlog -N -b ""-d>{0}"" module", cvs.FormatCommandDate(from)));
			ExpectToParseAndReturnNoModifications();

			cvs.Module = "module";
			cvs.WorkingDirectory = DefaultWorkingDirectory;
			cvs.GetModifications(IntegrationResult(from), IntegrationResult(to));
		}

		[Fact]
		public void VerifyLogCommandArgsWithCvsRootAndBranch()
		{
			ExpectToExecuteArguments(string.Format(@"-d myCvsRoot -q rlog -N -rbranch ""-d>{0}"" module", cvs.FormatCommandDate(from)));
			ExpectToParseAndReturnNoModifications();

			cvs.CvsRoot = "myCvsRoot";
			cvs.Module = "module";
			cvs.Branch = "branch";
			cvs.WorkingDirectory = DefaultWorkingDirectory;
			cvs.GetModifications(IntegrationResult(from), IntegrationResult(to));
		}

		[Fact]
		public void ShouldBuildCorrectLogProcessIfRestrictedLogins()
		{
			ExpectToExecuteArguments(string.Format(@"-d myCvsRoot -q rlog -N -b ""-d>{0}"" -wexortech -wmonkey module", cvs.FormatCommandDate(from)));
			ExpectToParseAndReturnNoModifications();

			cvs.CvsRoot = "myCvsRoot";
			cvs.Module = "module";			
			cvs.RestrictLogins = "exortech, monkey";
			cvs.WorkingDirectory = DefaultWorkingDirectory;
			cvs.GetModifications(IntegrationResult(from), IntegrationResult(to));
		}

		[Fact]
		public void ShouldBuildCorrectLogProcessIfSuppressRevisionHeaderIsSelected()
		{
			ExpectToExecuteArguments(string.Format(@"-d myCvsRoot -q rlog -N -S -b ""-d>{0}"" module", cvs.FormatCommandDate(from)));
			ExpectToParseAndReturnNoModifications();

			cvs.CvsRoot = "myCvsRoot";
			cvs.Module = "module";
			cvs.SuppressRevisionHeader = true;
			cvs.WorkingDirectory = DefaultWorkingDirectory;
			cvs.GetModifications(IntegrationResult(from), IntegrationResult(to));
		}

		[Fact]
		public void CvsShouldBeDefaultExecutable()
		{
			Assert.Equal("cvs", cvs.Executable);
		}

		[Fact]
		public void VerifyDateIsFormatedCorrectly()
		{
			DateTime dt = DateTime.Parse("2003-01-01 01:01:01 GMT", CultureInfo.InvariantCulture);
			Assert.Equal("2003-01-01 01:01:01 GMT", cvs.FormatCommandDate(dt));
		}

		[Fact]
		public void VerifyProcessInfoForGetSource()
		{
			ExpectToExecuteArguments(@"-q update -d -P -C");
			ExpectCvsDirectoryExists(true);

			cvs.AutoGetSource = true;
			cvs.CleanCopy = true; // set as default
			cvs.WorkingDirectory = DefaultWorkingDirectory;
			cvs.GetSource(IntegrationResult());
		}

		[Fact]
		public void VerifyProcessInfoForGetSourceOnBranch()
		{
			ExpectToExecuteArguments(@"-q update -d -P -C -r branch");
			ExpectCvsDirectoryExists(true);

			cvs.AutoGetSource = true;
			cvs.Branch = "branch";
			cvs.CleanCopy = true; // set as default
			cvs.WorkingDirectory = DefaultWorkingDirectory;
			cvs.GetSource(IntegrationResult());
		}

		[Fact]
		public void ShouldCheckoutInsteadOfUpdateIfCVSFoldersDoNotExist()
		{
            var lastDirectorySeparatorIndex = DefaultWorkingDirectory.TrimEnd().TrimEnd(Path.DirectorySeparatorChar).LastIndexOf(Path.DirectorySeparatorChar);
            var checkoutWd = DefaultWorkingDirectory.Substring(0, lastDirectorySeparatorIndex);
            var checkoutDir = DefaultWorkingDirectory.Substring(lastDirectorySeparatorIndex).Trim(Path.DirectorySeparatorChar);

		    ExpectToExecuteArguments(
		        string.Format(
		            @"-d :pserver:anonymous@ccnet.cvs.sourceforge.net:/cvsroot/ccnet -q checkout -R -P -d {0} ccnet",
                    StringUtil.AutoDoubleQuoteString(checkoutDir)), checkoutWd);

			ExpectCvsDirectoryExists(false);

			cvs.CvsRoot = ":pserver:anonymous@ccnet.cvs.sourceforge.net:/cvsroot/ccnet";
			cvs.Module = "ccnet";
			cvs.AutoGetSource = true;
			cvs.WorkingDirectory = DefaultWorkingDirectory;
			cvs.GetSource(IntegrationResult());
		}

		[Fact]
		public void ShouldCheckoutFromBranchInsteadOfUpdateIfCVSFoldersDoNotExist()
		{
            var lastDirectorySeparatorIndex = DefaultWorkingDirectory.TrimEnd().TrimEnd(Path.DirectorySeparatorChar).LastIndexOf(Path.DirectorySeparatorChar);
            var checkoutWd = DefaultWorkingDirectory.Substring(0, lastDirectorySeparatorIndex);
            var checkoutDir = DefaultWorkingDirectory.Substring(lastDirectorySeparatorIndex).Trim(Path.DirectorySeparatorChar);

		    ExpectToExecuteArguments(
		        string.Format(
		            @"-d :pserver:anonymous@ccnet.cvs.sourceforge.net:/cvsroot/ccnet -q checkout -R -P -r branch -d {0} ccnet",
                    StringUtil.AutoDoubleQuoteString(checkoutDir)), checkoutWd);

			ExpectCvsDirectoryExists(false);

			cvs.CvsRoot = ":pserver:anonymous@ccnet.cvs.sourceforge.net:/cvsroot/ccnet";
			cvs.Module = "ccnet";
			cvs.AutoGetSource = true;
			cvs.Branch = "branch";
			cvs.WorkingDirectory = DefaultWorkingDirectory;
			cvs.GetSource(IntegrationResult());
		}

		[Fact]
		public void ShouldCheckoutOnWorkingDictionaryWithSpaces()
		{
            var lastDirectorySeparatorIndex = DefaultWorkingDirectoryWithSpaces.TrimEnd().TrimEnd(Path.DirectorySeparatorChar).LastIndexOf(Path.DirectorySeparatorChar);
            var checkoutWd = DefaultWorkingDirectoryWithSpaces.Substring(0, lastDirectorySeparatorIndex);
            var checkoutDir = DefaultWorkingDirectoryWithSpaces.Substring(lastDirectorySeparatorIndex).Trim(Path.DirectorySeparatorChar);

		    ExpectToExecuteArguments(
		        string.Format(
		            @"-d :pserver:anonymous@ccnet.cvs.sourceforge.net:/cvsroot/ccnet -q checkout -R -P -r branch -d {0} ccnet",
                    StringUtil.AutoDoubleQuoteString(checkoutDir)), checkoutWd);

			mockFileSystem.Setup(fileSystem => fileSystem.DirectoryExists(Path.Combine(DefaultWorkingDirectoryWithSpaces, "CVS"))).Returns(false).Verifiable();

			cvs.CvsRoot = ":pserver:anonymous@ccnet.cvs.sourceforge.net:/cvsroot/ccnet";
			cvs.Module = "ccnet";
			cvs.AutoGetSource = true;
			cvs.Branch = "branch";
			cvs.WorkingDirectory = DefaultWorkingDirectoryWithSpaces;
			cvs.GetSource(IntegrationResult());
		}

		[Fact]
		public void ShouldThrowExceptionIfCVSRootIsNotSpecifiedAndCVSFoldersDoNotExist()
		{
			ExpectCvsDirectoryExists(false);
			
			cvs.AutoGetSource = true;
            Assert.Throws<ConfigurationException>(delegate { cvs.GetSource(IntegrationResult()); });
		}

		[Fact]
		public void ShouldUseCvsRootWithGetSource()
		{
			ExpectToExecuteArguments(@"-d myCvsRoot -q update -d -P -C");
			ExpectCvsDirectoryExists(true);

			cvs.AutoGetSource = true;
			cvs.CvsRoot = "myCvsRoot";
			cvs.GetSource(IntegrationResult());
		}

		[Fact]
		public void ShouldNotGetSourceIfAutoGetSourceIsFalse()
		{
			cvs.AutoGetSource = false;
			cvs.GetSource(IntegrationResult());
			mockProcessExecutor.VerifyNoOtherCalls();
		}

		[Fact]
		public void ShouldRebaseWorkingDirectoryForGetSource()
		{
			ExpectToExecuteArguments(@"-q update -d -P -C", Path.Combine(DefaultWorkingDirectory, "myproject"));
			mockFileSystem.Setup(fileSystem => fileSystem.DirectoryExists(Path.Combine(Path.Combine(DefaultWorkingDirectory, "myproject"), "CVS"))).Returns(true).Verifiable();

			cvs.AutoGetSource = true;
			cvs.CleanCopy = true; // set as default
			cvs.WorkingDirectory = "myproject";
			IntegrationResult result = new IntegrationResult();
			result.WorkingDirectory = DefaultWorkingDirectory;
			cvs.GetSource(result);
		}

		[Fact]
		public void ShouldBuildCorrectLabelProcessInfo()
		{
			ExpectToExecuteArguments("tag ver-foo");
			cvs.LabelOnSuccess = true;
			cvs.WorkingDirectory = DefaultWorkingDirectory;
			cvs.LabelSourceControl(IntegrationResultMother.CreateSuccessful("foo"));
		}

		[Fact]
		public void ShouldUseTagPrefixInLabelSpecificationIfSpecified()
		{
			ExpectToExecuteArguments("tag MyCustomPrefix_foo");

			cvs.TagPrefix = "MyCustomPrefix_";
			cvs.LabelOnSuccess = true;
			cvs.WorkingDirectory = DefaultWorkingDirectory;
			cvs.LabelSourceControl(IntegrationResultMother.CreateSuccessful("foo"));
		}

		[Fact]
		public void ShouldBuildCorrectLabelProcessInfoIfCvsRootIsSpecified()
		{
			ExpectToExecuteArguments("-d myCvsRoot tag ver-foo");
			cvs.CvsRoot = "myCvsRoot";
			cvs.LabelOnSuccess = true;
			cvs.WorkingDirectory = DefaultWorkingDirectory;
			cvs.LabelSourceControl(IntegrationResultMother.CreateSuccessful("foo"));
		}

		[Fact]
		public void ShouldConvertLabelsThatContainIllegalCharacters()
		{
			ExpectToExecuteArguments("-d myCvsRoot tag ver-2_1_4");
			cvs.CvsRoot = "myCvsRoot";
			cvs.LabelOnSuccess = true;
			cvs.WorkingDirectory = DefaultWorkingDirectory;
			cvs.LabelSourceControl(IntegrationResultMother.CreateSuccessful("2.1.4"));
		}

		[Fact]
		public void ShouldStripRepositoryFolderFromModificationFolderNames()
		{
			ExpectToExecuteArguments(string.Format(@"-d :pserver:anonymous@cruisecontrol.cvs.sourceforge.net:/cvsroot/cruisecontrol -q rlog -N -b ""-d>{0}"" cruisecontrol", cvs.FormatCommandDate(from)));
			Modification mod = new Modification();
			mod.FolderName = @"/cvsroot/cruisecontrol/cruisecontrol/main";
			ExpectToParseAndReturnModifications(new Modification[]{ mod });

			cvs.CvsRoot = @":pserver:anonymous@cruisecontrol.cvs.sourceforge.net:/cvsroot/cruisecontrol";
			cvs.Module = "cruisecontrol";
			Modification[] modifications = cvs.GetModifications(IntegrationResult(from), IntegrationResult(to));
			Assert.Equal("main", modifications[0].FolderName);
		}

		[Fact]
		public void ShouldStripRepositoryFolderFromModificationFolderNamesForLocalProtocol()
		{
			ExpectToExecuteArguments(string.Format(@"-d :local:C:\dev\CVSRoot -q rlog -N -b ""-d>{0}"" fitwebservice", cvs.FormatCommandDate(from)));
			Modification mod = new Modification();
			mod.FolderName = @"C:\dev\CVSRoot/fitwebservice/src/fitwebservice/src";
			ExpectToParseAndReturnModifications(new Modification[]{ mod });

			cvs.CvsRoot = @":local:C:\dev\CVSRoot";
			cvs.Module = "fitwebservice";
			Modification[] modifications = cvs.GetModifications(IntegrationResult(from), IntegrationResult(to));
			Assert.Equal("src/fitwebservice/src", modifications[0].FolderName);
		}

		private void ExpectToParseAndReturnNoModifications()
		{
			ExpectToParseAndReturnModifications(new Modification[0]);
		}

		private void ExpectToParseAndReturnModifications(Modification[] modifications)
		{
			mockHistoryParser.Setup(parser => parser.Parse(It.IsAny<TextReader>(), from, to)).Returns(modifications).Verifiable();
		}

		private void ExpectCvsDirectoryExists(bool doesCvsDirectoryExist)
		{
			mockFileSystem.Setup(fileSystem => fileSystem.DirectoryExists(Path.Combine(DefaultWorkingDirectory, "CVS"))).Returns(doesCvsDirectoryExist).Verifiable();
		}
	}
}
