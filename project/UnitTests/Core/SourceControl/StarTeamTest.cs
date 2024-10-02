using System;
using System.Globalization;
using Exortech.NetReflector;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	[TestFixture]
	public class StarTeamTest : CustomAssertion
	{
		public const string ST_XML =
			@"<sourceControl type=""starteam"" autoGetSource=""true"">
				<executable>..\tools\starteam\stcmd.exe</executable>
				<username>Admin</username>
				<password>admin</password>
				<host>10.1.1.64</host>
				<port>49201</port>
				<project>.NET LAB</project>
				<path>CC.NET/starteam-ccnet</path>				
			</sourceControl>";	

		private StarTeam starteam;

		[SetUp]
		protected void SetUp()
		{
			starteam = CreateStarTeam();
		}
		
		[Test]
		public void VerifyFormatOfHistoryProcessArguments()
		{				
			DateTime from = new DateTime(2001, 1, 21, 20, 0, 0);
			DateTime to = new DateTime(2002, 2, 22, 20, 0, 0);

			ProcessInfo actual = starteam.CreateHistoryProcessInfo(from, to);			

			string expectedExecutable = @"..\tools\starteam\stcmd.exe";
			string expectedArgs = "hist -nologo -x -is -filter IO -p \"Admin:admin@10.1.1.64:49201/.NET LAB/CC.NET/starteam-ccnet\" \"*\"";

			ClassicAssert.IsNotNull(actual);
			ClassicAssert.AreEqual(expectedExecutable, actual.FileName);
			ClassicAssert.AreEqual(expectedArgs, actual.Arguments);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }		
		
		[Test]
		public void VerifyFormatOfGetSourceProcessArguments()
		{				
			string args = starteam.GetSourceProcessArgs();			
			ClassicAssert.AreEqual("co -nologo -ts -x -is -q -f NCO -p \"Admin:admin@10.1.1.64:49201/.NET LAB/CC.NET/starteam-ccnet\" \"*\"", args);
		}
		
		[Test]
		public void VerifyValuesSetByNetReflector()
		{			
			ClassicAssert.AreEqual(@"..\tools\starteam\stcmd.exe", starteam.Executable);
			ClassicAssert.AreEqual("Admin", starteam.Username);
			ClassicAssert.AreEqual("admin", starteam.Password);
			ClassicAssert.AreEqual("10.1.1.64", starteam.Host);
			ClassicAssert.AreEqual(49201, starteam.Port);
			ClassicAssert.AreEqual(".NET LAB", starteam.Project);
			ClassicAssert.AreEqual("CC.NET/starteam-ccnet", starteam.Path);
			ClassicAssert.AreEqual(true, starteam.AutoGetSource);
		}

		[Test]
		public void FormatDate()
		{
			starteam.Culture = new CultureInfo("en-US", false);
			DateTime date = new DateTime(2002, 2, 22, 20, 0, 0);
			string expected = "2/22/2002 8:00:00 PM";
			string actual = starteam.FormatCommandDate(date);
			ClassicAssert.AreEqual(expected, actual);

			date = new DateTime(2002, 2, 22, 12, 0, 0);
			expected = "2/22/2002 12:00:00 PM";
			actual = starteam.FormatCommandDate(date);
			ClassicAssert.AreEqual(expected, actual);
		}

		[Test]
		public void VerifyDefaultExecutable()
		{
			ClassicAssert.AreEqual("stcmd.exe", new StarTeam().Executable);
		}

		[Test]
		public void VerifyDefaultHost()
		{
			ClassicAssert.AreEqual("127.0.0.1", new StarTeam().Host);
		}

		[Test]
		public void VerifyDefaultPort()
		{
			ClassicAssert.AreEqual(49201, new StarTeam().Port);
		}

		[Test]
		public void VerifyDefaultPath()
		{
			ClassicAssert.AreEqual(String.Empty, new StarTeam().Path);
		}
		
		private StarTeam CreateStarTeam()
		{
			StarTeam starTeam = new StarTeam();
			NetReflector.Read(ST_XML, starTeam);
			return starTeam;
		}
	} 	
}
