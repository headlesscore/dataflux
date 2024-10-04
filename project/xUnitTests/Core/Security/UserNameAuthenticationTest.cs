using Xunit;

using System;
using System.Collections.Generic;
using System.Text;
using ThoughtWorks.CruiseControl.Core.Security;
using ThoughtWorks.CruiseControl.Remote;
using ThoughtWorks.CruiseControl.Remote.Messages;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Security
{
    
    public class UserNameAuthenticationTest
    {
        [Fact]
        public void TestValidUserName()
        {
            UserNameAuthentication authentication = new UserNameAuthentication("johndoe");
            LoginRequest credentials = new LoginRequest("johndoe");
            bool isValid = authentication.Authenticate(credentials);
            Assert.True(isValid);
            Assert.True(true);
        }

        [Fact]
        public void TestInvalidUserName()
        {
            UserNameAuthentication authentication = new UserNameAuthentication("janedoe");
            LoginRequest credentials = new LoginRequest("johndoe");
            bool isValid = authentication.Authenticate(credentials);
            Assert.False(isValid);
        }

        [Fact]
        public void TestMissingUserName()
        {
            UserNameAuthentication authentication = new UserNameAuthentication("janedoe");
            LoginRequest credentials = new LoginRequest();
            bool isValid = authentication.Authenticate(credentials);
            Assert.False(isValid);
        }

        [Fact]
        public void GetSetAllProperties()
        {
            string userName = "johndoe";
            string displayName = "John Doe";
            UserNameAuthentication authentication = new UserNameAuthentication();
            authentication.UserName = userName;
            Assert.Equal(userName, authentication.UserName );
            Assert.Equal(userName, authentication.Identifier);
            authentication.DisplayName = displayName;
            Assert.Equal(displayName, authentication.DisplayName);
        }

        [Fact]
        public void GetUserNameReturnsName()
        {
            string userName = "johndoe";
            LoginRequest credentials = new LoginRequest(userName);
            UserNameAuthentication authentication = new UserNameAuthentication();
            string result = authentication.GetUserName(credentials);
            Assert.Equal(userName, result);
        }

        [Fact]
        public void GetDisplayNameReturnsDisplayName()
        {
            string userName = "johndoe";
            string displayName = "John Doe";
            LoginRequest credentials = new LoginRequest(userName);
            UserNameAuthentication authentication = new UserNameAuthentication();
            authentication.DisplayName = "John Doe";
            string result = authentication.GetDisplayName(credentials);
            Assert.Equal(displayName, result);
        }

        [Fact]
        public void GetDisplayNameReturnsUserName()
        {
            string userName = "johndoe";
            LoginRequest credentials = new LoginRequest(userName);
            UserNameAuthentication authentication = new UserNameAuthentication();
            string result = authentication.GetDisplayName(credentials);
            Assert.Equal(userName, result);
        }
    }
}
