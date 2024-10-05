using System;
using System.Globalization;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	
	public class VssHistoryParserTest : CustomAssertion
	{
		private VssHistoryParser parser;

		// [SetUp]
		public void SetUp()
		{
			parser = new VssHistoryParser(new VssLocale(CultureInfo.InvariantCulture));
		}

		[Fact]
		public void Parse()
		{
			Modification[] mods = parser.Parse(VssMother.ContentReader, VssMother.OLDEST_ENTRY, VssMother.NEWEST_ENTRY);
			Assert.NotNull(mods);
			Assert.Equal(19, mods.Length);
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public void ReadAllEntriesTest() 
		{
			string[] entries = parser.ReadAllEntries(VssMother.ContentReader);
			Assert.Equal(24, entries.Length);
		}

		[Fact]
		public void IsEntryDelimiter()
		{
			string line = "*****  cereal.txt  *****";
			Assert.True(parser.IsEntryDelimiter(line), "should recognize as delim");

			line = "*****************  Version 8   *****************";
			Assert.True(parser.IsEntryDelimiter(line), "should recognize as delim");

			line = "*****";
			Assert.True(parser.IsEntryDelimiter(line) == false, string.Format(System.Globalization.CultureInfo.CurrentCulture,"should not recognize as delim '{0}'", line));

			line = "*****************  Version 4   *****************";
			Assert.True(parser.IsEntryDelimiter(line), "should recognize as delim");
		}

		[Fact]
		public void ParseCreatedModification()
		{
			string entry = EntryWithSingleLineComment();
			
			Modification expected = new Modification();
			expected.Comment = "added subfolder";
			expected.UserName = "Admin";
			expected.ModifiedTime = new DateTime(2002, 9, 16, 14, 41, 0);
			expected.Type = "Created";
			expected.FileName = "[none]";
			expected.FolderName = "plant";

			Modification[] actual = parser.ParseModifications(makeArray(entry));
			Assert.NotNull(actual);
			Assert.Empty(actual);
		}

		[Fact]
		public void ParseUsernameAndUSDate()
		{
			Modification mod = new Modification();
			
			string line = "foo\r\nUser: Admin        Date:  9/16/02   Time:  2:40p\r\n";
			CheckInParser parser = new CheckInParser(line, new VssLocale(new CultureInfo("en-US")));
			parser.ParseUsernameAndDate(mod);
			string expectedUsername = "Admin";
			DateTime expectedDate = new DateTime(2002, 09, 16, 14, 40, 0);
			Assert.Equal(expectedUsername, mod.UserName);
			Assert.Equal(expectedDate, mod.ModifiedTime);
		}

		[Fact]
		public void ParseUsernameAndUKDate()
		{
			Modification mod = new Modification();
			string line = "foo\r\nUser: Admin        Date:  16/9/02   Time:  22:40\r\n";
            CheckInParser myParser = new CheckInParser(line, new VssLocale(new CultureInfo("en-GB")));
            myParser.ParseUsernameAndDate(mod);

			Assert.Equal("Admin", mod.UserName);
			Assert.Equal(new DateTime(2002, 9, 16, 22, 40, 0), mod.ModifiedTime);
		}

		[Fact]
		public void ShouldThrowCruiseControlExceptionShowingDateStringIfCannotParseDate()
		{
			Modification mod = new Modification();
			string line = "foo\r\nUser: Admin        Date:  16/24/02   Time:  22:40\r\n";
            CheckInParser myParser = new CheckInParser(line, new VssLocale(CultureInfo.InvariantCulture));
            Assert.Throws<CruiseControlException>(delegate { myParser.ParseUsernameAndDate(mod); });
		}

		[Fact(Skip ="")]
        //[Ignore("waiting for english build server")]
		public void ParseUsernameAndFRDate()
		{
			Modification mod = new Modification();
			string line = "foo\r\nUtilisateur: Admin        Date:  2/06/04   Heure: 14:04\r\n";
            CheckInParser myParser = new CheckInParser(line, new VssLocale(new CultureInfo("fr-FR")));
            myParser.ParseUsernameAndDate(mod);

			Assert.Equal("Admin", mod.UserName);
			Assert.Equal(new DateTime(2004,6,2,14,4,0), mod.ModifiedTime);
		}

        /// <summary>
        /// Regression lock test for CCNET-251 - "VSS Parser Error with french environnment"
        /// At least in french (not in english), VSS adds an ascii char 160 before the colon, which messes up the parsing.
        /// </summary>
        [Fact(Skip = "")]
        //[Ignore("waiting for english build server")]
		public void ParseUsernameAndFRDateWithAsciiCode160()
		{
			Modification mod = new Modification();
			string line = "*****  Tools.build  *****\r\nVersion 3\r\nUtilisateur\xA0: Thomas       Date\xA0: 15/11/04   Heure\xA0: 18:24\r\nArchivé dans $/Projets/Tools\r\nCommentaire: \r\n\r\n";
			CheckInParser parser = new CheckInParser(line, new VssLocale(new CultureInfo("fr-FR")));
			parser.ParseUsernameAndDate(mod);

			Assert.Equal("Thomas", mod.UserName);
			Assert.Equal(new DateTime(2004,11,15,18,24,0), mod.ModifiedTime);
		}

		[Fact]
		public void ParseUsernameAndDateWithPeriod() 
		{
			//User: Gabriel.gilabert     Date:  5/08/03   Time:  4:06a
			Modification mod = new Modification();
			
			string line = "foo\r\nUser: Gabriel.gilabert     Date:  5/08/03   Time:  4:06a\r\n";
			CheckInParser parser = new CheckInParser(line, new VssLocale(new CultureInfo("en-US")));
			parser.ParseUsernameAndDate(mod);
			string expectedUsername = "Gabriel.gilabert";
			DateTime expectedDate = new DateTime(2003, 05, 08, 04, 06, 0);
			Assert.Equal(expectedUsername, mod.UserName);
			Assert.Equal(expectedDate, mod.ModifiedTime);
		}
		
		[Fact]
		public void ParseMultiWordUsername()
		{
			Modification mod = new Modification();
			
			string line = "foo\r\nUser: Gabriel Gilabert     Date:  5/08/03   Time:  4:06a\r\n";
			CheckInParser parser = new CheckInParser(line, new VssLocale(new CultureInfo("en-US")));
			parser.ParseUsernameAndDate(mod);
			string expectedUsername = "Gabriel Gilabert";
			DateTime expectedDate = new DateTime(2003, 05, 08, 04, 06, 0);
			Assert.Equal(expectedUsername, mod.UserName);
			Assert.Equal(expectedDate, mod.ModifiedTime);
		}

		[Fact]
		public void ParseInvalidUsernameLine()
		{
			string line = "foo\r\nbar\r\n";
            Assert.Throws<CruiseControlException>(delegate { new CheckInParser(line, new VssLocale(new CultureInfo("en-US"))).ParseUsernameAndDate(new Modification()); });
		}

		[Fact]
		public void ParseFileName() 
		{
			string fileName = "**** Im a file name.fi     ********\r\n jakfakjfnb  **** ** lkjnbfgakj ****";
			CheckInParser myParser = new CheckInParser(fileName, new VssLocale(new CultureInfo("en-US")));
            string actual = myParser.ParseFileName();
			Assert.Equal("Im a file name.fi", actual);
		}

		[Fact]
		public void ParseCheckedInFileAndFolder()
		{
			string entry = @"*****  happyTheFile.txt  *****
Version 3
User: Admin        Date:  9/16/02   Time:  5:01p
Checked in $/you/want/folders/i/got/em
Comment: added fir to tree file, checked in recursively from project root";

			string expectedFile = "happyTheFile.txt";
			string expectedFolder = "$/you/want/folders/i/got/em";

			Modification mod = ParseAndAssertFilenameAndFolder(entry, expectedFile, expectedFolder);
			Assert.Equal("Admin", mod.UserName);
			Assert.Equal(new DateTime(2002, 9, 16, 17, 01, 0), mod.ModifiedTime);
			Assert.Equal("Checked in", mod.Type);
			Assert.Equal("added fir to tree file, checked in recursively from project root",mod.Comment);
		}

        [Fact(Skip = "")]
        //[Ignore("waiting for english build server")]
		public void ParseCheckedInFileAndFolderInFrench()
		{
			// change the parser culture for this test only
			parser = new VssHistoryParser(new VssLocale(new CultureInfo("fr-FR")));

			string entry = @"*****  happyTheFile.txt  *****
Version 16
Utilisateur: Admin        Date:  25/11/02   Heure: 17:32
Archivé dans $/you/want/folders/i/got/em
Commentaire: adding this file makes me so happy";

			string expectedFile = "happyTheFile.txt";
			string expectedFolder = "$/you/want/folders/i/got/em";

			Modification mod = ParseAndAssertFilenameAndFolder(entry, expectedFile, expectedFolder);
			Assert.Equal("Admin", mod.UserName);
			Assert.Equal(new DateTime(2002, 11, 25, 17, 32, 0), mod.ModifiedTime);
			Assert.Equal("Archivé dans", mod.Type);
			Assert.Equal("adding this file makes me so happy",mod.Comment);
		}

		[Fact]
		public void ParseCheckedInFileAndFolderWithHypenInFilename()
		{
			string entry = @"*****  happy-The-File.txt  *****
Version 3
User: Admin        Date:  9/16/02   Time:  5:01p
Checked in $/you/want/folders/i/got/em
Comment: added fir to tree file, checked in recursively from project root";

			string expectedFile = "happy-The-File.txt";
			string expectedFolder = "$/you/want/folders/i/got/em";

			Modification mod = ParseAndAssertFilenameAndFolder(entry, expectedFile, expectedFolder);
			Assert.Equal("Admin", mod.UserName);
			Assert.Equal(new DateTime(2002, 9, 16, 17, 01, 0), mod.ModifiedTime);
			Assert.Equal("Checked in", mod.Type);
			Assert.Equal("added fir to tree file, checked in recursively from project root",mod.Comment);
		}

		[Fact]
		public void ParseFileAndFolderWithNoComment()
		{
			string entry = @"*****  happyTheFile.txt  *****
Version 3
User: Admin        Date:  9/16/02   Time:  5:01p
Checked in $/you/want/folders/i/got/em
";

			Modification mod = ParseAndAssertFilenameAndFolder(entry, "happyTheFile.txt", "$/you/want/folders/i/got/em");
			Assert.Equal("Checked in", mod.Type);
			Assert.Null(mod.Comment);
		}

		[Fact]
		public void ParseFileAndFolder_addAtRoot()
		{
			// note: this represents the entry after version line insertion 
			// (see _parser.InsertVersionLine)
			string entry = @"*****************  Version 2   *****************
Version 2
User: Admin        Date:  9/16/02   Time:  2:40p
happyTheFile.txt added
";
			string expectedFile = "happyTheFile.txt";
			string expectedFolder = "[projectRoot]";

			Modification mod = ParseAndAssertFilenameAndFolder(entry, expectedFile, expectedFolder);
			Assert.Equal("Admin", mod.UserName);
			Assert.Equal(new DateTime(2002, 9, 16, 14, 40, 0), mod.ModifiedTime);
			Assert.Equal("added", mod.Type);
			Assert.Equal(null, mod.Comment);
		}
		
		[Fact]
		public void ParseFileAndFolderIfFolderIsCalledAdded()
		{
			string entry = @"*****  added  *****
Version 8
User: Admin        Date:  9/16/02   Time:  5:29p
happyTheFile.txt added
";
			string expectedFile = "happyTheFile.txt";
			string expectedFolder = "added";

			Modification mod = ParseAndAssertFilenameAndFolder(entry, expectedFile, expectedFolder);
			Assert.Equal("Admin", mod.UserName);
			Assert.Equal(new DateTime(2002, 9, 16, 17, 29, 0), mod.ModifiedTime);
			Assert.Equal("added", mod.Type);
			Assert.Equal(null, mod.Comment);
		}

		[Fact]
		public void ParseFileAndFolder_deleteFromSubfolder()
		{
string entry = @"*****  iAmAFolder  *****
Version 8
User: Admin        Date:  9/16/02   Time:  5:29p
happyTheFile.txt deleted";

			string expectedFile = "happyTheFile.txt";
			string expectedFolder = "iAmAFolder";

			Modification mod = ParseAndAssertFilenameAndFolder(entry, expectedFile, expectedFolder);
			Assert.Equal("Admin", mod.UserName);
			Assert.Equal(new DateTime(2002, 9, 16, 17, 29, 0), mod.ModifiedTime);
			Assert.Equal("deleted", mod.Type);
			Assert.Equal(null, mod.Comment);
		}

		private static string[] makeArray(params string[] entries) 
		{
			return entries;
		}

		private Modification ParseAndAssertFilenameAndFolder(
			string entry, string expectedFile, string expectedFolder)
		{
			string[] entries = makeArray(entry);

			Modification[] mod = parser.ParseModifications(entries);
			
			Assert.NotNull(mod);
			Assert.Single(mod);
			Assert.Equal(expectedFile, mod[0].FileName);
			Assert.Equal(expectedFolder, mod[0].FolderName);

			return mod[0];
		}

		[Fact]
		public void ParseSingleLineComment()
		{
			CheckInParser myParser = new CheckInParser(EntryWithSingleLineComment(), new VssLocale(CultureInfo.InvariantCulture));
			Modification mod = new Modification();
			myParser.ParseComment(mod);
			Assert.Equal("added subfolder", mod.Comment);
		}

		[Fact]
		public void ParseMultiLineComment()
		{
            CheckInParser myParser = new CheckInParser(EntryWithMultiLineComment(), new VssLocale(CultureInfo.InvariantCulture));
			Modification mod = new Modification();
            myParser.ParseComment(mod);
			Assert.Equal(@"added subfolder
and then added a new line", mod.Comment);
		}

		[Fact]
		public void ParseEmptyComment()
		{
            CheckInParser myParser = new CheckInParser(EntryWithEmptyComment(), new VssLocale(CultureInfo.InvariantCulture));
			Modification mod = new Modification();
            myParser.ParseComment(mod);
			Assert.Equal(String.Empty, mod.Comment);
		}

		[Fact]
		public void ParseEmptyLineComment()
		{
            CheckInParser myParser = new CheckInParser(EntryWithEmptyCommentLine(), new VssLocale(CultureInfo.InvariantCulture));
			Modification mod = new Modification();
            myParser.ParseComment(mod);
			Assert.Equal(null, mod.Comment);
		}

		[Fact]
		public void ParseNoComment()
		{
            CheckInParser myParser = new CheckInParser(EntryWithNoCommentLine(), new VssLocale(CultureInfo.InvariantCulture));
			Modification mod = new Modification();
            myParser.ParseComment(mod);
			Assert.Equal(null, mod.Comment);
		}

		[Fact]
		public void ParseNonCommentAtCommentLine()
		{
            CheckInParser myParser = new CheckInParser(EntryWithNonCommentAtCommentLine(), new VssLocale(CultureInfo.InvariantCulture));
			Modification mod = new Modification();
            myParser.ParseComment(mod);
			Assert.Equal(null, mod.Comment);
		}
        [Fact(Skip = "")]
        //[Test, Ignore("later")]
		public void ParseCheckedInFileAndFolderWithLineBreaks()
		{
			string entry = @"*****  happyTheFile.txt  *****
Version 3
User: Admin        Date:  9/16/02   Time:  5:01p
Checked in $/you/want/" + '\n' + @"folders/i/got/em
Comment: added fir to tree file, checked in recursively from project root";

			string expectedFile = "happyTheFile.txt";
			string expectedFolder = "$/you/want/folders/i/got/em";

			Modification mod = ParseAndAssertFilenameAndFolder(entry, expectedFile, expectedFolder);
			Assert.Equal("Admin", mod.UserName);
			Assert.Equal(new DateTime(2002, 9, 16, 17, 01, 0), mod.ModifiedTime);
			Assert.Equal("Checked in", mod.Type);
			Assert.Equal("added fir to tree file, checked in recursively from project root",mod.Comment);
		}

        [Fact]
        public void ShouldBeAbleToCreateAllLocales()
        {
            // Extend this list when new translations are added.  Each entry is
            // "ll" or "ll-CC" where "ll" is the two-character langage code (e.g.,
            // "en" for English) and "CC" is the two-character country code (e.g.,
            // "US" for the United States).  There must be a "VssLocale.ll.resx" file
            // or a "VssLocal.ll-CC.resx" file. Also include "en" for the default
            // English locale and "" for the invariate locale.
            string[] localeNames = {"", "de", "en", "es", "fr", "ja"};
            foreach (string localeName in localeNames)
            {
                CultureInfo culture = new CultureInfo(localeName);
                VssLocale locale = new VssLocale(culture);
                Assert.NotNull(locale);
            }
        }

		private static string EntryWithSingleLineComment()
		{
            return @"*****  plant  *****
Version 1
User: Admin        Date:  9/16/02   Time:  2:41p
Created
Comment: added subfolder";
		}

		private static string EntryWithMultiLineComment()
		{
			return @"*****  plant  *****
Version 1
User: Admin        Date:  9/16/02   Time:  2:41p
Created
Comment: added subfolder
and then added a new line";
		}

		private static string EntryWithEmptyComment()
		{
return @"*****************  Version 1   *****************
User: Admin        Date:  9/16/02   Time:  2:29p
Created
Comment: 

";
		}

		private static string EntryWithEmptyCommentLine()
		{
return @"*****************  Version 2   *****************
User: Admin        Date:  9/16/02   Time:  2:40p
jam.txt added

";
		}

		private static string EntryWithNoCommentLine()
		{
return @"*****************  Version 2   *****************
User: Admin        Date:  9/16/02   Time:  2:40p
jam.txt added";
		}
			
		private static string EntryWithNonCommentAtCommentLine()
		{
return @"*****************  Version 2   *****************
User: Admin        Date:  9/16/02   Time:  2:40p
jam.txt added
booya, grandma, booya";
		}
	}
}
