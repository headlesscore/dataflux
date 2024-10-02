using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Tasks;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
	[TestFixture]
	public class DevenvTaskResultTest
	{
		[Test]
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
            ClassicAssert.AreEqual(expected, result.Data);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

		[Test]
		public void ShouldHandleSpecialCharacters()
		{
			DevenvTaskResult result = new DevenvTaskResult(ProcessResultFixture.CreateSuccessfulResult("<T>"));
			ClassicAssert.AreEqual("<buildresults><message>&lt;T&gt;</message></buildresults>", result.Data);
		}
	}
}
