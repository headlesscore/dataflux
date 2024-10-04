using Xunit;

using System;
using System.Collections.Generic;
using System.Text;
using ThoughtWorks.CruiseControl.Core.Security;
using ThoughtWorks.CruiseControl.Remote.Security;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Security
{
    
    public class UserPermissionTests
    {
        [Fact]
        public void MatchingUserNameReturnsTrue()
        {
            UserPermission assertion = new UserPermission("johndoe", SecurityRight.Inherit, SecurityRight.Inherit, SecurityRight.Allow, SecurityRight.Inherit);
            bool result = assertion.CheckUser(null, "johndoe");
            Assert.True(result);
            Assert.True(true);
        }

        [Fact]
        public void DifferentUserNameReturnsFalse()
        {
            UserPermission assertion = new UserPermission("johndoe", SecurityRight.Inherit, SecurityRight.Inherit, SecurityRight.Allow, SecurityRight.Inherit);
            bool result = assertion.CheckUser(null, "janedoe");
            Assert.False(result);
        }

        [Fact]
        public void MatchingPermissionReturnsRight()
        {
            UserPermission assertion = new UserPermission("johndoe", SecurityRight.Inherit, SecurityRight.Inherit, SecurityRight.Allow, SecurityRight.Inherit);
            SecurityRight result = assertion.CheckPermission(null, SecurityPermission.ForceAbortBuild);
            Assert.Equal(SecurityRight.Allow, result);
        }

        [Fact]
        public void DifferentPermissionReturnsInherited()
        {
            UserPermission assertion = new UserPermission("johndoe", SecurityRight.Inherit, SecurityRight.Inherit, SecurityRight.Allow, SecurityRight.Inherit);
            SecurityRight result = assertion.CheckPermission(null, SecurityPermission.SendMessage);
            Assert.Equal(SecurityRight.Inherit, result);
        }

        [Fact]
        public void GetSetAllProperties()
        {
            string userName = "johndoe";
            UserPermission assertion = new UserPermission();
            assertion.UserName = userName;
            Assert.Equal(userName, assertion.UserName);
            Assert.Equal(userName, assertion.Identifier);

            assertion.DefaultRight = SecurityRight.Deny;
            Assert.Equal(SecurityRight.Deny, assertion.DefaultRight);
            assertion.ForceBuildRight = SecurityRight.Deny;
            Assert.Equal(SecurityRight.Deny, assertion.ForceBuildRight);
            assertion.SendMessageRight = SecurityRight.Deny;
            Assert.Equal(SecurityRight.Deny, assertion.SendMessageRight);
            assertion.StartProjectRight = SecurityRight.Deny;
            Assert.Equal(SecurityRight.Deny, assertion.StartProjectRight);

            assertion.RefId = "A reference";
            Assert.Equal("A reference", assertion.RefId);
        }
    }
}
