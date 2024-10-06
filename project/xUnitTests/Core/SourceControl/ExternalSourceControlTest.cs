using System;
using Exortech.NetReflector;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;
using ThoughtWorks.CruiseControl.Core.Util;
using Exortech.NetReflector;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	
	public class ExternalSourceControlTest
	{

        [Fact]
        public void VerifyDefaultValues()
        {
            ExternalSourceControl externalSC = new ExternalSourceControl();
            Assert.Equal(string.Empty, externalSC.ArgString);
            Assert.Equal(false, externalSC.AutoGetSource);
            Assert.Equal(0, externalSC.EnvironmentVariables.Length);
            Assert.Equal(false, externalSC.LabelOnSuccess);
            Assert.True(true);
        }

		[Fact]
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
            Assert.Equal(@"arg1 ""arg2 has blanks"" arg3", externalSC.ArgString);
            Assert.Equal(true, externalSC.AutoGetSource);
            Assert.Equal(3, externalSC.EnvironmentVariables.Length);
            Assert.Equal("name1", externalSC.EnvironmentVariables[0].name);
            Assert.Equal("value1", externalSC.EnvironmentVariables[0].value);
            Assert.Equal("name2", externalSC.EnvironmentVariables[1].name);
            Assert.Equal("", externalSC.EnvironmentVariables[1].value);
            Assert.Equal("name3", externalSC.EnvironmentVariables[2].name);
            Assert.Equal("value3", externalSC.EnvironmentVariables[2].value);
            Assert.Equal("banana.bat", externalSC.Executable);
            Assert.Equal(true, externalSC.LabelOnSuccess);
        }

        [Fact]
        public void ShouldPopulateCorrectlyFromMinimalXml()
        {
            const string xml =
@"<sourceControl type=""external"">
    <executable>banana.bat</executable>
</sourceControl>";
            ExternalSourceControl externalSC = new ExternalSourceControl();
            NetReflector.Read(xml, externalSC);
            Assert.Equal(string.Empty, externalSC.ArgString);
            Assert.Equal(false, externalSC.AutoGetSource);
            Assert.Equal(0, externalSC.EnvironmentVariables.Length);
            Assert.Equal("banana.bat", externalSC.Executable);
            Assert.Equal(false, externalSC.LabelOnSuccess);
        }

        [Fact]
        public void ShouldFailToPopulateFromConfigurationMissingRequiredFields()
        {
            const string xml = @"<sourceControl type=""external""></sourceControl>";
            ExternalSourceControl externalSC = new ExternalSourceControl();
            Assert.Throws<NetReflectorException>(delegate { NetReflector.Read(xml, externalSC); });
        }

		[Fact]
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

		[Fact]
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

