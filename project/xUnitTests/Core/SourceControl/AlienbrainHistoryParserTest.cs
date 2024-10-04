using System;
using System.IO;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	
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

		// [SetUp]
		protected void Setup()
		{
			parser = new AlienbrainHistoryParser();
		}

		[Fact]
		public void CanParseModifications()
		{
			Modification tokens = parser.ParseModification(SAMPLE_PARAMS);

			Assert.Equal(CHECKINCOMMENT, tokens.Comment);
            Assert.True(true);
            Assert.Equal(NAME, tokens.FileName);
			Assert.Equal(DBPATH_NOFILENAME, tokens.FolderName);
			Assert.Equal(DateTime.FromFileTime(SCIT), tokens.ModifiedTime);
			Assert.Equal(MIME_TYPE, tokens.Type);
			Assert.Equal(LOCALPATH, tokens.Url);
			Assert.Equal(CHANGED_BY, tokens.UserName);
			Assert.Equal(NXN_VERSIONNUMBER, tokens.Version);
		}

		[Fact]
		public void CanExtractParamsFromOneLine()
		{
			string[] tokens = parser.AllModificationParams(SAMPLE_ONE_LINES);

			Assert.Equal(8, tokens.Length);
			Assert.Equal(CHECKINCOMMENT, tokens[0]);
			Assert.Equal(NAME, tokens[1]);
			Assert.Equal(DBPATH, tokens[2]);
			Assert.Equal(SCIT.ToString(), tokens[3]);
			Assert.Equal(MIME_TYPE, tokens[4]);
			Assert.Equal(LOCALPATH, tokens[5]);
			Assert.Equal(CHANGED_BY, tokens[6]);
			Assert.Equal(NXN_VERSIONNUMBER, tokens[7]);
		}

		[Fact]
		public void CanExtractParamsFromMultipleLines()
		{
			Modification modification = parser.Parse(new StringReader(SAMPLE_TWO_LINES), DateTime.Now, DateTime.Now)[0];

			Assert.Equal(CHECKINCOMMENT, modification.Comment);
			Assert.Equal(NAME, modification.FileName);
			Assert.Equal(DBPATH_NOFILENAME, modification.FolderName);
			Assert.Equal(DateTime.FromFileTime(SCIT), modification.ModifiedTime);
			Assert.Equal(MIME_TYPE, modification.Type);
			Assert.Equal(LOCALPATH, modification.Url);
			Assert.Equal(CHANGED_BY, modification.UserName);
			Assert.Equal(NXN_VERSIONNUMBER, modification.Version);
		}

		[Fact]
		public void MustReturnNoModificationIfNoChange()
		{
			Modification[] modification = parser.Parse(new StringReader(SAMPLE_NO_CHANGES), DateTime.Now, DateTime.Now);
			Assert.Equal(new Modification[0], modification);
		}
	}
}
