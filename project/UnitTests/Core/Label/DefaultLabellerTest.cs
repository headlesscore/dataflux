using Exortech.NetReflector;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Label;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Label
{
	[TestFixture]
	public class DefaultLabellerTest : IntegrationFixture
	{
		private DefaultLabeller labeller;

		[SetUp]
		public void SetUp()
		{
			labeller = new DefaultLabeller();
		}

		[Test]
		public void GenerateIncrementedLabel()
		{
			ClassicAssert.AreEqual("36", labeller.Generate(SuccessfulResult("35")));
		}

		[Test]
		public void GenerateInitialLabel()
		{
			ClassicAssert.AreEqual(DefaultLabeller.INITIAL_LABEL.ToString(), labeller.Generate(InitialIntegrationResult()));
		}

		[Test]
		public void GenerateInitialLabelWithInitialBuildLabelSet()
		{
			labeller.InitialBuildLabel = 10;
			ClassicAssert.AreEqual("10", labeller.Generate(InitialIntegrationResult()));
		}

		[Test]
		public void GenerateLabelWhenLastBuildFailed()
		{
			ClassicAssert.AreEqual("23", labeller.Generate(FailedResult("23")));
            ClassicAssert.AreEqual("23", labeller.Generate(FailedResult("23")));
        }

		[Test]
		public void GenerateInitialPrefixedLabel()
		{
			labeller.LabelPrefix = "Sample";
			ClassicAssert.AreEqual("Sample" + DefaultLabeller.INITIAL_LABEL.ToString(), labeller.Generate(InitialIntegrationResult()));
		}


        [Test]
        public void GenerateInitialPostfixedLabel()
        {
            labeller.LabelPostfix = "QA_Approved";
            ClassicAssert.AreEqual(DefaultLabeller.INITIAL_LABEL.ToString()+ "QA_Approved", labeller.Generate(InitialIntegrationResult()));
        }


		[Test]
		public void GeneratePrefixedLabelWhenLastBuildSucceeded()
		{
			labeller.LabelPrefix = "Sample";
			ClassicAssert.AreEqual("Sample36", labeller.Generate(SuccessfulResult("35")));
		}


        [Test]
        public void GeneratePostfixedLabelWhenLastBuildSucceeded()
        {
            labeller.LabelPostfix = "Sample";
            ClassicAssert.AreEqual("36Sample", labeller.Generate(SuccessfulResult("35")));
        }


        [Test]
        public void GeneratePreAndPostfixedLabelWhenLastBuildSucceeded()
        {
            labeller.LabelPrefix = "Sample";
            labeller.LabelPostfix = "QA_OK";
            ClassicAssert.AreEqual("Sample36QA_OK", labeller.Generate(SuccessfulResult("35")));
            
        }


        [Test]
        public void GeneratePreAndPostfixedLabelWhenLastBuildSucceededPreAndPostFixContainingNumericParts()
        {
            labeller.LabelPrefix = "Numeric55Sample";
            labeller.LabelPostfix = "QA11OK";
            ClassicAssert.AreEqual("Numeric55Sample36QA11OK", labeller.Generate(SuccessfulResult("35")));

        }

        
        [Test]
		public void GeneratePrefixedLabelWhenLastBuildFailed()
		{
			labeller.LabelPrefix = "Sample";
			ClassicAssert.AreEqual("23", labeller.Generate(FailedResult("23")));
		}


        [Test]
        public void GeneratePostFixedLabelWhenLastBuildFailed()
        {
            labeller.LabelPostfix = "Sample";
            ClassicAssert.AreEqual("23", labeller.Generate(FailedResult("23")));
        }



		[Test]
		public void GeneratePrefixedLabelWhenLastBuildSucceededAndHasLabelWithPrefix()
		{
			labeller.LabelPrefix = "Sample";
			ClassicAssert.AreEqual("Sample24", labeller.Generate(SuccessfulResult("Sample23")));
		}


        [Test]
        public void GeneratePostfixedLabelWhenLastBuildSucceededAndHasLabelWithPostfix()
        {
            labeller.LabelPostfix = "Sample";
            ClassicAssert.AreEqual("24Sample", labeller.Generate(SuccessfulResult("23Sample")));
        }


		[Test]
		public void GeneratePrefixedLabelWhenPrefixAndLastIntegrationLabelDontMatch()
		{
			labeller.LabelPrefix = "Sample";
			ClassicAssert.AreEqual("Sample24", labeller.Generate(SuccessfulResult("SomethingElse23")));
		}


        [Test]
        public void GeneratePostfixedLabelWhenPostfixAndLastIntegrationLabelDontMatch()
        {
            labeller.LabelPostfix = "Sample";
            ClassicAssert.AreEqual("24Sample", labeller.Generate(SuccessfulResult("23Dummy")));
        }

        
        [Test]
		public void GeneratePrefixedLabelWhenPrefixIsNumeric()
		{
			labeller.LabelPrefix = "R3SX";
			ClassicAssert.AreEqual("R3SX24", labeller.Generate(SuccessfulResult("R3SX23")));
		}


        [Test]
        public void GeneratePrefixedLabelWhenPostfixIsNumeric()
        {
            labeller.LabelPostfix = "R3";
            ClassicAssert.AreEqual("24R3", labeller.Generate(SuccessfulResult("23R3")));
        }



        [Test]
        public void GenerateInitialFormattedLabelWithPrefix()
        {
            labeller.LabelPrefix = "Sample";
            labeller.LabelFormat = "000";
            ClassicAssert.AreEqual("Sample" + DefaultLabeller.INITIAL_LABEL.ToString("000"), labeller.Generate(InitialIntegrationResult()));
        }


        [Test]
        public void GenerateInitialFormattedLabelWithPostfix()
        {
            labeller.LabelPostfix = "Sample";
            labeller.LabelFormat = "000";
            ClassicAssert.AreEqual(DefaultLabeller.INITIAL_LABEL.ToString("000") + "Sample" , labeller.Generate(InitialIntegrationResult()));
        }

        
        [Test]
        public void GenerateFormattedLabelWhenLastBuildSucceeded()
        {
            labeller.LabelPrefix = "Sample";
            labeller.LabelFormat = "000";
            ClassicAssert.AreEqual("Sample036", labeller.Generate(SuccessfulResult("35")));
        }

        [Test]
        public void GenerateFormattedLabelWhenLastBuildFailedWithPrefix()
        {
            labeller.LabelPrefix = "Sample";
            labeller.LabelFormat = "000";
            ClassicAssert.AreEqual("23", labeller.Generate(FailedResult("23")));
        }


        [Test]
        public void GenerateFormattedLabelWhenLastBuildFailedWithPostFix()
        {
            labeller.LabelPostfix = "Sample";
            labeller.LabelFormat = "000";
            ClassicAssert.AreEqual("23", labeller.Generate(FailedResult("23")));
        }



		[Test]
		public void IncrementLabelOnFailedBuildIfIncrementConditionIsAlways()
		{
			labeller.IncrementOnFailed = true;
			ClassicAssert.AreEqual("24", labeller.Generate(FailedResult("23")));
		}




		[Test]
		public void PopulateFromConfiguration()
		{
			string xml = @"<defaultLabeller initialBuildLabel=""35"" prefix=""foo"" incrementOnFailure=""true"" postfix=""bar"" />";
			NetReflector.Read(xml, labeller);
			ClassicAssert.AreEqual(35, labeller.InitialBuildLabel);
			ClassicAssert.AreEqual("foo", labeller.LabelPrefix);
			ClassicAssert.AreEqual(true, labeller.IncrementOnFailed);
            ClassicAssert.AreEqual("bar", labeller.LabelPostfix);
		}

		[Test]
		public void DefaultValues()
		{
			ClassicAssert.AreEqual(DefaultLabeller.INITIAL_LABEL, labeller.InitialBuildLabel);
			ClassicAssert.AreEqual(string.Empty, labeller.LabelPrefix);
			ClassicAssert.AreEqual(false, labeller.IncrementOnFailed);
            ClassicAssert.AreEqual(string.Empty, labeller.LabelPostfix);
		}


        [Test]
        public void GeneratePrefixedLabelFromLabelPrefixFileWhenLastBuildSucceeded()
        {
            string lblFile = "thelabelprefix.txt";
            System.IO.File.WriteAllText(lblFile, "1.3.4.");

            labeller.LabelPrefixFile = lblFile;

            ClassicAssert.AreEqual("1.3.4.36", labeller.Generate(SuccessfulResult("1.3.4.35")));
        }


        [Test]
        public void GeneratePrefixedLabelFromLabelPrefixFileAndLabelPrefixsFileSearchPatternWhenLastBuildSucceeded()
        {
            string lblFile = "thelabelprefix.txt";
            System.IO.File.WriteAllText(lblFile, "1.3.4.");

            labeller.LabelPrefixFile = lblFile;
            labeller.LabelPrefixsFileSearchPattern = @"\d+\.\d+\.\d+\.";

            ClassicAssert.AreEqual("1.3.4.36", labeller.Generate(SuccessfulResult("1.3.4.35")));
        }



        [Test]
        public void MustThrowExceptionWhenSpecifyingNonExistentFile()
        {
            var ex = ClassicAssert.Throws<CruiseControl.Core.Config.ConfigurationException>(() =>
            {
                string lblFile = "DummyFile.txt";

                labeller.LabelPrefixFile = lblFile;

                labeller.Generate(SuccessfulResult("1.3.4.35"));
            });
            ClassicAssert.That(ex.Message, Is.EqualTo("File DummyFile.txt does not exist"));
        }


        [Test]
        public void MustThrowExceptionWhenContentsOfLabelPrefixFileDoesNotMatchLabelPrefixsFileSearchPattern()
        {
            var ex = ClassicAssert.Throws<CruiseControl.Core.Config.ConfigurationException>(() =>
            {
                string lblFile = "thelabelprefix.txt";
                System.IO.File.WriteAllText(lblFile, "ho ho ho");

                labeller.LabelPrefixFile = lblFile;
                labeller.LabelPrefixsFileSearchPattern = @"\d+\.\d+\.\d+\.";

                labeller.Generate(SuccessfulResult("1.3.4.35"));
            });
            ClassicAssert.That(ex.Message, Is.EqualTo("No valid prefix data found in file : thelabelprefix.txt"));
        }

    }
}
