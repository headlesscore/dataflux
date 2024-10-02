using System;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace ThoughtWorks.CruiseControl.UnitTests
{
    /// <summary>
    /// Class containing tests for CCNet deployment issues.
    /// Just to make sure that certaing settings are ok before a package is made
    /// ex.: 
    /// ° check that there are xsl files in the xsl files folder
    /// ° check that there are no default passwords
    /// </summary>
    [TestFixture]
    public class CCnetDeploymentTests
    {

        [Test]
        public void TestForAdminPackageOfWebDashboardIsEmpty()
        {
#if DEBUG
            string configFile = System.IO.Path.Combine(new string[] {Directory.GetCurrentDirectory(), "..", "..", "..", "WebDashboard", "dashboard.config"});
#else
            string configFile = System.IO.Path.Combine(new string[] {Directory.GetCurrentDirectory(), "..", "..", "project", "WebDashboard", "dashboard.config"});
#endif
            Assert.That(System.IO.File.Exists(configFile), $"Dashboard.config not found at {configFile}");

            System.Xml.XmlDocument xdoc = new System.Xml.XmlDocument();
            xdoc.Load(configFile);

            var adminPluginNode = xdoc.SelectSingleNode("/dashboard/plugins/farmPlugins/administrationPlugin");

            ClassicAssert.IsNotNull(adminPluginNode, "Admin package configuration not found in dashboard.config at {0}", configFile);

            var pwd = adminPluginNode.Attributes["password"];

            ClassicAssert.IsNotNull(pwd, "password attribute not defined in admin packackage in dashboard.config at {0}", configFile);

            ClassicAssert.AreEqual("", pwd.Value, "Password must be empty string, to force users to enter one. No default passwords allowed in distribution");


        }

    }
}
