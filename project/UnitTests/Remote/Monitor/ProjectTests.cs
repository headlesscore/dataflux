﻿using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using NUnit.Framework;
using ThoughtWorks.CruiseControl.Remote.Monitor;
using ThoughtWorks.CruiseControl.Remote;
using NUnit.Framework.Legacy;

namespace ThoughtWorks.CruiseControl.UnitTests.Remote.Monitor
{
    [TestFixture]
    public class ProjectTests
    {
        #region Private fields
        private MockRepository mocks;
        #endregion

        #region Setup
        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository(MockBehavior.Default);
        }
        #endregion

        #region Tests
        #region Constructor tests
        [Test]
        public void ConstructorDoesNotAllowNullClient()
        {
            try
            {
                var project = new Project(null, null, null);
                ClassicAssert.Fail("ArgumentNullException was expected");
                ClassicAssert.IsTrue(true);
                ClassicAssert.IsTrue(true);
            }
            catch (ArgumentNullException) { }
        }

        [Test]
        public void ConstructorDoesNotAllowNullServer()
        {
            var client = mocks.Create<CruiseServerClientBase>().Object;
            try
            {
                var project = new Project(client, null, null);
                ClassicAssert.Fail("ArgumentNullException was expected");
            }
            catch (ArgumentNullException) { }
        }

        [Test]
        public void ConstructorDoesNotAllowNullStatus()
        {
            var client = mocks.Create<CruiseServerClientBase>().Object;
            var server = InitialiseServer();
            try
            {
                var project = new Project(client, server, null);
                ClassicAssert.Fail("ArgumentNullException was expected");
            }
            catch (ArgumentNullException) { }
        }
        #endregion

        #region Server tests
        [Test]
        public void ServerReturnsUnderlyingServer()
        {
            var client = mocks.Create<CruiseServerClientBase>().Object;
            var server = InitialiseServer();
            var status = new ProjectStatus();
            var project = new Project(client, server, status);
            ClassicAssert.AreSame(server, project.Server);
        }
        #endregion

        #region Name tests
        [Test]
        public void NameReturnsNameFromStatus()
        {
            var client = mocks.Create<CruiseServerClientBase>().Object;
            var server = InitialiseServer();
            var status = new ProjectStatus { Name = "Test Project" };
            var project = new Project(client, server, status);
            ClassicAssert.AreEqual(status.Name, project.Name);
        }
        #endregion

        #region BuildStage tests
        [Test]
        public void BuildStageReturnsBuildStageFromStatus()
        {
            var client = mocks.Create<CruiseServerClientBase>().Object;
            var server = InitialiseServer();
            var status = new ProjectStatus { BuildStage = "Old" };
            var project = new Project(client, server, status);
            ClassicAssert.AreEqual(status.BuildStage, project.BuildStage);
        }
        #endregion

        #region BuildStatus tests
        [Test]
        public void BuildStatusReturnsBuildStatusFromStatus()
        {
            var client = mocks.Create<CruiseServerClientBase>().Object;
            var server = InitialiseServer();
            var status = new ProjectStatus { BuildStatus = IntegrationStatus.Exception };
            var project = new Project(client, server, status);
            ClassicAssert.AreEqual(status.BuildStatus, project.BuildStatus);
        }
        #endregion

        #region Status tests
        [Test]
        public void StatusReturnsStatusFromStatus()
        {
            var client = mocks.Create<CruiseServerClientBase>().Object;
            var server = InitialiseServer();
            var status = new ProjectStatus { Status = ProjectIntegratorState.Stopping };
            var project = new Project(client, server, status);
            ClassicAssert.AreEqual(status.Status, project.Status);
        }
        #endregion

        #region Activity tests
        [Test]
        public void ActivityReturnsActivityFromStatus()
        {
            var client = mocks.Create<CruiseServerClientBase>().Object;
            var server = InitialiseServer();
            var status = new ProjectStatus { Activity = ProjectActivity.CheckingModifications };
            var project = new Project(client, server, status);
            ClassicAssert.AreEqual(status.Activity, project.Activity);
        }
        #endregion

