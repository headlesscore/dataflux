using System;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.WebDashboard.IO;

namespace ThoughtWorks.CruiseControl.UnitTests.WebDashboard.IO
{
    [TestFixture]
    public class ConditionalGetFingerprintTest
    {
        private DateTime testDate;
        private string testETag;

        [SetUp]
        public void SetUp()
        {
            testDate = new DateTime(2007, 4, 22, 22, 50, 29, DateTimeKind.Utc);
            testETag = "test e tag";
        }

        [Test]
        public void ShouldBeEqualIfDateAndETagAreEqual()
        {
            ConditionalGetFingerprint firstFingerprint = new ConditionalGetFingerprint(testDate, testETag);
            ConditionalGetFingerprint secondFingerprint = new ConditionalGetFingerprint(testDate, testETag);

            ClassicAssert.AreEqual(firstFingerprint, secondFingerprint);
            ClassicAssert.AreNotSame(firstFingerprint, secondFingerprint);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

		[Test]
        public void ShouldNotBeEqualIfDatesDiffer()
        {
            ConditionalGetFingerprint firstFingerprint = new ConditionalGetFingerprint(testDate, testETag);

			DateTime differentDate = new DateTime(2007, 3, 22, 22, 50, 29, DateTimeKind.Utc);
			ConditionalGetFingerprint secondFingerprint = new ConditionalGetFingerprint(differentDate, testETag);

            ClassicAssert.AreNotEqual(firstFingerprint, secondFingerprint);
        }

        [Test]
        public void ShouldNeverEqualNotAvailable()
        {
            ConditionalGetFingerprint testFingerprint = new ConditionalGetFingerprint(testDate, testETag);

            ClassicAssert.AreNotEqual(testFingerprint, ConditionalGetFingerprint.NOT_AVAILABLE);
        }

        [Test]
        public void NotAvailableNotEvenEqualToItself()
        {
            ClassicAssert.IsFalse(ConditionalGetFingerprint.NOT_AVAILABLE.Equals(ConditionalGetFingerprint.NOT_AVAILABLE));
            ClassicAssert.AreSame(ConditionalGetFingerprint.NOT_AVAILABLE, ConditionalGetFingerprint.NOT_AVAILABLE);
        }

        [Test]
        public void ShouldThrowExceptionIfFingerprintsAreCombinedWhichHaveDifferentETags()
        {
            ConditionalGetFingerprint testFingerprint = new ConditionalGetFingerprint(testDate, testETag);
            ConditionalGetFingerprint fingerprintWithDifferentETag= new ConditionalGetFingerprint(testDate, testETag + "different");

            ClassicAssert.That(delegate { testFingerprint.Combine(fingerprintWithDifferentETag); },
                        Throws.TypeOf<UncombinableFingerprintException>());
        }

        [Test]
        public void ShouldUseMostRecentDateWhenCombined()
        {
            DateTime olderDate = new DateTime(2006,12,1);
            DateTime recentDate = new DateTime(2007,2,1);

            ConditionalGetFingerprint olderFingerprint = new ConditionalGetFingerprint(olderDate, testETag);
            ConditionalGetFingerprint newerFingerprint = new ConditionalGetFingerprint(recentDate, testETag);

            ConditionalGetFingerprint expectedFingerprint = newerFingerprint;
            ClassicAssert.AreEqual(expectedFingerprint, olderFingerprint.Combine(newerFingerprint));
        }

        [Test]
        public void NotAvailableShouldAlwaysProduceNotAvailableWhenCombined()
        {
            ConditionalGetFingerprint testFingerprint = new ConditionalGetFingerprint(testDate, testETag);

            ClassicAssert.AreSame(ConditionalGetFingerprint.NOT_AVAILABLE, ConditionalGetFingerprint.NOT_AVAILABLE.Combine(testFingerprint));
            ClassicAssert.AreSame(ConditionalGetFingerprint.NOT_AVAILABLE, testFingerprint.Combine(ConditionalGetFingerprint.NOT_AVAILABLE));
        }
    }
}
