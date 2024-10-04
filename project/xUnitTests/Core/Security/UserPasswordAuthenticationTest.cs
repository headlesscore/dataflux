using Xunit;

using System;
using System.Collections.Generic;
using System.Text;
using ThoughtWorks.CruiseControl.Core.Security;
using ThoughtWorks.CruiseControl.Remote;
using ThoughtWorks.CruiseControl.Remote.Messages;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Security
{
    
    public class UserPasswordAuthenticationTest
    {
        [Fact]
        public void TestValidUserNameAndPassword()
        {
            UserPasswordAuthentication authentication = new UserPasswordAuthentication("johndoe", "iknowyou");
            LoginRequest credentials = new LoginRequest("johndoe");
            credentials.AddCredential(LoginRequest.PasswordCredential, "iknowyou");
            bool isValid = authentication.Authenticate(credentials);
            Assert.True(isValid);
            Assert.True(true);
        }

        [Fact]
        public void TestMissingPassword()
        {
            UserPasswordAuthentication authentication = new UserPasswordAuthentication("johndoe", "iknowyou");
            LoginRequest credentials = new LoginRequest("johndoe");
            bool isValid = authentication.Authenticate(credentials);
            Assert.False(isValid);
        }

        [Fact]
        public void TestMissingUserName()
        {
            UserPasswordAuthentication authentication = new UserPasswordAuthentication("johndoe", "iknowyou");
            LoginRequest credentials = new LoginRequest();
            bool isValid = authentication.Authenticate(credentials);
            Assert.False(isValid);
        }

        [Fact]
        public void TestIncorrectPassword()
        {
            UserPasswordAuthentication authentication = new UserPasswordAuthentication("johndoe", "iknowyou");
            LoginRequest credentials = new LoginRequest("johndoe");
            credentials.AddCredential(LoginRequest.PasswordCredential, "idontknowyou");
            bool isValid = authentication.Authenticate(credentials);
            Assert.False(isValid);
        }

        [Fact]
        public void TestIncorrectUserName()
        {
            UserPasswordAuthentication authentication = new UserPasswordAuthentication("johndoe", "iknowyou");
            LoginRequest credentials = new LoginRequest("janedoe");
            credentials.AddCredential(LoginRequest.PasswordCredential, "iknowyou");
            bool isValid = authentication.Authenticate(credentials);
            Assert.False(isValid);
        }

        [Fact]
        public void GetSetAllProperties()
        {
            string userName = "johndoe";
            string displayName = "John Doe";
            string password = "whoareyou";
            UserPasswordAuthentication authentication = new UserPasswordAuthentication();
            authentication.UserName = userName;
            Assert.Equal(userName, authentication.UserName, "UserName not correctly set");
            Assert.Equal(userName, authentication.Identifier, "Identifier not correctly set");
            authentication.Password = password;
            Assert.Equal(password, authentication.Password, "Password not correctly set");
            authentication.DisplayName = displayName;
            Assert.Equal(displayName, authentication.DisplayName, "DisplayName not correctly set");
        }

        [Fact]
        public void GetUserNameReturnsName()
        {
            string userName = "johndoe";
            LoginRequest credentials = new LoginRequest(userName);
            UserPasswordAuthentication authentication = new UserPasswordAuthentication();
            string result = authentication.GetUserName(credentials);
            Assert.Equal(userName, result);
        }

        [Fact]
        public void GetDisplayNameReturnsDisplayName()
        {
            string userName = "johndoe";
            string displayName = "John Doe";
            LoginRequest credentials = new LoginRequest(userName);
            UserPasswordAuthentication authentication = new UserPasswordAuthentication();
            authentication.DisplayName = "John Doe";
            string result = authentication.GetDisplayName(credentials);
            Assert.Equal(displayName, result);
        }

        [Fact]
        public void GetDisplayNameReturnsUserName()
        {
            string userName = "johndoe";
            LoginRequest credentials = new LoginRequest(userName);
            UserPasswordAuthentication authentication = new UserPasswordAuthentication();
            string result = authentication.GetDisplayName(credentials);
            Assert.Equal(userName, result);
        }
    }
}
