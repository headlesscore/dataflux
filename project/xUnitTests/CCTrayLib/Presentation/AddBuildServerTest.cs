using System;
using Xunit;
using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using ThoughtWorks.CruiseControl.CCTrayLib.Presentation;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Presentation
{
	public class AddBuildServerTest
	{
		// This isn't really a test, just a quick way to invoke and display the
		// dialog for interactive testing
		[Fact(Skip = "This isn't really a test, just a quick way to invoke and display the dialog for interactive testing")]
		public void ShowDialogForInteractiveTesting()
		{
			AddBuildServer addBuildServer = new AddBuildServer(null);
			BuildServer server = addBuildServer.ChooseNewBuildServer(null);
			Console.WriteLine(server);
		}

	}
}
