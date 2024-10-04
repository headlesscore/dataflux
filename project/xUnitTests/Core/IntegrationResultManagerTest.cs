using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.State;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Core
{
	
	public class IntegrationResultManagerTest : IntegrationFixture
	{
		private Mock<IStateManager> mockStateManager;
		private Project project;
		private IntegrationResultManager manager;

		// [SetUp]
		public void SetUp()
		{
			mockStateManager = new Mock<IStateManager>();

			project = CreateProject();
			manager = new IntegrationResultManager(project);
		}

		// [TearDown]
		public void Verify()
		{
			mockStateManager.Verify();
		}

		[Fact]
		public void StartNewIntegrationShouldCreateNewIntegrationResultAndProperlyPopulate()
		{
			ExpectToLoadState(IntegrationResultMother.CreateSuccessful("success"));

			IIntegrationResult result = manager.StartNewIntegration(ForceBuildRequest());
			Assert.Equal("project", result.ProjectName);
			Assert.Equal(Platform.IsWindows ? @"c:\temp" : @"/tmp", result.WorkingDirectory);
			Assert.Equal(BuildCondition.ForceBuild, result.BuildCondition);
			Assert.Equal("success", result.Label);
			Assert.Equal(project.ArtifactDirectory, result.ArtifactDirectory);
			Assert.Equal(project.WebURL, result.ProjectUrl);
			Assert.Equal("success", result.LastSuccessfulIntegrationLabel);
            Assert.Equal(Source, result.IntegrationRequest.Source);
		}

		[Fact]
		public void LastIntegrationResultShouldBeLoadedOnlyOnceFromStateManager()
		{
			IntegrationResult expected = new IntegrationResult();
			ExpectToLoadState(expected);

			IIntegrationResult actual = manager.LastIntegrationResult;
			Assert.Equal(expected, actual);

			// re-request should not reload integration result
			actual = manager.LastIntegrationResult;
			Assert.Equal(expected, actual);
            Assert.Equal(expected, actual);
        }

		[Fact]
		public void SavingCurrentIntegrationShouldSetItToLastIntegrationResult()
		{
			IIntegrationResult lastResult = new IntegrationResult();
			ExpectToLoadState(lastResult);

			IIntegrationResult expected = manager.StartNewIntegration(ModificationExistRequest());
			Assert.Equal(lastResult, manager.LastIntegrationResult);

			mockStateManager.Setup(_manager => _manager.SaveState(expected)).Verifiable();
			manager.FinishIntegration();
			Assert.Equal(expected, manager.LastIntegrationResult);
		}

	    [Fact]
	    public void InitialBuildShouldBeForced()
	    {
            mockStateManager.Setup(_manager => _manager.HasPreviousState("project")).Returns(false).Verifiable();

            IIntegrationResult expected = manager.StartNewIntegration(ModificationExistRequest());
	        Assert.Equal(BuildCondition.ForceBuild, expected.BuildCondition);
        }

        [Fact]
        public void FailedIntegrationShouldAddModificationUsersToFailedUsers()
        {
            IIntegrationResult lastResult = IntegrationResultMother.CreateFailed();
            lastResult.FailureUsers.Add("user1");
            ExpectToLoadState(lastResult);

            IIntegrationResult newResult = manager.StartNewIntegration(ModificationExistRequest());
            Assert.Equal(1, newResult.FailureUsers.Count, "Mismatched count of inherited FailureUsers");

            Modification modification = new Modification();
            modification.UserName = "user";
            newResult.Modifications = new Modification[] { modification };
            newResult.Status = IntegrationStatus.Failure;
            mockStateManager.Setup(_manager => _manager.SaveState(newResult)).Verifiable();
            manager.FinishIntegration();

            Assert.Equal(2, newResult.FailureUsers.Count, "Mismatched count of resulting FailureUsers");
        }

        [Fact]
        public void SuccessfulIntegrationShouldClearFailedUsersOnNextIntegration()
        {
            IIntegrationResult result1 = IntegrationResultMother.CreateFailed();
            result1.FailureUsers.Add("user1");
            ExpectToLoadState(result1);

            IIntegrationResult result2 = manager.StartNewIntegration(ModificationExistRequest());
            Assert.Equal(1, result2.FailureUsers.Count);

            Modification modification = new Modification();
            modification.UserName = "user";
            result2.Modifications = new Modification[] { modification };
            result2.Status = IntegrationStatus.Success;
            mockStateManager.Setup(_manager => _manager.SaveState(result2)).Verifiable();
            manager.FinishIntegration();
            Assert.Equal(1, result2.FailureUsers.Count);

            IIntegrationResult result3 = manager.StartNewIntegration(ModificationExistRequest());
            Assert.Equal(0, result3.FailureUsers.Count);
        }

        [Fact]
        public void FailedIntegrationShouldResetFailedTasksOnNextIntegration()
        {
            IIntegrationResult lastResult = IntegrationResultMother.CreateFailed();
            lastResult.FailureTasks.Add("task1");
            ExpectToLoadState(lastResult);

            IIntegrationResult newResult = manager.StartNewIntegration(ModificationExistRequest());
            Assert.Equal(0, newResult.FailureTasks.Count, "Mismatched count of inherited FailureTasks");

            Modification modification = new Modification();
            modification.UserName = "user";
            newResult.Modifications = new Modification[] { modification };
            newResult.Status = IntegrationStatus.Failure;
            newResult.FailureTasks.Add("task2");
            mockStateManager.Setup(_manager => _manager.SaveState(newResult)).Verifiable();
            manager.FinishIntegration();

            Assert.Equal(1, newResult.FailureTasks.Count, "Mismatched count of resulting FailureTasks");
        }

        [Fact]
        public void SuccessfulIntegrationShouldResetFailedTasksOnNextIntegration()
        {
            IIntegrationResult result1 = IntegrationResultMother.CreateFailed();
            result1.FailureTasks.Add("task1");
            ExpectToLoadState(result1);

            IIntegrationResult result2 = manager.StartNewIntegration(ModificationExistRequest());
            Assert.Equal(0, result2.FailureTasks.Count);

            Modification modification = new Modification();
            modification.UserName = "user";
            result2.Modifications = new Modification[] { modification };
            result2.Status = IntegrationStatus.Success;
            result2.FailureTasks.Add("task2");
            mockStateManager.Setup(_manager => _manager.SaveState(result2)).Verifiable();
            manager.FinishIntegration();
            Assert.Equal(1, result2.FailureTasks.Count);
            Assert.Equal("task2", result2.FailureTasks[0]);

            IIntegrationResult result3 = manager.StartNewIntegration(ModificationExistRequest());
            Assert.Equal(0, result3.FailureTasks.Count);
        }

        private void ExpectToLoadState(IIntegrationResult result)
		{
			mockStateManager.Setup(_manager => _manager.HasPreviousState("project")).Returns(true).Verifiable();
			mockStateManager.Setup(_manager => _manager.LoadState("project")).Returns(result).Verifiable();
		}

		private Project CreateProject()
		{	
			project = new Project();
			project.Name = "project";
			project.ConfiguredWorkingDirectory = Platform.IsWindows ? @"c:\temp" : @"/tmp";
			project.StateManager = (IStateManager) mockStateManager.Object;
			project.ConfiguredArtifactDirectory = project.ConfiguredWorkingDirectory;            
			return project;
		}
	}
}
