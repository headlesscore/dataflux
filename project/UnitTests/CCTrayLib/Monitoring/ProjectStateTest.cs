using System;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Monitoring
{
	[TestFixture]
	public class ProjectStateTest
	{
		[Test]
		public void EqualityIsImplementedCorrectly()
		{
			ClassicAssert.AreEqual(ProjectState.Broken, ProjectState.Broken);
            //ClassicAssert.AreEqual(ProjectState.Broken, ProjectState.Broken);
            ClassicAssert.AreEqual(ProjectState.Success, ProjectState.Success);
			ClassicAssert.AreEqual(ProjectState.NotConnected, ProjectState.NotConnected);
			ClassicAssert.AreEqual(ProjectState.Building, ProjectState.Building);

			ClassicAssert.IsFalse(ProjectState.Broken.Equals(ProjectState.Success));
			ClassicAssert.IsFalse(ProjectState.Broken.Equals(ProjectState.NotConnected));
			ClassicAssert.IsFalse(ProjectState.Broken.Equals(ProjectState.Building));
		}

		[Test]
		public void IsMoreImportantThanIsImplementedCorrectly()
		{
			// broken is most important state to know about
			ClassicAssert.IsTrue(ProjectState.Broken.IsMoreImportantThan(ProjectState.Building));
			ClassicAssert.IsTrue(ProjectState.Broken.IsMoreImportantThan(ProjectState.NotConnected));
			ClassicAssert.IsTrue(ProjectState.Broken.IsMoreImportantThan(ProjectState.Success));

			// building is slightly less important
			ClassicAssert.IsFalse(ProjectState.Building.IsMoreImportantThan(ProjectState.Broken));
			ClassicAssert.IsTrue(ProjectState.Building.IsMoreImportantThan(ProjectState.NotConnected));
			ClassicAssert.IsTrue(ProjectState.Building.IsMoreImportantThan(ProjectState.Success));

			// not connected is next
			ClassicAssert.IsFalse(ProjectState.NotConnected.IsMoreImportantThan(ProjectState.Broken));
			ClassicAssert.IsFalse(ProjectState.NotConnected.IsMoreImportantThan(ProjectState.Building));
			ClassicAssert.IsTrue(ProjectState.NotConnected.IsMoreImportantThan(ProjectState.Success));

			// successful builds "least" important -- i.e. only show this if all projects
			// are in the successful state
			ClassicAssert.IsFalse(ProjectState.Success.IsMoreImportantThan(ProjectState.Broken));
			ClassicAssert.IsFalse(ProjectState.Success.IsMoreImportantThan(ProjectState.NotConnected));
			ClassicAssert.IsFalse(ProjectState.Success.IsMoreImportantThan(ProjectState.Building));
		}

		[Test]
		public void ToStringReturnsStateName()
		{
			ClassicAssert.AreEqual(ProjectState.Broken.Name, ProjectState.Broken.ToString());
		}
	}
}
