using System;
using System.Collections;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol.Telelogic;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol.Telelogic
{
	[TestFixture]
	public sealed class SynergyParserTest
	{
		private SynergyConnectionInfo connection;
		private SynergyProjectInfo project;

		[OneTimeSetUp]
		public void TestFixtureSetUp()
		{
			connection = new SynergyConnectionInfo();
			connection.Host = "localhost";
			connection.Database = @"\\server\share\mydb";
			connection.Delimiter = '-';
			project = new SynergyProjectInfo();
			project.ProjectSpecification = "MyProject-MyProject_Int";
		}

		[Test]
		public void ConnectionDefaults()
		{
			SynergyConnectionInfo actual = new SynergyConnectionInfo();

			ClassicAssert.IsNull(actual.Database, "#A1");
			ClassicAssert.IsNull(actual.SessionId, "#A2");
			ClassicAssert.AreEqual(3600, actual.Timeout, "#A3");
			ClassicAssert.AreEqual("ccm.exe", actual.Executable, "#A4");
			ClassicAssert.AreEqual(Environment.ExpandEnvironmentVariables("%USERNAME%"), actual.Username, "#A5");
			ClassicAssert.AreEqual("build_mgr", actual.Role, "#A6");
			ClassicAssert.AreEqual('-', actual.Delimiter, "#A7");
			ClassicAssert.AreEqual(Environment.ExpandEnvironmentVariables(@"%SystemDrive%\cmsynergy\%USERNAME%"), actual.HomeDirectory, "#A8");
			ClassicAssert.AreEqual(Environment.ExpandEnvironmentVariables(@"%SystemDrive%\cmsynergy\uidb"), actual.ClientDatabaseDirectory, "#A9");
			ClassicAssert.AreEqual(Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Telelogic\CM Synergy 6.3\bin"), actual.WorkingDirectory, "#A10");
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

		[Test]
		public void ProjectDefaults()
		{
			SynergyProjectInfo actual = new SynergyProjectInfo();

			ClassicAssert.IsNull(actual.Release);
			ClassicAssert.IsNull(actual.ProjectSpecification);
			ClassicAssert.AreEqual(0, actual.TaskFolder);
			ClassicAssert.AreEqual(DateTime.MinValue, actual.LastReconfigureTime);
			ClassicAssert.IsFalse(actual.BaseliningEnabled);
			ClassicAssert.AreEqual("Integration Testing", actual.Purpose);
		}

		[Test]
		public void CanParseNewTasks()
		{
			SynergyParser parser = new SynergyParser();

			// ngw_de0157~milligan_integrate
			Hashtable actual = parser.ParseTasks(SynergyMother.NewTaskInfo);

			// validate that a collection of 8 comments is returned
			ClassicAssert.IsNotNull(actual);
			ClassicAssert.AreEqual(6, actual.Count);

			// validate that each comment and timestamp exists, and defaults to String.Empty
			foreach (DictionaryEntry comment in actual)
			{
				ClassicAssert.IsNotNull(comment);
				SynergyParser.SynergyTaskInfo info = (SynergyParser.SynergyTaskInfo) comment.Value;
				ClassicAssert.IsNotNull(info.TaskNumber);
				ClassicAssert.IsNotNull(info.TaskSynopsis);
				ClassicAssert.IsNotNull(info.Resolver);
			}

			// test that the right comments are returned, and that the order of retrieval
			// does not matter
			if (null != actual["15"])
			{
				ClassicAssert.AreEqual("lorem ipsum dolerem ", ((SynergyParser.SynergyTaskInfo) actual["15"]).TaskSynopsis);
				ClassicAssert.AreEqual("Insulated Development projects for release PRODUCT/1.0", ((SynergyParser.SynergyTaskInfo) actual["22"]).TaskSynopsis);
				ClassicAssert.AreEqual("jdoe's Insulated Development projects", ((SynergyParser.SynergyTaskInfo) actual["21"]).TaskSynopsis);
				ClassicAssert.AreEqual("IGNORE THIS Sample Task ", ((SynergyParser.SynergyTaskInfo) actual["99"]).TaskSynopsis);
				ClassicAssert.AreEqual("the quick brown fox jumped over the lazy dog ", ((SynergyParser.SynergyTaskInfo) actual["17"]).TaskSynopsis);
				ClassicAssert.AreEqual("0123456789 ~!@#$%^&*()_=", ((SynergyParser.SynergyTaskInfo) actual["1"]).TaskSynopsis);
			}
			else
			{
				ClassicAssert.AreEqual("lorem ipsum dolerem ", ((SynergyParser.SynergyTaskInfo) actual["wwdev#15"]).TaskSynopsis);
				ClassicAssert.AreEqual("Insulated Development projects for release PRODUCT/1.0", ((SynergyParser.SynergyTaskInfo) actual["wwdev#22"]).TaskSynopsis);
				ClassicAssert.AreEqual("jdoe's Insulated Development projects", ((SynergyParser.SynergyTaskInfo) actual["wwdev#21"]).TaskSynopsis);
				ClassicAssert.AreEqual("IGNORE THIS Sample Task ", ((SynergyParser.SynergyTaskInfo) actual["wwdev#99"]).TaskSynopsis);
				ClassicAssert.AreEqual("the quick brown fox jumped over the lazy dog ", ((SynergyParser.SynergyTaskInfo) actual["wwdev#17"]).TaskSynopsis);
				ClassicAssert.AreEqual("0123456789 ~!@#$%^&*()_=", ((SynergyParser.SynergyTaskInfo) actual["wwdev#1"]).TaskSynopsis);
			}

			// assert that tasks not in the original list are null
			ClassicAssert.IsNull(actual["123456789"]);
		}

		[Test]
		public void ParseNewObjects()
		{
			ParseNewObjects(SynergyMother.NewTaskInfo, SynergyMother.NewObjects);
		}

		[Test]
		public void ParseDCMObjects()
		{
			ParseNewObjects(SynergyMother.NewDcmTaskInfo, SynergyMother.NewDCMObjects);
		}

		[Test]
		public void ParseWhenTasksAreEmpty()
		{
			SynergyParser parser = new SynergyParser();
			// set the from date to be one week back
			DateTime from = DateTime.Now.AddDays(-7L);

			Modification[] actual = parser.Parse(string.Empty, SynergyMother.NewObjects, from);
			ClassicAssert.AreEqual(7, actual.Length);
			ClassicAssert.AreEqual("15", actual[0].ChangeNumber);
			ClassicAssert.AreEqual("9999", actual[6].ChangeNumber);
		}

		private void ParseNewObjects(string newTasks, string newObjects)
		{
			SynergyParser parser = new SynergyParser();
			// set the from date to be one week back
			DateTime from = DateTime.Now.AddDays(-7L);

			Modification[] actual = parser.Parse(newTasks, newObjects, from);

			ClassicAssert.IsNotNull(actual);
			ClassicAssert.AreEqual(7, actual.Length);

			foreach (Modification modification in actual)
			{
				ClassicAssert.AreEqual("jdoe", modification.EmailAddress);
				ClassicAssert.AreEqual("jdoe", modification.UserName);
				ClassicAssert.IsNull(modification.Url);
			}

			ClassicAssert.AreEqual("15", actual[0].ChangeNumber);
			ClassicAssert.AreEqual(@"sourcecontrol-3", actual[0].FileName);
			ClassicAssert.AreEqual(@"$/MyProject/CruiseControl.NET/project/core", actual[0].FolderName);
			ClassicAssert.AreEqual(@"dir", actual[0].Type);
			ClassicAssert.AreEqual(@"lorem ipsum dolerem ", actual[0].Comment);

			// test that the last task number is used when an object is associated with multiple tasks
			ClassicAssert.AreEqual("21", actual[1].ChangeNumber);
			ClassicAssert.AreEqual(@"Synergy.cs-1", actual[1].FileName);
			ClassicAssert.AreEqual(@"$/MyProject/CruiseControl.NET/project/core/sourcecontrol", actual[1].FolderName);
			ClassicAssert.AreEqual(@"ms_cs", actual[1].Type);
			// check that trailing spaces are honored
			ClassicAssert.AreEqual("jdoe's Insulated Development projects", actual[1].Comment);

			ClassicAssert.AreEqual("22", actual[2].ChangeNumber);
			// check that branched version numbers are parsed
			ClassicAssert.AreEqual(@"SynergyCommandBuilder.cs-1.1.1", actual[2].FileName);
			ClassicAssert.AreEqual(@"$/MyProject/CruiseControl.NET/project/core/sourcecontrol", actual[2].FolderName);
			ClassicAssert.AreEqual(@"ms_cs", actual[2].Type);
			ClassicAssert.AreEqual("Insulated Development projects for release PRODUCT/1.0", actual[2].Comment);

			ClassicAssert.AreEqual("22", actual[3].ChangeNumber);
			ClassicAssert.AreEqual(@"SynergyConnectionInfo.cs-2", actual[3].FileName);
			ClassicAssert.AreEqual(@"$/MyProject/CruiseControl.NET/project/core/sourcecontrol", actual[3].FolderName);
			ClassicAssert.AreEqual(@"ms_cs", actual[3].Type);
			// check that trailing spaces are honored
			ClassicAssert.AreEqual("Insulated Development projects for release PRODUCT/1.0", actual[3].Comment);

			ClassicAssert.AreEqual("1", actual[4].ChangeNumber);
			// check that branched version numbers are parsed
			ClassicAssert.AreEqual(@"SynergyHistoryParser.cs-2.2.1", actual[4].FileName);
			ClassicAssert.AreEqual(@"$/MyProject/CruiseControl.NET/project/core/sourcecontrol", actual[4].FolderName);
			ClassicAssert.AreEqual(@"ms_cs", actual[4].Type);
			// check that trailing spaces are honored
			ClassicAssert.AreEqual(@"0123456789 ~!@#$%^&*()_=", actual[4].Comment);

			ClassicAssert.AreEqual("17", actual[5].ChangeNumber);
			// check that branched version numbers are parsed
			ClassicAssert.AreEqual(@"SynergyProjectInfo.cs-1", actual[5].FileName);
			ClassicAssert.AreEqual(@"$/MyProject/CruiseControl.NET/project/core/sourcecontrol", actual[5].FolderName);
			ClassicAssert.AreEqual(@"ms_cs", actual[5].Type);
			// check that reserved regular expression classes are escaped
			ClassicAssert.AreEqual(@"the quick brown fox jumped over the lazy dog ", actual[5].Comment);

			ClassicAssert.AreEqual("9999", actual[6].ChangeNumber);
			// check that branched version numbers are parsed
			ClassicAssert.AreEqual(@"NotUsed-10", actual[6].FileName);
			ClassicAssert.AreEqual(@"", actual[6].FolderName);
			ClassicAssert.AreEqual(@"dir", actual[6].Type);
			ClassicAssert.IsNull(actual[6].Comment);
		}
	}
}
