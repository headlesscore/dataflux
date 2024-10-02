using System;
using System.Collections.Generic;
using System.Drawing;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Reporting.Dashboard.Navigation;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.Remote;
using ThoughtWorks.CruiseControl.Remote.Parameters;
using ThoughtWorks.CruiseControl.UnitTests.Core;
using ThoughtWorks.CruiseControl.WebDashboard.Dashboard;
using ThoughtWorks.CruiseControl.WebDashboard.Plugins.ProjectReport;
using ThoughtWorks.CruiseControl.WebDashboard.Resources;
using ThoughtWorks.CruiseControl.WebDashboard.ServerConnection;

namespace ThoughtWorks.CruiseControl.UnitTests.WebDashboard.Dashboard
{
	[TestFixture]
	public class ProjectGridTest
	{
        private MockRepository mocks = new MockRepository(MockBehavior.Default);
		private ProjectGrid projectGrid;
		private ICruiseUrlBuilder urlBuilderMock;
		private Mock<ILinkFactory> linkFactoryMock;
		private IAbsoluteLink projectLink;
		private IServerSpecifier serverSpecifier;
		private IProjectSpecifier projectSpecifier;

		[SetUp]
		public void Setup()
		{
            urlBuilderMock = mocks.Create<ICruiseUrlBuilder>().Object;
            Mock.Get(urlBuilderMock).Setup(_urlBuilderMock => _urlBuilderMock.BuildProjectUrl(It.IsAny<string>(), It.IsAny<IProjectSpecifier>()))
                .Returns("myLinkUrl");
			linkFactoryMock = new Mock<ILinkFactory>();
			projectGrid = new ProjectGrid();

			serverSpecifier = new DefaultServerSpecifier("server");
			projectSpecifier = new DefaultProjectSpecifier(serverSpecifier, "my project");

			projectLink = new GeneralAbsoluteLink("myLinkText", "myLinkUrl");
		}

		private void VerifyAll()
		{
			linkFactoryMock.Verify();
		}

		private void SetupProjectLinkExpectation()
		{
			SetupProjectLinkExpectation(projectSpecifier);
		}

		private void SetupProjectLinkExpectation(IProjectSpecifier projectSpecifierForLink)
		{
            linkFactoryMock.Setup(factory => factory.CreateProjectLink(It.IsAny<IProjectSpecifier>(), It.IsAny<string>())).Returns(projectLink);
		}

		[Test]
		public void ShouldReturnEmptyListOfRowsWhenNoProjectStatusesAvailable()
		{
			ProjectStatusOnServer[] statusses = new ProjectStatusOnServer[0];

            ClassicAssert.AreEqual(0, projectGrid.GenerateProjectGridRows(statusses, "myAction", ProjectGridSortColumn.Name, true, "", urlBuilderMock, new Translations("en-US")).Length);
            //ClassicAssert.AreEqual(0, projectGrid.GenerateProjectGridRows(statusses, "myAction", ProjectGridSortColumn.Name, true, "", urlBuilderMock, new Translations("en-US")).Length);

            VerifyAll();
		}

		[Test]
		public void ShouldCopyProjectNameToProjectRow()
		{
			// Setup
			ProjectStatus projectStatus1 = ProjectStatusFixture.New(projectSpecifier.ProjectName);
			ProjectStatusOnServer[] statusses = new ProjectStatusOnServer[]
				{
					new ProjectStatusOnServer(projectStatus1, serverSpecifier)
				};

			// Execute
			SetupProjectLinkExpectation();
            ProjectGridRow[] rows = projectGrid.GenerateProjectGridRows(statusses, "myAction", ProjectGridSortColumn.Name, true, "", urlBuilderMock, new Translations("en-US"));

			// Verify
			ClassicAssert.AreEqual(1, rows.Length);
			ClassicAssert.AreEqual(projectSpecifier.ProjectName, rows[0].Name);
			VerifyAll();
		}

		[Test]
		public void ShouldHandleResultsWithNoBuildLabel()
		{
			// Setup
			ProjectStatus projectStatus1 = ProjectStatusFixture.New(projectSpecifier.ProjectName, null);
			ProjectStatusOnServer[] statusses = new ProjectStatusOnServer[]
				{
					new ProjectStatusOnServer(projectStatus1, serverSpecifier)
				};

			// Execute
			SetupProjectLinkExpectation();
            ProjectGridRow[] rows = projectGrid.GenerateProjectGridRows(statusses, "myAction", ProjectGridSortColumn.Name, true, "", urlBuilderMock, new Translations("en-US"));

			// Verify
			ClassicAssert.AreEqual(1, rows.Length);
			ClassicAssert.AreEqual("no build available", rows[0].LastBuildLabel);
			VerifyAll();
		}

