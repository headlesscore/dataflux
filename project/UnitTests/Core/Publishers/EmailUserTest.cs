using System;
using Exortech.NetReflector;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Publishers;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Publishers
{
    [TestFixture]
    public class EmailUserTest
	{
        [Test]
        public void ShouldFailToReadWithoutAddress()
        {
            ClassicAssert.That(delegate { NetReflector.Read(@"<user name=""username""/>"); },
                        Throws.TypeOf<NetReflectorException>());
            ClassicAssert.That(delegate { NetReflector.Read(@"<user name=""username""/>"); },
                        Throws.TypeOf<NetReflectorException>());
        }

        [Test]
        public void ShouldFailToReadWithoutName()
        {
            ClassicAssert.That(delegate { NetReflector.Read(@"<user address=""UserName@example.com""/>"); },
                        Throws.TypeOf<NetReflectorException>().With.Message.EqualTo(
                            "Missing Xml node (name) for required member (ThoughtWorks.CruiseControl.Core.Publishers.EmailUser.Name)." + Environment.NewLine + "Xml: <user address=\"UserName@example.com\" />"));
        }

        [Test]
        public void ShouldReadFromMinimalXml()
        {
            EmailUser user = (EmailUser) NetReflector.Read(@"<user name=""username"" address=""UserName@example.com""/>");
            ClassicAssert.AreEqual("username", user.Name);
            ClassicAssert.AreEqual("UserName@example.com", user.Address);
            ClassicAssert.AreEqual(null, user.Group);
        }

        [Test]
        public void ShouldReadFromMaximalSimpleXml()
        {
            EmailUser user = (EmailUser)NetReflector.Read(@"<user name=""username"" address=""UserName@example.com"" group=""group1""/>");
            ClassicAssert.AreEqual("username", user.Name);
            ClassicAssert.AreEqual("UserName@example.com", user.Address);
            ClassicAssert.AreEqual("group1", user.Group);
        }

        [Test]
        public void ShouldReadFromMaximalComplexXml()
        {
            EmailUser user = (EmailUser)NetReflector.Read(
@"<user>
    <name>username</name>
    <address>UserName@example.com</address>
    <group>group1</group>
</user>
");
            ClassicAssert.AreEqual("username", user.Name);
            ClassicAssert.AreEqual("UserName@example.com", user.Address);
            ClassicAssert.AreEqual("group1", user.Group);
        }

	}
}
