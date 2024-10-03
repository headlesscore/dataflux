using System.Windows.Forms;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.CCTrayLib.Presentation;
using ThoughtWorks.CruiseControl.Remote;
//using ThoughtWorks.CruiseControl.UnitTests.Core;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Presentation
{
	public class ProjectStatusListViewItemAdaptorTest
	{
		private Mock<IDetailStringProvider> mockProjectDetailStringFormatter;
		private IDetailStringProvider detailStringFormatter;

		//[SetUp]
		public void SetUp()
		{
			mockProjectDetailStringFormatter = new Mock<IDetailStringProvider>();
			detailStringFormatter = (IDetailStringProvider) mockProjectDetailStringFormatter.Object;
		}

		[Fact]
		public void CanCreateListViewItem()
		{
			StubProjectMonitor projectMonitor = new StubProjectMonitor("projectName");

			ProjectStatusListViewItemAdaptor adaptor = new ProjectStatusListViewItemAdaptor(detailStringFormatter);
			ListViewItem item = adaptor.Create(projectMonitor);

			Assert.Equal("projectName", item.Text);
            Assert.Equal("projectName", item.Text);
            Assert.Equal(0, item.ImageIndex);
		}


		[Fact]
		public void WhenTheStateOfTheProjectChangesTheIconIsUpdated()
		{
			StubProjectMonitor projectMonitor = new StubProjectMonitor("projectName");
			ProjectStatusListViewItemAdaptor adaptor = new ProjectStatusListViewItemAdaptor(detailStringFormatter);
			ListViewItem item = adaptor.Create(projectMonitor);

			Assert.Equal("projectName", item.Text);
			Assert.Equal(0, item.ImageIndex);

			projectMonitor.ProjectState = ProjectState.Building;

			projectMonitor.OnPolled(new MonitorPolledEventArgs(projectMonitor));

			Assert.Equal("projectName", item.Text);
			Assert.Equal(ProjectState.Building.ImageIndex, item.ImageIndex);
		}

		[Fact]
		public void WhenTheStateOfTheProjectChangesTheStatusEntriesOnTheListViewItemAreUpdated()
		{
			StubProjectMonitor projectMonitor = new StubProjectMonitor("projectName");
			projectMonitor.ProjectState = ProjectState.Building;
			projectMonitor.ProjectStatus = null;
            projectMonitor.Configuration = new CCTrayProject("http://somewhere", "projectName");

			ProjectStatusListViewItemAdaptor adaptor = new ProjectStatusListViewItemAdaptor(detailStringFormatter);
			ListViewItem item = adaptor.Create(projectMonitor);

			Assert.Equal(10, item.SubItems.Count);
			ListViewItem.ListViewSubItem activity = item.SubItems[3];
			ListViewItem.ListViewSubItem label = item.SubItems[5];

			Assert.Equal("", activity.Text);
			Assert.Equal("", label.Text);

			//ProjectStatus status = ProjectStatusFixture.New(ProjectActivity.Sleeping, "lastLabel");
			//projectMonitor.ProjectStatus = status;

			projectMonitor.OnPolled(new MonitorPolledEventArgs(projectMonitor));

			Assert.Equal("Sleeping", activity.Text);
			Assert.Equal("lastLabel", label.Text);

		}

		[Fact]
		public void UsesDescriptionBuilderToGenerateDetailCaption()
		{
			mockProjectDetailStringFormatter = new Mock<IDetailStringProvider>(MockBehavior.Strict);
			detailStringFormatter = mockProjectDetailStringFormatter.Object;

			StubProjectMonitor projectMonitor = new StubProjectMonitor("projectName");

			mockProjectDetailStringFormatter.Setup(formatter => formatter.FormatDetailString(projectMonitor)).Returns("test1").Verifiable();
			ProjectStatusListViewItemAdaptor adaptor = new ProjectStatusListViewItemAdaptor(detailStringFormatter);
			ListViewItem item = adaptor.Create(projectMonitor);

			ListViewItem.ListViewSubItem detail = item.SubItems[4];
			Assert.Equal("test1", detail.Text);

			mockProjectDetailStringFormatter.Setup(formatter => formatter.FormatDetailString(projectMonitor)).Returns("test2").Verifiable();
			projectMonitor.OnPolled(new MonitorPolledEventArgs(projectMonitor));

			Assert.Equal("test2", detail.Text);

			mockProjectDetailStringFormatter.Verify();
		}

	}
}
