using System;
using Xunit;

using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Monitoring
{
	public class ProjectStateTest
	{
		[Fact]
		public void EqualityIsImplementedCorrectly()
		{
			Assert.Equal(ProjectState.Broken, ProjectState.Broken);
            //Assert.Equal(ProjectState.Broken, ProjectState.Broken);
            Assert.Equal(ProjectState.Success, ProjectState.Success);
			Assert.Equal(ProjectState.NotConnected, ProjectState.NotConnected);
			Assert.Equal(ProjectState.Building, ProjectState.Building);

			Assert.False(ProjectState.Broken.Equals(ProjectState.Success));
			Assert.False(ProjectState.Broken.Equals(ProjectState.NotConnected));
			Assert.False(ProjectState.Broken.Equals(ProjectState.Building));
		}

		[Fact]
		public void IsMoreImportantThanIsImplementedCorrectly()
		{
			// broken is most important state to know about
			Assert.True(ProjectState.Broken.IsMoreImportantThan(ProjectState.Building));
			Assert.True(ProjectState.Broken.IsMoreImportantThan(ProjectState.NotConnected));
			Assert.True(ProjectState.Broken.IsMoreImportantThan(ProjectState.Success));

			// building is slightly less important
			Assert.False(ProjectState.Building.IsMoreImportantThan(ProjectState.Broken));
			Assert.True(ProjectState.Building.IsMoreImportantThan(ProjectState.NotConnected));
			Assert.True(ProjectState.Building.IsMoreImportantThan(ProjectState.Success));

			// not connected is next
			Assert.False(ProjectState.NotConnected.IsMoreImportantThan(ProjectState.Broken));
			Assert.False(ProjectState.NotConnected.IsMoreImportantThan(ProjectState.Building));
			Assert.True(ProjectState.NotConnected.IsMoreImportantThan(ProjectState.Success));

			// successful builds "least" important -- i.e. only show this if all projects
			// are in the successful state
			Assert.False(ProjectState.Success.IsMoreImportantThan(ProjectState.Broken));
			Assert.False(ProjectState.Success.IsMoreImportantThan(ProjectState.NotConnected));
			Assert.False(ProjectState.Success.IsMoreImportantThan(ProjectState.Building));
		}

		[Fact]
		public void ToStringReturnsStateName()
		{
			Assert.Equal(ProjectState.Broken.Name, ProjectState.Broken.ToString());
		}
	}
}
