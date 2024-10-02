namespace ThoughtWorks.CruiseControl.UnitTests.Remote
{
    using System;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using ThoughtWorks.CruiseControl.Remote;

    public class ProjectStatusSnapshotTests
    {
        #region Tests
        [Test]
        public void TimeOfSnapshotCanBeSetAndRetrieved()
        {
            var snapshot = new ProjectStatusSnapshot();
            var time = new DateTime(2010, 1, 2, 3, 4, 5);
            snapshot.TimeOfSnapshot = time;
            ClassicAssert.AreEqual(time, snapshot.TimeOfSnapshot);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }
        #endregion
    }
}
