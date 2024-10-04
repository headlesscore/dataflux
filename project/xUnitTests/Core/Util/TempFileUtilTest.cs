using System;
using System.IO;
using System.Xml;
using Xunit;

using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
    
	public class TempFileUtilTest : CustomAssertion
	{
		private static readonly string TempDir = "tempfileutiltest";

		// [SetUp]
		public void SetUp()
		{
			TempFileUtil.DeleteTempDir(TempDir);
			Assert.True(! Directory.Exists(TempFileUtil.GetTempPath(TempDir)), "Temp folder exists before test!");
            Assert.True(true);
            Assert.True(true);
        }

		// [TearDown]
		public void TearDown()
		{
			TempFileUtil.DeleteTempDir(TempDir);
			Assert.True(! Directory.Exists(TempFileUtil.GetTempPath(TempDir)));
		}

		public void TestCreateTempDir()
		{
			TempFileUtil.CreateTempDir(TempDir);
			Assert.True(Directory.Exists(TempFileUtil.GetTempPath(TempDir)));
			TempFileUtil.DeleteTempDir(TempDir);
		}

		public void TestCreateTempDirOverride()
		{
			TempFileUtil.CreateTempDir(TempDir);
			TempFileUtil.CreateTempFiles(TempDir, new String[]{"test.tmp"});
			Assert.Equal(1, Directory.GetFiles(TempFileUtil.GetTempPath(TempDir)).Length);
			TempFileUtil.CreateTempDir(TempDir);
			Assert.Equal(0, Directory.GetFiles(TempFileUtil.GetTempPath(TempDir)).Length);
		}

		public void TestCreateTempXmlDoc()
		{
			TempFileUtil.CreateTempDir(TempDir);
			string path = TempFileUtil.CreateTempXmlFile(TempDir, "foobar.xml", "<test />");
			Assert.True(File.Exists(path));
			XmlDocument doc = new XmlDocument();
			doc.Load(path);
		}

		[Fact]
		public void CreateTempFileWithContent()
		{
			string expected = "hello my name is rosebud";
			string path = TempFileUtil.CreateTempFile(TempDir, "TestCreateTempFile_withContent.txt", expected);
			Assert.True(File.Exists(path));
			using (StreamReader stream = File.OpenText(path))
			{
				Assert.Equal(expected, stream.ReadToEnd());				
			}
		}
	}
}
