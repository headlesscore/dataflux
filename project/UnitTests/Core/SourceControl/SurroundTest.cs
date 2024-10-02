using System;
using System.Globalization;
using Exortech.NetReflector;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	[TestFixture]
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

		[SetUp]
		protected void SetUp()
		{
			surround = new Surround();
			NetReflector.Read(SSCM_XML, surround);
		}

		[Test]
		public void VerifyValuesSetByNetReflector()
		{
			ClassicAssert.AreEqual(@"C:\Program Files\Seapine\Surround SCM\sscm.exe", surround.Executable);
			ClassicAssert.AreEqual("build:build", surround.ServerLogin);
			ClassicAssert.AreEqual("198.187.17.157:4900", surround.ServerConnect);
			ClassicAssert.AreEqual("m20040908", surround.Branch);
			ClassicAssert.AreEqual("m20040908/scctt3", surround.Repository);
			ClassicAssert.AreEqual(@"C:\scctt3", surround.WorkingDirectory);
			ClassicAssert.AreEqual(1, surround.Recursive);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

		[Test]
		public void VerifyFormatDate()
		{
			DateTime dateExpected = new DateTime(2005, 9, 30, 1, 2, 3);
			string strDateExpected = "20050930010203";

			DateTime checkDate = DateTime.ParseExact(strDateExpected, Surround.TO_SSCM_DATE_FORMAT, CultureInfo.InvariantCulture);
			ClassicAssert.AreEqual(dateExpected, checkDate);

			string checkStrDate = dateExpected.ToString(Surround.TO_SSCM_DATE_FORMAT);
			ClassicAssert.AreEqual(strDateExpected, checkStrDate);
		}

		[Test]
		public void VerifyDefaults()
		{
			surround = new Surround();
			ClassicAssert.AreEqual("127.0.0.1:4900", surround.ServerConnect);
			ClassicAssert.AreEqual("Administrator:", surround.ServerLogin);
			ClassicAssert.AreEqual(0, surround.SearchRegExp);
			ClassicAssert.AreEqual(0, surround.Recursive);
		}
	}
}
