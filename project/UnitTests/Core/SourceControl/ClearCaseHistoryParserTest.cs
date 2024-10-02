using System;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	[TestFixture]
	public class ClearCaseHistoryParserTest 
	{
		ClearCaseHistoryParser parser;
        string path = Platform.IsWindows ? @"D:\CCase\ppunjani_view\RefArch\tutorial\wwhdata\common" : @"/CCase/ppunjani_view/RefArch/tutorial/wwhdata/common";

		[SetUp]
		protected void Setup()
		{
			parser = new ClearCaseHistoryParser();
		}

		[Test]
		public void CanTokenizeWithNoComment()
		{
			string[] tokens = parser.TokenizeEntry( @"ppunjani#~#Friday, September 27, 2002 06:31:36 PM#~#" + System.IO.Path.Combine(path, "context.js") + @"#~#\main\0#~#mkelem#~#!#~#!#~#" );
			ClassicAssert.AreEqual( 8, tokens.Length );
			ClassicAssert.AreEqual( "ppunjani", tokens[ 0 ] );
			ClassicAssert.AreEqual( "Friday, September 27, 2002 06:31:36 PM", tokens[ 1 ] );
			ClassicAssert.AreEqual( System.IO.Path.Combine(path, "context.js"), tokens[ 2 ] );
			ClassicAssert.AreEqual( @"\main\0", tokens[ 3 ] );
			ClassicAssert.AreEqual("mkelem", tokens[ 4 ] );
			ClassicAssert.AreEqual( "!", tokens[ 5 ] );
			ClassicAssert.AreEqual( "!", tokens[ 6 ] );
			ClassicAssert.AreEqual( string.Empty, tokens[ 7 ] );
            ClassicAssert.IsTrue(true);
        }

		[Test]
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

			ClassicAssert.AreEqual( comment, modification.Comment );
			ClassicAssert.IsNull( modification.EmailAddress );
			ClassicAssert.AreEqual( file, modification.FileName );
			ClassicAssert.AreEqual( path, modification.FolderName );
			ClassicAssert.AreEqual( expectedTime, modification.ModifiedTime );
			ClassicAssert.AreEqual( modificationType, modification.Type );
			ClassicAssert.IsNull( modification.Url );
			ClassicAssert.AreEqual("\\main\\17", modification.ChangeNumber );
			ClassicAssert.AreEqual( userName, modification.UserName );
		}

		[Test]
		public void CanAssignFileInfo()
		{
			const string file = "context.js";
			string fullPath = System.IO.Path.Combine(path, file);
			Modification modification = new Modification();

			parser.AssignFileInfo( modification, fullPath );

			ClassicAssert.AreEqual( path, modification.FolderName, "FolderName" );
			ClassicAssert.AreEqual( file, modification.FileName, "FileName" );
		}

		
		[Test]
		public void CanAssignFileInfoWithNoPath()
		{
			const string file = "context.js";
			Modification modification = new Modification();

			parser.AssignFileInfo( modification, file );

			ClassicAssert.AreEqual( string.Empty, modification.FolderName );
			ClassicAssert.AreEqual( file, modification.FileName );
		}

		[Test]
		public void CanAssignModificationTime()
		{
			const string time = "Friday, September 27, 2002 06:31:38 PM";
			Modification modification = new Modification();

			parser.AssignModificationTime( modification, time );

			ClassicAssert.AreEqual( new DateTime( 2002, 09, 27, 18, 31, 38 ), modification.ModifiedTime );
		}

		[Test]
		public void CanAssignModificationTimeWithBadTime()
		{
			const string time = "not a valid time";
			Modification modification = new Modification();

			parser.AssignModificationTime( modification, time );

			ClassicAssert.AreEqual( new DateTime(), modification.ModifiedTime );
		}
		
		[Test]
		public void IgnoresMkBranchEvent()
		{
			Modification modification = parser.ParseEntry( @"ppunjani#~#Friday, September 27, 2002 06:31:36 PM#~#" + System.IO.Path.Combine(path, "context.js") + @"#~#\main\0#~#mkbranch#~#!#~#!#~#" );
			ClassicAssert.IsNull( modification );
		}

		[Test]
		public void IgnoresRmBranchEvent()
		{
			Modification modification = parser.ParseEntry( @"ppunjani#~#Friday, September 27, 2002 06:31:36 PM#~#" + System.IO.Path.Combine(path, "context.js") + @"#~#\main\0#~#rmbranch#~#!#~#!#~#" );
			ClassicAssert.IsNull( modification );
		}

		[Test]
		public void CanParseBadEntry()
		{
			Modification modification = parser.ParseEntry( @"ppunjani#~#Tuesday, February 18, 2003 05:09:14 PM#~#" + System.IO.Path.Combine(path, "wwhpagef.js") + @"#~#\main\0#~#mkbranch#~#!#~#!#~#" );
			ClassicAssert.IsNull( modification );
		}

		[Test]
		public void CanParse()
		{
			Modification[] mods = parser.Parse( ClearCaseMother.ContentReader, ClearCaseMother.OLDEST_ENTRY, ClearCaseMother.NEWEST_ENTRY);
			ClassicAssert.IsNotNull( mods, "mods should not be null" );
			ClassicAssert.AreEqual( 28, mods.Length );			
		}

		[Test]
		public void CanParseEntry()
		{
			Modification modification = parser.ParseEntry( @"ppunjani#~#Wednesday, November 20, 2002 07:37:22 PM#~#" + System.IO.Path.Combine(path, "towwhdir.js") + @"#~#\main#~#mkelem#~#!#~#!#~#" );
			ClassicAssert.AreEqual( "ppunjani", modification.UserName );
			ClassicAssert.AreEqual( new DateTime( 2002, 11, 20, 19, 37, 22), modification.ModifiedTime );
			ClassicAssert.AreEqual( path, modification.FolderName );
			ClassicAssert.AreEqual( "towwhdir.js", modification.FileName );
			ClassicAssert.AreEqual( "mkelem", modification.Type );
			ClassicAssert.AreEqual( "!", modification.ChangeNumber );
			ClassicAssert.IsNull( modification.Comment );
		}
		
		[Test]
		public void CanParseEntryWithNoComment()
		{
			Modification modification = parser.ParseEntry( @"ppunjani#~#Wednesday, February 25, 2004 01:09:36 PM#~#" + System.IO.Path.Combine(path, "topics.js") + @"#~##~#**null operation kind**#~#!#~#!#~#" );
			ClassicAssert.AreEqual( "ppunjani", modification.UserName);
			ClassicAssert.AreEqual( new DateTime( 2004, 02, 25, 13, 09, 36 ), modification.ModifiedTime);
			ClassicAssert.AreEqual( path, modification.FolderName);
			ClassicAssert.AreEqual( "topics.js", modification.FileName);
			ClassicAssert.AreEqual( "**null operation kind**", modification.Type );
			ClassicAssert.AreEqual( "!", modification.ChangeNumber );
			ClassicAssert.IsNull( modification.Comment );
		}
		
		[Test]
		public void CanTokenize()
		{
			string[] tokens = parser.TokenizeEntry( @"ppunjani#~#Friday, March 21, 2003 03:32:24 PM#~#" + System.IO.Path.Combine(path, "files.js") + @"#~##~#mkelem#~#!#~#!#~#made from flat file" );
			ClassicAssert.AreEqual( 8, tokens.Length );
			ClassicAssert.AreEqual( "ppunjani", tokens[0] );
			ClassicAssert.AreEqual( "Friday, March 21, 2003 03:32:24 PM", tokens[1] );
			ClassicAssert.AreEqual( System.IO.Path.Combine(path, "files.js"), tokens[2] );
			ClassicAssert.AreEqual( string.Empty, tokens[3] );
			ClassicAssert.AreEqual( "mkelem", tokens[4] );
			ClassicAssert.AreEqual( "!", tokens[5] );
			ClassicAssert.AreEqual( "!", tokens[6] );
			ClassicAssert.AreEqual( "made from flat file", tokens[7] );
		}

		[Test]
		public void CanParseEntryWithNoLineBreakInComment()
		{
			Modification modification = parser.ParseEntry( @"ppunjani#~#Wednesday, November 20, 2002 07:37:22 PM#~#" + System.IO.Path.Combine(path, "towwhdir.js") + @"#~#\main#~#mkelem#~#!#~#!#~#simple comment" );
			ClassicAssert.AreEqual( "ppunjani", modification.UserName );
			ClassicAssert.AreEqual( new DateTime( 2002, 11, 20, 19, 37, 22), modification.ModifiedTime );
			ClassicAssert.AreEqual( path, modification.FolderName );
			ClassicAssert.AreEqual( "towwhdir.js", modification.FileName );
			ClassicAssert.AreEqual( "mkelem", modification.Type );
			ClassicAssert.AreEqual( "!", modification.ChangeNumber );
			ClassicAssert.AreEqual( "simple comment", modification.Comment );
		}

		[Test]
		public void CanParseStreamWithNoLineBreakInComment()
		{
			string input = @"ppunjani#~#Wednesday, November 20, 2002 07:37:22 PM#~#" + System.IO.Path.Combine(path, "towwhdir.js") + @"#~#\main#~#mkelem#~#!#~#!#~#simple comment@#@#@#@#@#@#@#@#@#@#@#@";

			Modification modification = parser.Parse(new StringReader(input), DateTime.Now, DateTime.Now)[0];
			ClassicAssert.AreEqual( "ppunjani", modification.UserName );
			ClassicAssert.AreEqual( new DateTime( 2002, 11, 20, 19, 37, 22), modification.ModifiedTime );
			ClassicAssert.AreEqual( path, modification.FolderName );
			ClassicAssert.AreEqual( "towwhdir.js", modification.FileName );
			ClassicAssert.AreEqual( "mkelem", modification.Type );
			ClassicAssert.AreEqual( "!", modification.ChangeNumber );
			ClassicAssert.AreEqual( "simple comment", modification.Comment );
		}

		[Test]
		public void CanParseStreamWithLineBreakInComment()
		{
			string input = @"ppunjani#~#Wednesday, November 20, 2002 07:37:22 PM#~#" + System.IO.Path.Combine(path, "towwhdir.js") + @"#~#\main#~#mkelem#~#!#~#!#~#simple comment 
with linebreak@#@#@#@#@#@#@#@#@#@#@#@";

			Modification modification = parser.Parse(new StringReader(input), DateTime.Now, DateTime.Now)[0];
			ClassicAssert.AreEqual( "ppunjani", modification.UserName );
			ClassicAssert.AreEqual( new DateTime( 2002, 11, 20, 19, 37, 22), modification.ModifiedTime );
			ClassicAssert.AreEqual( path, modification.FolderName );
			ClassicAssert.AreEqual( "towwhdir.js", modification.FileName );
			ClassicAssert.AreEqual( "mkelem", modification.Type );
			ClassicAssert.AreEqual( "!", modification.ChangeNumber );
			ClassicAssert.AreEqual( "simple comment with linebreak", modification.Comment );
		}

	}
}