        #region Description tests
        [Test]
        public void DescriptionReturnsDescriptionFromStatus()
        {
            var client = mocks.Create<CruiseServerClientBase>().Object;
            var server = InitialiseServer();
            var status = new ProjectStatus { Description = "Description" };
            var project = new Project(client, server, status);
            ClassicAssert.AreEqual(status.Description, project.Description);
        }
        #endregion

        #region Category tests
        [Test]
        public void CategoryReturnsCategoryFromStatus()
        {
            var client = mocks.Create<CruiseServerClientBase>().Object;
            var server = InitialiseServer();
            var status = new ProjectStatus { Category = "Category" };
            var project = new Project(client, server, status);
            ClassicAssert.AreEqual(status.Category, project.Category);
        }
        #endregion

        #region Queue tests
        [Test]
        public void QueueReturnsQueueFromStatus()
        {
            var client = mocks.Create<CruiseServerClientBase>().Object;
            var server = InitialiseServer();
            var status = new ProjectStatus { Queue = "Queue Name" };
            var project = new Project(client, server, status);
            ClassicAssert.AreEqual(status.Queue, project.Queue);
        }
        #endregion

        #region QueuePriority tests
        [Test]
        public void QueuePriorityReturnsQueuePriorityFromStatus()
        {
            var client = mocks.Create<CruiseServerClientBase>().Object;
            var server = InitialiseServer();
            var status = new ProjectStatus { QueuePriority = 7 };
            var project = new Project(client, server, status);
            ClassicAssert.AreEqual(status.QueuePriority, project.QueuePriority);
        }
        #endregion

        #region WebURL tests
        [Test]
        public void WebURLReturnsWebURLFromStatus()
        {
            var client = mocks.Create<CruiseServerClientBase>().Object;
            var server = InitialiseServer();
            var status = new ProjectStatus { WebURL = "http://somewhere" };
            var project = new Project(client, server, status);
            ClassicAssert.AreEqual(status.WebURL, project.WebURL);
        }
        #endregion

        #region LastBuildDate tests
        [Test]
        public void LastBuildDateReturnsLastBuildDateFromStatus()
        {
            var client = mocks.Create<CruiseServerClientBase>().Object;
            var server = InitialiseServer();
            var status = new ProjectStatus { LastBuildDate = new DateTime(2009, 1, 1) };
            var project = new Project(client, server, status);
            ClassicAssert.AreEqual(status.LastBuildDate, project.LastBuildDate);
        }
        #endregion

        #region LastBuildLabel tests
        [Test]
        public void LastBuildLabelReturnsLastBuildLabelFromStatus()
        {
            var client = mocks.Create<CruiseServerClientBase>().Object;
            var server = InitialiseServer();
            var status = new ProjectStatus { LastBuildLabel = "Last label" };
            var project = new Project(client, server, status);
            ClassicAssert.AreEqual(status.LastBuildLabel, project.LastBuildLabel);
        }
        #endregion

        #region LastSuccessfulBuildLabel tests
        [Test]
        public void LastSuccessfulBuildLabelReturnsLastSuccessfulBuildLabelFromStatus()
        {
            var client = mocks.Create<CruiseServerClientBase>().Object;
            var server = InitialiseServer();
            var status = new ProjectStatus { LastSuccessfulBuildLabel = "Last success label" };
            var project = new Project(client, server, status);
            ClassicAssert.AreEqual(status.LastSuccessfulBuildLabel, project.LastSuccessfulBuildLabel);
        }
        #endregion

        #region NextBuildTime tests
        [Test]
        public void NextBuildTimeReturnsNextBuildTimeFromStatus()
        {
            var client = mocks.Create<CruiseServerClientBase>().Object;
            var server = InitialiseServer();
            var status = new ProjectStatus { NextBuildTime = new DateTime(2009, 1, 2) };
            var project = new Project(client, server, status);
            ClassicAssert.AreEqual(status.NextBuildTime, project.NextBuildTime);
        }
        #endregion

        #region Messages tests
        [Test]
        public void MessagesReturnsMessagesFromStatus()
        {
            var client = mocks.Create<CruiseServerClientBase>().Object;
            var server = InitialiseServer();
            var messages = new Message[] {
                new Message { Text = "Testing"}
            };
            var status = new ProjectStatus { Messages = messages };
            var project = new Project(client, server, status);
            ClassicAssert.AreSame(status.Messages, project.Messages);
        }
        #endregion

