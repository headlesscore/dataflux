using System;
using Exortech.NetReflector;
using Xunit;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Remote;


namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
    
    public class PlasticSCMTest : CustomAssertion
    {
        public const string PLASTICSCM_XML =
            @"<sourceControl type=""plasticscm"" branch=""br:/main"">
                <executable>c:\plastic\client\cm.exe</executable>
                <repository>mainrep</repository>
                <workingDirectory>c:\workspace</workingDirectory>
                <labelOnSuccess>true</labelOnSuccess>
                <labelPrefix>BL</labelPrefix>
                <forced>true</forced>
            </sourceControl>";

        public const string PLASTICSCM_BASIC_XML =
            @"<sourceControl type=""plasticscm"">
                <branch>br:/main</branch>
                <workingDirectory>c:\workspace</workingDirectory>
            </sourceControl>";

        [Fact]
        public void ShouldFailToConfigureWithoutRequiredAttributes()
        {
            const string PLASTICSCM_ERR2_XML =
                @"<sourceControl type=""plasticscm"">
                    <repository>mainrep</repository>
                    <workingDirectory>c:\workspace</workingDirectory>
                    <forced>true</forced>
                 </sourceControl>";

            PlasticSCM plasticscm = new PlasticSCM();
            Assert.True(delegate { NetReflector.Read(PLASTICSCM_ERR2_XML, plasticscm); },
                        Throws.TypeOf<NetReflectorException>());
            Assert.True(true);
            Assert.True(true);

        }

        [Fact]
        public void ShouldConfigureWithBasicXml()
        {
            PlasticSCM plasticscm = new PlasticSCM();
            NetReflector.Read(PLASTICSCM_BASIC_XML, plasticscm);
        }

        [Fact]
        public void VerifyValuesSetByNetReflector()
        {
            PlasticSCM plasticscm = new PlasticSCM();
            NetReflector.Read(PLASTICSCM_XML, plasticscm);

            Assert.Equal(@"c:\plastic\client\cm.exe", plasticscm.Executable);
            Assert.Equal("mainrep", plasticscm.Repository);
            Assert.Equal("br:/main", plasticscm.Branch);
            Assert.Equal(true, plasticscm.Forced);
            Assert.Equal(true, plasticscm.LabelOnSuccess);
            Assert.Equal("BL", plasticscm.LabelPrefix);
            Assert.Equal(@"c:\workspace", plasticscm.WorkingDirectory);
        }

        [Fact]
        public void VerifyGoToBranchProcessInfoBasic()
        {
            IntegrationRequest request = new IntegrationRequest(BuildCondition.ForceBuild, "source", null);
            IntegrationSummary lastSummary =
                new IntegrationSummary(IntegrationStatus.Success, "label", "lastlabel", DateTime.Now);
            IntegrationResult result = new IntegrationResult("test", @"c:\workspace", @"c:\artifacts", request, lastSummary);

            PlasticSCM plasticscm = new PlasticSCM();
            NetReflector.Read(PLASTICSCM_XML, plasticscm);
            string expected = @"c:\plastic\client\cm.exe stb br:/main -repository=mainrep";
            ProcessInfo info = plasticscm.GoToBranchProcessInfo(result);
            Assert.Equal(expected, info.FileName + " " + info.Arguments);
        }

        [Fact]
        public void VerifyNewGetSourceProcessInfoWithAttributes()
        {
            IntegrationRequest request = new IntegrationRequest(BuildCondition.ForceBuild, "source", null);
            IntegrationSummary lastSummary = new IntegrationSummary(IntegrationStatus.Success, "label", "lastlabel", DateTime.Now);
            IntegrationResult result = new IntegrationResult("test", @"c:\workspace", @"c:\artifacts", request, lastSummary);

            PlasticSCM plasticscm = new PlasticSCM();
            NetReflector.Read(PLASTICSCM_XML, plasticscm);
            string expected = @"c:\plastic\client\cm.exe stb br:/main -repository=mainrep";
            ProcessInfo info = plasticscm.GoToBranchProcessInfo(result);
            Assert.Equal(expected, info.FileName + " " + info.Arguments);

        }

        [Fact]
        public void VerifyCreateQueryProcessInfoBasic()
        {
            string fromtime = "01/02/2003 00:00:00";
            string totime = "23/02/2006 23:14:05";
            IntegrationRequest request = new IntegrationRequest(BuildCondition.ForceBuild, "source", null);
            IntegrationSummary lastSummary =
                new IntegrationSummary(IntegrationStatus.Success, "label", "lastlabel", DateTime.Now);
            IntegrationResult from =
                new IntegrationResult("test", @"c:\workspace", @"c:\artifacts", request, lastSummary);
            from.StartTime =
                DateTime.ParseExact(fromtime, PlasticSCM.DATEFORMAT, System.Globalization.CultureInfo.InvariantCulture);
            IntegrationResult to = new IntegrationResult("test", @"c:\workspace", @"c:\artifacts", request, lastSummary);
            to.StartTime =
                DateTime.ParseExact(totime, PlasticSCM.DATEFORMAT, System.Globalization.CultureInfo.InvariantCulture);

            PlasticSCM plasticscm = new PlasticSCM();
            NetReflector.Read(PLASTICSCM_BASIC_XML, plasticscm);
            string query = string.Format(
                "cm find changesets where branch = 'br:/main' "
                + "and date between '{0}' and '{1}' ", fromtime, totime);
            string dateformat = string.Format(System.Globalization.CultureInfo.CurrentCulture, "--dateformat=\"{0}\" ", PlasticSCM.DATEFORMAT);
            string format = string.Format(System.Globalization.CultureInfo.CurrentCulture, "--format=\"{0}\"", PlasticSCM.FORMAT);

            ProcessInfo info = plasticscm.CreateQueryProcessInfo(from, to);
            Assert.Equal(query + dateformat + format, info.FileName + " " + info.Arguments);
        }

        public void VerifyCreateQueryProcessInfoWithAttributes()
        {
            string fromtime = "01/02/2003 00:00:00";
            string totime = "23/02/2006 23:14:05";
            IntegrationRequest request = new IntegrationRequest(BuildCondition.ForceBuild, "source", null);
            IntegrationSummary lastSummary = new IntegrationSummary(IntegrationStatus.Success, "label", "lastlabel", DateTime.Now);
            IntegrationResult from = new IntegrationResult("test", @"c:\workspace", @"c:\artifacts", request, lastSummary);
            from.StartTime = DateTime.ParseExact(fromtime, PlasticSCM.DATEFORMAT, System.Globalization.CultureInfo.InvariantCulture);
            IntegrationResult to = new IntegrationResult("test", @"c:\workspace", @"c:\artifacts", request, lastSummary);
            to.StartTime = DateTime.ParseExact(totime, PlasticSCM.DATEFORMAT, System.Globalization.CultureInfo.InvariantCulture);

            PlasticSCM plasticscm = new PlasticSCM();
            NetReflector.Read(PLASTICSCM_XML, plasticscm);
            string query = string.Format(
                @"c:\plastic\client\cm.exe find changesets where branch = 'br:/main' "
                + "and date between '{0}' and '{1}' on repository 'mainrep' ", fromtime, totime);
            string dateformat = string.Format(System.Globalization.CultureInfo.CurrentCulture, "--dateformat=\"{0}\" ", PlasticSCM.DATEFORMAT);
            string format = string.Format(System.Globalization.CultureInfo.CurrentCulture, "--format=\"{0}\"", PlasticSCM.FORMAT);

            ProcessInfo info = plasticscm.CreateQueryProcessInfo(from, to);
            Assert.Equal(query + dateformat + format, info.FileName + " " + info.Arguments);

        }

        [Fact]
        public void VerifyCreateLabelProcessInfoBasic()
        {
            IntegrationRequest request = new IntegrationRequest(BuildCondition.ForceBuild, "source", null);
            IntegrationSummary lastSummary = new IntegrationSummary(IntegrationStatus.Success, "label", "lastlabel", DateTime.Now);
            IntegrationResult result = new IntegrationResult("test", @"c:\workspace", @"c:\artifacts", request, lastSummary);
            result.Label = "1";

            //basic check
            PlasticSCM plasticscm = new PlasticSCM();
            NetReflector.Read(PLASTICSCM_BASIC_XML, plasticscm);
            string expected = @"cm mklb ccver-1";
            ProcessInfo info = plasticscm.CreateLabelProcessInfo(result);
            Assert.Equal(expected, info.FileName + " " + info.Arguments);
        }

        [Fact]
        public void VerifyCreateLabelProcessInfoWithAttributes()
        {
            IntegrationRequest request = new IntegrationRequest(BuildCondition.ForceBuild, "source", null);
            IntegrationSummary lastSummary = new IntegrationSummary(IntegrationStatus.Success, "label", "lastlabel", DateTime.Now);
            IntegrationResult result = new IntegrationResult("test", @"c:\workspace", @"c:\artifacts", request, lastSummary);
            result.Label = "1";

            //check with attributes
            PlasticSCM plasticscm = new PlasticSCM();
            NetReflector.Read(PLASTICSCM_XML, plasticscm);
            string expected = @"c:\plastic\client\cm.exe mklb BL1";
            ProcessInfo info = plasticscm.CreateLabelProcessInfo(result);
            Assert.Equal(expected, info.FileName + " " + info.Arguments);
        }

        [Fact]
        public void VerifyLabelProcessInfoBasic()
        {

            IntegrationRequest request = new IntegrationRequest(BuildCondition.ForceBuild, "source", null);
            IntegrationSummary lastSummary = new IntegrationSummary(IntegrationStatus.Success, "label", "lastlabel", DateTime.Now);
            IntegrationResult result = new IntegrationResult("test", @"c:\workspace", @"c:\artifacts", request, lastSummary);
            result.Label = "1";

            PlasticSCM plasticscm = new PlasticSCM();
            NetReflector.Read(PLASTICSCM_BASIC_XML, plasticscm);
            string expected = @"cm label lb:ccver-1 .";
            ProcessInfo info = plasticscm.LabelProcessInfo(result);
            Assert.Equal(expected, info.FileName + " " + info.Arguments);
        }

        [Fact]
        public void VerifyLabelProcessInfoWithAttributes()
        {

            IntegrationRequest request = new IntegrationRequest(BuildCondition.ForceBuild, "source", null);
            IntegrationSummary lastSummary = new IntegrationSummary(IntegrationStatus.Success, "label", "lastlabel", DateTime.Now);
            IntegrationResult result = new IntegrationResult("test", @"c:\workspace", @"c:\artifacts", request, lastSummary);
            result.Label = "1";

            PlasticSCM plasticscm = new PlasticSCM();
            NetReflector.Read(PLASTICSCM_XML, plasticscm);
            string expected = @"c:\plastic\client\cm.exe label lb:BL1 .";
            ProcessInfo info = plasticscm.LabelProcessInfo(result);
            Assert.Equal(expected, info.FileName + " " + info.Arguments);
        }

        [Fact]
        public void VerifyDefaults()
        {
            PlasticSCM plasticscm = new PlasticSCM();

            Assert.Equal("cm", plasticscm.Executable);
            Assert.Equal(string.Empty, plasticscm.Repository);
            Assert.Equal(string.Empty, plasticscm.Branch);
            Assert.Equal(false, plasticscm.Forced);
            Assert.Equal(false, plasticscm.LabelOnSuccess);
            Assert.Equal("ccver-", plasticscm.LabelPrefix);
            Assert.Equal(string.Empty, plasticscm.WorkingDirectory);
        }
    }
}
