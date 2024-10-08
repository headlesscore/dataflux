using System;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	[TestFixture]
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

		[SetUp]
		protected void SetUp()
		{
			parser = new VaultHistoryParser(CultureInfo.InvariantCulture);
		}

		private static StringReader GetReader(string xml)
		{
			return new StringReader(xml);
		}

		[Test]
		public void NumberOfModifications()
		{
			StringReader reader = GetReader(XML);
			Modification[] modifications = parser.Parse(reader, new DateTime(2003, 5, 12), DateTime.Now);
			reader.Close();
			ClassicAssert.AreEqual(1, modifications.Length);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

		[Test]
		public void NumberOfModificationsWithInvalidDate()
		{
			StringReader reader = GetReader(XML);
			Modification[] modifications = parser.Parse(reader, DateTime.Now.AddMinutes(-1), DateTime.Now);
			reader.Close();
			ClassicAssert.AreEqual(0, modifications.Length);
		}

		[Test]
		public void ModificationData()
		{
			StringReader reader = GetReader(XML);
			Modification[] modifications = parser.Parse(reader, new DateTime(2003, 5, 12), DateTime.Now);
			reader.Close();
			Modification mod = modifications[0];
			ClassicAssert.AreEqual("$", mod.FolderName);
			ClassicAssert.AreEqual(null, mod.FileName);
			ClassicAssert.AreEqual(new DateTime(2003, 5, 13, 22, 41, 30), mod.ModifiedTime);
			ClassicAssert.AreEqual("Created", mod.Type);
			ClassicAssert.AreEqual("admin", mod.UserName);
			ClassicAssert.AreEqual("2", mod.ChangeNumber);
		}

		[Test]
		public void ShouldStripCharactersOutsideOfVaultElement()
		{
			StringReader reader = GetReader(XML_PADDED_WITH_EXTRA_CHARACTERS);
			Modification[] modifications = parser.Parse(reader, new DateTime(2003, 5, 12), DateTime.Now);			
			ClassicAssert.AreEqual(1, modifications.Length);
		}

		/// <summary>
		/// Tests a history entry with no comments (bug in first release of these classes).
		/// </summary>
		/// <remarks>
		/// Apparently the "comments" attribute is not always in the XML.
		/// </remarks>
		[Test]
		public void NoComments()
		{
			StringReader reader = GetReader(NO_COMMENT_XML);
			parser.Parse(reader, new DateTime(2003, 5, 12), DateTime.Now);
			reader.Close();
		}

		[Test]
		public void ShouldFindFileAndFolderNamesForAddsAndDeletes()
		{
			StringReader reader = GetReader(ADD_AND_DELETE_FILES_XML);
			Modification[] modifications = parser.Parse(reader, new DateTime(2003, 5, 12), DateTime.Now);			
			ClassicAssert.AreEqual(2, modifications.Length);
			ClassicAssert.AreEqual("1.tmp", modifications[0].FileName);
			ClassicAssert.AreEqual("$/1", modifications[0].FolderName);
			ClassicAssert.AreEqual("2.tmp", modifications[1].FileName);
			ClassicAssert.AreEqual("$/2", modifications[1].FolderName);
		}

        /// <summary>
        /// Test the history parser against some significant actual data.
        /// </summary>
        [Test]
        public void ActualOutputTest()
        {
            StringReader reader = GetReader(ACTUAL_VAULT_OUTPUT);
            Modification[] modifications = parser.Parse(reader, new DateTime(2003, 5, 12), DateTime.Now);
            reader.Close();
            ClassicAssert.AreEqual(7, modifications.Length);

            ClassicAssert.AreEqual("43754", modifications[0].ChangeNumber);
            ClassicAssert.AreEqual("update length", modifications[0].Comment);
            ClassicAssert.AreEqual("Message.cs", modifications[0].FileName);
            ClassicAssert.AreEqual("MyProductName.Enterprise/MyProductName.BusinessLayer", modifications[0].FolderName);
            ClassicAssert.AreEqual(new DateTime(2007,8,21,10,34,07), modifications[0].ModifiedTime);
            ClassicAssert.AreEqual("Checked in", modifications[0].Type);
            ClassicAssert.AreEqual("joe_coder", modifications[0].UserName);

            ClassicAssert.AreEqual("43753", modifications[1].ChangeNumber);
            ClassicAssert.AreEqual("Updated Version to 1.0.4", modifications[1].Comment);
            ClassicAssert.AreEqual("AssemblyInfo.cs", modifications[1].FileName);
            ClassicAssert.AreEqual("MyProductName.Enterprise/DataWorkbenchMessageCenter/Properties", modifications[1].FolderName);
            ClassicAssert.AreEqual(new DateTime(2007,8,21,10, 33, 46), modifications[1].ModifiedTime);
            ClassicAssert.AreEqual("Checked in", modifications[1].Type);
            ClassicAssert.AreEqual("admin", modifications[1].UserName);

            ClassicAssert.AreEqual("43752", modifications[2].ChangeNumber);
            ClassicAssert.AreEqual("Updated Version to 1.0.4", modifications[2].Comment);
            ClassicAssert.AreEqual("AssemblyInfo.cs", modifications[2].FileName);
            ClassicAssert.AreEqual("MyProductName.Enterprise/MPSAdministrator/Properties", modifications[2].FolderName);
            ClassicAssert.AreEqual(new DateTime(2007,8,21,10, 33, 41), modifications[2].ModifiedTime);
            ClassicAssert.AreEqual("Checked in", modifications[2].Type);
            ClassicAssert.AreEqual("admin", modifications[2].UserName);

            ClassicAssert.AreEqual("43751", modifications[3].ChangeNumber);
            ClassicAssert.AreEqual("Updated Version to 1.0.4", modifications[3].Comment);
            ClassicAssert.AreEqual("AssemblyInfo.cs", modifications[3].FileName);
            ClassicAssert.AreEqual("MyProductName.Enterprise/MyProductName.Tester/Properties", modifications[3].FolderName);
            ClassicAssert.AreEqual(new DateTime(2007,8,21,10, 33, 36), modifications[3].ModifiedTime);
            ClassicAssert.AreEqual("Checked in", modifications[3].Type);
            ClassicAssert.AreEqual("admin", modifications[3].UserName);

            ClassicAssert.AreEqual("43750", modifications[4].ChangeNumber);
            ClassicAssert.AreEqual("Updated Version to 1.0.4", modifications[4].Comment);
            ClassicAssert.AreEqual("AssemblyInfo.cs", modifications[4].FileName);
            ClassicAssert.AreEqual("MyProductName.Enterprise/MyProductName.WebService/Properties", modifications[4].FolderName);
            ClassicAssert.AreEqual(new DateTime(2007,8,21,10, 33, 30), modifications[4].ModifiedTime);
            ClassicAssert.AreEqual("Checked in", modifications[4].Type);
            ClassicAssert.AreEqual("admin", modifications[4].UserName);

            ClassicAssert.AreEqual("43749", modifications[5].ChangeNumber); 
            ClassicAssert.AreEqual("Updated Version to 1.0.4", modifications[5].Comment);
            ClassicAssert.AreEqual("AssemblyInfo.cs", modifications[5].FileName);
            ClassicAssert.AreEqual("MyProductName.Enterprise/MyProductName.DataLayer/Properties", modifications[5].FolderName);
            ClassicAssert.AreEqual(new DateTime(2007,8,21,10, 33, 25), modifications[5].ModifiedTime);
            ClassicAssert.AreEqual("Checked in", modifications[5].Type);
            ClassicAssert.AreEqual("admin", modifications[5].UserName);

            ClassicAssert.AreEqual("43748", modifications[6].ChangeNumber);
            ClassicAssert.AreEqual("Updated Version to 1.0.4", modifications[6].Comment);
            ClassicAssert.AreEqual("AssemblyInfo.cs", modifications[6].FileName);
            ClassicAssert.AreEqual("MyProductName.Enterprise/MyProductName.BusinessLayer/Properties", modifications[6].FolderName);
            ClassicAssert.AreEqual(new DateTime(2007,8,21,10, 33, 18), modifications[6].ModifiedTime);
            ClassicAssert.AreEqual("Checked in", modifications[6].Type);
            ClassicAssert.AreEqual("admin", modifications[6].UserName);
        }

	}
}
