using System.Collections.Generic;
using System.IO;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Configuration
{
	[TestFixture]
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

		[Test]
		public void CanLoadConfigurationFromFile()
		{
			CCTrayMultiConfiguration configuration = CreateTestConfiguration(ConfigXml);
			ClassicAssert.AreEqual(2, configuration.Projects.Length);
			ClassicAssert.AreEqual("tcp://blah1", configuration.Projects[0].ServerUrl);
			ClassicAssert.AreEqual("ProjectOne", configuration.Projects[0].ProjectName);
			ClassicAssert.AreEqual("tcp://blah2", configuration.Projects[1].ServerUrl);
			ClassicAssert.AreEqual("Project Two", configuration.Projects[1].ProjectName);

			ClassicAssert.IsTrue(configuration.ShouldShowBalloonOnBuildTransition);
		}

		[Test]
		public void WhenTheConfigurationDoesNotContainDirectivesRelatingToShowingBalloonsItDefaultsToTrue()
		{
			const string ConfigWithoutBalloonStuff = @"
<Configuration>
	<Projects />
</Configuration>";

			CCTrayMultiConfiguration configuration = CreateTestConfiguration(ConfigWithoutBalloonStuff);
			ClassicAssert.IsTrue(configuration.ShouldShowBalloonOnBuildTransition);
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


		[Test]
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
			ClassicAssert.AreEqual(2, monitorList.Length);

			mockProjectConfigFactory.Verify();
            mockServerConfigFactory.Verify();
            mockCruiseServerManager.Verify();
		}

		[Test]
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
			ClassicAssert.AreEqual(1, configuration.Projects.Length);
			ClassicAssert.AreEqual("projName", configuration.Projects[0].ProjectName);
		}

		[Test]
		public void CreatesAnEmptySettingsFileIfTheConfigFileIsNotFound()
		{
			mockProjectConfigFactory = new Mock<ICruiseProjectManagerFactory>(MockBehavior.Strict);
			CCTrayMultiConfiguration configuration = new CCTrayMultiConfiguration(
				(ICruiseServerManagerFactory) mockServerConfigFactory.Object,
				(ICruiseProjectManagerFactory) mockProjectConfigFactory.Object,
				"config_file_that_isnt_present.xml");

			ClassicAssert.IsNotNull(configuration);
			ClassicAssert.AreEqual(0, configuration.Projects.Length);
			ClassicAssert.IsTrue(configuration.ShouldShowBalloonOnBuildTransition);
			ClassicAssert.IsNull(configuration.Audio.BrokenBuildSound);
			ClassicAssert.IsNull(configuration.Audio.FixedBuildSound);
			ClassicAssert.IsNull(configuration.Audio.StillFailingBuildSound);
			ClassicAssert.IsNull(configuration.Audio.StillSuccessfulBuildSound);
			ClassicAssert.IsFalse(configuration.X10.Enabled);
		}

		[Test]
		public void CanBuildUniqueServerListWithTwoUniqueServerProjects()
		{
			CCTrayMultiConfiguration configuration = CreateTestConfiguration(ConfigXml);

			BuildServer[] buildServers = configuration.GetUniqueBuildServerList();
			ClassicAssert.AreEqual(2, buildServers.Length);
			ClassicAssert.AreEqual("tcp://blah1", buildServers[0].Url);
			ClassicAssert.AreEqual("tcp://blah2", buildServers[1].Url);
		}

		[Test]
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
			ClassicAssert.AreEqual(1, buildServers.Length);
			ClassicAssert.AreEqual("tcp://blah1", buildServers[0].Url);
		}

		[Test]
		public void CanProvideASetOfServerMonitors()
		{
			CCTrayMultiConfiguration configuration = CreateTestConfiguration(ConfigXml);

			mockServerConfigFactory.Setup(factory => factory.Create(configuration.GetUniqueBuildServerList()[0]))
				.Returns(() => null).Verifiable();
			mockServerConfigFactory.Setup(factory => factory.Create(configuration.GetUniqueBuildServerList()[1]))
				.Returns(() => null).Verifiable();

			IServerMonitor[] monitorList = configuration.GetServerMonitors();
			ClassicAssert.AreEqual(2, monitorList.Length);

			mockServerConfigFactory.Verify();
		}

		[Test]
		public void CanPersistAndReloadX10Configuration()
		{
			CCTrayMultiConfiguration configuration = CreateTestConfiguration(ConfigXml);

			configuration.Persist();

			configuration.Reload();
		}

        [Test]
        public void SetExtensionSettings()
        {
            CCTrayProject newValue = new CCTrayProject();
            newValue.ExtensionSettings = "Some settings";
            ClassicAssert.AreEqual("Some settings", newValue.ExtensionSettings);
        }

        [Test]
        public void SetExtensionNameNonBlank()
        {
            CCTrayProject newValue = new CCTrayProject();
            newValue.ExtensionName = "An extension";
            ClassicAssert.AreEqual("An extension", newValue.ExtensionName);
            ClassicAssert.AreEqual(BuildServerTransport.Extension, newValue.BuildServer.Transport);
        }

        [Test]
        public void SetExtensionNameBlank()
        {
            CCTrayProject newValue = new CCTrayProject();
            newValue.ExtensionName = string.Empty;
            ClassicAssert.AreEqual(string.Empty, newValue.ExtensionName);
            ClassicAssert.AreEqual(BuildServerTransport.HTTP, newValue.BuildServer.Transport);
        }
	}
}
