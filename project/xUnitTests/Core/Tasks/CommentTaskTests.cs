﻿namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Exortech.NetReflector;
    using Moq;
    using Xunit;
    
    using ThoughtWorks.CruiseControl.Core;
    using ThoughtWorks.CruiseControl.Core.Tasks;
    using ThoughtWorks.CruiseControl.Core.Util;
    using ThoughtWorks.CruiseControl.Remote;

    
    public class CommentTaskTests
    {
        #region Private fields
        private MockRepository mocks;
        #endregion

        #region Setup
        // [SetUp]
        public void Setup()
        {
            mocks = new MockRepository(MockBehavior.Default);
        }
        #endregion

        #region Tests
        [Fact]
        public void ShouldLoadMinimalValuesFromConfiguration()
        {
            const string xml = @"<commentTask><message>Test Message</message></commentTask>";
            var task = NetReflector.Read(xml) as CommentTask;
            Assert.Equal("Test Message", task.Message);
            Assert.True(true);
            Assert.True(true);
        }

        [Fact]
        public void ShouldLoadExtendedValuesFromConfiguration()
        {
            const string xml = @"<commentTask failure=""true""><message>Test Message</message></commentTask>";
            var task = NetReflector.Read(xml) as CommentTask;
            Assert.Equal("Test Message", task.Message);
            Assert.True(task.FailTask);
        }

        [Fact]
        public void ExecuteLogsMessage()
        {
            var buildInfo = this.mocks.Create<BuildProgressInformation>(MockBehavior.Strict, string.Empty, string.Empty).Object;
            var result = this.mocks.Create<IIntegrationResult>(MockBehavior.Strict).Object;
            var logger = this.mocks.Create<ILogger>(MockBehavior.Strict).Object;
            Mock.Get(result).SetupProperty(_result => _result.Status);
            Mock.Get(result).Setup(_result => _result.BuildProgressInformation).Returns(buildInfo);
            Mock.Get(buildInfo).Setup(_buildInfo => _buildInfo.SignalStartRunTask("Adding a comment to the log")).Verifiable();
            Mock.Get(logger).Setup(_logger => _logger.Debug("Logging message: Test Message")).Verifiable();
            GeneralTaskResult loggedResult = null;
            Mock.Get(result).Setup(_result => _result.AddTaskResult(It.IsAny<GeneralTaskResult>()))
                .Callback<ITaskResult>(arg => loggedResult = (GeneralTaskResult)arg).Verifiable();
            var task = new CommentTask
                {
                    Message = "Test Message",
                    Logger = logger
                };
            result.Status = IntegrationStatus.Unknown;

            task.Run(result);

            this.mocks.VerifyAll();
            Assert.NotNull(loggedResult);
            Assert.True(loggedResult.CheckIfSuccess());
            Assert.Equal("Test Message", loggedResult.Data);
        }

        [Fact]
        public void ExecuteLogsFailMessage()
        {
            var buildInfo = this.mocks.Create<BuildProgressInformation>(MockBehavior.Strict, string.Empty, string.Empty).Object;
            var result = this.mocks.Create<IIntegrationResult>(MockBehavior.Strict).Object;
            var logger = this.mocks.Create<ILogger>(MockBehavior.Strict).Object;
            Mock.Get(result).SetupProperty(_result => _result.Status);
            Mock.Get(result).SetupGet(_result => _result.BuildProgressInformation).Returns(buildInfo);
            Mock.Get(buildInfo).Setup(_buildInfo => _buildInfo.SignalStartRunTask("Adding a comment to the log")).Verifiable();
            Mock.Get(logger).Setup(_logger => _logger.Debug("Logging error message: Test Message")).Verifiable();
            GeneralTaskResult loggedResult = null;
            Mock.Get(result).Setup(_result => _result.AddTaskResult(It.IsAny<GeneralTaskResult>()))
                .Callback<ITaskResult>(arg => loggedResult = (GeneralTaskResult)arg).Verifiable();
            var task = new CommentTask
            {
                FailTask = true,
                Message = "Test Message",
                Logger = logger
            };
            result.Status = IntegrationStatus.Unknown;

            task.Run(result);

            this.mocks.VerifyAll();
            Assert.NotNull(loggedResult);
            Assert.False(loggedResult.CheckIfSuccess());
            Assert.Equal("Test Message", loggedResult.Data);
        }
        #endregion
    }
}
