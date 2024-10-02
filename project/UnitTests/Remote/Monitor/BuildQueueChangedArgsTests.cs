namespace ThoughtWorks.CruiseControl.UnitTests.Remote.Monitor
{
    using Moq;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using ThoughtWorks.CruiseControl.Remote;
    using ThoughtWorks.CruiseControl.Remote.Monitor;

    public class BuildQueueChangedArgsTests
    {
        #region Tests
        [Test]
        public void ConstructorSetsBuildQueue()
        {
            var mocks = new MockRepository(MockBehavior.Default);
            var client = mocks.Create<CruiseServerClientBase>().Object;
            var watcher = mocks.Create<IServerWatcher>().Object;
            var monitor = new Server(client, watcher);
            var queue = new BuildQueue(client, monitor, new QueueSnapshot());
            var args = new BuildQueueChangedArgs(queue);
            ClassicAssert.AreSame(queue, args.BuildQueue);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }
        #endregion
    }
}
