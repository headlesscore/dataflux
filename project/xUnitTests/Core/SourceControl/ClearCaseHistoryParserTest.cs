using System;
using System.IO;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	
	public class ClearCaseHistoryParserTest 
	{
		ClearCaseHistoryParser parser;
        string path = Platform.IsWindows ? @"D:\CCase\ppunjani_view\RefArch\tutorial\wwhdata\common" : @"/CCase/ppunjani_view/RefArch/tutorial/wwhdata/common";

		// [SetUp]
		protected void Setup()
		{
			parser = new ClearCaseHistoryParser();
		}

		[Fact]
		public void CanTokenizeWithNoComment()
		{
			string[] tokens = parser.TokenizeEntry( @"ppunjani#~#Friday, September 27, 2002 06:31:36 PM#~#" + System.IO.Path.Combine(path, "context.js") + @"#~#\main\0#~#mkelem#~#!#~#!#~#" );
			Assert.Equal( 8, tokens.Length );
			Assert.Equal( "ppunjani", tokens[ 0 ] );
			Assert.Equal( "Friday, September 27, 2002 06:31:36 PM", tokens[ 1 ] );
			Assert.Equal( System.IO.Path.Combine(path, "context.js"), tokens[ 2 ] );
			Assert.Equal( @"\main\0", tokens[ 3 ] );
			Assert.Equal("mkelem", tokens[ 4 ] );
			Assert.Equal( "!", tokens[ 5 ] );
			Assert.Equal( "!", tokens[ 6 ] );
			Assert.Equal( string.Empty, tokens[ 7 ] );
            Assert.True(true);
        }

		[Fact]
		public void CanCreateNewModification()
		{
			const string userName = "gsmith";
			const string timeStamp = "Wednesday, March 10, 2004 08:52:05 AM";
			DateTime expectedTime = new DateTime( 2004, 03, 10, 08, 52, 05 );
			const string file = "context.js";
			string elementName = System.IO.Path.Combine(path, file);
			const string modificationType = "checkin";
			const string comment = "implemented dwim";
			const string change = @"\main\17";
			Modification modification = parser.CreateNewModification( userName,
				timeStamp,
				elementName,
				modificationType,
				comment,
				change );

			Assert.Equal( comment, modification.Comment );
			Assert.Null( modification.EmailAddress );
			Assert.Equal( file, modification.FileName );
			Assert.Equal( path, modification.FolderName );
			Assert.Equal( expectedTime, modification.ModifiedTime );
			Assert.Equal( modificationType, modification.Type );
			Assert.Null( modification.Url );
			Assert.Equal("\\main\\17", modification.ChangeNumber );
			Assert.Equal( userName, modification.UserName );
		}

		[Fact]
		public void CanAssignFileInfo()
		{
			const string file = "context.js";
			string fullPath = System.IO.Path.Combine(path, file);
			Modification modification = new Modification();

			parser.AssignFileInfo( modification, fullPath );

			Assert.True( path == modification.FolderName, "FolderName" );
			Assert.True( file == modification.FileName, "FileName" );
		}

		
		[Fact]
		public void CanAssignFileInfoWithNoPath()
		{
			const string file = "context.js";
			Modification modification = new Modification();

			parser.AssignFileInfo( modification, file );

			Assert.Equal( string.Empty, modification.FolderName );
			Assert.Equal( file, modification.FileName );
		}

		[Fact]
		public void CanAssignModificationTime()
		{
			const string time = "Friday, September 27, 2002 06:31:38 PM";
			Modification modification = new Modification();

			parser.AssignModificationTime( modification, time );

			Assert.Equal( new DateTime( 2002, 09, 27, 18, 31, 38 ), modification.ModifiedTime );
		}

		[Fact]
		public void CanAssignModificationTimeWithBadTime()
		{
			const string time = "not a valid time";
			Modification modification = new Modification();

			parser.AssignModificationTime( modification, time );

			Assert.Equal( new DateTime(), modification.ModifiedTime );
		}
		
		[Fact]
		public void IgnoresMkBranchEvent()
		{
			Modification modification = parser.ParseEntry( @"ppunjani#~#Friday, September 27, 2002 06:31:36 PM#~#" + System.IO.Path.Combine(path, "context.js") + @"#~#\main\0#~#mkbranch#~#!#~#!#~#" );
			Assert.Null( modification );
		}

		[Fact]
		public void IgnoresRmBranchEvent()
		{
			Modification modification = parser.ParseEntry( @"ppunjani#~#Friday, September 27, 2002 06:31:36 PM#~#" + System.IO.Path.Combine(path, "context.js") + @"#~#\main\0#~#rmbranch#~#!#~#!#~#" );
			Assert.Null( modification );
		}

		[Fact]
		public void CanParseBadEntry()
		{
			Modification modification = parser.ParseEntry( @"ppunjani#~#Tuesday, February 18, 2003 05:09:14 PM#~#" + System.IO.Path.Combine(path, "wwhpagef.js") + @"#~#\main\0#~#mkbranch#~#!#~#!#~#" );
			Assert.Null( modification );
		}

		[Fact]
		public void CanParse()
		{
			Modification[] mods = parser.Parse( ClearCaseMother.ContentReader, ClearCaseMother.OLDEST_ENTRY, ClearCaseMother.NEWEST_ENTRY);
			Assert.NotNull( mods );
			Assert.Equal( 28, mods.Length );			
		}

		[Fact]
		public void CanParseEntry()
		{
			Modification modification = parser.ParseEntry( @"ppunjani#~#Wednesday, November 20, 2002 07:37:22 PM#~#" + System.IO.Path.Combine(path, "towwhdir.js") + @"#~#\main#~#mkelem#~#!#~#!#~#" );
			Assert.Equal( "ppunjani", modification.UserName );
			Assert.Equal( new DateTime( 2002, 11, 20, 19, 37, 22), modification.ModifiedTime );
			Assert.Equal( path, modification.FolderName );
			Assert.Equal( "towwhdir.js", modification.FileName );
			Assert.Equal( "mkelem", modification.Type );
			Assert.Equal( "!", modification.ChangeNumber );
			Assert.Null( modification.Comment );
		}
		
		[Fact]
		public void CanParseEntryWithNoComment()
		{
			Modification modification = parser.ParseEntry( @"ppunjani#~#Wednesday, February 25, 2004 01:09:36 PM#~#" + System.IO.Path.Combine(path, "topics.js") + @"#~##~#**null operation kind**#~#!#~#!#~#" );
			Assert.Equal( "ppunjani", modification.UserName);
			Assert.Equal( new DateTime( 2004, 02, 25, 13, 09, 36 ), modification.ModifiedTime);
			Assert.Equal( path, modification.FolderName);
			Assert.Equal( "topics.js", modification.FileName);
			Assert.Equal( "**null operation kind**", modification.Type );
			Assert.Equal( "!", modification.ChangeNumber );
			Assert.Null( modification.Comment );
		}
		
		[Fact]
		public void CanTokenize()
		{
			string[] tokens = parser.TokenizeEntry( @"ppunjani#~#Friday, March 21, 2003 03:32:24 PM#~#" + System.IO.Path.Combine(path, "files.js") + @"#~##~#mkelem#~#!#~#!#~#made from flat file" );
			Assert.Equal( 8, tokens.Length );
			Assert.Equal( "ppunjani", tokens[0] );
			Assert.Equal( "Friday, March 21, 2003 03:32:24 PM", tokens[1] );
			Assert.Equal( System.IO.Path.Combine(path, "files.js"), tokens[2] );
			Assert.Equal( string.Empty, tokens[3] );
			Assert.Equal( "mkelem", tokens[4] );
			Assert.Equal( "!", tokens[5] );
			Assert.Equal( "!", tokens[6] );
			Assert.Equal( "made from flat file", tokens[7] );
		}

		[Fact]
		public void CanParseEntryWithNoLineBreakInComment()
		{
			Modification modification = parser.ParseEntry( @"ppunjani#~#Wednesday, November 20, 2002 07:37:22 PM#~#" + System.IO.Path.Combine(path, "towwhdir.js") + @"#~#\main#~#mkelem#~#!#~#!#~#simple comment" );
			Assert.Equal( "ppunjani", modification.UserName );
			Assert.Equal( new DateTime( 2002, 11, 20, 19, 37, 22), modification.ModifiedTime );
			Assert.Equal( path, modification.FolderName );
			Assert.Equal( "towwhdir.js", modification.FileName );
			Assert.Equal( "mkelem", modification.Type );
			Assert.Equal( "!", modification.ChangeNumber );
			Assert.Equal( "simple comment", modification.Comment );
		}

		[Fact]
		public void CanParseStreamWithNoLineBreakInComment()
		{
			string input = @"ppunjani#~#Wednesday, November 20, 2002 07:37:22 PM#~#" + System.IO.Path.Combine(path, "towwhdir.js") + @"#~#\main#~#mkelem#~#!#~#!#~#simple comment@#@#@#@#@#@#@#@#@#@#@#@";

			Modification modification = parser.Parse(new StringReader(input), DateTime.Now, DateTime.Now)[0];
			Assert.Equal( "ppunjani", modification.UserName );
			Assert.Equal( new DateTime( 2002, 11, 20, 19, 37, 22), modification.ModifiedTime );
			Assert.Equal( path, modification.FolderName );
			Assert.Equal( "towwhdir.js", modification.FileName );
			Assert.Equal( "mkelem", modification.Type );
			Assert.Equal( "!", modification.ChangeNumber );
			Assert.Equal( "simple comment", modification.Comment );
		}

		[Fact]
		public void CanParseStreamWithLineBreakInComment()
		{
			string input = @"ppunjani#~#Wednesday, November 20, 2002 07:37:22 PM#~#" + System.IO.Path.Combine(path, "towwhdir.js") + @"#~#\main#~#mkelem#~#!#~#!#~#simple comment 
with linebreak@#@#@#@#@#@#@#@#@#@#@#@";

			Modification modification = parser.Parse(new StringReader(input), DateTime.Now, DateTime.Now)[0];
			Assert.Equal( "ppunjani", modification.UserName );
			Assert.Equal( new DateTime( 2002, 11, 20, 19, 37, 22), modification.ModifiedTime );
			Assert.Equal( path, modification.FolderName );
			Assert.Equal( "towwhdir.js", modification.FileName );
			Assert.Equal( "mkelem", modification.Type );
			Assert.Equal( "!", modification.ChangeNumber );
			Assert.Equal( "simple comment with linebreak", modification.Comment );
		}

	}
}
