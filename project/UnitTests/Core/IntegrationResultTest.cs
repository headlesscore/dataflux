using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;
using ThoughtWorks.CruiseControl.Remote;
using ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core
{
    [TestFixture]
    public class IntegrationResultTest : CustomAssertion
    {
        private IntegrationResult result;

        [SetUp]
        protected void CreateIntegrationResult()
        {
            result = new IntegrationResult();
        }

        [Test]
        public void LastModificationDate()
        {
            Modification earlierModification = new Modification();
            earlierModification.ModifiedTime = new DateTime(0);

            Modification laterModification = new Modification();
            laterModification.ModifiedTime = new DateTime(1);

            result.Modifications = new Modification[] { earlierModification, laterModification };
            ClassicAssert.AreEqual(laterModification.ModifiedTime, result.LastModificationDate);
        }

        [Test]
        public void LastModificationDateWhenThereAreNoModifications()
        {
            // Project relies on this behavior, but is it really what we want?
            DateTime yesterday = DateTime.Now.AddDays(-1).Date;
            ClassicAssert.AreEqual(yesterday, result.LastModificationDate.Date);
        }

        [Test]
        public void VerifyInitialIntegrationResult()
        {
            string workingDir = Path.GetFullPath(Path.Combine(".", "workingdir"));
            string artifactDir = Path.GetFullPath(Path.Combine(".", "artifacts"));

            IntegrationResult initial = IntegrationResult.CreateInitialIntegrationResult("project", workingDir, artifactDir);

            ClassicAssert.AreEqual("project", initial.ProjectName);
            ClassicAssert.AreEqual(IntegrationStatus.Unknown, initial.LastIntegrationStatus, "last integration status is unknown because no previous integrations exist.");
            ClassicAssert.AreEqual(IntegrationStatus.Unknown, initial.Status, "status should be unknown as integration has not run yet.");
            ClassicAssert.AreEqual(DateTime.Now.AddDays(-1).Day, initial.StartTime.Day, "assume start date is yesterday in order to detect some modifications.");
            ClassicAssert.AreEqual(DateTime.Now.Day, initial.EndTime.Day, "assume end date is today in order to detect some modifications.");
            ClassicAssert.AreEqual(workingDir, initial.WorkingDirectory);
            ClassicAssert.AreEqual(artifactDir, initial.ArtifactDirectory);
            ClassicAssert.AreEqual(IntegrationResult.InitialLabel, initial.Label);
            ClassicAssert.AreEqual(IntegrationResult.InitialLabel, initial.Label);

            ClassicAssert.IsTrue(initial.IsInitial());
        }

        [Test]
        public void ShouldReturnNullAsLastChangeNumberIfNoModifications()
        {
            ClassicAssert.AreEqual(null, result.LastChangeNumber);
        }

        [Test]
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
            ClassicAssert.AreEqual("10", result.LastChangeNumber);
            result.Modifications = new Modification[] { mod1, mod2 };
            ClassicAssert.AreEqual("20", result.LastChangeNumber);
            result.Modifications = new Modification[] { mod2, mod1 };
            ClassicAssert.AreEqual("20", result.LastChangeNumber);
        }

        [Test]
        public void ShouldNotRunBuildIfThereAreNoModifications()
        {
            result.Modifications = new Modification[0];
            ClassicAssert.IsFalse(result.ShouldRunBuild());
        }

        [Test]
        public void ShouldRunBuildIfThereAreModifications()
        {
            Modification modification = ModificationMother.CreateModification("foo", DateTime.Now.AddSeconds(-2));
            result.Modifications = new Modification[] { modification };
            ClassicAssert.IsTrue(result.ShouldRunBuild());
        }

        [Test]
        public void ShouldRunBuildIfInForcedCondition()
        {
            result.BuildCondition = BuildCondition.ForceBuild;
            ClassicAssert.IsTrue(result.ShouldRunBuild());
        }

        [Test]
        public void TaskOutputShouldAggregateOutputOfTaskResults()
        {
            result.AddTaskResult("<foo/>");
            result.AddTaskResult("<bar/>");
            ClassicAssert.AreEqual("<foo/><bar/>", result.TaskOutput);
        }

        [Test]
        public void ShouldBaseRelativePathFromArtifactsDirectory()
        {
            string artifactDir = Path.GetFullPath(Path.Combine(".", "artifacts"));

            result.ArtifactDirectory = artifactDir;
            ClassicAssert.AreEqual(Path.Combine(artifactDir, "hello.bat"), result.BaseFromArtifactsDirectory("hello.bat"));
        }

        [Test]
        public void ShouldNotReBaseRelativeToArtifactsDirectoryForAbsolutePath()
        {
            string artifactDir = Path.GetFullPath(Path.Combine(".", "artifacts"));

            result.ArtifactDirectory = artifactDir;
            ClassicAssert.AreEqual(Path.Combine(artifactDir, "hello.bat"), result.BaseFromArtifactsDirectory(Path.Combine(artifactDir, "hello.bat")));
        }

        [Test]
        public void ShouldBaseRelativePathFromWorkingDirectory()
        {
            string workingDir = Path.GetFullPath(Path.Combine(".", "workingdir"));

            result.WorkingDirectory = workingDir;
            ClassicAssert.AreEqual(Path.Combine(workingDir, "hello.bat"), result.BaseFromWorkingDirectory("hello.bat"));
        }

        [Test]
        public void ShouldNotReBaseRelativeToWorkingDirectoryForAbsolutePath()
        {
            string workingDir = Path.GetFullPath(Path.Combine(".", "workingdir"));

            result.WorkingDirectory = workingDir;
            ClassicAssert.AreEqual(Path.Combine(workingDir, "hello.bat"), result.BaseFromWorkingDirectory(Path.Combine(workingDir, "hello.bat")));
        }

        [Test]
        public void ShouldSucceedIfContainsOnlySuccessfulTaskResults()
        {
            result.AddTaskResult(new ProcessTaskResult(ProcessResultFixture.CreateSuccessfulResult()));
            ClassicAssert.IsTrue(result.Succeeded);
        }

        [Test]
        public void ShouldHaveFailedIfContainsFailedTaskResults()
        {
            result.AddTaskResult(new ProcessTaskResult(ProcessResultFixture.CreateNonZeroExitCodeResult()));
            result.AddTaskResult(new ProcessTaskResult(ProcessResultFixture.CreateSuccessfulResult()));
            ClassicAssert.IsTrue(result.Failed);
        }

        [Test]
        public void ShouldHaveExceptionStatusIfExceptionHasBeenThrown()
        {
            result.ExceptionResult = new Exception("build blew up");
            result.AddTaskResult(new ProcessTaskResult(ProcessResultFixture.CreateSuccessfulResult()));
            ClassicAssert.AreEqual(IntegrationStatus.Exception, result.Status);
        }

        [Test]
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

            ClassicAssert.AreEqual(18, result.IntegrationProperties.Count);
            ClassicAssert.AreEqual("project", result.IntegrationProperties[IntegrationPropertyNames.CCNetProject]);
            ClassicAssert.AreEqual("http://localhost/ccnet2", result.IntegrationProperties[IntegrationPropertyNames.CCNetProjectUrl]);
            ClassicAssert.AreEqual("label23", result.IntegrationProperties[IntegrationPropertyNames.CCNetLabel]);
            ClassicAssert.AreEqual(23, result.IntegrationProperties[IntegrationPropertyNames.CCNetNumericLabel]);
            ClassicAssert.AreEqual(artifactDir, result.IntegrationProperties[IntegrationPropertyNames.CCNetArtifactDirectory]);
            ClassicAssert.AreEqual(workingDir, result.IntegrationProperties[IntegrationPropertyNames.CCNetWorkingDirectory]);
            // We purposefully use culture-independent string formats
            ClassicAssert.AreEqual("2005-06-06", result.IntegrationProperties[IntegrationPropertyNames.CCNetBuildDate]);
            ClassicAssert.AreEqual(IntegrationResultMother.DefaultBuildId, result.IntegrationProperties[IntegrationPropertyNames.CCNetBuildId]);
            ClassicAssert.AreEqual("08:45:00", result.IntegrationProperties[IntegrationPropertyNames.CCNetBuildTime]);
            ClassicAssert.AreEqual(BuildCondition.IfModificationExists, result.IntegrationProperties[IntegrationPropertyNames.CCNetBuildCondition]);
            ClassicAssert.AreEqual(IntegrationStatus.Unknown, result.IntegrationProperties[IntegrationPropertyNames.CCNetIntegrationStatus]);
            ClassicAssert.AreEqual(IntegrationStatus.Unknown, result.IntegrationProperties[IntegrationPropertyNames.CCNetLastIntegrationStatus]);
            ClassicAssert.AreEqual("myTrigger", result.IntegrationProperties[IntegrationPropertyNames.CCNetRequestSource]);
            ClassicAssert.AreEqual("John Doe", result.IntegrationProperties[IntegrationPropertyNames.CCNetUser]);
            ClassicAssert.AreEqual(Path.Combine(artifactDir, "project_ListenFile.xml"), result.IntegrationProperties[IntegrationPropertyNames.CCNetListenerFile]);
            ArrayList failureUsers = result.IntegrationProperties[IntegrationPropertyNames.CCNetFailureUsers] as ArrayList;
            ClassicAssert.IsNotNull(failureUsers);
            ClassicAssert.AreEqual(1, failureUsers.Count);
            ClassicAssert.AreEqual("user", failureUsers[0]);
            ArrayList failureTasks = result.IntegrationProperties[IntegrationPropertyNames.CCNetFailureTasks] as ArrayList;
            ClassicAssert.IsNotNull(failureTasks);
            ClassicAssert.AreEqual(1, failureTasks.Count);
            ClassicAssert.AreEqual("task", failureTasks[0]);
            ArrayList Modifiers = result.IntegrationProperties[IntegrationPropertyNames.CCNetModifyingUsers] as ArrayList;
            ClassicAssert.IsNotNull(Modifiers);
            ClassicAssert.AreEqual(1, Modifiers.Count);
            ClassicAssert.AreEqual("John", Modifiers[0]);
        }


        [Test]
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


            ClassicAssert.AreEqual(result.IntegrationProperties.Count, TheClone.IntegrationProperties.Count);
            ClassicAssert.AreEqual(result.IntegrationProperties[IntegrationPropertyNames.CCNetProject], TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetProject]);
            ClassicAssert.AreEqual(result.IntegrationProperties[IntegrationPropertyNames.CCNetProjectUrl], TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetProjectUrl]);
            ClassicAssert.AreEqual(result.IntegrationProperties[IntegrationPropertyNames.CCNetLabel], TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetLabel]);
            ClassicAssert.AreEqual(result.IntegrationProperties[IntegrationPropertyNames.CCNetNumericLabel], TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetNumericLabel]);
            ClassicAssert.AreEqual(result.IntegrationProperties[IntegrationPropertyNames.CCNetArtifactDirectory], TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetArtifactDirectory]);
            ClassicAssert.AreEqual(result.IntegrationProperties[IntegrationPropertyNames.CCNetWorkingDirectory], TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetWorkingDirectory]);
            ClassicAssert.AreEqual(result.IntegrationProperties[IntegrationPropertyNames.CCNetBuildCondition], TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetBuildCondition]);
            ClassicAssert.AreEqual(result.IntegrationProperties[IntegrationPropertyNames.CCNetIntegrationStatus], TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetIntegrationStatus]);
            ClassicAssert.AreEqual(result.IntegrationProperties[IntegrationPropertyNames.CCNetLastIntegrationStatus], TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetLastIntegrationStatus]);
            ClassicAssert.AreEqual(result.IntegrationProperties[IntegrationPropertyNames.CCNetRequestSource], TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetRequestSource]);
            ClassicAssert.AreEqual(result.IntegrationProperties[IntegrationPropertyNames.CCNetUser], TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetUser]);
            ClassicAssert.AreEqual(result.IntegrationProperties[IntegrationPropertyNames.CCNetListenerFile], TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetListenerFile]);
            
            ArrayList failureUsers = TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetFailureUsers] as ArrayList;
            ClassicAssert.IsNotNull(failureUsers);
            ArrayList failureTasks = TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetFailureTasks] as ArrayList;
            ClassicAssert.IsNotNull(failureTasks);
            ArrayList Modifiers = TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetModifyingUsers] as ArrayList;
            ClassicAssert.IsNotNull(Modifiers);
            ClassicAssert.AreEqual(1, Modifiers.Count);
            ClassicAssert.AreEqual("John", Modifiers[0]);
            
            ClassicAssert.AreEqual(result.Status, TheClone.Status);
            
            //below are the ones that are not cloned, should these be cloned also, see bug 240
            //http://www.cruisecontrolnet.org/issues/240

            // We purposefully use culture-independent string formats
            //ClassicAssert.AreEqual("2005-06-06", TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetBuildDate]);
            //ClassicAssert.AreEqual(IntegrationResultMother.DefaultBuildId, TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetBuildId]);
            //ClassicAssert.AreEqual("08:45:00", TheClone.IntegrationProperties[IntegrationPropertyNames.CCNetBuildTime]);
            //ClassicAssert.AreEqual(1, failureUsers.Count);
            //ClassicAssert.AreEqual("user", failureUsers[0]);
            //ClassicAssert.AreEqual(1, failureTasks.Count);
            //ClassicAssert.AreEqual("task", failureTasks[0]);

        
        }



        [Test]
        public void VerifyIntegrationArtifactDir()
        {
            string artifactDir = Path.GetFullPath(Path.Combine(".", "artifacts"));

            result = new IntegrationResult();
            result.ArtifactDirectory = artifactDir;
            result.Label = "1.2.3.4";
            ClassicAssert.AreEqual(Path.Combine(artifactDir, "1.2.3.4"), result.IntegrationArtifactDirectory);
        }

        [Test]
        public void NumericLabel()
        {
            result = new IntegrationResult();
            result.Label = "23";
            ClassicAssert.AreEqual(23, result.NumericLabel);
        }

        [Test]
        public void NumericLabelWithPrefix()
        {
            result = new IntegrationResult();
            result.Label = "Prefix23";
            ClassicAssert.AreEqual(23, result.NumericLabel);
        }

        [Test]
        public void NumericLabelWithNumericPrefix()
        {
            result = new IntegrationResult();
            result.Label = "R3SX23";
            ClassicAssert.AreEqual(23, result.NumericLabel);
        }

        [Test]
        public void NumericLabelTextOnly()
        {
            result = new IntegrationResult();
            result.Label = "foo";
            // Make sure we don't throw an exception
            ClassicAssert.AreEqual(0, result.NumericLabel);
        }

        [Test]
        public void CanGetPreviousState()
        {
            string workingDir = Path.GetFullPath(Path.Combine(".", "workingdir"));
            string artifactDir = Path.GetFullPath(Path.Combine(".", "artifacts"));

            IntegrationSummary expectedSummary = new IntegrationSummary(IntegrationStatus.Exception, "foo", "foo", DateTime.MinValue);
            result = new IntegrationResult("project", workingDir, artifactDir, IntegrationRequest.NullRequest, expectedSummary);
            ClassicAssert.AreEqual(new IntegrationSummary(IntegrationStatus.Exception, "foo", "foo", DateTime.MinValue), result.LastIntegration);
        }

        [Test]
        public void ShouldReturnPreviousLabelAsLastSuccessfulIntegrationLabelIfFailed()
        {
            result.AddTaskResult(new ProcessTaskResult(ProcessResultFixture.CreateNonZeroExitCodeResult()));
            result.LastSuccessfulIntegrationLabel = "1";
            result.Label = "2";
            ClassicAssert.AreEqual("1", result.LastSuccessfulIntegrationLabel);
        }

        [Test]
        public void ShouldReturnCurrentLabelAsLastSuccessfulIntegrationLabelIfSuccessful()
        {
            result.AddTaskResult(new ProcessTaskResult(ProcessResultFixture.CreateSuccessfulResult()));
            result.LastSuccessfulIntegrationLabel = "1";
            result.Label = "2";
            ClassicAssert.AreEqual("2", result.LastSuccessfulIntegrationLabel);
        }
    }
}