        #region Update() tests
        [Test]
        public void UpdateValidatesArguments()
        {
            var client = mocks.Create<CruiseServerClientBase>().Object;
            var server = InitialiseServer();
            var status = new ProjectStatus { BuildStatus = IntegrationStatus.Exception };
            var project = new Project(client, server, status);
            try
            {
                project.Update(null);
                ClassicAssert.Fail("ArgumentNullException was expected");
            }
            catch (ArgumentNullException) { }
        }

        [Test]
        public void UpdateChangesUnderlyingStatus()
        {
            var client = mocks.Create<CruiseServerClientBase>().Object;
            var server = InitialiseServer();
            var status = new ProjectStatus { BuildStatus = IntegrationStatus.Exception };
            var project = new Project(client, server, status);

            var newStatus = new ProjectStatus { BuildStatus = IntegrationStatus.Failure };
            project.Update(newStatus);
            ClassicAssert.AreEqual(newStatus.BuildStatus, project.BuildStatus);
        }

        [Test]
        public void UpdateFiresPropertyChangedWhenPropertyHasChanged()
        {
            RunPropertyChangedTest("BuildStage", "Stage 1", "Stage 2");
            RunPropertyChangedTest("BuildStatus", IntegrationStatus.Exception, IntegrationStatus.Failure);
            RunPropertyChangedTest("Status", ProjectIntegratorState.Running, ProjectIntegratorState.Stopped);
            RunPropertyChangedTest("Activity", ProjectActivity.Building, ProjectActivity.Pending);
            RunPropertyChangedTest("Description", "Old", "New");
            RunPropertyChangedTest("Category", "Old", "New");
            RunPropertyChangedTest("Queue", "Old", "New");
            RunPropertyChangedTest("QueuePriority", 1, 2);
            RunPropertyChangedTest("WebURL", "Old", "New");
            RunPropertyChangedTest("LastBuildDate", new DateTime(2009, 1, 1), new DateTime(2009, 1, 2));
            RunPropertyChangedTest("LastBuildLabel", "Old", "New");
            RunPropertyChangedTest("LastSuccessfulBuildLabel", "Old", "New");
            RunPropertyChangedTest("NextBuildTime", new DateTime(2009, 1, 1), new DateTime(2009, 1, 2));
        }

