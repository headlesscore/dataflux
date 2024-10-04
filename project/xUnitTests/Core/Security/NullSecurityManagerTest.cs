using Xunit;
using System;
using System.Collections.Generic;
using System.Text;
using ThoughtWorks.CruiseControl.Core.Security;
using ThoughtWorks.CruiseControl.Remote;
using ThoughtWorks.CruiseControl.Remote.Security;
using ThoughtWorks.CruiseControl.Remote.Messages;


namespace ThoughtWorks.CruiseControl.UnitTests.Core.Security
{
    
    public class NullSecurityManagerTest
    {
        [Fact]
        public void LoginReturnsUserName()
        {
            LoginRequest credentials = new LoginRequest("johndoe");
            NullSecurityManager manager = new NullSecurityManager();
            manager.Initialise();
            string sessionToken = manager.Login(credentials);
            Assert.Equal(NameValuePair.FindNamedValue(credentials.Credentials, LoginRequest.UserNameCredential), sessionToken);
            Assert.True(true);
        }

        [Fact]
        public void LogoutDoesNothing()
        {
            NullSecurityManager manager = new NullSecurityManager();
            manager.Logout("anydetailsinhere");
        }

        [Fact]
        public void ValidateSessionReturnsTrue()
        {
            NullSecurityManager manager = new NullSecurityManager();
            bool result = manager.ValidateSession("anydetailsinhere");
            Assert.True(result);
        }

        [Fact]
        public void GetUserNameReturnsSessionToken()
        {
            string sessionToken = "anydetailsinhere";
            NullSecurityManager manager = new NullSecurityManager();
            string userName = manager.GetUserName(sessionToken);
            Assert.Equal(sessionToken, userName);
        }

        [Fact]
        public void GetDisplayNameReturnsSessionToken()
        {
            string sessionToken = "anydetailsinhere";
            NullSecurityManager manager = new NullSecurityManager();
            string userName = manager.GetDisplayName(sessionToken, null);
            Assert.Equal(sessionToken, userName);
        }

        [Fact]
        public void RetrieveSettingReturnsNull()
        {
            NullSecurityManager manager = new NullSecurityManager();
            ISecuritySetting setting = manager.RetrievePermission("anything");
            Assert.Null(setting);
        }

        [Fact]
        public void LogEventDoesNothing()
        {
            NullSecurityManager manager = new NullSecurityManager();
            manager.LogEvent("A project", "A user", SecurityEvent.ForceBuild, SecurityRight.Allow, "A message");
        }

        //[Fact]
        //[ExpectedException(typeof(NotImplementedException),
        //    ExpectedMessage = "Password management is not allowed for this security manager")]
        //public void ChangePasswordThrowsAnException()
        //{
        //    NullSecurityManager manager = new NullSecurityManager();
        //    manager.ChangePassword("session", "oldPassword", "newPassword");
        //}

        //[Fact]
        //[ExpectedException(typeof(NotImplementedException),
        //    ExpectedMessage = "Password management is not allowed for this security manager")]
        //public void ResetPasswordThrowsAnException()
        //{
        //    NullSecurityManager manager = new NullSecurityManager();
        //    manager.ResetPassword("session", "user", "newPassword");
        //}
    }
}
