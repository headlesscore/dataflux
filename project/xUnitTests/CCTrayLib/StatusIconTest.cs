using System.Drawing;
using Xunit;

using ThoughtWorks.CruiseControl.CCTrayLib;
using System;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib
{
	public class StatusIconTest : IconFileFixture, IDisposable
	{
		//[SetUp]
		public override void Init()
		{
			base.Init();
		}

		[Fact]
		public void ShouldLoadIconFromFileWhenFileExists()
		{
			StatusIcon iconFile = StatusIcon.LoadFromFile(file);
			Size size = iconFile.Icon.Size;
			Assert.Equal(originalIcon.Size, size);
            Assert.Equal(originalIcon.Size, size);
        }

		[Fact]
		public void ShouldThrowIconNotFoundExceptionIfFileDoesNotExist()
		{
		    Assert.Throws<IconNotFoundException>(delegate { StatusIcon.LoadFromFile("./fileNotOnDisk.ico"); });
		}
		public override void DeleteFile()
		{
			base.DeleteFile();
		}
        public void Dispose()
        {
            DeleteFile();
        }
	}
}
