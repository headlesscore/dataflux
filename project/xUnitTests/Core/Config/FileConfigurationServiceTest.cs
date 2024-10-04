using System.IO;
using Moq;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Config;
using Xunit;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Config
{
	public class FileConfigurationServiceTest
	{
		private Mock<IConfigurationFileLoader> configurationFileLoaderMock;
		private Mock<IConfigurationFileSaver> configurationFileSaverMock;
		private FileConfigurationService fileService;

		private Mock<IConfiguration> configurationMock;
		private IConfiguration configuration;
		private FileInfo configFile;

		//// [SetUp]
		public void Setup()
		{
			configurationFileLoaderMock = new Mock<IConfigurationFileLoader>();
			configurationFileSaverMock = new Mock<IConfigurationFileSaver>();
			configFile = new FileInfo("testFileName");

			fileService = new FileConfigurationService((IConfigurationFileLoader) configurationFileLoaderMock.Object,
			                                           (IConfigurationFileSaver) configurationFileSaverMock.Object,
			                                           configFile);

			configurationMock = new Mock<IConfiguration>();
			configuration = (IConfiguration) configurationMock.Object;
		}

		private void VerifyAll()
		{
			configurationFileLoaderMock.Verify();
			configurationFileSaverMock.Verify();
		}

		[Fact]
		public void ShouldDelegateLoadRequests()
		{
			// Setup
			configurationFileLoaderMock.Setup(loader => loader.Load(configFile)).Returns(configuration).Verifiable();

			// Execute & Verify
			Assert.Equal(configuration, fileService.Load());
            Assert.Equal(configuration, fileService.Load());

            VerifyAll();
		}

		[Fact]
		public void ShouldDelegateSaveRequests()
		{
			// Setup
			configurationFileSaverMock.Setup(saver => saver.Save(configuration, configFile)).Verifiable();

			// Execute & Verify
			fileService.Save(configuration);

			VerifyAll();
		}

		[Fact(Skip ="unimplemented")]
		public void DoesSomethingSaneWhenBadLoadThingsHappen()
		{}
	}
}
