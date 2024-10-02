using System;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using CCNet = ThoughtWorks.CruiseControl;

namespace ThoughtWorks.CruiseControl.UnitTests.IntegrationTests
{
    /// <summary>
    /// These are integration tests, so we use the real classes of CCNet as much as possible
    /// Mocks should not be used as we want to test the real classes.
    /// (mocks like an e-mail server are ok)
    /// </summary>
    [TestFixture]
    [Category("Integration")]
    public class SimpleScenarioTests
    {
        System.Collections.Generic.Dictionary<string, bool> IntegrationCompleted = new System.Collections.Generic.Dictionary<string, bool>();

        [OneTimeSetUp]
        public void fixLog4Net()
        {
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("test.config"));
        }

        [Test]
        public void ForceBuildOf1ProjectAndCheckBasicPropertiesOfProjectStatus()
        {
            const string ProjectName1 = "test01";
            const string ProjectName2 = "test02";

            string IntegrationFolder = System.IO.Path.Combine("scenarioTests", ProjectName1);
            string CCNetConfigFile = System.IO.Path.Combine("IntegrationScenarios", "Simple.xml");
            string ProjectStateFile = new System.IO.FileInfo(ProjectName1 + ".state").FullName;

            IntegrationCompleted.Add(ProjectName1, false);
            IntegrationCompleted.Add(ProjectName2, false);

            Log("Clear existing state file, to simulate first run : " + ProjectStateFile);
            System.IO.File.Delete(ProjectStateFile);

            Log("Clear integration folder to simulate first run");
            if (System.IO.Directory.Exists(IntegrationFolder)) System.IO.Directory.Delete(IntegrationFolder, true);


            CCNet.Remote.Messages.ProjectStatusResponse psr;
            CCNet.Remote.Messages.ProjectRequest pr1 = new CCNet.Remote.Messages.ProjectRequest(null, ProjectName1);
            CCNet.Remote.Messages.ProjectRequest pr2 = new CCNet.Remote.Messages.ProjectRequest(null, ProjectName2);


            Log("Making CruiseServerFactory");
            CCNet.Core.CruiseServerFactory csf = new CCNet.Core.CruiseServerFactory();

            Log("Making cruiseServer with config from :" + CCNetConfigFile);
            using (var cruiseServer = csf.Create(true, CCNetConfigFile))
            {

                // subscribe to integration complete to be able to wait for completion of a build
                cruiseServer.IntegrationCompleted += new EventHandler<ThoughtWorks.CruiseControl.Remote.Events.IntegrationCompletedEventArgs>(cruiseServer_IntegrationCompleted);

                Log("Starting cruiseServer");
                cruiseServer.Start();

                System.Threading.Thread.Sleep(250); // give time to start

                Log("Forcing build");
                CheckResponse(cruiseServer.ForceBuild(pr1));

                System.Threading.Thread.Sleep(250); // give time to start the build

                Log("Waiting for integration to complete");
                while (!IntegrationCompleted[ProjectName1])
                {
                    for (int i = 1; i <= 4; i++) System.Threading.Thread.Sleep(250);
                    Log(" waiting ...");
                }

                // un-subscribe to integration complete 
                cruiseServer.IntegrationCompleted -= new EventHandler<ThoughtWorks.CruiseControl.Remote.Events.IntegrationCompletedEventArgs>(cruiseServer_IntegrationCompleted);

                Log("getting project status");
                psr = cruiseServer.GetProjectStatus(pr1);
                CheckResponse(psr);

                Log("Stopping cruiseServer");
                cruiseServer.Stop();

                Log("waiting for cruiseServer to stop");
                cruiseServer.WaitForExit(pr1);
                Log("cruiseServer stopped");

            }

            Log("Checking the data");
            ClassicAssert.AreEqual(2, psr.Projects.Count, "Amount of projects in configfile is not correct." + CCNetConfigFile);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
            CCNet.Remote.ProjectStatus ps = null;

            Log("checking data of project " + ProjectName1);
            foreach (var p in psr.Projects)
            {
                if (p.Name == ProjectName1) ps = p;
            }

            ClassicAssert.AreEqual(ProjectName1, ps.Name);
            ClassicAssert.AreEqual(CCNet.Remote.IntegrationStatus.Success, ps.BuildStatus);
            ClassicAssert.IsTrue(ps.Activity.IsSleeping(), "Activity should be sleeping after an integration");
            ClassicAssert.AreEqual(ps.Category, "cat1");
            ClassicAssert.AreEqual(string.Empty, ps.CurrentMessage, "message should be empty after an ok build");
            ClassicAssert.AreEqual("first testing project", ps.Description);
            ClassicAssert.AreEqual("1", ps.LastBuildLabel);
            ClassicAssert.AreEqual("1", ps.LastSuccessfulBuildLabel);
            ClassicAssert.AreEqual(0, ps.Messages.Length);
            ClassicAssert.AreEqual("Q1", ps.Queue);
            ClassicAssert.AreEqual(1, ps.QueuePriority);
            ClassicAssert.AreEqual(System.Environment.MachineName, ps.ServerName);
            ClassicAssert.AreEqual(CCNet.Remote.ProjectIntegratorState.Running, ps.Status);
            ClassicAssert.AreEqual("http://confluence.public.thoughtworks.org", ps.WebURL);


            Log("checking data of project " + ProjectName2);
            foreach (var p in psr.Projects)
            {
                if (p.Name == ProjectName2) ps = p;
            }

            ClassicAssert.IsFalse(IntegrationCompleted[ProjectName2], "integration not done, event may not be fired");
            ClassicAssert.AreEqual(ProjectName2, ps.Name);
            ClassicAssert.AreEqual(CCNet.Remote.IntegrationStatus.Unknown, ps.BuildStatus);
            ClassicAssert.IsTrue(ps.Activity.IsSleeping(), "Activity should be still sleeping");
            ClassicAssert.AreEqual(ps.Category, "cat2");
            ClassicAssert.AreEqual(string.Empty, ps.CurrentMessage, "message should still be empty");
            ClassicAssert.AreEqual("second testing project", ps.Description);
            ClassicAssert.AreEqual("UNKNOWN", ps.LastBuildLabel);
            ClassicAssert.AreEqual("UNKNOWN", ps.LastSuccessfulBuildLabel);
            ClassicAssert.AreEqual(0, ps.Messages.Length);
            ClassicAssert.AreEqual("Q1", ps.Queue);
            ClassicAssert.AreEqual(2, ps.QueuePriority);
            ClassicAssert.AreEqual(System.Environment.MachineName, ps.ServerName);
            ClassicAssert.AreEqual(CCNet.Remote.ProjectIntegratorState.Unknown, ps.Status);
            ClassicAssert.AreEqual("http://" + System.Environment.MachineName + "/ccnet", ps.WebURL, "Default url not correct");



        }


