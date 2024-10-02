using System.IO;
using Xunit;

namespace ThoughtWorks.CruiseControl.xUnitTests
{
    /// <summary>
    /// Class containing tests for CCNet deployment issues.
    /// Just to make sure that certaing settings are ok before a package is made
    /// ex.: 
    /// ° check that there are xsl files in the xsl files folder
    /// ° check that there are no default passwords
    /// </summary>
    public class CCnetDeploymentTests
    {

        [Fact]
        public void TestForAdminPackageOfWebDashboardIsEmpty()
        {
#if DEBUG
            string configFile = System.IO.Path.Combine(new string[] { Directory.GetCurrentDirectory(), "..", "..", "..", "WebDashboard", "dashboard.config" });
#else
            string configFile = System.IO.Path.Combine(new string[] {Directory.GetCurrentDirectory(), "..", "..", "project", "WebDashboard", "dashboard.config"});
#endif
            Assert.True(System.IO.File.Exists(configFile), "Dashboard.config not found at {configFile}");

            System.Xml.XmlDocument xdoc = new System.Xml.XmlDocument();
            xdoc.Load(configFile);

            var adminPluginNode = xdoc.SelectSingleNode("/dashboard/plugins/farmPlugins/administrationPlugin");

            Assert.False(adminPluginNode is null, $"Admin package configuration not found in dashboard.config at {configFile}");

            var pwd = adminPluginNode.Attributes["password"];

            Assert.False(pwd is null, $"password attribute not defined in admin package in dashboard.config at {configFile}");

            Assert.True("" == pwd.Value, "Password must be empty string, to force users to enter one. No default passwords allowed in distribution");


        }

    }
}
