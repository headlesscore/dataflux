using System.Xml;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
	
	public class XmlUtilTest : CustomAssertion
	{
		private static string TWO_SUCH_ELEMENTS = "two";
		private static string ONE_SUCH_ELEMENT = "one";
		private XmlDocument doc;
		private XmlElement elementOne;
		private XmlElement elementTwo;
		private XmlElement elementTwoAgain;
		
		// [SetUp]
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

		[Fact]
		public void GetFirstElement()
		{
			Assert.Equal(elementTwo, XmlUtil.GetFirstElement(doc, TWO_SUCH_ELEMENTS));
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public void GetSingleElement()
		{
            try
            {
                _ = XmlUtil.GetSingleElement(doc, ONE_SUCH_ELEMENT);
            } catch (System.Exception ex) {
                Assert.Fail(ex.Message);
            }

			Assert.Throws<CruiseControlException>(delegate { XmlUtil.GetSingleElement(doc, TWO_SUCH_ELEMENTS); });
		}

		[Fact]
		public void SelectValue()
		{
			XmlDocument document = XmlUtil.CreateDocument("<configuration><monkeys>bananas</monkeys></configuration>");
			Assert.Equal("bananas", XmlUtil.SelectValue(document, "/configuration/monkeys", "orangutan"));			
		}

		[Fact]
		public void SelectValueWithMissingValue()
		{
			XmlDocument document = XmlUtil.CreateDocument("<configuration><monkeys></monkeys></configuration>");
			Assert.Equal("orangutan", XmlUtil.SelectValue(document, "/configuration/monkeys", "orangutan"));			
		}

		[Fact]
		public void SelectValueWithMissingElement()
		{
			XmlDocument document = XmlUtil.CreateDocument("<configuration><monkeys></monkeys></configuration>");
			Assert.Equal("orangutan", XmlUtil.SelectValue(document, "/configuration/apes", "orangutan"));			
		}

		[Fact]
		public void SelectValueWithAttribute()
		{
			XmlDocument document = XmlUtil.CreateDocument("<configuration><monkeys part=\"brains\">booyah</monkeys></configuration>");
			Assert.Equal("brains", XmlUtil.SelectValue(document, "/configuration/monkeys/@part", "orangutan"));			
		}

		[Fact]
		public void SelectRequiredValue()
		{
			XmlDocument document = XmlUtil.CreateDocument("<configuration><martin>andersen</martin></configuration>");
			Assert.Equal("andersen", XmlUtil.SelectRequiredValue(document, "/configuration/martin"));			
		}

		[Fact]
		public void SelectRequiredValueWithMissingValue()
		{
			XmlDocument document = XmlUtil.CreateDocument("<configuration><martin></martin></configuration>");
            Assert.Throws<CruiseControlException>(delegate { XmlUtil.SelectRequiredValue(document, "/configuration/martin"); });
		}

		[Fact]
		public void SelectRequiredValueWithMissingElement()
		{
			XmlDocument document = XmlUtil.CreateDocument("<configuration><martin></martin></configuration>");
			Assert.Throws<CruiseControlException>(delegate { XmlUtil.SelectRequiredValue(document, "/configuration/larry"); });
		}

		[Fact]
		public void VerifyCDATAEncode()
		{
			string test = "a b <f>]]></a>";
			Assert.Equal("a b <f>] ]></a>", XmlUtil.EncodeCDATA(test));
		}

        [Fact]
        public void VerifyPCDATAEncodeEncodes()
        {
            string test = "a&b <c>-</d>";
            Assert.Equal("a&amp;b &lt;c&gt;&#x2d;&lt;/d&gt;", XmlUtil.EncodePCDATA(test));
        }

        [Fact]
        public void VerifyPCDATAEncodeDoesNotEncodeOthers()
        {
            string test = @"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890`~!@#$%^*()_+={[}]|\:;',.?/';" + '"';
            Assert.Equal(test, XmlUtil.EncodePCDATA(test));
        }
    }
}
