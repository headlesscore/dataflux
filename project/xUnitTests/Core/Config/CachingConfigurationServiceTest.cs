using System;
using Moq;
using Xunit;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Config;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Config
{
	
	public class CachingConfigurationServiceTest
	{
		private Mock<IConfigurationService> slaveServiceMock;
		private CachingConfigurationService cachingConfigurationService;

		private Mock<IConfiguration> configurationMock;
		private IConfiguration configuration;

		// [SetUp]
		public void Setup()
		{
			slaveServiceMock = new Mock<IConfigurationService>();
			cachingConfigurationService = new CachingConfigurationService((IConfigurationService) slaveServiceMock.Object);

			configurationMock = new Mock<IConfiguration>();
			configuration = (IConfiguration) configurationMock.Object;
		}

		private void VerifyAll()
		{
			slaveServiceMock.Verify();
		}

		[Fact]
		public void ShouldDelegateLoadRequests()
		{
			// Setup
			slaveServiceMock.Setup(service => service.Load()).Returns(configuration).Verifiable();

			// Execute & Verify
			Assert.Equal(configuration, cachingConfigurationService.Load());
            Assert.Equal(configuration, cachingConfigurationService.Load());

            VerifyAll();
		}

		[Fact]
		public void ShouldCacheLoad()
		{
			// Setup
			// (only 1 call expected)
			slaveServiceMock.Setup(service => service.Load()).Returns(configuration).Verifiable();

			// Execute
			cachingConfigurationService.Load();
			cachingConfigurationService.Load();

			// Verify
			slaveServiceMock.Verify(service => service.Load(), Times.Once);
			VerifyAll();
		}

		[Fact]
		public void ShouldDelegateSaveRequests()
		{
			// Setup
			slaveServiceMock.Setup(service => service.Save(configuration)).Verifiable();

			// Execute
			cachingConfigurationService.Save(configuration);

			VerifyAll();
		}

		[Fact]
		public void ShouldDelegateEventHanderRequests()
		{
			ConfigurationUpdateHandler handler = new ConfigurationUpdateHandler(HandlerTarget);
			// Setup
			slaveServiceMock.Setup(service => service.AddConfigurationUpdateHandler(handler)).Verifiable();

			// Execute
			cachingConfigurationService.AddConfigurationUpdateHandler(handler);

			VerifyAll();
		}

		[Fact]
		public void InvalidatesCacheIfSlaveServiceChanges()
		{
			// Setup
			SlaveServiceForTestingEvents slaveService = new SlaveServiceForTestingEvents();
			cachingConfigurationService = new CachingConfigurationService(slaveService);
			IConfiguration configuration2 = (IConfiguration) new Mock<IConfiguration>().Object;

			// Execute & Verify
			slaveService.configuration = configuration;
			Assert.Equal(configuration, cachingConfigurationService.Load());

			slaveService.handler();
			slaveService.configuration = configuration2;
			Assert.Equal(configuration2, cachingConfigurationService.Load());

			VerifyAll();
		}

		public void HandlerTarget()
		{
			
		}

		class SlaveServiceForTestingEvents : IConfigurationService
		{
			public IConfiguration configuration;
			public ConfigurationUpdateHandler handler;

			public IConfiguration Load()
			{
				return configuration;
			}

			public void Save(IConfiguration configuration)
			{
				throw new NotImplementedException();
			}

			public void AddConfigurationUpdateHandler(ConfigurationUpdateHandler handler)
			{
				this.handler = handler;
			}

		    public void AddConfigurationSubfileLoadedHandler (
		        ConfigurationSubfileLoadedHandler handler)
		    {		        
		    }
		}
	}
}
