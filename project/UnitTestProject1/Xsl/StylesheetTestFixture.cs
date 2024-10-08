using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Xsl
{
	public abstract class StylesheetTestFixture
	{
		private XslCompiledTransform transform;

		protected abstract string Stylesheet { get; }

		protected string LoadStylesheetAndTransformInput(string input)
		{
			XPathDocument document = new XPathDocument(new StringReader(input));
		    StringWriter output = new StringWriter();
		    transform.Transform(document, null, output);
            if (String.IsNullOrEmpty(output.ToString())) return string.Empty;

			XmlDocument outputDocument = new XmlDocument();
			outputDocument.Load(new StringReader(output.ToString()));
			return outputDocument.OuterXml;
		}

		protected static string WrapInBuildResultsElement(string xml)
		{
			return string.Format(@"<cruisecontrol><build><buildresults>{0}</buildresults></build></cruisecontrol>", xml);
		}

		[TestInitialize]
		public void LoadStyleSheet()
		{
            transform = new XslCompiledTransform(true);
            Environment.CurrentDirectory = "C:\\Users\\Administrator\\Desktop\\dataflux\\project";
            transform.Load(new SystemPath(Stylesheet).ToString());
		}
	}
}
