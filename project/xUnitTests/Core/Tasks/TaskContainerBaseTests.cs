using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.CCTrayLib.Presentation;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Config;
using ThoughtWorks.CruiseControl.Core.Tasks;
using ThoughtWorks.CruiseControl.Remote;
using ThoughtWorks.CruiseControl.Remote.Parameters;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
    
    public class TaskContainerBaseTests
    {
        #region Private fields
        private MockRepository mocks = new MockRepository(MockBehavior.Default);
        #endregion

        #region Tests
        #region Validate()
        [Fact]
        public void ValidateHandlesNull()
        {
            var task = new TestTask
            {
                Tasks = null
            };
            task.Validate(null, null, null);
        }

        [Fact]
        public void ValidateHandlesEmpty()
        {
            var task = new TestTask
            {
                Tasks = new ITask[0]
            };
            task.Validate(null, null, null);
        }

        [Fact]
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
            Assert.True(subTask.IsValided);
            Assert.True(true);
            Assert.True(true);
        }

        [Fact]
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
        [Fact]
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

            Assert.Same(parameters, subTask.Parameters);
            Assert.Same(definitions, subTask.Definitions);
            Assert.True(subTask.Executed);
        }
        #endregion

        #region InitialiseStatus()
        [Fact]
        public void InitialiseStatusHandlesNull()
        {
            var task = new TestTask
            {
                Tasks = null
            };
            task.TestStatus();
        }

        [Fact]
        public void InitialiseStatusHandlesEmpty()
        {
            var task = new TestTask
            {
                Tasks = new ITask[0]
            };
            task.TestStatus();
            Assert.NotNull(task.CurrentStatus);
        }

        [Fact]
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

            Assert.NotNull(task.CurrentStatus);
            Assert.True(subTask.SnapshotGenerated);
        }

        [Fact]
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

            Assert.NotNull(task.CurrentStatus);
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