		[Test]
		public void ShouldCopyBuildStatusToProjectRow()
		{
			// Setup
			ProjectStatusOnServer[] statusses = new ProjectStatusOnServer[]
				{
					new ProjectStatusOnServer(ProjectStatusFixture.New(projectSpecifier.ProjectName, IntegrationStatus.Success), serverSpecifier)
				};

			SetupProjectLinkExpectation();

			// Execute
            ProjectGridRow[] rows = projectGrid.GenerateProjectGridRows(statusses, "myAction", ProjectGridSortColumn.Name, true, "", urlBuilderMock, new Translations("en-US"));

			// Verify
			ClassicAssert.AreEqual("Success", rows[0].BuildStatus);
			ClassicAssert.AreEqual(Color.Green.Name, rows[0].BuildStatusHtmlColor);

			// Setup
			statusses = new ProjectStatusOnServer[]
				{
					new ProjectStatusOnServer(ProjectStatusFixture.New(projectSpecifier.ProjectName, IntegrationStatus.Failure), serverSpecifier)
				};
			SetupProjectLinkExpectation();

			// Execute
            rows = projectGrid.GenerateProjectGridRows(statusses, "myAction", ProjectGridSortColumn.Name, true, "", urlBuilderMock, new Translations("en-US"));

			// Verify
			ClassicAssert.AreEqual("Failure", rows[0].BuildStatus);
			ClassicAssert.AreEqual(Color.Red.Name, rows[0].BuildStatusHtmlColor);

			// Setup
			statusses = new ProjectStatusOnServer[]
				{
					new ProjectStatusOnServer(ProjectStatusFixture.New(projectSpecifier.ProjectName, IntegrationStatus.Unknown), serverSpecifier)
				};
			SetupProjectLinkExpectation();

			// Execute
            rows = projectGrid.GenerateProjectGridRows(statusses, "myAction", ProjectGridSortColumn.Name, true, "", urlBuilderMock, new Translations("en-US"));

			// Verify
			ClassicAssert.AreEqual("Unknown", rows[0].BuildStatus);
			ClassicAssert.AreEqual(Color.Blue.Name, rows[0].BuildStatusHtmlColor);

			// Setup
			statusses = new ProjectStatusOnServer[]
				{
					new ProjectStatusOnServer(ProjectStatusFixture.New(projectSpecifier.ProjectName, IntegrationStatus.Exception), serverSpecifier)
				};
			SetupProjectLinkExpectation();

			// Execute
            rows = projectGrid.GenerateProjectGridRows(statusses, "myAction", ProjectGridSortColumn.Name, true, "", urlBuilderMock, new Translations("en-US"));

			// Verify
			ClassicAssert.AreEqual("Exception", rows[0].BuildStatus);
			ClassicAssert.AreEqual(Color.Red.Name, rows[0].BuildStatusHtmlColor);

			VerifyAll();
		}

		[Test]
		public void ShouldCopyLastBuildDateToProjectRow()
		{
			// Setup
			DateTime date = DateTime.Today;
			ProjectStatusOnServer[] statusses = new ProjectStatusOnServer[]
				{
					new ProjectStatusOnServer(ProjectStatusFixture.New(projectSpecifier.ProjectName, IntegrationStatus.Success, date), serverSpecifier)
				};
			SetupProjectLinkExpectation();

			// Execute
            ProjectGridRow[] rows = projectGrid.GenerateProjectGridRows(statusses, "myAction", ProjectGridSortColumn.Name, true, "", urlBuilderMock, new Translations("en-US"));

			// Verify
			ClassicAssert.AreEqual(DateUtil.FormatDate(date), rows[0].LastBuildDate);
			VerifyAll();
		}