        [Test]
        public void ForceBuildOfGoodProjectWithDefaultLabelerWithInitialBuildLabelMustHaveInitialBuildLabelAsLastBuildLabel()
        {
            const string ProjectName1 = "LabelTest";

            string IntegrationFolder = System.IO.Path.Combine("scenarioTests", ProjectName1);
            string WorkingFolder = System.IO.Path.Combine(IntegrationFolder, "wf");
            string CCNetConfigFile = System.IO.Path.Combine("IntegrationScenarios", "Simple02.xml");
            string ProjectStateFile = new System.IO.FileInfo(ProjectName1 + ".state").FullName;

            string CheckFileOfConditionalTask = System.IO.Path.Combine(WorkingFolder, "checkFile.txt");


            IntegrationCompleted.Add(ProjectName1, false);

            Log("Clear existing state file, to simulate first run : " + ProjectStateFile);
            System.IO.File.Delete(ProjectStateFile);

            Log("Clear integration folder to simulate first run");
            if (System.IO.Directory.Exists(IntegrationFolder)) System.IO.Directory.Delete(IntegrationFolder, true);


            CCNet.Remote.Messages.ProjectStatusResponse psr;
            CCNet.Remote.Messages.ProjectRequest pr1 = new CCNet.Remote.Messages.ProjectRequest(null, ProjectName1);


            Log("Making CruiseServerFactory");
            CCNet.Core.CruiseServerFactory csf = new CCNet.Core.CruiseServerFactory();

            Log("Making cruiseServer with config from :" + CCNetConfigFile);
            using (var cruiseServer = csf.Create(true, CCNetConfigFile))
            {
                // subscribe to integration complete to be able to wait for completion of a build
                cruiseServer.IntegrationCompleted += new EventHandler<ThoughtWorks.CruiseControl.Remote.Events.IntegrationCompletedEventArgs>(cruiseServer_IntegrationCompleted);

                Log("Starting cruiseServer");
                cruiseServer.Start();

                System.IO.File.WriteAllText(CheckFileOfConditionalTask, "hello");

                System.Threading.Thread.Sleep(250); // give time to start


                Log("Forcing build - conditional ok");
                CheckResponse(cruiseServer.ForceBuild(pr1));


                Log("Waiting for integration to complete");
                while (!IntegrationCompleted[ProjectName1])
                {
                    for (int i = 1; i <= 4; i++) System.Threading.Thread.Sleep(250);
                    Log(" waiting ...");
                }

                // un-subscribe to integration complete 
                cruiseServer.IntegrationCompleted -= new EventHandler<ThoughtWorks.CruiseControl.Remote.Events.IntegrationCompletedEventArgs>(cruiseServer_IntegrationCompleted);

                Log("getting project status");
                psr = cruiseServer.GetProjectStatus(pr1);
                CheckResponse(psr);

                Log("Stopping cruiseServer");
                cruiseServer.Stop();

                Log("waiting for cruiseServer to stop");
                cruiseServer.WaitForExit(pr1);
                Log("cruiseServer stopped");

            }

            Log("Checking the data");
            ClassicAssert.AreEqual(1, psr.Projects.Count, "Amount of projects in configfile is not correct." + CCNetConfigFile);

            CCNet.Remote.ProjectStatus ps = null;

            Log("checking data of project " + ProjectName1);
            foreach (var p in psr.Projects)
            {
                if (p.Name == ProjectName1) ps = p;
            }

            // 1 good build 

            ClassicAssert.AreEqual(ProjectName1, ps.Name);
            ClassicAssert.AreEqual(CCNet.Remote.IntegrationStatus.Success, ps.BuildStatus);
            ClassicAssert.AreEqual(string.Empty, ps.CurrentMessage, "message should be empty after ok build");
            ClassicAssert.AreEqual("1.5.1603", ps.LastBuildLabel, "after ok build with initial label, the result must be the inital label");
            ClassicAssert.AreEqual("1.5.1603", ps.LastSuccessfulBuildLabel, "after ok build with initial label, the result must be the inital label");
            ClassicAssert.AreEqual(0, ps.Messages.Length);

        }

