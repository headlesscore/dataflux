using System;
using Exortech.NetReflector;
using Xunit;

using ThoughtWorks.CruiseControl.Core.Publishers;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Publishers
{
    
    public class EmailUserTest
	{
        [Fact]
        public void ShouldFailToReadWithoutAddress()
        {
            Assert.Throws<NetReflectorException>(delegate { NetReflector.Read(@"<user name=""username""/>"); });
            Assert.Throws<NetReflectorException>(delegate { NetReflector.Read(@"<user name=""username""/>"); });
        }

        [Fact]
        public void ShouldFailToReadWithoutName()
        {
            Assert.Equal("Missing Xml node (name) for required member (ThoughtWorks.CruiseControl.Core.Publishers.EmailUser.Name)." + Environment.NewLine + "Xml: <user address=\"UserName@example.com\" />",
                Assert.Throws<NetReflectorException>(()=> { NetReflector.Read(@"<user address=""UserName@example.com""/>"); }).Message);
        }

        [Fact]
        public void ShouldReadFromMinimalXml()
        {
            EmailUser user = (EmailUser) NetReflector.Read(@"<user name=""username"" address=""UserName@example.com""/>");
            Assert.Equal("username", user.Name);
            Assert.Equal("UserName@example.com", user.Address);
            Assert.Equal(null, user.Group);
        }

        [Fact]
        public void ShouldReadFromMaximalSimpleXml()
        {
            EmailUser user = (EmailUser)NetReflector.Read(@"<user name=""username"" address=""UserName@example.com"" group=""group1""/>");
            Assert.Equal("username", user.Name);
            Assert.Equal("UserName@example.com", user.Address);
            Assert.Equal("group1", user.Group);
        }

        [Fact]
        public void ShouldReadFromMaximalComplexXml()
        {
            EmailUser user = (EmailUser)NetReflector.Read(
@"<user>
    <name>username</name>
    <address>UserName@example.com</address>
    <group>group1</group>
</user>
");
            Assert.Equal("username", user.Name);
            Assert.Equal("UserName@example.com", user.Address);
            Assert.Equal("group1", user.Group);
        }

	}
}
