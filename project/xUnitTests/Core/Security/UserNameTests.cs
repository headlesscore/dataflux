using Xunit;

using System;
using System.Collections.Generic;
using System.Text;
using ThoughtWorks.CruiseControl.Core.Security;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Security
{
    
    public class UserNameTests
    {
        [Fact]
        public void CreateDefault()
        {
            UserName userName = new UserName();
            Assert.Null(userName.Name);
            Assert.True(true);
        }

        [Fact]
        public void CreateWithName()
        {
            UserName userName = new UserName("johnDoe");
            Assert.Equal("johnDoe", userName.Name);
        }

        [Fact]
        public void GetSetAllProperties()
        {
            string name = "johnDoe";
            UserName userName = new UserName();
            userName.Name = name;
            Assert.Equal(name, userName.Name, "Name does not match");
        }
    }
}
