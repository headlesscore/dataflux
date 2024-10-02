using System;
using System.Globalization;
using Exortech.NetReflector;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol.Telelogic;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol.Telelogic
{
	[TestFixture]
	public sealed class SynergyTest
	{
		[Test]
		public void VerifyDefaultValues()
		{
			Synergy synergy = new Synergy();
			ClassicAssert.AreEqual("ccm.exe", synergy.Connection.Executable, "#A1");
			ClassicAssert.AreEqual("localhost", synergy.Connection.Host, "#A2");
			ClassicAssert.IsNull(synergy.Connection.Database, "#A3");
			ClassicAssert.IsNull(synergy.Connection.SessionId, "#A4");
			ClassicAssert.AreEqual(3600, synergy.Connection.Timeout, "#A5");
			ClassicAssert.AreEqual('-', synergy.Connection.Delimiter, "#A6");
			ClassicAssert.IsNull(synergy.Project.Release, "#A7");
			ClassicAssert.AreEqual(0, synergy.Project.TaskFolder, "#A8");
			ClassicAssert.AreEqual(Environment.ExpandEnvironmentVariables("%USERNAME%"), synergy.Connection.Username, "#A9");
			ClassicAssert.AreEqual(String.Empty, synergy.Connection.Password, "#A10");
			ClassicAssert.AreEqual("build_mgr", synergy.Connection.Role, "#A11");
			ClassicAssert.IsFalse(synergy.Connection.PollingEnabled, "#A12");
			ClassicAssert.IsFalse(synergy.Project.BaseliningEnabled, "#A13");
			ClassicAssert.IsFalse(synergy.Project.TemplateEnabled, "#A14");
			ClassicAssert.IsNull(synergy.Project.ReconcilePaths, "#A15");
			ClassicAssert.AreEqual("Integration Testing", synergy.Project.Purpose, "#A16");
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

		[Test]
		public void PopulateFromConfigurationXml()
		{
			Synergy synergy = (Synergy) NetReflector.Read(SynergyMother.ConfigValues);
			ClassicAssert.AreEqual("ccm.cmd", synergy.Connection.Executable);
			ClassicAssert.AreEqual("myserver", synergy.Connection.Host);
			ClassicAssert.AreEqual(@"\\myserver\share\mydatabase", synergy.Connection.Database);
			ClassicAssert.AreEqual(600, synergy.Connection.Timeout);
			ClassicAssert.AreEqual("Product/1.0", synergy.Project.Release);
			ClassicAssert.AreEqual(1234, synergy.Project.TaskFolder);
			ClassicAssert.AreEqual("jdoe", synergy.Connection.Username);
			ClassicAssert.AreEqual("password", synergy.Connection.Password);
			ClassicAssert.AreEqual("developer", synergy.Connection.Role);
			ClassicAssert.IsTrue(synergy.Connection.PollingEnabled);
			ClassicAssert.IsTrue(synergy.Project.BaseliningEnabled);
			ClassicAssert.IsTrue(synergy.Project.TemplateEnabled);
			ClassicAssert.IsNotNull(synergy.Project.ReconcilePaths);
			ClassicAssert.AreEqual(2, synergy.Project.ReconcilePaths.Length);
			ClassicAssert.AreEqual(@"Product\bin", synergy.Project.ReconcilePaths[0]);
			ClassicAssert.AreEqual(@"Product\temp.txt", synergy.Project.ReconcilePaths[1]);
			ClassicAssert.AreEqual("Custom Purpose", synergy.Project.Purpose);
			ClassicAssert.AreEqual(@"D:\cmsynergy\jdoe", synergy.Connection.HomeDirectory);
			ClassicAssert.AreEqual(@"D:\cmsynergy\uidb", synergy.Connection.ClientDatabaseDirectory);
		}

		[Test]
		public void ProtectedDatabase()
		{
			const string status = @"
Warning: Database \\myserver\share\mydatabase on host myserver is protected.  Starting a session is not allowed.
Warning: CM Synergy startup failed.
";
			Synergy synergy = (Synergy) NetReflector.Read(SynergyMother.ConfigValues);
			SynergyCommand command = new SynergyCommand(synergy.Connection, synergy.Project);

			ClassicAssert.IsTrue(command.IsDatabaseProtected(status, synergy.Connection.Host, synergy.Connection.Database));
		}

		[Test]
		public void UnprotectedDatabase()
		{
			const string status = @"
Warning: CM Synergy startup failed.
";
			Synergy synergy = (Synergy) NetReflector.Read(SynergyMother.ConfigValues);
			SynergyCommand command = new SynergyCommand(synergy.Connection, synergy.Project);

			ClassicAssert.IsFalse(command.IsDatabaseProtected(status, synergy.Connection.Host, synergy.Connection.Database));
		}

		[Test]
		public void DatabaseName()
		{
			// test non-default configured values
			SynergyConnectionInfo info = new SynergyConnectionInfo();
			info.Database = System.IO.Path.DirectorySeparatorChar + System.IO.Path.Combine("myserver", "share", "mydatabase");
			ClassicAssert.AreEqual(@"mydatabase", info.DatabaseName);

			info.Database = System.IO.Path.DirectorySeparatorChar + System.IO.Path.Combine("myserver", "share", "mydatabase") + System.IO.Path.DirectorySeparatorChar;
			ClassicAssert.AreEqual(@"mydatabase", info.DatabaseName);
		}

		[Test]
		public void DeadSession()
		{
			const string status = @"
Sessions for user jdoe:

	No sessions found.

Current project could not be identified.
";
			AssertSession(status, "COMPUTERNAME:1234:127.0.0.1", @"\\myserver\share\mydatabase", false);
		}

		[Test]
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

		[Test]
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

		[Test]
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
		
		[Test]
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

		[Test]
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

		[Test]
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
			ClassicAssert.IsTrue(isAlive == result, "IsSessionAlive checked failed");
		}
	}
}