        [Test]
        [Ignore("Fails because of very old bug")]
        public void ForceBuildOfBadProjectAfterGoodWithDefaultLabelerWithInitialBuildLabelMustHaveInitialBuildLabelAsLastBuildLabel()
        {
            // in the labeller is a comparison with previous integration result, and an ITaskResult that always returns true for CheckIfSuccess 
            // this looks weird and should be investigated
            // old part of the codebase since 2009

            
            const string ProjectName1 = "LabelTest";
            string IntegrationFolder = System.IO.Path.Combine("scenarioTests", ProjectName1);
            string WorkingFolder = System.IO.Path.Combine(IntegrationFolder, "wf");
            string CCNetConfigFile = System.IO.Path.Combine("IntegrationScenarios", "Simple02.xml");
            string ProjectStateFile = new System.IO.FileInfo(ProjectName1 + ".state").FullName;

            string CheckFileOfConditionalTask = System.IO.Path.Combine(WorkingFolder, "checkFile.txt");


            IntegrationCompleted.Add(ProjectName1, false);

            Log("Clear existing state file, to simulate first run : " + ProjectStateFile);
            System.IO.File.Delete(ProjectStateFile);

            Log("Clear integration folder to simulate first run");
            if (System.IO.Directory.Exists(IntegrationFolder)) System.IO.Directory.Delete(IntegrationFolder, true);


            CCNet.Remote.Messages.ProjectStatusResponse psr;
            CCNet.Remote.Messages.ProjectRequest pr1 = new CCNet.Remote.Messages.ProjectRequest(null, ProjectName1);


            Log("Making CruiseServerFactory");
            CCNet.Core.CruiseServerFactory csf = new CCNet.Core.CruiseServerFactory();

            Log("Making cruiseServer with config from :" + CCNetConfigFile);
            using (var cruiseServer = csf.Create(true, CCNetConfigFile))
            {
                // subscribe to integration complete to be able to wait for completion of a build
                cruiseServer.IntegrationCompleted += new EventHandler<ThoughtWorks.CruiseControl.Remote.Events.IntegrationCompletedEventArgs>(cruiseServer_IntegrationCompleted);

                Log("Starting cruiseServer");
                cruiseServer.Start();

                System.IO.File.WriteAllText(CheckFileOfConditionalTask, "hello");

                System.Threading.Thread.Sleep(250); // give time to start


                Log("Forcing build - conditional ok");
                CheckResponse(cruiseServer.ForceBuild(pr1));


                Log("Waiting for integration to complete");
                while (!IntegrationCompleted[ProjectName1])
                {
                    for (int i = 1; i <= 4; i++) System.Threading.Thread.Sleep(250);
                    Log(" waiting ...");
                }

                IntegrationCompleted[ProjectName1] = false;
                System.IO.File.Delete(CheckFileOfConditionalTask);
                System.Threading.Thread.Sleep(250); // give time to finish delete


                Log("Forcing build - conditional Not ok");
                CheckResponse(cruiseServer.ForceBuild(pr1));


                Log("Waiting for integration to complete");
                while (!IntegrationCompleted[ProjectName1])
                {
                    for (int i = 1; i <= 4; i++) System.Threading.Thread.Sleep(250);
                    Log(" waiting ...");
                }


                // un-subscribe to integration complete 
                cruiseServer.IntegrationCompleted -= new EventHandler<ThoughtWorks.CruiseControl.Remote.Events.IntegrationCompletedEventArgs>(cruiseServer_IntegrationCompleted);

                Log("getting project status");
                psr = cruiseServer.GetProjectStatus(pr1);
                CheckResponse(psr);

                Log("Stopping cruiseServer");
                cruiseServer.Stop();

                Log("waiting for cruiseServer to stop");
                cruiseServer.WaitForExit(pr1);
                Log("cruiseServer stopped");

            }

            Log("Checking the data");
            ClassicAssert.AreEqual(1, psr.Projects.Count, "Amount of projects in configfile is not correct." + CCNetConfigFile);

            CCNet.Remote.ProjectStatus ps = null;

            Log("checking data of project " + ProjectName1);
            foreach (var p in psr.Projects)
            {
                if (p.Name == ProjectName1) ps = p;
            }

            // 1 good build and 1 bad

            ClassicAssert.AreEqual(ProjectName1, ps.Name);
            ClassicAssert.AreEqual(CCNet.Remote.IntegrationStatus.Failure, ps.BuildStatus);
            ClassicAssert.AreEqual("bad task", ps.CurrentMessage, "message should be the descripton / name of the failing task");
            ClassicAssert.AreEqual("1.5.1603", ps.LastBuildLabel, "do not increase label, because the increase on failure is set to false");
            ClassicAssert.AreEqual("1.5.1603", ps.LastSuccessfulBuildLabel, "do not increase label, because the increase on failure is set to false");

        }

        void cruiseServer_IntegrationCompleted(object sender, CCNet.Remote.Events.IntegrationCompletedEventArgs e)
        {
            Log(string.Format(System.Globalization.CultureInfo.CurrentCulture,"Integration complete. Project {0} ", e.ProjectName));
            IntegrationCompleted[e.ProjectName] = true;
        }

        private void Log(string message)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(System.Globalization.CultureInfo.CurrentCulture,"--> {0} {1}", DateTime.Now.ToUniversalTime(), message));
        }

        private void CheckResponse(ThoughtWorks.CruiseControl.Remote.Messages.Response value)
        {
            if (value.Result == ThoughtWorks.CruiseControl.Remote.Messages.ResponseResult.Failure)
            {
                string message = "Request has failed on the server:" + System.Environment.NewLine +
                    value.ConcatenateErrors();
                throw new CCNet.Core.CruiseControlException(message);
            }
        }

    }
}
