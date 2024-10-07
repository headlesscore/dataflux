using System.IO;
using NUnit.Framework;
using ThoughtWorks.CruiseControl.Core;
using System;
using ThoughtWorks.CruiseControl.Core.Util;
using NUnit.Framework.Legacy;

namespace ThoughtWorks.CruiseControl.UnitTests.Core
{
	[TestFixture]
	public class ProjectBaseTest
	{
		private ProjectBase project;
		private class ConcreteProject : ProjectBase { }
		private IExecutionEnvironment executionEnvironment;

		[SetUp]
		public void Setup()
		{
			executionEnvironment = new ExecutionEnvironment();

			project = new ConcreteProject();
		}

		[Test]
		public void ShouldReturnConfiguredWorkingDirectoryIfOneIsSet()
		{
			string workingDir = Path.GetFullPath(Path.Combine(".", "workingdir"));

			// Setup
			project.ConfiguredWorkingDirectory = workingDir;

			// Execute & Verify
			ClassicAssert.AreEqual(workingDir, project.WorkingDirectory);
            ClassicAssert.AreEqual(workingDir, project.WorkingDirectory);
        }

		[Test]
		public void ShouldReturnCalculatedWorkingDirectoryIfOneIsNotSet()
		{
			// Setup
			project.Name = "myProject";

			// Execute & Verify
			ClassicAssert.AreEqual(
				Path.Combine(executionEnvironment.GetDefaultProgramDataFolder(ApplicationType.Server),
                    Path.Combine("myProject", "WorkingDirectory")), 
                project.WorkingDirectory);
		}

		[Test]
		public void ShouldReturnConfiguredArtifactDirectoryIfOneIsSet()
		{
			string artifactDir = Path.GetFullPath(Path.Combine(".", "artifacts"));

			// Setup
			project.ConfiguredArtifactDirectory = artifactDir;

			// Execute & Verify
			ClassicAssert.AreEqual(artifactDir, project.ArtifactDirectory);
		}

		[Test]
		public void ShouldReturnCalculatedArtifactDirectoryIfOneIsNotSet()
		{
			// Setup
			project.Name = "myProject";

			// Execute & Verify
			ClassicAssert.AreEqual(
				Path.Combine(executionEnvironment.GetDefaultProgramDataFolder(ApplicationType.Server), 
                    Path.Combine("myProject", "Artifacts")), 
                project.ArtifactDirectory);
		}




	}
}
