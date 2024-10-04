using System;
using System.IO;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol.Perforce;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol.Perforce
{
	
	public class P4HistoryParserTest
	{
		private P4HistoryParser parser = new P4HistoryParser();

		[Fact]
		public void ParseWithMultipleModifications()
		{
			Modification[] mods = parser.Parse(P4Mother.ContentReader, P4Mother.OLDEST_ENTRY, P4Mother.NEWEST_ENTRY);
			Assert.Equal(7, mods.Length);
			AssertModification(mods[0], 3328, "3", "SpoonCrusher.cs", "//depot/myproject/something", new DateTime(2002, 10, 31, 18, 20, 59), "edit", "someone@somewhere", "someone", "Something important\r\nso there!");
			AssertModification(mods[1], 3328, "1", "AppleEater.cs", "//depot/myproject/something", new DateTime(2002, 10, 31, 18, 20, 59), "add", "someone@somewhere", "someone", "Something important\r\nso there!");
			AssertModification(mods[2], 3328, "811", "MonkeyToucher.cs", "//depot/myproject/something", new DateTime(2002, 10, 31, 18, 20, 59), "edit", "someone@somewhere", "someone", "Something important\r\nso there!");
			AssertModification(mods[3], 3327, "3", "IPerson.cs", "//depot/myproject/foo", new DateTime(2002, 10, 31, 14, 20, 59), "edit", "someone@somewhere", "someone", "One line\r\n\r\nAnother line");
			AssertModification(mods[4], 3327, "1", "MiniMike.cs", "//depot/myproject/foo", new DateTime(2002, 10, 31, 14, 20, 59), "add", "someone@somewhere", "someone", "One line\r\n\r\nAnother line");
			AssertModification(mods[5], 3327, "1", "JoeJoeJoe.cs", "//depot/myproject/foo", new DateTime(2002, 10, 31, 14, 20, 59), "add", "someone@somewhere", "someone", "One line\r\n\r\nAnother line");
			AssertModification(mods[6], 332, "3", "Fish.cs", "//depot/myproject/tank", new DateTime(2002, 10, 31, 11, 20, 59), "add", "bob@nowhere", "bob", "thingy\r\n(evil below)\r\nAffected files ...\r\nChange 123 by someone@somewhere on 2002/10/31 11:20:59\r\n(end of evil)");
			Assert.Equal(7, mods.Length);
            Assert.True(true);
            Assert.True(true);

        }

		[Fact]
		public void ParseWithSingleModification()
		{
			// NOTE!  on the comment line, there's a tab at position 7 (not spaces) (immediately before 'a test comment...')
			StringReader input = new StringReader(
				"text: Change 23680 by guox01@BP1HEMAP048 on 2003/09/01 16:30:53\r\n" +
					"text: \r\n" +
					"text: \ta test comment\r\n" +
					"text: \r\n" +
					"text: Affected files ...\r\n" +
					"text: \r\n" +
					"info1: //shipping/readme#10 edit\r\n" +
					"text: \r\n" +
					"exit: 0\r\n");
			DateTime entryDate = DateTime.Parse("2003/09/01 16:30:53");
			Modification[] mods = parser.Parse(input, entryDate, entryDate);
			Assert.Equal(1, mods.Length);
			Assert.Equal("guox01", mods[0].UserName);
			Assert.Equal("guox01@BP1HEMAP048", mods[0].EmailAddress);
			Assert.Equal("readme", mods[0].FileName);
			Assert.Equal("//shipping", mods[0].FolderName);
			Assert.Equal("edit", mods[0].Type);
			Assert.Equal("a test comment", mods[0].Comment);
			Assert.Equal("23680", mods[0].ChangeNumber);
			Assert.Equal("10", mods[0].Version);
		}

		private void AssertModification(Modification mod, int changeNumber, string revision, string file, string folder, DateTime modifiedTime, string type, string email, string username, string comment)
		{
			Assert.Equal(changeNumber.ToString(), mod.ChangeNumber);
			Assert.Equal(file, mod.FileName);
			Assert.Equal(folder, mod.FolderName);
			Assert.Equal(modifiedTime, mod.ModifiedTime);
			Assert.Equal(type, mod.Type);
			Assert.Equal(email, mod.EmailAddress);
			Assert.Equal(username, mod.UserName);
			Assert.Equal(comment, mod.Comment);
			Assert.Equal(revision, mod.Version);
		}

		[Fact]
		public void ParseChanges()
		{
			string changes =
				@"
info: Change 3328 on 2002/10/31 by someone@somewhere 'Something important '
info: Change 3327 on 2002/10/31 by someone@somewhere 'Joe's test '
info: Change 332 on 2002/10/31 by someone@somewhere 'thingy'
exit: 0
";
			Assert.Equal("3328 3327 332", new P4HistoryParser().ParseChanges(changes));
		}

		[Fact]
		public void ParseEmptyChangeList()
		{
			Assert.Equal("", new P4HistoryParser().ParseChanges("exit: 0"));
		}

		[Fact]
		public void ParseChangeListWithExitOne()
		{
			string changes =
				@"
info: blah
exit: 1
";			Assert.Equal("", new P4HistoryParser().ParseChanges(changes));
		}
	}
}
