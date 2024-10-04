namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol.Mercurial
{
	using Xunit;
    
    using System;
    using System.IO;
	using ThoughtWorks.CruiseControl.Core;
	using ThoughtWorks.CruiseControl.Core.Sourcecontrol.Mercurial;

	/// <summary>
	/// Test fixture for the <see cref="MercurialHistoryParser"/>.
	/// </summary>
	
	public class MercurialHistoryParserTest
	{
		#region Constants

		private const string EmptyLogXml = "<log />";
		private const string FullLogXml =
			@"<log>
<logentry revision=""3"" node=""48365ef6a3ea"" >
<author email=""bbarry@stellarfinancial.com"">B Barry</author>
<date>2008-04-24T22:14:59-0600</date>
<msg xml:space=""preserve"">asdf</msg>
<paths>
<path action=""M"">New Text Document.txt</path>
</paths>
</logentry>
<logentry revision=""2"" node=""f5e50e14bf09"" >
<author email=""bbarry@stellarfinancial.com"">B Barry</author>
<date>2008-04-24T1111:30:48-0600</date>
<msg xml:space=""preserve"">asdf</msg>
<paths>
<path action=""M"">in_branch.txt</path>
</paths>
</logentry>
<logentry revision=""1"" node=""a903fa9d43f4"" >
<author email=""bbarry@stellarfinancial.com"">B Barry</author>
<date>2008-04-23T22:12:48-0600</date>
<msg xml:space=""preserve"">adding a file</msg>
<paths>
<path action=""M"">ReadMe.txt</path>
</paths>
</logentry>
<logentry revision=""0"" node=""030098cf5256"" >
<author email=""bbarry@stellarfinancial.com"">B Barry</author>
<date>2008-04-23T16:28:15-0600</date>
<msg xml:space=""preserve"">c# app 1</msg>
<paths>
<path action=""M"">.hgignore</path>
<path action=""M"">hg.sln</path>
<path action=""M"">hg/Class1.cs</path>
<path action=""M"">hg/Properties/AssemblyInfo.cs</path>
<path action=""M"">hg/hg.csproj</path>
</paths>
</logentry>
</log>";
		private const string OneEntryLogXml =
			@"<log>
<logentry node=""48365ef6a3ea"" revision=""3"" >
<author email=""bbarry@stellarfinancial.com"">B Barry</author>
<date>2008-04-24T22:14:59-0600</date>
<msg xml:space=""preserve"">asdf</msg>
<paths>
<path action=""M"">New Text Document.txt</path>
</paths>
</logentry>
</log>";
		private const string LongPathEntryLogXml =
			@"<log>
<logentry node=""48365ef6a3ea"" revision=""3"" >
<author email=""bbarry@stellarfinancial.com"">B Barry</author>
<date>2008-04-24T22:14:59-0600</date>
<msg xml:space=""preserve"">asdf</msg>
<paths>
<path action=""M"">these/are/the/parent/folders/to/this/file.txt</path>
</paths>
</logentry>
</log>";
		private const string InvalidEmail =
			@"<log>
<logentry revision=""79"" node=""bbcb38e1ceb6e050482336aa2fcfd40da4e3f3ac"">
<parent revision=""77"" node=""d5a94550fd9115d60d3bd9b45aa1991c14b81dc9"" />
<parent revision=""78"" node=""50af4a864f54c8624e00b6d0d468f0afab433f4c"" />
<author email=""example"">example</author>
<date>2010-08-16T08:22:49-07:00</date>
<msg xml:space=""preserve"">Merge Commit?</msg>
<paths>
<path action=""M"">p/w/p/r/s/rs.asmx.cs</path>
<path action=""M"">p/w/p/r/s/wwpr.csproj</path>
</paths>
</logentry>
</log>";

		#endregion

		#region Private Members

		private MercurialHistoryParser hg;

		#endregion

		#region SetUp Method

		// [SetUp]
		public void SetUp()
		{
			hg = new MercurialHistoryParser();
		}

		#endregion

		#region Tests

		[Fact]
		public void ParsingEmptyLogProducesNoModifications()
		{
			Modification[] modifications = hg.Parse(new StringReader(EmptyLogXml), DateTime.Now, DateTime.Now);
			Assert.Empty(modifications);
        }

		[Fact]
		public void ParsingSingleLogMessageProducesOneModification()
		{
			var modifications = hg.Parse(new StringReader(OneEntryLogXml), DateTime.Now, DateTime.Now);
			Assert.True(modifications.Length == 1);

			var mod = modifications[0];
			Assert.True(mod.Version == "48365ef6a3ea");
			Assert.True(mod.ChangeNumber == "3");
			Assert.True(mod.EmailAddress == "bbarry@stellarfinancial.com");
			Assert.True(mod.UserName == "B Barry");
			var expectedModifiedTime = new DateTimeOffset(2008, 4, 24, 22, 14, 59, new TimeSpan(-6, 0, 0));
			Assert.True(mod.ModifiedTime == expectedModifiedTime.LocalDateTime);
			Assert.True(mod.Comment == "asdf");
			Assert.True(string.IsNullOrEmpty( mod.FolderName));
			Assert.True(mod.FileName == "New Text Document.txt");
		}

		[Fact]
		public void ShouldSeparateFolderFromFileName()
		{
			var modifications = hg.Parse(new StringReader(LongPathEntryLogXml), DateTime.Now, DateTime.Now);
			Assert.True(modifications.Length == 1);

			var mod = modifications[0];
			Assert.True(mod.FolderName == Path.Combine(new string[] {"these", "are", "the", "parent", "folders", "to", "this"}));
			Assert.True(mod.FileName == "file.txt");
		}

		[Fact]
		public void ShouldHandleInvalidEmailAddress()
		{
			var modifications = hg.Parse(new StringReader(InvalidEmail), DateTime.Now, DateTime.Now);

			var mod = modifications[0];
			Assert.True(mod.EmailAddress == "example");
			Assert.True(mod.UserName == "example");
		}

		[Fact]
		public void ParsingLotsOfEntries()
		{
			Modification[] modifications = hg.Parse(new StringReader(FullLogXml), DateTime.Now, DateTime.Now);
			Assert.True(modifications.Length == 8);
		}

		[Fact]
		public void HandleInvalidXml()
		{
			Assert.True(delegate { hg.Parse(new StringReader("<foo/><bar/>"), DateTime.Now, DateTime.Now); },
			            Throws.TypeOf<CruiseControlException>());
		}

		#endregion
	}
}
