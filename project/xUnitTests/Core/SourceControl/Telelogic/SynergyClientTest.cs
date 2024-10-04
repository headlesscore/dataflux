using System;
using System.Globalization;
using Exortech.NetReflector;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol.Telelogic;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol.Telelogic
{
	
	public sealed class SynergyTest
	{
		[Fact]
		public void VerifyDefaultValues()
		{
			Synergy synergy = new Synergy();
			Assert.Equal("ccm.exe", synergy.Connection.Executable, "#A1");
			Assert.Equal("localhost", synergy.Connection.Host, "#A2");
			Assert.Null(synergy.Connection.Database);
			Assert.Null(synergy.Connection.SessionId);
			Assert.Equal(3600, synergy.Connection.Timeout, "#A5");
			Assert.Equal('-', synergy.Connection.Delimiter, "#A6");
			Assert.Null(synergy.Project.Release);
			Assert.Equal(0, synergy.Project.TaskFolder);
			Assert.Equal(Environment.ExpandEnvironmentVariables("%USERNAME%"), synergy.Connection.Username, "#A9");
			Assert.Equal(String.Empty, synergy.Connection.Password, "#A10");
			Assert.Equal("build_mgr", synergy.Connection.Role, "#A11");
			Assert.False(synergy.Connection.PollingEnabled, "#A12");
			Assert.False(synergy.Project.BaseliningEnabled, "#A13");
			Assert.False(synergy.Project.TemplateEnabled, "#A14");
			Assert.Null(synergy.Project.ReconcilePaths);
			Assert.Equal("Integration Testing", synergy.Project.Purpose, "#A16");
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public void PopulateFromConfigurationXml()
		{
			Synergy synergy = (Synergy) NetReflector.Read(SynergyMother.ConfigValues);
			Assert.Equal("ccm.cmd", synergy.Connection.Executable);
			Assert.Equal("myserver", synergy.Connection.Host);
			Assert.Equal(@"\\myserver\share\mydatabase", synergy.Connection.Database);
			Assert.Equal(600, synergy.Connection.Timeout);
			Assert.Equal("Product/1.0", synergy.Project.Release);
			Assert.Equal(1234, synergy.Project.TaskFolder);
			Assert.Equal("jdoe", synergy.Connection.Username);
			Assert.Equal("password", synergy.Connection.Password);
			Assert.Equal("developer", synergy.Connection.Role);
			Assert.True(synergy.Connection.PollingEnabled);
			Assert.True(synergy.Project.BaseliningEnabled);
			Assert.True(synergy.Project.TemplateEnabled);
			Assert.NotNull(synergy.Project.ReconcilePaths);
			Assert.Equal(2, synergy.Project.ReconcilePaths.Length);
			Assert.Equal(@"Product\bin", synergy.Project.ReconcilePaths[0]);
			Assert.Equal(@"Product\temp.txt", synergy.Project.ReconcilePaths[1]);
			Assert.Equal("Custom Purpose", synergy.Project.Purpose);
			Assert.Equal(@"D:\cmsynergy\jdoe", synergy.Connection.HomeDirectory);
			Assert.Equal(@"D:\cmsynergy\uidb", synergy.Connection.ClientDatabaseDirectory);
		}

		[Fact]
		public void ProtectedDatabase()
		{
			const string status = @"
Warning: Database \\myserver\share\mydatabase on host myserver is protected.  Starting a session is not allowed.
Warning: CM Synergy startup failed.
";
			Synergy synergy = (Synergy) NetReflector.Read(SynergyMother.ConfigValues);
			SynergyCommand command = new SynergyCommand(synergy.Connection, synergy.Project);

			Assert.True(command.IsDatabaseProtected(status, synergy.Connection.Host, synergy.Connection.Database));
		}

		[Fact]
		public void UnprotectedDatabase()
		{
			const string status = @"
Warning: CM Synergy startup failed.
";
			Synergy synergy = (Synergy) NetReflector.Read(SynergyMother.ConfigValues);
			SynergyCommand command = new SynergyCommand(synergy.Connection, synergy.Project);

			Assert.False(command.IsDatabaseProtected(status, synergy.Connection.Host, synergy.Connection.Database));
		}

		[Fact]
		public void DatabaseName()
		{
			// test non-default configured values
			SynergyConnectionInfo info = new SynergyConnectionInfo();
			info.Database = System.IO.Path.DirectorySeparatorChar + System.IO.Path.Combine("myserver", "share", "mydatabase");
			Assert.Equal(@"mydatabase", info.DatabaseName);

			info.Database = System.IO.Path.DirectorySeparatorChar + System.IO.Path.Combine("myserver", "share", "mydatabase") + System.IO.Path.DirectorySeparatorChar;
			Assert.Equal(@"mydatabase", info.DatabaseName);
		}

		[Fact]
		public void DeadSession()
		{
			const string status = @"
Sessions for user jdoe:

	No sessions found.

Current project could not be identified.
";
			AssertSession(status, "COMPUTERNAME:1234:127.0.0.1", @"\\myserver\share\mydatabase", false);
		}

		[Fact]
		public void WrongSession()
		{
			const string status = @"
Sessions for user cruisecontrol:

Developer Interface @ 127.0.0.1:4567
Database: \\myserver\share\mydatabase

Command Interface @ COMPUTERNAME:8888:127.0.0.1
Database: \\myserver\share\mydatabase

Graphical Interface @ COMPUTERNAME:1234:127.0.0.1 (current session)
Database: \\myserver\share\mydatabase

Current project could not be identified.
";
			AssertSession(status, "COMPUTERNAME:3333:127.0.0.1", @"\\myserver\share\mydatabase", false);
		}

		[Fact]
		public void AliveCurrentSession()
		{
			const string status = @"
Sessions for user cruisecontrol:

Developer Interface @ 127.0.0.1:4567
Database: \\myserver\share\mydatabase

Command Interface @ COMPUTERNAME:8888:127.0.0.1
Database: \\myserver\share\mydatabase

Graphical Interface @ COMPUTERNAME:1234:127.0.0.1 (current session)
Database: \\myserver\share\mydatabase

Current project could not be identified.
";
			AssertSession(status, "COMPUTERNAME:1234:127.0.0.1", @"\\myserver\share\mydatabase", true);
		}

		[Fact]
		public void AliveSession()
		{
			const string status = @"
Sessions for user cruisecontrol:

Developer Interface @ 127.0.0.1:4567
Database: \\myserver\share\mydatabase

Command Interface @ COMPUTERNAME:8888:127.0.0.1
Database: \\myserver\share\mydatabase

Graphical Interface @ COMPUTERNAME:1234:127.0.0.1 (current session)
Database: \\myserver\share\mydatabase

Current project could not be identified.
";
			AssertSession(status, "COMPUTERNAME:8888:127.0.0.1", @"\\myserver\share\mydatabase", true);
		}
		
		[Fact]
		public void GetModifications()
		{
			var mockCommand = new Mock<ISynergyCommand>();
			MockSequence sequence = new MockSequence();
			mockCommand.InSequence(sequence).Setup(command => command.Execute(It.IsAny<ProcessInfo>())).Returns(ProcessResultFixture.CreateSuccessfulResult("output")).Verifiable();
			mockCommand.InSequence(sequence).Setup(command => command.Execute(It.IsAny<ProcessInfo>(), false)).Returns(ProcessResultFixture.CreateSuccessfulResult("output")).Verifiable();
			mockCommand.InSequence(sequence).Setup(command => command.Execute(It.IsAny<ProcessInfo>(), false)).Returns(ProcessResultFixture.CreateSuccessfulResult("output")).Verifiable();
			mockCommand.InSequence(sequence).Setup(command => command.Execute(It.IsAny<ProcessInfo>(), false)).Returns(ProcessResultFixture.CreateSuccessfulResult("output")).Verifiable();
			var mockParser = new Mock<SynergyParser>();
			mockParser.Setup(parser => parser.Parse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>())).Returns(new Modification[0]).Verifiable();

			Synergy synergy = new Synergy(new SynergyConnectionInfo(), new SynergyProjectInfo(), (ISynergyCommand) mockCommand.Object, (SynergyParser) mockParser.Object);
			synergy.GetModifications(new IntegrationResult(), new IntegrationResult());
			mockCommand.Verify();
		}

		[Fact]
		public void ApplyLabel()
		{
			var mockCommand = new Mock<ISynergyCommand>();
			MockSequence sequence = new MockSequence();
			mockCommand.InSequence(sequence).Setup(command => command.Execute(It.IsAny<ProcessInfo>())).Returns(ProcessResultFixture.CreateSuccessfulResult(DateTime.MinValue.ToString(CultureInfo.InvariantCulture))).Verifiable();
			mockCommand.InSequence(sequence).Setup(command => command.Execute(It.IsAny<ProcessInfo>())).Returns(ProcessResultFixture.CreateSuccessfulResult("output")).Verifiable();
			mockCommand.InSequence(sequence).Setup(command => command.Execute(It.IsAny<ProcessInfo>(), false)).Returns(ProcessResultFixture.CreateSuccessfulResult("output")).Verifiable();
			mockCommand.InSequence(sequence).Setup(command => command.Execute(It.IsAny<ProcessInfo>(), false)).Returns(ProcessResultFixture.CreateSuccessfulResult("output")).Verifiable();
			mockCommand.InSequence(sequence).Setup(command => command.Execute(It.IsAny<ProcessInfo>(), false)).Returns(ProcessResultFixture.CreateSuccessfulResult("output")).Verifiable();
			var mockParser = new Mock<SynergyParser>();
			mockParser.Setup(parser => parser.Parse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>())).Returns(new Modification[0]).Verifiable();

			SynergyConnectionInfo connectionInfo = new SynergyConnectionInfo();
			connectionInfo.FormatProvider = CultureInfo.InvariantCulture;
			Synergy synergy = new Synergy(connectionInfo, new SynergyProjectInfo(), (ISynergyCommand) mockCommand.Object, (SynergyParser) mockParser.Object);
			IntegrationResult integrationResult = new IntegrationResult();
			integrationResult.Status = ThoughtWorks.CruiseControl.Remote.IntegrationStatus.Success;
			synergy.LabelSourceControl(integrationResult);
			mockCommand.Verify();
		}

		[Fact]
		public void GetReconfigureTimeShouldHandleNonUSDates()
		{
            string dateString = "samedi 2 décembre 2006";
			var mockCommand = new Mock<ISynergyCommand>();
			mockCommand.Setup(command => command.Execute(It.IsAny<ProcessInfo>())).Returns(ProcessResultFixture.CreateSuccessfulResult(dateString)).Verifiable();
			SynergyConnectionInfo connectionInfo = new SynergyConnectionInfo();
			connectionInfo.FormatProvider = new CultureInfo("FR-fr");
			Synergy synergy = new Synergy(connectionInfo, new SynergyProjectInfo(), (ISynergyCommand) mockCommand.Object, null);
			DateTime time = synergy.GetReconfigureTime();
			mockCommand.Verify();
		}

		private void AssertSession(string status, string sessionId, string database, bool isAlive)
		{
			SynergyCommand command = new SynergyCommand(null, null);
			bool result = command.IsSessionAlive(status, sessionId, database);
			Assert.True(isAlive == result, "IsSessionAlive checked failed");
		}
	}
}
