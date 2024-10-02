using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Text;
using ThoughtWorks.CruiseControl.Core.Security;
using ThoughtWorks.CruiseControl.Remote.Security;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Security
{
    [TestFixture]
    public class UserPermissionTests
    {
        [Test]
        public void MatchingUserNameReturnsTrue()
        {
            UserPermission assertion = new UserPermission("johndoe", SecurityRight.Inherit, SecurityRight.Inherit, SecurityRight.Allow, SecurityRight.Inherit);
            bool result = assertion.CheckUser(null, "johndoe");
            ClassicAssert.IsTrue(result);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void DifferentUserNameReturnsFalse()
        {
            UserPermission assertion = new UserPermission("johndoe", SecurityRight.Inherit, SecurityRight.Inherit, SecurityRight.Allow, SecurityRight.Inherit);
            bool result = assertion.CheckUser(null, "janedoe");
            ClassicAssert.IsFalse(result);
        }

        [Test]
        public void MatchingPermissionReturnsRight()
        {
            UserPermission assertion = new UserPermission("johndoe", SecurityRight.Inherit, SecurityRight.Inherit, SecurityRight.Allow, SecurityRight.Inherit);
            SecurityRight result = assertion.CheckPermission(null, SecurityPermission.ForceAbortBuild);
            ClassicAssert.AreEqual(SecurityRight.Allow, result);
        }

        [Test]
        public void DifferentPermissionReturnsInherited()
        {
            UserPermission assertion = new UserPermission("johndoe", SecurityRight.Inherit, SecurityRight.Inherit, SecurityRight.Allow, SecurityRight.Inherit);
            SecurityRight result = assertion.CheckPermission(null, SecurityPermission.SendMessage);
            ClassicAssert.AreEqual(SecurityRight.Inherit, result);
        }

        [Test]
        public void GetSetAllProperties()
        {
            string userName = "johndoe";
            UserPermission assertion = new UserPermission();
            assertion.UserName = userName;
            ClassicAssert.AreEqual(userName, assertion.UserName, "UserName not correctly set");
            ClassicAssert.AreEqual(userName, assertion.Identifier, "Identifier not correctly set");

            assertion.DefaultRight = SecurityRight.Deny;
            ClassicAssert.AreEqual(SecurityRight.Deny, assertion.DefaultRight, "DefaultRight not correctly set");
            assertion.ForceBuildRight = SecurityRight.Deny;
            ClassicAssert.AreEqual(SecurityRight.Deny, assertion.ForceBuildRight, "ForceBuildRight not correctly set");
            assertion.SendMessageRight = SecurityRight.Deny;
            ClassicAssert.AreEqual(SecurityRight.Deny, assertion.SendMessageRight, "SendMessageRight not correctly set");
            assertion.StartProjectRight = SecurityRight.Deny;
            ClassicAssert.AreEqual(SecurityRight.Deny, assertion.StartProjectRight, "StartProjectRight not correctly set");

            assertion.RefId = "A reference";
            ClassicAssert.AreEqual("A reference", assertion.RefId, "RefId not correctly set");
        }
    }
}
