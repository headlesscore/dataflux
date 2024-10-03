using Xunit;

using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Monitoring
{
    public class WebDashboardUrlTest
    {
        [Fact]
        public void ReturnsCorrectXmlServerReportUrl()
        {
            const string SERVER_URL = @"http://localhost/ccnet";
            WebDashboardUrl webDashboardUrl = new WebDashboardUrl(SERVER_URL);
            Assert.Equal(SERVER_URL + "/XmlServerReport.aspx", webDashboardUrl.XmlServerReport);
            //Assert.Equal(SERVER_URL + "/XmlServerReport.aspx", webDashboardUrl.XmlServerReport);

            // Try again with an extra trailing slash.
            webDashboardUrl = new WebDashboardUrl(SERVER_URL + @"/");
            Assert.Equal(SERVER_URL + "/XmlServerReport.aspx", webDashboardUrl.XmlServerReport);
        }

        [Fact]
        public void ReturnsCorrectXmlServerReportAndViewFarmReportUrl()
        {
            const string SERVER_URL = @"http://localhost/ccnet";
            const string SERVER_ALIAS = @"someotherserver";
            WebDashboardUrl webDashboardUrl = new WebDashboardUrl(SERVER_URL, SERVER_ALIAS);
            Assert.Equal(SERVER_URL + "/XmlServerReport.aspx", webDashboardUrl.XmlServerReport);
            Assert.Equal(SERVER_URL + "/server/" + SERVER_ALIAS + "/ViewFarmReport.aspx", webDashboardUrl.ViewFarmReport);

            // Try again with some extra slashes.
            webDashboardUrl = new WebDashboardUrl(SERVER_URL + @"/", @"/" + SERVER_ALIAS + @"/");
            Assert.Equal(SERVER_URL + "/XmlServerReport.aspx", webDashboardUrl.XmlServerReport);
            Assert.Equal(SERVER_URL + "/server/" + SERVER_ALIAS + "/ViewFarmReport.aspx", webDashboardUrl.ViewFarmReport);
        }

        [Fact]
        public void ViewFarmReportUrlDefaultsToLocalServer()
        {
            const string SERVER_URL = @"http://localhost/ccnet";
            WebDashboardUrl webDashboardUrl = new WebDashboardUrl(SERVER_URL);
            Assert.Equal(SERVER_URL + "/server/local/ViewFarmReport.aspx", webDashboardUrl.ViewFarmReport);
        }
    }
}
