using System.Xml;
using Exortech.NetReflector;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
	
	public class ModificationWriterTaskTest
	{
		private Mock<IFileSystem> mockIO;
		private ModificationWriterTask task;

		// [SetUp]
		public void SetUp()
		{
			mockIO = new Mock<IFileSystem>();
			task = new ModificationWriterTask(mockIO.Object as IFileSystem);
		}

		// [TearDown]
		public void TearDown()
		{
			mockIO.Verify();
		}

		[Fact]
		public void ShouldWriteOutModificationsToFileAsXml()
		{
			mockIO.Setup(fileSystem => fileSystem.Save(System.IO.Path.Combine(@"artifactDir", "modifications.xml"), It.IsAny<string>())).
				Callback<string, string>((file, content) => {
					XmlUtil.VerifyXmlIsWellFormed(content);
					XmlElement element = XmlUtil.CreateDocumentElement(content);
					Assert.Equal(2, element.ChildNodes.Count);
				}).Verifiable();
            Assert.True(true);
            Assert.True(true);
            IntegrationResult result = IntegrationResultMother.CreateSuccessful();
			result.ArtifactDirectory = "artifactDir";
			result.Modifications = new Modification[]
				{
					ModificationMother.CreateModification("foo.txt", @"c\src"),
					ModificationMother.CreateModification("bar.txt", @"c\src")
				};
			task.Run(result);
		}


        [Fact]
        public void ShouldWriteOutModificationsToFileAsXmlWithBuildTimeAppended()
        {

            IntegrationResult result = IntegrationResultMother.CreateSuccessful();
            string newFileName = string.Format(System.Globalization.CultureInfo.CurrentCulture, System.IO.Path.Combine("artifactDir", "modifications_{0}.xml"),result.StartTime.ToString("yyyyMMddHHmmssfff"));

            mockIO.Setup(fileSystem => fileSystem.Save(newFileName, It.IsAny<string>())).
                Callback<string, string>((file, content) => {
                    XmlUtil.VerifyXmlIsWellFormed(content);
                    XmlElement element = XmlUtil.CreateDocumentElement(content);
                    Assert.Equal(2, element.ChildNodes.Count);
            }).Verifiable();

            result.ArtifactDirectory = "artifactDir";
            result.Modifications = new Modification[]
				{
					ModificationMother.CreateModification("foo.txt", @"c\src"),
					ModificationMother.CreateModification("bar.txt", @"c\src")
				};
            task.AppendTimeStamp = true;
            task.Run(result);
        
        }


		[Fact]
		public void ShouldSaveEmptyFileIfNoModificationsSpecified()
		{
			mockIO.Setup(fileSystem => fileSystem.Save(System.IO.Path.Combine(@"artifactDir", "output.xml"), It.IsAny<string>())).
				Callback<string, string>((file, content) => {
					XmlUtil.VerifyXmlIsWellFormed(content);
					XmlElement element = XmlUtil.CreateDocumentElement(content);
					Assert.Equal(0, element.ChildNodes.Count);
				}).Verifiable();

			IntegrationResult result = IntegrationResultMother.CreateSuccessful();
			result.ArtifactDirectory = "artifactDir";
			task.Filename = "output.xml";
			task.Run(result);
		}


        [Fact]
        public void ShouldSaveEmptyFileIfNoModificationsSpecifiedWithBuildTimeAppended()
        {

            IntegrationResult result = IntegrationResultMother.CreateSuccessful();
            string newFileName = string.Format(System.Globalization.CultureInfo.CurrentCulture, System.IO.Path.Combine("artifactDir", "output_{0}.xml"), result.StartTime.ToString("yyyyMMddHHmmssfff"));
            mockIO.Setup(fileSystem => fileSystem.Save(newFileName, It.IsAny<string>())).
                Callback<string, string>((file, content) => {
                    XmlUtil.VerifyXmlIsWellFormed(content);
                    XmlElement element = XmlUtil.CreateDocumentElement(content);
                    Assert.Equal(0, element.ChildNodes.Count);
                }).Verifiable();

            result.ArtifactDirectory = "artifactDir";
            task.Filename = "output.xml";
            task.AppendTimeStamp = true;


            task.Run(result);
        }


		[Fact]
		public void ShouldRebaseDirectoryRelativeToArtifactDir()
		{
			mockIO.Setup(fileSystem => fileSystem.Save(System.IO.Path.Combine("artifactDir", "relativePath", "modifications.xml"), It.IsAny<string>())).
				Callback<string, string>((file, content) => {
					XmlUtil.VerifyXmlIsWellFormed(content);
					XmlElement element = XmlUtil.CreateDocumentElement(content);
					Assert.Equal(0, element.ChildNodes.Count);
				}).Verifiable();

			IntegrationResult result = IntegrationResultMother.CreateSuccessful();
			result.ArtifactDirectory = "artifactDir";
			task.OutputPath = "relativePath";
			task.Run(result);
		}


        [Fact]
        public void ShouldRebaseDirectoryRelativeToArtifactDirWithBuildTimeAppended()
        {
            IntegrationResult result = IntegrationResultMother.CreateSuccessful();
            string newFileName = string.Format(System.Globalization.CultureInfo.CurrentCulture, System.IO.Path.Combine("artifactDir", "relativePath", "modifications_{0}.xml"), result.StartTime.ToString("yyyyMMddHHmmssfff"));

            mockIO.Setup(fileSystem => fileSystem.Save(newFileName, It.IsAny<string>())).
                Callback<string, string>((file, content) => {
                    XmlUtil.VerifyXmlIsWellFormed(content);
                    XmlElement element = XmlUtil.CreateDocumentElement(content);
                    Assert.Equal(0, element.ChildNodes.Count);
                }).Verifiable();

            result.ArtifactDirectory = "artifactDir";
            task.OutputPath = "relativePath";
            task.AppendTimeStamp = true;
            
            task.Run(result);
        }


		[Fact]
		public void ShouldWriteXmlUsingUTF8Encoding()
		{
			mockIO.Setup(fileSystem => fileSystem.Save(System.IO.Path.Combine("artifactDir", "modifications.xml"), It.IsAny<string>())).
				Callback<string, string>((file, content) => {
					Assert.True(content.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>"));
				}).Verifiable();

			IntegrationResult result = IntegrationResultMother.CreateSuccessful();
			result.ArtifactDirectory = "artifactDir";
			task.Run(result);			
		}



        [Fact]
        public void ShouldWriteXmlUsingUTF8EncodingWithBuildTimeAppended()
        {
            IntegrationResult result = IntegrationResultMother.CreateSuccessful();
            string newFileName = string.Format(System.Globalization.CultureInfo.CurrentCulture, System.IO.Path.Combine("artifactDir", "modifications_{0}.xml"), result.StartTime.ToString("yyyyMMddHHmmssfff"));

            mockIO.Setup(fileSystem => fileSystem.Save(newFileName, It.IsAny<string>())).
                Callback<string, string>((file, content) => {
                    Assert.True(content.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>"));
                }).Verifiable();

            result.ArtifactDirectory = "artifactDir";
            task.AppendTimeStamp = true;


            task.Run(result);
        }



		[Fact]
		public void LoadFromConfigurationXml()
		{
			ModificationWriterTask writer = (ModificationWriterTask) NetReflector.Read(@"<modificationWriter filename=""foo.xml"" path=""c:\bar"" />");
			Assert.Equal("foo.xml", writer.Filename);
			Assert.Equal(@"c:\bar", writer.OutputPath);
            Assert.Equal(false, writer.AppendTimeStamp);

		}


        [Fact]
        public void LoadFromConfigurationXmlWithBuildTimeSetToTrue()
        {
            ModificationWriterTask writer = (ModificationWriterTask)NetReflector.Read(@"<modificationWriter filename=""foo.xml"" path=""c:\bar"" appendTimeStamp=""true""/>");
            Assert.Equal("foo.xml", writer.Filename);
            Assert.Equal(@"c:\bar", writer.OutputPath);
            Assert.Equal(true, writer.AppendTimeStamp);

        }


		[Fact]
		public void LoadFromMinimalConfigurationXml()
		{
			NetReflector.Read(@"<modificationWriter />");
		}




      


	}
}