        [Test]
        public void UpdateFiresPropertyChangedWhenMessageIsAdded()
        {
            mocks = new MockRepository(MockBehavior.Default);
            var client = mocks.Create<CruiseServerClientBase>().Object;
            var server = InitialiseServer();
            var status = new ProjectStatus
                {
                    Messages = new Message[] {
                    new Message{Text = "Message 1"}
                }
            };
            var project = new Project(client, server, status);
            var eventFired = false;

            var newStatus = new ProjectStatus
            {
                Messages = new Message[] {
                    new Message{Text = "Message 1"},
                    new Message{Text = "Message 2"}
                }
            };
            project.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == "Messages") eventFired = true;
            };
            project.Update(newStatus);
            ClassicAssert.IsTrue(eventFired, "PropertyChanged for Messages change not fired");
        }

        [Test]
        public void UpdateFiresPropertyChangedWhenMessageIsRemoved()
        {
            mocks = new MockRepository(MockBehavior.Default);
            var client = mocks.Create<CruiseServerClientBase>().Object;
            var server = InitialiseServer();
            var status = new ProjectStatus
            {
                Messages = new Message[] {
                    new Message{Text = "Message 1"},
                    new Message{Text = "Message 2"}
                }
            };
            var project = new Project(client, server, status);
            var eventFired = false;

            var newStatus = new ProjectStatus
            {
                Messages = new Message[] {
                    new Message{Text = "Message 2"}
                }
            };
            project.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == "Messages") eventFired = true;
            };
            project.Update(newStatus);
            ClassicAssert.IsTrue(eventFired, "PropertyChanged for Messages change not fired");
        }

        [Test]
        public void UpdateFiresPropertyChangedWhenMessageIsChanged()
        {
            mocks = new MockRepository(MockBehavior.Default);
            var client = mocks.Create<CruiseServerClientBase>().Object;
            var server = InitialiseServer();
            var status = new ProjectStatus
            {
                Messages = new Message[] {
                    new Message{Text = "Message 1"}
                }
            };
            var project = new Project(client, server, status);
            var eventFired = false;

            var newStatus = new ProjectStatus
            {
                Messages = new Message[] {
                    new Message{Text = "Message 2"}
                }
            };
            project.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == "Messages") eventFired = true;
            };
            project.Update(newStatus);
            ClassicAssert.IsTrue(eventFired, "PropertyChanged for Messages change not fired");
        }
        #endregion

        #region ForceBuild() tests
        [Test]
        public void ForceBuildSendsRequestToClient()
        {
            var client = mocks.Create<CruiseServerClientBase>().Object;
            var server = InitialiseServer();
            var status = new ProjectStatus { Name = "Test Project" };
            var project = new Project(client, server, status);
            Mock.Get(client).Setup(_client => _client.ForceBuild("Test Project")).Verifiable();
            project.ForceBuild();
            mocks.VerifyAll();
        }

        [Test]
        public void ForceBuildWithParametersSendsRequestToClient()
        {
            var client = mocks.Create<CruiseServerClientBase>().Object;
            var server = InitialiseServer();
            var status = new ProjectStatus { Name = "Test Project" };
            var project = new Project(client, server, status);
            var parameters = new List<NameValuePair>();
            Mock.Get(client).Setup(_client => _client.ForceBuild("Test Project", parameters)).Verifiable();
            project.ForceBuild(parameters);
            mocks.VerifyAll();
        }
        #endregion

        #region AbortBuild() tests
        [Test]
        public void AbortBuildSendsRequestToClient()
        {
            var client = mocks.Create<CruiseServerClientBase>().Object;
            var server = InitialiseServer();
            var status = new ProjectStatus { Name = "Test Project" };
            var project = new Project(client, server, status);
            Mock.Get(client).Setup(_client => _client.AbortBuild("Test Project")).Verifiable();
            project.AbortBuild();
            mocks.VerifyAll();
        }
        #endregion

        #region Start() tests
        [Test]
        public void StartSendsRequestToClient()
        {
            var client = mocks.Create<CruiseServerClientBase>().Object;
            var server = InitialiseServer();
            var status = new ProjectStatus { Name = "Test Project" };
            var project = new Project(client, server, status);
            Mock.Get(client).Setup(_client => _client.StartProject("Test Project")).Verifiable();
            project.Start();
            mocks.VerifyAll();
        }
        #endregion

        #region Stop() tests
        [Test]
        public void StopSendsRequestToClient()
        {
            var client = mocks.Create<CruiseServerClientBase>().Object;
            var server = InitialiseServer();
            var status = new ProjectStatus { Name = "Test Project" };
            var project = new Project(client, server, status);
            Mock.Get(client).Setup(_client => _client.StopProject("Test Project")).Verifiable();
            project.Stop();
            mocks.VerifyAll();
        }
        #endregion
        #endregion

        #region Helper methods
        private void RunPropertyChangedTest(string property, object originalValue, object newValue)
        {
            mocks = new MockRepository(MockBehavior.Default);
            var client = mocks.Create<CruiseServerClientBase>().Object;
            var server = InitialiseServer();
            var statusType = typeof(ProjectStatus);
            var propertyMember = statusType.GetProperty(property);

            var status = new ProjectStatus();
            propertyMember.SetValue(status, originalValue, new object[0]);
            var project = new Project(client, server, status);
            var eventFired = false;

            var newStatus = new ProjectStatus();
            propertyMember.SetValue(newStatus, newValue, new object[0]);
            project.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == property) eventFired = true;
            };
            project.Update(newStatus);
            ClassicAssert.IsTrue(eventFired, "PropertyChanged for " + property + " change not fired");
        }

        private Server InitialiseServer()
        {
            var watcher = mocks.Create<IServerWatcher>().Object;
            var client = new CruiseServerClientMock();
            var monitor = new Server(client, watcher);
            return monitor;
        }
        #endregion
    }
}
