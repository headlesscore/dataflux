using System.IO;
using System.Xml;
using Xunit;
using ThoughtWorks.CruiseControl.Core.Config;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Config
{
	
	public class ConfigurationFileLoaderTest : CustomAssertion
	{
		private DefaultConfigurationFileLoader fileLoader;

		// [SetUp]
		protected void SetUp()
		{
			fileLoader = new DefaultConfigurationFileLoader();
		}

		// [TearDown]
		protected void TearDown() 
		{
			TempFileUtil.DeleteTempDir(this);
		}

		[Fact]
		public void LoadConfigurationFile()
		{
			string xml = "<cruisecontrol />";
			FileInfo configFile = new FileInfo(TempFileUtil.CreateTempXmlFile(TempFileUtil.CreateTempDir(this), "loadernet.config", xml));
			XmlDocument config = fileLoader.LoadConfiguration(configFile);
			Assert.NotNull(config);
            Assert.NotNull(config);
            Assert.Equal(xml, config.OuterXml);
		}

		[Fact]
		public void LoadConfigurationFile_MissingFile()
		{
			FileInfo configFile = new FileInfo(@"c:\unknown\config.file.xml");
			Assert.Throws<ConfigurationFileMissingException>(delegate { fileLoader.LoadConfiguration(configFile); });
		}

		[Fact]
		public void LoadConfigurationFile_FileOnlyNoPath()
		{
			FileInfo configFile = new FileInfo(@"ccnet_unknown.config");
			Assert.Throws<ConfigurationFileMissingException>(delegate { fileLoader.LoadConfiguration(configFile); });
		}

		[Fact]
		public void LoadConfiguration_BadXml()
		{
			FileInfo configFile = new FileInfo(TempFileUtil.CreateTempXmlFile(TempFileUtil.CreateTempDir(this), "loadernet.config"
				, "<test><a><b/></test>"));
            Assert.Throws<ConfigurationException>(delegate { fileLoader.LoadConfiguration(configFile); });
		}
	}
}
