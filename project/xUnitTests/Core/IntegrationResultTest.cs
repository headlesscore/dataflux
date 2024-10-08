using System;
using System.Collections;
using System.IO;
using Xunit;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;
using ThoughtWorks.CruiseControl.Remote;
using ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core
{
    
    public class IntegrationResultTest : CustomAssertion
    {
        private IntegrationResult result;

        // [SetUp]
        protected void CreateIntegrationResult()
        {
            result = new IntegrationResult();
        }

        [Fact]
        public void LastModificationDate()
        {
            Modification earlierModification = new Modification();
            earlierModification.ModifiedTime = new DateTime(0);

            Modification laterModification = new Modification();
            laterModification.ModifiedTime = new DateTime(1);

            result.Modifications = new Modification[] { earlierModification, laterModification };
            Assert.Equal(laterModification.ModifiedTime, result.LastModificationDate);
        }

        [Fact]
        public void LastModificationDateWhenThereAreNoModifications()
        {
            // Project relies on this behavior, but is it really what we want?
            DateTime yesterday = DateTime.Now.AddDays(-1).Date;
            Assert.Equal(yesterday, result.LastModificationDate.Date);
        }

        [Fact]
        public void VerifyInitialIntegrationResult()
        {
            string workingDir = Path.GetFullPath(Path.Combine(".", "workingdir"));
            string artifactDir = Path.GetFullPath(Path.Combine(".", "artifacts"));

            IntegrationResult initial = IntegrationResult.CreateInitialIntegrationResult("project", workingDir, artifactDir);

            Assert.Equal("project", initial.ProjectName);
            Assert.Equal(IntegrationStatus.Unknown, initial.LastIntegrationStatus);
            Assert.Equal(IntegrationStatus.Unknown, initial.Status);
            Assert.Equal(DateTime.Now.AddDays(-1).Day, initial.StartTime.Day);
            Assert.Equal(DateTime.Now.Day, initial.EndTime.Day);
            Assert.Equal(workingDir, initial.WorkingDirectory);
            Assert.Equal(artifactDir, initial.ArtifactDirectory);
            Assert.Equal(IntegrationResult.InitialLabel, initial.Label);
            Assert.Equal(IntegrationResult.InitialLabel, initial.Label);

            Assert.True(initial.IsInitial());
        }

        [Fact]
        public void ShouldReturnNullAsLastChangeNumberIfNoModifications()
        {
            Assert.Equal(null, result.LastChangeNumber);
        }

        [Fact]
        public void ShouldReturnTheMaximumChangeNumberFromAllModificationsForLastChangeNumber()
        {
            Modification mod1 = new Modification
            {
                ChangeNumber = "10",
                ModifiedTime = new DateTime(2009, 1, 2)
            };

            Modification mod2 = new Modification
            {
                ChangeNumber = "20",
                ModifiedTime = new DateTime(2009, 1, 3)
            };

            result.Modifications = new Modification[] { mod1 };
            Assert.Equal("10", result.LastChangeNumber);
            result.Modifications = new Modification[] { mod1, mod2 };
            Assert.Equal("20", result.LastChangeNumber);
            result.Modifications = new Modification[] { mod2, mod1 };
            Assert.Equal("20", result.LastChangeNumber);
        }

        [Fact]
        public void ShouldNotRunBuildIfThereAreNoModifications()
        {
            result.Modifications = new Modification[0];
            Assert.False(result.ShouldRunBuild());
        }

        [Fact]
        public void ShouldRunBuildIfThereAreModifications()
        {
            Modification modification = ModificationMother.CreateModification("foo", DateTime.Now.AddSeconds(-2));
            result.Modifications = new Modification[] { modification };
            Assert.True(result.ShouldRunBuild());
        }

        [Fact]
        public void ShouldRunBuildIfInForcedCondition()
        {
            result.BuildCondition = BuildCondition.ForceBuild;
            Assert.True(result.ShouldRunBuild());
        }

        [Fact]
        public void TaskOutputShouldAggregateOutputOfTaskResults()
        {
            result.AddTaskResult("<foo/>");
            result.AddTaskResult("<bar/>");
            Assert.Equal("<foo/><bar/>", result.TaskOutput);
        }

        [Fact]
        public void ShouldBaseRelativePathFromArtifactsDirectory()
        {
            string artifactDir = Path.GetFullPath(Path.Combine(".", "artifacts"));

            result.ArtifactDirectory = artifactDir;
            Assert.Equal(Path.Combine(artifactDir, "hello.bat"), result.BaseFromArtifactsDirectory("hello.bat"));
        }

        [Fact]
        public void ShouldNotReBaseRelativeToArtifactsDirectoryForAbsolutePath()
        {
            string artifactDir = Path.GetFullPath(Path.Combine(".", "artifacts"));

            result.ArtifactDirectory = artifactDir;
            Assert.Equal(Path.Combine(artifactDir, "hello.bat"), result.BaseFromArtifactsDirectory(Path.Combine(artifactDir, "hello.bat")));
        }

        [Fact]
        public void ShouldBaseRelativePathFromWorkingDirectory()
        {
            string workingDir = Path.GetFullPath(Path.Combine(".", "workingdir"));

            result.WorkingDirectory = workingDir;
            Assert.Equal(Path.Combine(workingDir, "hello.bat"), result.BaseFromWorkingDirectory("hello.bat"));
        }

        [Fact]
        public void ShouldNotReBaseRelativeToWorkingDirectoryForAbsolutePath()
        {
            string workingDir = Path.GetFullPath(Path.Combine(".", "workingdir"));

            result.WorkingDirectory = workingDir;
            Assert.Equal(Path.Combine(workingDir, "hello.bat"), result.BaseFromWorkingDirectory(Path.Combine(workingDir, "hello.bat")));
        }

        [Fact]
        public void ShouldSucceedIfContainsOnlySuccessfulTaskResults()
        {
            result.AddTaskResult(new ProcessTaskResult(ProcessResultFixture.CreateSuccessfulResult()));
            Assert.True(result.Succeeded);
        }

        [Fact]
        public void ShouldHaveFailedIfContainsFailedTaskResults()
        {
            result.AddTaskResult(new ProcessTaskResult(ProcessResultFixture.CreateNonZeroExitCodeResult()));
            result.AddTaskResult(new ProcessTaskResult(ProcessResultFixture.CreateSuccessfulResult()));
            Assert.True(result.Failed);
        }

        [Fact]
        public void ShouldHaveExceptionStatusIfExceptionHasBeenThrown()
        {
            result.ExceptionResult = new Exception("build blew up");
            result.AddTaskResult(new ProcessTaskResult(ProcessResultFixture.CreateSuccessfulResult()));
            Assert.Equal(IntegrationStatus.Exception, result.Status);
        }

        [Fact]
        public void MapIntegrationProperties()
        {
            string workingDir = Path.GetFullPath(Path.Combine(".", "workingdir"));
            string artifactDir = Path.GetFullPath(Path.Combine(".", "artifacts"));

            result = new IntegrationResult("project", workingDir, artifactDir,
                                           new IntegrationRequest(BuildCondition.IfModificationExists, "myTrigger", "John Doe"),
                                           new IntegrationSummary(IntegrationStatus.Unknown, "label23", "label22",
                                                                  new DateTime(2005, 06, 06, 08, 45, 00)));
            result.StartTime = new DateTime(2005, 06, 06, 08, 45, 00);
            result.ProjectUrl = "http://localhost/ccnet2";
            result.BuildId = new Guid(IntegrationResultMother.DefaultBuildId);
            result.FailureUsers.Add("user");
            result.FailureTasks.Add("task" );

            Modification mods = new Modification();
            mods.UserName = "John";

            result.Modifications = new Modification[] { mods };

            Assert.Equal(18, result.IntegrationProperties.Count);
            Assert.Equal("project", result.IntegrationProperties[IntegrationPropertyNames.CCNetProject]);
            Assert.Equal("http://localhost/ccnet2", result.IntegrationProperties[IntegrationPropertyNames.CCNetProjectUrl]);
            Assert.Equal("label23", result.IntegrationProperties[IntegrationPropertyNames.CCNetLabel]);
            Assert.Equal(23, result.IntegrationProperties[IntegrationPropertyNames.CCNetNumericLabel]);
            Assert.Equal(artifactDir, result.IntegrationProperties[IntegrationPropertyNames.CCNetArtifactDirectory]);
            Assert.Equal(workingDir, result.IntegrationProperties[IntegrationPropertyNames.CCNetWorkingDirectory]);
            // We purposefully use culture-independent string formats
            Assert.Equal("2005-06-06", result.IntegrationProperties[IntegrationPropertyNames.CCNetBuildDate]);
            Assert.Equal(IntegrationResultMother.DefaultBuildId, result.IntegrationProperties[IntegrationPropertyNames.CCNetBuildId]);
            Assert.Equal("08:45:00", result.IntegrationProperties[IntegrationPropertyNames.CCNetBuildTime]);
            Assert.Equal(BuildCondition.IfModificationExists, result.IntegrationProperties[IntegrationPropertyNames.CCNetBuildCondition]);
            Assert.Equal(IntegrationStatus.Unknown, result.IntegrationProperties[IntegrationPropertyNames.CCNetIntegrationStatus]);
            Assert.Equal(IntegrationStatus.Unknown, result.IntegrationProperties[IntegrationPropertyNames.CCNetLastIntegrationStatus]);
            Assert.Equal("myTrigger", result.IntegrationProperties[IntegrationPropertyNames.CCNetRequestSource]);
            Assert.Equal("John Doe", result.IntegrationProperties[IntegrationPropertyNames.CCNetUser]);
            Assert.Equal(Path.Combine(artifactDir, "project_ListenFile.xml"), result.IntegrationProperties[IntegrationPropertyNames.CCNetListenerFile]);
            ArrayList failureUsers = result.IntegrationProperties[IntegrationPropertyNames.CCNetFailureUsers] as ArrayList;
            Assert.NotNull(failureUsers);
            Assert.Equal(1, failureUsers.Count);
            Assert.Equal("user", failureUsers[0]);
            ArrayList failureTasks = result.IntegrationProperties[IntegrationPropertyNames.CCNetFailureTasks] as ArrayList;
            Assert.NotNull(failureTasks);
            Assert.Equal(1, failureTasks.Count);
            Assert.Equal("task", failureTasks[0]);
            ArrayList Modifiers = result.IntegrationProperties[IntegrationPropertyNames.CCNetModifyingUsers] as ArrayList;
            Assert.NotNull(Modifiers);
            Assert.Equal(1, Modifiers.Count);
            Assert.Equal("John", Modifiers[0]);
        }


        [Fact]
        public void CloneShouldWork()
        {
            string workingDir = Path.GetFullPath(Path.Combine(".", "workingdir"));
            string artifactDir = Path.GetFullPath(Path.Combine(".", "artifacts"));

            result = new IntegrationResult("project", workingDir, artifactDir,
                                           new IntegrationRequest(BuildCondition.IfModificationExists, "myTrigger", "John Doe"),
                                           new IntegrationSummary(IntegrationStatus.Failure, "label23", "label22",
                                                                  new DateTime(2005, 06, 06, 08, 45, 00)));
            result.StartTime = new DateTime(2005, 06, 06, 08, 45, 00);
            result.ProjectUrl = "http://localhost/ccnet2";
            result.BuildId = new Guid(IntegrationResultMother.DefaultBuildId);

            result.AddTaskResult(new ProcessTaskResult(ProcessResultFixture.CreateNonZeroExitCodeResult()));
            result.AddTaskResult(new ProcessTaskResult(ProcessResultFixture.CreateSuccessfulResult()));

            Modification mods = new Modification();
            mods.UserName = "John";
            result.Modifications = new Modification[] { mods };


            var TheClone = result.Clone();


            Assert.Equal(result.IntegrationProperties.Count, TheClone.IntegrationProperties.Count);
            Assert.Equal(result.IntegrationProperties[IntegrationPropertyNames.CCNetProject], TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetProject]);
            Assert.Equal(result.IntegrationProperties[IntegrationPropertyNames.CCNetProjectUrl], TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetProjectUrl]);
            Assert.Equal(result.IntegrationProperties[IntegrationPropertyNames.CCNetLabel], TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetLabel]);
            Assert.Equal(result.IntegrationProperties[IntegrationPropertyNames.CCNetNumericLabel], TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetNumericLabel]);
            Assert.Equal(result.IntegrationProperties[IntegrationPropertyNames.CCNetArtifactDirectory], TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetArtifactDirectory]);
            Assert.Equal(result.IntegrationProperties[IntegrationPropertyNames.CCNetWorkingDirectory], TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetWorkingDirectory]);
            Assert.Equal(result.IntegrationProperties[IntegrationPropertyNames.CCNetBuildCondition], TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetBuildCondition]);
            Assert.Equal(result.IntegrationProperties[IntegrationPropertyNames.CCNetIntegrationStatus], TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetIntegrationStatus]);
            Assert.Equal(result.IntegrationProperties[IntegrationPropertyNames.CCNetLastIntegrationStatus], TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetLastIntegrationStatus]);
            Assert.Equal(result.IntegrationProperties[IntegrationPropertyNames.CCNetRequestSource], TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetRequestSource]);
            Assert.Equal(result.IntegrationProperties[IntegrationPropertyNames.CCNetUser], TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetUser]);
            Assert.Equal(result.IntegrationProperties[IntegrationPropertyNames.CCNetListenerFile], TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetListenerFile]);
            
            ArrayList failureUsers = TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetFailureUsers] as ArrayList;
            Assert.NotNull(failureUsers);
            ArrayList failureTasks = TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetFailureTasks] as ArrayList;
            Assert.NotNull(failureTasks);
            ArrayList Modifiers = TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetModifyingUsers] as ArrayList;
            Assert.NotNull(Modifiers);
            Assert.Equal(1, Modifiers.Count);
            Assert.Equal("John", Modifiers[0]);
            
            Assert.Equal(result.Status, TheClone.Status);
            
            //below are the ones that are not cloned, should these be cloned also, see bug 240
            //http://www.cruisecontrolnet.org/issues/240

            // We purposefully use culture-independent string formats
            //Assert.Equal("2005-06-06", TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetBuildDate]);
            //Assert.Equal(IntegrationResultMother.DefaultBuildId, TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetBuildId]);
            //Assert.Equal("08:45:00", TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetBuildTime]);
            //Assert.Equal(1, failureUsers.Count);
            //Assert.Equal("user", failureUsers[0]);
            //Assert.Equal(1, failureTasks.Count);
            //Assert.Equal("task", failureTasks[0]);

        
        }



        [Fact]
        public void VerifyIntegrationArtifactDir()
        {
            string artifactDir = Path.GetFullPath(Path.Combine(".", "artifacts"));

            result = new IntegrationResult();
            result.ArtifactDirectory = artifactDir;
            result.Label = "1.2.3.4";
            Assert.Equal(Path.Combine(artifactDir, "1.2.3.4"), result.IntegrationArtifactDirectory);
        }

        [Fact]
        public void NumericLabel()
        {
            result = new IntegrationResult();
            result.Label = "23";
            Assert.Equal(23, result.NumericLabel);
        }

        [Fact]
        public void NumericLabelWithPrefix()
        {
            result = new IntegrationResult();
            result.Label = "Prefix23";
            Assert.Equal(23, result.NumericLabel);
        }

        [Fact]
        public void NumericLabelWithNumericPrefix()
        {
            result = new IntegrationResult();
            result.Label = "R3SX23";
            Assert.Equal(23, result.NumericLabel);
        }

        [Fact]
        public void NumericLabelTextOnly()
        {
            result = new IntegrationResult();
            result.Label = "foo";
            // Make sure we don't throw an exception
            Assert.Equal(0, result.NumericLabel);
        }

        [Fact]
        public void CanGetPreviousState()
        {
            string workingDir = Path.GetFullPath(Path.Combine(".", "workingdir"));
            string artifactDir = Path.GetFullPath(Path.Combine(".", "artifacts"));

            IntegrationSummary expectedSummary = new IntegrationSummary(IntegrationStatus.Exception, "foo", "foo", DateTime.MinValue);
            result = new IntegrationResult("project", workingDir, artifactDir, IntegrationRequest.NullRequest, expectedSummary);
            Assert.Equal(new IntegrationSummary(IntegrationStatus.Exception, "foo", "foo", DateTime.MinValue), result.LastIntegration);
        }

        [Fact]
        public void ShouldReturnPreviousLabelAsLastSuccessfulIntegrationLabelIfFailed()
        {
            result.AddTaskResult(new ProcessTaskResult(ProcessResultFixture.CreateNonZeroExitCodeResult()));
            result.LastSuccessfulIntegrationLabel = "1";
            result.Label = "2";
            Assert.Equal("1", result.LastSuccessfulIntegrationLabel);
        }

        [Fact]
        public void ShouldReturnCurrentLabelAsLastSuccessfulIntegrationLabelIfSuccessful()
        {
            result.AddTaskResult(new ProcessTaskResult(ProcessResultFixture.CreateSuccessfulResult()));
            result.LastSuccessfulIntegrationLabel = "1";
            result.Label = "2";
            Assert.Equal("2", result.LastSuccessfulIntegrationLabel);
        }
    }
}
