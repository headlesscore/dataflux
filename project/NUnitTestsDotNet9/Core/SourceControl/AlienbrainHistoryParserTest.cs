using System;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	[TestFixture]
	public class AlienbrainHistoryParserTest
	{
		private AlienbrainHistoryParser parser;

		// #CheckInComment#|#Name#|#DbPath#|#SCIT#|#Mime Type#|#LocalPath#|#Changed By#|#NxN_VersionNumber#
		private const string CHECKINCOMMENT = "Fixed Crash";
		private const string NAME = "UnNativeScript.cpp";
		private const string DBPATH = "/Code/Core/Src/UnNativeScript.cpp";
		private const string DBPATH_NOFILENAME = "ab://Code/Core/Src";
		private const long SCIT = 127771952139476549;
		private const string MIME_TYPE = "C++ Implementation File";
		private const string LOCALPATH = @"d:\project\code\Core\Src\UnNativeScript.cpp";
		private const string CHANGED_BY = "luke";
		private const string NXN_VERSIONNUMBER = "6";

		// #CheckInComment#|#Name#|#DbPath#|#SCIT#|#Mime Type#|#LocalPath#|#Changed By#|#NxN_VersionNumber#
		private const string COMMAND_OUTPUT_FORMAT = "{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}\r\n";

		private static string[] SAMPLE_PARAMS = {CHECKINCOMMENT, NAME, DBPATH, SCIT.ToString(), MIME_TYPE, LOCALPATH, CHANGED_BY, NXN_VERSIONNUMBER};

		private static string SAMPLE_ONE_LINES = string.Format(COMMAND_OUTPUT_FORMAT, CHECKINCOMMENT, NAME, DBPATH, SCIT.ToString(), MIME_TYPE,
		                                                       LOCALPATH, CHANGED_BY, NXN_VERSIONNUMBER);

		private static string SAMPLE_TWO_LINES = string.Format(System.Globalization.CultureInfo.CurrentCulture,"{0}\n{1}", SAMPLE_ONE_LINES, SAMPLE_ONE_LINES);
		private static string SAMPLE_NO_CHANGES = string.Format(System.Globalization.CultureInfo.CurrentCulture,"{0}\n{1}",string.Empty, Alienbrain.NO_CHANGE);

		[SetUp]
		protected void Setup()
		{
			parser = new AlienbrainHistoryParser();
		}

		[Test]
		public void CanParseModifications()
		{
			Modification tokens = parser.ParseModification(SAMPLE_PARAMS);

			ClassicAssert.AreEqual(CHECKINCOMMENT, tokens.Comment);
            ClassicAssert.IsTrue(true);
            ClassicAssert.AreEqual(NAME, tokens.FileName);
			ClassicAssert.AreEqual(DBPATH_NOFILENAME, tokens.FolderName);
			ClassicAssert.AreEqual(DateTime.FromFileTime(SCIT), tokens.ModifiedTime);
			ClassicAssert.AreEqual(MIME_TYPE, tokens.Type);
			ClassicAssert.AreEqual(LOCALPATH, tokens.Url);
			ClassicAssert.AreEqual(CHANGED_BY, tokens.UserName);
			ClassicAssert.AreEqual(NXN_VERSIONNUMBER, tokens.Version);
		}

		[Test]
		public void CanExtractParamsFromOneLine()
		{
			string[] tokens = parser.AllModificationParams(SAMPLE_ONE_LINES);

			ClassicAssert.AreEqual(8, tokens.Length);
			ClassicAssert.AreEqual(CHECKINCOMMENT, tokens[0]);
			ClassicAssert.AreEqual(NAME, tokens[1]);
			ClassicAssert.AreEqual(DBPATH, tokens[2]);
			ClassicAssert.AreEqual(SCIT.ToString(), tokens[3]);
			ClassicAssert.AreEqual(MIME_TYPE, tokens[4]);
			ClassicAssert.AreEqual(LOCALPATH, tokens[5]);
			ClassicAssert.AreEqual(CHANGED_BY, tokens[6]);
			ClassicAssert.AreEqual(NXN_VERSIONNUMBER, tokens[7]);
		}

		[Test]
		public void CanExtractParamsFromMultipleLines()
		{
			Modification modification = parser.Parse(new StringReader(SAMPLE_TWO_LINES), DateTime.Now, DateTime.Now)[0];

			ClassicAssert.AreEqual(CHECKINCOMMENT, modification.Comment);
			ClassicAssert.AreEqual(NAME, modification.FileName);
			ClassicAssert.AreEqual(DBPATH_NOFILENAME, modification.FolderName);
			ClassicAssert.AreEqual(DateTime.FromFileTime(SCIT), modification.ModifiedTime);
			ClassicAssert.AreEqual(MIME_TYPE, modification.Type);
			ClassicAssert.AreEqual(LOCALPATH, modification.Url);
			ClassicAssert.AreEqual(CHANGED_BY, modification.UserName);
			ClassicAssert.AreEqual(NXN_VERSIONNUMBER, modification.Version);
		}

		[Test]
		public void MustReturnNoModificationIfNoChange()
		{
			Modification[] modification = parser.Parse(new StringReader(SAMPLE_NO_CHANGES), DateTime.Now, DateTime.Now);
			ClassicAssert.AreEqual(new Modification[0], modification);
		}
	}
}
