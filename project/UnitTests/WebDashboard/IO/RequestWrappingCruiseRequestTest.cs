using System.Collections.Specialized;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Reporting.Dashboard.Navigation;
using ThoughtWorks.CruiseControl.WebDashboard.IO;
using ThoughtWorks.CruiseControl.WebDashboard.MVC;

namespace ThoughtWorks.CruiseControl.UnitTests.WebDashboard.IO
{
	[TestFixture]
	public class RequestWrappingCruiseRequestTest
	{
		private NameValueCollection queryString;
		private IRequest underlyingRequest;
		private RequestWrappingCruiseRequest cruiseRequest;
		private string applicationPath;

		[SetUp]
		public void Setup()
		{
			queryString = new NameValueCollection();
			applicationPath = "http://bar/";
			CreateCruiseRequest("baz.html");
		}

		private void CreateCruiseRequest(string relativePath)
		{
            var urlBuilderMock = new Mock<ICruiseUrlBuilder>();
            underlyingRequest = new NameValueCollectionRequest(queryString, null, applicationPath + relativePath, null, applicationPath);
            cruiseRequest = new RequestWrappingCruiseRequest(underlyingRequest, (ICruiseUrlBuilder)urlBuilderMock.Object, null);
		}

		[Test]
		public void ReturnsEmptyStringIfNoProjectSpecified()
		{
			ClassicAssert.AreEqual(string.Empty, cruiseRequest.ProjectName);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

		[Test]
		public void ReturnsProjectNameIfProjectSpecified()
		{
			CreateCruiseRequest("server/myserver/project/myproject/baz.html");
			ClassicAssert.AreEqual("myproject", cruiseRequest.ProjectName);
		}

		[Test]
		public void ReturnsEmptyStringIfNoServerSpecified()
		{
			ClassicAssert.AreEqual(string.Empty, cruiseRequest.ServerName);
		}

		[Test]
		public void ReturnsServerNameIfServerSpecified()
		{
			CreateCruiseRequest("server/myserver/baz.html");
			ClassicAssert.AreEqual("myserver", cruiseRequest.ServerName);
		}

		[Test]
		public void ReturnsEmptyStringIfNoBuildSpecified()
		{
			ClassicAssert.AreEqual(string.Empty, cruiseRequest.BuildName);
		}

		[Test]
		public void ReturnsBuildNameIfBuildSpecified()
		{
			CreateCruiseRequest("server/myserver/project/myproject/build/mybuild/baz.html");
			ClassicAssert.AreEqual("mybuild", cruiseRequest.BuildName);
		}

		[Test]
		public void DecodeServerName()
		{
			CreateCruiseRequest("server/my+server/baz.html");
			ClassicAssert.AreEqual("my server", cruiseRequest.ServerName);
		}

		[Test]
		public void DecodeProjectName()
		{
			CreateCruiseRequest("server/my+server/project/my+project%232/baz.html");
			ClassicAssert.AreEqual("my project#2", cruiseRequest.ProjectName);
		}
	}
}
