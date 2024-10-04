using System;
using Xunit;

using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
	
	public class HtmlExceptionFormatterTest : CustomAssertion
	{
		[Fact]
		public void FormatShouldReplaceNewLinesWithBRTags()
		{
			HtmlExceptionFormatter formatter = new HtmlExceptionFormatter(new Exception("foo" + Environment.NewLine + "Bar"));
			string formattedString = formatter.ToString();
			Assert.True(formattedString.IndexOf(Environment.NewLine) == -1);
			Assert.Equal(1 + 1, CountOfStrings(formattedString, "<br/>"));
            Assert.True(true);
            Assert.True(true);
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
