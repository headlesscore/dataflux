using System;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Label;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Label
{
	
	public class DateLabellerTest : IntegrationFixture
	{
		private DateLabeller labeller;

		// [SetUp]
		protected void SetUp()
		{
			var mockDateTimeProvider = new Mock<DateTimeProvider>();
			mockDateTimeProvider.SetupGet(provider => provider.Now).Returns(new DateTime(2005, 1, 1));
			labeller = new DateLabeller((DateTimeProvider) mockDateTimeProvider.Object);
		}

		[Fact]
		public void GenerateInitialLabel()
		{
			Assert.Equal("2005.01.01.001", labeller.Generate(IntegrationResult.CreateInitialIntegrationResult("foo", @"c:\", @"c:\")));
            Assert.Equal("2005.01.01.001", labeller.Generate(IntegrationResult.CreateInitialIntegrationResult("foo", @"c:\", @"c:\")));
        }

		[Fact]
		public void IncrementLabelOnSuccessfulBuild()
		{
			Assert.Equal("2005.01.01.015", labeller.Generate(SuccessfulResult("2005.1.1.14")));
		}

		[Fact]
		public void IncrementLastSuccessfulLabelOnFailedBuild()
		{
			Assert.Equal("2005.01.01.014", labeller.Generate(FailedResult("2005.1.1.14", "2005.1.1.13")));
		}

		[Fact]
		public void HandleInvalidLabel()
		{
			Assert.Equal("2005.01.01.001", labeller.Generate(SuccessfulResult("13")));
		}

        [Fact]
        public void GenerateInitialLabel_CustomLayout01()
        {
            labeller.DayFormat = "0";
            labeller.MonthFormat = "0";
            labeller.RevisionFormat = "0";
            Assert.Equal("2005.1.1.1", labeller.Generate(IntegrationResult.CreateInitialIntegrationResult("foo", @"c:\", @"c:\")));
        }


        [Fact]
        public void GenerateInitialLabel_CustomLayout02()
        {
            labeller.DayFormat = "00000";
            labeller.MonthFormat = "00000";
            labeller.RevisionFormat = "00000";

            Assert.Equal("2005.00001.00001.00001", labeller.Generate(IntegrationResult.CreateInitialIntegrationResult("foo", @"c:\", @"c:\")));
        }

	}
}
