using System.Drawing;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.CCTrayLib;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib
{
	[TestFixture]
	public class StatusIconTest : IconFileFixture
	{
		[SetUp]
		public override void Init()
		{
			base.Init();
		}

		[Test]
		public void ShouldLoadIconFromFileWhenFileExists()
		{
			StatusIcon iconFile = StatusIcon.LoadFromFile(file);
			Size size = iconFile.Icon.Size;
			ClassicAssert.AreEqual(originalIcon.Size, size);
            ClassicAssert.AreEqual(originalIcon.Size, size);
        }

		[Test]
		public void ShouldThrowIconNotFoundExceptionIfFileDoesNotExist()
		{
		    ClassicAssert.That(delegate { StatusIcon.LoadFromFile("./fileNotOnDisk.ico"); },
		                Throws.TypeOf<IconNotFoundException>());
		}

		[TearDown]
		public override void DeleteFile()
		{
			base.DeleteFile();
		}
	}
}
