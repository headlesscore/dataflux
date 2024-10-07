using System;
using Exortech.NetReflector;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Label;
using ThoughtWorks.CruiseControl.UnitTests.Core;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Label
{
    [TestFixture]
    public class FileLabellerTest : IntegrationFixture
    {
        [Test]
        public void VerifyDefaultValues()
        {
            FileLabeller labeller = new FileLabeller(new TestFileReader("001")); 
            ClassicAssert.AreEqual(string.Empty, labeller.Prefix);
            ClassicAssert.AreEqual(string.Empty, labeller.LabelFilePath);
            ClassicAssert.AreEqual(true, labeller.AllowDuplicateSubsequentLabels);
        }

        [Test]
        public void ShouldPopulateCorrectlyFromXml()
        {
            FileLabeller labeller = new FileLabeller(new TestFileReader("001"));
            string xml = @"<fileLabeller prefix=""foo"" labelFilePath=""label.txt"" allowDuplicateSubsequentLabels=""false"" />";
            NetReflector.Read(xml, labeller);
            ClassicAssert.AreEqual("foo", labeller.Prefix);
            ClassicAssert.AreEqual("foo", labeller.Prefix);
            ClassicAssert.AreEqual("label.txt", labeller.LabelFilePath);
            ClassicAssert.AreEqual(false, labeller.AllowDuplicateSubsequentLabels);
        }

        [Test]
        public void ShouldPopulateCorrectlyFromMinimalXml()
        {
            FileLabeller labeller = new FileLabeller(new TestFileReader("001"));
            string xml = @"<fileLabeller labelFilePath=""label.txt"" />";
            NetReflector.Read(xml, labeller);
            ClassicAssert.AreEqual("", labeller.Prefix);
            ClassicAssert.AreEqual("label.txt", labeller.LabelFilePath);
            ClassicAssert.AreEqual(true, labeller.AllowDuplicateSubsequentLabels);
        }

        [Test]
        public void ShouldFailToPopulateFromConfigurationMissingRequiredFields()
        {
            FileLabeller labeller = new FileLabeller(new TestFileReader("001"));
            string xml = @"<fileLabeller prefix=""foo"" allowDuplicateSubsequentLabels=""false"" />";
            ClassicAssert.That(delegate { NetReflector.Read(xml, labeller); },
                        Throws.TypeOf<NetReflectorException>());
        }

        [Test]
        public void ShouldGenerateLabelWithPrefix()
        {
            FileLabeller labeller = new FileLabeller(new TestFileReader("001"));
            labeller.Prefix = "V0-";
            string label = labeller.Generate(InitialIntegrationResult());
            ClassicAssert.AreEqual("V0-001", label);
        }

        [Test]
        public void ShouldGenerateFirstLabel()
        {
            FileLabeller labeller = new FileLabeller(new TestFileReader("001"));
            labeller.AllowDuplicateSubsequentLabels = false;
            string label = labeller.Generate(InitialIntegrationResult());
            ClassicAssert.AreEqual("001", label);
        }

        [Test]
        public void ShouldGenerateDuplicateLabelWithSuffixForSubsequentDuplicateFileContent()
        {
            FileLabeller labeller = new FileLabeller(new TestFileReader("001"));
            string firstLabel = labeller.Generate(InitialIntegrationResult());
            string secondLabel = labeller.Generate(SuccessfulResult(firstLabel));
            ClassicAssert.AreEqual("001", secondLabel);
        }

        [Test]
        public void ShouldGenerateLabelWithSuffixForSubsequentDuplicateFileContent()
        {
            FileLabeller labeller = new FileLabeller(new TestFileReader("001"));
            labeller.AllowDuplicateSubsequentLabels = false;
            string firstLabel = labeller.Generate(InitialIntegrationResult());
            IntegrationResult integrationResult = SuccessfulResult(firstLabel);
            string secondLabel = labeller.Generate(integrationResult);
            ClassicAssert.AreEqual("001-1", secondLabel);
            IntegrationResult integrationResult2 = SuccessfulResult(secondLabel);
            string thirdLabel = labeller.Generate(integrationResult2);
            ClassicAssert.AreEqual("001-2", thirdLabel);
        }

        [Test]
        public void ShouldIgnoreLeadingAndTrailingWhitespaceInFile()
        {
            FileLabeller labeller = new FileLabeller(new TestFileReader("\r\n\t 001 \t\r\n"));
            string label = labeller.Generate(InitialIntegrationResult());
            ClassicAssert.AreEqual("001", label);
            
        }
        
        [Test]
        public void ShouldReplaceWhitespaceWithBlanks()
        {
            FileLabeller labeller = new FileLabeller(new TestFileReader("001 \r\n\t 002 \t\r\n 003"));
            string label = labeller.Generate(InitialIntegrationResult());
            ClassicAssert.AreEqual("001     002     003", label);

        }

        private class TestFileReader : FileLabeller.FileReader
        {
            private readonly string label;

            public TestFileReader(string label)
            {
                this.label = label;
            }

            public override string ReadLabel(string labelFilePath)
            {
                return label;
            }
        }
    }
}
