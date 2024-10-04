using Xunit;

using ThoughtWorks.CruiseControl.Core.Sourcecontrol.Perforce;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol.Perforce
{
	
	public class P4ConfigProcessInfoCreatorTest 
	{
		[Fact]
		public void ShouldCreateProcessUsingAllConfigurationVariablesIfTheyAreSet()
		{
			// Setup
			P4 p4 = new P4();
			p4.Executable = "myExecutable";
			p4.Client = "myClient";
			p4.User = "myUser";
			p4.Port = "anotherserver:2666";

			ProcessInfo info = new P4ConfigProcessInfoCreator().CreateProcessInfo(p4, "my arguments");

			Assert.Equal("myExecutable", info.FileName);
			Assert.Equal("-s -c myClient -p anotherserver:2666 -u myUser my arguments", info.Arguments);
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public void ShouldCreateProcessWithDefaultArgumentsIfNoneAreSet()
		{
			// Setup
			P4 p4 = new P4();

			ProcessInfo info = new P4ConfigProcessInfoCreator().CreateProcessInfo(p4, "my arguments");

			Assert.Equal("p4", info.FileName);
			Assert.Equal("-s my arguments", info.Arguments);
		}
		
		[Fact]
		public void ShouldCreateProcessWithDefaultArgumentsIfOnlyUserIsSet()
		{
			// Setup
			P4 p4 = new P4();
			p4.User = "myUser";

			ProcessInfo info = new P4ConfigProcessInfoCreator().CreateProcessInfo(p4, "my arguments");

			Assert.Equal("p4", info.FileName);
			Assert.Equal("-s -u myUser my arguments", info.Arguments);
		}
	}
}
