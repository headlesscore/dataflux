using System.Linq;
using System.IO;
using Xunit;
using ThoughtWorks.CruiseControl.Core.Util;
using System;


namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
    
    public class SystemIoFileSystemTest
    {
        private SystemPath tempRoot;
        private SystemPath tempSubRoot;
        private SystemPath tempOtherRoot;

        // [SetUp]
        public void Setup()
        {
            tempRoot = SystemPath.UniqueTempPath().CreateDirectory();
            tempSubRoot = tempRoot.CreateSubDirectory("subrepo");
            tempOtherRoot = SystemPath.UniqueTempPath().CreateDirectory();
        }

        // [TearDown]
        public void TearDown()
        {
            tempRoot.DeleteDirectory();
            tempOtherRoot.DeleteDirectory();
        }

        [Fact]
        public void ShouldCopyFileToDirectory()
        {
            SystemPath file1 = tempRoot.CreateEmptyFile("File1");
            new SystemIoFileSystem().Copy(file1.ToString(), tempSubRoot.ToString());
            Assert.True(tempSubRoot.Combine("File1").Exists());
            Assert.True(true);
            Assert.True(true);
        }

        [Fact]
        public void ShouldCopyFileToFile()
        {
            SystemPath sourceFile = tempRoot.CreateEmptyFile("File1");
            SystemPath targetFile = tempSubRoot.Combine("File2");
            new SystemIoFileSystem().Copy(sourceFile.ToString(), targetFile.ToString());
            Assert.True(targetFile.Exists());
        }

        [Fact]
        public void ShouldAllowOverwrites()
        {
            SystemPath sourceFile = tempRoot.CreateEmptyFile("File1");
            SystemPath targetFile = tempSubRoot.CreateEmptyFile("File2");
            new SystemIoFileSystem().Copy(sourceFile.ToString(), targetFile.ToString());
            Assert.True(targetFile.Exists());
        }

        [Fact]
        public void ShouldAllowOverwritesEvenWhenDestinationHasReadOnlyAttributeSet()
        {
            SystemPath sourceFile = tempRoot.CreateEmptyFile("File1");
            SystemPath targetFile = tempSubRoot.CreateEmptyFile("File2");
            File.SetAttributes(targetFile.ToString(), FileAttributes.ReadOnly);
            new SystemIoFileSystem().Copy(sourceFile.ToString(), targetFile.ToString());

            Assert.True(targetFile.Exists());
        }

        [Fact]
        public void ShouldCopyDirectoryToDirectoryRecursively()
        {
            tempRoot.CreateEmptyFile("File1");
            tempSubRoot.CreateEmptyFile("File2");
            new SystemIoFileSystem().Copy(tempRoot.ToString(), tempOtherRoot.ToString());

            Assert.True(tempOtherRoot.Combine("File1").Exists());
            Assert.True(tempOtherRoot.Combine("subrepo").Combine("File2").Exists());
        }

        [Fact]
        public void ShouldSaveToFile()
        {
            SystemPath tempFile = tempRoot.Combine("foo.txt");
            Assert.False(tempFile.Exists());
            new SystemIoFileSystem().Save(tempFile.ToString(), "bar");
            Assert.True(tempFile.Exists());
            using (StreamReader reader = File.OpenText(tempFile.ToString()))
            {
                Assert.Equal("bar", reader.ReadToEnd());
            }
        }

        [Fact]
        public void ShouldSaveUnicodeToFile()
        {
            SystemPath tempFile = tempRoot.Combine("foo.txt");
            Assert.False(tempFile.Exists());
            new SystemIoFileSystem().Save(tempFile.ToString(), "hi there? håkan! \u307b");
            Assert.True(tempFile.Exists());
            using (StreamReader reader = File.OpenText(tempFile.ToString()))
            {
                Assert.Equal("hi there? håkan! \u307b", reader.ReadToEnd());
            }
        }

        [Fact]
        public void ShouldSaveToFileAtomically()
        {
            SystemPath tempFile = tempRoot.Combine("foo.txt");
            Assert.False(tempFile.Exists());
            new SystemIoFileSystem().AtomicSave(tempFile.ToString(), "bar");
            Assert.True(tempFile.Exists());
            using (StreamReader reader = File.OpenText(tempFile.ToString()))
            {
                Assert.Equal("bar", reader.ReadToEnd());
            }
            new SystemIoFileSystem().AtomicSave(tempFile.ToString(), "baz");
            Assert.True(tempFile.Exists());
            using (StreamReader reader = File.OpenText(tempFile.ToString()))
            {
                Assert.Equal("baz", reader.ReadToEnd());
            }
        }

        [Fact]
        public void ShouldSaveUnicodeToFileAtomically()
        {
            SystemPath tempFile = tempRoot.Combine("foo.txt");
            Assert.False(tempFile.Exists());
            new SystemIoFileSystem().AtomicSave(tempFile.ToString(), "hi there? håkan! \u307b");
            Assert.True(tempFile.Exists());
            using (StreamReader reader = File.OpenText(tempFile.ToString()))
            {
                Assert.Equal("hi there? håkan! \u307b", reader.ReadToEnd());
            }
            new SystemIoFileSystem().AtomicSave(tempFile.ToString(), "hi there? håkan! \u307b sadfasdf");
            Assert.True(tempFile.Exists());
            using (StreamReader reader = File.OpenText(tempFile.ToString()))
            {
                Assert.Equal("hi there? håkan! \u307b sadfasdf", reader.ReadToEnd());
            }
        }

        [Fact]
        public void LoadReadsFileContentCorrectly()
        {
            SystemPath tempFile = tempRoot.CreateTextFile("foo.txt", "bar");
            Assert.Equal("bar", new SystemIoFileSystem().Load(tempFile.ToString()).ReadToEnd());
        }

        [Fact]
        public void GetFilesInDirectoryListsAllFiles()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                Directory.CreateDirectory(tempPath);
                var files = new[] 
                    {
                        Path.Combine(tempPath, "file1.txt"),
                        Path.Combine(tempPath, "file2.txt"),
                        Path.Combine(tempPath, "file3.txt")
                    };
                foreach (var file in files)
                {
                    File.WriteAllText(file, "Some data");
                }
                var results = new SystemIoFileSystem().GetFilesInDirectory(tempPath);
                Array.Sort(results);
                Assert.Equal(files, results);
            }
            finally
            {
                // Clean up
                if (Directory.Exists(tempPath))
                {
                    Directory.Delete(tempPath, true);
                }
            }
        }


        [Fact]
        public void GetFilesInDirectoryExcludesSubDirectories()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                Directory.CreateDirectory(tempPath);
                var files = new[] 
                    {
                        Path.Combine(tempPath, "file1.txt"),
                        Path.Combine(tempPath, "file2.txt"),
                        Path.Combine(tempPath, "file3.txt")
                    };
                foreach (var file in files)
                {
                    File.WriteAllText(file, "Some data");
                }

                var subDir = Path.Combine(tempPath, "subDir");
                Directory.CreateDirectory(subDir);
                File.WriteAllText(Path.Combine(subDir, "file4.txt"), "Some data");

                var results = new SystemIoFileSystem().GetFilesInDirectory(tempPath, false);
                Array.Sort(results);
                CollectionAssert.AreEqual(files, results);
            }
            finally
            {
                // Clean up
                if (Directory.Exists(tempPath))
                {
                    Directory.Delete(tempPath, true);
                }
            }
        }

        [Fact]
        public void GetFilesInDirectoryIncludesSubDirectories()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                Directory.CreateDirectory(tempPath);
                var files = new[] 
                    {
                        Path.Combine(tempPath, "file1.txt"),
                        Path.Combine(tempPath, "file2.txt"),
                        Path.Combine(tempPath, "file3.txt")
                    };
                foreach (var file in files)
                {
                    File.WriteAllText(file, "Some data");
                }

                var subDir = Path.Combine(tempPath, "subDir");
                Directory.CreateDirectory(subDir);
                var subFile = Path.Combine(subDir, "file4.txt");
                File.WriteAllText(subFile, "Some data");

                var results = new SystemIoFileSystem().GetFilesInDirectory(tempPath, true);
                Array.Sort(results);
                CollectionAssert.AreEqual(files.Concat(new[] { subFile}), results);
            }
            finally
            {
                // Clean up
                if (Directory.Exists(tempPath))
                {
                    Directory.Delete(tempPath, true);
                }
            }
        }
    }
}
