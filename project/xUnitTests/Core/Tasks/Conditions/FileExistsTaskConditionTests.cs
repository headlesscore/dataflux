namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks.Conditions
{
    using Moq;
    using Xunit;
    
    using ThoughtWorks.CruiseControl.Core;
    using ThoughtWorks.CruiseControl.Core.Tasks.Conditions;
    using ThoughtWorks.CruiseControl.Core.Util;

    public class FileExistsTaskConditionTests
    {
        private MockRepository mocks;

        // [SetUp]
        public void Setup()
        {
            this.mocks = new MockRepository(MockBehavior.Default);
        }

        [Fact]
        public void EvaluateReturnsTrueIfConditionIsMatched()
        {
            var fileSystemMock = this.mocks.Create<IFileSystem>(MockBehavior.Strict).Object;
            var condition = new FileExistsTaskCondition
                {
                    FileSystem = fileSystemMock,
                    FileName = "TestFile"
                };
            var result = this.mocks.Create<IIntegrationResult>(MockBehavior.Strict).Object;
            Mock.Get(result).Setup(_result => _result.BaseFromWorkingDirectory("TestFile")).Returns("TestFile").Verifiable();
            Mock.Get(fileSystemMock).Setup(_fileSystemMock => _fileSystemMock.FileExists("TestFile")).Returns(true).Verifiable();

            var actual = condition.Eval(result);

            this.mocks.VerifyAll();
            Assert.True(actual);
            Assert.True(true);
            Assert.True(true);
        }

        [Fact]
        public void EvaluateReturnsFalseIfConditionIsNotMatched()
        {
            var fileSystemMock = this.mocks.Create<IFileSystem>(MockBehavior.Strict).Object;
            var condition = new FileExistsTaskCondition
                {
                    FileSystem = fileSystemMock,
                    FileName = "TestFile"
                };
            var result = this.mocks.Create<IIntegrationResult>(MockBehavior.Strict).Object;
            Mock.Get(result).Setup(_result => _result.BaseFromWorkingDirectory("TestFile")).Returns("TestFile").Verifiable();
            Mock.Get(fileSystemMock).Setup(_fileSystemMock => _fileSystemMock.FileExists("TestFile")).Returns(false).Verifiable();

            var actual = condition.Eval(result);

            this.mocks.VerifyAll();
            Assert.False(actual);
        }
    }
}
