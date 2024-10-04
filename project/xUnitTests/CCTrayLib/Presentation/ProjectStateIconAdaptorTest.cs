using System;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.CCTrayLib;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.CCTrayLib.Presentation;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Presentation
{
	public class ProjectStateIconAdaptorTest
	{
		private Mock<IProjectStateIconProvider> mockIconProvider;
		private StubProjectMonitor monitor;
		private IProjectStateIconProvider iconProvider;

		//// [SetUp]
		public void SetUp()
		{
			monitor = new StubProjectMonitor( "testProject" );

			mockIconProvider = new Mock<IProjectStateIconProvider>(MockBehavior.Strict);

			iconProvider = (IProjectStateIconProvider) this.mockIconProvider.Object;

			this.monitor.ProjectState = ProjectState.Building;

		}

		[Fact]
		public void OnCreationTheCurrentStateOfTheIconIsRead()
		{
			StatusIcon icon = new StatusIcon();
			mockIconProvider.Setup(_iconProvider => _iconProvider.GetStatusIconForState(ProjectState.Building)).Returns(icon).Verifiable();

			ProjectStateIconAdaptor adaptor = new ProjectStateIconAdaptor( monitor, iconProvider );
			Assert.Same( icon, adaptor.StatusIcon );
            Assert.Same(icon, adaptor.StatusIcon);

            mockIconProvider.Verify();
		}

		[Fact]
		public void WhenTheMonitorPollsTheIconMayBeUpdated()
		{
			StatusIcon icon = new StatusIcon();
			mockIconProvider.Setup(_iconProvider => _iconProvider.GetStatusIconForState(ProjectState.Building)).Returns(icon).Verifiable();

			ProjectStateIconAdaptor adaptor = new ProjectStateIconAdaptor( monitor, iconProvider );
			Assert.Same( icon, adaptor.StatusIcon );

			monitor.ProjectState = ProjectState.Broken;

			StatusIcon icon2 = new StatusIcon();
			mockIconProvider.Setup(_iconProvider => _iconProvider.GetStatusIconForState(ProjectState.Broken)).Returns(icon2).Verifiable();

			monitor.Poll();

			Assert.Same( icon2, adaptor.StatusIcon );
			mockIconProvider.Verify();
		}

		int iconChangedCount;
		
		[Fact]
		public void WhenTheStatusIconIsChangedAnEventIsFired()
		{
			iconChangedCount = 0;

			StatusIcon icon = new StatusIcon();
			mockIconProvider.Setup(_iconProvider => _iconProvider.GetStatusIconForState(ProjectState.Building)).Returns(icon).Verifiable();

			ProjectStateIconAdaptor adaptor = new ProjectStateIconAdaptor( monitor, iconProvider );
			adaptor.IconChanged += new EventHandler(IconChanged);

			Assert.Equal(0,iconChangedCount);

			StatusIcon icon2 = new StatusIcon();
			adaptor.StatusIcon = icon2;
			Assert.Equal(1,iconChangedCount);

			adaptor.StatusIcon = icon2;
			Assert.Equal(1,iconChangedCount);

		}

		private void IconChanged( object sender, EventArgs e )
		{
			iconChangedCount++;
		}
	}

}
