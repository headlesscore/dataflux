using System;
using Xunit;

using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
	
	public class PathUtilTest
	{
		private static readonly string[] names = {
		                                         	"theName.dat", "TheName.dat",
		                                         	"/theFolder/theName.dat", "theFile.bin",
		                                         	"/theFolder/theFile.bin", "/TheFolder/theName.dat"
		                                         	, "/theFolder/theSubFolder/theName.dat",
		                                         	"/theFolder/theSubFolder/theSubSubFolder/theName.dat"
		                                         	,
		                                         	"/theFolder/theSubFolder/theSubFolder/theName.dat"
		                                         	,
		                                         	"/theFolder/theSubFolder/ThesubFolder/theName.dat"
		                                         	, "theName", "/theFolder/theSubFolder/theName",
		                                         	"\\theFolder\\theSubFolder/theName.dav",
		                                         	"\\theFolder/theName.dav"
		                                         };

		private static readonly int[] namesTokenLen = {1, 1, 2, 1, 2, 2, 3, 4, 4, 4, 1, 3, 3, 2};

		[Fact]
		public void TheFolderCaseInsensitive()
		{
			new TestCase("/thefolder/**/*", false, names,
			             new bool[]
			             	{
			             		false, false, true, false, true, true, true, true, true, true, false,
			             		true, true, true
			             	}).RunTest();
		}

		[Fact]
		public void AnySubFolder()
		{
			new TestCase("**/theSubFolder/**/*", true, names,
			             new bool[]
			             	{
			             		false, false, false, false, false, false, true, true, true, true,
			             		false, true, true, false
			             	}).RunTest();
		}

		[Fact]
		public void SingleCharacterInName()
		{
			new TestCase("**/the???e.da?", true, names,
			             new bool[]
			             	{
			             		true, false, true, false, false, true, true, true, true, true, false,
			             		false, true, true
			             	}).RunTest();
		}

		[Fact]
		public void CaseInsensitiveMatch()
		{
			Assert.True(PathUtils.Match("thename", "TheName", false));
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public void CaseInsensitiveMisMatch()
		{
			Assert.False(PathUtils.Match("thefile", "TheName", false));
		}

		[Fact]
		public void CaseInsensitiveStarMismatch()
		{
			Assert.False(PathUtils.Match("the*", "ratBone", false));
		}

		[Fact]
		public void MatchWithStarAtEnd()
		{
			Assert.True(PathUtils.Match("thename*", "thename", false));
		}

		[Fact]
		public void MisMatchWithMatchBeforeStar()
		{
			Assert.False(PathUtils.Match("thename*x", "thename", false));
		}

		[Fact]
		public void MisMatchWithStar()
		{
			Assert.False(PathUtils.Match("the*name", "thexfile", false));
		}

		[Fact]
		public void MatchWithExhaustingStrings()
		{
			Assert.True(PathUtils.Match("the*name*back", "thexnameisBack", false));
		}

		[Fact]
		public void MatchMiddleStar()
		{
			Assert.False(PathUtils.Match("t*a*x", "tx", false));
		}

		[Fact]
		public void MatchTwoStars()
		{
			Assert.True(PathUtils.Match("t**a*x", "txax", false));
		}

		[Fact]
		public void MisMatchWithTwoStarsExhaustedStrings()
		{
			Assert.False(PathUtils.Match("t*ay*x", "txax", false));
		}

		[Fact]
		public void MatchStarStarStarStar()
		{
			Assert.True(PathUtils.MatchPath("**/**/*", "/f/dat", false));
		}

		[Fact]
		public void MismatchExhaustedStrings()
		{
			Assert.False(PathUtils.Match("the*name*back", "thexnamisBack", false));
		}

		[Fact]
		public void MismatchStringExhausted()
		{
			Assert.False(PathUtils.MatchPath("/f/dat/**/x", "/f/dat", false));
		}

		[Fact]
		public void MismatchExhaustedStringNoStarAtEnd()
		{
			Assert.False(PathUtils.Match("the*name*backxx", "thexnameisBack", false));
		}

		[Fact]
		public void TokenizeTest()
		{
			for (int i = 0; i < names.Length; i++)
			{
				Assert.Equal(namesTokenLen[i], PathUtils.SplitPath(names[i]).Length);
			}
		}

		[Fact]
		public void TokenizeComplexTest()
		{
			string[] r = PathUtils.SplitPath(names[7]);
			Assert.Equal(4, r.Length);
			Assert.Equal("theFolder", r[0]);
			Assert.Equal("theSubFolder", r[1]);
			Assert.Equal("theSubSubFolder", r[2]);
			Assert.Equal("theName.dat", r[3]);
		}

		[Fact]
		public void TokenizeFunnySlants()
		{
			string[] r = PathUtils.SplitPath("\\theFolder/theName.dav");
			Assert.Equal(2, r.Length);
			Assert.Equal("theFolder", r[0]);
			Assert.Equal("theName.dav", r[1]);
		}

		[Fact]
		public void FileNameMatch()
		{
			new TestCase("theName.dat", true, names,
			             new bool[]
			             	{
			             		true, false, false, false, false, false, false, false, false, false,
			             		false, false, false, false
			             	}).RunTest();
		}

		[Fact]
		public void AnyFileNameMatch()
		{
			new TestCase("*", true, names,
			             new bool[]
			             	{
			             		true, true, false, true, false, false, false, false, false, false,
			             		true, false, false, false
			             	}).RunTest();
		}

		[Fact]
		public void ExactFolderAnyFile()
		{
			new TestCase("/theFolder/*.*", true, names,
			             new bool[]
			             	{
			             		false, false, true, false, true, false, false, false, false, false,
			             		false, false, false, true
			             	}).RunTest();
		}

		[Fact]
		public void AnyFolderExactFolder()
		{
			new TestCase("**/theName.dat", true, names,
			             new bool[]
			             	{
			             		true, false, true, false, false, true, true, true, true, true, false,
			             		false, false, false
			             	}).RunTest();
		}

		[Fact]
		public void ExactSubFolderAnyFile()
		{
			new TestCase("**/theSubFolder/*.*", true, names,
			             new bool[]
			             	{
			             		false, false, false, false, false, false, true, false, true, false,
			             		false, true, true, false
			             	}).RunTest();
		}

		[Fact]
		public void AcceptAll()
		{
			new TestCase("**/*.*", true, names,
			             new bool[]
			             	{
			             		true, true, true, true, true, true, true, true, true, true, true, true
			             		, true, true
			             	}).RunTest();
		}

		[Fact]
		public void PartialFileNames()
		{
			new TestCase("**/the*.dat", true, names,
			             new bool[]
			             	{
			             		true, false, true, false, false, true, true, true, true, true, false,
			             		false, false, false
			             	}).RunTest();
		}

		[Fact]
		public void SpecificExtensionInAnyFolder()
		{
			new TestCase("**/*.bin", true, names,
			             new bool[]
			             	{
			             		false, false, false, true, true, false, false, false, false, false,
			             		false, false, false, false
			             	}).RunTest();
		}

		[Fact]
		public void PartialFolderAnyFile()
		{
			new TestCase("**/the*Folder/*.*", true, names,
			             new bool[]
			             	{
			             		false, false, true, false, true, false, true, true, true, false, false
			             		, true, true, true
			             	}).RunTest();
		}

		[Fact]
		public void AnyFolderPartialExtension()
		{
			new TestCase("**/*.da*", true, names,
			             new bool[]
			             	{
			             		true, true, true, false, false, true, true, true, true, true, false,
			             		false, true, true
			             	}).RunTest();
		}

		[Fact]
		public void PathPrefixAnyFolder()
		{
			new TestCase("/theFolder/**/*.*", true, names,
			             new bool[]
			             	{
			             		false, false, true, false, true, false, true, true, true, true, false,
			             		true, true, true
			             	}).RunTest();
		}

		[Fact]
		public void SingledOutFunnySlants()
		{
			Assert.True(PathUtils.MatchPath("/theFolder/*", "\\theFolder/theName.dav", true));
		}

		[Fact]
		public void SingledOutStarStarNoExtension()
		{
			Assert.True(PathUtils.Match("*.*", "theName", true));
			Assert.True(PathUtils.Match("*.*", "theName.dat", true));
			Assert.True(PathUtils.Match("*", "theName", true));
			Assert.True(PathUtils.Match("*", "theName.dat", true));
		}

        [Fact]
        public void NullOrEmptyTargetShouldNotMatchPattern()
        {
            Assert.False(PathUtils.MatchPath("/theFolder/*", null, true));
            Assert.False(PathUtils.MatchPath("/theFolder/*", null, false));
            Assert.False(PathUtils.MatchPath("/theFolder/*", "", true));
            Assert.False(PathUtils.MatchPath("/theFolder/*", "", false));
        }
	}

	internal class TestCase
	{
		private string pattern;
		private string[] strs;
		private bool caseSensitive;
		private bool[] expected;

		public TestCase(string pattern, bool caseSensitive, string[] strs, bool[] expected)
		{
			this.pattern = pattern;
			this.strs = strs;
			this.caseSensitive = caseSensitive;
			this.expected = expected;
		}

		public void RunTest()
		{
			Assert.Equal(strs.Length, expected.Length);
			for (int i = 0; i < strs.Length; i++)
			{
				if (PathUtils.MatchPath(pattern, strs[i], caseSensitive) != expected[i])
				{
					Assert.Fail(
						string.Format(System.Globalization.CultureInfo.CurrentCulture,"[{4}] pattern={0} str={1} caseSensitive={2} expected={3}",
						              pattern, strs[i], caseSensitive, expected[i], i));
				}
			}
		}
	}
}
