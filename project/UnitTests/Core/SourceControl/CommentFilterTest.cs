using Exortech.NetReflector;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
    [TestFixture]
	public class CommentFilterTest
	{
        [Test]
        public void ShouldNotPopulateWithoutPattern()
        {
            ClassicAssert.That(delegate { NetReflector.Read(@"<commentFilter/>"); },
                        Throws.TypeOf<NetReflectorException>());
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void ShouldPopulateFromMinimalSimpleXml()
        {
            CommentFilter filter = (CommentFilter) NetReflector.Read(@"<commentFilter pattern="".*""/>");
            ClassicAssert.AreEqual(".*", filter.Pattern, "Wrong filter value found.");
        }

        [Test]
        public void ShouldPopulateFromMinimalComplexXml()
        {
            CommentFilter filter = (CommentFilter) NetReflector.Read(@"<commentFilter> <pattern>.*</pattern> </commentFilter>");
            ClassicAssert.AreEqual(".*", filter.Pattern, "Wrong filter value found.");
        }

        [Test]
		public void ShouldRejectModificationsWithNullComments()
		{
			Modification modification = new Modification();
            CommentFilter filter = new CommentFilter();
			filter.Pattern = ".*";
			ClassicAssert.IsFalse(filter.Accept(modification), "Should not have matched but did.");
		}

        [Test]
        public void ShouldAcceptModificationsWithMatchingComments()
        {
            Modification modification = new Modification();
            modification.Comment = "This is a comment.";
            CommentFilter filter = new CommentFilter();
            filter.Pattern = ".* is a .*";
            ClassicAssert.IsTrue(filter.Accept(modification), "Should have matched but did not.");
        }

        [Test]
        public void ShouldRejectModificationsWithMatchingComments()
        {
            Modification modification = new Modification();
            modification.Comment = "This is a comment.";
            CommentFilter filter = new CommentFilter();
            filter.Pattern = ".* is not a .*";
            ClassicAssert.IsFalse(filter.Accept(modification), "Should not have matched but did.");
        }
    }
}

