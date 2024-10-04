using System.IO;
using Exortech.NetReflector;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Publishers;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.UnitTests.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Publishers
{
	
	public class BuildPublisherTest : CustomAssertion
	{
		private SystemPath srcRoot;
		private SystemPath pubRoot;
		private BuildPublisher publisher;
		private IntegrationResult result;
		private SystemPath labelPubDir;
		private const string fileName = "foo.txt";
		private const string fileContents = "I'm the contents of foo.txt";

		// [SetUp]
		public void SetUp()
		{
			srcRoot = SystemPath.UniqueTempPath();
			pubRoot = SystemPath.UniqueTempPath();

			publisher = new BuildPublisher();
			publisher.PublishDir = pubRoot.ToString();
			publisher.SourceDir = srcRoot.ToString();
			result = IntegrationResultMother.CreateSuccessful("99");
			labelPubDir = pubRoot.Combine("99");
		}

		[Fact]
		public void CopyFiles()
		{
			SystemPath subRoot = srcRoot.CreateSubDirectory("SubDir");
			SystemPath subSubRoot = subRoot.CreateSubDirectory("SubSubDir");
			srcRoot.CreateTextFile(fileName, fileContents);
			subRoot.CreateTextFile(fileName, fileContents);
			subSubRoot.CreateTextFile(fileName, fileContents);

			publisher.Run(result);

			Assert.True(labelPubDir.Combine(fileName).Exists(), "File not found in build number directory");
            Assert.True(labelPubDir.Combine(fileName).Exists(), "File not found in build number directory");
            SystemPath subPubDir = labelPubDir.Combine("SubDir");
			Assert.True(subPubDir.Combine(fileName).Exists(), "File not found in sub directory");
			Assert.True(subPubDir.Combine("SubSubDir").Combine(fileName).Exists(), "File not found in sub sub directory");
		}


        [Fact]
        public void DeleteFilesAtPublishFolderWhenCleanPublishDirPriorToCopyIsTrue()
        {            
            SystemPath rootFile = srcRoot.CreateDirectory().CreateTextFile(fileName, fileContents);
                                    
            publisher.UseLabelSubDirectory = false;
            publisher.Run(result);

            Assert.True(pubRoot.Combine(fileName).Exists(), "File not found in publish folder");

            // simulate deletion of a file
            rootFile.DeleteFile();
            Assert.False(srcRoot.Combine(fileName).Exists(), "File found in root folder");


            // publish again
            publisher.CleanPublishDirPriorToCopy = true;
            publisher.Run(result);


            Assert.False(pubRoot.Combine(fileName).Exists(), "File found in publish folder");

        }


        [Fact]
        public void ShouldNotCopyFilesIfBuildBrokenAndAlwaysCopyIsSetToFalse()
        {
            SystemPath subRoot = srcRoot.CreateSubDirectory("SubDir");
            SystemPath subSubRoot = subRoot.CreateSubDirectory("SubSubDir");
            srcRoot.CreateTextFile(fileName, fileContents);
            subRoot.CreateTextFile(fileName, fileContents);
            subSubRoot.CreateTextFile(fileName, fileContents);

            result = IntegrationResultMother.CreateFailed("99");

            publisher.Run(result);

            Assert.False(labelPubDir.Combine(fileName).Exists(), "File found in build number directory");
            SystemPath subPubDir = labelPubDir.Combine("SubDir");
            Assert.False(subPubDir.Combine(fileName).Exists(), "File found in sub directory");
            Assert.False(subPubDir.Combine("SubSubDir").Combine(fileName).Exists(), "File found in sub sub directory");
        }


        [Fact]
        public void ShouldCopyFilesIfBuildBrokenAndAlwaysCopyIsSetToTrue()
        {
            SystemPath subRoot = srcRoot.CreateSubDirectory("SubDir");
            SystemPath subSubRoot = subRoot.CreateSubDirectory("SubSubDir");
            srcRoot.CreateTextFile(fileName, fileContents);
            subRoot.CreateTextFile(fileName, fileContents);
            subSubRoot.CreateTextFile(fileName, fileContents);

            result = IntegrationResultMother.CreateFailed("99");

            publisher.AlwaysPublish = true;
            publisher.Run(result);

            Assert.True(labelPubDir.Combine(fileName).Exists(), "File not found in build number directory");
            SystemPath subPubDir = labelPubDir.Combine("SubDir");
            Assert.True(subPubDir.Combine(fileName).Exists(), "File not found in sub directory");
            Assert.True(subPubDir.Combine("SubSubDir").Combine(fileName).Exists(), "File not found in sub sub directory");
        }



		[Fact]
		public void SourceRootShouldBeRelativeToIntegrationWorkingDirectory()
		{
			srcRoot.CreateSubDirectory("foo").CreateTextFile(fileName, fileContents);

			result.WorkingDirectory = srcRoot.ToString();

			publisher.SourceDir = "foo";
			publisher.Run(result);

			Assert.True(labelPubDir.Combine(fileName).Exists(), "File not found in build number directory");
		}

		[Fact]
		public void PublishDirShouldBeRelativeToIntegrationArtifactDirectory()
		{
			srcRoot.CreateSubDirectory("foo").CreateTextFile(fileName, fileContents);
			result.ArtifactDirectory = pubRoot.ToString();
			
			publisher.PublishDir = "bar";
			publisher.Run(result);

			labelPubDir = pubRoot.Combine("bar").Combine("99").Combine("foo");
			Assert.True(labelPubDir.Combine(fileName).Exists(), "File not found in build number directory");
		}

		[Fact]
		public void DoNotUseLabelSubdirectoryAndCreatePublishDirIfItDoesntExist()
		{
			srcRoot.CreateDirectory().CreateTextFile(fileName, fileContents);
			publisher.UseLabelSubDirectory = false;
			publisher.Run(result);

			Assert.True(pubRoot.Combine(fileName).Exists(), "File not found in pubRoot directory");
		}

		[Fact]
		public void OverwriteReadOnlyFileAtDestination()
		{
			srcRoot.CreateDirectory().CreateTextFile(fileName, fileContents);
			pubRoot.CreateDirectory();
			FileInfo readOnlyDestFile = new FileInfo(pubRoot.CreateEmptyFile(fileName).ToString());
			readOnlyDestFile.Attributes = FileAttributes.ReadOnly;
			publisher.UseLabelSubDirectory = false;
			publisher.Run(result);			
		}

		[Fact]
		public void LoadFromXml()
		{
            string xml = @"<buildpublisher useLabelSubDirectory=""false"" 
    alwaysPublish=""true"" 
    cleanPublishDirPriorToCopy=""true""
    cleanUpMethod=""KeepLastXBuilds""
    cleanUpValue=""10"">
	<sourceDir>c:\source</sourceDir>
	<publishDir>\\file\share\build</publishDir>
</buildpublisher>";

			publisher = (BuildPublisher) NetReflector.Read(xml);

            var expected = new BuildPublisher
            {
                AlwaysPublish = true,
                CleanPublishDirPriorToCopy = true,
                CleanUpMethod = BuildPublisher.CleanupPolicy.KeepLastXBuilds,
                CleanUpValue = 10,
                PublishDir = @"\\file\share\build",
                SourceDir = @"c:\source",
                UseLabelSubDirectory = false
            };
            this.AssertAreSame(expected, publisher);
		}

		[Fact]
		public void LoadMinimalXml()
		{
			string xml = @"<buildpublisher />";

			publisher = (BuildPublisher) NetReflector.Read(xml);
			Assert.Null(publisher.SourceDir);
			Assert.Null(publisher.PublishDir);

            var expected = new BuildPublisher
            {
                AlwaysPublish = false,
                CleanPublishDirPriorToCopy = false,
                CleanUpMethod = BuildPublisher.CleanupPolicy.NoCleaning,
                CleanUpValue = 5,
                PublishDir = null,
                SourceDir = null,
                UseLabelSubDirectory = true
            };
            this.AssertAreSame(expected, publisher);
		}
		
		[Fact]
		public void PublishWorksIfNoPropertiesAreSpecified()
		{
			srcRoot.CreateDirectory().CreateTextFile(fileName, fileContents);
			result.WorkingDirectory = srcRoot.ToString();
			result.ArtifactDirectory = pubRoot.ToString();
			
			publisher = new BuildPublisher();
			publisher.Run(result);

			Assert.True(labelPubDir.Combine(fileName).Exists(), "File not found in pubRoot directory");
		}
		
		// [TearDown]
		public void TearDown()
		{
			srcRoot.DeleteDirectory();
			pubRoot.DeleteDirectory();
		}

        /// <summary>
        /// Checks that two <see cref="BuildPublisher"/> instances have the same properties.
        /// </summary>
        /// <param name="expected">The expected <see cref="BuildPublisher"/>.</param>
        /// <param name="actual">The actual <see cref="BuildPublisher"/>.</param>
        private void AssertAreSame(BuildPublisher expected, BuildPublisher actual)
        {
            InstanceAssert.PropertiesAreEqual(
                expected,
                actual,
                "PublishDir",
                "SourceDir",
                "UseLabelSubDirectory",
                "AlwaysPublish",
                "CleanPublishDirPriorToCopy",
                "CleanUpMethod",
                "CleanUpValue");
        }
	}
}
