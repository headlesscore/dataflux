using System;
using Exortech.NetReflector;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Label;
using ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Label
{
    /// <remarks>
    /// This code is based on code\label\DefaultLabeller.cs.
    /// </remarks> 
    
    public class LastChangeLabellerTest : IntegrationFixture
    {
        private LastChangeLabeller labeller;

        // [SetUp]
        public void SetUp()
        {
            labeller = new LastChangeLabeller();
        }

        [Fact]
        public void GenerateLabel()
        {
            IntegrationResult result = CreateSucessfullIntegrationResult();
            AddModifications(result);
            Assert.Equal("30", labeller.Generate(result));
        }

        [Fact]
        public void GenerateLabelFromNoMods()
        {
            IntegrationResult result = CreateSucessfullIntegrationResult();
            Assert.Equal("unknown", labeller.Generate(result));
        }

        [Fact]
        public void GeneratePrefixedLabel()
        {
            IntegrationResult result = CreateSucessfullIntegrationResult();
            AddModifications(result);
            labeller.LabelPrefix = "Sample";
            Assert.Equal("Sample30", labeller.Generate(result));
        }

        [Fact]
        public void GeneratePrefixedLabelWhenPrefixAndLastIntegrationLabelDontMatch()
        {
            IntegrationResult result = CreateSucessfullIntegrationResult();
            AddModifications(result);
            labeller.LabelPrefix = "Sample";
            result.LastSuccessfulIntegrationLabel = "SomethingElse23";
            Assert.Equal("Sample30", labeller.Generate(result));
            Assert.Equal("Sample30", labeller.Generate(result));
        }

        [Fact]
        public void GeneratePrefixedLabelWhenPrefixIsNumeric()
        {
            IntegrationResult result = CreateSucessfullIntegrationResult();
            AddModifications(result);
            labeller.LabelPrefix = "R3SX";
            result.LastSuccessfulIntegrationLabel = "R3SX23";
            Assert.Equal("R3SX30", labeller.Generate(result));
        }


        [Fact]
        public void GeneratePrefixedLabelWhenPrefixIsVersionLikePrefix()
        {
            IntegrationResult result = CreateSucessfullIntegrationResult();
            AddModifications(result);
            labeller.LabelPrefix = "1.2.";
            Assert.Equal("1.2.30", labeller.Generate(result));
        }

        [Fact]
        public void GeneratePrefixedLabelWhenPrefixIsVersionLikePrefix2()
        {
            IntegrationResult result = CreateSucessfullIntegrationResult();
            AddModifications(result);
            labeller.LabelPrefix = "2.2.0.";
            Assert.Equal("2.2.0.30", labeller.Generate(result));
        }



        [Fact]
        public void PopulateFromConfiguration()
        {
            string xml = "<LastChangeLabeller prefix=\"foo\" allowDuplicateSubsequentLabels=\"false\" />";
            NetReflector.Read(xml, labeller);
            Assert.Equal("foo", labeller.LabelPrefix);
            Assert.Equal(false, labeller.AllowDuplicateSubsequentLabels);
        }

        [Fact]
        public void PopulateFromMinimalConfiguration()
        {
            string xml = "<LastChangeLabeller/>";
            NetReflector.Read(xml, labeller);
            Assert.Equal(string.Empty, labeller.LabelPrefix);
            Assert.Equal(true, labeller.AllowDuplicateSubsequentLabels);
        }

        [Fact]
        public void VerifyDefaultValues()
        {
            Assert.Equal(string.Empty, labeller.LabelPrefix);
            Assert.Equal(true, labeller.AllowDuplicateSubsequentLabels);
        }

        private static IntegrationResult CreateSucessfullIntegrationResult()
        {
            IntegrationResult result = IntegrationResultMother.CreateSuccessful();
            return result;
        }

        private static void AddModifications(IntegrationResult result)
        {
            result.Modifications = new Modification[3];
            result.Modifications[0] = ModificationMother.CreateModification("fileName", "folderName",
                                                                            new DateTime(2009, 1, 1), "userName",
                                                                            "comment", "10", "email@address",
                                                                            "http://url");
            result.Modifications[1] = ModificationMother.CreateModification("fileName", "folderName",
                                                                            new DateTime(2009, 1, 3), "userName",
                                                                            "comment", "30", "email@address",
                                                                            "http://url");
            result.Modifications[2] = ModificationMother.CreateModification("fileName", "folderName",
                                                                            new DateTime(2009, 1, 2), "userName",
                                                                            "comment", "20", "email@address",
                                                                            "http://url");
        }


        [Fact]
        public void GenerateLabelFromNoModsIterative()
        {
            labeller.LabelPrefix = "DoesNotMatterForNoMods";
            Assert.Equal("unknown", labeller.Generate(SuccessfulResult("unknown")));
            Assert.Equal("30", labeller.Generate(SuccessfulResult("30")));
            Assert.Equal("30.1", labeller.Generate(SuccessfulResult("30.1")));
            Assert.Equal("Sample.30", labeller.Generate(SuccessfulResult("Sample.30")));
            Assert.Equal("Sample.30.1", labeller.Generate(SuccessfulResult("Sample.30.1")));
        }

        [Fact]
        public void GenerateLabelFromNoModsIterativeWhenDuplicatesAreNotAllowed()
        {
            labeller.LabelPrefix = "DoesNotMatterForNoMods";
            labeller.AllowDuplicateSubsequentLabels = false;
            Assert.Equal("unknown.1", labeller.Generate(SuccessfulResult("unknown")));
            Assert.Equal("30.1", labeller.Generate(SuccessfulResult("30")));
            Assert.Equal("30.2", labeller.Generate(SuccessfulResult("30.1")));
            Assert.Equal("Sample.30.1", labeller.Generate(SuccessfulResult("Sample.30")));
            Assert.Equal("Sample.30.2", labeller.Generate(SuccessfulResult("Sample.30.1")));
        }

        [Fact]
        public void GeneratePrefixedLabelWhenDuplicatesAreNotAllowed()
        {
            IntegrationResult result = CreateSucessfullIntegrationResult();
            AddModifications(result);
            labeller.LabelPrefix = "Sample";
            labeller.AllowDuplicateSubsequentLabels = false;
            Assert.Equal("Sample30.1", labeller.Generate(result));
        }
    }
}
