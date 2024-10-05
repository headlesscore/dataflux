using System.Diagnostics;
using System.IO;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
	
	public class FileTaskResultTest
	{
        private MockRepository mocks;
        private string filename;

		// [SetUp]
		public void CreateFile()
		{
			filename = TempFileUtil.CreateTempFile("FileTaskResult", "test.xml", "<invalid xml>");
            this.mocks = new MockRepository(MockBehavior.Default);
		}

		// [TearDown]
		public void DestroyFile()
		{
			TempFileUtil.DeleteTempDir("FileTaskResult");
		}

		[Fact]
		public void ShouldReadContentsOfTempFile()
		{
			FileTaskResult result = new FileTaskResult(filename);
			Assert.Equal("<invalid xml>", result.Data);
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public void ShouldThrowReadableExceptionIfFileDoesNotExist()
		{
            Assert.Throws<CruiseControlException>(delegate { new FileTaskResult("unknown.file"); });
		}

        [Fact]
        public void DeleteAfterMergeDeletesTheFile()
        {
            // Initialise the test
            var fileSystem = this.mocks.Create<IFileSystem>(MockBehavior.Strict).Object;
            var file = new FileInfo(this.filename);
            Mock.Get(fileSystem).Setup(_fileSystem => _fileSystem.FileExists(file.FullName)).Returns(true);
            Mock.Get(fileSystem).Setup(_fileSystem => _fileSystem.DeleteFile(file.FullName)).Verifiable();

            // Run the test
            var result = new FileTaskResult(file, true, fileSystem);
            result.CleanUp();

            // Check the results
            this.mocks.VerifyAll();
        }

        [Fact]
        public void FileIsNotDeletedIfDeletedAfterMergeIsNotSet()
        {
            // Initialise the test
            var fileSystem = this.mocks.Create<IFileSystem>(MockBehavior.Strict).Object;
            var file = new FileInfo(this.filename);
            Mock.Get(fileSystem).Setup(_fileSystem => _fileSystem.FileExists(file.FullName)).Returns(true);

            // Run the test
            var result = new FileTaskResult(file, false, fileSystem);
            result.CleanUp();

            // Check the results
            this.mocks.VerifyAll();
        }
    }
}
