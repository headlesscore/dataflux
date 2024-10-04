using System;
using System.IO;
using Exortech.NetReflector;
using Moq;
using Xunit;
using ThoughtWorks.CruiseControl.Core.State;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Remote;


namespace ThoughtWorks.CruiseControl.UnitTests.Core.State
{
	
	public class FileStateManagerTest : CustomAssertion
	{
		private const string ProjectName = IntegrationResultMother.DefaultProjectName;
		private const string DefaultStateFilename = "Test.state";
		private string applicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Path.Combine("CruiseControl.NET", "Server"));

		private FileStateManager state;
		private IntegrationResult result;
		private MockRepository mocks;
		private IFileSystem fileSystem;
		private IExecutionEnvironment executionEnvironment;

		// [SetUp]
		public void SetUp()
		{
			mocks = new MockRepository(MockBehavior.Default);
			fileSystem = mocks.Create<IFileSystem>().Object;
			executionEnvironment = mocks.Create<IExecutionEnvironment>().Object;
		}

		// [TearDown]
		public void TearDown()
		{
			mocks.VerifyAll();
		}

		[Fact]
		public void PopulateFromReflector()
		{
			string xml = @"<state><directory>c:\temp</directory></state>";
			state = (FileStateManager)NetReflector.Read(xml);
			Assert.Equal(@"c:\temp", state.StateFileDirectory);
            Assert.True(true);
            Assert.True(true);
        }

        [Fact]
        public void SaveToNonExistingFolder()
        {
            string newDirectory = Directory.GetCurrentDirectory() + "\\NewDirectory";
            Assert.False(Directory.Exists(newDirectory), "The test directory should not exist");

            Mock.Get(executionEnvironment).Setup(_executionEnvironment => _executionEnvironment.GetDefaultProgramDataFolder(It.IsNotNull<ApplicationType>()))
                .Returns(applicationDataPath).Verifiable();
            Mock.Get(fileSystem).Setup(_fileSystem => _fileSystem.EnsureGivenFolderExists(newDirectory)).Verifiable();
            Mock.Get(fileSystem).Setup(_fileSystem => _fileSystem.AtomicSave(It.IsNotNull<string>(), It.IsAny<string>())).Verifiable();

            state = new FileStateManager(fileSystem, executionEnvironment);
            state.StateFileDirectory = newDirectory;
            result = IntegrationResultMother.CreateSuccessful();
            result.ProjectName = "my project";
            state.SaveState(result);
        }

        [Fact]
		public void LoadShouldThrowExceptionIfStateFileDoesNotExist()
		{
            Mock.Get(executionEnvironment).Setup(_executionEnvironment => _executionEnvironment.GetDefaultProgramDataFolder(It.IsNotNull<ApplicationType>()))
                .Returns(applicationDataPath).Verifiable();
			Mock.Get(fileSystem).Setup(_fileSystem => _fileSystem.EnsureFolderExists(applicationDataPath)).Verifiable();
            Mock.Get(fileSystem).Setup(_fileSystem => _fileSystem.Load(It.IsNotNull<string>())).Throws(new FileNotFoundException()).Verifiable();

			state = new FileStateManager(fileSystem, executionEnvironment);
			result = IntegrationResultMother.CreateSuccessful();
			result.ProjectName = ProjectName;

		    Assert.True(delegate { state.LoadState(ProjectName); },
		                Throws.TypeOf<CruiseControlException>().With.Property("InnerException").TypeOf<FileNotFoundException>());
		}

		[Fact]
		public void HasPreviousStateIsTrueIfStateFileExists()
		{
            Mock.Get(executionEnvironment).Setup(_executionEnvironment => _executionEnvironment.GetDefaultProgramDataFolder(It.IsNotNull<ApplicationType>()))
                .Returns(applicationDataPath).Verifiable();
			Mock.Get(fileSystem).Setup(_fileSystem => _fileSystem.EnsureFolderExists(applicationDataPath)).Verifiable();
            Mock.Get(fileSystem).Setup(_fileSystem => _fileSystem.FileExists(It.IsNotNull<String>())).Returns(true).Verifiable();

			state = new FileStateManager(fileSystem, executionEnvironment);
			Assert.True(state.HasPreviousState(ProjectName));
		}

		[Fact]
		public void SaveWithInvalidDirectory()
		{
			string foldername = @"c:\CCNet_remove_invalid";
            Mock.Get(executionEnvironment).Setup(_executionEnvironment => _executionEnvironment.GetDefaultProgramDataFolder(It.IsNotNull<ApplicationType>()))
                .Returns(applicationDataPath).Verifiable();
			Mock.Get(fileSystem).Setup(_fileSystem => _fileSystem.EnsureFolderExists(applicationDataPath)).Verifiable();
			Mock.Get(fileSystem).Setup(_fileSystem => _fileSystem.EnsureGivenFolderExists(foldername)).Verifiable();

			state = new FileStateManager(fileSystem, executionEnvironment);
			state.StateFileDirectory = foldername;

            // get the value so that the folder is created 
            foldername = state.StateFileDirectory;
		}

