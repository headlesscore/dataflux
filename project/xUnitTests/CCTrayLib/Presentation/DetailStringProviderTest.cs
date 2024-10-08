using System;
using System.Collections.Generic;
using Xunit;

using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.CCTrayLib.Presentation;
using ThoughtWorks.CruiseControl.Remote;
using ThoughtWorks.CruiseControl.Remote.Parameters;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Presentation
{
	public class DetailStringProviderTest
	{
		private StubProjectMonitor monitor;
		private DetailStringProvider provider;

		//// [SetUp]
		protected void SetUp()
		{
			monitor = new StubProjectMonitor("name");
			provider = new DetailStringProvider();			
		}
		[Fact]
		public void WhenTheProjecStatusIndicatesAnExceptionItsMessageIsReportedInTheDetailString()
		{
			Assert.Equal("Connecting...", provider.FormatDetailString(monitor.Detail));
            Assert.Equal("Connecting...", provider.FormatDetailString(monitor.Detail));

            monitor.SetUpAsIfExceptionOccurredOnConnect(new ApplicationException("message"));

			Assert.Equal("Error: message", provider.FormatDetailString(monitor.Detail));
		}

		[Fact]
		public void WhenSleepingIndicatesTimeOfNextBuildCheck()
		{
			DateTime nextBuildTime = new DateTime(2005, 7, 20, 15, 12, 30);
			monitor.ProjectStatus = CreateNewProjectStatus(nextBuildTime);
			monitor.ProjectState = ProjectState.Success;

			Assert.Equal(string.Format(System.Globalization.CultureInfo.CurrentCulture,"Next build check: {0:G}", nextBuildTime), provider.FormatDetailString(monitor.Detail));
		}

		[Fact]
		public void WhenTheNextBuildTimeIsMaxValueIndicateThatNoBuildIsScheduled()
		{
			DateTime nextBuildTime = DateTime.MaxValue;
			monitor.ProjectStatus = CreateNewProjectStatus(nextBuildTime);
			monitor.ProjectState = ProjectState.Success;

			Assert.Equal("Project is not automatically triggered", provider.FormatDetailString(monitor.Detail));
		}

		[Fact]
		public void IncludeCurrentProjectMessage()
		{
			monitor.ProjectStatus = CreateNewProjectStatus(DateTime.MaxValue);
			monitor.ProjectStatus.Messages = new Message[] { new Message("foo") };
			monitor.ProjectState = ProjectState.Success;

			Assert.Equal("Project is not automatically triggered - foo", provider.FormatDetailString(monitor.Detail));			
		}

		private static ProjectStatus CreateNewProjectStatus(DateTime nextBuildTime)
		{
			return new ProjectStatus(
				"NAME", "category",
				ProjectActivity.Sleeping,
				IntegrationStatus.Unknown,
                ProjectIntegratorState.Running, "url", DateTime.MinValue, "lastLabel", null, nextBuildTime, string.Empty, string.Empty, 0, new List<ParameterBase>());
		}
	}
}
