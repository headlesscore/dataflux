namespace ThoughtWorks.CruiseControl.UnitTests
open System.IO;
open Xunit;

module Tests =
    type CCnetDeploymentTests () =

        [<Fact>]
        member this.TestForAdminPackageOfWebDashboardIsEmpty() =
            System.Environment.CurrentDirectory <- @"C:\Users\Administrator\Desktop\dataflux\project\FSMSUnitTests";
            let configFile = System.IO.Path.Combine([|Directory.GetCurrentDirectory(); ".."; "WebDashboard"; "dashboard.config"|])
            Assert.True(System.IO.File.Exists(configFile), $"Dashboard.config not found at {configFile}")

            let xdoc = new System.Xml.XmlDocument()
            xdoc.Load(configFile)

            let adminPluginNode = xdoc.SelectSingleNode("/dashboard/plugins/farmPlugins/administrationPlugin")

            Assert.True(not(adminPluginNode = null), $"Admin package configuration not found in dashboard.config at {configFile}")

            let pwd = adminPluginNode.Attributes["password"]

            Assert.True(not(pwd = null), $"password attribute not defined in admin packackage in dashboard.config at {configFile}")

            Assert.True(pwd.Value = "", "Password must be empty string, to force users to enter one. No default passwords allowed in distribution")
