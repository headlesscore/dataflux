using System;
using Exortech.NetReflector;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Label;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Label
{
	
	public class IterationLabellerTest : IntegrationFixture
	{
		private IterationLabeller labeller;
		private DateTime releaseStartDate = new DateTime(2005, 01, 01, 00, 00, 00, 00);
		private Mock<DateTimeProvider> dateTimeMock;

		// [SetUp]
		public void SetUp()
		{
			dateTimeMock = new Mock<DateTimeProvider>();
			dateTimeMock.SetupGet(provider => provider.Today).Returns(new DateTime(2005, 7, 20, 0, 0, 0, 0));
			labeller = new IterationLabeller((DateTimeProvider) dateTimeMock.Object);
			labeller.ReleaseStartDate = releaseStartDate;
		}

		[Fact]
		public void PopulateFromConfiguration()
		{
			string xml = string.Format(@"<iterationlabeller incrementOnFailure=""true"" duration=""1"" releaseStartDate=""{0}"" prefix=""foo"" separator=""-"" />", releaseStartDate);
			labeller = (IterationLabeller) NetReflector.Read(xml);
			Assert.Equal(true, labeller.IncrementOnFailed);
			Assert.Equal(1, labeller.Duration);
			Assert.Equal("foo", labeller.LabelPrefix);
			Assert.Equal("-", labeller.Separator);
            Assert.Equal("-", labeller.Separator);
            Assert.Equal(releaseStartDate, labeller.ReleaseStartDate);
		}

		[Fact]
		public void GenerateIncrementedLabel()
		{
			Assert.Equal("14.36", labeller.Generate(SuccessfulResult("14.35")));
		}

		[Fact]
		public void GenerateWithNullLabel()
		{
			Assert.Equal("14.1", labeller.Generate(SuccessfulResult(null)));
		}

		[Fact]
		public void GenerateAfterLastBuildFailed()
		{
			Assert.Equal("14.23", labeller.Generate(FailedResult("14.23")));
		}

		[Fact]
		public void GeneratePrefixedLabelWithNullResultLabel()
		{
			labeller.LabelPrefix = "Sample";
			Assert.Equal("Sample.14.1", labeller.Generate(SuccessfulResult(null)));
		}

		[Fact]
		public void GeneratePrefixedLabelOnSuccessAndPreviousLabel()
		{
			labeller.LabelPrefix = "Sample";
			Assert.Equal("Sample.14.24", labeller.Generate(SuccessfulResult("Sample.14.23")));
		}

		[Fact]
		public void GeneratePrefixedLabelOnFailureAndPreviousLabel()
		{
			labeller.LabelPrefix = "Sample";
			Assert.Equal("Sample.14.23", labeller.Generate(FailedResult("Sample.14.23")));
		}

		[Fact]
		public void GeneratePrefixedLabelOnSuccessAndPreviousLabelWithDifferentPrefix()
		{
			labeller.LabelPrefix = "Sample";
			Assert.Equal("Sample.14.24", labeller.Generate(SuccessfulResult("SomethingElse.14.23")));
		}

		[Fact]
		public void IncrementPrefixedLabelWithNumericPrefix()
		{
			labeller.LabelPrefix = "R3SX";
			Assert.Equal("R3SX.14.24", labeller.Generate(SuccessfulResult("R3SX.14.23")));
		}

		[Fact]
		public void IncrementPrefixedLabelWithNumericSeperatorSeperatedPrefix()
		{
			labeller.LabelPrefix = "1.0";
			Assert.Equal("1.0.14.24", labeller.Generate(SuccessfulResult("1.0.14.23")));
		}

		[Fact]
		public void WhenTheBuildIsPerformedDuringANewIterationTheIterationNumberIsUpdatedAndTheLabelReset()
		{
			// Set the release start date needs to be 15 iterations ago
			// from today.  So take today's date and remove 15 weeks and a couple more days.
			dateTimeMock.SetupGet(provider => provider.Today).Returns(DateTime.Today);
			labeller.ReleaseStartDate = DateTime.Today.AddDays(- (15*7 + 2));

			// one week iterations
			labeller.Duration = 1;
			Assert.Equal("15.1", labeller.Generate(SuccessfulResult("14.35")));
		}

		[Fact]
		public void WhenTheLabelIsUpdatedDueToANewIterationThePrefixRemains()
		{
			// Set the release start date needs to be 15 iterations ago
			// from today.  So take today's date and remove 15 weeks and a couple more days.
			dateTimeMock.SetupGet(provider => provider.Today).Returns(DateTime.Today);
			labeller.ReleaseStartDate = DateTime.Today.AddDays(- (15*7 + 2));

			// one week iterations
			labeller.Duration = 1;

			labeller.LabelPrefix = "R3SX";
			Assert.Equal("R3SX.15.1", labeller.Generate(SuccessfulResult("R3SX.14.23")));
		}

		[Fact]
		public void ShouldCorrectlyGenerateInitialLabel()
		{
			labeller.ReleaseStartDate = new DateTime(2005, 6, 2);
			labeller.LabelPrefix = "5.3.";
			labeller.Duration = 1;
			Assert.Equal("5.3.6.1", labeller.Generate(IntegrationResult.CreateInitialIntegrationResult("foo", "c:\\bar", "c:\\baz")));
		}

		[Fact]
		public void GenerateIncrementedLabelOnFailureIfIncrementOnFailedIsTrue()
		{
			labeller.IncrementOnFailed = true;
			Assert.Equal("14.36", labeller.Generate(FailedResult("14.35")));
		}
	}
}
