﻿using Exortech.NetReflector;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
    [TestFixture]
    public class MultiFilterTest
    {
        private MultiFilter filter;

        [SetUp]
        protected void CreateFilter()
        {
            filter = new MultiFilter();
        }

        [Test]
        public void ShouldNotAcceptIfNoActionIsSpecified()
        {
            ClassicAssert.IsFalse(filter.Accept(new Modification()));
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
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

            ClassicAssert.IsTrue(filter.Accept(mod), "Modifcation not filtered");
        }

        [Test]
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

            ClassicAssert.IsFalse(filter.Accept(mod), "Modifcation was filtered");
        }

        [Test]
        public void LoadFromConfiguration()
        {
            filter = (MultiFilter)NetReflector.Read(@"<multiFilter><filters><actionFilter><actions><action>Delete</action></actions></actionFilter>
                                                        <userFilter><names><name>bob</name><name>perry</name></names></userFilter></filters></multiFilter>");
            ClassicAssert.AreEqual(2, filter.Filters.Length);
        }
    }
}
