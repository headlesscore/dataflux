using System;
using FluentAssertions;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Util;
using Xunit;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	
	public class ModificationTest : CustomAssertion
	{
		[Fact]
		public void ModificationsAreComparedByModifiedDatetime()
		{
			Modification alpha = new Modification();
			alpha.ModifiedTime = new DateTime(1975, 3, 3);

			Modification beta = new Modification();
			alpha.ModifiedTime = new DateTime(1961, 3, 3);

			Assert.True(alpha.CompareTo(beta) > 0, string.Format(System.Globalization.CultureInfo.CurrentCulture,"expected alpha greater than beta {0}", alpha.CompareTo(beta)));
			Assert.True(alpha.CompareTo(alpha) == 0, string.Format(System.Globalization.CultureInfo.CurrentCulture,"expected alpha-beta equality {0}", alpha.CompareTo(beta)));
			Assert.True(beta.CompareTo(alpha) < 0, string.Format(System.Globalization.CultureInfo.CurrentCulture,"expected alpha less than beta {0}", alpha.CompareTo(beta)));
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public void OutputModificationToXml() 
		{
			Modification mod = CreateModification();

			string expected = string.Format(
@"<modification type=""unknown"">
	<filename>File""Name&amp;</filename>
	<project>Folder'Name</project>
	<date>{0}</date>
	<user>User&lt;&gt;Name</user>
	<comment>Comment</comment>
	<changeNumber>16</changeNumber>
	<url>http://localhost/viewcvs/</url>
	<email>foo.bar@quuuux.quuux.quux.qux</email>
</modification>", DateUtil.FormatDate(mod.ModifiedTime));

			Assert.Equal(XmlUtil.GenerateOuterXml(expected), mod.ToXml());
		}

		[Fact]
		public void OutputToXmlWithSpecialCharactersInCommentField()
		{
			Modification mod = CreateModification();
			mod.Comment = "math says 2 < 4 & XML CDATA ends with ]]>; don't nest <![CDATA in <![CDATA]]> ]]>";

			string actual = XmlUtil.SelectRequiredValue(mod.ToXml(), "/modification/comment");
			Assert.Equal(mod.Comment, actual);
		}

		[Fact]
		public void NullEmailAddressOrUrlShouldNotBeIncludedInXml()
		{
			Modification mod = CreateModification();
			mod.EmailAddress = null;
			mod.Url = null;

			Assert.Null(XmlUtil.SelectNode(mod.ToXml(), "/modification/email"));
			Assert.Null(XmlUtil.SelectNode(mod.ToXml(), "/modification/url"));
		}

        [Fact]
        public void ShouldReturnTheMaximumChangeNumberFromAllModificationsAsLastChangeNumber()
        {
            Modification mod1 = new Modification
            {
                ChangeNumber = "10",
                ModifiedTime = new DateTime(2009, 1, 1)
            };

            Modification mod2 = new Modification
            {
                ChangeNumber = "20",
                ModifiedTime = new DateTime(2009, 1, 2)
            };

            Modification[] modifications = new Modification[] { mod1 };
            Assert.Equal("10", Modification.GetLastChangeNumber(modifications));
            modifications = new Modification[] { mod1, mod2 };
            Assert.Equal("20", Modification.GetLastChangeNumber(modifications));
            modifications = new Modification[] { mod2, mod1 };
            Assert.Equal("20", Modification.GetLastChangeNumber(modifications));
        }

        [Fact]
        public void ShouldReturnNullAsLastChangeNumberIfNoModifications()
        {
            Modification[] modifications = new Modification[0];
            Modification.GetLastChangeNumber(modifications).Should().Be(null);
        }

		private static Modification CreateModification()
		{
			Modification mod = new Modification();
			mod.FileName = "File\"Name&";
			mod.FolderName = "Folder'Name";
			mod.ModifiedTime = DateTime.Now;
			mod.UserName = "User<>Name";
			mod.Comment = "Comment";
			mod.ChangeNumber = "16";
			mod.EmailAddress = "foo.bar@quuuux.quuux.quux.qux";
			mod.Url = "http://localhost/viewcvs/";
			return mod;
		}
	}
}
