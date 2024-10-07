using System;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
	[TestFixture]
	public class HtmlExceptionFormatterTest : CustomAssertion
	{
		[Test]
		public void FormatShouldReplaceNewLinesWithBRTags()
		{
			HtmlExceptionFormatter formatter = new HtmlExceptionFormatter(new Exception("foo" + Environment.NewLine + "Bar"));
			string formattedString = formatter.ToString();
			ClassicAssert.IsTrue(formattedString.IndexOf(Environment.NewLine) == -1);
			ClassicAssert.AreEqual(1 + 1, CountOfStrings(formattedString, "<br/>"));
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

		private int CountOfStrings(string baseString, string stringToSearch)
		{
			int count = 0;
			int curPos = 0;
			while (true)
			{
				int offset = baseString.IndexOf(stringToSearch, curPos);
				if (offset == -1) break;
				curPos = offset + stringToSearch.Length;
				count++;
			}

			return count;
		}
	}
}