		[Test]
		public void ShouldCopyProjectStatusToProjectRow()
		{
			// Setup
			ProjectStatus projectStatus1 = new ProjectStatus(projectSpecifier.ProjectName, "category",
			                                                 ProjectActivity.Sleeping, IntegrationStatus.Success, 
                                                             ProjectIntegratorState.Running, "url", 
                                                             DateTime.Today, "my label", null,
                                                             DateTime.Today, "building", "", 0, new List<ParameterBase>());
			ProjectStatusOnServer[] statusses = new ProjectStatusOnServer[]
				{
					new ProjectStatusOnServer(projectStatus1, serverSpecifier)
				};
			SetupProjectLinkExpectation();

			// Execute
            ProjectGridRow[] rows = projectGrid.GenerateProjectGridRows(statusses, "myAction", ProjectGridSortColumn.Name, true, "", urlBuilderMock, new Translations("en-US"));

			// Verify
			ClassicAssert.AreEqual("Running", rows[0].Status);
           
			VerifyAll();

			// Setup
			projectStatus1 = new ProjectStatus(projectSpecifier.ProjectName, "category",
			                                   ProjectActivity.Sleeping, IntegrationStatus.Success, 
                                               ProjectIntegratorState.Stopped, "url",
                                               DateTime.Today, "my label", null, DateTime.Today, "", "", 0, new List<ParameterBase>());
			statusses = new ProjectStatusOnServer[]
				{
					new ProjectStatusOnServer(projectStatus1, serverSpecifier)
				};
			SetupProjectLinkExpectation();

			// Execute
            rows = projectGrid.GenerateProjectGridRows(statusses, "myAction", ProjectGridSortColumn.Name, true, "", urlBuilderMock, new Translations("en-US"));

			// Verify
			ClassicAssert.AreEqual("Stopped", rows[0].Status);
			VerifyAll();
		}

		[Test]
		public void ShouldCopyProjectActivityToProjectRow()
		{
			// Setup
			ProjectStatus projectStatus1 = new ProjectStatus(projectSpecifier.ProjectName, "category",
			                                                 ProjectActivity.Sleeping, IntegrationStatus.Success, 
                                                             ProjectIntegratorState.Running, "url",
                                                             DateTime.Today, "my label", null, DateTime.Today, "", "", 0, new List<ParameterBase>());
			ProjectStatusOnServer[] statusses = new ProjectStatusOnServer[]
				{
					new ProjectStatusOnServer(projectStatus1, serverSpecifier)
				};
			SetupProjectLinkExpectation();

			// Execute
            ProjectGridRow[] rows = projectGrid.GenerateProjectGridRows(statusses, "myAction", ProjectGridSortColumn.Name, true, "", urlBuilderMock, new Translations("en-US"));

			// Verify
			ClassicAssert.AreEqual("Sleeping", rows[0].Activity);
			VerifyAll();

			// Setup
			projectStatus1 = new ProjectStatus(projectSpecifier.ProjectName, "category",
			                                   ProjectActivity.CheckingModifications, IntegrationStatus.Success, 
                                               ProjectIntegratorState.Stopped, "url", DateTime.Today, "my label",
                                               null, DateTime.Today, "", "", 0, new List<ParameterBase>());
			statusses = new ProjectStatusOnServer[]
				{
					new ProjectStatusOnServer(projectStatus1, new DefaultServerSpecifier("server"))
				};
			SetupProjectLinkExpectation();

			// Execute
            rows = projectGrid.GenerateProjectGridRows(statusses, "myAction", ProjectGridSortColumn.Name, true, "", urlBuilderMock, new Translations("en-US"));

			// Verify
			ClassicAssert.AreEqual("CheckingModifications", rows[0].Activity);
			VerifyAll();
		}

		[Test]
		public void ShouldCopyLastBuildLabelToProjectRow()
		{
			// Setup
			DateTime date = DateTime.Today;
			ProjectStatus projectStatus1 = new ProjectStatus(projectSpecifier.ProjectName, "category",
			                                                 ProjectActivity.Sleeping, IntegrationStatus.Success,
                                                             ProjectIntegratorState.Running, "url", date,
                                                             "my label", null, DateTime.Today, "", "", 0, new List<ParameterBase>());

			ProjectStatusOnServer[] statusses = new ProjectStatusOnServer[]
				{
					new ProjectStatusOnServer(projectStatus1, serverSpecifier)
				};
			SetupProjectLinkExpectation();

			// Execute
            ProjectGridRow[] rows = projectGrid.GenerateProjectGridRows(statusses, "myAction", ProjectGridSortColumn.Name, true, "", urlBuilderMock, new Translations("en-US"));

			// Verify
			ClassicAssert.AreEqual("my label", rows[0].LastBuildLabel);
			VerifyAll();
		}

