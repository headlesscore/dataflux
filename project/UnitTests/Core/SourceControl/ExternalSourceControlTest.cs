using System;
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
	public class ExternalSourceControlTest
	{

        [Test]
        public void VerifyDefaultValues()
        {
            ExternalSourceControl externalSC = new ExternalSourceControl();
            ClassicAssert.AreEqual(string.Empty, externalSC.ArgString);
            ClassicAssert.AreEqual(false, externalSC.AutoGetSource);
            ClassicAssert.AreEqual(0, externalSC.EnvironmentVariables.Length);
            ClassicAssert.AreEqual(false, externalSC.LabelOnSuccess);
            ClassicAssert.IsTrue(true);
        }

		[Test]
		public void ShouldPopulateCorrectlyFromXml()
		{
			const string xml =
@"<sourceControl type=""external"">
    <args>arg1 ""arg2 has blanks"" arg3</args>
    <autoGetSource>true</autoGetSource>
    <executable>banana.bat</executable>
    <labelOnSuccess>true</labelOnSuccess>
    <environment>
        <variable name=""name1"" value=""value1""/>
        <variable><name>name2</name></variable>
        <variable name=""name3""><value>value3</value></variable>
    </environment>
</sourceControl>";

            ExternalSourceControl externalSC = new ExternalSourceControl();
            NetReflector.Read(xml, externalSC);
            ClassicAssert.AreEqual(@"arg1 ""arg2 has blanks"" arg3", externalSC.ArgString);
            ClassicAssert.AreEqual(true, externalSC.AutoGetSource);
            ClassicAssert.AreEqual(3, externalSC.EnvironmentVariables.Length);
            ClassicAssert.AreEqual("name1", externalSC.EnvironmentVariables[0].name);
            ClassicAssert.AreEqual("value1", externalSC.EnvironmentVariables[0].value);
            ClassicAssert.AreEqual("name2", externalSC.EnvironmentVariables[1].name);
            ClassicAssert.AreEqual("", externalSC.EnvironmentVariables[1].value);
            ClassicAssert.AreEqual("name3", externalSC.EnvironmentVariables[2].name);
            ClassicAssert.AreEqual("value3", externalSC.EnvironmentVariables[2].value);
            ClassicAssert.AreEqual("banana.bat", externalSC.Executable);
            ClassicAssert.AreEqual(true, externalSC.LabelOnSuccess);
        }

        [Test]
        public void ShouldPopulateCorrectlyFromMinimalXml()
        {
            const string xml =
@"<sourceControl type=""external"">
    <executable>banana.bat</executable>
</sourceControl>";
            ExternalSourceControl externalSC = new ExternalSourceControl();
            NetReflector.Read(xml, externalSC);
            ClassicAssert.AreEqual(string.Empty, externalSC.ArgString);
            ClassicAssert.AreEqual(false, externalSC.AutoGetSource);
            ClassicAssert.AreEqual(0, externalSC.EnvironmentVariables.Length);
            ClassicAssert.AreEqual("banana.bat", externalSC.Executable);
            ClassicAssert.AreEqual(false, externalSC.LabelOnSuccess);
        }

        [Test]
        public void ShouldFailToPopulateFromConfigurationMissingRequiredFields()
        {
            const string xml = @"<sourceControl type=""external""></sourceControl>";
            ExternalSourceControl externalSC = new ExternalSourceControl();
            ClassicAssert.That(delegate { NetReflector.Read(xml, externalSC); },
                        Throws.TypeOf<NetReflectorException>());
        }

		[Test]
		public void ShouldGetSourceIfAutoGetSourceTrue()
		{
			var executor = new Mock<ProcessExecutor>();
            ExternalSourceControl externalSC = new ExternalSourceControl((ProcessExecutor)executor.Object);
            externalSC.AutoGetSource = true;
		    externalSC.Executable = "banana.bat";
		    externalSC.ArgString = @"arg1 ""arg2 is longer"" arg3";

		    IntegrationResult intResult = new IntegrationResult();
            intResult.StartTime = new DateTime(1959,9,11,7,53,0);
		    intResult.WorkingDirectory = @"C:\SomeDir\Or\Other";
            intResult.ProjectName = "MyProject";

			ProcessInfo expectedProcessRequest = new ProcessInfo(
                "banana.bat", 
                @"GETSOURCE ""C:\SomeDir\Or\Other"" ""1959-09-11 07:53:00"" arg1 ""arg2 is longer"" arg3",
                @"C:\SomeDir\Or\Other"
                );
			expectedProcessRequest.TimeOut = Timeout.DefaultTimeout.Millis;

			executor.Setup(e => e.Execute(expectedProcessRequest)).Returns(new ProcessResult("foo", null, 0, false)).Verifiable();
            externalSC.GetSource(intResult);
			executor.Verify();
		}

		[Test]
		public void ShouldNotGetSourceIfAutoGetSourceFalse()
		{
			var executor = new Mock<ProcessExecutor>();
            ExternalSourceControl externalSC = new ExternalSourceControl((ProcessExecutor)executor.Object);
            externalSC.AutoGetSource = false;

            externalSC.GetSource(new IntegrationResult());
			executor.Verify();
			executor.VerifyNoOtherCalls();
		}
	}
}

