using Xunit;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Config;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Config
{
	
	public class ConfigurationTest
	{
		[Fact]
		public void CreateIntegrators()
		{
			Project project1 = new Project();
			project1.Name = "project1";
			Project project2 = new Project();
			project2.Name = "project2";

			Configuration config = new Configuration();
			config.AddProject(project1);
			config.AddProject(project2);
		}

        [Fact]
        public void FindQueueFound()
        {
            DefaultQueueConfiguration queueConfig = new DefaultQueueConfiguration("Test Queue");
            Configuration config = new Configuration();
            config.QueueConfigurations.Add(queueConfig);
            IQueueConfiguration foundConfig = config.FindQueueConfiguration("Test Queue");
            Assert.NotNull(foundConfig);
            Assert.NotNull(foundConfig);
            Assert.Same(queueConfig, foundConfig);
        }

        [Fact]
        public void FindQueueNotFound()
        {
            Configuration config = new Configuration();
            IQueueConfiguration foundConfig = config.FindQueueConfiguration("Test Queue");
            Assert.NotNull(foundConfig);
            Assert.IsType<DefaultQueueConfiguration>(foundConfig);
            Assert.Equal("Test Queue", foundConfig.Name);
            Assert.Equal(QueueDuplicateHandlingMode.UseFirst, foundConfig.HandlingMode);
        }
    }
}
