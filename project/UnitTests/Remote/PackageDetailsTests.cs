namespace ThoughtWorks.CruiseControl.UnitTests.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using ThoughtWorks.CruiseControl.Remote;

    [TestFixture]
    public class PackageDetailsTests
    {
        #region Tests
        [Test]
        public void ConstructorSetsName()
        {
            var package = new PackageDetails("The name");
            ClassicAssert.AreEqual("The name", package.FileName);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void AllPropertiesCanBeSetAndRetrieved()
        {
            var package = new PackageDetails();
            var theName = "the name";
            var theBuild = "buildLabel.x";
            var theDate = new DateTime(2010, 1, 1);
            var theSize = 123456;
            var theCount = 4;
            var theFile = "The Name of Some file";

            package.Name = theName;
            package.BuildLabel = theBuild;
            package.DateTime = theDate;
            package.Size = theSize;
            package.NumberOfFiles = theCount;
            package.FileName = theFile;

            ClassicAssert.AreEqual(theName, package.Name);
            ClassicAssert.AreEqual(theBuild, package.BuildLabel);
            ClassicAssert.AreEqual(theDate, package.DateTime);
            ClassicAssert.AreEqual(theSize, package.Size);
            ClassicAssert.AreEqual(theCount, package.NumberOfFiles);
            ClassicAssert.AreEqual(theFile, package.FileName);
        }

        [Test]
        public void PassThroughSerialisation()
        {
            TestHelpers.EnsureLanguageIsValid();
            var package = new PackageDetails();
            var theName = "the name";
            var theBuild = "buildLabel.x";
            var theDate = new DateTime(2010, 1, 1);
            var theSize = 123456;
            var theCount = 4;
            var theFile = "The Name of Some file";

            package.Name = theName;
            package.BuildLabel = theBuild;
            package.DateTime = theDate;
            package.Size = theSize;
            package.NumberOfFiles = theCount;
            package.FileName = theFile;
            var result = TestHelpers.RunSerialisationTest(package);

            ClassicAssert.IsNotNull(result);
            ClassicAssert.IsInstanceOf<PackageDetails>(result);
            var actualPackage = result as PackageDetails;
            ClassicAssert.AreEqual(theName, actualPackage.Name);
            ClassicAssert.AreEqual(theBuild, actualPackage.BuildLabel);
            ClassicAssert.AreEqual(theDate, actualPackage.DateTime);
            ClassicAssert.AreEqual(theSize, actualPackage.Size);
            ClassicAssert.AreEqual(theCount, actualPackage.NumberOfFiles);
            ClassicAssert.AreEqual(theFile, actualPackage.FileName);
        }
        #endregion
    }
}
