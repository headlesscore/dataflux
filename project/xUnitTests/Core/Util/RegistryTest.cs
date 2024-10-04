using System.IO;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{

	public class RegistryTest
	{
		private const string VALID_REGISTRY_PATH = @"SOFTWARE\Microsoft\Windows\CurrentVersion";

		[Fact]
		public void GetLocalMachineSubKeyValue()
		{
			string programFilesPath = new Registry().GetLocalMachineSubKeyValue(VALID_REGISTRY_PATH, "ProgramFilesPath");
			Assert.False(programFilesPath is null, "#A1");
			Assert.False(string.IsNullOrEmpty(programFilesPath), "#A2");
			Assert.True(Directory.Exists(programFilesPath), "#A3");
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public void TryToGetInvalidSubKey()
		{
			Assert.True(new Registry().GetLocalMachineSubKeyValue(@"SOFTWARE\BozosSoftwareEmporium\Clowns", "Barrios") is null, "#B1");
		}

		[Fact]
		public void TryToGetInvalidSubKeyValue()
		{
			Assert.True(new Registry().GetLocalMachineSubKeyValue(VALID_REGISTRY_PATH, "Barrios") is null, "#C1");
		}

		[Fact]
		public void TryToGetExpectedInvalidSubKey()
		{
			Assert.Throws<CruiseControlException>(delegate { new Registry().GetExpectedLocalMachineSubKeyValue(@"SOFTWARE\BozosSoftwareEmporium\Clowns", "Barrios"); });
		}

		[Fact]
		public void TryToGetExpectedInvalidSubKeyValue()
		{
            Assert.Throws<CruiseControlException>(delegate { new Registry().GetExpectedLocalMachineSubKeyValue(VALID_REGISTRY_PATH, "Barrios"); });
		}
	}
}
