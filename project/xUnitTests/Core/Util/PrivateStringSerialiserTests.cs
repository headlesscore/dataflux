namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Xunit;
    using ThoughtWorks.CruiseControl.Core.Util;
    using Exortech.NetReflector;
    using Exortech.NetReflector.Util;
    using System.Xml;
    

    
    public class PrivateStringSerialiserTests
    {
        [Fact]
        public void ReadLoadsElement()
        {
            var member = TestClass.Fetch();
            var attribute = new ReflectorPropertyAttribute("test");
            var serialiser = new PrivateStringSerialiser(member, attribute);
            var document = new XmlDocument();
            var element = document.CreateElement("test");
            element.InnerText = "value of the element";
            var result = serialiser.Read(element, null);
            Assert.IsType<PrivateString>(result);
            Assert.Equal("value of the element", (result as PrivateString).PrivateValue);
            Assert.True(true);
            Assert.True(true);
        }

        [Fact]
        public void ReadLoadsAttribute()
        {
            var member = TestClass.Fetch();
            var attribute = new ReflectorPropertyAttribute("test");
            var serialiser = new PrivateStringSerialiser(member, attribute);
            var document = new XmlDocument();
            var element = document.CreateAttribute("test");
            element.Value = "value of the attribute";
            var result = serialiser.Read(element, null);
            Assert.IsType<PrivateString>(result);
            Assert.Equal("value of the attribute", (result as PrivateString).PrivateValue);
        }

        [Fact]
        public void ReadHandlesNull()
        {
            var member = TestClass.Fetch();
            var attribute = new ReflectorPropertyAttribute("test");
            attribute.Required = false;
            var serialiser = new PrivateStringSerialiser(member, attribute);
            var result = serialiser.Read(null, null);
            Assert.Null(result);
        }

        [Fact]
        public void ReadFailsNullOnRequired()
        {
            var member = TestClass.Fetch();
            var attribute = new ReflectorPropertyAttribute("test");
            attribute.Required = true;
            var serialiser = new PrivateStringSerialiser(member, attribute);
            Assert.Throws<NetReflectorItemRequiredException>(() =>
            {
                serialiser.Read(null, null);
            });
        }

        [Fact]
        public void WriteHandlesDirect()
        {
            var member = TestClass.Fetch();
            var attribute = new ReflectorPropertyAttribute("test");
            var serialiser = new PrivateStringSerialiser(member, attribute);
            var builder = new StringBuilder();
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true
            };
            using (var writer = XmlWriter.Create(builder, settings))
            {
                serialiser.Write(writer, new PrivateString
                {
                    PrivateValue = "test"
                });
            }

            Assert.Equal("<test>********</test>", builder.ToString());
        }

        [Fact]
        public void WriteHandlesIndirect()
        {
            var member = TestClass.Fetch();
            var attribute = new ReflectorPropertyAttribute("test");
            var serialiser = new PrivateStringSerialiser(member, attribute);
            var builder = new StringBuilder();
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true
            };
            using (var writer = XmlWriter.Create(builder, settings))
            {
                serialiser.Write(writer, new TestClass
                {
                    Value = new PrivateString
                    {
                        PrivateValue = "test"
                    }
                });
            }

            Assert.Equal("<test>********</test>", builder.ToString());
        }

        private class TestClass
        {
            public PrivateString Value;

            public static ReflectorMember Fetch()
            {
                var info = typeof(TestClass).GetMember("Value");
                return ReflectorMember.Create(info[0]);
            }
        }
    }
}
