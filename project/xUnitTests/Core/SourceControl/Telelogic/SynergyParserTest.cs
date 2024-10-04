using System;
using System.Collections;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol.Telelogic;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol.Telelogic
{
	
	public sealed class SynergyParserTest
	{
		private SynergyConnectionInfo connection;
		private SynergyProjectInfo project;

		// [OneTimeSetUp]
		public void TestFixtureSetUp()
		{
			connection = new SynergyConnectionInfo();
			connection.Host = "localhost";
			connection.Database = @"\\server\share\mydb";
			connection.Delimiter = '-';
			project = new SynergyProjectInfo();
			project.ProjectSpecification = "MyProject-MyProject_Int";
		}

		[Fact]
		public void ConnectionDefaults()
		{
			SynergyConnectionInfo actual = new SynergyConnectionInfo();

			Assert.Null(actual.Database);
			Assert.Null(actual.SessionId);
			Assert.Equal(3600, actual.Timeout);
			Assert.Equal("ccm.exe", actual.Executable);
			Assert.Equal(Environment.ExpandEnvironmentVariables("%USERNAME%"), actual.Username);
			Assert.Equal("build_mgr", actual.Role);
			Assert.Equal('-', actual.Delimiter);
			Assert.Equal(Environment.ExpandEnvironmentVariables(@"%SystemDrive%\cmsynergy\%USERNAME%"), actual.HomeDirectory);
			Assert.Equal(Environment.ExpandEnvironmentVariables(@"%SystemDrive%\cmsynergy\uidb"), actual.ClientDatabaseDirectory);
			Assert.Equal(Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Telelogic\CM Synergy 6.3\bin"), actual.WorkingDirectory);

        }

		[Fact]
		public void ProjectDefaults()
		{
			SynergyProjectInfo actual = new SynergyProjectInfo();

			Assert.Null(actual.Release);
			Assert.Null(actual.ProjectSpecification);
			Assert.Equal(0, actual.TaskFolder);
			Assert.Equal(DateTime.MinValue, actual.LastReconfigureTime);
			Assert.False(actual.BaseliningEnabled);
			Assert.Equal("Integration Testing", actual.Purpose);
		}

		[Fact]
		public void CanParseNewTasks()
		{
			SynergyParser parser = new SynergyParser();

			// ngw_de0157~milligan_integrate
			Hashtable actual = parser.ParseTasks(SynergyMother.NewTaskInfo);

			// validate that a collection of 8 comments is returned
			Assert.NotNull(actual);
			Assert.Equal(6, actual.Count);

			// validate that each comment and timestamp exists, and defaults to String.Empty
			foreach (DictionaryEntry comment in actual)
			{
				Assert.NotNull(comment);
				SynergyParser.SynergyTaskInfo info = (SynergyParser.SynergyTaskInfo) comment.Value;
				Assert.NotNull(info.TaskNumber);
				Assert.NotNull(info.TaskSynopsis);
				Assert.NotNull(info.Resolver);
			}

			// test that the right comments are returned, and that the order of retrieval
			// does not matter
			if (null != actual["15"])
			{
				Assert.Equal("lorem ipsum dolerem ", ((SynergyParser.SynergyTaskInfo) actual["15"]).TaskSynopsis);
				Assert.Equal("Insulated Development projects for release PRODUCT/1.0", ((SynergyParser.SynergyTaskInfo) actual["22"]).TaskSynopsis);
				Assert.Equal("jdoe's Insulated Development projects", ((SynergyParser.SynergyTaskInfo) actual["21"]).TaskSynopsis);
				Assert.Equal("IGNORE THIS Sample Task ", ((SynergyParser.SynergyTaskInfo) actual["99"]).TaskSynopsis);
				Assert.Equal("the quick brown fox jumped over the lazy dog ", ((SynergyParser.SynergyTaskInfo) actual["17"]).TaskSynopsis);
				Assert.Equal("0123456789 ~!@#$%^&*()_=", ((SynergyParser.SynergyTaskInfo) actual["1"]).TaskSynopsis);
			}
			else
			{
				Assert.Equal("lorem ipsum dolerem ", ((SynergyParser.SynergyTaskInfo) actual["wwdev#15"]).TaskSynopsis);
				Assert.Equal("Insulated Development projects for release PRODUCT/1.0", ((SynergyParser.SynergyTaskInfo) actual["wwdev#22"]).TaskSynopsis);
				Assert.Equal("jdoe's Insulated Development projects", ((SynergyParser.SynergyTaskInfo) actual["wwdev#21"]).TaskSynopsis);
				Assert.Equal("IGNORE THIS Sample Task ", ((SynergyParser.SynergyTaskInfo) actual["wwdev#99"]).TaskSynopsis);
				Assert.Equal("the quick brown fox jumped over the lazy dog ", ((SynergyParser.SynergyTaskInfo) actual["wwdev#17"]).TaskSynopsis);
				Assert.Equal("0123456789 ~!@#$%^&*()_=", ((SynergyParser.SynergyTaskInfo) actual["wwdev#1"]).TaskSynopsis);
			}

			// assert that tasks not in the original list are null
			Assert.Null(actual["123456789"]);
		}

		[Fact]
		public void ParseNewObjects()
		{
			ParseNewObjects(SynergyMother.NewTaskInfo, SynergyMother.NewObjects);
		}

		[Fact]
		public void ParseDCMObjects()
		{
			ParseNewObjects(SynergyMother.NewDcmTaskInfo, SynergyMother.NewDCMObjects);
		}

		[Fact]
		public void ParseWhenTasksAreEmpty()
		{
			SynergyParser parser = new SynergyParser();
			// set the from date to be one week back
			DateTime from = DateTime.Now.AddDays(-7L);

			Modification[] actual = parser.Parse(string.Empty, SynergyMother.NewObjects, from);
			Assert.Equal(7, actual.Length);
			Assert.Equal("15", actual[0].ChangeNumber);
			Assert.Equal("9999", actual[6].ChangeNumber);
		}

		private void ParseNewObjects(string newTasks, string newObjects)
		{
			SynergyParser parser = new SynergyParser();
			// set the from date to be one week back
			DateTime from = DateTime.Now.AddDays(-7L);

			Modification[] actual = parser.Parse(newTasks, newObjects, from);

			Assert.NotNull(actual);
			Assert.Equal(7, actual.Length);

			foreach (Modification modification in actual)
			{
				Assert.Equal("jdoe", modification.EmailAddress);
				Assert.Equal("jdoe", modification.UserName);
				Assert.Null(modification.Url);
			}

			Assert.Equal("15", actual[0].ChangeNumber);
			Assert.Equal(@"sourcecontrol-3", actual[0].FileName);
			Assert.Equal(@"$/MyProject/CruiseControl.NET/project/core", actual[0].FolderName);
			Assert.Equal(@"dir", actual[0].Type);
			Assert.Equal(@"lorem ipsum dolerem ", actual[0].Comment);

			// test that the last task number is used when an object is associated with multiple tasks
			Assert.Equal("21", actual[1].ChangeNumber);
			Assert.Equal(@"Synergy.cs-1", actual[1].FileName);
			Assert.Equal(@"$/MyProject/CruiseControl.NET/project/core/sourcecontrol", actual[1].FolderName);
			Assert.Equal(@"ms_cs", actual[1].Type);
			// check that trailing spaces are honored
			Assert.Equal("jdoe's Insulated Development projects", actual[1].Comment);

			Assert.Equal("22", actual[2].ChangeNumber);
			// check that branched version numbers are parsed
			Assert.Equal(@"SynergyCommandBuilder.cs-1.1.1", actual[2].FileName);
			Assert.Equal(@"$/MyProject/CruiseControl.NET/project/core/sourcecontrol", actual[2].FolderName);
			Assert.Equal(@"ms_cs", actual[2].Type);
			Assert.Equal("Insulated Development projects for release PRODUCT/1.0", actual[2].Comment);

			Assert.Equal("22", actual[3].ChangeNumber);
			Assert.Equal(@"SynergyConnectionInfo.cs-2", actual[3].FileName);
			Assert.Equal(@"$/MyProject/CruiseControl.NET/project/core/sourcecontrol", actual[3].FolderName);
			Assert.Equal(@"ms_cs", actual[3].Type);
			// check that trailing spaces are honored
			Assert.Equal("Insulated Development projects for release PRODUCT/1.0", actual[3].Comment);

			Assert.Equal("1", actual[4].ChangeNumber);
			// check that branched version numbers are parsed
			Assert.Equal(@"SynergyHistoryParser.cs-2.2.1", actual[4].FileName);
			Assert.Equal(@"$/MyProject/CruiseControl.NET/project/core/sourcecontrol", actual[4].FolderName);
			Assert.Equal(@"ms_cs", actual[4].Type);
			// check that trailing spaces are honored
			Assert.Equal(@"0123456789 ~!@#$%^&*()_=", actual[4].Comment);

			Assert.Equal("17", actual[5].ChangeNumber);
			// check that branched version numbers are parsed
			Assert.Equal(@"SynergyProjectInfo.cs-1", actual[5].FileName);
			Assert.Equal(@"$/MyProject/CruiseControl.NET/project/core/sourcecontrol", actual[5].FolderName);
			Assert.Equal(@"ms_cs", actual[5].Type);
			// check that reserved regular expression classes are escaped
			Assert.Equal(@"the quick brown fox jumped over the lazy dog ", actual[5].Comment);

			Assert.Equal("9999", actual[6].ChangeNumber);
			// check that branched version numbers are parsed
			Assert.Equal(@"NotUsed-10", actual[6].FileName);
			Assert.Equal(@"", actual[6].FolderName);
			Assert.Equal(@"dir", actual[6].Type);
			Assert.Null(actual[6].Comment);
		}
	}
}
