using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Remote
{
    [TestFixture]
    public class ProjectActivityTests
    {
        #region Test methods
        #region Properties
        [Test]
        public void TypeGetSetTest()
        {
            ProjectActivity activity = new ProjectActivity();
            activity.Type = "testing";
            ClassicAssert.AreEqual("testing", activity.Type);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }
        #endregion

        #region IsPending()
        [Test]
        public void IsPendingReturnsTrueForPendingType()
        {
            ProjectActivity activity = ProjectActivity.Pending;
            ClassicAssert.IsTrue(activity.IsPending());
        }

        [Test]
        public void IsPendingReturnsFalseForNonPendingType()
        {
            ProjectActivity activity = ProjectActivity.CheckingModifications;
            ClassicAssert.IsFalse(activity.IsPending());
        }
        #endregion

        #region IsCheckingModifications()
        [Test]
        public void IsCheckingModificationsReturnsTrueWhenCheckingModifications()
        {
            ClassicAssert.IsTrue(ProjectActivity.CheckingModifications.IsCheckingModifications());
        }

        [Test]
        public void IsCheckingModificationsReturnsFalseForAllOtherStates()
        {
            ClassicAssert.IsFalse(ProjectActivity.Building.IsCheckingModifications());
            ClassicAssert.IsFalse(ProjectActivity.Pending.IsCheckingModifications());
            ClassicAssert.IsFalse(ProjectActivity.Sleeping.IsCheckingModifications());
        }
        #endregion
        #endregion
    }
}