		[Test]
		public void ShouldCreateLinkToProjectReport()
		{
			// Setup
			ProjectStatus projectStatus1 = new ProjectStatus(projectSpecifier.ProjectName, "category",
			                                                 ProjectActivity.Sleeping, IntegrationStatus.Success, 
                                                             ProjectIntegratorState.Running, "url", DateTime.Today,
                                                             "1", null, DateTime.Today, "", "", 0, new List<ParameterBase>());
			
			ProjectStatusOnServer[] statusses = new ProjectStatusOnServer[]
				{
					new ProjectStatusOnServer(projectStatus1, serverSpecifier)
				};
			SetupProjectLinkExpectation();

			// Execute
            ProjectGridRow[] rows = projectGrid.GenerateProjectGridRows(statusses, "myAction", ProjectGridSortColumn.Name, true, "", urlBuilderMock, new Translations("en-US"));

			// Verify
			ClassicAssert.AreEqual("myLinkUrl", rows[0].Url);
			VerifyAll();
		}

		[Test]
		public void ShouldDisplayCurrentProjectMessagesInProjectGridRow()
		{
			// Setup
			ProjectStatus projectStatus1 = new ProjectStatus(projectSpecifier.ProjectName, "category",
				                                            ProjectActivity.Sleeping, IntegrationStatus.Success, 
                                                            ProjectIntegratorState.Running, "url", DateTime.Today,
                                                            "my label", null, DateTime.Today, "", "", 0, new List<ParameterBase>());

			projectStatus1.Messages = new Message[1] {new Message("Test Message")};

			ProjectStatusOnServer[] statusses = new ProjectStatusOnServer[]
				{
					new ProjectStatusOnServer(projectStatus1, serverSpecifier)
				};
			SetupProjectLinkExpectation();

			// Execute
            ProjectGridRow[] rows = projectGrid.GenerateProjectGridRows(statusses, "myAction", ProjectGridSortColumn.Name, true, "", urlBuilderMock, new Translations("en-US"));

			// Verify
			ClassicAssert.IsNotNull(rows[0].CurrentMessage);
			ClassicAssert.AreEqual("Test Message", rows[0].CurrentMessage);
			VerifyAll();

			// Setup
			projectStatus1 = new ProjectStatus(projectSpecifier.ProjectName, "category",
				                                ProjectActivity.Sleeping, IntegrationStatus.Success, 
                                                ProjectIntegratorState.Stopped, "url", DateTime.Today,
                                                "my label", null, DateTime.Today, "", "", 0, new List<ParameterBase>());

			projectStatus1.Messages = new Message[2] {new Message(string.Empty), new Message("Second Message")};
            
			statusses = new ProjectStatusOnServer[]
				{
					new ProjectStatusOnServer(projectStatus1, serverSpecifier)
				};
			SetupProjectLinkExpectation();

			// Execute
            rows = projectGrid.GenerateProjectGridRows(statusses, "myAction", ProjectGridSortColumn.Name, true, "", urlBuilderMock, new Translations("en-US"));

			// Verify
			ClassicAssert.IsNotNull(rows[0].CurrentMessage);
			ClassicAssert.AreEqual("Second Message", rows[0].CurrentMessage);
			VerifyAll();
		}

		[Test]
		public void ShouldCopyProjectCategoryToProjectRow()
		{
			// Setup
			ProjectStatus projectStatus1 = new ProjectStatus(projectSpecifier.ProjectName, "category",
				                                                ProjectActivity.Sleeping, IntegrationStatus.Success, 
                                                                ProjectIntegratorState.Running, "url", DateTime.Today,
                                                                "my label", null, DateTime.Today, "", "", 0, new List<ParameterBase>());


			ProjectStatusOnServer[] statusses = new ProjectStatusOnServer[]
				{
					new ProjectStatusOnServer(projectStatus1, serverSpecifier)
				};
			SetupProjectLinkExpectation();

			// Execute
            ProjectGridRow[] rows = projectGrid.GenerateProjectGridRows(statusses, "myAction", ProjectGridSortColumn.Name, true, "", urlBuilderMock, new Translations("en-US"));

			// Verify
			ClassicAssert.AreEqual("category", rows[0].Category);
			VerifyAll();

			// Setup
			projectStatus1 = new ProjectStatus(projectSpecifier.ProjectName, "category1",
				                                ProjectActivity.Sleeping, IntegrationStatus.Success, 
                                                ProjectIntegratorState.Stopped, "url", DateTime.Today,
                                                "my label", null, DateTime.Today, "", "", 0, new List<ParameterBase>());


			statusses = new ProjectStatusOnServer[]
				{
					new ProjectStatusOnServer(projectStatus1, serverSpecifier)
				};
			SetupProjectLinkExpectation();

			// Execute
            rows = projectGrid.GenerateProjectGridRows(statusses, "myAction", ProjectGridSortColumn.Name, true, "", urlBuilderMock, new Translations("en-US"));

			// Verify
			ClassicAssert.AreEqual("category1", rows[0].Category);
			VerifyAll();
		}

