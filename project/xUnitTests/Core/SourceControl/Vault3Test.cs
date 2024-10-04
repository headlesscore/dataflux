using System;
using System.IO;
using Exortech.NetReflector;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	
	public class Vault3Test : ProcessExecutorTestFixtureBase
	{
		protected readonly string listFolderOutputWithWorkingFolderSet = @"
			<vault>
				<listworkingfolders>
						<workingfolder reposfolder=""$"" localfolder=""{0}"" />
				</listworkingfolders>
				<result success=""yes"" />
			</vault>";
		protected readonly string listFolderOutputWithNonXml = @"
			Some junk to be removed
			<vault>
				<listworkingfolders>
						<workingfolder reposfolder=""$"" localfolder=""{0}"" />
				</listworkingfolders>
				<result success=""yes"" />
			</vault>";
		protected readonly string listFolderOutputWithNoWorkingFolderSet = @"
			<vault>
				<listworkingfolders />
				<result success=""yes"" />
			</vault>";

		protected VaultVersionChecker vault;
		protected Mock<IHistoryParser> mockHistoryParser;
		protected IntegrationResult result;

		/* 
		* CleanFolder tests commented out because tests that are tied to a
		* particular file system layout are undesirable, per Owen 11/29/05
		* 
		private string tempFileToTestCleanCopy = null;
		private bool expectCleanCopy = false;
		*/

		// [SetUp]
		public virtual void SetUp()
		{
			CreateProcessExecutorMock(VaultVersionChecker.DefaultExecutable);
			mockHistoryParser = new Mock<IHistoryParser>();
			vault = new VaultVersionChecker((IHistoryParser) mockHistoryParser.Object, (ProcessExecutor) mockProcessExecutor.Object, VaultVersionChecker.EForcedVaultVersion.Vault3);

			result = IntegrationResultMother.CreateSuccessful("foo");
			result.WorkingDirectory = this.DefaultWorkingDirectory;
		}

		[Fact]
		public virtual void ValuesShouldBeSetFromConfigurationXml()
		{
			const string ST_XML_SSL = @"<vault>
				<executable>d:\program files\sourcegear\vault client\vault.exe</executable>
				<username>username</username>
				<password>password</password>
				<host>localhost</host>
				<repository>repository</repository>
				<folder>$\foo</folder>
				<ssl>True</ssl>
				<autoGetSource>True</autoGetSource>
				<applyLabel>True</applyLabel>
				<useWorkingDirectory>false</useWorkingDirectory>
				<historyArgs>-blah test</historyArgs>
				<timeout>2400000</timeout>
				<workingDirectory>c:\source</workingDirectory>
				<cleanCopy>true</cleanCopy>
				<setFileTime>current</setFileTime>
				<proxyServer>proxyhost</proxyServer>
				<proxyPort>12345</proxyPort>
				<proxyUser>proxyuser</proxyUser>
				<proxyPassword>proxypassword</proxyPassword>
				<proxyDomain>proxydomain</proxyDomain>
				<pollRetryAttempts>10</pollRetryAttempts>
				<pollRetryWait>30</pollRetryWait >
			</vault>";

			vault = CreateVault(ST_XML_SSL);
			Assert.Equal(@"d:\program files\sourcegear\vault client\vault.exe", vault.Executable);
			Assert.Equal("username", vault.Username);
			Assert.Equal("password", vault.Password.PrivateValue);
			Assert.Equal("localhost", vault.Host);
			Assert.Equal("repository", vault.Repository);
			Assert.Equal("$\\foo", vault.Folder);
			Assert.Equal(true, vault.Ssl);
			Assert.Equal(true, vault.AutoGetSource);
			Assert.Equal(true, vault.ApplyLabel);
			Assert.Equal(false, vault.UseVaultWorkingDirectory);
			Assert.Equal("-blah test", vault.HistoryArgs);
			Assert.Equal(2400000, vault.Timeout.Millis);
			Assert.Equal(@"c:\source", vault.WorkingDirectory);
			Assert.Equal(true, vault.CleanCopy);
			Assert.Equal("current", vault.setFileTime);
			Assert.Equal("proxyhost", vault.proxyServer);
			Assert.Equal("12345", vault.proxyPort);
			Assert.Equal("proxyuser", vault.proxyUser);
			Assert.Equal("proxypassword", vault.proxyPassword);
			Assert.Equal("proxydomain", vault.proxyDomain);
			Assert.Equal(10, vault.pollRetryAttempts);
			Assert.Equal(30, vault.pollRetryWait);
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public virtual void ShouldBePopulatedWithDefaultValuesWhenLoadingFromMinimalXml()
		{
			vault = CreateVault
				(@"
				<vault>
					<host>localhost</host>
					<username>name</username>
					<password>password</password>
					<repository>repository</repository>
				</vault>
			");
			Assert.Equal(VaultVersionChecker.DefaultExecutable, vault.Executable);
			Assert.Equal("name", vault.Username);
			Assert.Equal("password", vault.Password.PrivateValue);
			Assert.Equal("localhost", vault.Host);
			Assert.Equal("repository", vault.Repository);
			Assert.Equal("$", vault.Folder);
			Assert.Equal(false, vault.Ssl);
			Assert.Equal(true, vault.AutoGetSource);
			Assert.Equal(false, vault.ApplyLabel);
			Assert.Equal(true, vault.UseVaultWorkingDirectory);
			Assert.Equal(VaultVersionChecker.DefaultHistoryArgs, vault.HistoryArgs);
			Assert.Equal(Timeout.DefaultTimeout, vault.Timeout);
			Assert.Null(vault.WorkingDirectory);
			Assert.Equal(false, vault.CleanCopy);
			Assert.Equal("checkin", vault.setFileTime);
			Assert.Null(vault.proxyServer);
			Assert.Null(vault.proxyPort);
			Assert.Null(vault.proxyUser);
			Assert.Null(vault.proxyPassword);
			Assert.Null(vault.proxyDomain);
			Assert.Equal(VaultVersionChecker.DefaultPollRetryAttempts, vault.pollRetryAttempts);
			Assert.Equal(VaultVersionChecker.DefaultPollRetryWait, vault.pollRetryWait);
		}

		[Fact]
		public virtual void ShouldBuildGetModificationsArgumentsCorrectly()
		{
			DateTime today = DateTime.Now;
			DateTime yesterday = today.AddDays(-1);
			result.StartTime = yesterday;
			string args = string.Format(@"history $ -excludeactions label,obliterate -rowlimit 0 -begindate {0:s} -enddate {1:s}{2}",
			                            yesterday, today, SetAndGetCommonOptionalArguments());
			ExpectToExecuteArguments(args);
			ExpectToParseHistory();

			vault.GetModifications(result, IntegrationResultMother.CreateSuccessful(today));
			VerifyAll();
		}

		/*
		 *               Get Source Scenarios           || Correct Action
		 * ---------------------------------------------||-------------------------------------------------------------------
		 *          |       |   Use   | Working |       ||  List   |
		 *          | Apply | Working | Folder  | Clean || Working | Get Command
		 * Scenario | Label | Folder  | Spec'd  | Copy  || Folders | and Arguments
		 * ------------------------------------------------------------------------------------------------------------------
		 *    1     |   T   |    T    |    T    |   T   ||    F    | getlabel -labelworkingfolder <specified working folder>
		 *    2     |   T   |    T    |    T    |   F   ||    F    | getlabel -labelworkingfolder <specified working folder>
		 *    3     |   T   |    T    |    F    |   T   ||    T    | getlabel -labelworkingfolder <retrieved working folder>
		 *    4     |   T   |    T    |    F    |   F   ||    T    | getlabel -labelworkingfolder <retrieved working folder>
		 *    5     |   T   |    F    |    T    |   T   ||    F    | getlabel -destpath <specified working folder>
		 *    6     |   T   |    F    |    T    |   F   ||    F    | getlabel -destpath <specified working folder>
		 *    7     |   T   |    F    |    F    |   T   ||    T    | getlabel -destpath <retrieved working folder>
		 *    8     |   T   |    F    |    F    |   F   ||    T    | getlabel -destpath <retrieved working folder>
		 *    9     |   F   |    T    |    T    |   T   ||    F    | get
		 *   10     |   F   |    T    |    T    |   F   ||    F    | get
		 *   11     |   F   |    T    |    F    |   T   ||    T    | get
		 *   12     |   F   |    T    |    F    |   F   ||    F    | get
		 *   13     |   F   |    F    |    T    |   T   ||    F    | get -destpath <specified working folder>
		 *   14     |   F   |    F    |    T    |   F   ||    F    | get -destpath <specified working folder>
		 *   15     |   F   |    F    |    F    |   T   ||    T    | get -destpath <retrieved working folder>
		 *   16     |   F   |    F    |    F    |   F   ||    T    | get -destpath <retrieved working folder>
		 */

		[Fact]
		public virtual void ArgumentsCorrectForGetSourceScenario1()
		{
			vault.Folder = "$";
			vault.AutoGetSource = true;

			vault.ApplyLabel = true;
			vault.UseVaultWorkingDirectory = true;
			vault.WorkingDirectory = DefaultWorkingDirectory;
			vault.CleanCopy = true;

			ExpectToCleanFolder();
			ExpectToExecuteArguments(@"label $ foo" + SetAndGetCommonOptionalArguments());
			ExpectToExecuteArguments(@"getlabel $ foo -labelworkingfolder " + StringUtil.AutoDoubleQuoteString(DefaultWorkingDirectory)
				+ GetWorkingFolderArguments() + GetFileTimeArgument() + SetAndGetCommonOptionalArguments());

			vault.GetSource(result);
			VerifyAll();
		}

		[Fact]
		public virtual void ArgumentsCorrectForGetSourceScenario2()
		{
			vault.Folder = "$";
			vault.AutoGetSource = true;

			vault.ApplyLabel = true;
			vault.UseVaultWorkingDirectory = true;
			vault.WorkingDirectory = DefaultWorkingDirectory;
			vault.CleanCopy = false;

			ExpectToNotCleanFolder();
			ExpectToExecuteArguments(@"label $ foo" + SetAndGetCommonOptionalArguments());
			ExpectToExecuteArguments(@"getlabel $ foo -labelworkingfolder " + StringUtil.AutoDoubleQuoteString(DefaultWorkingDirectory)
				+ GetWorkingFolderArguments() + GetFileTimeArgument() + SetAndGetCommonOptionalArguments());

			vault.GetSource(result);
			VerifyAll();
		}

		[Fact]
		public virtual void ArgumentsCorrectForGetSourceScenario3()
		{
			vault.Folder = "$";
			vault.AutoGetSource = true;

			vault.ApplyLabel = true;
			vault.UseVaultWorkingDirectory = true;
			vault.WorkingDirectory = null;
			vault.CleanCopy = true;

			this.ProcessResultOutput = string.Format(listFolderOutputWithWorkingFolderSet, DefaultWorkingDirectory);
			ExpectToExecuteArguments(@"listworkingfolders" + SetAndGetCommonOptionalArguments());
			ExpectToExecuteArguments(@"label $ foo" + SetAndGetCommonOptionalArguments());
			ExpectToExecuteArguments(@"getlabel $ foo -labelworkingfolder " + StringUtil.AutoDoubleQuoteString(DefaultWorkingDirectory)
				+ GetWorkingFolderArguments() + GetFileTimeArgument() + SetAndGetCommonOptionalArguments());

			vault.GetSource(result);
			VerifyAll();
		}

		[Fact]
		public virtual void ArgumentsCorrectForGetSourceScenario4()
		{
			vault.Folder = "$";
			vault.AutoGetSource = true;

			vault.ApplyLabel = true;
			vault.UseVaultWorkingDirectory = true;
			vault.WorkingDirectory = null;
			vault.CleanCopy = false;

			this.ProcessResultOutput = string.Format(listFolderOutputWithWorkingFolderSet, DefaultWorkingDirectory);
			ExpectToExecuteArguments(@"listworkingfolders" + SetAndGetCommonOptionalArguments());
			ExpectToExecuteArguments(@"label $ foo" + SetAndGetCommonOptionalArguments());
			ExpectToExecuteArguments(@"getlabel $ foo -labelworkingfolder " + StringUtil.AutoDoubleQuoteString(DefaultWorkingDirectory)
				+ GetWorkingFolderArguments() + GetFileTimeArgument() + SetAndGetCommonOptionalArguments());

			vault.GetSource(result);
			VerifyAll();
		}

		[Fact]
		public virtual void ArgumentsCorrectForGetSourceScenario5()
		{
			vault.Folder = "$";
			vault.AutoGetSource = true;

			vault.ApplyLabel = true;
			vault.UseVaultWorkingDirectory = false;
			vault.WorkingDirectory = DefaultWorkingDirectory;
			vault.CleanCopy = true;

			this.ProcessResultOutput =string.Empty;
			ExpectToCleanFolder();
			ExpectToExecuteArguments(@"label $ foo" + SetAndGetCommonOptionalArguments());
			ExpectToExecuteArguments(@"getlabel $ foo -destpath " + StringUtil.AutoDoubleQuoteString(DefaultWorkingDirectory)
				+ GetWorkingFolderArguments() + GetFileTimeArgument() + SetAndGetCommonOptionalArguments());

			vault.GetSource(result);
			VerifyAll();
		}

		[Fact]
		public virtual void ArgumentsCorrectForGetSourceScenario6()
		{
			vault.Folder = "$";
			vault.AutoGetSource = true;

			vault.ApplyLabel = true;
			vault.UseVaultWorkingDirectory = false;
			vault.WorkingDirectory = DefaultWorkingDirectory;
			vault.CleanCopy = false;

			this.ProcessResultOutput =string.Empty;
			ExpectToNotCleanFolder();
			ExpectToExecuteArguments(@"label $ foo" + SetAndGetCommonOptionalArguments());
			ExpectToExecuteArguments(@"getlabel $ foo -destpath " + StringUtil.AutoDoubleQuoteString(DefaultWorkingDirectory)
				+ GetWorkingFolderArguments() + GetFileTimeArgument() + SetAndGetCommonOptionalArguments());

			vault.GetSource(result);
			VerifyAll();
		}

		[Fact]
		public virtual void ArgumentsCorrectForGetSourceScenario7()
		{
			vault.Folder = "$";
			vault.AutoGetSource = true;

			vault.ApplyLabel = true;
			vault.UseVaultWorkingDirectory = false;
			vault.WorkingDirectory = @"";
			vault.CleanCopy = true;

			this.ProcessResultOutput = string.Format(listFolderOutputWithWorkingFolderSet, DefaultWorkingDirectory);
			ExpectToExecuteArguments(@"listworkingfolders" + SetAndGetCommonOptionalArguments());
			ExpectToExecuteArguments(@"label $ foo" + SetAndGetCommonOptionalArguments());
			ExpectToExecuteArguments(@"getlabel $ foo -destpath " + StringUtil.AutoDoubleQuoteString(DefaultWorkingDirectory)
				+ GetWorkingFolderArguments() + GetFileTimeArgument() + SetAndGetCommonOptionalArguments());

			vault.GetSource(result);
			VerifyAll();
		}

		[Fact]
		public virtual void ArgumentsCorrectForGetSourceScenario8()
		{
			vault.Folder = "$";
			vault.AutoGetSource = true;

			vault.ApplyLabel = true;
			vault.UseVaultWorkingDirectory = false;
			vault.WorkingDirectory = @"";
			vault.CleanCopy = false;

			this.ProcessResultOutput = string.Format(listFolderOutputWithWorkingFolderSet, DefaultWorkingDirectory);
			ExpectToExecuteArguments(@"listworkingfolders" + SetAndGetCommonOptionalArguments());
			ExpectToExecuteArguments(@"label $ foo" + SetAndGetCommonOptionalArguments());
			ExpectToExecuteArguments(@"getlabel $ foo -destpath " + StringUtil.AutoDoubleQuoteString(DefaultWorkingDirectory)
				+ GetWorkingFolderArguments() + GetFileTimeArgument() + SetAndGetCommonOptionalArguments());

			vault.GetSource(result);
			VerifyAll();
		}

		[Fact]
		public virtual void ArgumentsCorrectForGetSourceScenario9()
		{
			vault.Folder = "$";
			vault.AutoGetSource = true;

			vault.ApplyLabel = false;
			vault.UseVaultWorkingDirectory = true;
			vault.WorkingDirectory = DefaultWorkingDirectory;
			vault.CleanCopy = true;

			this.ProcessResultOutput =string.Empty;
			ExpectToCleanFolder();
			ExpectToExecuteArguments(@"get $" + GetWorkingFolderArguments() + GetFileTimeArgument() + SetAndGetCommonOptionalArguments());

			vault.GetSource(result);
			VerifyAll();
		}

		[Fact]
		public virtual void ArgumentsCorrectForGetSourceScenario10()
		{
			vault.Folder = "$";
			vault.AutoGetSource = true;

			vault.ApplyLabel = false;
			vault.UseVaultWorkingDirectory = true;
			vault.WorkingDirectory = DefaultWorkingDirectory;
			vault.CleanCopy = false;

			this.ProcessResultOutput =string.Empty;
			ExpectToNotCleanFolder();
			ExpectToExecuteArguments(@"get $" + GetWorkingFolderArguments() + GetFileTimeArgument() + SetAndGetCommonOptionalArguments());

			vault.GetSource(result);
			VerifyAll();
		}

		[Fact]
		public virtual void ArgumentsCorrectForGetSourceScenario11()
		{
			vault.Folder = "$";
			vault.AutoGetSource = true;

			vault.ApplyLabel = false;
			vault.UseVaultWorkingDirectory = true;
			vault.WorkingDirectory = @"";
			vault.CleanCopy = true;

			this.ProcessResultOutput = string.Format(listFolderOutputWithWorkingFolderSet, DefaultWorkingDirectory);
			ExpectToExecuteArguments(@"listworkingfolders" + SetAndGetCommonOptionalArguments());
			ExpectToExecuteArguments(@"get $" + GetWorkingFolderArguments() + GetFileTimeArgument() + SetAndGetCommonOptionalArguments());

			vault.GetSource(result);
			VerifyAll();
		}

		[Fact]
		public virtual void ArgumentsCorrectForGetSourceScenario12()
		{
			vault.Folder = "$";
			vault.AutoGetSource = true;

			vault.ApplyLabel = false;
			vault.UseVaultWorkingDirectory = true;
			vault.WorkingDirectory = @"";
			vault.CleanCopy = false;

			this.ProcessResultOutput =string.Empty;
			ExpectToExecuteArguments(@"get $" + GetWorkingFolderArguments() + GetFileTimeArgument() + SetAndGetCommonOptionalArguments());

			vault.GetSource(result);
			VerifyAll();
		}

		[Fact]
		public virtual void ArgumentsCorrectForGetSourceScenario13()
		{
			vault.Folder = "$";
			vault.AutoGetSource = true;

			vault.ApplyLabel = false;
			vault.UseVaultWorkingDirectory = false;
			vault.WorkingDirectory = DefaultWorkingDirectory;
			vault.CleanCopy = true;

			this.ProcessResultOutput =string.Empty;
			ExpectToCleanFolder();
			ExpectToExecuteArguments(@"get $ -destpath " + StringUtil.AutoDoubleQuoteString(DefaultWorkingDirectory) + GetWorkingFolderArguments() + GetFileTimeArgument() + SetAndGetCommonOptionalArguments());

			vault.GetSource(result);
			VerifyAll();
		}

		[Fact]
		public virtual void ArgumentsCorrectForGetSourceScenario14()
		{
			vault.Folder = "$";
			vault.AutoGetSource = true;

			vault.ApplyLabel = false;
			vault.UseVaultWorkingDirectory = false;
			vault.WorkingDirectory = DefaultWorkingDirectory;
			vault.CleanCopy = false;

			this.ProcessResultOutput =string.Empty;
			ExpectToNotCleanFolder();
			ExpectToExecuteArguments(@"get $ -destpath " + StringUtil.AutoDoubleQuoteString(DefaultWorkingDirectory) + GetWorkingFolderArguments() + GetFileTimeArgument() + SetAndGetCommonOptionalArguments());

			vault.GetSource(result);
			VerifyAll();
		}

		[Fact]
		public virtual void ArgumentsCorrectForGetSourceScenario15()
		{
			vault.Folder = "$";
			vault.AutoGetSource = true;

			vault.ApplyLabel = false;
			vault.UseVaultWorkingDirectory = false;
			vault.WorkingDirectory = @"";
			vault.CleanCopy = true;

			this.ProcessResultOutput = string.Format(listFolderOutputWithWorkingFolderSet, DefaultWorkingDirectory);
			ExpectToExecuteArguments(@"listworkingfolders" + SetAndGetCommonOptionalArguments());
			ExpectToExecuteArguments(@"get $ -destpath " + StringUtil.AutoDoubleQuoteString(DefaultWorkingDirectory) + GetWorkingFolderArguments() + GetFileTimeArgument() + SetAndGetCommonOptionalArguments());

			vault.GetSource(result);
			VerifyAll();
		}

		[Fact]
		public virtual void ArgumentsCorrectForGetSourceScenario16()
		{
			vault.Folder = "$";
			vault.AutoGetSource = true;

			vault.ApplyLabel = false;
			vault.UseVaultWorkingDirectory = false;
			vault.WorkingDirectory = @"";
			vault.CleanCopy = false;

			this.ProcessResultOutput = string.Format(listFolderOutputWithWorkingFolderSet, DefaultWorkingDirectory);
			ExpectToExecuteArguments(@"listworkingfolders" + SetAndGetCommonOptionalArguments());
			ExpectToExecuteArguments(@"get $ -destpath " + StringUtil.AutoDoubleQuoteString(DefaultWorkingDirectory) + GetWorkingFolderArguments() + GetFileTimeArgument() + SetAndGetCommonOptionalArguments());

			vault.GetSource(result);
			VerifyAll();
		}

		[Fact]
		public virtual void ShouldNotGetSourceIfNoWorkingFolderMatchesAndUseWorkingFolderTrueAndNoWorkingFolderSetInVault()
		{
			this.ProcessResultOutput = string.Format(listFolderOutputWithWorkingFolderSet, DefaultWorkingDirectory);
			ExpectToExecuteArguments(@"listworkingfolders");
			vault.AutoGetSource = true;
			vault.WorkingDirectory =string.Empty;
			vault.UseVaultWorkingDirectory = true;
			vault.Folder = @"$/noworkingfoldersetforme";
			vault.ApplyLabel = true;
			bool VaultExceptionThrown = false;
			try
			{
				vault.GetSource(result);
			}
			catch (Vault3.VaultException)
			{
				VaultExceptionThrown = true;
			}
			VerifyAll();
			Assert.True(VaultExceptionThrown, "Vault class did not throw expected exception.");
		}
        [Fact(Skip = "")]
        //[Test, Ignore("Ignored until i get right of the problem.")]
		public virtual void ShouldNotGetSourceIfNoWorkingFolderSpecifiedAndUseWorkingFolderTrueAndNoWorkingFolderSetInVault()
		{
			this.ProcessResultOutput = listFolderOutputWithNoWorkingFolderSet;
			ExpectToExecuteArguments(@"listworkingfolders", null);
			vault.AutoGetSource = true;
			vault.WorkingDirectory = null;
			vault.UseVaultWorkingDirectory = true;
			vault.Folder = @"$";
			vault.ApplyLabel = true;
			bool VaultExceptionThrown = false;
			try
			{
				vault.GetSource(result);
			}
			catch (Vault3.VaultException)
			{
				VaultExceptionThrown = true;
			}
			VerifyAll();
			Assert.True(VaultExceptionThrown, "Vault class did not throw expected exception.");
		}

		[Fact]
		public virtual void ShouldNotGetSourceIfAutoGetSourceIsFalse()
		{
			vault.AutoGetSource = false;
			vault.GetSource(IntegrationResultMother.CreateSuccessful());
			mockProcessExecutor.VerifyNoOtherCalls();
			VerifyAll();
		}

		[Fact]
		public virtual void ShouldNotApplyLabelOrGetByLabelIfApplyLabelIsFalse()
		{
			ExpectToExecuteArguments(@"get $ -performdeletions removeworkingcopy -merge overwrite -makewritable -setfiletime checkin");
			vault.ApplyLabel = false;
			vault.AutoGetSource = true;
			vault.UseVaultWorkingDirectory = true;
			vault.Folder = "$";
			vault.CleanCopy = false;
			vault.GetSource(result);
			VerifyAll();
		}

		[Fact]
		public virtual void ShouldBuildApplyLabelArgumentsCorrectlyNonAutoGet()
		{
			ExpectToExecuteArguments(@"label $ foo");
			vault.ApplyLabel = true;
			vault.AutoGetSource = false;
			vault.Folder = "$";
			vault.LabelSourceControl(result);
			VerifyAll();
		}

		[Fact]
		public virtual void ShouldSetAndRemoveLabelOnFailure()
		{
			ExpectToExecuteArguments(@"label $ foo");
			ExpectToExecuteArguments(@"getlabel $ foo -destpath " + StringUtil.AutoDoubleQuoteString(DefaultWorkingDirectory) + " -merge overwrite -makewritable -setfiletime checkin");
			ExpectToExecuteArguments(@"deletelabel $ foo");
			vault.ApplyLabel = true;
			vault.UseVaultWorkingDirectory = false;
			vault.WorkingDirectory = DefaultWorkingDirectory;
			vault.AutoGetSource = true;
			vault.Folder = "$";
			vault.GetSource(result);
			IntegrationResult failed = IntegrationResultMother.CreateFailed();
			failed.Label = result.Label;
			failed.WorkingDirectory = result.WorkingDirectory;
			vault.LabelSourceControl(failed);
			VerifyAll();
		}

		[Fact]
		public virtual void ShouldNotDeleteLabelIfItWasNeverApplied()
		{
			vault.ApplyLabel = true;
			vault.UseVaultWorkingDirectory = false;
			vault.WorkingDirectory = DefaultWorkingDirectory;
			vault.AutoGetSource = true;
			vault.Folder = "$";

			IntegrationResult failed = IntegrationResultMother.CreateFailed();
			failed.Label = result.Label;
			failed.WorkingDirectory = result.WorkingDirectory;
			vault.LabelSourceControl(failed);
			mockProcessExecutor.VerifyNoOtherCalls();
			VerifyAll();
		}

		[Fact]
		public virtual void ShouldBuildLabelArgumentsCorrectlyOnFailureNonAutoget()
		{
			ExpectToExecuteArguments(@"label $ foo");
			vault.ApplyLabel = true;
			vault.AutoGetSource = false;
			vault.Folder = "$";
			IntegrationResult failed = IntegrationResultMother.CreateFailed();
			failed.Label = result.Label;
			failed.WorkingDirectory = result.WorkingDirectory;
			vault.LabelSourceControl(failed);
			VerifyAll();
		}

		[Fact]
		public virtual void ShouldBuildApplyLabelArgumentsIncludingCommonArguments()
		{
			ExpectToExecuteArguments(@"label $ foo" + SetAndGetCommonOptionalArguments());
			vault.ApplyLabel = true;
			vault.AutoGetSource = false;
			vault.Folder = "$";
			vault.LabelSourceControl(result);
			VerifyAll();
		}

		[Fact]
		public virtual void ShouldStripNonXmlFromWorkingFolderList()
		{
			vault.Folder = "$";
			vault.AutoGetSource = true;

			vault.ApplyLabel = true;
			vault.UseVaultWorkingDirectory = true;
			vault.WorkingDirectory = null;
			vault.CleanCopy = false;

			this.ProcessResultOutput = string.Format(listFolderOutputWithNonXml, DefaultWorkingDirectory);
			ExpectToExecuteArguments(@"listworkingfolders" + SetAndGetCommonOptionalArguments());
			ExpectToExecuteArguments(@"label $ foo" + SetAndGetCommonOptionalArguments());
			ExpectToExecuteArguments(@"getlabel $ foo -labelworkingfolder " + StringUtil.AutoDoubleQuoteString(DefaultWorkingDirectory)
				+ GetWorkingFolderArguments() + GetFileTimeArgument() + SetAndGetCommonOptionalArguments());

			vault.GetSource(result);
			VerifyAll();
		}

		protected string SetAndGetCommonOptionalArguments()
		{
			vault.Host = "localhost";
			vault.Username = "username";
			vault.Password = "password";
			vault.Repository = "my repository";
			vault.Ssl = true;
			return @" -host localhost -user username -password password -repository ""my repository"" -ssl";
		}

		protected string GetWorkingFolderArguments()
		{
			if (vault.ApplyLabel)
				return @" -merge overwrite -makewritable";
			else
			{
				if (vault.UseVaultWorkingDirectory)
					return @" -performdeletions removeworkingcopy -merge overwrite -makewritable";
				else
					return @" -merge overwrite -makewritable";
			}
		}

		protected string GetFileTimeArgument()
		{
			return @" -setfiletime " + vault.setFileTime;
		}

		protected VaultVersionChecker CreateVault(string xml)
		{
			return (VaultVersionChecker) NetReflector.Read(xml);
		}

		protected void ExpectToParseHistory()
		{
			mockHistoryParser.Setup(parser => parser.Parse(It.IsAny<TextReader>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(new Modification[0]).Verifiable();
		}

		/* 
		* CleanFolder tests commented out because tests that are tied to a
		* particular file system layout are undesirable, per Owen 11/29/05
		*
		private void CreateTempFileForCleanFolderTest()
		{
			tempFileToTestCleanCopy = Path.GetTempFileName();
			string tempFileNameOnly = Path.GetFileName(tempFileToTestCleanCopy);
			File.Move(tempFileToTestCleanCopy, Path.Combine(vault.WorkingDirectory, tempFileNameOnly));
			tempFileToTestCleanCopy = Path.Combine(vault.WorkingDirectory, tempFileNameOnly);
		}
		*/

		protected void ExpectToCleanFolder()
		{
			/* 
			 * CleanFolder tests commented out because tests that are tied to a
			 * particular file system layout are undesirable, per Owen 11/29/05
			 * 

			// can't test if we don't know the working folder ahead of time
			if ( StringUtil.IsBlank(vault.WorkingDirectory) )
			{
				Assert.Fail("\"ExpectToCleanFolder\" can only be used when the working directory is specified.");
			}
			CreateTempFileForCleanFolderTest();
			this.expectCleanCopy = true;
			*/
		}

		protected void ExpectToNotCleanFolder()
		{
			/* 
			 * CleanFolder tests commented out because tests that are tied to a
			 * particular file system layout are undesirable, per Owen 11/29/05
			 *

			// can't test if we don't know the working folder ahead of time
			if ( StringUtil.IsBlank(vault.WorkingDirectory) )
			{
				Assert.Fail("\"ExpectNotToCleanFolder\" can only be used when the working directory is specified.");
			}
			CreateTempFileForCleanFolderTest();
			this.expectCleanCopy = false;
			*/
		}

		protected void VerifyAll()
		{
			/* 
			 * CleanFolder tests commented out because tests that are tied to a
			 * particular file system layout are undesirable, per Owen 11/29/05
			 *
			  
			if ( !StringUtil.IsBlank(tempFileToTestCleanCopy) )
			{
				if ( this.expectCleanCopy )
					Assert.False( File.Exists(tempFileToTestCleanCopy), "The working directory should have been cleaned, but was not." );
				else
					Assert.True( File.Exists(tempFileToTestCleanCopy), "The working directory should not have been cleaned, but it was." );
				File.Delete(tempFileToTestCleanCopy);
				tempFileToTestCleanCopy = null;
			}
			*/
			Verify();
			mockHistoryParser.Verify();
		}
	}
}
