namespace ThoughtWorks.CruiseControl.UnitTests.Remote
{
    using System.Xml;
    using NUnit.Framework;
    using ThoughtWorks.CruiseControl.Remote;
    using Exortech.NetReflector;
    using System.IO;
    using System.Text;
    using NUnit.Framework.Legacy;

    [TestFixture]
    public class NameValuePairSerialiserTests
    {
        #region Tests
        #region Read() tests
        [Test]
        public void ReadLoadsASingleInstance()
        {
            var serialiser = new NameValuePairSerialiser(null, null, false);
            var document = new XmlDocument();
            document.LoadXml("<value name=\"a name\">the value</value>");
            var value = serialiser.Read(document.DocumentElement, null);
            ClassicAssert.IsInstanceOf<NameValuePair>(value);
            var actualValue = value as NameValuePair;
            ClassicAssert.AreEqual("a name", actualValue.Name);
            ClassicAssert.AreEqual("the value", actualValue.Value);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void ReadLoadsAList()
        {
            var serialiser = new NameValuePairSerialiser(null, null, true);
            var document = new XmlDocument();
            document.LoadXml("<list><value name=\"name1\">the value of 1</value><value name=\"name2\">the value of 2</value></list>");
            var value = serialiser.Read(document.DocumentElement, null);
            ClassicAssert.IsInstanceOf<NameValuePair[]>(value);
            var actualList = value as NameValuePair[];
            ClassicAssert.AreEqual(2, actualList.Length);
            ClassicAssert.AreEqual("name1", actualList[0].Name);
            ClassicAssert.AreEqual("the value of 1", actualList[0].Value);
            ClassicAssert.AreEqual("name2", actualList[1].Name);
            ClassicAssert.AreEqual("the value of 2", actualList[1].Value);
        }

        [Test]
        public void ReadDoesNotAllowChildNodesForASingleNode()
        {
            var serialiser = new NameValuePairSerialiser(null, null, false);
            var document = new XmlDocument();
            document.LoadXml("<value name=\"a name\"><child/></value>");
            ClassicAssert.Throws<NetReflectorException>(() =>
            {
                serialiser.Read(document.DocumentElement, null);
            });
        }

        [Test]
        public void ReadDoesNotAllowAttributesOnAList()
        {
            var serialiser = new NameValuePairSerialiser(null, null, true);
            var document = new XmlDocument();
            document.LoadXml("<list error=\"true\"><value name=\"name1\">the value of 1</value><value name=\"name2\">the value of 2</value></list>");
            ClassicAssert.Throws<NetReflectorException>(() =>
            {
                serialiser.Read(document.DocumentElement, null);
            });
        }

        [Test]
        public void ReadDoesNotAllowElementsNotCalledValueOnAList()
        {
            var serialiser = new NameValuePairSerialiser(null, null, true);
            var document = new XmlDocument();
            document.LoadXml("<list><good name=\"name1\">the value of 1</good><bad name=\"name2\">the value of 2</bad></list>");
            ClassicAssert.Throws<NetReflectorException>(() =>
            {
                serialiser.Read(document.DocumentElement, null);
            });
        }
        #endregion

        #region Write() tests
        [Test]
        public void WriteHandlesASingleItem()
        {
            var attribute = new ReflectorPropertyAttribute("value");
            var serialiser = new NameValuePairSerialiser(null, attribute, false);
            var value = new NameValuePair("a name", "the value");
            var builder = new StringBuilder();
            using (var stringWriter = new StringWriter(builder))
            {
                var settings = new XmlWriterSettings
                {
                    Indent = false,
                    NewLineOnAttributes = false,
                    OmitXmlDeclaration = true
                };
                using (var writer = XmlWriter.Create(stringWriter, settings))
                {
                    serialiser.Write(writer, value);
                }
            }

            ClassicAssert.AreEqual("<value name=\"a name\">the value</value>", builder.ToString());
        }

        [Test]
        public void WriteHandlesAList()
        {
            var attribute = new ReflectorPropertyAttribute("list");
            var serialiser = new NameValuePairSerialiser(null, attribute, true);
            var values = new NameValuePair[] 
            {
                new NameValuePair("name1", "the value of 1"),
                new NameValuePair("name2", "the value of 2")
            };
            var builder = new StringBuilder();
            using (var stringWriter = new StringWriter(builder))
            {
                var settings = new XmlWriterSettings
                {
                    Indent = false,
                    NewLineOnAttributes = false,
                    OmitXmlDeclaration = true
                };
                using (var writer = XmlWriter.Create(stringWriter, settings))
                {
                    serialiser.Write(writer, values);
                }
            }

            ClassicAssert.AreEqual(
                "<list><value name=\"name1\">the value of 1</value><value name=\"name2\">the value of 2</value></list>", 
                builder.ToString());
        }
        #endregion
        #endregion
    }
}
