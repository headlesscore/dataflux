using System;
using System.Xml;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace ThoughtWorks.CruiseControl.UnitTests.UnitTestUtils
{
    public class XPathAssert
    {
        public static void Matches(XmlNode source, string xpath, string expectedValue)
        {
            XmlNode node = source.SelectSingleNode(xpath);
            ClassicAssert.IsNotNull(node, string.Format(System.Globalization.CultureInfo.CurrentCulture,"Expected to find match for xpath {0} in xml:\n {1}", xpath, source.OuterXml));
            ClassicAssert.AreEqual(node.InnerText, expectedValue, "Unexpected value for xpath " + xpath);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        // cannot just compare the xml string, since we correctly expect the string to vary based on the
        // timezone in which this code is executing
        public static void Matches(XmlNode source, string xpath, DateTime expectedDate)
        {
            Matches(source, xpath, XmlConvert.ToString(expectedDate, XmlDateTimeSerializationMode.Local));
        }

        public static XmlDocument LoadAsDocument(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc;
        }
    }
}
