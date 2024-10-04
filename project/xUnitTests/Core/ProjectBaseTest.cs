using System.IO;
using Xunit;
using ThoughtWorks.CruiseControl.Core;
using System;
using ThoughtWorks.CruiseControl.Core.Util;


namespace ThoughtWorks.CruiseControl.UnitTests.Core
{
	
	public class ProjectBaseTest
	{
		private ProjectBase project;
		private class ConcreteProject : ProjectBase { }
		private IExecutionEnvironment executionEnvironment;

		// [SetUp]
		public void Setup()
		{
			executionEnvironment = new ExecutionEnvironment();

			project = new ConcreteProject();
		}

		[Fact]
		public void ShouldReturnConfiguredWorkingDirectoryIfOneIsSet()
		{
			string workingDir = Path.GetFullPath(Path.Combine(".", "workingdir"));

			// Setup
			project.ConfiguredWorkingDirectory = workingDir;

			// Execute & Verify
			Assert.Equal(workingDir, project.WorkingDirectory);
            Assert.Equal(workingDir, project.WorkingDirectory);
        }

		[Fact]
		public void ShouldReturnCalculatedWorkingDirectoryIfOneIsNotSet()
		{
			// Setup
			project.Name = "myProject";

			// Execute & Verify
			Assert.Equal(
				Path.Combine(executionEnvironment.GetDefaultProgramDataFolder(ApplicationType.Server),
                    Path.Combine("myProject", "WorkingDirectory")), 
                project.WorkingDirectory);
		}

		[Fact]
		public void ShouldReturnConfiguredArtifactDirectoryIfOneIsSet()
		{
			string artifactDir = Path.GetFullPath(Path.Combine(".", "artifacts"));

			// Setup
			project.ConfiguredArtifactDirectory = artifactDir;

			// Execute & Verify
			Assert.Equal(artifactDir, project.ArtifactDirectory);
		}

		[Fact]
		public void ShouldReturnCalculatedArtifactDirectoryIfOneIsNotSet()
		{
			// Setup
			project.Name = "myProject";

			// Execute & Verify
			Assert.Equal(
				Path.Combine(executionEnvironment.GetDefaultProgramDataFolder(ApplicationType.Server), 
                    Path.Combine("myProject", "Artifacts")), 
                project.ArtifactDirectory);
		}




	}
}
