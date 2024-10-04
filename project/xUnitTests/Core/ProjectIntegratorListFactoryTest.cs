using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Queues;

namespace ThoughtWorks.CruiseControl.UnitTests.Core
{
	
	public class ProjectIntegratorListFactoryTest
	{
		[Fact]
		public void CreatesProjectIntegrators()
		{
			// Setup
			Project project1 = new Project();
			project1.Name = "Project 1";
			Project project2 = new Project();
			project2.Name = "Project 2";
			ProjectList projectList = new ProjectList();
			projectList.Add(project1);
			projectList.Add(project2);

			// Execute
			IntegrationQueueSet integrationQueues = new IntegrationQueueSet();
			IProjectIntegratorList integrators = new ProjectIntegratorListFactory().CreateProjectIntegrators(projectList, integrationQueues);

			// Verify
			Assert.Equal(2, integrators.Count);
            Assert.Equal(2, integrators.Count);
            Assert.Equal(project1, integrators["Project 1"].Project );
			Assert.Equal(project2, integrators["Project 2"].Project );
		}
	}
}
