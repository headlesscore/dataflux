using Moq;
using Xunit;
using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.CCTrayLib.Presentation;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Presentation
{
	public class AddProjectsTest
	{
		// This isn't really a test, just a quick way to invoke and display the
		// dialog for interactive testing
		[Fact(Skip = "This isn't really a test, just a quick way to invoke and display the dialog for interactive testing")]
		public void ShowDialogForInteractiveTesting()
		{
			AddProjects addProjects = new AddProjects(null, null, new CCTrayProject[0]);
			addProjects.GetListOfNewProjects(null);
		}

		[Fact]
        
		public void TheServerListBoxIsPopulatedWithAListOfAllServersCurrentlyConfigured()
		{
			CCTrayProject[] projects = {
			                     	new CCTrayProject("tcp://localhost:123/blah", "proj1"),
			                     	new CCTrayProject("tcp://otherserver:456/blah", "proj2"),
			                     };

            AddProjects addProjects = new AddProjects(null, null, projects);
		}

		[Fact]
        
		public void TheServerListBoxIsPopulatedInAlphabeticalOrder()
		{
			CCTrayProject[] projects = {
			                     	new CCTrayProject("tcp://b:123/blah", "proj1"),
			                     	new CCTrayProject("tcp://a:123/blah", "proj2"),
			                     };

            AddProjects addProjects = new AddProjects(null, null, projects);
		}

		[Fact]
        
		public void DuplicateServersAreIgnoredWhenAddingToTheServerList()
		{
			CCTrayProject[] projects = {
			                     	new CCTrayProject("tcp://localhost:123/blah", "proj1"),
			                     	new CCTrayProject("tcp://localhost:123/blah", "proj2"),
			                     };

            AddProjects addProjects = new AddProjects(null, null, projects);
		}

		[Fact]
        
		public void CurrentlyAddedProjectsAreIgnoredWhenServerIsSelected()
		{
			CCTrayProject[] allProjects = {
			                        	new CCTrayProject("tcp://localhost:123/blah", "proj1"),
			                        	new CCTrayProject("tcp://localhost:123/blah", "proj2"),
			                        	new CCTrayProject("tcp://localhost:123/blah", "proj3"),
			                        };

			CCTrayProject[] selectedProjects = {
			                             	new CCTrayProject("tcp://localhost:123/blah", "proj1"),
			                             	new CCTrayProject("tcp://localhost:123/blah", "proj2"),
			                             };

			var mockCruiseManagerFactory = new Mock<ICruiseProjectManagerFactory>();
			mockCruiseManagerFactory.Setup(factory => factory.GetProjectList(allProjects[0].BuildServer, false)).Returns(allProjects).Verifiable();
			AddProjects addProjects = new AddProjects(
                (ICruiseProjectManagerFactory)mockCruiseManagerFactory.Object,
                null, 
                selectedProjects);
		}
	}
}
