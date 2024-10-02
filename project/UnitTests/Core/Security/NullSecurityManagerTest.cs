using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using ThoughtWorks.CruiseControl.Core.Security;
using ThoughtWorks.CruiseControl.Remote;
using ThoughtWorks.CruiseControl.Remote.Security;
using ThoughtWorks.CruiseControl.Remote.Messages;
using NUnit.Framework.Legacy;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Security
{
    [TestFixture]
    public class NullSecurityManagerTest
    {
        [Test]
        public void LoginReturnsUserName()
        {
            LoginRequest credentials = new LoginRequest("johndoe");
            NullSecurityManager manager = new NullSecurityManager();
            manager.Initialise();
            string sessionToken = manager.Login(credentials);
            ClassicAssert.AreEqual(NameValuePair.FindNamedValue(credentials.Credentials, LoginRequest.UserNameCredential), sessionToken);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void LogoutDoesNothing()
        {
            NullSecurityManager manager = new NullSecurityManager();
            manager.Logout("anydetailsinhere");
        }

        [Test]
        public void ValidateSessionReturnsTrue()
        {
            NullSecurityManager manager = new NullSecurityManager();
            bool result = manager.ValidateSession("anydetailsinhere");
            ClassicAssert.IsTrue(result);
        }

        [Test]
        public void GetUserNameReturnsSessionToken()
        {
            string sessionToken = "anydetailsinhere";
            NullSecurityManager manager = new NullSecurityManager();
            string userName = manager.GetUserName(sessionToken);
            ClassicAssert.AreEqual(sessionToken, userName);
        }

        [Test]
        public void GetDisplayNameReturnsSessionToken()
        {
            string sessionToken = "anydetailsinhere";
            NullSecurityManager manager = new NullSecurityManager();
            string userName = manager.GetDisplayName(sessionToken, null);
            ClassicAssert.AreEqual(sessionToken, userName);
        }

        [Test]
        public void RetrieveSettingReturnsNull()
        {
            NullSecurityManager manager = new NullSecurityManager();
            ISecuritySetting setting = manager.RetrievePermission("anything");
            ClassicAssert.IsNull(setting);
        }

        [Test]
        public void LogEventDoesNothing()
        {
            NullSecurityManager manager = new NullSecurityManager();
            manager.LogEvent("A project", "A user", SecurityEvent.ForceBuild, SecurityRight.Allow, "A message");
        }

        //[Test]
        //[ExpectedException(typeof(NotImplementedException),
        //    ExpectedMessage = "Password management is not allowed for this security manager")]
        //public void ChangePasswordThrowsAnException()
        //{
        //    NullSecurityManager manager = new NullSecurityManager();
        //    manager.ChangePassword("session", "oldPassword", "newPassword");
        //}

        //[Test]
        //[ExpectedException(typeof(NotImplementedException),
        //    ExpectedMessage = "Password management is not allowed for this security manager")]
        //public void ResetPasswordThrowsAnException()
        //{
        //    NullSecurityManager manager = new NullSecurityManager();
        //    manager.ResetPassword("session", "user", "newPassword");
        //}
    }
}
