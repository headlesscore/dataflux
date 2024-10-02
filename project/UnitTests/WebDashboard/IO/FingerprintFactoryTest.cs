using System;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.WebDashboard.IO;
using ThoughtWorks.CruiseControl.WebDashboard.MVC;

namespace ThoughtWorks.CruiseControl.UnitTests.WebDashboard.IO
{
    [TestFixture]
    public class FingerprintFactoryTest
    {
        private Mock<IRequest> mockRequest;
        private IRequest request;

        [SetUp]
        public void SetUp()
        {
            mockRequest = new Mock<IRequest>();
            request = (IRequest)mockRequest.Object;
        }

        [Test]
        public void ShouldReturnNotAvailableIfEitherOrBothHeadersAreMissing()
        {
            mockRequest.SetupGet(_request => _request.IfModifiedSince).Returns(() => null).Verifiable();
            mockRequest.SetupGet(_request => _request.IfNoneMatch).Returns(() => null).Verifiable();
            ConditionalGetFingerprint fingerprint = new FingerprintFactory(null, null).BuildFromRequest(request);
            ClassicAssert.AreSame(ConditionalGetFingerprint.NOT_AVAILABLE, fingerprint);

            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);

            mockRequest.SetupGet(_request => _request.IfModifiedSince).Returns(DateTime.Now.ToString("r")).Verifiable();
            mockRequest.SetupGet(_request => _request.IfNoneMatch).Returns(() => null).Verifiable();
            fingerprint = new FingerprintFactory(null, null).BuildFromRequest(request);
            ClassicAssert.AreSame(ConditionalGetFingerprint.NOT_AVAILABLE, fingerprint);


            mockRequest.SetupGet(_request => _request.IfModifiedSince).Returns(() => null).Verifiable();
            mockRequest.SetupGet(_request => _request.IfNoneMatch).Returns("\"opaque value in etag\"").Verifiable();
            fingerprint = new FingerprintFactory(null, null).BuildFromRequest(request);
            ClassicAssert.AreSame(ConditionalGetFingerprint.NOT_AVAILABLE, fingerprint);
        }

        [Test]
        public void ShouldBuildAFingerprintWithValuesFromRequestIfBothHeadersAreAvailable()
        {
            DateTime lastModifiedDate = new DateTime(2007, 4, 20);
            string etagValue = "\"some opaque value\"";

            mockRequest.SetupGet(_request => _request.IfModifiedSince).Returns(lastModifiedDate.ToString("r")).Verifiable();
            mockRequest.SetupGet(_request => _request.IfNoneMatch).Returns(etagValue).Verifiable();
            ConditionalGetFingerprint fingerprint = new FingerprintFactory(null, null).BuildFromRequest(request);
            ClassicAssert.AreEqual(new ConditionalGetFingerprint(lastModifiedDate, etagValue), fingerprint);
        }

        [Test]
        [Ignore("TODO: provide a reason")]
        public void ShouldFailGracefullyWithDatesFromBrowserWhichAreNotInRfc1123FormatByReturningValidButIncorrectFingerprint()
        {
            DateTime lastModifiedDate = new DateTime(2007, 4, 20);
            string etagValue = "\"some opaque value\"";

            mockRequest.SetupGet(_request => _request.IfModifiedSince).Returns(lastModifiedDate.ToString()).Verifiable();
            mockRequest.SetupGet(_request => _request.IfNoneMatch).Returns(etagValue).Verifiable();
            ConditionalGetFingerprint fingerprint = new FingerprintFactory(null, null).BuildFromRequest(request);
            ClassicAssert.AreNotEqual(new ConditionalGetFingerprint(lastModifiedDate, etagValue), fingerprint);
        }

        [Test]
        public void ShouldAddQuotesToStringFromVersionAssemblyProviderForFingerprintFromDate()
        {
            string testETag = "test e tag value";
            DateTime testDate = new DateTime(2007, 4, 20);
            var mockVersionProvider = new Mock<IVersionProvider>();
            mockVersionProvider.Setup(provider => provider.GetVersion()).Returns(testETag);

            ConditionalGetFingerprint testConditionalGetFingerprint =
                new FingerprintFactory((IVersionProvider) mockVersionProvider.Object, null).BuildFromDate(testDate);
                

            string expectedETag = "\"" + testETag + "\"";
            ClassicAssert.AreEqual(expectedETag, testConditionalGetFingerprint.ETag);
        }
    }
}
