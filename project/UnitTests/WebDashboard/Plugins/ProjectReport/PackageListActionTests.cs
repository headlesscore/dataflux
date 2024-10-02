namespace ThoughtWorks.CruiseControl.UnitTests.WebDashboard.Plugins.ProjectReport
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Moq;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using ThoughtWorks.CruiseControl.Core.Reporting.Dashboard.Navigation;
    using ThoughtWorks.CruiseControl.Remote;
    using ThoughtWorks.CruiseControl.WebDashboard.IO;
    using ThoughtWorks.CruiseControl.WebDashboard.MVC;
    using ThoughtWorks.CruiseControl.WebDashboard.MVC.View;
    using ThoughtWorks.CruiseControl.WebDashboard.Plugins.ProjectReport;
    using ThoughtWorks.CruiseControl.WebDashboard.ServerConnection;

    public class PackageListActionTests
    {
        #region Private fields
        private MockRepository mocks;
        #endregion

        #region Setup
        [SetUp]
        public void Setup()
        {
            this.mocks = new MockRepository(MockBehavior.Default);
        }
        #endregion

        #region Tests
        [Test]
        public void ExecuteGeneratesList()
        {
            var package1 = new PackageDetails
            {
                BuildLabel = "label1",
                DateTime = new DateTime(2010, 1, 2, 3, 4, 5),
                FileName = "somewhere\\thefile.type",
                Name = "theName",
                NumberOfFiles = 2,
                Size = 1
            };
            var package2 = new PackageDetails
            {
                BuildLabel = "label2",
                DateTime = new DateTime(2010, 10, 9, 8, 7, 6),
                FileName = "anotherfile.txt",
                Name = "secondName",
                NumberOfFiles = 5,
                Size = 9876
            };
            var package3 = new PackageDetails
            {
                BuildLabel = "label3",
                DateTime = new DateTime(2010, 1, 1, 2, 3, 5),
                FileName = "anotherfile.txt",
                Name = "secondName",
                NumberOfFiles = 5,
                Size = 1234567890
            };
            var packages = new PackageDetails[] { package1, package2, package3 };
            var projectName = "Test Project";
            var farmService = this.mocks.Create<IFarmService>(MockBehavior.Strict).Object;
            var viewGenerator = this.mocks.Create<IVelocityViewGenerator>(MockBehavior.Strict).Object;
            var cruiseRequest = this.mocks.Create<ICruiseRequest>(MockBehavior.Strict).Object;
            var projectSpec = this.mocks.Create<IProjectSpecifier>(MockBehavior.Strict).Object;
            Mock.Get(cruiseRequest).SetupGet(_cruiseRequest => _cruiseRequest.ProjectName).Returns(projectName);
            Mock.Get(cruiseRequest).SetupGet(_cruiseRequest => _cruiseRequest.ProjectSpecifier).Returns(projectSpec);
            Mock.Get(cruiseRequest).Setup(_cruiseRequest => _cruiseRequest.RetrieveSessionToken()).Returns((string)null);
            Mock.Get(farmService).Setup(_farmService => _farmService.RetrievePackageList(projectSpec, null)).Returns(packages);
            Mock.Get(viewGenerator).Setup(_viewGenerator => _viewGenerator.GenerateView(It.IsAny<string>(), It.IsAny<Hashtable>()))
                .Callback<string, Hashtable>((n, ht) =>
                {
                    ClassicAssert.AreEqual("PackageList.vm", n);
                    ClassicAssert.IsNotNull(ht);
                    ClassicAssert.IsTrue(ht.ContainsKey("projectName"));
                    ClassicAssert.AreEqual(projectName, ht["projectName"]);
                    ClassicAssert.IsTrue(ht.ContainsKey("packages"));
                    ClassicAssert.IsInstanceOf<List<PackageListAction.PackageDisplay>>(ht["packages"]);
                    ClassicAssert.IsTrue(true);
                    ClassicAssert.IsTrue(true);
                })
                .Returns(new HtmlFragmentResponse("from nVelocity")).Verifiable();

            var plugin = new PackageListAction(viewGenerator, farmService);
            var response = plugin.Execute(cruiseRequest);

            this.mocks.VerifyAll();
            ClassicAssert.IsInstanceOf<HtmlFragmentResponse>(response);
            var actual = response as HtmlFragmentResponse;
            var expected = "from nVelocity";
            ClassicAssert.AreEqual(expected, actual.ResponseFragment);
        }
        #endregion
    }
}
