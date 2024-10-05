using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using ThoughtWorks.CruiseControl.Core.Publishers;
using ThoughtWorks.CruiseControl.Remote;
using ThoughtWorks.CruiseControl.Core;
using System.Xml;
using System.IO;


namespace ThoughtWorks.CruiseControl.UnitTests.Core.Publishers
{
    
    public class ManifestImporterTests
    {
        #region Test methods
        #region CheckAllProperties()
        /// <summary>
        /// Make sure all the properties return the same value that they were set with.
        /// </summary>
        [Fact]
        public void CheckAllProperties()
        {
            ManifestImporter generator = new ManifestImporter();
            generator.FileName = "File name";
            Assert.Equal("File name", generator.FileName);
            Assert.Equal("File name", generator.FileName);
        }
        #endregion

        #region ImportAbsoluteBasedManifest()
        /// <summary>
        /// Import a manifest from an absolute-based filename.
        /// </summary>
        [Fact]
        public void ImportAbsoluteBasedManifest()
        {
            string sourceFile = Path.Combine(Path.GetTempPath(), "ImportManifest.xml");
            string expectedManifest = "<manifest>" +
                    "From a file" +
                "</manifest>";
            if (File.Exists(sourceFile)) File.Delete(sourceFile);
            File.WriteAllText(sourceFile, expectedManifest);

            ManifestImporter generator = new ManifestImporter();
            generator.FileName = sourceFile;
            IntegrationRequest request = new IntegrationRequest(BuildCondition.ForceBuild, "Somewhere", null);
            IntegrationSummary summary = new IntegrationSummary(IntegrationStatus.Success, "A Label", "Another Label", new DateTime(2009, 1, 1));
            IntegrationResult result = new IntegrationResult("Test project", "Working directory", "Artifact directory", request, summary);
            List<string> files = new List<string>();
            XmlDocument manifest = generator.Generate(result, files.ToArray());

            Assert.NotNull(manifest);
            string actualManifest = manifest.OuterXml;
            Assert.Equal(expectedManifest, actualManifest);
        }
        #endregion

        #region ImportRelativeBasedManifest()
        /// <summary>
        /// Import a manifest from an relative-based filename.
        /// </summary>
        [Fact]
        public void ImportRelativeBasedManifest()
        {
            string sourceFile = Path.Combine(Path.GetTempPath(), "ImportManifest.xml");
            string expectedManifest = "<manifest>" +
                    "From a file" +
                "</manifest>";
            if (File.Exists(sourceFile)) File.Delete(sourceFile);
            File.WriteAllText(sourceFile, expectedManifest);

            ManifestImporter generator = new ManifestImporter();
            generator.FileName = "ImportManifest.xml";
            IntegrationRequest request = new IntegrationRequest(BuildCondition.ForceBuild, "Somewhere", null);
            IntegrationSummary summary = new IntegrationSummary(IntegrationStatus.Success, "A Label", "Another Label", new DateTime(2009, 1, 1));
            IntegrationResult result = new IntegrationResult("Test project", "Working directory", "Artifact directory", request, summary);
            List<string> files = new List<string>();
            result.WorkingDirectory = Path.GetTempPath();
            XmlDocument manifest = generator.Generate(result, files.ToArray());

            Assert.NotNull(manifest);
            string actualManifest = manifest.OuterXml;
            Assert.Equal(expectedManifest, actualManifest);
        }
        #endregion

        #region ImportWithoutAFilename()
        /// <summary>
        /// Import a manifest without setting the filename.
        /// </summary>
        [Fact]
        public void ImportWithoutAFilename()
        {
            ManifestImporter generator = new ManifestImporter();
            Assert.Equal("FileName", Assert.Throws<ArgumentOutOfRangeException>(delegate { generator.Generate(null, null); }).ParamName);
        }
        #endregion
        #endregion
    }
}
