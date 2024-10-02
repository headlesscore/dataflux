﻿namespace ThoughtWorks.CruiseControl.UnitTests.Remote.Messages
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using ThoughtWorks.CruiseControl.Remote;
    using ThoughtWorks.CruiseControl.Remote.Messages;

    [TestFixture]
    public class ListPackagesResponseTests
    {
        #region Tests
        #region Constructor tests
        [Test]
        public void RequestConstructorInitialisesTheValues()
        {
            var request = new EncryptedRequest();
            var response = new ListPackagesResponse(request);
            // Only check one property is set, since the properties are set by the base class
            ClassicAssert.AreEqual(request.Identifier, response.RequestIdentifier);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void FullConstructorInitialisesTheValues()
        {
            var response1 = new ListPackagesResponse();
            response1.RequestIdentifier = "12345";
            var response2 = new ListPackagesResponse(response1);
            // Only check one property is set, since the properties are set by the base class
            ClassicAssert.AreEqual(response1.RequestIdentifier, response2.RequestIdentifier);
        }
        #endregion

        #region Getter/setter tests
        [Test]
        public void EncryptedDataCanBeSetAndRetrieved()
        {
            var request = new ListPackagesResponse();
            var packages = new List<PackageDetails>();
            request.Packages = packages;
            ClassicAssert.AreEqual(packages, request.Packages);
        }
        #endregion
        #endregion
    }
}
