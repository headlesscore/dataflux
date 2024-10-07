using System.IO;
using System.Xml;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Config;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Config
{
	[TestFixture]
	public class ConfigurationFileLoaderTest : CustomAssertion
	{
		private DefaultConfigurationFileLoader fileLoader;

		[SetUp]
		protected void SetUp()
		{
			fileLoader = new DefaultConfigurationFileLoader();
		}

		[TearDown]
		protected void TearDown() 
		{
			TempFileUtil.DeleteTempDir(this);
		}

		[Test]
		public void LoadConfigurationFile()
		{
			string xml = "<cruisecontrol />";
			FileInfo configFile = new FileInfo(TempFileUtil.CreateTempXmlFile(TempFileUtil.CreateTempDir(this), "loadernet.config", xml));
			XmlDocument config = fileLoader.LoadConfiguration(configFile);
			ClassicAssert.IsNotNull(config);
            ClassicAssert.IsNotNull(config);
            ClassicAssert.AreEqual(xml, config.OuterXml);
		}

		[Test]
		public void LoadConfigurationFile_MissingFile()
		{
			FileInfo configFile = new FileInfo(@"c:\unknown\config.file.xml");
			ClassicAssert.That(delegate { fileLoader.LoadConfiguration(configFile); },
                        Throws.TypeOf<ConfigurationFileMissingException>());
		}

		[Test]
		public void LoadConfigurationFile_FileOnlyNoPath()
		{
			FileInfo configFile = new FileInfo(@"ccnet_unknown.config");
			ClassicAssert.That(delegate { fileLoader.LoadConfiguration(configFile); },
                        Throws.TypeOf<ConfigurationFileMissingException>());
		}

		[Test]
		public void LoadConfiguration_BadXml()
		{
			FileInfo configFile = new FileInfo(TempFileUtil.CreateTempXmlFile(TempFileUtil.CreateTempDir(this), "loadernet.config"
				, "<test><a><b/></test>"));
            ClassicAssert.That(delegate { fileLoader.LoadConfiguration(configFile); },
                        Throws.TypeOf<ConfigurationException>());
		}
	}
}
