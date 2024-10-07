﻿using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.CCTrayLib.Presentation;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Config;
using ThoughtWorks.CruiseControl.Core.Tasks;
using ThoughtWorks.CruiseControl.Remote;
using ThoughtWorks.CruiseControl.Remote.Parameters;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
    [TestFixture]
    public class TaskContainerBaseTests
    {
        #region Private fields
        private MockRepository mocks = new MockRepository(MockBehavior.Default);
        #endregion

        #region Tests
        #region Validate()
        [Test]
        public void ValidateHandlesNull()
        {
            var task = new TestTask
            {
                Tasks = null
            };
            task.Validate(null, null, null);
        }

        [Test]
        public void ValidateHandlesEmpty()
        {
            var task = new TestTask
            {
                Tasks = new ITask[0]
            };
            task.Validate(null, null, null);
        }

        [Test]
        public void ValidateHandlesValidationTasks()
        {
            var subTask = new MockTask();
            var task = new TestTask
            {
                Tasks = new ITask[] 
                {
                    subTask
                }
            };

            task.Validate(null, ConfigurationTrace.Start(null), null);
            ClassicAssert.IsTrue(subTask.IsValided);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void ValidateHandlesNonValidationTasks()
        {
            var subTask = mocks.Create<ITask>(MockBehavior.Strict).Object;
            var task = new TestTask
            {
                Tasks = new ITask[] 
                {
                    subTask
                }
            };

            task.Validate(null, null, null);
            mocks.VerifyAll();
        }
        #endregion

        #region ApplyParameters()
        [Test]
        public void ApplyParametersStoresTheArguments()
        {
            var parameters = new Dictionary<string, string>();
            var definitions = new List<ParameterBase>();
            var subTask = new MockTask();
            var task = new TestTask
            {
                Tasks = new ITask[] 
                {
                    subTask
                }
            };
            var result = mocks.Create<IIntegrationResult>().Object;

            task.ApplyParameters(parameters, definitions);
            task.Run(result);
            mocks.VerifyAll();

            ClassicAssert.AreSame(parameters, subTask.Parameters);
            ClassicAssert.AreSame(definitions, subTask.Definitions);
            ClassicAssert.IsTrue(subTask.Executed);
        }
        #endregion

        #region InitialiseStatus()
        [Test]
        public void InitialiseStatusHandlesNull()
        {
            var task = new TestTask
            {
                Tasks = null
            };
            task.TestStatus();
        }

        [Test]
        public void InitialiseStatusHandlesEmpty()
        {
            var task = new TestTask
            {
                Tasks = new ITask[0]
            };
            task.TestStatus();
            ClassicAssert.IsNotNull(task.CurrentStatus);
        }

        [Test]
        public void InitialiseStatusHandlesStatusTask()
        {
            var subTask = new MockTask();
            var task = new TestTask
            {
                Tasks = new ITask[] 
                {
                    subTask
                }
            };

            task.TestStatus();

            ClassicAssert.IsNotNull(task.CurrentStatus);
            ClassicAssert.IsTrue(subTask.SnapshotGenerated);
        }

        [Test]
        public void InitialiseStatusHandlesNonStatusTask()
        {
            var subTask = mocks.Create<ITask>(MockBehavior.Strict).Object;
            var task = new TestTask
            {
                Tasks = new ITask[] 
                {
                    subTask
                }
            };

            task.TestStatus();
            mocks.VerifyAll();

            ClassicAssert.IsNotNull(task.CurrentStatus);
        }
        #endregion
        #endregion

        #region Private classes
        private class TestTask
            : TaskContainerBase
        {
            protected override bool Execute(IIntegrationResult result)
            {
                int loop = 0;
                foreach (var task in Tasks)
                {
                    var taskResult = result.Clone();
                    RunTask(task, taskResult, new RunningSubTaskDetails(loop, result));
                    result.Merge(taskResult);

                    loop++;
                }
                return true;
            }

            public void TestStatus()
            {
                InitialiseStatus();
            }

            protected override string GetStatusInformation(RunningSubTaskDetails Details)
            {
                string Value = !string.IsNullOrEmpty(Description)
                                ? Description
                                : string.Format("Running test sub tasks ({0} task(s))", Tasks.Length);

                if (Details != null)
                    Value += string.Format(": [{0}] {1}",
                                            Details.Index,
                                            !string.IsNullOrEmpty(Details.Information)
                                             ? Details.Information
                                             : "No information");

                return Value;
            }
        }

        private class MockTask
            : ITask, IConfigurationValidation, IStatusSnapshotGenerator, IParamatisedItem
        {
            public bool Executed { get; set; }
            public bool IsValided { get; set; }
            public bool SnapshotGenerated { get; set; }
            public Dictionary<string, string> Parameters { get; set; }
            public IEnumerable<ParameterBase> Definitions { get; set; }

            public void Run(IIntegrationResult result)
            {
                Executed = true;
            }

            public void Validate(IConfiguration configuration, ConfigurationTrace parent, IConfigurationErrorProcesser errorProcesser)
            {
                IsValided = true;
            }

            public ItemStatus GenerateSnapshot()
            {
                SnapshotGenerated = true;
                return new ItemStatus();
            }

            public void ApplyParameters(Dictionary<string, string> parameters, IEnumerable<ParameterBase> parameterDefinitions)
            {
                Parameters = parameters;
                Definitions = parameterDefinitions;
            }
        }
        #endregion
    }
}
