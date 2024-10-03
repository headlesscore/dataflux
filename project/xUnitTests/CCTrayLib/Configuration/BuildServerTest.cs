using System;
using Xunit;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using ThoughtWorks.CruiseControl.CCTrayLib;


namespace ThoughtWorks.CruiseControl.xUnitTests.CCTrayLib.Configuration
{
	public class BuildServerTest
	{
		[Fact]
		public void CanBuildADisplayNameFromAServerUrl()
		{
			BuildServer server = new BuildServer("tcp://server:21234/blahblahblah.rem");
            Assert.Equal("server", server.DisplayName);			
		}

		[Fact]
		public void ForHttpUrlsDisplayNameDisplaysTheEntireUrl()
		{
			BuildServer server = new BuildServer("http://one");
            Assert.Equal("http://one", server.DisplayName);
		}

		[Fact]
		public void WhenThePortNumberIsNonDefaultThePortNumberIsDisplayed()
		{
			BuildServer server = new BuildServer("tcp://server:123/blahblahblah.rem");
            Assert.Equal("server:123", server.DisplayName);			
		}
		
		[Fact]
		public void CanParseADisplayNameWithAPort()
		{
			BuildServer server = BuildServer.BuildFromRemotingDisplayName("server:123");
            Assert.Equal("tcp://server:123/CruiseManager.rem", server.Url);
		}

		[Fact]
		public void CanParseADisplayNameWithoutAPort()
		{
			BuildServer server = BuildServer.BuildFromRemotingDisplayName("server");
            Assert.Equal("tcp://server:21234/CruiseManager.rem", server.Url);
		}

        [Fact(DisplayName = "Expected string in format server[:port]")]
        public void ThrowsWhenParsingAStringThatContainsMoreThanOneColon()
        {
            Assert.Throws<ApplicationException>(delegate { BuildServer.BuildFromRemotingDisplayName("tcp://server:123/blah.rem"); });
        }

        [Fact(DisplayName = "Port number must be an integer")]
        public void ThrowsWhenParsingAStringWithNonNumericPortNumber()
        {
            Assert.Throws<ApplicationException>(delegate { BuildServer.BuildFromRemotingDisplayName("server:xxx"); });
        }

        [Fact(DisplayName = "Extension transport must always define an extension name")]
        public void ThrowsWhenMissingExtension()
        {
            Assert.Throws<CCTrayLibException>(delegate { new BuildServer("http://test", BuildServerTransport.Extension, null, null); });
        }

        [Fact]
        public void SetExtensionName()
        {
            BuildServer newServer = new BuildServer();
            newServer.ExtensionName = "new extension";
            Assert.Equal("new extension", newServer.ExtensionName);
        }

        [Fact]
        public void SetExtensionSettings()
        {
            BuildServer newServer = new BuildServer();
            newServer.ExtensionSettings = "new extension";
            Assert.Equal("new extension", newServer.ExtensionSettings);
        }

        [Fact]
        public void SetTransport()
        {
            BuildServer newServer = new BuildServer();
            newServer.Transport = BuildServerTransport.Extension;
            Assert.Equal(BuildServerTransport.Extension, newServer.Transport);
        }
		
		[Fact]
		public void TwoBuildServersAreEqualIfTheirUrlsAreTheSame()
		{
			BuildServer one = new BuildServer("http://one");
			BuildServer anotherOne = new BuildServer("http://one");
			BuildServer two = new BuildServer("http://two");

            Assert.True(one.Equals(anotherOne));
            Assert.True(anotherOne.Equals(one));
            Assert.False(one.Equals(two));
            Assert.False(two.Equals(one));
		}

	}
}
