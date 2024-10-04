using System;
using System.Globalization;
using System.IO;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	
	public class VaultHistoryParserTest : CustomAssertion
	{
		//Creating the date string this way will give us a string in the format of the builder's locale.
		//Can end up with DateTime parsing errors in the test otherwise...
		//e.g. US format date string "5/13/2003" gives format exception when parsed on UK locale system.
		private static string XML_COMMENT_DATE = new DateTime(2003, 5, 13, 22, 41, 30).ToString(CultureInfo.InvariantCulture);
		private static readonly string XML = 
			@"<vault>
				<history>
					<item txid=""2"" date=""" + XML_COMMENT_DATE + @""" name=""$"" type=""70"" version=""1"" user=""admin"" comment=""creating repository"" />
				</history>
			  </vault>";
		private static readonly string NO_COMMENT_XML =
			@"<vault>
				<history>
					<item txid=""2"" date=""" + XML_COMMENT_DATE + @""" name=""$"" type=""70"" version=""1"" user=""admin"" />
				</history>
			  </vault>";
		private static readonly string XML_PADDED_WITH_EXTRA_CHARACTERS = @"_s ""Certificate Problem with
							accessing https://xxxx/VaultService/VaultService.asmx" + XML;

		private static readonly string ADD_AND_DELETE_FILES_XML = @"
			<vault>
				<history>
					<item txid=""13345"" date=""" + XML_COMMENT_DATE + @""" name=""$/1"" type=""80"" version=""319"" user=""jsmith"" comment=""temp file"" actionString=""Deleted 1.tmp"" />
					<item txid=""13344"" date=""" + XML_COMMENT_DATE + @""" name=""$/2"" type=""10"" version=""318"" user=""jsmith"" comment=""temp file"" actionString=""Added 2.tmp"" />
				</history>
			</vault>";

        // Actual output from "vault history $/Development/MyProductName.Enterprise -excludeactions label,obliterate -rowlimit 0 -begindate 2007-08-21T10:32:22 -enddate 2007-08-21T10:39:49 -host quad-us-dns01 -user build -password build -repository Quadrate] -- WorkingDirectory: [C:\Quadrate\MyProductName.Enterprise",
        // slightly anonymized for privacy.
	    private static readonly string ACTUAL_VAULT_OUTPUT = 
@"<vault>
<history>
 <item txid=""43754"" date=""8/21/2007 10:34:07 AM"" name=""MyProductName.Enterprise/MyProductName.BusinessLayer/Message.cs"" type=""60"" version=""6"" user=""joe_coder"" comment=""update length"" actionString=""Checked In"" />
 <item txid=""43753"" date=""8/21/2007 10:33:46 AM"" name=""MyProductName.Enterprise/DataWorkbenchMessageCenter/Properties/AssemblyInfo.cs"" type=""60"" version=""6"" user=""admin"" comment=""Updated Version to 1.0.4"" actionString=""Checked In"" />
 <item txid=""43752"" date=""8/21/2007 10:33:41 AM"" name=""MyProductName.Enterprise/MPSAdministrator/Properties/AssemblyInfo.cs"" type=""60"" version=""6"" user=""admin"" comment=""Updated Version to 1.0.4"" actionString=""Checked In"" />
 <item txid=""43751"" date=""8/21/2007 10:33:36 AM"" name=""MyProductName.Enterprise/MyProductName.Tester/Properties/AssemblyInfo.cs"" type=""60"" version=""8"" user=""admin"" comment=""Updated Version to 1.0.4"" actionString=""Checked In"" />
 <item txid=""43750"" date=""8/21/2007 10:33:30 AM"" name=""MyProductName.Enterprise/MyProductName.WebService/Properties/AssemblyInfo.cs"" type=""60"" version=""9"" user=""admin"" comment=""Updated Version to 1.0.4"" actionString=""Checked In"" />
 <item txid=""43749"" date=""8/21/2007 10:33:25 AM"" name=""MyProductName.Enterprise/MyProductName.DataLayer/Properties/AssemblyInfo.cs"" type=""60"" version=""8"" user=""admin"" comment=""Updated Version to 1.0.4"" actionString=""Checked In"" />
 <item txid=""43748"" date=""8/21/2007 10:33:18 AM"" name=""MyProductName.Enterprise/MyProductName.BusinessLayer/Properties/AssemblyInfo.cs"" type=""60"" version=""8"" user=""admin"" comment=""Updated Version to 1.0.4"" actionString=""Checked In"" />
</history>
<result success=""yes"" />
</vault>";

		private VaultHistoryParser parser;

		// [SetUp]
		protected void SetUp()
		{
			parser = new VaultHistoryParser(CultureInfo.InvariantCulture);
		}

		private static StringReader GetReader(string xml)
		{
			return new StringReader(xml);
		}

		[Fact]
		public void NumberOfModifications()
		{
			StringReader reader = GetReader(XML);
			Modification[] modifications = parser.Parse(reader, new DateTime(2003, 5, 12), DateTime.Now);
			reader.Close();
			Assert.Equal(1, modifications.Length);
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public void NumberOfModificationsWithInvalidDate()
		{
			StringReader reader = GetReader(XML);
			Modification[] modifications = parser.Parse(reader, DateTime.Now.AddMinutes(-1), DateTime.Now);
			reader.Close();
			Assert.Equal(0, modifications.Length);
		}

		[Fact]
		public void ModificationData()
		{
			StringReader reader = GetReader(XML);
			Modification[] modifications = parser.Parse(reader, new DateTime(2003, 5, 12), DateTime.Now);
			reader.Close();
			Modification mod = modifications[0];
			Assert.Equal("$", mod.FolderName);
			Assert.Equal(null, mod.FileName);
			Assert.Equal(new DateTime(2003, 5, 13, 22, 41, 30), mod.ModifiedTime);
			Assert.Equal("Created", mod.Type);
			Assert.Equal("admin", mod.UserName);
			Assert.Equal("2", mod.ChangeNumber);
		}

		[Fact]
		public void ShouldStripCharactersOutsideOfVaultElement()
		{
			StringReader reader = GetReader(XML_PADDED_WITH_EXTRA_CHARACTERS);
			Modification[] modifications = parser.Parse(reader, new DateTime(2003, 5, 12), DateTime.Now);			
			Assert.Equal(1, modifications.Length);
		}

		/// <summary>
		/// Tests a history entry with no comments (bug in first release of these classes).
		/// </summary>
		/// <remarks>
		/// Apparently the "comments" attribute is not always in the XML.
		/// </remarks>
		[Fact]
		public void NoComments()
		{
			StringReader reader = GetReader(NO_COMMENT_XML);
			parser.Parse(reader, new DateTime(2003, 5, 12), DateTime.Now);
			reader.Close();
		}

		[Fact]
		public void ShouldFindFileAndFolderNamesForAddsAndDeletes()
		{
			StringReader reader = GetReader(ADD_AND_DELETE_FILES_XML);
			Modification[] modifications = parser.Parse(reader, new DateTime(2003, 5, 12), DateTime.Now);			
			Assert.Equal(2, modifications.Length);
			Assert.Equal("1.tmp", modifications[0].FileName);
			Assert.Equal("$/1", modifications[0].FolderName);
			Assert.Equal("2.tmp", modifications[1].FileName);
			Assert.Equal("$/2", modifications[1].FolderName);
		}

        /// <summary>
        /// Test the history parser against some significant actual data.
        /// </summary>
        [Fact]
        public void ActualOutputTest()
        {
            StringReader reader = GetReader(ACTUAL_VAULT_OUTPUT);
            Modification[] modifications = parser.Parse(reader, new DateTime(2003, 5, 12), DateTime.Now);
            reader.Close();
            Assert.Equal(7, modifications.Length);

            Assert.Equal("43754", modifications[0].ChangeNumber);
            Assert.Equal("update length", modifications[0].Comment);
            Assert.Equal("Message.cs", modifications[0].FileName);
            Assert.Equal("MyProductName.Enterprise/MyProductName.BusinessLayer", modifications[0].FolderName);
            Assert.Equal(new DateTime(2007,8,21,10,34,07), modifications[0].ModifiedTime);
            Assert.Equal("Checked in", modifications[0].Type);
            Assert.Equal("joe_coder", modifications[0].UserName);

            Assert.Equal("43753", modifications[1].ChangeNumber);
            Assert.Equal("Updated Version to 1.0.4", modifications[1].Comment);
            Assert.Equal("AssemblyInfo.cs", modifications[1].FileName);
            Assert.Equal("MyProductName.Enterprise/DataWorkbenchMessageCenter/Properties", modifications[1].FolderName);
            Assert.Equal(new DateTime(2007,8,21,10, 33, 46), modifications[1].ModifiedTime);
            Assert.Equal("Checked in", modifications[1].Type);
            Assert.Equal("admin", modifications[1].UserName);

            Assert.Equal("43752", modifications[2].ChangeNumber);
            Assert.Equal("Updated Version to 1.0.4", modifications[2].Comment);
            Assert.Equal("AssemblyInfo.cs", modifications[2].FileName);
            Assert.Equal("MyProductName.Enterprise/MPSAdministrator/Properties", modifications[2].FolderName);
            Assert.Equal(new DateTime(2007,8,21,10, 33, 41), modifications[2].ModifiedTime);
            Assert.Equal("Checked in", modifications[2].Type);
            Assert.Equal("admin", modifications[2].UserName);

            Assert.Equal("43751", modifications[3].ChangeNumber);
            Assert.Equal("Updated Version to 1.0.4", modifications[3].Comment);
            Assert.Equal("AssemblyInfo.cs", modifications[3].FileName);
            Assert.Equal("MyProductName.Enterprise/MyProductName.Tester/Properties", modifications[3].FolderName);
            Assert.Equal(new DateTime(2007,8,21,10, 33, 36), modifications[3].ModifiedTime);
            Assert.Equal("Checked in", modifications[3].Type);
            Assert.Equal("admin", modifications[3].UserName);

            Assert.Equal("43750", modifications[4].ChangeNumber);
            Assert.Equal("Updated Version to 1.0.4", modifications[4].Comment);
            Assert.Equal("AssemblyInfo.cs", modifications[4].FileName);
            Assert.Equal("MyProductName.Enterprise/MyProductName.WebService/Properties", modifications[4].FolderName);
            Assert.Equal(new DateTime(2007,8,21,10, 33, 30), modifications[4].ModifiedTime);
            Assert.Equal("Checked in", modifications[4].Type);
            Assert.Equal("admin", modifications[4].UserName);

            Assert.Equal("43749", modifications[5].ChangeNumber); 
            Assert.Equal("Updated Version to 1.0.4", modifications[5].Comment);
            Assert.Equal("AssemblyInfo.cs", modifications[5].FileName);
            Assert.Equal("MyProductName.Enterprise/MyProductName.DataLayer/Properties", modifications[5].FolderName);
            Assert.Equal(new DateTime(2007,8,21,10, 33, 25), modifications[5].ModifiedTime);
            Assert.Equal("Checked in", modifications[5].Type);
            Assert.Equal("admin", modifications[5].UserName);

            Assert.Equal("43748", modifications[6].ChangeNumber);
            Assert.Equal("Updated Version to 1.0.4", modifications[6].Comment);
            Assert.Equal("AssemblyInfo.cs", modifications[6].FileName);
            Assert.Equal("MyProductName.Enterprise/MyProductName.BusinessLayer/Properties", modifications[6].FolderName);
            Assert.Equal(new DateTime(2007,8,21,10, 33, 18), modifications[6].ModifiedTime);
            Assert.Equal("Checked in", modifications[6].Type);
            Assert.Equal("admin", modifications[6].UserName);
        }

	}
}
