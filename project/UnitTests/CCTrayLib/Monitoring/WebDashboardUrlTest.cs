using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Monitoring
{
    [TestFixture]
    public class WebDashboardUrlTest
    {
        [Test]
        public void ReturnsCorrectXmlServerReportUrl()
        {
            const string SERVER_URL = @"http://localhost/ccnet";
            WebDashboardUrl webDashboardUrl = new WebDashboardUrl(SERVER_URL);
            ClassicAssert.AreEqual(SERVER_URL + "/XmlServerReport.aspx", webDashboardUrl.XmlServerReport);
            //ClassicAssert.AreEqual(SERVER_URL + "/XmlServerReport.aspx", webDashboardUrl.XmlServerReport);

            // Try again with an extra trailing slash.
            webDashboardUrl = new WebDashboardUrl(SERVER_URL + @"/");
            ClassicAssert.AreEqual(SERVER_URL + "/XmlServerReport.aspx", webDashboardUrl.XmlServerReport);
        }

        [Test]
        public void ReturnsCorrectXmlServerReportAndViewFarmReportUrl()
        {
            const string SERVER_URL = @"http://localhost/ccnet";
            const string SERVER_ALIAS = @"someotherserver";
            WebDashboardUrl webDashboardUrl = new WebDashboardUrl(SERVER_URL, SERVER_ALIAS);
            ClassicAssert.AreEqual(SERVER_URL + "/XmlServerReport.aspx", webDashboardUrl.XmlServerReport);
            ClassicAssert.AreEqual(SERVER_URL + "/server/" + SERVER_ALIAS + "/ViewFarmReport.aspx", webDashboardUrl.ViewFarmReport);

            // Try again with some extra slashes.
            webDashboardUrl = new WebDashboardUrl(SERVER_URL + @"/", @"/" + SERVER_ALIAS + @"/");
            ClassicAssert.AreEqual(SERVER_URL + "/XmlServerReport.aspx", webDashboardUrl.XmlServerReport);
            ClassicAssert.AreEqual(SERVER_URL + "/server/" + SERVER_ALIAS + "/ViewFarmReport.aspx", webDashboardUrl.ViewFarmReport);
        }

        [Test]
        public void ViewFarmReportUrlDefaultsToLocalServer()
        {
            const string SERVER_URL = @"http://localhost/ccnet";
            WebDashboardUrl webDashboardUrl = new WebDashboardUrl(SERVER_URL);
            ClassicAssert.AreEqual(SERVER_URL + "/server/local/ViewFarmReport.aspx", webDashboardUrl.ViewFarmReport);
        }
    }
}