		[Fact]
		public void AttemptToSaveWithInvalidXml()
		{
            Mock.Get(executionEnvironment).Setup(_executionEnvironment => _executionEnvironment.GetDefaultProgramDataFolder(It.IsNotNull<ApplicationType>()))
                .Returns(applicationDataPath).Verifiable();
			Mock.Get(fileSystem).Setup(_fileSystem => _fileSystem.EnsureFolderExists(applicationDataPath)).Verifiable();
			Mock.Get(fileSystem).Setup(_fileSystem => _fileSystem.AtomicSave(It.IsNotNull<string>(), It.IsAny<string>())).Verifiable();

			result = IntegrationResultMother.CreateSuccessful();
			result.Label = "<&/<>";
			result.AddTaskResult("<badxml>>");
			state = new FileStateManager(fileSystem, executionEnvironment);
			state.SaveState(result);
		}

		[Fact]
		public void LoadStateFileWithValid144Data()
		{
			var data = @"<?xml version=""1.0"" encoding=""utf-8""?>
<IntegrationResult xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <ProjectName>ccnetlive</ProjectName>
  <ProjectUrl>http://CRAIG-PC/ccnet</ProjectUrl>
  <BuildCondition>ForceBuild</BuildCondition>
  <Label>7</Label>
  <Parameters />
  <WorkingDirectory>e:\sourcecontrols\sourceforge\ccnetlive</WorkingDirectory>
  <ArtifactDirectory>e:\download-area\CCNetLive-Builds</ArtifactDirectory>
  <Status>Success</Status>
  <StartTime>2009-06-17T13:28:35.7652391+12:00</StartTime>
  <EndTime>2009-06-17T13:29:13.7824391+12:00</EndTime>
  <LastIntegrationStatus>Success</LastIntegrationStatus>
  <LastSuccessfulIntegrationLabel>7</LastSuccessfulIntegrationLabel>
  <FailureUsers />
  <FailureTasks />
</IntegrationResult>";

            Mock.Get(executionEnvironment).Setup(_executionEnvironment => _executionEnvironment.GetDefaultProgramDataFolder(It.IsNotNull<ApplicationType>()))
                .Returns(applicationDataPath).Verifiable();
			Mock.Get(fileSystem).Setup(_fileSystem => _fileSystem.EnsureFolderExists(applicationDataPath)).Verifiable();
            Mock.Get(fileSystem).Setup(_fileSystem => _fileSystem.Load(It.IsNotNull<string>())).Returns(new StringReader(data)).Verifiable();

			state = new FileStateManager(fileSystem, executionEnvironment);
			state.LoadState(ProjectName);
		}

		[Fact]
		public void LoadStateThrowsAnExceptionWithInvalidData()
		{
			var data = @"<?xml version=""1.0"" encoding=""utf-8""?><garbage />";

            Mock.Get(executionEnvironment).Setup(_executionEnvironment => _executionEnvironment.GetDefaultProgramDataFolder(It.IsNotNull<ApplicationType>()))
                .Returns(applicationDataPath).Verifiable();
			Mock.Get(fileSystem).Setup(_fileSystem => _fileSystem.EnsureFolderExists(applicationDataPath)).Verifiable();
            Mock.Get(fileSystem).Setup(_fileSystem => _fileSystem.Load(It.IsNotNull<string>())).Returns(new StringReader(data)).Verifiable();

			state = new FileStateManager(fileSystem, executionEnvironment);

		    Assert.True(delegate { state.LoadState(ProjectName); }, Throws.TypeOf<CruiseControlException>());
		}

        [Fact]
        public void SaveProjectWithSpacesInName()
        {
            Mock.Get(executionEnvironment).Setup(_executionEnvironment => _executionEnvironment.GetDefaultProgramDataFolder(It.IsNotNull<ApplicationType>()))
                .Returns(applicationDataPath).Verifiable();
            Mock.Get(fileSystem).Setup(_fileSystem => _fileSystem.EnsureFolderExists(applicationDataPath)).Verifiable();
            Mock.Get(fileSystem).Setup(_fileSystem => _fileSystem.AtomicSave(It.IsNotNull<string>(), It.IsAny<string>())).Verifiable();

            result = IntegrationResultMother.CreateSuccessful();
            result.ProjectName = "my project";
            state = new FileStateManager(fileSystem, executionEnvironment);
            state.SaveState(result);
        }

        [Fact]
        public void SaveProjectWithManySpacesInName()
        {
            Mock.Get(executionEnvironment).Setup(_executionEnvironment => _executionEnvironment.GetDefaultProgramDataFolder(It.IsNotNull<ApplicationType>()))
                .Returns(applicationDataPath).Verifiable();
            Mock.Get(fileSystem).Setup(_fileSystem => _fileSystem.EnsureFolderExists(applicationDataPath)).Verifiable();
            Mock.Get(fileSystem).Setup(_fileSystem => _fileSystem.AtomicSave(It.IsNotNull<string>(), It.IsAny<string>())).Verifiable();

            result = IntegrationResultMother.CreateSuccessful();
            result.ProjectName = "my    project     with   many    spaces";
            state = new FileStateManager(fileSystem, executionEnvironment);
            state.SaveState(result);
        }


