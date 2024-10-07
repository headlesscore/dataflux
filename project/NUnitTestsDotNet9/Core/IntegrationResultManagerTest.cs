using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.State;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Core
{
	[TestFixture]
	public class IntegrationResultManagerTest : IntegrationFixture
	{
		private Mock<IStateManager> mockStateManager;
		private Project project;
		private IntegrationResultManager manager;

		[SetUp]
		public void SetUp()
		{
			mockStateManager = new Mock<IStateManager>();

			project = CreateProject();
			manager = new IntegrationResultManager(project);
		}

		[TearDown]
		public void Verify()
		{
			mockStateManager.Verify();
		}

		[Test]
		public void StartNewIntegrationShouldCreateNewIntegrationResultAndProperlyPopulate()
		{
			ExpectToLoadState(IntegrationResultMother.CreateSuccessful("success"));

			IIntegrationResult result = manager.StartNewIntegration(ForceBuildRequest());
			ClassicAssert.AreEqual("project", result.ProjectName);
			ClassicAssert.AreEqual(Platform.IsWindows ? @"c:\temp" : @"/tmp", result.WorkingDirectory);
			ClassicAssert.AreEqual(BuildCondition.ForceBuild, result.BuildCondition);
			ClassicAssert.AreEqual("success", result.Label);
			ClassicAssert.AreEqual(project.ArtifactDirectory, result.ArtifactDirectory);
			ClassicAssert.AreEqual(project.WebURL, result.ProjectUrl);
			ClassicAssert.AreEqual("success", result.LastSuccessfulIntegrationLabel);
            ClassicAssert.AreEqual(Source, result.IntegrationRequest.Source);
		}

		[Test]
		public void LastIntegrationResultShouldBeLoadedOnlyOnceFromStateManager()
		{
			IntegrationResult expected = new IntegrationResult();
			ExpectToLoadState(expected);

			IIntegrationResult actual = manager.LastIntegrationResult;
			ClassicAssert.AreEqual(expected, actual);

			// re-request should not reload integration result
			actual = manager.LastIntegrationResult;
			ClassicAssert.AreEqual(expected, actual);
            ClassicAssert.AreEqual(expected, actual);
        }

		[Test]
		public void SavingCurrentIntegrationShouldSetItToLastIntegrationResult()
		{
			IIntegrationResult lastResult = new IntegrationResult();
			ExpectToLoadState(lastResult);

			IIntegrationResult expected = manager.StartNewIntegration(ModificationExistRequest());
			ClassicAssert.AreEqual(lastResult, manager.LastIntegrationResult);

			mockStateManager.Setup(_manager => _manager.SaveState(expected)).Verifiable();
			manager.FinishIntegration();
			ClassicAssert.AreEqual(expected, manager.LastIntegrationResult);
		}

	    [Test]
	    public void InitialBuildShouldBeForced()
	    {
            mockStateManager.Setup(_manager => _manager.HasPreviousState("project")).Returns(false).Verifiable();

            IIntegrationResult expected = manager.StartNewIntegration(ModificationExistRequest());
	        ClassicAssert.AreEqual(BuildCondition.ForceBuild, expected.BuildCondition);
        }

        [Test]
        public void FailedIntegrationShouldAddModificationUsersToFailedUsers()
        {
            IIntegrationResult lastResult = IntegrationResultMother.CreateFailed();
            lastResult.FailureUsers.Add("user1");
            ExpectToLoadState(lastResult);

            IIntegrationResult newResult = manager.StartNewIntegration(ModificationExistRequest());
            ClassicAssert.AreEqual(1, newResult.FailureUsers.Count, "Mismatched count of inherited FailureUsers");

            Modification modification = new Modification();
            modification.UserName = "user";
            newResult.Modifications = new Modification[] { modification };
            newResult.Status = IntegrationStatus.Failure;
            mockStateManager.Setup(_manager => _manager.SaveState(newResult)).Verifiable();
            manager.FinishIntegration();

            ClassicAssert.AreEqual(2, newResult.FailureUsers.Count, "Mismatched count of resulting FailureUsers");
        }

        [Test]
        public void SuccessfulIntegrationShouldClearFailedUsersOnNextIntegration()
        {
            IIntegrationResult result1 = IntegrationResultMother.CreateFailed();
            result1.FailureUsers.Add("user1");
            ExpectToLoadState(result1);

            IIntegrationResult result2 = manager.StartNewIntegration(ModificationExistRequest());
            ClassicAssert.AreEqual(1, result2.FailureUsers.Count);

            Modification modification = new Modification();
            modification.UserName = "user";
            result2.Modifications = new Modification[] { modification };
            result2.Status = IntegrationStatus.Success;
            mockStateManager.Setup(_manager => _manager.SaveState(result2)).Verifiable();
            manager.FinishIntegration();
            ClassicAssert.AreEqual(1, result2.FailureUsers.Count);

            IIntegrationResult result3 = manager.StartNewIntegration(ModificationExistRequest());
            ClassicAssert.AreEqual(0, result3.FailureUsers.Count);
        }

        [Test]
        public void FailedIntegrationShouldResetFailedTasksOnNextIntegration()
        {
            IIntegrationResult lastResult = IntegrationResultMother.CreateFailed();
            lastResult.FailureTasks.Add("task1");
            ExpectToLoadState(lastResult);

            IIntegrationResult newResult = manager.StartNewIntegration(ModificationExistRequest());
            ClassicAssert.AreEqual(0, newResult.FailureTasks.Count, "Mismatched count of inherited FailureTasks");

            Modification modification = new Modification();
            modification.UserName = "user";
            newResult.Modifications = new Modification[] { modification };
            newResult.Status = IntegrationStatus.Failure;
            newResult.FailureTasks.Add("task2");
            mockStateManager.Setup(_manager => _manager.SaveState(newResult)).Verifiable();
            manager.FinishIntegration();

            ClassicAssert.AreEqual(1, newResult.FailureTasks.Count, "Mismatched count of resulting FailureTasks");
        }

        [Test]
        public void SuccessfulIntegrationShouldResetFailedTasksOnNextIntegration()
        {
            IIntegrationResult result1 = IntegrationResultMother.CreateFailed();
            result1.FailureTasks.Add("task1");
            ExpectToLoadState(result1);

            IIntegrationResult result2 = manager.StartNewIntegration(ModificationExistRequest());
            ClassicAssert.AreEqual(0, result2.FailureTasks.Count);

            Modification modification = new Modification();
            modification.UserName = "user";
            result2.Modifications = new Modification[] { modification };
            result2.Status = IntegrationStatus.Success;
            result2.FailureTasks.Add("task2");
            mockStateManager.Setup(_manager => _manager.SaveState(result2)).Verifiable();
            manager.FinishIntegration();
            ClassicAssert.AreEqual(1, result2.FailureTasks.Count);
            ClassicAssert.AreEqual("task2", result2.FailureTasks[0]);

            IIntegrationResult result3 = manager.StartNewIntegration(ModificationExistRequest());
            ClassicAssert.AreEqual(0, result3.FailureTasks.Count);
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
