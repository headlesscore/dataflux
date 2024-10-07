using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.Remote;
using System.Diagnostics;
using NUnit.Framework.Legacy;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
	[TestFixture]
	public class StringUtilTest : CustomAssertion
	{
		[Test]
		public void TestEqualsIgnoreCase()
		{
			const string lower = "abcde";
			const string upper = "ABCDE";
			const string mixed = "aBcDe";
			const string mixed2 = "AbCdE";

			ClassicAssert.IsTrue(StringUtil.EqualsIgnoreCase(lower, upper));
			ClassicAssert.IsTrue(StringUtil.EqualsIgnoreCase(lower, mixed));
			ClassicAssert.IsTrue(StringUtil.EqualsIgnoreCase(lower, mixed2));
			ClassicAssert.IsTrue(StringUtil.EqualsIgnoreCase(upper, mixed));
			ClassicAssert.IsTrue(StringUtil.EqualsIgnoreCase(upper, mixed2));
		}

		[Test]
		public void TestIsWhitespace()
		{
			ClassicAssert.IsTrue(StringUtil.IsWhitespace(null));
			ClassicAssert.IsTrue(StringUtil.IsWhitespace(string.Empty));
            ClassicAssert.IsTrue(StringUtil.IsWhitespace(" "));
            ClassicAssert.IsTrue(StringUtil.IsWhitespace(Environment.NewLine));
            ClassicAssert.IsTrue(StringUtil.IsWhitespace("\t\r\v "));
            ClassicAssert.IsFalse(StringUtil.IsWhitespace("foo"));
            ClassicAssert.IsFalse(StringUtil.IsWhitespace("\t\r\v foo \t\r\v "));
		}

		[Test]
		public void TestGenerateHashCode()
		{
			const string a = "a";
			const string b = "b";
			ClassicAssert.AreEqual(
				a.GetHashCode() + b.GetHashCode(), StringUtil.GenerateHashCode(a, b));

			ClassicAssert.AreEqual(
				a.GetHashCode() + b.GetHashCode(), StringUtil.GenerateHashCode(a, b, null));
		}

		[Test]
		public void TestLastWord()
		{			
			string s = "this is a sentence without punctuation\n";
			ClassicAssert.AreEqual("punctuation", StringUtil.LastWord(s));
			
			s = "this is a sentence with punctuation.\n";
			ClassicAssert.AreEqual("punctuation", StringUtil.LastWord(s));

			s = "thisisoneword";
			ClassicAssert.AreEqual("thisisoneword", StringUtil.LastWord(s));

			s = "";
			ClassicAssert.AreEqual(String.Empty, StringUtil.LastWord(s));
			
			s = null;
			ClassicAssert.IsNull(StringUtil.LastWord(s));
		}

		[Test]
		public void TestLastWord_withSeps()
		{
			string s = "this#is$my%crazy*sentence!!!";
			ClassicAssert.AreEqual("sentence", StringUtil.LastWord(s, "#$%*!"));

			s = "!@#$%";
			ClassicAssert.AreEqual(String.Empty, StringUtil.LastWord(s, s));
		}

		[Test]
		public void TestStrip()
		{
			string input = "strip the word monkey and chinchilla or there's trouble";
			string actual = StringUtil.Strip(input, "monkey ", "chinchilla ");
			ClassicAssert.AreEqual("strip the word and or there's trouble", actual);

			input = "hey la monkey monkey chinchilla banana";
			actual = StringUtil.Strip(input, "monkey ", "chinchilla ");
			ClassicAssert.AreEqual("hey la banana", actual);
		}

		[Test]
		public void TestStripQuotes()
		{
			const string input = "\"C:\foo\"";
			string actual = StringUtil.StripQuotes(input);

			ClassicAssert.AreEqual("C:\foo", actual);			
		}

        [Test]
        public void TestRemoveInvalidCharactersFromFileName()
        {
            const string BadFileName = "Go Stand ? in the <*/:*?> corner.txt";
            string actual = StringUtil.RemoveInvalidCharactersFromFileName(BadFileName);

            ClassicAssert.AreEqual("Go Stand  in the  corner.txt", actual);
        }


		[Test]
		public void TestRemoveNulls()
		{
			ClassicAssert.AreEqual(StringUtil.RemoveNulls("\0\0hello"), "hello");
			ClassicAssert.AreEqual(StringUtil.RemoveNulls("\0\0hello\0\0"), "hello");
		}

		[Test]
		public void TestAutoDoubleQuoteString()
		{
			const string nonQuotedString = "foo";
			const string nonQuotedStringWithSpaces = "f o o";
			const string quotedString = "\"foo\"";
			const string quotedStringWithSpaces = "\"f o o\"";			
			
			ClassicAssert.AreEqual(StringUtil.AutoDoubleQuoteString(nonQuotedString), nonQuotedString);
			ClassicAssert.AreEqual(StringUtil.AutoDoubleQuoteString(quotedString), quotedString);
			ClassicAssert.AreEqual(StringUtil.AutoDoubleQuoteString(nonQuotedStringWithSpaces), quotedStringWithSpaces);
			ClassicAssert.AreEqual(StringUtil.AutoDoubleQuoteString(quotedStringWithSpaces), quotedStringWithSpaces);
		}

		[Test]
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
                ClassicAssert.AreEqual(test[1], StringUtil.StripThenEncodeParameterArgument(test[0]));
			}
		}

		[Test]
		public void TestRemoveTrailingPathDelimiter()
		{			
			const string actual = "foo";
			string trailingSeparator = "foo" + Path.DirectorySeparatorChar;
			string trailingSeparator2 = "foo" + Path.DirectorySeparatorChar + Path.DirectorySeparatorChar;

			ClassicAssert.AreEqual(StringUtil.RemoveTrailingPathDelimeter(trailingSeparator), actual);
			ClassicAssert.AreEqual(StringUtil.RemoveTrailingPathDelimeter(trailingSeparator2), actual);
		}

		[Test]
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
			
			ClassicAssert.AreEqual(StringUtil.IntegrationPropertyToString(integer), integerString);
			ClassicAssert.AreEqual(StringUtil.IntegrationPropertyToString(integerString), integerString);
			ClassicAssert.AreEqual(StringUtil.IntegrationPropertyToString(buildCondition), buildCondition.ToString());
			ClassicAssert.AreEqual(StringUtil.IntegrationPropertyToString(integrationStatus), integrationStatus.ToString());
			ClassicAssert.AreEqual(StringUtil.IntegrationPropertyToString(arrayList), defaultConvertedArrayList);

			ClassicAssert.AreEqual(StringUtil.IntegrationPropertyToString(integer, customDelimiter), integerString);
			ClassicAssert.AreEqual(StringUtil.IntegrationPropertyToString(integerString, customDelimiter), integerString);
			ClassicAssert.AreEqual(StringUtil.IntegrationPropertyToString(buildCondition, customDelimiter), buildCondition.ToString());
			ClassicAssert.AreEqual(StringUtil.IntegrationPropertyToString(integrationStatus, customDelimiter), integrationStatus.ToString());
			ClassicAssert.AreEqual(StringUtil.IntegrationPropertyToString(arrayList, customDelimiter), customConvertedArrayList);

			ClassicAssert.AreEqual(StringUtil.IntegrationPropertyToString(null), null);

			ArrayList arrayList2 = new ArrayList();
			arrayList2.Add("foo");
			ClassicAssert.AreEqual(StringUtil.IntegrationPropertyToString(arrayList2), "foo");
			ClassicAssert.AreEqual(StringUtil.IntegrationPropertyToString(arrayList2, customDelimiter), "foo");
		}

		[Test]
		public void TestIntegrationPropertyToStringWithUnsupportedType()
		{
			ClassicAssert.That(delegate { StringUtil.IntegrationPropertyToString(new object()); },
                        Throws.TypeOf<ArgumentException>().With.Property("ParamName").EqualTo("value"));
			ClassicAssert.That(delegate { StringUtil.IntegrationPropertyToString(new object(), "-"); },
                        Throws.TypeOf<ArgumentException>().With.Property("ParamName").EqualTo("value"));
		}

		[Test]
		public void TestMakeBuildResult()
		{
			ClassicAssert.AreEqual(Environment.NewLine + "<buildresults>" + Environment.NewLine + "  <message>"
				+ "foo" + "</message>" + Environment.NewLine + "</buildresults>"
				+ Environment.NewLine, StringUtil.MakeBuildResult("foo", ""));
            ClassicAssert.AreEqual(Environment.NewLine + "<buildresults>" + Environment.NewLine + "  <message level=\"Error\">"
				+ "foo" + "</message>" + Environment.NewLine + "</buildresults>"
				+ Environment.NewLine, StringUtil.MakeBuildResult("foo", "Error"));
			ClassicAssert.AreEqual("", StringUtil.MakeBuildResult("", ""));
		}

		[Test]
		public void TestMakeBuildResultThrowsArgumentNullException()
		{
            ClassicAssert.That(delegate { StringUtil.MakeBuildResult(null, ""); },
                        Throws.TypeOf<ArgumentNullException>().With.Property("ParamName").EqualTo("input"));
		}

		[Test]
		public void TestArrayToNewLineSeparatedString()
		{
			ClassicAssert.AreEqual("foo", StringUtil.ArrayToNewLineSeparatedString(new string[] { "foo" }));
			ClassicAssert.AreEqual("foo" + Environment.NewLine + "bar", StringUtil.ArrayToNewLineSeparatedString(new string[] {"foo", "bar"}));
			ClassicAssert.AreEqual("", StringUtil.ArrayToNewLineSeparatedString(new string[0]));
			ClassicAssert.AreEqual("", StringUtil.ArrayToNewLineSeparatedString(new string[1] { "" }));
			ClassicAssert.AreEqual("", StringUtil.ArrayToNewLineSeparatedString(new string[1] { null }));
		}

		[Test]
		public void TestNewLineSeparatedStringToArray()
		{
			ClassicAssert.AreEqual(new string[] { "foo" }, StringUtil.NewLineSeparatedStringToArray("foo"));
			ClassicAssert.AreEqual(new string[] { "foo", "bar" }, StringUtil.NewLineSeparatedStringToArray("foo" + Environment.NewLine + "bar"));
			ClassicAssert.AreEqual(new string[0], StringUtil.NewLineSeparatedStringToArray(""));
			ClassicAssert.AreEqual(new string[0], StringUtil.NewLineSeparatedStringToArray(null));
			ClassicAssert.AreEqual(new string[1] { "" }, StringUtil.NewLineSeparatedStringToArray(Environment.NewLine));
		}

        [Test]
        public void UrlEncodeNameCorrectlyEncodesNames()
        {
            ClassicAssert.AreEqual("cc.net%20rocks", StringUtil.UrlEncodeName("cc.net rocks"));
            ClassicAssert.AreEqual("http%3a%2f%2fserver%2fcc%20net", StringUtil.UrlEncodeName("http://server/cc net"));
            ClassicAssert.AreEqual("abcdefHIJKLMNopqrstUVWXYZ0123456789-_.~", StringUtil.UrlEncodeName("abcdefHIJKLMNopqrstUVWXYZ0123456789-_.~"));
            ClassicAssert.AreEqual("%60%21%40%23%24%25%5e%26%2a%28%29%3d%2b%3c%3e%3f%2f%5c%7c%7b%7d%5b%5d%3a%3b%22%27", StringUtil.UrlEncodeName("`!@#$%^&*()=+<>?/\\|{}[]:;\"'"));
            ClassicAssert.AreEqual("\x100\x200\x300\x400", StringUtil.UrlEncodeName("\x100\x200\x300\x400"));
        }
	}
}
