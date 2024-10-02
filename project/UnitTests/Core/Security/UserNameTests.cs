using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Text;
using ThoughtWorks.CruiseControl.Core.Security;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Security
{
    [TestFixture]
    public class UserNameTests
    {
        [Test]
        public void CreateDefault()
        {
            UserName userName = new UserName();
            ClassicAssert.IsNull(userName.Name);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void CreateWithName()
        {
            UserName userName = new UserName("johnDoe");
            ClassicAssert.AreEqual("johnDoe", userName.Name);
        }

        [Test]
        public void GetSetAllProperties()
        {
            string name = "johnDoe";
            UserName userName = new UserName();
            userName.Name = name;
            ClassicAssert.AreEqual(name, userName.Name, "Name does not match");
        }
    }
}
