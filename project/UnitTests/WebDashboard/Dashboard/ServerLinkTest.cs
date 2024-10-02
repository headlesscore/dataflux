using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Reporting.Dashboard.Navigation;
using ThoughtWorks.CruiseControl.WebDashboard.Dashboard;

namespace ThoughtWorks.CruiseControl.UnitTests.WebDashboard.Dashboard
{
	[TestFixture]
	public class ServerLinkTest
	{
		private Mock<ICruiseUrlBuilder> urlBuilderMock;
		private string serverName = "my server";
		private IServerSpecifier serverSpecifier;
		private string description = "my description";
		private ServerLink serverLink;
		private string action = "my action";

		[SetUp]
		public void Setup()
		{
			serverSpecifier = new DefaultServerSpecifier(serverName);
			urlBuilderMock = new Mock<ICruiseUrlBuilder>();
			serverLink = new ServerLink((ICruiseUrlBuilder) urlBuilderMock.Object, serverSpecifier, description, this.action);
		}

		private void VerifyAll()
		{
			urlBuilderMock.Verify();
		}

		[Test]
		public void ShouldReturnGivenDescription()
		{
			ClassicAssert.AreEqual(description, serverLink.Text);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
            VerifyAll();
		}

		[Test]
		public void ShouldReturnCalculatedAbsoluteUrl()
		{
			urlBuilderMock.Setup(builder => builder.BuildServerUrl(action, serverSpecifier)).Returns("my absolute url").Verifiable();
			ClassicAssert.AreEqual("my absolute url", serverLink.Url);
			VerifyAll();
		}
	}
}
