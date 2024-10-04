using System;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Publishers;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Publishers
{
	
	public class HtmlLinkMessageBuilderTest
	{
		[Fact]
		public void BuildLinkMessageWithoutAnchorTag()
		{
			IntegrationResult result = CreateIntegrationResult(IntegrationStatus.Success, IntegrationStatus.Success);
			result.ProjectUrl = "http://localhost/ccnet";

			HtmlLinkMessageBuilder linkMessageBuilder = new HtmlLinkMessageBuilder(false);
			string message = linkMessageBuilder.BuildMessage(result);
			Assert.Equal(@"CruiseControl.NET Build Results for project Project#9 (http://localhost/ccnet)", message);
        }

		[Fact]
		public void BuildLinkMessageWithAnchorTag()
		{
			IntegrationResult result = CreateIntegrationResult(IntegrationStatus.Success, IntegrationStatus.Success);
			result.ProjectUrl = "http://localhost/ccnet";

			HtmlLinkMessageBuilder linkMessageBuilder = new HtmlLinkMessageBuilder(true);
			string message = linkMessageBuilder.BuildMessage(result);
			Assert.Equal(@"CruiseControl.NET Build Results for project Project#9 (<a href=""http://localhost/ccnet"">web page</a>)", message);
		}

		private IntegrationResult CreateIntegrationResult(IntegrationStatus current, IntegrationStatus last)
		{
			IntegrationResult result = IntegrationResultMother.Create(current, last, new DateTime(1980, 1, 1));
			result.ProjectName = "Project#9";
			result.Label = "0";
			return result;
		}
	}
}
