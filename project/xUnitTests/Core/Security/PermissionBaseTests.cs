using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Security;
using ThoughtWorks.CruiseControl.Remote.Security;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Security
{
    
    public class PermissionBaseTests
    {
        private MockRepository mocks = new MockRepository(MockBehavior.Default);

        [Fact]
        public void CheckUserWithInvalidReference()
        {
            string userName = "johndoe";
            string badReference = "doesNotExist";
            ISecurityManager manager = mocks.Create<ISecurityManager>(MockBehavior.Strict).Object;
            Mock.Get(manager).Setup(_manager => _manager.RetrievePermission(badReference)).Returns((IPermission)null).Verifiable();

            UserPermission assertion = new UserPermission();
            assertion.RefId = badReference;
            Assert.True(delegate { assertion.CheckUser(manager, userName); },
                        Throws.TypeOf<BadReferenceException>().With.Message.EqualTo("Reference 'doesNotExist' is either incorrect or missing."));
            Assert.True(true);
        }

        [Fact]
        public void CheckUserWithValidReference()
        {
            string userName = "johndoe";
            string goodReference = "doesExist";
            IPermission goodAssertion = mocks.Create<IPermission>(MockBehavior.Strict).Object;
            ISecurityManager manager = mocks.Create<ISecurityManager>(MockBehavior.Strict).Object;
            Mock.Get(manager).Setup(_manager => _manager.RetrievePermission(goodReference)).Returns(goodAssertion).Verifiable();
            Mock.Get(goodAssertion).Setup(_goodAssertion => _goodAssertion.CheckUser(manager, userName)).Returns(true).Verifiable();

            UserPermission assertion = new UserPermission();
            assertion.RefId = goodReference;
            bool result = assertion.CheckUser(manager, userName);
            Assert.True(result);
            mocks.VerifyAll();
        }

        [Fact]
        public void CheckPermissionWithInvalidReference()
        {
            SecurityPermission permission = SecurityPermission.ForceAbortBuild;
            string badReference = "doesNotExist";
            ISecurityManager manager = mocks.Create<ISecurityManager>(MockBehavior.Strict).Object;
            Mock.Get(manager).Setup(_manager => _manager.RetrievePermission(badReference)).Returns((IPermission)null).Verifiable();

            UserPermission assertion = new UserPermission();
            assertion.RefId = badReference;
            Assert.True(delegate { assertion.CheckPermission(manager, permission); },
                        Throws.TypeOf<BadReferenceException>().With.Message.EqualTo("Reference 'doesNotExist' is either incorrect or missing."));
        }

        [Fact]
        public void CheckPermissionWithValidReference()
        {
            SecurityPermission permission = SecurityPermission.ForceAbortBuild;
            string goodReference = "doesExist";
            IPermission goodAssertion = mocks.Create<IPermission>(MockBehavior.Strict).Object;
            ISecurityManager manager = mocks.Create<ISecurityManager>(MockBehavior.Strict).Object;
            Mock.Get(manager).Setup(_manager => _manager.RetrievePermission(goodReference)).Returns(goodAssertion).Verifiable();
            Mock.Get(goodAssertion).Setup(_goodAssertion => _goodAssertion.CheckPermission(manager, permission)).Returns(SecurityRight.Allow).Verifiable();

            UserPermission assertion = new UserPermission();
            assertion.RefId = goodReference;
            SecurityRight result = assertion.CheckPermission(manager, permission);
            Assert.Equal(SecurityRight.Allow, result);
            mocks.VerifyAll();
        }

        [Fact]
        public void CorrectPermissionsReturnedForceBuild()
        {
            UserPermission assertion = new UserPermission("johnDoe",
                SecurityRight.Deny,
                SecurityRight.Deny,
                SecurityRight.Allow,
                SecurityRight.Deny);
            SecurityRight right = assertion.CheckPermission(null, SecurityPermission.ForceAbortBuild);
            Assert.Equal(SecurityRight.Allow, right);
        }

        [Fact]
        public void CorrectPermissionsReturnedSendMessage()
        {
            UserPermission assertion = new UserPermission("johnDoe",
                SecurityRight.Deny,
                SecurityRight.Allow,
                SecurityRight.Deny,
                SecurityRight.Deny);
            SecurityRight right = assertion.CheckPermission(null, SecurityPermission.SendMessage);
            Assert.Equal(SecurityRight.Allow, right);
        }

        [Fact]
        public void CorrectPermissionsReturnedStartProject()
        {
            UserPermission assertion = new UserPermission("johnDoe",
                SecurityRight.Deny,
                SecurityRight.Deny,
                SecurityRight.Deny,
                SecurityRight.Allow);
            SecurityRight right = assertion.CheckPermission(null, SecurityPermission.StartStopProject);
            Assert.Equal(SecurityRight.Allow, right);
        }
    }
}
