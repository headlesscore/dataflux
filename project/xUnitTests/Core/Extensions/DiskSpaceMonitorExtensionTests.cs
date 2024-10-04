namespace ThoughtWorks.CruiseControl.UnitTests.Core.Extensions
{
    using System;
    using System.Xml;
    using Moq;
    using Xunit;
    using CruiseControl.Core.Extensions;
    using CruiseControl.Core.Util;
    using CruiseControl.Remote;
    using CruiseControl.Remote.Events;
    

    /// <summary>
    /// Test the disk space monitor.
    /// </summary>
    
    public class DiskSpaceMonitorExtensionTests
    {
        private MockRepository mocks;

        // [SetUp]
        public void Setup()
        {
            mocks = new MockRepository(MockBehavior.Strict);
        }

        [Fact]
        public void InitialiseLoadsTheSpaceCorrectlyForGb()
        {
            var server = mocks.Create<ICruiseServer>().Object;
            var extension = new DiskSpaceMonitorExtension();
            var configuration = new ExtensionConfiguration();
            configuration.Items = new XmlElement[] {
                CreateSizeElement("Gb", 100, "C:\\")
            };
            extension.Initialise(server, configuration);
            Assert.Equal(107374182400, extension.RetrieveMinimumSpaceRequired("C:\\"));
            Assert.Equal(107374182400, extension.RetrieveMinimumSpaceRequired("C:\\"));
        }

        [Fact]
        public void InitialiseLoadsTheSpaceCorrectlyForMb()
        {
            var server = mocks.Create<ICruiseServer>().Object;
            var extension = new DiskSpaceMonitorExtension();
            var configuration = new ExtensionConfiguration();
            configuration.Items = new XmlElement[] {
                CreateSizeElement("Mb", 100, "C:\\")
            };
            extension.Initialise(server, configuration);
            Assert.Equal(104857600, extension.RetrieveMinimumSpaceRequired("C:\\"));
        }

        [Fact]
        public void InitialiseLoadsTheSpaceCorrectlyForKb()
        {
            var server = mocks.Create<ICruiseServer>().Object;
            var extension = new DiskSpaceMonitorExtension();
            var configuration = new ExtensionConfiguration();
            configuration.Items = new XmlElement[] {
                CreateSizeElement("Kb", 100, "C:\\")
            };
            extension.Initialise(server, configuration);
            Assert.Equal(102400, extension.RetrieveMinimumSpaceRequired("C:\\"));
        }

        [Fact]
        public void InitialiseLoadsTheSpaceCorrectlyForB()
        {
            var server = mocks.Create<ICruiseServer>().Object;
            var extension = new DiskSpaceMonitorExtension();
            var configuration = new ExtensionConfiguration();
            configuration.Items = new XmlElement[] {
                CreateSizeElement("b", 100, "C:\\")
            };
            extension.Initialise(server, configuration);
            Assert.Equal(100, extension.RetrieveMinimumSpaceRequired("C:\\"));
        }

        [Fact]
        public void InitialiseLoadsTheSpaceCorrectlyForMissing()
        {
            var server = mocks.Create<ICruiseServer>().Object;
            var extension = new DiskSpaceMonitorExtension();
            var configuration = new ExtensionConfiguration();
            configuration.Items = new XmlElement[] {
                CreateSizeElement(null, 100, "C:\\")
            };
            extension.Initialise(server, configuration);
            Assert.Equal(104857600, extension.RetrieveMinimumSpaceRequired("C:\\"));
        }

        [Fact]
        public void InitialiseThrowsAnErrorForUnknownUnit()
        {
            var server = mocks.Create<ICruiseServer>().Object;
            var extension = new DiskSpaceMonitorExtension();
            var configuration = new ExtensionConfiguration();
            configuration.Items = new XmlElement[] {
                CreateSizeElement("garbage", 100, "C:\\")
            };
            Assert.True(delegate { extension.Initialise(server, configuration); },
                        Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Fact]
        public void InitialiseThrowsAnErrorForUnknownElement()
        {
            var server = mocks.Create<ICruiseServer>().Object;
            var extension = new DiskSpaceMonitorExtension();
            var configuration = new ExtensionConfiguration();
            var document = new XmlDocument();
            configuration.Items = new XmlElement[] {
                document.CreateElement("garbage")
            };
            Assert.True(delegate { extension.Initialise(server, configuration); },
                        Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Fact]
        public void InitialiseThrowsAnErrorWithNoDrives()
        {
            var server = mocks.Create<ICruiseServer>().Object;
            var extension = new DiskSpaceMonitorExtension();
            var configuration = new ExtensionConfiguration();
            configuration.Items = new XmlElement[] {
            };
            Assert.True(delegate { extension.Initialise(server, configuration); },
                        Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Fact]
        public void InitialiseLoadsTheSpaceCorrectlyForMultipleDrives()
        {
            var server = mocks.Create<ICruiseServer>().Object;
            var extension = new DiskSpaceMonitorExtension();
            var configuration = new ExtensionConfiguration();
            configuration.Items = new XmlElement[] {
                CreateSizeElement("Mb", 100, "C:\\"), CreateSizeElement("Kb", 100, "D:\\")
            };
            extension.Initialise(server, configuration);
            Assert.Equal(104857600, extension.RetrieveMinimumSpaceRequired("C:\\"));
            Assert.Equal(102400, extension.RetrieveMinimumSpaceRequired("D:\\"));
        }

        [Fact]
        public void IntegrationIsSuccessfulWhenSufficientSpace()
        {
            // Initialise the file system
            var fileSystem = mocks.Create<IFileSystem>().Object;
            Mock.Get(fileSystem)
                .Setup(_fileSystem => _fileSystem.GetFreeDiskSpace("c:\\"))
                .Returns(104857600);

            // Initialise the server
            var server = mocks.Create<ICruiseServer>().Object;
            Mock.Get(server)
                .Setup(_server => _server.RetrieveService(typeof(IFileSystem)))
                .Returns(fileSystem);

            // Initialise the extension
            var extension = new DiskSpaceMonitorExtension();
            var configuration = new ExtensionConfiguration();
            configuration.Items = new XmlElement[] {
                CreateSizeElement("Mb", 1, "C:\\")
            };

            // Run the actual test
            extension.Initialise(server, configuration);
            var args = new IntegrationStartedEventArgs(null, "Project 1");
            Mock.Get(server).Raise(_server => _server.IntegrationStarted += null, args);
            Assert.Equal(IntegrationStartedEventArgs.EventResult.Continue, args.Result);
        }

        [Fact]
        public void IntegrationIsStoppedWhenInsufficientSpace()
        {
            // Initialise the file system
            var fileSystem = mocks.Create<IFileSystem>().Object;
            Mock.Get(fileSystem)
                .Setup(_fileSystem => _fileSystem.GetFreeDiskSpace("c:\\"))
                .Returns(102400);

            // Initialise the server
            var server = mocks.Create<ICruiseServer>().Object;
            Mock.Get(server)
                .Setup(_server => _server.RetrieveService(typeof(IFileSystem)))
                .Returns(fileSystem);

            // Initialise the extension
            var extension = new DiskSpaceMonitorExtension();
            var configuration = new ExtensionConfiguration();
            configuration.Items = new XmlElement[] {
                CreateSizeElement("Mb", 1, "C:\\")
            };

            // Run the actual test
            extension.Initialise(server, configuration);
            var args = new IntegrationStartedEventArgs(null, "Project 1");
            Mock.Get(server).Raise(_server => _server.IntegrationStarted += null, args);
            Assert.Equal(IntegrationStartedEventArgs.EventResult.Cancel, args.Result);
        }

        [Fact]
        public void StartAndStopDoesNothing()
        {
            var extension = new DiskSpaceMonitorExtension();

            extension.Start();
            extension.Stop();

            mocks.VerifyAll();
        }

        [Fact]
        public void StartAndAbortDoesNothing()
        {
            var extension = new DiskSpaceMonitorExtension();

            extension.Start();
            extension.Abort();

            mocks.VerifyAll();
        }

        private XmlElement CreateSizeElement(string unit, int size, string drive)
        {
            var document = new XmlDocument();
            var element = document.CreateElement("drive");
            if (!string.IsNullOrEmpty(unit)) element.SetAttribute("unit", unit);
            element.SetAttribute("name", drive);
            element.InnerText = size.ToString();
            return element;
        }
    }
}
