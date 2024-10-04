using System;
using Exortech.NetReflector;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Label;
using ThoughtWorks.CruiseControl.UnitTests.Core;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Label
{
    
    public class FileLabellerTest : IntegrationFixture
    {
        [Fact]
        public void VerifyDefaultValues()
        {
            FileLabeller labeller = new FileLabeller(new TestFileReader("001")); 
            Assert.Equal(string.Empty, labeller.Prefix);
            Assert.Equal(string.Empty, labeller.LabelFilePath);
            Assert.Equal(true, labeller.AllowDuplicateSubsequentLabels);
        }

        [Fact]
        public void ShouldPopulateCorrectlyFromXml()
        {
            FileLabeller labeller = new FileLabeller(new TestFileReader("001"));
            string xml = @"<fileLabeller prefix=""foo"" labelFilePath=""label.txt"" allowDuplicateSubsequentLabels=""false"" />";
            NetReflector.Read(xml, labeller);
            Assert.Equal("foo", labeller.Prefix);
            Assert.Equal("foo", labeller.Prefix);
            Assert.Equal("label.txt", labeller.LabelFilePath);
            Assert.Equal(false, labeller.AllowDuplicateSubsequentLabels);
        }

        [Fact]
        public void ShouldPopulateCorrectlyFromMinimalXml()
        {
            FileLabeller labeller = new FileLabeller(new TestFileReader("001"));
            string xml = @"<fileLabeller labelFilePath=""label.txt"" />";
            NetReflector.Read(xml, labeller);
            Assert.Equal("", labeller.Prefix);
            Assert.Equal("label.txt", labeller.LabelFilePath);
            Assert.Equal(true, labeller.AllowDuplicateSubsequentLabels);
        }

        [Fact]
        public void ShouldFailToPopulateFromConfigurationMissingRequiredFields()
        {
            FileLabeller labeller = new FileLabeller(new TestFileReader("001"));
            string xml = @"<fileLabeller prefix=""foo"" allowDuplicateSubsequentLabels=""false"" />";
            Assert.True(delegate { NetReflector.Read(xml, labeller); },
                        Throws.TypeOf<NetReflectorException>());
        }

        [Fact]
        public void ShouldGenerateLabelWithPrefix()
        {
            FileLabeller labeller = new FileLabeller(new TestFileReader("001"));
            labeller.Prefix = "V0-";
            string label = labeller.Generate(InitialIntegrationResult());
            Assert.Equal("V0-001", label);
        }

        [Fact]
        public void ShouldGenerateFirstLabel()
        {
            FileLabeller labeller = new FileLabeller(new TestFileReader("001"));
            labeller.AllowDuplicateSubsequentLabels = false;
            string label = labeller.Generate(InitialIntegrationResult());
            Assert.Equal("001", label);
        }

        [Fact]
        public void ShouldGenerateDuplicateLabelWithSuffixForSubsequentDuplicateFileContent()
        {
            FileLabeller labeller = new FileLabeller(new TestFileReader("001"));
            string firstLabel = labeller.Generate(InitialIntegrationResult());
            string secondLabel = labeller.Generate(SuccessfulResult(firstLabel));
            Assert.Equal("001", secondLabel);
        }

        [Fact]
        public void ShouldGenerateLabelWithSuffixForSubsequentDuplicateFileContent()
        {
            FileLabeller labeller = new FileLabeller(new TestFileReader("001"));
            labeller.AllowDuplicateSubsequentLabels = false;
            string firstLabel = labeller.Generate(InitialIntegrationResult());
            IntegrationResult integrationResult = SuccessfulResult(firstLabel);
            string secondLabel = labeller.Generate(integrationResult);
            Assert.Equal("001-1", secondLabel);
            IntegrationResult integrationResult2 = SuccessfulResult(secondLabel);
            string thirdLabel = labeller.Generate(integrationResult2);
            Assert.Equal("001-2", thirdLabel);
        }

        [Fact]
        public void ShouldIgnoreLeadingAndTrailingWhitespaceInFile()
        {
            FileLabeller labeller = new FileLabeller(new TestFileReader("\r\n\t 001 \t\r\n"));
            string label = labeller.Generate(InitialIntegrationResult());
            Assert.Equal("001", label);
            
        }
        
        [Fact]
        public void ShouldReplaceWhitespaceWithBlanks()
        {
            FileLabeller labeller = new FileLabeller(new TestFileReader("001 \r\n\t 002 \t\r\n 003"));
            string label = labeller.Generate(InitialIntegrationResult());
            Assert.Equal("001     002     003", label);

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
