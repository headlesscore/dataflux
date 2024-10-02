using System.IO;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
	[TestFixture]
	public class RegistryTest
	{
		private const string VALID_REGISTRY_PATH = @"SOFTWARE\Microsoft\Windows\CurrentVersion";

		[Test]
        [Platform(Exclude = "Mono", Reason = "No real registry under Linux")]
		public void GetLocalMachineSubKeyValue()
		{
			string programFilesPath = new Registry().GetLocalMachineSubKeyValue(VALID_REGISTRY_PATH, "ProgramFilesPath");
			ClassicAssert.IsNotNull(programFilesPath, "#A1");
			ClassicAssert.AreNotEqual(string.Empty, programFilesPath, "#A2");
			ClassicAssert.IsTrue(Directory.Exists(programFilesPath), "#A3");
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

		[Test]
		public void TryToGetInvalidSubKey()
		{
			ClassicAssert.IsNull(new Registry().GetLocalMachineSubKeyValue(@"SOFTWARE\BozosSoftwareEmporium\Clowns", "Barrios"), "#B1");
		}

		[Test]
		public void TryToGetInvalidSubKeyValue()
		{
			ClassicAssert.IsNull(new Registry().GetLocalMachineSubKeyValue(VALID_REGISTRY_PATH, "Barrios"), "#C1");
		}

		[Test]
		public void TryToGetExpectedInvalidSubKey()
		{
			ClassicAssert.That(delegate { new Registry().GetExpectedLocalMachineSubKeyValue(@"SOFTWARE\BozosSoftwareEmporium\Clowns", "Barrios"); },
                        Throws.TypeOf<CruiseControlException>());
		}

		[Test]
		public void TryToGetExpectedInvalidSubKeyValue()
		{
            ClassicAssert.That(delegate { new Registry().GetExpectedLocalMachineSubKeyValue(VALID_REGISTRY_PATH, "Barrios"); },
                        Throws.TypeOf<CruiseControlException>());
		}
	}
}
