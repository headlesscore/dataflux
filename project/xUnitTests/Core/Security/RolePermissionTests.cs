using Xunit;

using System;
using System.Collections.Generic;
using System.Text;
using ThoughtWorks.CruiseControl.Core.Security;
using ThoughtWorks.CruiseControl.Remote.Security;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Security
{
    
    public class RolePermissionTests
    {
        [Fact]
        public void UserNameInRole()
        {
            RolePermission assertion = new RolePermission("testrole", SecurityRight.Inherit, SecurityRight.Inherit, SecurityRight.Allow, SecurityRight.Inherit, new UserName("johndoe"));
            bool result = assertion.CheckUser(null, "johndoe");
            Assert.True(result);
            Assert.True(true);
        }

        [Fact]
        public void UserNameNotInRole()
        {
            RolePermission assertion = new RolePermission("testrole", SecurityRight.Inherit, SecurityRight.Inherit, SecurityRight.Allow, SecurityRight.Inherit, new UserName("johndoe"));
            bool result = assertion.CheckUser(null, "janedoe");
            Assert.False(result);
        }

        [Fact]
        public void MatchingPermissionReturnsRight()
        {
            RolePermission assertion = new RolePermission("testrole", SecurityRight.Inherit, SecurityRight.Inherit, SecurityRight.Allow, SecurityRight.Inherit, new UserName("johndoe"));
            SecurityRight result = assertion.CheckPermission(null, SecurityPermission.ForceAbortBuild);
            Assert.Equal(SecurityRight.Allow, result);
        }

        [Fact]
        public void DifferentPermissionReturnsInherited()
        {
            RolePermission assertion = new RolePermission("testrole", SecurityRight.Inherit, SecurityRight.Inherit, SecurityRight.Allow, SecurityRight.Inherit, new UserName("johndoe"));
            SecurityRight result = assertion.CheckPermission(null, SecurityPermission.SendMessage);
            Assert.Equal(SecurityRight.Inherit, result);
        }

        [Fact]
        public void GetSetAllProperties()
        {
            string userName = "testrole";
            RolePermission assertion = new RolePermission();
            assertion.RoleName = userName;
            Assert.Equal(userName, assertion.RoleName);
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
            assertion.Users = new UserName[0];
            Assert.Empty(assertion.Users);
            assertion.Users = new UserName[] { new UserName("JohnDoe") };
            Assert.Single(assertion.Users);
        }
    }
}
