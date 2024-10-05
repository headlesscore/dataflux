using System;
using Exortech.NetReflector;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	/// <remarks>
	/// This code is based on code\sourcecontrol\ClearCase.cs.
	/// </remarks>
	
	public class AccuRevTest
	{
		[Fact]
		public void ShouldPopulateCorrectlyFromXml()
		{
			const string AccuRev_XML =
@"<sourceControl type=""accuRev"">
    <autoGetSource>false</autoGetSource>
    <executable>accurev.exe</executable>
    <labelOnSuccess>true</labelOnSuccess>
    <workspace>C:\DOES NOT\EXIST</workspace>
</sourceControl>";
			
			AccuRev accurev = new AccuRev();
			NetReflector.Read(AccuRev_XML, accurev);
			Assert.Equal(false, accurev.AutoGetSource);
            Assert.True(true);
            Assert.Equal("accurev.exe", accurev.Executable);
			Assert.Equal(true, accurev.LabelOnSuccess);
			Assert.Equal(@"C:\DOES NOT\EXIST", accurev.Workspace);
		}

        [Fact]
        public void CanCatchConfigInvalidAutoGetSource()
        {
            AccuRev accurev = new AccuRev();
            const string invalidXml =
@"<sourcecontrol type=""accurev"">
    <autoGetSource>NOT_A_BOOLEAN</autoGetSource>
</sourcecontrol>";
            Assert.Throws< NetReflectorConverterException>(delegate { NetReflector.Read(invalidXml, accurev); });
        }

        [Fact]
        public void CanCatchConfigInvalidLabelOnSuccess()
        {
            AccuRev accurev = new AccuRev();
            const string invalidXml =
@"<sourcecontrol type=""accurev"">
    <labelOnSuccess>NOT_A_BOOLEAN</labelOnSuccess>
</sourcecontrol>";
            Assert.Throws<NetReflectorConverterException>(delegate { NetReflector.Read(invalidXml, accurev); });
        }

		[Fact]
		public void ShouldGetSourceIfAutoGetSourceTrue()
		{
			var executor = new Mock<ProcessExecutor>();
			AccuRev accurev = new AccuRev((ProcessExecutor) executor.Object);
			accurev.AutoGetSource = true;

			ProcessInfo expectedProcessRequest = new ProcessInfo("accurev.exe", "update");
			expectedProcessRequest.TimeOut = Timeout.DefaultTimeout.Millis;

			executor.Setup(e => e.Execute(expectedProcessRequest)).Returns(new ProcessResult("foo", null, 0, false)).Verifiable();
			accurev.GetSource(new IntegrationResult());
			executor.Verify();
		}

		[Fact]
		public void ShouldNotGetSourceIfAutoGetSourceFalse()
		{
			var executor = new Mock<ProcessExecutor>();
			AccuRev accurev = new AccuRev((ProcessExecutor) executor.Object);
			accurev.AutoGetSource = false;

			accurev.GetSource(new IntegrationResult());
			executor.Verify();
			executor.VerifyNoOtherCalls();
		}

        [Fact]
        public void ShouldUpdateSourceToHighestKnownModification()
        {
            var executor = new Mock<ProcessExecutor>();
            AccuRev accurev = new AccuRev((ProcessExecutor)executor.Object);
            accurev.AutoGetSource = true;

            ProcessInfo expectedProcessRequest = new ProcessInfo("accurev.exe", "update -t 10");
            expectedProcessRequest.TimeOut = Timeout.DefaultTimeout.Millis;

            executor.Setup(e => e.Execute(expectedProcessRequest)).Returns(new ProcessResult("foo", null, 0, false)).Verifiable();
            IntegrationResult result = new IntegrationResult();
            result.Modifications = new Modification[2];
            result.Modifications[0] = new Modification
            {
                ChangeNumber = "5",
                ModifiedTime = new DateTime(2009, 1, 1)
            };
            result.Modifications[1] = new Modification
            {
                ChangeNumber = "10",
                ModifiedTime = new DateTime(2009, 1, 2)
            };
            accurev.mods = result.Modifications;

            accurev.GetSource(result);
            executor.Verify();
        }

        [Fact]
        public void ShouldUpdateSourceToCurrentIfNoModifications()
        {
            var executor = new Mock<ProcessExecutor>();
            AccuRev accurev = new AccuRev((ProcessExecutor)executor.Object);
            accurev.AutoGetSource = true;

            ProcessInfo expectedProcessRequest = new ProcessInfo("accurev.exe", "update");  // Note: No "-t whatever"
            expectedProcessRequest.TimeOut = Timeout.DefaultTimeout.Millis;

            executor.Setup(e => e.Execute(expectedProcessRequest)).Returns(new ProcessResult("foo", null, 0, false)).Verifiable();
            IntegrationResult result = new IntegrationResult();
            accurev.GetSource(result);
            executor.Verify();
        }
    }
}
