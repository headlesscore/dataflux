using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Remote
{
    [TestFixture]
    public class QueuedRequestSnapshotTests
    {
        #region Test methods
        #region Properties
        [Test]
        public void ProjectNameGetSetTest()
        {
            QueuedRequestSnapshot activity = new QueuedRequestSnapshot();
            activity.ProjectName = "testing";
            ClassicAssert.AreEqual("testing", activity.ProjectName);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void ActivityGetSetTest()
        {
            QueuedRequestSnapshot activity = new QueuedRequestSnapshot();
            activity.Activity = ProjectActivity.Building;
            ClassicAssert.AreEqual(ProjectActivity.Building, activity.Activity);
        }

        [Test]
        public void LastBuildDateGetSetTest()
        {
            DateTime timeNow = DateTime.Now;
            QueuedRequestSnapshot activity = new QueuedRequestSnapshot();
            activity.RequestTime = timeNow;
            ClassicAssert.AreEqual(timeNow, activity.RequestTime);
        }
        #endregion
        #endregion
    }
}
