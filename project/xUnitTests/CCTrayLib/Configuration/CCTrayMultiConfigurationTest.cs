using System.Collections.Generic;
using System.IO;
using Moq;
using Xunit;
using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Configuration
{
	public class CCTrayMultiConfigurationTest
	{
		public const string ConfigXml =
			@"
<Configuration>
	<Projects>
		<Project serverUrl='tcp://blah1' projectName='ProjectOne' />
		<Project serverUrl='tcp://blah2' projectName='Project Two' />
	</Projects>
	<BuildTransitionNotification showBalloon='true'>
	</BuildTransitionNotification>

</Configuration>";

		private Mock<ICruiseServerManagerFactory> mockServerConfigFactory;
		private Mock<ICruiseProjectManagerFactory> mockProjectConfigFactory;

		[Fact]
		public void CanLoadConfigurationFromFile()
		{
			CCTrayMultiConfiguration configuration = CreateTestConfiguration(ConfigXml);
			Assert.Equal(2, configuration.Projects.Length);
			Assert.Equal("tcp://blah1", configuration.Projects[0].ServerUrl);
			Assert.Equal("ProjectOne", configuration.Projects[0].ProjectName);
			Assert.Equal("tcp://blah2", configuration.Projects[1].ServerUrl);
			Assert.Equal("Project Two", configuration.Projects[1].ProjectName);

			Assert.True(configuration.ShouldShowBalloonOnBuildTransition);
		}

		[Fact]
		public void WhenTheConfigurationDoesNotContainDirectivesRelatingToShowingBalloonsItDefaultsToTrue()
		{
			const string ConfigWithoutBalloonStuff = @"
<Configuration>
	<Projects />
</Configuration>";

			CCTrayMultiConfiguration configuration = CreateTestConfiguration(ConfigWithoutBalloonStuff);
			Assert.True(configuration.ShouldShowBalloonOnBuildTransition);
		}

		private const string configFileName = "test_config.xml";

		private CCTrayMultiConfiguration CreateTestConfiguration(string configFileContents)
		{
			using (TextWriter configFile = File.CreateText(configFileName))
				configFile.Write(configFileContents);

			mockServerConfigFactory = new Mock<ICruiseServerManagerFactory>(MockBehavior.Strict);

			mockProjectConfigFactory = new Mock<ICruiseProjectManagerFactory>(MockBehavior.Strict);
			return new CCTrayMultiConfiguration(
				(ICruiseServerManagerFactory) mockServerConfigFactory.Object,
				(ICruiseProjectManagerFactory) mockProjectConfigFactory.Object,
				configFileName);
		}


		[Fact]
		public void CanProvideASetOfProjectStatusMonitors()
		{
			CCTrayMultiConfiguration provider = CreateTestConfiguration(ConfigXml);
            var mockCruiseServerManager = new Mock<ICruiseServerManager>(MockBehavior.Strict);
            MockSequence sequence = new MockSequence();
            mockCruiseServerManager.InSequence(sequence).SetupGet(_manager => _manager.Configuration)
                .Returns(new BuildServer("tcp://blah1")).Verifiable();
            mockCruiseServerManager.InSequence(sequence).SetupGet(_manager => _manager.Configuration)
                .Returns(new BuildServer("tcp://blah2")).Verifiable();
		    ICruiseServerManager cruiseServerManagerInstance = (ICruiseServerManager) mockCruiseServerManager.Object;

            mockServerConfigFactory.Setup(factory => factory.Create(provider.GetUniqueBuildServerList()[0]))
                .Returns(() => cruiseServerManagerInstance).Verifiable();
            mockServerConfigFactory.Setup(factory => factory.Create(provider.GetUniqueBuildServerList()[1]))
                .Returns(() => cruiseServerManagerInstance).Verifiable();
            ISingleServerMonitor[] serverMonitorList = provider.GetServerMonitors();

			mockProjectConfigFactory.Setup(factory => factory.Create(provider.Projects[0], It.IsAny<IDictionary<BuildServer, ICruiseServerManager>>()))
				.Returns(() => null).Verifiable();
			mockProjectConfigFactory.Setup(factory => factory.Create(provider.Projects[1], It.IsAny<IDictionary<BuildServer, ICruiseServerManager>>()))
				.Returns(() => null).Verifiable();

            IProjectMonitor[] monitorList = provider.GetProjectStatusMonitors(serverMonitorList);
			Assert.Equal(2, monitorList.Length);

			mockProjectConfigFactory.Verify();
            mockServerConfigFactory.Verify();
            mockCruiseServerManager.Verify();
		}

		[Fact]
		public void CanPersist()
		{
			const string SimpleConfig = @"
<Configuration>
	<Projects />
</Configuration>";

			CCTrayMultiConfiguration configuration = CreateTestConfiguration(SimpleConfig);
			configuration.Projects = new CCTrayProject[1] {new CCTrayProject("tcp://url", "projName")};

			configuration.Persist();

			configuration.Reload();
			Assert.Single(configuration.Projects);
			Assert.Equal("projName", configuration.Projects[0].ProjectName);
		}

		[Fact]
		public void CreatesAnEmptySettingsFileIfTheConfigFileIsNotFound()
		{
			mockProjectConfigFactory = new Mock<ICruiseProjectManagerFactory>(MockBehavior.Strict);
			CCTrayMultiConfiguration configuration = new CCTrayMultiConfiguration(
				(ICruiseServerManagerFactory) mockServerConfigFactory.Object,
				(ICruiseProjectManagerFactory) mockProjectConfigFactory.Object,
				"config_file_that_isnt_present.xml");

			Assert.False(configuration is null);
			Assert.Empty(configuration.Projects);
			Assert.True(configuration.ShouldShowBalloonOnBuildTransition);
			Assert.True(configuration.Audio.BrokenBuildSound is null);
			Assert.True(configuration.Audio.FixedBuildSound is null);
			Assert.True(configuration.Audio.StillFailingBuildSound is null);
			Assert.True(configuration.Audio.StillSuccessfulBuildSound is null);
			Assert.False(configuration.X10.Enabled);
		}

		[Fact]
		public void CanBuildUniqueServerListWithTwoUniqueServerProjects()
		{
			CCTrayMultiConfiguration configuration = CreateTestConfiguration(ConfigXml);

			BuildServer[] buildServers = configuration.GetUniqueBuildServerList();
			Assert.Equal(2, buildServers.Length);
			Assert.Equal("tcp://blah1", buildServers[0].Url);
			Assert.Equal("tcp://blah2", buildServers[1].Url);
		}

		[Fact]
		public void CanBuildUniqueServerListWithTwoSameServerProjects()
		{
			const string SameServerProjectConfigXml = @"
<Configuration>
	<Projects>
		<Project serverUrl='tcp://blah1' projectName='ProjectOne' />
		<Project serverUrl='tcp://blah1' projectName='ProjectTwo' />
	</Projects>
</Configuration>";
			CCTrayMultiConfiguration configuration = CreateTestConfiguration(SameServerProjectConfigXml);

			BuildServer[] buildServers = configuration.GetUniqueBuildServerList();
			Assert.Single(buildServers);
			Assert.Equal("tcp://blah1", buildServers[0].Url);
		}

		[Fact]
		public void CanProvideASetOfServerMonitors()
		{
			CCTrayMultiConfiguration configuration = CreateTestConfiguration(ConfigXml);

			mockServerConfigFactory.Setup(factory => factory.Create(configuration.GetUniqueBuildServerList()[0]))
				.Returns(() => null).Verifiable();
			mockServerConfigFactory.Setup(factory => factory.Create(configuration.GetUniqueBuildServerList()[1]))
				.Returns(() => null).Verifiable();

			IServerMonitor[] monitorList = configuration.GetServerMonitors();
			Assert.Equal(2, monitorList.Length);

			mockServerConfigFactory.Verify();
		}

		[Fact]
		public void CanPersistAndReloadX10Configuration()
		{
			CCTrayMultiConfiguration configuration = CreateTestConfiguration(ConfigXml);

			configuration.Persist();

			configuration.Reload();
		}

        [Fact]
        public void SetExtensionSettings()
        {
            CCTrayProject newValue = new CCTrayProject();
            newValue.ExtensionSettings = "Some settings";
            Assert.Equal("Some settings", newValue.ExtensionSettings);
        }

        [Fact]
        public void SetExtensionNameNonBlank()
        {
            CCTrayProject newValue = new CCTrayProject();
            newValue.ExtensionName = "An extension";
            Assert.Equal("An extension", newValue.ExtensionName);
            Assert.Equal(BuildServerTransport.Extension, newValue.BuildServer.Transport);
        }

        [Fact]
        public void SetExtensionNameBlank()
        {
            CCTrayProject newValue = new CCTrayProject();
            newValue.ExtensionName = string.Empty;
            Assert.Equal(string.Empty, newValue.ExtensionName);
            Assert.Equal(BuildServerTransport.HTTP, newValue.BuildServer.Transport);
        }
	}
}
