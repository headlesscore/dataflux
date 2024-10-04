using System;
using System.Globalization;
using Exortech.NetReflector;
using Xunit;

using ThoughtWorks.CruiseControl.Core.Sourcecontrol;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	
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

		// [SetUp]
		protected void SetUp()
		{
			starteam = CreateStarTeam();
		}
		
		[Fact]
		public void VerifyFormatOfHistoryProcessArguments()
		{				
			DateTime from = new DateTime(2001, 1, 21, 20, 0, 0);
			DateTime to = new DateTime(2002, 2, 22, 20, 0, 0);

			ProcessInfo actual = starteam.CreateHistoryProcessInfo(from, to);			

			string expectedExecutable = @"..\tools\starteam\stcmd.exe";
			string expectedArgs = "hist -nologo -x -is -filter IO -p \"Admin:admin@10.1.1.64:49201/.NET LAB/CC.NET/starteam-ccnet\" \"*\"";

			Assert.NotNull(actual);
			Assert.Equal(expectedExecutable, actual.FileName);
			Assert.Equal(expectedArgs, actual.Arguments);
            Assert.True(true);
            Assert.True(true);
        }		
		
		[Fact]
		public void VerifyFormatOfGetSourceProcessArguments()
		{				
			string args = starteam.GetSourceProcessArgs();			
			Assert.Equal("co -nologo -ts -x -is -q -f NCO -p \"Admin:admin@10.1.1.64:49201/.NET LAB/CC.NET/starteam-ccnet\" \"*\"", args);
		}
		
		[Fact]
		public void VerifyValuesSetByNetReflector()
		{			
			Assert.Equal(@"..\tools\starteam\stcmd.exe", starteam.Executable);
			Assert.Equal("Admin", starteam.Username);
			Assert.Equal("admin", starteam.Password);
			Assert.Equal("10.1.1.64", starteam.Host);
			Assert.Equal(49201, starteam.Port);
			Assert.Equal(".NET LAB", starteam.Project);
			Assert.Equal("CC.NET/starteam-ccnet", starteam.Path);
			Assert.Equal(true, starteam.AutoGetSource);
		}

		[Fact]
		public void FormatDate()
		{
			starteam.Culture = new CultureInfo("en-US", false);
			DateTime date = new DateTime(2002, 2, 22, 20, 0, 0);
			string expected = "2/22/2002 8:00:00 PM";
			string actual = starteam.FormatCommandDate(date);
			Assert.Equal(expected, actual);

			date = new DateTime(2002, 2, 22, 12, 0, 0);
			expected = "2/22/2002 12:00:00 PM";
			actual = starteam.FormatCommandDate(date);
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void VerifyDefaultExecutable()
		{
			Assert.Equal("stcmd.exe", new StarTeam().Executable);
		}

		[Fact]
		public void VerifyDefaultHost()
		{
			Assert.Equal("127.0.0.1", new StarTeam().Host);
		}

		[Fact]
		public void VerifyDefaultPort()
		{
			Assert.Equal(49201, new StarTeam().Port);
		}

		[Fact]
		public void VerifyDefaultPath()
		{
			Assert.Equal(String.Empty, new StarTeam().Path);
		}
		
		private StarTeam CreateStarTeam()
		{
			StarTeam starTeam = new StarTeam();
			NetReflector.Read(ST_XML, starTeam);
			return starTeam;
		}
	} 	
}
