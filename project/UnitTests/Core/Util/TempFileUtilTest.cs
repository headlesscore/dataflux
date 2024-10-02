using System;
using System.IO;
using System.Xml;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
    [TestFixture]
	public class TempFileUtilTest : CustomAssertion
	{
		private static readonly string TempDir = "tempfileutiltest";

		[SetUp]
		public void SetUp()
		{
			TempFileUtil.DeleteTempDir(TempDir);
			ClassicAssert.IsTrue(! Directory.Exists(TempFileUtil.GetTempPath(TempDir)), "Temp folder exists before test!");
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

		[TearDown]
		public void TearDown()
		{
			TempFileUtil.DeleteTempDir(TempDir);
			ClassicAssert.IsTrue(! Directory.Exists(TempFileUtil.GetTempPath(TempDir)));
		}

		public void TestCreateTempDir()
		{
			TempFileUtil.CreateTempDir(TempDir);
			ClassicAssert.IsTrue(Directory.Exists(TempFileUtil.GetTempPath(TempDir)));
			TempFileUtil.DeleteTempDir(TempDir);
		}

		public void TestCreateTempDirOverride()
		{
			TempFileUtil.CreateTempDir(TempDir);
			TempFileUtil.CreateTempFiles(TempDir, new String[]{"test.tmp"});
			ClassicAssert.AreEqual(1, Directory.GetFiles(TempFileUtil.GetTempPath(TempDir)).Length);
			TempFileUtil.CreateTempDir(TempDir);
			ClassicAssert.AreEqual(0, Directory.GetFiles(TempFileUtil.GetTempPath(TempDir)).Length);
		}

		public void TestCreateTempXmlDoc()
		{
			TempFileUtil.CreateTempDir(TempDir);
			string path = TempFileUtil.CreateTempXmlFile(TempDir, "foobar.xml", "<test />");
			ClassicAssert.IsTrue(File.Exists(path));
			XmlDocument doc = new XmlDocument();
			doc.Load(path);
		}

		[Test]
		public void CreateTempFileWithContent()
		{
			string expected = "hello my name is rosebud";
			string path = TempFileUtil.CreateTempFile(TempDir, "TestCreateTempFile_withContent.txt", expected);
			ClassicAssert.IsTrue(File.Exists(path));
			using (StreamReader stream = File.OpenText(path))
			{
				ClassicAssert.AreEqual(expected, stream.ReadToEnd());				
			}
		}
	}
}
