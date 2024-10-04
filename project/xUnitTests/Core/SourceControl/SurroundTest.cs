using System;
using System.Globalization;
using Exortech.NetReflector;
using Xunit;

using ThoughtWorks.CruiseControl.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	
	public class SurroundTest : CustomAssertion
	{
		public const string SSCM_XML =
			@"<sourceControl type=""sscm"">
				<executable>C:\Program Files\Seapine\Surround SCM\sscm.exe</executable>
				<serverlogin>build:build</serverlogin>
				<serverconnect>198.187.17.157:4900</serverconnect>
            <branch>m20040908</branch>
            <repository>m20040908/scctt3</repository>
            <workingDirectory>C:\scctt3</workingDirectory>
            <recursive>1</recursive>
			</sourceControl>";

		private Surround surround;

		// [SetUp]
		protected void SetUp()
		{
			surround = new Surround();
			NetReflector.Read(SSCM_XML, surround);
		}

		[Fact]
		public void VerifyValuesSetByNetReflector()
		{
			Assert.Equal(@"C:\Program Files\Seapine\Surround SCM\sscm.exe", surround.Executable);
			Assert.Equal("build:build", surround.ServerLogin);
			Assert.Equal("198.187.17.157:4900", surround.ServerConnect);
			Assert.Equal("m20040908", surround.Branch);
			Assert.Equal("m20040908/scctt3", surround.Repository);
			Assert.Equal(@"C:\scctt3", surround.WorkingDirectory);
			Assert.Equal(1, surround.Recursive);
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public void VerifyFormatDate()
		{
			DateTime dateExpected = new DateTime(2005, 9, 30, 1, 2, 3);
			string strDateExpected = "20050930010203";

			DateTime checkDate = DateTime.ParseExact(strDateExpected, Surround.TO_SSCM_DATE_FORMAT, CultureInfo.InvariantCulture);
			Assert.Equal(dateExpected, checkDate);

			string checkStrDate = dateExpected.ToString(Surround.TO_SSCM_DATE_FORMAT);
			Assert.Equal(strDateExpected, checkStrDate);
		}

		[Fact]
		public void VerifyDefaults()
		{
			surround = new Surround();
			Assert.Equal("127.0.0.1:4900", surround.ServerConnect);
			Assert.Equal("Administrator:", surround.ServerLogin);
			Assert.Equal(0, surround.SearchRegExp);
			Assert.Equal(0, surround.Recursive);
		}
	}
}
