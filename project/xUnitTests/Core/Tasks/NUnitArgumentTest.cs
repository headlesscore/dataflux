using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
	
	public class NUnitArgumentTest : CustomAssertion
	{
		[Fact]
		public void IfNoAssembliesAreSpecifiedThenTheArgumentIsInvalid()
		{
            Assert.Throws<CruiseControlException>(delegate { new NUnitArgument(null, null); });
        }

		[Fact]
		public void ShouldUseNoLogoArgument()
		{
			string argString = new NUnitArgument(new string[] {"foo.dll"}, "testfile.xml").ToString();
			AssertContains("/nologo", argString);
		}

		[Fact]
		public void ShouldSpecifyXmlOutputFileToUse()
		{
			string argString = new NUnitArgument(new string[] {"foo.dll"}, "testfile.xml").ToString();
			AssertContains(@"/xml=testfile.xml", argString);
		}

		[Fact]
		public void ShouldWrapOutputFileInQuotesIfItContainsASpace()
		{
			string argString = new NUnitArgument(new string[] {"foo.dll"}, @"c:\program files\cruisecontrol.net\testfile.xml").ToString();
			AssertContains(@"/xml=""c:\program files\cruisecontrol.net\testfile.xml""", argString);
		}

		[Fact]
		public void AllAssembliesShouldBeIncludedInTheArgument()
		{
			string argString = new NUnitArgument(new string[] {"foo.dll", "bar.dll", "car.dll"}, "testfile.xml").ToString();
			AssertContains(" foo.dll ", argString);
			AssertContains(" bar.dll ", argString);
			AssertContains(" car.dll", argString);
		}

        [Fact]
        public void ShouldSpecifyCategoriesIfTheRelativePropertiesAreSet()
        {
            NUnitArgument nunitArgument = new NUnitArgument(new string[] { "foo.dll" }, "testfile.xml");
            nunitArgument.ExcludedCategories = new string[] { "ExcludedCategory1", "ExcludedCategory2" };
            nunitArgument.IncludedCategories = new string[] { "IncludedCategory1", "IncludedCategory2" };
            string argString = nunitArgument.ToString();
            AssertContains(@"/exclude=ExcludedCategory1,ExcludedCategory2", argString);
            AssertContains(@"/include=IncludedCategory1,IncludedCategory2", argString);
        }

        [Fact]
        public void ShouldNotSpecifyCategoriesIfTheRelativePropertiesAreSetToNull()
        {
            NUnitArgument nunitArgument = new NUnitArgument(new string[] { "foo.dll" }, "testfile.xml");
            nunitArgument.ExcludedCategories = null;
            nunitArgument.IncludedCategories = null;
            string argString = nunitArgument.ToString();
            AssertNotContains(@"/exclude", argString);
            AssertNotContains(@"/include", argString);
        }

        [Fact]
        public void ShouldNotSpecifyCategoriesIfTheRelativePropertiesAreSetToAnEmptyArray()
        {
            NUnitArgument nunitArgument = new NUnitArgument(new string[] { "foo.dll" }, "testfile.xml");
            nunitArgument.ExcludedCategories = new string[0];
            nunitArgument.IncludedCategories = new string[0];
            string argString = nunitArgument.ToString();
            AssertNotContains(@"/exclude", argString);
            AssertNotContains(@"/include", argString);
        }

        [Fact]
        public void ShouldNotSpecifyCategoriesWhoseNameIsEmptyStringOrWhiteSpace()
        {
            NUnitArgument nunitArgument = new NUnitArgument(new string[] { "foo.dll" }, "testfile.xml");
            nunitArgument.ExcludedCategories = new string[] { "ExcludedCategory1", " ", "ExcludedCategory2" };
            nunitArgument.IncludedCategories = new string[] { "IncludedCategory1", "", "IncludedCategory2" };
            string argString = nunitArgument.ToString();
            AssertContains(@"/exclude=ExcludedCategory1,ExcludedCategory2", argString);
            AssertContains(@"/include=IncludedCategory1,IncludedCategory2", argString);
        }

        [Fact]
        public void ShouldDoubleQuoteCategoriesWhoseNameContainsWhiteSpace()
        {
            NUnitArgument nunitArgument = new NUnitArgument(new string[] { "foo.dll" }, "testfile.xml");
            nunitArgument.ExcludedCategories = new string[] { "Excluded Category1" };
            nunitArgument.IncludedCategories = new string[] { "Included Category1" };
            string argString = nunitArgument.ToString();
            AssertContains(@"/exclude=""Excluded Category1""", argString);
            AssertContains(@"/include=""Included Category1""", argString);
        }
	}
}
