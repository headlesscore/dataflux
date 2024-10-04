using Exortech.NetReflector;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
    
	public class CommentFilterTest
	{
        [Fact]
        public void ShouldNotPopulateWithoutPattern()
        {
            Assert.Throws<NetReflectorException>(delegate { NetReflector.Read(@"<commentFilter/>"); });
        }

        [Fact]
        public void ShouldPopulateFromMinimalSimpleXml()
        {
            CommentFilter filter = (CommentFilter) NetReflector.Read(@"<commentFilter pattern="".*""/>");
            Assert.True(".*" == filter.Pattern, "Wrong filter value found.");
        }

        [Fact]
        public void ShouldPopulateFromMinimalComplexXml()
        {
            CommentFilter filter = (CommentFilter) NetReflector.Read(@"<commentFilter> <pattern>.*</pattern> </commentFilter>");
            Assert.True(".*" == filter.Pattern, "Wrong filter value found.");
        }

        [Fact]
		public void ShouldRejectModificationsWithNullComments()
		{
			Modification modification = new Modification();
            CommentFilter filter = new CommentFilter();
			filter.Pattern = ".*";
			Assert.False(filter.Accept(modification), "Should not have matched but did.");
		}

        [Fact]
        public void ShouldAcceptModificationsWithMatchingComments()
        {
            Modification modification = new Modification();
            modification.Comment = "This is a comment.";
            CommentFilter filter = new CommentFilter();
            filter.Pattern = ".* is a .*";
            Assert.True(filter.Accept(modification), "Should have matched but did not.");
        }

        [Fact]
        public void ShouldRejectModificationsWithMatchingComments()
        {
            Modification modification = new Modification();
            modification.Comment = "This is a comment.";
            CommentFilter filter = new CommentFilter();
            filter.Pattern = ".* is not a .*";
            Assert.False(filter.Accept(modification), "Should not have matched but did.");
        }
    }
}

