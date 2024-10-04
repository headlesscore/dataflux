using System;
using System.Collections.Generic;
using System.IO;
using Exortech.NetReflector;
using FluentAssertions;
using Moq;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.Remote;
using Xunit;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{

    public class GitTest : ProcessExecutorTestFixtureBase
    {
        const string GIT_CLONE = "clone xyz.git";
        const string GIT_FETCH = "fetch origin";
        const string GIT_REMOTE_HASH = "log origin/master -1 --pretty=format:\"%H\"";
        const string GIT_LOCAL_HASH = "log -1 --pretty=format:\"%H\"";
        const string GIT_COMMIT_KEY = "commit";
        const string FROM_COMMIT = "0123456789abcdef";
        const string TO_COMMIT = "fedcba9876543210";
        const string GIT_LOG_OPTIONS = "-100 --name-status --pretty=format:\"Commit:%H%nTime:%ci%nAuthor:%an%nE-Mail:%ae%nMessage:%s%n%n%b%nChanges:\" -m";
        const string GIT_LOG_REMOTE_COMMITS = "log " + FROM_COMMIT + "..origin/master " + GIT_LOG_OPTIONS;
        const string GIT_LOG_ALL = "log origin/master " + GIT_LOG_OPTIONS;

        private Git git;
        private Mock<IHistoryParser> mockHistoryParser;
        private Mock<IFileSystem> mockFileSystem;
        private Mock<IFileDirectoryDeleter> mockFileDirectoryDeleter;

        // [SetUp]
        protected void CreateGit()
        {
            mockHistoryParser = new Mock<IHistoryParser>();
            mockFileSystem = new Mock<IFileSystem>();
            mockFileDirectoryDeleter = new Mock<IFileDirectoryDeleter>();
            CreateProcessExecutorMock("git");

            SetupGit((IFileSystem)mockFileSystem.Object, (IFileDirectoryDeleter)mockFileDirectoryDeleter.Object);
        }

        // [TearDown]
        protected void VerifyAll()
        {
            Verify();
            mockHistoryParser.Verify();
            mockFileSystem.Verify();
        }

        [Fact]
        public void GitShouldBeDefaultExecutable()
        {
            git.Executable.Should().Be("git");

        }

        [Fact]
        public void PopulateFromFullySpecifiedXml()
        {
            const string xml = @"
<git>
	<executable>git</executable>
	<repository>c:\git\ccnet\mygitrepo</repository>
	<branch>master</branch>
	<timeout>5</timeout>
	<workingDirectory>c:\git\working</workingDirectory>
	<tagOnSuccess>true</tagOnSuccess>
	<commitBuildModifications>true</commitBuildModifications>
	<commitUntrackedFiles>true</commitUntrackedFiles>
    <maxAmountOfModificationsToFetch>500</maxAmountOfModificationsToFetch>
	<autoGetSource>true</autoGetSource>
	<tagCommitMessage>CCNet Test Build {0}</tagCommitMessage>
	<tagNameFormat>{0}</tagNameFormat>
	<committerName>Max Mustermann</committerName>
	<committerEMail>max.mustermann@gmx.de</committerEMail>
</git>";

            git = (Git)NetReflector.Read(xml);
            git.Executable.Should().Be("git");
            Assert.Equal(@"c:\git\ccnet\mygitrepo", git.Repository);
            Assert.Equal("master", git.Branch);
            Assert.Equal(new Timeout(5), git.Timeout);
            Assert.Equal(@"c:\git\working", git.WorkingDirectory);
            Assert.True(git.TagOnSuccess);
            Assert.True(git.AutoGetSource);
            Assert.Equal("CCNet Test Build {0}", git.TagCommitMessage);
            Assert.Equal("{0}", git.TagNameFormat);
            Assert.Equal("Max Mustermann", git.CommitterName);
            Assert.Equal("max.mustermann@gmx.de", git.CommitterEMail);
            Assert.True(git.CommitBuildModifications);
            Assert.True(git.CommitUntrackedFiles);
            Assert.Equal(500, git.MaxAmountOfModificationsToFetch);

        }

        [Fact]
        public void PopulateFromMinimallySpecifiedXml()
        {
            const string xml = @"
<git>
    <repository>c:\git\ccnet\mygitrepo</repository>
</git>";
            git = (Git)NetReflector.Read(xml);
            git.Executable.Should().Be(@"git");
            git.Repository.Should().Be(@"c:\git\ccnet\mygitrepo");
            git.Branch.Should().Be(@"master");
            Assert.Equal(new Timeout(600000), git.Timeout);
            git.WorkingDirectory.Should().Be(null);
            Assert.False(git.TagOnSuccess);
            Assert.True(git.AutoGetSource);
            git.TagCommitMessage.Should().Be("CCNet Build {0}");
            git.TagNameFormat.Should().Be("CCNet-Build-{0}");
            git.CommitterName.Should().Be(null);
            git.CommitterEMail.Should().Be(null);
            Assert.False(git.CommitBuildModifications);
            Assert.False(git.CommitUntrackedFiles);
            Assert.Equal(100, git.MaxAmountOfModificationsToFetch);
        }

        [Fact]
        public void ShouldApplyLabelIfTagOnSuccessTrue()
        {
            git.TagOnSuccess = true;

            ExpectToExecuteArguments(@"tag -a -m ""CCNet Build foo"" CCNet-Build-foo");
            ExpectToExecuteArguments(@"push origin tag CCNet-Build-foo");

            git.LabelSourceControl(IntegrationResultMother.CreateSuccessful("foo"));
        }

        [Fact]
        public void ShouldCommitBuildModificationsAndApplyLabelIfCommitBuildModificationsAndTagOnSuccessIsTrue()
        {
            git.TagOnSuccess = true;
            git.CommitBuildModifications = true;

            ExpectToExecuteArguments(@"commit --all --allow-empty -m ""CCNet Build foo""");
            ExpectToExecuteArguments(@"tag -a -m ""CCNet Build foo"" CCNet-Build-foo");
            ExpectToExecuteArguments(@"push origin tag CCNet-Build-foo");

            git.LabelSourceControl(IntegrationResultMother.CreateSuccessful("foo"));
        }

        [Fact]
        public void ShouldAddAndCommitBuildModificationsAndApplyLabelIfCommitUntrackedFilesAndCommitBuildModificationsAndTagOnSuccessIsTrue()
        {
            git.TagOnSuccess = true;
            git.CommitBuildModifications = true;
            git.CommitUntrackedFiles = true;

            ExpectToExecuteArguments(@"add --all");
            ExpectToExecuteArguments(@"commit --all --allow-empty -m ""CCNet Build foo""");
            ExpectToExecuteArguments(@"tag -a -m ""CCNet Build foo"" CCNet-Build-foo");
            ExpectToExecuteArguments(@"push origin tag CCNet-Build-foo");

            git.LabelSourceControl(IntegrationResultMother.CreateSuccessful("foo"));
        }

        [Fact]
        public void ShouldApplyLabelIfTagOnSuccessTrueAndNotAddFilesIfCommitBuildModificationsIsFalseAndCommitUntrackedFilesIsTrue()
        {
            git.TagOnSuccess = true;
            git.CommitBuildModifications = false;
            git.CommitUntrackedFiles = true;

            ExpectToExecuteArguments(@"tag -a -m ""CCNet Build foo"" CCNet-Build-foo");
            ExpectToExecuteArguments(@"push origin tag CCNet-Build-foo");

            git.LabelSourceControl(IntegrationResultMother.CreateSuccessful("foo"));
        }

        [Fact]
        public void ShouldApplyLabelWithCustomMessageIfTagOnSuccessTrueAndACustomMessageIsSpecified()
        {
            git.TagOnSuccess = true;
            git.TagCommitMessage = "a---- {0} ----a";

            ExpectToExecuteArguments(@"tag -a -m ""a---- foo ----a"" CCNet-Build-foo");
            ExpectToExecuteArguments(@"push origin tag CCNet-Build-foo");

            git.LabelSourceControl(IntegrationResultMother.CreateSuccessful("foo"));
        }

        [Fact]
        public void ShouldApplyTagNameFormatWithCustomFormatIfTagOnSuccessTrueAndATagNameFormatIsSpecified()
        {
            git.TagOnSuccess = true;
            git.TagNameFormat = "Build/{0}";

            ExpectToExecuteArguments(@"tag -a -m ""CCNet Build foo"" Build/foo");
            ExpectToExecuteArguments(@"push origin tag Build/foo");

            git.LabelSourceControl(IntegrationResultMother.CreateSuccessful("foo"));
        }

        [Fact]
        public void ShouldApplyTagNameFormatWithJustBuildLabelAsCustomFormatTagOnSuccessTrue()
        {
            git.TagOnSuccess = true;
            git.TagNameFormat = "{0}";
            git.TagCommitMessage = "{0}";

            ExpectToExecuteArguments(@"tag -a -m foo foo");
            ExpectToExecuteArguments(@"push origin tag foo");

            git.LabelSourceControl(IntegrationResultMother.CreateSuccessful("foo"));
        }



        [Fact]
        public void ShouldCloneIfDirectoryDoesNotExist()
        {
            mockFileSystem.Setup(fileSystem => fileSystem.DirectoryExists(DefaultWorkingDirectory)).Returns(false).Verifiable();
            ExpectCloneAndInitialiseRepository();

            ExpectToExecuteArguments(GIT_LOG_REMOTE_COMMITS);
            ExpectLogRemoteHead(TO_COMMIT);
            mockHistoryParser.Setup(parser => parser.Parse(It.IsAny<TextReader>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(new Modification[] { }).Verifiable();

            IIntegrationResult to = IntegrationResult();
            git.GetModifications(IntegrationResult(FROM_COMMIT), to);

            AssertIntegrationResultTaggedWithCommit(to, TO_COMMIT);
        }

        [Fact]
        public void ShouldCloneAndDeleteWorkingDirIfGitDirectoryDoesNotExist()
        {
            MockSequence sequence = new MockSequence();
            mockFileSystem.InSequence(sequence).Setup(fileSystem => fileSystem.DirectoryExists(DefaultWorkingDirectory)).Returns(true).Verifiable();
            mockFileSystem.Setup(fileSystem => fileSystem.DirectoryExists(Path.Combine(DefaultWorkingDirectory, ".git"))).Returns(false).Verifiable();
            mockFileDirectoryDeleter.Setup(deleter => deleter.DeleteIncludingReadOnlyObjects(DefaultWorkingDirectory)).Verifiable();
            mockFileSystem.InSequence(sequence).Setup(fileSystem => fileSystem.DirectoryExists(DefaultWorkingDirectory)).Returns(false).Verifiable();
            ExpectCloneAndInitialiseRepository();

            ExpectToExecuteArguments(GIT_LOG_REMOTE_COMMITS);
            ExpectLogRemoteHead(TO_COMMIT);
            mockHistoryParser.Setup(parser => parser.Parse(It.IsAny<TextReader>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(new Modification[] { }).Verifiable();

            IIntegrationResult to = IntegrationResult();
            git.GetModifications(IntegrationResult(FROM_COMMIT), to);

            AssertIntegrationResultTaggedWithCommit(to, TO_COMMIT);
        }

        [Fact]
        public void ShouldLogWholeHistoryIfCommitNotPresentInFromIntegrationResult()
        {
            mockFileSystem.Setup(fileSystem => fileSystem.DirectoryExists(DefaultWorkingDirectory)).Returns(true).Verifiable();
            mockFileSystem.Setup(fileSystem => fileSystem.DirectoryExists(Path.Combine(DefaultWorkingDirectory, ".git"))).Returns(true).Verifiable();

            ExpectToExecuteArguments(GIT_FETCH);
            ExpectToExecuteArguments(GIT_LOG_ALL);
            ExpectLogRemoteHead(TO_COMMIT);

            IIntegrationResult to = IntegrationResult();
            git.GetModifications(IntegrationResult(), to);

            AssertIntegrationResultTaggedWithCommit(to, TO_COMMIT);
        }

        private void ExpectToExecuteWithArgumentsAndReturn(string args, ProcessResult returnValue)
        {
            var processInfo = NewProcessInfo(args, DefaultWorkingDirectory);
            processInfo.StandardInputContent = "";
            mockProcessExecutor.Setup(executor => executor.Execute(processInfo)).Returns(returnValue).Verifiable();
        }

        private new void ExpectToExecuteArguments(string args)
        {
            ExpectToExecuteArguments(args, DefaultWorkingDirectory);
        }

        protected new void ExpectToExecuteArguments(string args, string workingDirectory)
        {
            ProcessInfo processInfo = NewProcessInfo(args, workingDirectory);
            processInfo.StandardInputContent = "";
            ExpectToExecute(processInfo);
        }

        [Fact]
        public void ShouldGetSourceIfModificationsFound()
        {
            git.AutoGetSource = true;

            ExpectToExecuteArguments("checkout -q -f origin/master");
            ExpectToExecuteArguments("clean -d -f -x");

            git.GetSource(IntegrationResult());
        }

        [Fact]
        public void ShouldNotApplyLabelIfIntegrationFailed()
        {
            git.TagOnSuccess = true;

            git.LabelSourceControl(IntegrationResultMother.CreateFailed());

            mockProcessExecutor.VerifyNoOtherCalls();
        }

        [Fact]
        public void ShouldNotApplyLabelIfTagOnSuccessFalse()
        {
            git.TagOnSuccess = false;

            git.LabelSourceControl(IntegrationResultMother.CreateSuccessful());

            mockProcessExecutor.VerifyNoOtherCalls();
        }

        [Fact]
        public void ShouldNotGetSourceIfAutoGetSourceFalse()
        {
            git.AutoGetSource = false;

            git.GetSource(IntegrationResult());

            mockProcessExecutor.VerifyNoOtherCalls();
        }

        [Fact]
        public void ShouldReturnModificationsWhenHashsDifferent()
        {
            mockFileSystem.Setup(fileSystem => fileSystem.DirectoryExists(DefaultWorkingDirectory)).Returns(true).Verifiable();
            mockFileSystem.Setup(fileSystem => fileSystem.DirectoryExists(Path.Combine(DefaultWorkingDirectory, ".git"))).Returns(true).Verifiable();

            Modification[] modifications = new Modification[2] { new Modification(), new Modification() };

            ExpectToExecuteArguments(GIT_FETCH);
            ExpectToExecuteArguments(GIT_LOG_REMOTE_COMMITS);
            ExpectLogRemoteHead(TO_COMMIT);

            mockHistoryParser.Setup(parser => parser.Parse(It.IsAny<TextReader>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(modifications).Verifiable();

            IIntegrationResult to = IntegrationResult();
            Modification[] result = git.GetModifications(IntegrationResult(FROM_COMMIT), to);

            Assert.Equal(modifications, result);
            AssertIntegrationResultTaggedWithCommit(to, TO_COMMIT);
        }

        private void SetupGit(IFileSystem filesystem, IFileDirectoryDeleter fileDirectoryDeleter)
        {
            git = new Git((IHistoryParser)mockHistoryParser.Object, (ProcessExecutor)mockProcessExecutor.Object, filesystem, fileDirectoryDeleter);
            git.Repository = @"xyz.git";
            git.WorkingDirectory = DefaultWorkingDirectory;
        }

        private IIntegrationResult IntegrationResult(string commit)
        {
            IntegrationResult r = new IntegrationResult();
            r.SourceControlData.Add(new NameValuePair(GIT_COMMIT_KEY, commit));
            return r;
        }

        /// <summary>
        /// Sets an expectation that git will call 'log' to get the remote head commit, printing the value of
        /// <paramref name="commit"/> to stdout.
        /// </summary>
        /// <param name="commit"></param>
        private void ExpectLogRemoteHead(string commit)
        {
            ExpectToExecuteWithArgumentsAndReturn(GIT_REMOTE_HASH, new ProcessResult(commit, "", 0, false));
        }

        private void ExpectCloneAndInitialiseRepository()
        {
            ExpectToExecuteArguments(string.Concat(GIT_CLONE, " ", StringUtil.AutoDoubleQuoteString(DefaultWorkingDirectory)), Path.GetDirectoryName(DefaultWorkingDirectory.TrimEnd(Path.DirectorySeparatorChar)));
            ExpectToExecuteArguments("config --get user.name");
            ExpectToExecuteArguments("config --get user.email");
        }

        private void AssertIntegrationResultTaggedWithCommit(IIntegrationResult result, string commit)
        {
            Dictionary<string, string> data = NameValuePair.ToDictionary(result.SourceControlData);
            Assert.True(data.ContainsKey(GIT_COMMIT_KEY), "IntegrationResult.SourceControlData did not contain commit info.");
            Assert.True(data[GIT_COMMIT_KEY] == commit);
        }
    }
}
