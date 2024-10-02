namespace ThoughtWorks.CruiseControl.UnitTests.Remote
{
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using ThoughtWorks.CruiseControl.Remote;

    public class QueueSetSnapshotTests
    {
        #region Tests
        [Test]
        public void ConstructorWorks()
        {
            var snapshot = new QueueSetSnapshot();
            ClassicAssert.IsNotNull(snapshot.Queues);
            ClassicAssert.IsEmpty(snapshot.Queues);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }
        #endregion
    }
}
