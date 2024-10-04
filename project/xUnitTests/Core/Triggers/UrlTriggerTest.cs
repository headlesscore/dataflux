using System;
using Exortech.NetReflector;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core.Triggers;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Triggers
{
	
	public class UrlTriggerTest : IntegrationFixture
	{
		private Mock<DateTimeProvider> mockDateTime;
		private Mock<HttpWrapper> mockHttpWrapper;
		private UrlTrigger trigger;
		private DateTime initialDateTimeNow;
		private const string DefaultUrl = @"http://confluence.public.thoughtworks.org/";

		// [SetUp]
		public void Setup()
		{
			Source = "UrlTrigger";
			initialDateTimeNow = new DateTime(2002, 1, 2, 3, 0, 0, 0);
			mockDateTime = new Mock<DateTimeProvider>();
			mockDateTime.SetupGet(provider => provider.Now).Returns(this.initialDateTimeNow);
			mockHttpWrapper = new Mock<HttpWrapper>();

			trigger = new UrlTrigger((DateTimeProvider) mockDateTime.Object, (HttpWrapper) mockHttpWrapper.Object);
			trigger.Url = DefaultUrl;
		}

		public void VerifyAll()
		{
			mockDateTime.Verify();
			mockHttpWrapper.Verify();
		}

		[Fact]
		public void ShouldFullyPopulateFromReflector()
		{
			string xml = string.Format(@"<urlTrigger name=""url"" seconds=""1"" buildCondition=""ForceBuild"" url=""{0}"" />", DefaultUrl);
			trigger = (UrlTrigger) NetReflector.Read(xml);
            Assert.Equal(1, trigger.IntervalSeconds, "trigger.IntervalSeconds");
            Assert.Equal(BuildCondition.ForceBuild, trigger.BuildCondition, "trigger.BuildCondition");
            Assert.Equal(DefaultUrl, trigger.Url, "trigger.Url");
            Assert.Equal("url", trigger.Name, "trigger.Name");
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public void ShouldDefaultPopulateFromReflector()
		{
			string xml = string.Format(@"<urlTrigger url=""{0}"" />", DefaultUrl);
			trigger = (UrlTrigger) NetReflector.Read(xml);
            Assert.Equal(UrlTrigger.DefaultIntervalSeconds, trigger.IntervalSeconds, "trigger.IntervalSeconds");
            Assert.Equal(BuildCondition.IfModificationExists, trigger.BuildCondition, "trigger.BuildCondition");
            Assert.Equal(DefaultUrl, trigger.Url, "trigger.Url");
			Assert.Equal("UrlTrigger", trigger.Name, "trigger.Name");
		}

		[Fact]
		public void ShouldNotBuildFirstTime()
		{
            Assert.Null(trigger.Fire());
			VerifyAll();
		}


        [Fact]
        public void ShouldBuildAfterFirstInterval()
        {
            mockDateTime.SetupGet(provider => provider.Now).Returns(initialDateTimeNow.AddSeconds(trigger.IntervalSeconds));
            mockHttpWrapper.Setup(http => http.GetLastModifiedTimeFor(new Uri(DefaultUrl), DateTime.MinValue)).Returns(initialDateTimeNow).Verifiable();
            Assert.Equal(ModificationExistRequest(), trigger.Fire(), "trigger.Fire()");
            VerifyAll();
        }

        [Fact]
		public void ShouldNotBuildIfUrlHasNotChanged()
		{
            // Initial check, no build
			mockHttpWrapper.Setup(http => http.GetLastModifiedTimeFor(new Uri(DefaultUrl), DateTime.MinValue)).Returns(initialDateTimeNow).Verifiable();
            Assert.Null(trigger.Fire());
			trigger.IntegrationCompleted();

            // First interval passes, initial build because url date/time is unknown
            mockDateTime.SetupGet(provider => provider.Now).Returns(initialDateTimeNow.AddSeconds(trigger.IntervalSeconds));
            Assert.Equal(ModificationExistRequest(), trigger.Fire());

            // Second interval passes, no build because url has not changed
			mockDateTime.SetupGet(provider => provider.Now).Returns(initialDateTimeNow.AddSeconds(trigger.IntervalSeconds * 2));
			mockHttpWrapper.Setup(http => http.GetLastModifiedTimeFor(new Uri(DefaultUrl), initialDateTimeNow)).Returns(initialDateTimeNow).Verifiable();
            Assert.Null(trigger.Fire());
            Assert.Equal(initialDateTimeNow.AddSeconds(trigger.IntervalSeconds * 3), trigger.NextBuild);		// Next build should be at third interval
			VerifyAll();
		}

		[Fact]
		public void ShouldHandleExceptionAccessingUrl()
		{
			mockHttpWrapper.Setup(http => http.GetLastModifiedTimeFor(new Uri(DefaultUrl), DateTime.MinValue)).Throws(new Exception("Uh-oh")).Verifiable();
            Assert.Null(trigger.Fire(), "trigger.Fire()");
		}
	}
}
