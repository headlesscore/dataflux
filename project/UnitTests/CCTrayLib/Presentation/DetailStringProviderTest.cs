using System;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.CCTrayLib.Presentation;
using ThoughtWorks.CruiseControl.Remote;
using ThoughtWorks.CruiseControl.Remote.Parameters;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Presentation
{
	[TestFixture]
	public class DetailStringProviderTest
	{
		private StubProjectMonitor monitor;
		private DetailStringProvider provider;

		[SetUp]
		protected void SetUp()
		{
			monitor = new StubProjectMonitor("name");
			provider = new DetailStringProvider();			
		}
		[Test]
		public void WhenTheProjecStatusIndicatesAnExceptionItsMessageIsReportedInTheDetailString()
		{
			ClassicAssert.AreEqual("Connecting...", provider.FormatDetailString(monitor.Detail));
            ClassicAssert.AreEqual("Connecting...", provider.FormatDetailString(monitor.Detail));

            monitor.SetUpAsIfExceptionOccurredOnConnect(new ApplicationException("message"));

			ClassicAssert.AreEqual("Error: message", provider.FormatDetailString(monitor.Detail));
		}

		[Test]
		public void WhenSleepingIndicatesTimeOfNextBuildCheck()
		{
			DateTime nextBuildTime = new DateTime(2005, 7, 20, 15, 12, 30);
			monitor.ProjectStatus = CreateNewProjectStatus(nextBuildTime);
			monitor.ProjectState = ProjectState.Success;

			ClassicAssert.AreEqual(string.Format(System.Globalization.CultureInfo.CurrentCulture,"Next build check: {0:G}", nextBuildTime), provider.FormatDetailString(monitor.Detail));
		}

		[Test]
		public void WhenTheNextBuildTimeIsMaxValueIndicateThatNoBuildIsScheduled()
		{
			DateTime nextBuildTime = DateTime.MaxValue;
			monitor.ProjectStatus = CreateNewProjectStatus(nextBuildTime);
			monitor.ProjectState = ProjectState.Success;

			ClassicAssert.AreEqual("Project is not automatically triggered", provider.FormatDetailString(monitor.Detail));
		}

		[Test]
		public void IncludeCurrentProjectMessage()
		{
			monitor.ProjectStatus = CreateNewProjectStatus(DateTime.MaxValue);
			monitor.ProjectStatus.Messages = new Message[] { new Message("foo") };
			monitor.ProjectState = ProjectState.Success;

			ClassicAssert.AreEqual("Project is not automatically triggered - foo", provider.FormatDetailString(monitor.Detail));			
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
