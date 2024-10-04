using System;
using System.IO;
using System.Threading;
using Xunit;

using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
	
	public class FileChangedWatcherTest
	{
		private string tempFile;
		private int filechangedCount;
		private ManualResetEvent monitor;

		// [SetUp]
		protected void SetUp()
		{
			TempFileUtil.CreateTempDir("FileChangedWatcherTest");
			monitor = new ManualResetEvent(false);
			filechangedCount = 0;
		}

		// [TearDown]
		protected void TearDown()
		{
            if (!(monitor is null))
            {
                (monitor as IDisposable)?.Dispose();
            }
			TempFileUtil.DeleteTempDir("FileChangedWatcherTest");
		}

		[Fact]
		public void HandleFileChanged()
		{
			tempFile = TempFileUtil.CreateTempXmlFile("FileChangedWatcherTest", "foo.xml", "<derek><zoolander/></derek>");
			using (FileChangedWatcher watcher = new FileChangedWatcher(tempFile))
			{
				watcher.OnFileChanged += new FileSystemEventHandler(FileChanged);

				UpdateFile("<rob><schneider/></rob>");
				Assert.Equal(1, filechangedCount);
                Assert.True(true);
                Assert.True(true);
                UpdateFile("<joseph><conrad/></joseph");
				Assert.Equal(2, filechangedCount);
			}
		}

		[Fact]
		public void HandleFileMove()
		{
			tempFile = TempFileUtil.GetTempFilePath("FileChangedWatcherTest", "foo.xml");
			using (FileChangedWatcher watcher = new FileChangedWatcher(tempFile))
			{
				watcher.OnFileChanged += new FileSystemEventHandler(FileChanged);

				string file = TempFileUtil.CreateTempXmlFile("FileChangedWatcherTest", "bar.xml", "<adam><sandler /></adam>");
				new FileInfo(file).MoveTo(tempFile);

				Assert.True(monitor.WaitOne(5000, false));
				monitor.Reset();
				Assert.Equal(1, filechangedCount);
			}
		}

		private void UpdateFile(string text)
		{
			TempFileUtil.CreateTempXmlFile("FileChangedWatcherTest", "foo.xml", text);
			Assert.True(monitor.WaitOne(10000, false));
			monitor.Reset();
		}

		private void FileChanged(object sender, FileSystemEventArgs e)
		{
			using (File.OpenWrite(tempFile))
			{
				filechangedCount++;
			}
			monitor.Set();
		}
	}
}
