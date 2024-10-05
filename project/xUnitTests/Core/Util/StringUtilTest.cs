using System;
using System.Collections;
using System.IO;
using Xunit;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.Remote;
using System.Diagnostics;


namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
	
	public class StringUtilTest : CustomAssertion
	{
		[Fact]
		public void TestEqualsIgnoreCase()
		{
			const string lower = "abcde";
			const string upper = "ABCDE";
			const string mixed = "aBcDe";
			const string mixed2 = "AbCdE";

			Assert.True(StringUtil.EqualsIgnoreCase(lower, upper));
			Assert.True(StringUtil.EqualsIgnoreCase(lower, mixed));
			Assert.True(StringUtil.EqualsIgnoreCase(lower, mixed2));
			Assert.True(StringUtil.EqualsIgnoreCase(upper, mixed));
			Assert.True(StringUtil.EqualsIgnoreCase(upper, mixed2));
		}

		[Fact]
		public void TestIsWhitespace()
		{
			Assert.True(StringUtil.IsWhitespace(null));
			Assert.True(StringUtil.IsWhitespace(string.Empty));
            Assert.True(StringUtil.IsWhitespace(" "));
            Assert.True(StringUtil.IsWhitespace(Environment.NewLine));
            Assert.True(StringUtil.IsWhitespace("\t\r\v "));
            Assert.False(StringUtil.IsWhitespace("foo"));
            Assert.False(StringUtil.IsWhitespace("\t\r\v foo \t\r\v "));
		}

		[Fact]
		public void TestGenerateHashCode()
		{
			const string a = "a";
			const string b = "b";
			Assert.Equal(
				a.GetHashCode() + b.GetHashCode(), StringUtil.GenerateHashCode(a, b));

			Assert.Equal(
				a.GetHashCode() + b.GetHashCode(), StringUtil.GenerateHashCode(a, b, null));
		}

		[Fact]
		public void TestLastWord()
		{			
			string s = "this is a sentence without punctuation\n";
			Assert.Equal("punctuation", StringUtil.LastWord(s));
			
			s = "this is a sentence with punctuation.\n";
			Assert.Equal("punctuation", StringUtil.LastWord(s));

			s = "thisisoneword";
			Assert.Equal("thisisoneword", StringUtil.LastWord(s));

			s = "";
			Assert.Equal(String.Empty, StringUtil.LastWord(s));
			
			s = null;
			Assert.Null(StringUtil.LastWord(s));
		}

		[Fact]
		public void TestLastWord_withSeps()
		{
			string s = "this#is$my%crazy*sentence!!!";
			Assert.Equal("sentence", StringUtil.LastWord(s, "#$%*!"));

			s = "!@#$%";
			Assert.Equal(String.Empty, StringUtil.LastWord(s, s));
		}

		[Fact]
		public void TestStrip()
		{
			string input = "strip the word monkey and chinchilla or there's trouble";
			string actual = StringUtil.Strip(input, "monkey ", "chinchilla ");
			Assert.Equal("strip the word and or there's trouble", actual);

			input = "hey la monkey monkey chinchilla banana";
			actual = StringUtil.Strip(input, "monkey ", "chinchilla ");
			Assert.Equal("hey la banana", actual);
		}

		[Fact]
		public void TestStripQuotes()
		{
			const string input = "\"C:\foo\"";
			string actual = StringUtil.StripQuotes(input);

			Assert.Equal("C:\foo", actual);			
		}

        [Fact]
        public void TestRemoveInvalidCharactersFromFileName()
        {
            const string BadFileName = "Go Stand ? in the <*/:*?> corner.txt";
            string actual = StringUtil.RemoveInvalidCharactersFromFileName(BadFileName);

            Assert.Equal("Go Stand  in the  corner.txt", actual);
        }


		[Fact]
		public void TestRemoveNulls()
		{
			Assert.Equal(StringUtil.RemoveNulls("\0\0hello"), "hello");
			Assert.Equal(StringUtil.RemoveNulls("\0\0hello\0\0"), "hello");
		}

		[Fact]
		public void TestAutoDoubleQuoteString()
		{
			const string nonQuotedString = "foo";
			const string nonQuotedStringWithSpaces = "f o o";
			const string quotedString = "\"foo\"";
			const string quotedStringWithSpaces = "\"f o o\"";			
			
			Assert.Equal(StringUtil.AutoDoubleQuoteString(nonQuotedString), nonQuotedString);
			Assert.Equal(StringUtil.AutoDoubleQuoteString(quotedString), quotedString);
			Assert.Equal(StringUtil.AutoDoubleQuoteString(nonQuotedStringWithSpaces), quotedStringWithSpaces);
			Assert.Equal(StringUtil.AutoDoubleQuoteString(quotedStringWithSpaces), quotedStringWithSpaces);
		}

		[Fact]
		public void TestStripThenEncodeParameterArgument()
		{
			string[][] tests = new string[][] {
				new string[] { "foo", "foo"}
				,
				new string[] { "\"foo\"", "foo"}
				,
				new string[] { "\"foo", "foo"}
				,
				new string[] { "f o o", "\"f o o\""}
				,
				new string[] { @"\foo\", @"\foo\"}
				,
				new string[] { @"\\fo\o\\\", @"\\fo\o\\\"}
				,
				new string[] { " ", "\" \""}
				,
				new string[] { "fo\"o", "fo\\\"o"}
				,
				new string[] { "fo\\\"o", "fo\\\\\\\"o"}
				,
				new string[] { "foo \"something\" bar", "\"foo \\\"something\\\" bar\""}
				,
				new string[] { "fo o\\", "\"fo o\\\\\""}
				,
				new string[] { "foo\\ ", "\"foo\\ \""}
				,
				new string[] { "foo,bar", "\"foo,bar\""}
				,
				new string[] { "foo \"something\" bar,baz", "\"foo \\\"something\\\" bar,baz\""}
			};

			foreach (string[] test in tests)
			{
                Assert.Equal(test[1], StringUtil.StripThenEncodeParameterArgument(test[0]));
			}
		}

		[Fact]
		public void TestRemoveTrailingPathDelimiter()
		{			
			const string actual = "foo";
			string trailingSeparator = "foo" + Path.DirectorySeparatorChar;
			string trailingSeparator2 = "foo" + Path.DirectorySeparatorChar + Path.DirectorySeparatorChar;

			Assert.Equal(StringUtil.RemoveTrailingPathDelimeter(trailingSeparator), actual);
			Assert.Equal(StringUtil.RemoveTrailingPathDelimeter(trailingSeparator2), actual);
		}

		[Fact]
		public void TestIntegrationPropertyToString()
		{
			const int integer = 5;
			string integerString = integer.ToString();			
			const BuildCondition buildCondition = BuildCondition.ForceBuild;			
			const IntegrationStatus integrationStatus = IntegrationStatus.Success;			
			
			ArrayList arrayList = new ArrayList();
			arrayList.Add("foo");
			arrayList.Add("5");
			arrayList.Add("bar");			
			
			const string customDelimiter = "-";
			const string defaultConvertedArrayList = "\"foo" + StringUtil.DEFAULT_DELIMITER + "5" + StringUtil.DEFAULT_DELIMITER + "bar\"";
			const string customConvertedArrayList = "\"foo" + customDelimiter + "5" + customDelimiter + "bar\"";
			
			Assert.Equal(StringUtil.IntegrationPropertyToString(integer), integerString);
			Assert.Equal(StringUtil.IntegrationPropertyToString(integerString), integerString);
			Assert.Equal(StringUtil.IntegrationPropertyToString(buildCondition), buildCondition.ToString());
			Assert.Equal(StringUtil.IntegrationPropertyToString(integrationStatus), integrationStatus.ToString());
			Assert.Equal(StringUtil.IntegrationPropertyToString(arrayList), defaultConvertedArrayList);

			Assert.Equal(StringUtil.IntegrationPropertyToString(integer, customDelimiter), integerString);
			Assert.Equal(StringUtil.IntegrationPropertyToString(integerString, customDelimiter), integerString);
			Assert.Equal(StringUtil.IntegrationPropertyToString(buildCondition, customDelimiter), buildCondition.ToString());
			Assert.Equal(StringUtil.IntegrationPropertyToString(integrationStatus, customDelimiter), integrationStatus.ToString());
			Assert.Equal(StringUtil.IntegrationPropertyToString(arrayList, customDelimiter), customConvertedArrayList);

			Assert.Equal(StringUtil.IntegrationPropertyToString(null), null);

			ArrayList arrayList2 = new ArrayList();
			arrayList2.Add("foo");
			Assert.Equal(StringUtil.IntegrationPropertyToString(arrayList2), "foo");
			Assert.Equal(StringUtil.IntegrationPropertyToString(arrayList2, customDelimiter), "foo");
		}

		[Fact]
		public void TestIntegrationPropertyToStringWithUnsupportedType()
		{
            Assert.Equal("value",
                Assert.Throws<ArgumentException>(()=> { StringUtil.IntegrationPropertyToString(new object()); }).ParamName);

            Assert.Equal("value",
                Assert.Throws<ArgumentException>(()=> { StringUtil.IntegrationPropertyToString(new object(), "-"); }).ParamName);

		}

		[Fact]
		public void TestMakeBuildResult()
		{
			Assert.Equal(Environment.NewLine + "<buildresults>" + Environment.NewLine + "  <message>"
				+ "foo" + "</message>" + Environment.NewLine + "</buildresults>"
				+ Environment.NewLine, StringUtil.MakeBuildResult("foo", ""));
            Assert.Equal(Environment.NewLine + "<buildresults>" + Environment.NewLine + "  <message level=\"Error\">"
				+ "foo" + "</message>" + Environment.NewLine + "</buildresults>"
				+ Environment.NewLine, StringUtil.MakeBuildResult("foo", "Error"));
			Assert.Equal("", StringUtil.MakeBuildResult("", ""));
		}

		[Fact]
		public void TestMakeBuildResultThrowsArgumentNullException()
		{
            Assert.Equal("input",
                Assert.Throws<ArgumentNullException>(()=>{ StringUtil.MakeBuildResult(null, ""); }).Message);
		}

		[Fact]
		public void TestArrayToNewLineSeparatedString()
		{
			Assert.Equal("foo", StringUtil.ArrayToNewLineSeparatedString(new string[] { "foo" }));
			Assert.Equal("foo" + Environment.NewLine + "bar", StringUtil.ArrayToNewLineSeparatedString(new string[] {"foo", "bar"}));
			Assert.Equal("", StringUtil.ArrayToNewLineSeparatedString(new string[0]));
			Assert.Equal("", StringUtil.ArrayToNewLineSeparatedString(new string[1] { "" }));
			Assert.Equal("", StringUtil.ArrayToNewLineSeparatedString(new string[1] { null }));
		}

		[Fact]
		public void TestNewLineSeparatedStringToArray()
		{
			Assert.Equal(new string[] { "foo" }, StringUtil.NewLineSeparatedStringToArray("foo"));
			Assert.Equal(new string[] { "foo", "bar" }, StringUtil.NewLineSeparatedStringToArray("foo" + Environment.NewLine + "bar"));
			Assert.Equal(new string[0], StringUtil.NewLineSeparatedStringToArray(""));
			Assert.Equal(new string[0], StringUtil.NewLineSeparatedStringToArray(null));
			Assert.Equal(new string[1] { "" }, StringUtil.NewLineSeparatedStringToArray(Environment.NewLine));
		}

        [Fact]
        public void UrlEncodeNameCorrectlyEncodesNames()
        {
            Assert.Equal("cc.net%20rocks", StringUtil.UrlEncodeName("cc.net rocks"));
            Assert.Equal("http%3a%2f%2fserver%2fcc%20net", StringUtil.UrlEncodeName("http://server/cc net"));
            Assert.Equal("abcdefHIJKLMNopqrstUVWXYZ0123456789-_.~", StringUtil.UrlEncodeName("abcdefHIJKLMNopqrstUVWXYZ0123456789-_.~"));
            Assert.Equal("%60%21%40%23%24%25%5e%26%2a%28%29%3d%2b%3c%3e%3f%2f%5c%7c%7b%7d%5b%5d%3a%3b%22%27", StringUtil.UrlEncodeName("`!@#$%^&*()=+<>?/\\|{}[]:;\"'"));
            Assert.Equal("\x100\x200\x300\x400", StringUtil.UrlEncodeName("\x100\x200\x300\x400"));
        }
	}
}
