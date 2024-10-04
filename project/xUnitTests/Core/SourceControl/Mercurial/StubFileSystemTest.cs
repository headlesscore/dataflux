namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol.Mercurial
{
	using Xunit;
    
    using System;
    using System.IO;
	using System.Text;

	/// <summary>
	/// Coverage test for <see cref="StubFileSystem"/>.
	/// </summary>
	
	public class StubFileSystemTest
	{

		#region Private Members

		private StubFileSystem sf;

		#endregion

		#region SetUp Method

		// [SetUp]
		public void SetUp()
		{
			sf = new StubFileSystem();
		}

		#endregion

		#region Tests

		[Fact]
		public void AtomicSaveCoverage()
		{
			sf.AtomicSave("asdf", "asdf");
		}

		[Fact]
		public void AtomicSaveCoverage2()
		{
			sf.AtomicSave("asdf", "asdf", Encoding.Unicode);
		}

		[Fact]
		public void CopyCoverage()
		{
			sf.Copy("asdf", "asdf");
		}

		[Fact]
		public void CreateDirectoryCoverage()
		{
			sf.CreateDirectory("asdf");
		}

		[Fact]
		public void DeleteDirectoryCoverage()
		{
			sf.DeleteDirectory("asdsf");
		}

		[Fact]
		public void DeleteDirectoryCoverage2()
		{
			sf.DeleteDirectory("asdf", true);
		}

		[Fact]
		public void DirectoryExistsCoverage()
		{
			sf.DirectoryExists("asdf");
		}

		[Fact]
		public void DeleteFileCoverage()
		{
			sf.DeleteFile("asdf");
		}


		[Fact]
		public void EnsureFolderExistsCoverage()
		{
			sf.EnsureFolderExists("asdf");
		}

		[Fact]
		public void EnsureFileExistsShouldThrowException()
		{
			Assert.True(delegate { sf.EnsureFileExists("asdf"); },
			            Throws.TypeOf<NotImplementedException>());
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public void FileExistsCoverage()
		{
			sf.FileExists("asdf");
		}

		[Fact]
		public void GenerateTaskResultFromFileCoverage()
		{
			sf.GenerateTaskResultFromFile("asdf");
		}

		[Fact]
		public void GenerateTaskResultFromFileCoverage2()
		{
			sf.GenerateTaskResultFromFile("asdf", true);
		}

		[Fact]
		public void GetFilesInDirectoryCoverage()
		{
			sf.GetFilesInDirectory("asdf");
		}

		[Fact]
		public void GetFilesInDirectoryCoverage2()
		{
			sf.GetFilesInDirectory("asdf", true);
		}

		[Fact]
		public void GetFilesInDirectoryShouldThrowException()
		{
			Assert.True(delegate {sf.GetFilesInDirectory("asdf", "asdf", SearchOption.AllDirectories); },
			            Throws.TypeOf<NotImplementedException>());
		}

		[Fact]
		public void GetFileLengthShouldThrowException()
		{
			Assert.True(delegate { sf.GetFileLength("asdf"); },
			            Throws.TypeOf<NotImplementedException>());
		}

		[Fact]
		public void GetFileVersionShouldThrowException()
		{
			Assert.True(delegate {sf.GetFileVersion("asdf"); },
			            Throws.TypeOf<NotImplementedException>());
		}

		[Fact]
		public void GetFreeDiskSpaceCoverage()
		{
			sf.GetFreeDiskSpace("asdf");
		}

		[Fact]
		public void GetLastWriteTimeCoverage()
		{
			sf.GetLastWriteTime("asdf");
		}

		[Fact]
		public void LoadCoverage()
		{
			sf.Load("asdf");
		}

		[Fact]
		public void OpenInputStreamCoverage()
		{
			sf.OpenInputStream("asdf");
		}

		[Fact]
		public void OpenOutputStreamCoverage()
		{
			sf.OpenOutputStream("asdf");
		}

		[Fact]
		public void SaveCoverage()
		{
			sf.Save("asdf", "Asdf");
		}

		#endregion
	}
}
