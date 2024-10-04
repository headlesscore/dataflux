using Exortech.NetReflector;
using Xunit;

using ThoughtWorks.CruiseControl.Core.Label;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Label
{
	
	public class DefaultLabellerTest : IntegrationFixture
	{
		private DefaultLabeller labeller;

		// [SetUp]
		public void SetUp()
		{
			labeller = new DefaultLabeller();
		}

		[Fact]
		public void GenerateIncrementedLabel()
		{
			Assert.Equal("36", labeller.Generate(SuccessfulResult("35")));
		}

		[Fact]
		public void GenerateInitialLabel()
		{
			Assert.Equal(DefaultLabeller.INITIAL_LABEL.ToString(), labeller.Generate(InitialIntegrationResult()));
		}

		[Fact]
		public void GenerateInitialLabelWithInitialBuildLabelSet()
		{
			labeller.InitialBuildLabel = 10;
			Assert.Equal("10", labeller.Generate(InitialIntegrationResult()));
		}

		[Fact]
		public void GenerateLabelWhenLastBuildFailed()
		{
			Assert.Equal("23", labeller.Generate(FailedResult("23")));
            Assert.Equal("23", labeller.Generate(FailedResult("23")));
        }

		[Fact]
		public void GenerateInitialPrefixedLabel()
		{
			labeller.LabelPrefix = "Sample";
			Assert.Equal("Sample" + DefaultLabeller.INITIAL_LABEL.ToString(), labeller.Generate(InitialIntegrationResult()));
		}


        [Fact]
        public void GenerateInitialPostfixedLabel()
        {
            labeller.LabelPostfix = "QA_Approved";
            Assert.Equal(DefaultLabeller.INITIAL_LABEL.ToString()+ "QA_Approved", labeller.Generate(InitialIntegrationResult()));
        }


		[Fact]
		public void GeneratePrefixedLabelWhenLastBuildSucceeded()
		{
			labeller.LabelPrefix = "Sample";
			Assert.Equal("Sample36", labeller.Generate(SuccessfulResult("35")));
		}


        [Fact]
        public void GeneratePostfixedLabelWhenLastBuildSucceeded()
        {
            labeller.LabelPostfix = "Sample";
            Assert.Equal("36Sample", labeller.Generate(SuccessfulResult("35")));
        }


        [Fact]
        public void GeneratePreAndPostfixedLabelWhenLastBuildSucceeded()
        {
            labeller.LabelPrefix = "Sample";
            labeller.LabelPostfix = "QA_OK";
            Assert.Equal("Sample36QA_OK", labeller.Generate(SuccessfulResult("35")));
            
        }


        [Fact]
        public void GeneratePreAndPostfixedLabelWhenLastBuildSucceededPreAndPostFixContainingNumericParts()
        {
            labeller.LabelPrefix = "Numeric55Sample";
            labeller.LabelPostfix = "QA11OK";
            Assert.Equal("Numeric55Sample36QA11OK", labeller.Generate(SuccessfulResult("35")));

        }

        
        [Fact]
		public void GeneratePrefixedLabelWhenLastBuildFailed()
		{
			labeller.LabelPrefix = "Sample";
			Assert.Equal("23", labeller.Generate(FailedResult("23")));
		}


        [Fact]
        public void GeneratePostFixedLabelWhenLastBuildFailed()
        {
            labeller.LabelPostfix = "Sample";
            Assert.Equal("23", labeller.Generate(FailedResult("23")));
        }



		[Fact]
		public void GeneratePrefixedLabelWhenLastBuildSucceededAndHasLabelWithPrefix()
		{
			labeller.LabelPrefix = "Sample";
			Assert.Equal("Sample24", labeller.Generate(SuccessfulResult("Sample23")));
		}


        [Fact]
        public void GeneratePostfixedLabelWhenLastBuildSucceededAndHasLabelWithPostfix()
        {
            labeller.LabelPostfix = "Sample";
            Assert.Equal("24Sample", labeller.Generate(SuccessfulResult("23Sample")));
        }


		[Fact]
		public void GeneratePrefixedLabelWhenPrefixAndLastIntegrationLabelDontMatch()
		{
			labeller.LabelPrefix = "Sample";
			Assert.Equal("Sample24", labeller.Generate(SuccessfulResult("SomethingElse23")));
		}


        [Fact]
        public void GeneratePostfixedLabelWhenPostfixAndLastIntegrationLabelDontMatch()
        {
            labeller.LabelPostfix = "Sample";
            Assert.Equal("24Sample", labeller.Generate(SuccessfulResult("23Dummy")));
        }

        
        [Fact]
		public void GeneratePrefixedLabelWhenPrefixIsNumeric()
		{
			labeller.LabelPrefix = "R3SX";
			Assert.Equal("R3SX24", labeller.Generate(SuccessfulResult("R3SX23")));
		}


        [Fact]
        public void GeneratePrefixedLabelWhenPostfixIsNumeric()
        {
            labeller.LabelPostfix = "R3";
            Assert.Equal("24R3", labeller.Generate(SuccessfulResult("23R3")));
        }



        [Fact]
        public void GenerateInitialFormattedLabelWithPrefix()
        {
            labeller.LabelPrefix = "Sample";
            labeller.LabelFormat = "000";
            Assert.Equal("Sample" + DefaultLabeller.INITIAL_LABEL.ToString("000"), labeller.Generate(InitialIntegrationResult()));
        }


        [Fact]
        public void GenerateInitialFormattedLabelWithPostfix()
        {
            labeller.LabelPostfix = "Sample";
            labeller.LabelFormat = "000";
            Assert.Equal(DefaultLabeller.INITIAL_LABEL.ToString("000") + "Sample" , labeller.Generate(InitialIntegrationResult()));
        }

        
        [Fact]
        public void GenerateFormattedLabelWhenLastBuildSucceeded()
        {
            labeller.LabelPrefix = "Sample";
            labeller.LabelFormat = "000";
            Assert.Equal("Sample036", labeller.Generate(SuccessfulResult("35")));
        }

        [Fact]
        public void GenerateFormattedLabelWhenLastBuildFailedWithPrefix()
        {
            labeller.LabelPrefix = "Sample";
            labeller.LabelFormat = "000";
            Assert.Equal("23", labeller.Generate(FailedResult("23")));
        }


        [Fact]
        public void GenerateFormattedLabelWhenLastBuildFailedWithPostFix()
        {
            labeller.LabelPostfix = "Sample";
            labeller.LabelFormat = "000";
            Assert.Equal("23", labeller.Generate(FailedResult("23")));
        }



		[Fact]
		public void IncrementLabelOnFailedBuildIfIncrementConditionIsAlways()
		{
			labeller.IncrementOnFailed = true;
			Assert.Equal("24", labeller.Generate(FailedResult("23")));
		}




		[Fact]
		public void PopulateFromConfiguration()
		{
			string xml = @"<defaultLabeller initialBuildLabel=""35"" prefix=""foo"" incrementOnFailure=""true"" postfix=""bar"" />";
			NetReflector.Read(xml, labeller);
			Assert.Equal(35, labeller.InitialBuildLabel);
			Assert.Equal("foo", labeller.LabelPrefix);
			Assert.Equal(true, labeller.IncrementOnFailed);
            Assert.Equal("bar", labeller.LabelPostfix);
		}

		[Fact]
		public void DefaultValues()
		{
			Assert.Equal(DefaultLabeller.INITIAL_LABEL, labeller.InitialBuildLabel);
			Assert.Equal(string.Empty, labeller.LabelPrefix);
			Assert.Equal(false, labeller.IncrementOnFailed);
            Assert.Equal(string.Empty, labeller.LabelPostfix);
		}


        [Fact]
        public void GeneratePrefixedLabelFromLabelPrefixFileWhenLastBuildSucceeded()
        {
            string lblFile = "thelabelprefix.txt";
            System.IO.File.WriteAllText(lblFile, "1.3.4.");

            labeller.LabelPrefixFile = lblFile;

            Assert.Equal("1.3.4.36", labeller.Generate(SuccessfulResult("1.3.4.35")));
        }


        [Fact]
        public void GeneratePrefixedLabelFromLabelPrefixFileAndLabelPrefixsFileSearchPatternWhenLastBuildSucceeded()
        {
            string lblFile = "thelabelprefix.txt";
            System.IO.File.WriteAllText(lblFile, "1.3.4.");

            labeller.LabelPrefixFile = lblFile;
            labeller.LabelPrefixsFileSearchPattern = @"\d+\.\d+\.\d+\.";

            Assert.Equal("1.3.4.36", labeller.Generate(SuccessfulResult("1.3.4.35")));
        }



        [Fact]
        public void MustThrowExceptionWhenSpecifyingNonExistentFile()
        {
            var ex = Assert.Throws<CruiseControl.Core.Config.ConfigurationException>(() =>
            {
                string lblFile = "DummyFile.txt";

                labeller.LabelPrefixFile = lblFile;

                labeller.Generate(SuccessfulResult("1.3.4.35"));
            });
            Assert.True(ex.Message == "File DummyFile.txt does not exist");
        }


        [Fact]
        public void MustThrowExceptionWhenContentsOfLabelPrefixFileDoesNotMatchLabelPrefixsFileSearchPattern()
        {
            var ex = Assert.Throws<CruiseControl.Core.Config.ConfigurationException>(() =>
            {
                string lblFile = "thelabelprefix.txt";
                System.IO.File.WriteAllText(lblFile, "ho ho ho");

                labeller.LabelPrefixFile = lblFile;
                labeller.LabelPrefixsFileSearchPattern = @"\d+\.\d+\.\d+\.";

                labeller.Generate(SuccessfulResult("1.3.4.35"));
            });
            Assert.True(ex.Message == "No valid prefix data found in file : thelabelprefix.txt");
        }

    }
}
