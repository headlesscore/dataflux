using System;
using Exortech.NetReflector;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Label;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Label
{
	[TestFixture]
	public class IterationLabellerTest : IntegrationFixture
	{
		private IterationLabeller labeller;
		private DateTime releaseStartDate = new DateTime(2005, 01, 01, 00, 00, 00, 00);
		private Mock<DateTimeProvider> dateTimeMock;

		[SetUp]
		public void SetUp()
		{
			dateTimeMock = new Mock<DateTimeProvider>();
			dateTimeMock.SetupGet(provider => provider.Today).Returns(new DateTime(2005, 7, 20, 0, 0, 0, 0));
			labeller = new IterationLabeller((DateTimeProvider) dateTimeMock.Object);
			labeller.ReleaseStartDate = releaseStartDate;
		}

		[Test]
		public void PopulateFromConfiguration()
		{
			string xml = string.Format(@"<iterationlabeller incrementOnFailure=""true"" duration=""1"" releaseStartDate=""{0}"" prefix=""foo"" separator=""-"" />", releaseStartDate);
			labeller = (IterationLabeller) NetReflector.Read(xml);
			ClassicAssert.AreEqual(true, labeller.IncrementOnFailed);
			ClassicAssert.AreEqual(1, labeller.Duration);
			ClassicAssert.AreEqual("foo", labeller.LabelPrefix);
			ClassicAssert.AreEqual("-", labeller.Separator);
            ClassicAssert.AreEqual("-", labeller.Separator);
            ClassicAssert.AreEqual(releaseStartDate, labeller.ReleaseStartDate);
		}

		[Test]
		public void GenerateIncrementedLabel()
		{
			ClassicAssert.AreEqual("14.36", labeller.Generate(SuccessfulResult("14.35")));
		}

		[Test]
		public void GenerateWithNullLabel()
		{
			ClassicAssert.AreEqual("14.1", labeller.Generate(SuccessfulResult(null)));
		}

		[Test]
		public void GenerateAfterLastBuildFailed()
		{
			ClassicAssert.AreEqual("14.23", labeller.Generate(FailedResult("14.23")));
		}

		[Test]
		public void GeneratePrefixedLabelWithNullResultLabel()
		{
			labeller.LabelPrefix = "Sample";
			ClassicAssert.AreEqual("Sample.14.1", labeller.Generate(SuccessfulResult(null)));
		}

		[Test]
		public void GeneratePrefixedLabelOnSuccessAndPreviousLabel()
		{
			labeller.LabelPrefix = "Sample";
			ClassicAssert.AreEqual("Sample.14.24", labeller.Generate(SuccessfulResult("Sample.14.23")));
		}

		[Test]
		public void GeneratePrefixedLabelOnFailureAndPreviousLabel()
		{
			labeller.LabelPrefix = "Sample";
			ClassicAssert.AreEqual("Sample.14.23", labeller.Generate(FailedResult("Sample.14.23")));
		}

		[Test]
		public void GeneratePrefixedLabelOnSuccessAndPreviousLabelWithDifferentPrefix()
		{
			labeller.LabelPrefix = "Sample";
			ClassicAssert.AreEqual("Sample.14.24", labeller.Generate(SuccessfulResult("SomethingElse.14.23")));
		}

		[Test]
		public void IncrementPrefixedLabelWithNumericPrefix()
		{
			labeller.LabelPrefix = "R3SX";
			ClassicAssert.AreEqual("R3SX.14.24", labeller.Generate(SuccessfulResult("R3SX.14.23")));
		}

		[Test]
		public void IncrementPrefixedLabelWithNumericSeperatorSeperatedPrefix()
		{
			labeller.LabelPrefix = "1.0";
			ClassicAssert.AreEqual("1.0.14.24", labeller.Generate(SuccessfulResult("1.0.14.23")));
		}

		[Test]
		public void WhenTheBuildIsPerformedDuringANewIterationTheIterationNumberIsUpdatedAndTheLabelReset()
		{
			// Set the release start date needs to be 15 iterations ago
			// from today.  So take today's date and remove 15 weeks and a couple more days.
			dateTimeMock.SetupGet(provider => provider.Today).Returns(DateTime.Today);
			labeller.ReleaseStartDate = DateTime.Today.AddDays(- (15*7 + 2));

			// one week iterations
			labeller.Duration = 1;
			ClassicAssert.AreEqual("15.1", labeller.Generate(SuccessfulResult("14.35")));
		}

		[Test]
		public void WhenTheLabelIsUpdatedDueToANewIterationThePrefixRemains()
		{
			// Set the release start date needs to be 15 iterations ago
			// from today.  So take today's date and remove 15 weeks and a couple more days.
			dateTimeMock.SetupGet(provider => provider.Today).Returns(DateTime.Today);
			labeller.ReleaseStartDate = DateTime.Today.AddDays(- (15*7 + 2));

			// one week iterations
			labeller.Duration = 1;

			labeller.LabelPrefix = "R3SX";
			ClassicAssert.AreEqual("R3SX.15.1", labeller.Generate(SuccessfulResult("R3SX.14.23")));
		}

		[Test]
		public void ShouldCorrectlyGenerateInitialLabel()
		{
			labeller.ReleaseStartDate = new DateTime(2005, 6, 2);
			labeller.LabelPrefix = "5.3.";
			labeller.Duration = 1;
			ClassicAssert.AreEqual("5.3.6.1", labeller.Generate(IntegrationResult.CreateInitialIntegrationResult("foo", "c:\\bar", "c:\\baz")));
		}

		[Test]
		public void GenerateIncrementedLabelOnFailureIfIncrementOnFailedIsTrue()
		{
			labeller.IncrementOnFailed = true;
			ClassicAssert.AreEqual("14.36", labeller.Generate(FailedResult("14.35")));
		}
	}
}