		[Test]
		public void ShouldReturnProjectsSortedByNameIfNameColumnSpecifiedAsSortSeed()
		{
			// Setup
			IProjectSpecifier projectA = new DefaultProjectSpecifier(serverSpecifier, "a");
			IProjectSpecifier projectB = new DefaultProjectSpecifier(serverSpecifier, "b");

			ProjectStatus projectStatus1 = new ProjectStatus("a", "category",
			                                                 ProjectActivity.Sleeping, IntegrationStatus.Success, 
                                                             ProjectIntegratorState.Running, "url",
                                                             DateTime.Today, "1", null, DateTime.Today, "", "", 0, new List<ParameterBase>());
			ProjectStatus projectStatus2 = new ProjectStatus("b", "category",
			                                                 ProjectActivity.Sleeping, IntegrationStatus.Success, 
                                                             ProjectIntegratorState.Running, "url",
                                                             DateTime.Today, "1", null, DateTime.Today, "", "", 0, new List<ParameterBase>());
			ProjectStatusOnServer[] statusses = new ProjectStatusOnServer[]
				{
					new ProjectStatusOnServer(projectStatus1, serverSpecifier),
					new ProjectStatusOnServer(projectStatus2, serverSpecifier)
				};
			SetupProjectLinkExpectation(projectA);
			SetupProjectLinkExpectation(projectB);

			// Execute
            ProjectGridRow[] rows = projectGrid.GenerateProjectGridRows(statusses, "myAction", ProjectGridSortColumn.Name, true, "", urlBuilderMock, new Translations("en-US"));

			// Verify
			ClassicAssert.AreEqual(2, rows.Length);
			ClassicAssert.AreEqual("a", rows[0].Name);
			ClassicAssert.AreEqual("b", rows[1].Name);

			// Setup
			SetupProjectLinkExpectation(projectA);
			SetupProjectLinkExpectation(projectB);

			// Execute
            rows = projectGrid.GenerateProjectGridRows(statusses, "myAction", ProjectGridSortColumn.Name, false, "", urlBuilderMock, new Translations("en-US"));

			// Verify
			ClassicAssert.AreEqual(2, rows.Length);
			ClassicAssert.AreEqual("b", rows[0].Name);
			ClassicAssert.AreEqual("a", rows[1].Name);

			VerifyAll();
		}

		[Test]
		public void ShouldReturnProjectsSortedByLastBuildDateIfLastBuildDateColumnSpecifiedAsSortSeed()
		{
			// Setup
			IProjectSpecifier projectA = new DefaultProjectSpecifier(serverSpecifier, "a");
			IProjectSpecifier projectB = new DefaultProjectSpecifier(serverSpecifier, "b");

			ProjectStatus projectStatus1 = new ProjectStatus("b", "category",
			                                                 ProjectActivity.Sleeping, IntegrationStatus.Success, 
                                                             ProjectIntegratorState.Running, "url", DateTime.Today,
                                                             "1", null, DateTime.Today, "", "", 0, new List<ParameterBase>());
			ProjectStatus projectStatus2 = new ProjectStatus("a", "category",
			                                                 ProjectActivity.Sleeping, IntegrationStatus.Success, 
                                                             ProjectIntegratorState.Running, "url", 
                                                             DateTime.Today.AddHours(1), "1", null,
                                                             DateTime.Today, "", "", 0, new List<ParameterBase>());
			ProjectStatusOnServer[] statusses = new ProjectStatusOnServer[]
				{
					new ProjectStatusOnServer(projectStatus1, serverSpecifier),
					new ProjectStatusOnServer(projectStatus2, serverSpecifier)
				};
			SetupProjectLinkExpectation(projectB);
			SetupProjectLinkExpectation(projectA);

			// Execute
            ProjectGridRow[] rows = projectGrid.GenerateProjectGridRows(statusses, "myAction", ProjectGridSortColumn.LastBuildDate, true, "", urlBuilderMock, new Translations("en-US"));

			// Verify
			ClassicAssert.AreEqual(2, rows.Length);
			ClassicAssert.AreEqual("b", rows[0].Name);
			ClassicAssert.AreEqual("a", rows[1].Name);

			// Setup
			SetupProjectLinkExpectation(projectB);
			SetupProjectLinkExpectation(projectA);

			// Execute
            rows = projectGrid.GenerateProjectGridRows(statusses, "myAction", ProjectGridSortColumn.LastBuildDate, false, "", urlBuilderMock, new Translations("en-US"));

			// Verify
			ClassicAssert.AreEqual(2, rows.Length);
			ClassicAssert.AreEqual("a", rows[0].Name);
			ClassicAssert.AreEqual("b", rows[1].Name);

			VerifyAll();
		}

