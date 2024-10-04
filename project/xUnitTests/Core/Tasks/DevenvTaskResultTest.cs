using Xunit;

using ThoughtWorks.CruiseControl.Core.Tasks;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
	
	public class DevenvTaskResultTest
	{
		[Fact]
		public void CreateFailedXmlFromDevenvOutput()
		{
            string stdOut = @"------ Build started: Project: Refactoring, Configuration: Debug .NET ------

Performing main compilation...
D:\dev\Refactoring\Movie.cs(30,2): error CS1513: } expected" + "\0" + @"

Build complete -- 1 errors, 0 warnings";
            string stdErr = @"Package 'Microsoft.VisualStudio.TeamFoundation.VersionControl.HatPackage, Microsoft.VisualStudio.TeamFoundation.VersionControl, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' failed to load.";

            string expected = @"<buildresults>" +
"<message>------ Build started: Project: Refactoring, Configuration: Debug .NET ------</message>" +
"<message>Performing main compilation...</message>" +
@"<message>D:\dev\Refactoring\Movie.cs(30,2): error CS1513: } expected</message>" +
"<message>Build complete -- 1 errors, 0 warnings</message>" +
"<message level=\"error\">Package 'Microsoft.VisualStudio.TeamFoundation.VersionControl.HatPackage, Microsoft.VisualStudio.TeamFoundation.VersionControl, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' failed to load.</message>" +
"</buildresults>";

            DevenvTaskResult result = new DevenvTaskResult(ProcessResultFixture.CreateNonZeroExitCodeResult(stdOut, stdErr));
            Assert.Equal(expected, result.Data);
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public void ShouldHandleSpecialCharacters()
		{
			DevenvTaskResult result = new DevenvTaskResult(ProcessResultFixture.CreateSuccessfulResult("<T>"));
			Assert.Equal("<buildresults><message>&lt;T&gt;</message></buildresults>", result.Data);
		}
	}
}
