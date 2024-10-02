using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Reporting.Dashboard.Navigation;
using ThoughtWorks.CruiseControl.WebDashboard.Dashboard;

namespace ThoughtWorks.CruiseControl.UnitTests.WebDashboard.Dashboard
{
	[TestFixture]
	public class ProjectLinkTest
	{
		private Mock<ICruiseUrlBuilder> urlBuilderMock;
		private string serverName = "my server";
		private string projectName = "my project";
		private IProjectSpecifier projectSpecifier;
		private string description = "my description";
		private ProjectLink projectLink;
		private string action = "my action";

		[SetUp]
		public void Setup()
		{
			projectSpecifier = new DefaultProjectSpecifier(new DefaultServerSpecifier(serverName), projectName);
			urlBuilderMock = new Mock<ICruiseUrlBuilder>();
			projectLink = new ProjectLink((ICruiseUrlBuilder) urlBuilderMock.Object, projectSpecifier, description, this.action);
		}

		private void VerifyAll()
		{
			urlBuilderMock.Verify();
		}

		[Test]
		public void ShouldReturnGivenDescription()
		{
			ClassicAssert.AreEqual(description, projectLink.Text);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
            VerifyAll();
		}

		[Test]
		public void ShouldReturnCalculatedAbsoluteUrl()
		{
			urlBuilderMock.Setup(builder => builder.BuildProjectUrl(action, projectSpecifier)).Returns("my absolute url").Verifiable();
			ClassicAssert.AreEqual("my absolute url", projectLink.Url);
			VerifyAll();
		}
	}
}