		[Test]
		public void ShouldReturnProjectsSortedByBuildStatusIfBuildStatusColumnSpecifiedAsSortSeed()
		{
			// Setup
			IProjectSpecifier projectA = new DefaultProjectSpecifier(serverSpecifier, "a");
			IProjectSpecifier projectB = new DefaultProjectSpecifier(serverSpecifier, "b");
			ProjectStatus projectStatus1 = new ProjectStatus("a", "category",
			                                                 ProjectActivity.Sleeping, IntegrationStatus.Success, 
                                                             ProjectIntegratorState.Running, "url", DateTime.Today,
                                                             "1", null, DateTime.Today, "", "", 0, new List<ParameterBase>());
			ProjectStatus projectStatus2 = new ProjectStatus("b", "category",
			                                                 ProjectActivity.Sleeping, IntegrationStatus.Failure, 
                                                             ProjectIntegratorState.Running, "url",
                                                             DateTime.Today.AddHours(1), "1", null, DateTime.Today, "", "", 0, new List<ParameterBase>());
			ProjectStatusOnServer[] statusses = new ProjectStatusOnServer[]
				{
					new ProjectStatusOnServer(projectStatus1, serverSpecifier),
					new ProjectStatusOnServer(projectStatus2, serverSpecifier)
				};
			SetupProjectLinkExpectation(projectA);
			SetupProjectLinkExpectation(projectB);

			// Execute
            ProjectGridRow[] rows = projectGrid.GenerateProjectGridRows(statusses, "myAction", ProjectGridSortColumn.BuildStatus, true, "", urlBuilderMock, new Translations("en-US"));

			// Verify
			ClassicAssert.AreEqual(2, rows.Length);
			ClassicAssert.AreEqual("b", rows[0].Name);
			ClassicAssert.AreEqual("a", rows[1].Name);

			// Setup
			SetupProjectLinkExpectation(projectA);
			SetupProjectLinkExpectation(projectB);

			// Execute
            rows = projectGrid.GenerateProjectGridRows(statusses, "myAction", ProjectGridSortColumn.BuildStatus, false, "", urlBuilderMock, new Translations("en-US"));

			// Verify
			ClassicAssert.AreEqual(2, rows.Length);
			ClassicAssert.AreEqual("a", rows[0].Name);
			ClassicAssert.AreEqual("b", rows[1].Name);

			VerifyAll();
		}