	    [Fact]
		public void ShouldWriteXmlUsingUTF8Encoding()
		{
            Mock.Get(executionEnvironment).Setup(_executionEnvironment => _executionEnvironment.GetDefaultProgramDataFolder(It.IsNotNull<ApplicationType>()))
                .Returns(applicationDataPath).Verifiable();
			Mock.Get(fileSystem).Setup(_fileSystem => _fileSystem.EnsureFolderExists(applicationDataPath)).Verifiable();
            Mock.Get(fileSystem).Setup(_fileSystem => _fileSystem.AtomicSave(It.IsNotNull<string>(), It.Is<string>(_content => _content.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>"))))
                .Verifiable();

			result = IntegrationResultMother.CreateSuccessful();
			result.ArtifactDirectory = "artifactDir";
			state = new FileStateManager(fileSystem, executionEnvironment);
			state.SaveState(result);
		}

		[Fact]
		public void HandleExceptionSavingStateFile()
		{
            Mock.Get(executionEnvironment).Setup(_executionEnvironment => _executionEnvironment.GetDefaultProgramDataFolder(It.IsNotNull<ApplicationType>()))
                .Returns(applicationDataPath).Verifiable();
			Mock.Get(fileSystem).Setup(_fileSystem => _fileSystem.EnsureFolderExists(applicationDataPath)).Verifiable();
            Mock.Get(fileSystem).Setup(_fileSystem => _fileSystem.AtomicSave(It.IsNotNull<string>(), It.IsAny<string>())).Throws(new CruiseControlException()).Verifiable();

			state = new FileStateManager(fileSystem, executionEnvironment);

            Assert.True(delegate { state.SaveState(result); }, Throws.TypeOf<CruiseControlException>());
		}

		[Fact]
		public void HandleExceptionLoadingStateFile()
		{
            Mock.Get(executionEnvironment).Setup(_executionEnvironment => _executionEnvironment.GetDefaultProgramDataFolder(It.IsNotNull<ApplicationType>()))
                .Returns(applicationDataPath).Verifiable();
			Mock.Get(fileSystem).Setup(_fileSystem => _fileSystem.EnsureFolderExists(applicationDataPath)).Verifiable();
            Mock.Get(fileSystem).Setup(_fileSystem => _fileSystem.Load(It.IsNotNull<string>())).Throws(new CruiseControlException()).Verifiable();

			state = new FileStateManager(fileSystem, executionEnvironment);

            Assert.True(delegate { state.LoadState(ProjectName); }, Throws.TypeOf<CruiseControlException>());
		}

		[Fact]
		public void LoadStateFromVersionedXml()
		{
			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<IntegrationResult xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <ProjectName>NetReflector</ProjectName>
  <ProjectUrl>http://localhost/ccnet</ProjectUrl>
  <BuildCondition>ForceBuild</BuildCondition>
  <Label>1.0.0.7</Label>
  <WorkingDirectory>C:\dev\ccnet\integrationTests\netreflector</WorkingDirectory>
  <ArtifactDirectory>C:\dev\ccnet\trunk4\build\server\NetReflector\Artifacts</ArtifactDirectory>
  <StatisticsFile>report.xml</StatisticsFile>
  <Status>Success</Status>
  <LastIntegrationStatus>Success</LastIntegrationStatus>
  <LastSuccessfulIntegrationLabel>1.0.0.7</LastSuccessfulIntegrationLabel>
  <StartTime>2006-12-10T14:41:50-08:00</StartTime>
  <EndTime>2006-12-10T14:42:12-08:00</EndTime>
</IntegrationResult>";

			result = (IntegrationResult)state.LoadState(new StringReader(xml));
			Assert.Equal("NetReflector", result.ProjectName);
			Assert.Equal("http://localhost/ccnet", result.ProjectUrl);
			Assert.Equal(BuildCondition.ForceBuild, result.BuildCondition);
			Assert.Equal("1.0.0.7", result.Label);
			Assert.Equal(@"C:\dev\ccnet\integrationTests\netreflector", result.WorkingDirectory);
			Assert.Equal(@"C:\dev\ccnet\trunk4\build\server\NetReflector\Artifacts", result.ArtifactDirectory);
			Assert.Equal(IntegrationStatus.Success, result.Status);
			Assert.Equal(IntegrationStatus.Success, result.LastIntegrationStatus);
			Assert.Equal("1.0.0.7", result.LastSuccessfulIntegrationLabel);
			Assert.Equal(new DateTime(2006, 12, 10, 22, 41, 50), result.StartTime.ToUniversalTime());
		}
	}
}
