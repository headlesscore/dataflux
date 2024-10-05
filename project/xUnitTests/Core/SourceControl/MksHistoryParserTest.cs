using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	
	public class MksHistoryParserTest
	{
	    private string TEST_DATA = String.Empty;
        private string MEMBER_INFO = String.Empty;
        private static string windowsPath = @"c:\Sandboxes\Personal2";
        private string path = Platform.IsWindows ? windowsPath : @"/Sandboxes/Personal2";

        // [OneTimeSetUp]
        public void SetUp()
        {
            // Transform xml output
            try
            {
                Assembly execAssem = Assembly.GetExecutingAssembly();
                using (Stream s =
                    execAssem.GetManifestResourceStream(
                        "ThoughtWorks.CruiseControl.UnitTests.Core.SourceControl.MksHistoryParserTestData.xml"))
                {

                    if (s != null)
                    {
                        StreamReader rdr = new StreamReader(s);
                        TEST_DATA = rdr.ReadToEnd().Replace(windowsPath.Replace('\\', '/'), path);
                    }
                    else
                    {
                        throw new CruiseControlException("Exception encountered while retrieving MksHistoryParserTestData.xml");
                    }
                }

                using (Stream s =
                    execAssem.GetManifestResourceStream(
                        "ThoughtWorks.CruiseControl.UnitTests.Core.SourceControl.MksHistoryParserTestDataMemberInfo.xml"))
                {
                    if (s != null)
                    {
                        StreamReader rdr = new StreamReader(s);
                        MEMBER_INFO = rdr.ReadToEnd();
                    }
                    else
                    {
                        throw new CruiseControlException("Exception encountered while retrieving MksHistoryParserTestDataMemberInfo.xml");
                    }
                }
            }
            catch (Exception e)
            {
                throw new CruiseControlException("Exception retrieving MKS test data.", e);
            }
        }

		[Fact]
		public void ParseOnlyRevisions()
		{
			MksHistoryParser parser = new MksHistoryParser();
			Modification[] modifications = parser.Parse(new StringReader(TEST_DATA), DateTime.Now, DateTime.Now);

            int changeCount = 0;

		    foreach (var modification in modifications)
		    {
		        if (modification.Type == "change")
		        {
		            changeCount++;
                    Assert.Equal("TestFile1.txt", modification.FileName);
                    Assert.Equal(path, modification.FolderName);
                    Assert.Equal("1.3", modification.Version);
                    Assert.True(true);
                    Assert.True(true);
                }
		    }
            Assert.Equal(1, changeCount);
            Assert.Equal(3, modifications.Length);
		}

		[Fact]
		public void ParseOnlyAdded()
		{
			MksHistoryParser parser = new MksHistoryParser();
			Modification[] modifications = parser.Parse(new StringReader(TEST_DATA), DateTime.Now, DateTime.Now);

            int changeCount = 0;

            foreach (var modification in modifications)
            {
                if (modification.Type == "add")
                {
                    changeCount++;
                    Assert.Equal("TestNew.txt", modification.FileName);
                    Assert.Equal(path, modification.FolderName);
                    Assert.Equal("1.1", modification.Version);
                }
            }

            Assert.Equal(1, changeCount);
			Assert.Equal(3, modifications.Length);
		}

		[Fact]
		public void ParseOnlyDeleted()
		{
			MksHistoryParser parser = new MksHistoryParser();
			Modification[] modifications = parser.Parse(new StringReader(TEST_DATA), DateTime.Now, DateTime.Now);

            int changeCount = 0;

            foreach (var modification in modifications)
            {
                if (modification.Type == "deleted")
                {
                    changeCount++;
                    Assert.Equal("TestFile2.txt", modification.FileName);
                    Assert.Equal(path, modification.FolderName);
                    Assert.Equal("NA", modification.Version);
                }
            }

            Assert.Equal(1, changeCount);
			Assert.Equal(3, modifications.Length);
		}

        [Fact]
        public void ParseMemberInfo()
        {
            Modification modification = new Modification();
            MksHistoryParser parser = new MksHistoryParser();
            parser.ParseMemberInfoAndAddToModification(modification, new StringReader(MEMBER_INFO));

            DateTime modifiedTimeWithLocalTimeZone = DateTime.Parse("2009-10-16T18:07:08");
            DateTime modifiedTimeWithCorrectTimeZoneInformation = modification.ModifiedTime;
            TimeSpan actualOffsetAtModifiedTime = modifiedTimeWithCorrectTimeZoneInformation.Subtract(modifiedTimeWithLocalTimeZone);

            TimeSpan expectedOffsetAtModifiedTime = TimeZone.CurrentTimeZone.GetUtcOffset(modifiedTimeWithLocalTimeZone);

            Assert.Equal("Test", modification.UserName);
            Assert.Equal(expectedOffsetAtModifiedTime, actualOffsetAtModifiedTime);
            Assert.Equal("Test Comment", modification.Comment);
            Assert.Equal("1234", modification.ChangeNumber);
        }
	}
}
