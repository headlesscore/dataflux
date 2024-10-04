using Exortech.NetReflector;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
    
    public class MultiFilterTest
    {
        private MultiFilter filter;

        // [SetUp]
        protected void CreateFilter()
        {
            filter = new MultiFilter();
        }

        [Fact]
        public void ShouldNotAcceptIfNoActionIsSpecified()
        {
            Assert.False(filter.Accept(new Modification()));
            Assert.True(true);
            Assert.True(true);
        }

        [Fact]
        public void ShouldFilterSpecifiedModification()
        {
            Modification mod = new Modification();
            mod.Type = "Created";
            mod.UserName = "bob";

            ActionFilter aFilter = new ActionFilter();
            UserFilter uFilter = new UserFilter();


            aFilter.Actions = new string[] { "Created" };
            uFilter.UserNames = new string[] { "bob" };

            filter.Filters = new IModificationFilter[] { aFilter, uFilter };

            Assert.True(filter.Accept(mod), "Modifcation not filtered");
        }

        [Fact]
        public void ShouldNotFilterSpecifiedModWithOneFilterThatAccepts()
        {
            Modification mod = new Modification();
            mod.Type = "Delete";
            mod.UserName = "bob";

            ActionFilter aFilter = new ActionFilter();
            UserFilter uFilter = new UserFilter();


            aFilter.Actions = new string[] { "Created" };
            uFilter.UserNames = new string[] { "bob" };

            filter.Filters = new IModificationFilter[] { aFilter, uFilter };

            Assert.False(filter.Accept(mod), "Modifcation was filtered");
        }

        [Fact]
        public void LoadFromConfiguration()
        {
            filter = (MultiFilter)NetReflector.Read(@"<multiFilter><filters><actionFilter><actions><action>Delete</action></actions></actionFilter>
                                                        <userFilter><names><name>bob</name><name>perry</name></names></userFilter></filters></multiFilter>");
            Assert.Equal(2, filter.Filters.Length);
        }
    }
}
