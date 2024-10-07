using System;
using Exortech.NetReflector;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Label;
using ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Label
{
    /// <remarks>
    /// This code is based on code\label\DefaultLabeller.cs.
    /// </remarks> 
    [TestFixture]
    public class LastChangeLabellerTest : IntegrationFixture
    {
        private LastChangeLabeller labeller;

        [SetUp]
        public void SetUp()
        {
            labeller = new LastChangeLabeller();
        }

        [Test]
        public void GenerateLabel()
        {
            IntegrationResult result = CreateSucessfullIntegrationResult();
            AddModifications(result);
            ClassicAssert.AreEqual("30", labeller.Generate(result));
        }

        [Test]
        public void GenerateLabelFromNoMods()
        {
            IntegrationResult result = CreateSucessfullIntegrationResult();
            ClassicAssert.AreEqual("unknown", labeller.Generate(result));
        }

        [Test]
        public void GeneratePrefixedLabel()
        {
            IntegrationResult result = CreateSucessfullIntegrationResult();
            AddModifications(result);
            labeller.LabelPrefix = "Sample";
            ClassicAssert.AreEqual("Sample30", labeller.Generate(result));
        }

        [Test]
        public void GeneratePrefixedLabelWhenPrefixAndLastIntegrationLabelDontMatch()
        {
            IntegrationResult result = CreateSucessfullIntegrationResult();
            AddModifications(result);
            labeller.LabelPrefix = "Sample";
            result.LastSuccessfulIntegrationLabel = "SomethingElse23";
            ClassicAssert.AreEqual("Sample30", labeller.Generate(result));
            ClassicAssert.AreEqual("Sample30", labeller.Generate(result));
        }

        [Test]
        public void GeneratePrefixedLabelWhenPrefixIsNumeric()
        {
            IntegrationResult result = CreateSucessfullIntegrationResult();
            AddModifications(result);
            labeller.LabelPrefix = "R3SX";
            result.LastSuccessfulIntegrationLabel = "R3SX23";
            ClassicAssert.AreEqual("R3SX30", labeller.Generate(result));
        }


        [Test]
        public void GeneratePrefixedLabelWhenPrefixIsVersionLikePrefix()
        {
            IntegrationResult result = CreateSucessfullIntegrationResult();
            AddModifications(result);
            labeller.LabelPrefix = "1.2.";
            ClassicAssert.AreEqual("1.2.30", labeller.Generate(result));
        }

        [Test]
        public void GeneratePrefixedLabelWhenPrefixIsVersionLikePrefix2()
        {
            IntegrationResult result = CreateSucessfullIntegrationResult();
            AddModifications(result);
            labeller.LabelPrefix = "2.2.0.";
            ClassicAssert.AreEqual("2.2.0.30", labeller.Generate(result));
        }



        [Test]
        public void PopulateFromConfiguration()
        {
            string xml = "<LastChangeLabeller prefix=\"foo\" allowDuplicateSubsequentLabels=\"false\" />";
            NetReflector.Read(xml, labeller);
            ClassicAssert.AreEqual("foo", labeller.LabelPrefix);
            ClassicAssert.AreEqual(false, labeller.AllowDuplicateSubsequentLabels);
        }

        [Test]
        public void PopulateFromMinimalConfiguration()
        {
            string xml = "<LastChangeLabeller/>";
            NetReflector.Read(xml, labeller);
            ClassicAssert.AreEqual(string.Empty, labeller.LabelPrefix);
            ClassicAssert.AreEqual(true, labeller.AllowDuplicateSubsequentLabels);
        }

        [Test]
        public void VerifyDefaultValues()
        {
            ClassicAssert.AreEqual(string.Empty, labeller.LabelPrefix);
            ClassicAssert.AreEqual(true, labeller.AllowDuplicateSubsequentLabels);
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


        [Test]
        public void GenerateLabelFromNoModsIterative()
        {
            labeller.LabelPrefix = "DoesNotMatterForNoMods";
            ClassicAssert.AreEqual("unknown", labeller.Generate(SuccessfulResult("unknown")));
            ClassicAssert.AreEqual("30", labeller.Generate(SuccessfulResult("30")));
            ClassicAssert.AreEqual("30.1", labeller.Generate(SuccessfulResult("30.1")));
            ClassicAssert.AreEqual("Sample.30", labeller.Generate(SuccessfulResult("Sample.30")));
            ClassicAssert.AreEqual("Sample.30.1", labeller.Generate(SuccessfulResult("Sample.30.1")));
        }

        [Test]
        public void GenerateLabelFromNoModsIterativeWhenDuplicatesAreNotAllowed()
        {
            labeller.LabelPrefix = "DoesNotMatterForNoMods";
            labeller.AllowDuplicateSubsequentLabels = false;
            ClassicAssert.AreEqual("unknown.1", labeller.Generate(SuccessfulResult("unknown")));
            ClassicAssert.AreEqual("30.1", labeller.Generate(SuccessfulResult("30")));
            ClassicAssert.AreEqual("30.2", labeller.Generate(SuccessfulResult("30.1")));
            ClassicAssert.AreEqual("Sample.30.1", labeller.Generate(SuccessfulResult("Sample.30")));
            ClassicAssert.AreEqual("Sample.30.2", labeller.Generate(SuccessfulResult("Sample.30.1")));
        }

        [Test]
        public void GeneratePrefixedLabelWhenDuplicatesAreNotAllowed()
        {
            IntegrationResult result = CreateSucessfullIntegrationResult();
            AddModifications(result);
            labeller.LabelPrefix = "Sample";
            labeller.AllowDuplicateSubsequentLabels = false;
            ClassicAssert.AreEqual("Sample30.1", labeller.Generate(result));
        }
    }
}
