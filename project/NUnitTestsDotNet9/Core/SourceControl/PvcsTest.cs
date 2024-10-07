using System;
using System.Globalization;
using System.IO;
using Exortech.NetReflector;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	[TestFixture]
	public class PvcsTest : CustomAssertion
	{
		private Mock<IHistoryParser> mockParser;
		private Mock<ProcessExecutor> mockExecutor;
		private Pvcs pvcs;

		[SetUp]
		protected void CreatePvcs()
		{
			mockParser = new Mock<IHistoryParser>();
			mockExecutor = new Mock<ProcessExecutor>();
			pvcs = new Pvcs((IHistoryParser) mockParser.Object, (ProcessExecutor) mockExecutor.Object);
		}

		[TearDown]
		protected void VerifyMocks()
		{
			mockParser.Verify();
			mockExecutor.Verify();
		}

		[Test]
		public void ValuePopulation()
		{
			string xml = @"    <sourceControl type=""pvcs"">
      <executable>..\etc\pvcs\mockpcli.bat</executable>
	  <project>fooproject</project>
	  <subproject>barsub</subproject>
    </sourceControl>
	";

			NetReflector.Read(xml, pvcs);

			ClassicAssert.AreEqual(@"..\etc\pvcs\mockpcli.bat", pvcs.Executable);
			ClassicAssert.AreEqual("fooproject", pvcs.Project);
			ClassicAssert.AreEqual("barsub", pvcs.Subproject);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

		// Daylight savings time bug
		// This was necessary to resolve a bug with PVCS 7.5.1 (would not properly
		// detect modifications during periods where daylight savings was active)
		[Test]
		public void AdjustForDayLightSavingsBugDuringDayLightSavings()
		{
			TimeZone timeZoneWhereItIsAlwaysDayLightSavings = CreateMockTimeZone(true);
			pvcs.CurrentTimeZone = timeZoneWhereItIsAlwaysDayLightSavings;

			DateTime date = new DateTime(2000, 1, 1, 1, 0, 0);
			DateTime anHourBefore = new DateTime(2000, 1, 1, 0, 0, 0);
			ClassicAssert.AreEqual(anHourBefore, pvcs.AdjustForDayLightSavingsBug(date));
		}

		[Test]
		public void AdjustForDayLightSavingsBugOutsideDayLightSavings()
		{
			TimeZone timeZoneWhereItIsNeverDayLightSavings = CreateMockTimeZone(false);
			pvcs.CurrentTimeZone = timeZoneWhereItIsNeverDayLightSavings;

			DateTime date = new DateTime(2000, 1, 1, 1, 0, 0);
			ClassicAssert.AreEqual(date, pvcs.AdjustForDayLightSavingsBug(date));
		}

		[Test]
		public void VerifyDateStringParser()
		{
			DateTime expected = new DateTime(2005, 05, 01, 12, 0, 0, 0);
			DateTime actual = Pvcs.GetDate("May 1 2005 12:00:00", CultureInfo.InvariantCulture);
			ClassicAssert.AreEqual(expected, actual);

			expected = new DateTime(2005, 10, 01, 15, 0, 0, 0);
			actual = Pvcs.GetDate("Oct 1 2005 15:00:00", CultureInfo.InvariantCulture);
			ClassicAssert.AreEqual(expected, actual);
		}

		[Test]
		public void VerifyDateParser()
		{
			string expected = "10/31/2001 18:52";
			string actual = Pvcs.GetDateString(new DateTime(2001, 10, 31, 18, 52, 13), CultureInfo.InvariantCulture.DateTimeFormat);
			ClassicAssert.AreEqual(expected, actual);
		}

		[Test]
		public void CreatePcliContentsForGettingVLog()
		{
			string expected = "run -xe\"" + pvcs.ErrorFile + "\" -xo\"" + pvcs.LogFile + "\" -q vlog -pr\"" + pvcs.Project + "\"  -z -ds\"beforedate\" -de\"afterdate\" " + pvcs.Subproject;
			string actual = pvcs.CreatePcliContentsForCreatingVLog("beforedate", "afterdate");
			ClassicAssert.AreEqual(expected, actual);
		}

		[Test]
		public void CreatePcliContentsForLabeling()
		{
			string expected = "Vcs -q -xo\"" + pvcs.LogFile + "\" -xe\"" + pvcs.ErrorFile + "\"  -v\"temp\" \"@" + pvcs.TempFile + "\"";
			string actual = pvcs.CreatePcliContentsForLabeling("temp");
			ClassicAssert.AreEqual(expected, actual);
		}

		[Test]
		public void GetModifications()
		{
			mockExecutor.Setup(executor => executor.Execute(It.IsAny<ProcessInfo>())).Returns(ProcessResultFixture.CreateSuccessfulResult()).Verifiable();
			mockParser.Setup(parser => parser.Parse(It.IsAny<TextReader>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(new Modification[] { new Modification(), new Modification() }).Verifiable();

			Modification[] mods = pvcs.GetModifications(IntegrationResultMother.CreateSuccessful(new DateTime(2004, 6, 1, 1, 1, 1)), 
				IntegrationResultMother.CreateSuccessful(new DateTime(2004, 6, 1, 2, 2, 2)));
			ClassicAssert.AreEqual(2, mods.Length);
		}

		[Test]
		public void CreateIndividualGetString()
		{
			string expected = "-r1.0 \"fooproject\\archives\\test\\myfile.txt-arc\"(\"c:\\source\\test\") ";
			Modification mod = new Modification();
			mod.Version = "1.0";
			mod.FolderName = @"fooproject\archives\test";
			mod.FileName = "myfile.txt-arc";
			string actual = pvcs.CreateIndividualGetString(mod, @"c:\source\test");
			ClassicAssert.AreEqual(expected, actual);
		}

		[Test]
		public void CreateIndividualLabelString()
		{
			string expected = @"-vTestLabel ""fooproject\archives\test\myfile.txt-arc"" ";
			Modification mod = new Modification();
			mod.Version = "1.0";
			mod.FolderName = @"fooproject\archives\test";
			mod.FileName = "myfile.txt-arc";
			string actual = pvcs.CreateIndividualLabelString(mod,"TestLabel");
			ClassicAssert.AreEqual(expected, actual);
		}

		[Test]
		public void GetLoginIdStringWithoutDoubleQuotes()
		{
			pvcs.Username = "foo";
			pvcs.Password = "bar";
			ClassicAssert.AreEqual(" -id\"foo\":\"bar\" ", pvcs.GetLogin(false));
		}

		[Test]
		public void GetLoginIdStringWithDoubleQuotes()
		{
			pvcs.Username = "foo";
			pvcs.Password = "bar";
			ClassicAssert.AreEqual(" \"\"-id\"foo\":\"bar\"\"\" ", pvcs.GetLogin(true));
		}

        [Test]
        public void GetLoginIdStringWithoutPassword()
        {
            pvcs.Username = "foo";
            pvcs.Password = "";
            ClassicAssert.AreEqual(" -id\"foo\" ", pvcs.GetLogin(false));
        }

		[Test]
		public void GetExeFilenameShouldNotBeRootedIfPathIsNotSpecified()
		{
			ClassicAssert.AreEqual("Get.exe", pvcs.GetExeFilename());
		}

		private TimeZone CreateMockTimeZone(bool inDayLightSavings)
		{
			var mock = new Mock<TimeZone>();
			mock.Setup(timeZone => timeZone.IsDaylightSavingTime(It.IsAny<DateTime>())).Returns(inDayLightSavings).Verifiable();
			return (TimeZone) mock.Object;
		}
	}
}
