using System.Xml;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
	[TestFixture]
	public class XmlUtilTest : CustomAssertion
	{
		private static string TWO_SUCH_ELEMENTS = "two";
		private static string ONE_SUCH_ELEMENT = "one";
		private XmlDocument doc;
		private XmlElement elementOne;
		private XmlElement elementTwo;
		private XmlElement elementTwoAgain;
		
		[SetUp]
		public void SetUp()
		{
			doc = new XmlDocument();
			doc.AppendChild(doc.CreateElement("root"));

			elementOne = doc.CreateElement(ONE_SUCH_ELEMENT);
			elementTwo = doc.CreateElement(TWO_SUCH_ELEMENTS);
			elementTwoAgain = doc.CreateElement(TWO_SUCH_ELEMENTS);

			doc.DocumentElement.AppendChild(elementOne);
			doc.DocumentElement.AppendChild(elementTwo);
			doc.DocumentElement.AppendChild(elementTwoAgain);
		}

		[Test]
		public void GetFirstElement()
		{
			ClassicAssert.AreEqual(elementTwo, XmlUtil.GetFirstElement(doc, TWO_SUCH_ELEMENTS));
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

		[Test]
		public void GetSingleElement()
		{
			ClassicAssert.That(delegate { XmlUtil.GetSingleElement(doc, ONE_SUCH_ELEMENT); },
                        Throws.Nothing);
			ClassicAssert.That(delegate { XmlUtil.GetSingleElement(doc, TWO_SUCH_ELEMENTS); },
                        Throws.TypeOf<CruiseControlException>());
		}

		[Test]
		public void SelectValue()
		{
			XmlDocument document = XmlUtil.CreateDocument("<configuration><monkeys>bananas</monkeys></configuration>");
			ClassicAssert.AreEqual("bananas", XmlUtil.SelectValue(document, "/configuration/monkeys", "orangutan"));			
		}

		[Test]
		public void SelectValueWithMissingValue()
		{
			XmlDocument document = XmlUtil.CreateDocument("<configuration><monkeys></monkeys></configuration>");
			ClassicAssert.AreEqual("orangutan", XmlUtil.SelectValue(document, "/configuration/monkeys", "orangutan"));			
		}

		[Test]
		public void SelectValueWithMissingElement()
		{
			XmlDocument document = XmlUtil.CreateDocument("<configuration><monkeys></monkeys></configuration>");
			ClassicAssert.AreEqual("orangutan", XmlUtil.SelectValue(document, "/configuration/apes", "orangutan"));			
		}

		[Test]
		public void SelectValueWithAttribute()
		{
			XmlDocument document = XmlUtil.CreateDocument("<configuration><monkeys part=\"brains\">booyah</monkeys></configuration>");
			ClassicAssert.AreEqual("brains", XmlUtil.SelectValue(document, "/configuration/monkeys/@part", "orangutan"));			
		}

		[Test]
		public void SelectRequiredValue()
		{
			XmlDocument document = XmlUtil.CreateDocument("<configuration><martin>andersen</martin></configuration>");
			ClassicAssert.AreEqual("andersen", XmlUtil.SelectRequiredValue(document, "/configuration/martin"));			
		}

		[Test]
		public void SelectRequiredValueWithMissingValue()
		{
			XmlDocument document = XmlUtil.CreateDocument("<configuration><martin></martin></configuration>");
            ClassicAssert.That(delegate { XmlUtil.SelectRequiredValue(document, "/configuration/martin"); },
                        Throws.TypeOf<CruiseControlException>());
		}

		[Test]
		public void SelectRequiredValueWithMissingElement()
		{
			XmlDocument document = XmlUtil.CreateDocument("<configuration><martin></martin></configuration>");
			ClassicAssert.That(delegate { XmlUtil.SelectRequiredValue(document, "/configuration/larry"); },
                        Throws.TypeOf<CruiseControlException>());
		}

		[Test]
		public void VerifyCDATAEncode()
		{
			string test = "a b <f>]]></a>";
			ClassicAssert.AreEqual("a b <f>] ]></a>", XmlUtil.EncodeCDATA(test));
		}

        [Test]
        public void VerifyPCDATAEncodeEncodes()
        {
            string test = "a&b <c>-</d>";
            ClassicAssert.AreEqual("a&amp;b &lt;c&gt;&#x2d;&lt;/d&gt;", XmlUtil.EncodePCDATA(test));
        }

        [Test]
        public void VerifyPCDATAEncodeDoesNotEncodeOthers()
        {
            string test = @"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890`~!@#$%^*()_+={[}]|\:;',.?/';" + '"';
            ClassicAssert.AreEqual(test, XmlUtil.EncodePCDATA(test));
        }
    }
}
