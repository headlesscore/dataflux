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
            Assert.Equal(userName, assertion.RoleName, "RoleName not correctly set");
            Assert.Equal(userName, assertion.Identifier, "Identifier not correctly set");

            assertion.DefaultRight = SecurityRight.Deny;
            Assert.Equal(SecurityRight.Deny, assertion.DefaultRight, "DefaultRight not correctly set");
            assertion.ForceBuildRight = SecurityRight.Deny;
            Assert.Equal(SecurityRight.Deny, assertion.ForceBuildRight, "ForceBuildRight not correctly set");
            assertion.SendMessageRight = SecurityRight.Deny;
            Assert.Equal(SecurityRight.Deny, assertion.SendMessageRight, "SendMessageRight not correctly set");
            assertion.StartProjectRight = SecurityRight.Deny;
            Assert.Equal(SecurityRight.Deny, assertion.StartProjectRight, "StartProjectRight not correctly set");

            assertion.RefId = "A reference";
            Assert.Equal("A reference", assertion.RefId, "RefId not correctly set");
            assertion.Users = new UserName[0];
            Assert.Equal(0, assertion.Users.Length, "Users not correctly set - empty array");
            assertion.Users = new UserName[] { new UserName("JohnDoe") };
            Assert.Equal(1, assertion.Users.Length, "Users not correctly set - array with data");
        }
    }
}