		[Test]
		public void ShouldReturnProjectsSortedByServerIfServerNameColumnSpecifiedAsSortSeed()
		{
			// Setup
			IServerSpecifier serverSpecifierA = new DefaultServerSpecifier("Aserver");
			IServerSpecifier serverSpecifierB = new DefaultServerSpecifier("Bserver");
			IProjectSpecifier projectA = new DefaultProjectSpecifier(serverSpecifierA, "a");
			IProjectSpecifier projectB = new DefaultProjectSpecifier(serverSpecifierB, "b");

			ProjectStatus projectStatus1 = new ProjectStatus("a", "category",
				ProjectActivity.Sleeping, IntegrationStatus.Success, ProjectIntegratorState.Running, "url",
                DateTime.Today, "1", null, DateTime.Today, "", "", 0, new List<ParameterBase>());
			ProjectStatus projectStatus2 = new ProjectStatus("b", "category",
				ProjectActivity.Sleeping, IntegrationStatus.Failure, ProjectIntegratorState.Running, "url",
                DateTime.Today.AddHours(1), "1", null, DateTime.Today, "", "", 0, new List<ParameterBase>());
			ProjectStatusOnServer[] statusses = new ProjectStatusOnServer[]
				{
					new ProjectStatusOnServer(projectStatus1, serverSpecifierA),
					new ProjectStatusOnServer(projectStatus2, serverSpecifierB)
				};
			SetupProjectLinkExpectation(projectA);
			SetupProjectLinkExpectation(projectB);

			// Execute
            ProjectGridRow[] rows = projectGrid.GenerateProjectGridRows(statusses, "myAction", ProjectGridSortColumn.ServerName, true, "", urlBuilderMock, new Translations("en-US"));

			// Verify
			ClassicAssert.AreEqual(2, rows.Length);
			ClassicAssert.AreEqual("a", rows[0].Name);
			ClassicAssert.AreEqual("b", rows[1].Name);

			// Setup
			SetupProjectLinkExpectation(projectA);
			SetupProjectLinkExpectation(projectB);

			// Execute
            rows = projectGrid.GenerateProjectGridRows(statusses, "myAction", ProjectGridSortColumn.ServerName, false, "", urlBuilderMock, new Translations("en-US"));

			// Verify
			ClassicAssert.AreEqual(2, rows.Length);
			ClassicAssert.AreEqual("b", rows[0].Name);
			ClassicAssert.AreEqual("a", rows[1].Name);

			VerifyAll();
		}

		[Test]
		public void ShouldReturnProjectsSortedByCategoryIfCategoryColumnSpecifiedAsSortSeed()
		{
			// Setup
			IProjectSpecifier projectA = new DefaultProjectSpecifier(serverSpecifier, "A");
			IProjectSpecifier projectB = new DefaultProjectSpecifier(serverSpecifier, "B");
			IProjectSpecifier projectC = new DefaultProjectSpecifier(serverSpecifier, "C");

			ProjectStatus projectStatusA = new ProjectStatus("A", "CategoryX", ProjectActivity.Sleeping, 
                                                    IntegrationStatus.Success, ProjectIntegratorState.Running, "url",
                                                    DateTime.Today, "1", null, DateTime.Today, "", "", 0, new List<ParameterBase>());
			ProjectStatus projectStatusB = new ProjectStatus("B", "CategoryY", ProjectActivity.Sleeping, 
                                                    IntegrationStatus.Success, ProjectIntegratorState.Running, "url",
                                                    DateTime.Today, "1", null, DateTime.Today, "", "", 0, new List<ParameterBase>());
			ProjectStatus projectStatusC = new ProjectStatus("C", "CategoryX", ProjectActivity.Sleeping, 
                                                    IntegrationStatus.Success, ProjectIntegratorState.Running, "url",
                                                    DateTime.Today, "1", null, DateTime.Today, "", "", 0, new List<ParameterBase>());

			ProjectStatusOnServer[] status = new ProjectStatusOnServer[]
				{
					new ProjectStatusOnServer(projectStatusA, serverSpecifier),
					new ProjectStatusOnServer(projectStatusB, serverSpecifier),
					new ProjectStatusOnServer(projectStatusC, serverSpecifier)
				};
			SetupProjectLinkExpectation(projectA);
			SetupProjectLinkExpectation(projectB);
			SetupProjectLinkExpectation(projectC);

			// Execute
            ProjectGridRow[] rows = projectGrid.GenerateProjectGridRows(status, "myAction", ProjectGridSortColumn.Category, true, "", urlBuilderMock, new Translations("en-US"));

			// Verify
			ClassicAssert.AreEqual(3, rows.Length);
			ClassicAssert.AreEqual("A", rows[0].Name);
			ClassicAssert.AreEqual("C", rows[1].Name);
			ClassicAssert.AreEqual("B", rows[2].Name);

			// Setup
			SetupProjectLinkExpectation(projectA);
			SetupProjectLinkExpectation(projectB);
			SetupProjectLinkExpectation(projectC);

			// Execute
            rows = projectGrid.GenerateProjectGridRows(status, "myAction", ProjectGridSortColumn.Category, false, "", urlBuilderMock, new Translations("en-US"));

			// Verify
			ClassicAssert.AreEqual(3, rows.Length);
			ClassicAssert.AreEqual("B", rows[0].Name);
			ClassicAssert.AreEqual("A", rows[1].Name);
			ClassicAssert.AreEqual("C", rows[2].Name);

			VerifyAll();
		}
	}
}
