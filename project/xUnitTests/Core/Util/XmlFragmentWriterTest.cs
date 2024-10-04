using System;
using System.IO;
using System.Text;
using System.Xml;
using Xunit;

using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
	
	public class XmlFragmentWriterTest
	{
		private TextWriter baseWriter;
		private XmlFragmentWriter writer;

        // [TearDown]
        protected void TearDown()
        {
            if(!(baseWriter is null))
            {
                (baseWriter as IDisposable)?.Dispose();
            }
            if (!(writer is null))
            {
                (writer as IDisposable)?.Dispose();
            }
        }

		// [SetUp]
		public void CreateWriter()
		{
			baseWriter = new StringWriter();
			writer = new XmlFragmentWriter(baseWriter);
		}

		[Fact]
		public void ShouldWriteValidXmlContentToUnderlyingWriter()
		{
			writer.WriteNode("<foo><bar /></foo>");
			Assert.Equal("<foo><bar /></foo>", baseWriter.ToString());
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public void ShouldWriteInvalidXmlContentToUnderlyingWriterAsCData()
		{
			writer.WriteNode("<foo><bar></foo></bar>");
			Assert.Equal("<![CDATA[<foo><bar></foo></bar>]]>", baseWriter.ToString());
		}

		[Fact]
		public void ShouldClearBufferIfInvalidXmlContentWrittenTwice()
		{
			writer.WriteNode("<foo><bar></foo></bar>");
			writer.WriteNode("<foo><bar/></foo>");
			Assert.Equal("<![CDATA[<foo><bar></foo></bar>]]><foo><bar /></foo>", baseWriter.ToString());
		}

		[Fact]
		public void ShouldBeAbleToWriteWhenFragmentIsSurroundedByText()
		{
			writer.WriteNode("outside<foo/>text");
			Assert.Equal("outside<foo />text", baseWriter.ToString());
		}

		[Fact]
		public void ShouldBeAbleToWriteWhenFragmentHasMultipleRootElements()
		{
			writer.WriteNode("<foo/><bar/>");
			Assert.Equal("<foo /><bar />", baseWriter.ToString());
		}

		[Fact]
		public void ShouldIgnoreXmlDeclaration()
		{
			writer.WriteNode(@"<?xml version=""1.0"" encoding=""utf-16""?><foo/><bar/>");
			Assert.Equal("<foo /><bar />", baseWriter.ToString());
		}

		[Fact]
		public void WriteOutputWithInvalidXmlContainingCDATACloseCommand()
		{
			writer.WriteNode("<tag><c>]]></tag>");
			Assert.Equal("<![CDATA[<tag><c>] ]></tag>]]>", baseWriter.ToString());
		}

		[Fact]
		public void IndentOutputWhenFormattingIsSpecified()
		{
			writer.Formatting = Formatting.Indented;
			writer.WriteNode("<foo><bar/></foo>");
			Assert.Equal(@"<foo>
  <bar />
</foo>", baseWriter.ToString());
		}

		[Fact]
		public void WriteTextContainingMalformedXmlElements()
		{
			string text = @"log4net:ERROR XmlConfigurator: Failed to find configuration section
'log4net' in the application's .config file. Check your .config file for the

<log4net> and <configSections> elements. The configuration section should
look like: <section name=""log4net""
type=""log4net.Config.Log4NetConfigurationSectionHandler,log4net"" />";
			writer.WriteNode(text);
			Assert.Equal(string.Format(System.Globalization.CultureInfo.CurrentCulture,"<![CDATA[{0}]]>", text), baseWriter.ToString());
		}

		[Fact]
		public void XmlWithoutClosingElementShouldEncloseInCDATA()
		{
			string text = "<a><b><c/></b>";
			writer.WriteNode(text);
			Assert.Equal(string.Format(System.Globalization.CultureInfo.CurrentCulture,"<![CDATA[{0}]]>", text), baseWriter.ToString());
		}

		[Fact]
		public void UnclosedXmlFragmentEndingInCarriageReturnShouldEncloseInCDATATag()
		{
			string xml = @"<a>
";
			writer.WriteNode(xml);
			Assert.Equal(@"<![CDATA[<a>
]]>", baseWriter.ToString());
		}

		[Fact]
		public void ShouldStripIllegalCharacters()
		{
			writer.WriteNode(string.Format(System.Globalization.CultureInfo.CurrentCulture,"<foo>{0}</foo>", IllegalCharacters()));
			Assert.Equal("<foo>\t\n\n</foo>", baseWriter.ToString());
		}

		[Fact]
		public void ShouldStripIllegalCharactersFromCDATABlock()
		{
			writer.WriteNode("a <> b " + IllegalCharacters());
			Assert.Equal("<![CDATA[a <> b \t\n\r]]>", baseWriter.ToString());
		}
        [Fact(Skip = "")]
        //[Test, Ignore("come back to this.")]
		public void ShouldIgnoreDTDEntities()
		{
			string xml = @"<?xml version=""1.0""?>
<!DOCTYPE Report
[
<!ELEMENT Report (General ,(Doc|BPT)) >
<!ATTLIST Report ver CDATA #REQUIRED tmZone CDATA #REQUIRED>

<!ELEMENT General ( DocLocation ) >
<!ATTLIST General productName CDATA #REQUIRED productVer CDATA #REQUIRED os CDATA #REQUIRED host CDATA #REQUIRED>
]
>
<Report ver=""2.0"" tmZone=""Pacific Standard Time"" />";
			writer.WriteNode(xml);
			Assert.Equal(@"<Report ver=""2.0"" tmZone=""Pacific Standard Time"" />", baseWriter.ToString());
		}
		
		private string IllegalCharacters()
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < 31; i++)
			{
				builder.Append((char) i, 1);
			}
			return builder.ToString();
		}
	}
}
