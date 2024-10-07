using System;
using System.Collections;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	[TestFixture]
	public class CvsHistoryParserTest : CustomAssertion
	{
		private CvsHistoryParser cvs = new CvsHistoryParser();

		[Test]
		public void ParseStream() 
		{
			TextReader input = new StringReader(CvsMother.CVS_LOGFILE_CONTENT);
			Modification[] modifications = cvs.Parse(input, CvsMother.OLDEST_ENTRY, CvsMother.NEWEST_ENTRY);
			
			ClassicAssert.AreEqual(6, modifications.Length);
            ClassicAssert.IsTrue(true);
            Modification mod1 = new Modification();
			mod1.Type = "modified";
			mod1.FileName = "log4j.properties";
			mod1.FolderName = "/cvsroot/cruisecontrol/cruisecontrol/main";
			mod1.ModifiedTime = CreateDate("2002/03/13 13:45:50 -6");
			mod1.UserName = "alden";
			mod1.Comment = "Shortening ConversionPattern so we don't use up all of the available screen space.";
			mod1.Version = "1.2";

			Modification mod2 = new Modification();
			mod2.Type = "modified";
			mod2.FileName = "build.xml";
			mod2.FolderName = "/cvsroot/cruisecontrol/cruisecontrol/main";
			mod2.ModifiedTime = CreateDate("2002/03/13 19:56:34 -6");
			mod2.UserName = "alden";
			mod2.Comment = "Added target to clean up test results.";
			mod2.Version = "1.41";

			Modification mod3 = new Modification();
			mod3.Type = "modified";
			mod3.FileName = "build.xml";
			mod3.FolderName = "/cvsroot/cruisecontrol/cruisecontrol/main";
			mod3.ModifiedTime = CreateDate("2002/03/15 13:20:28 -6");
			mod3.UserName = "alden";
			mod3.Comment = "enabled debug info when compiling tests.";
			mod3.Version = "1.42";

			Modification mod4 = new Modification();
			mod4.Type = "deleted";
			mod4.FileName = "kungfu.xml";
			mod4.FolderName = "/cvsroot/cruisecontrol/cruisecontrol/main";
			mod4.ModifiedTime = CreateDate("2002/03/13 13:45:42 -6");
			mod4.UserName = "alden";
			mod4.Comment = "Hey, look, a deleted file.";
			mod4.Version = "1.2";

			Modification mod5 = new Modification();
			mod5.Type = "deleted";
			mod5.FileName = "stuff.xml";
			mod5.FolderName = "/cvsroot/cruisecontrol/cruisecontrol/main";
			mod5.ModifiedTime = CreateDate("2002/03/13 13:38:42 -6");
			mod5.UserName = "alden";
			mod5.Comment = "Hey, look, another deleted file.";
			mod5.Version = "1.4";
		
			Modification mod6 = new Modification();
			mod6.Type = "modified";
			mod6.FileName = "File.cs";
			mod6.FolderName = "/cvsroot/cruisecontrol/cruisecontrol/Project";
			mod6.ModifiedTime = CreateDate("2005/09/12 15:01:10 +0");
			mod6.UserName = "szko";
			mod6.Comment = "Fixed some bugs.";
			mod6.Version = "1.11";

			ArrayList.Adapter(modifications).Sort();
			ClassicAssert.AreEqual(mod5, modifications[0]);
			ClassicAssert.AreEqual(mod4, modifications[1]);
			ClassicAssert.AreEqual(mod1, modifications[2]);
			ClassicAssert.AreEqual(mod2, modifications[3]);
			ClassicAssert.AreEqual(mod3, modifications[4]);
			ClassicAssert.AreEqual(mod6, modifications[5]);
		}

		[Test]
		public void ParseCvs112Examples() 
		{
			TextReader input = new StringReader(CvsMother.Cvs112Examples());
			Modification[] mods = cvs.Parse(input, CvsMother.OLDEST_ENTRY, CvsMother.NEWEST_ENTRY);
			ClassicAssert.AreEqual(2, mods.Length);
		}

		[Test]
		public void ParseCvsNTExamples() 
		{
			TextReader input = new StringReader(CvsMother.CvsNTExamples());
			Modification[] mods = cvs.Parse(input, CvsMother.OLDEST_ENTRY, CvsMother.NEWEST_ENTRY);
			ClassicAssert.AreEqual(2, mods.Length);
		}

		[Test]
		public void ParseExampleOfFileAddedOnBranch()
		{
			TextReader input = new StringReader(CvsMother.ExampleOfFileAddedOnBranch());
			Modification[] mods = cvs.Parse(input, CvsMother.OLDEST_ENTRY, CvsMother.NEWEST_ENTRY);
			ClassicAssert.AreEqual(0, mods.Length);			
		}

		[Test]
		public void VerifyModifiedFile() 
		{
			TextReader input = new StringReader(CvsMother.CvsModifiedFileExample());
			Modification[] mods = cvs.Parse(input, CvsMother.OLDEST_ENTRY, CvsMother.NEWEST_ENTRY);
			ClassicAssert.AreEqual(1, mods.Length);
			ClassicAssert.AreEqual("modified", mods[0].Type);
		}

		[Test]
		public void ParseRLog()
		{
			string output = @"RCS file: C:\dev\CVSRoot/fitwebservice/src/fitwebservice/src/Run.cs,v
head: 1.5
branch:
locks: strict
access list:
keyword substitution: kv
total revisions: 5;       selected revisions: 2
description:
----------------------------
revision 1.5
date: 2006/12/10 06:09:42;  author: owen;  state: Exp;  lines: +2 -0;  kopt: kv;  commitid: 1568457ba4a37557;  filename: Run.cs;
asdf
----------------------------
revision 1.4
date: 2006/12/10 06:07:25;  author: owen;  state: Exp;  lines: +0 -2;  kopt: kv;  commitid: d6c457ba41c739f;  filename: Run.cs;
foo
=============================================================================";
			Modification[] mods = cvs.Parse(new StringReader(output), new DateTime(2006, 12, 10), new DateTime(2006, 12, 11));
			ClassicAssert.AreEqual(2, mods.Length);
			ClassicAssert.AreEqual("1.5", mods[0].Version);
			ClassicAssert.AreEqual("owen", mods[0].UserName);
			ClassicAssert.AreEqual("Run.cs", mods[0].FileName);
			ClassicAssert.AreEqual("modified", mods[0].Type);
			ClassicAssert.AreEqual(@"C:\dev\CVSRoot/fitwebservice/src/fitwebservice/src", mods[0].FolderName);
		}

		[Test]
		public void ParseRLogWithSpacesInFilename()
		{
			string output = @"RCS file: C:\dev\CVSRoot/fitwebservice/src/fit webservice/src/Run File.cs,v
head: 1.5
branch:
locks: strict
access list:
keyword substitution: kv
total revisions: 5;       selected revisions: 2
description:
----------------------------
revision 1.5
date: 2006/12/10 06:09:42;  author: owen;  state: Exp;  lines: +2 -0;  kopt: kv;  commitid: 1568457ba4a37557;  filename: Run.cs;
asdf
=============================================================================";
			Modification[] mods = cvs.Parse(new StringReader(output), new DateTime(2006, 12, 10), new DateTime(2006, 12, 11));
			ClassicAssert.AreEqual(1, mods.Length);
			ClassicAssert.AreEqual("1.5", mods[0].Version);
			ClassicAssert.AreEqual("owen", mods[0].UserName);
			ClassicAssert.AreEqual("modified", mods[0].Type);
			ClassicAssert.AreEqual(@"C:\dev\CVSRoot/fitwebservice/src/fit webservice/src", mods[0].FolderName);
			ClassicAssert.AreEqual("Run File.cs", mods[0].FileName);
		}
		
		private DateTime CreateDate(string dateString) 
		{
			DateTime date = DateTime.ParseExact(dateString,"yyyy/MM/dd HH:mm:ss z",DateTimeFormatInfo.GetInstance(CultureInfo.InvariantCulture));
			return date;
		}
	}
}

