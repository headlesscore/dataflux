﻿namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
    using NUnit.Framework;
    using ThoughtWorks.CruiseControl.Core.Util;
    using System;
    using System.Diagnostics;
    using NUnit.Framework.Legacy;

    /// <summary>
    /// Unit tests for <see cref="ZipCompressionService"/>.
    /// </summary>
    public class ZipCompressionServiceTests
    {
        #region Tests
        #region CompressString() tests
        /// <summary>
        /// The CompressString() method should validate it's arguments.
        /// </summary>
        [Test]
        public void CompressStringValidatesInput()
        {
            var service = new ZipCompressionService();
            var error = ClassicAssert.Throws<ArgumentNullException>(() =>
            {
                service.CompressString(null);
            });
            ClassicAssert.AreEqual("value", error.ParamName);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        /// <summary>
        /// The CompressString() method should compress a string using ZIP compression.
        /// </summary>
        [Test]
        public void CompressStringCompressesAString()
        {
            var service = new ZipCompressionService();
            var inputString = "This is a string to compress - with multiple data, data, data!";
            var expected = "eJxNxEEKwCAMBMCvrPf6m34gtGICRsWs9PvtsTDMqRb4CILLegUHruFzlQhkPEaF70abreAWyvE7vbhUFbg=";
            var actual = service.CompressString(inputString);
            ClassicAssert.AreEqual(expected, actual);
        }
        #endregion

        #region ExpandString() tests
        /// <summary>
        /// The ExpandString() method should validate it's arguments.
        /// </summary>
        [Test]
        public void ExpandStringValidatesInput()
        {
            var service = new ZipCompressionService();
            var error = ClassicAssert.Throws<ArgumentNullException>(() =>
            {
                service.ExpandString(null);
            });
            ClassicAssert.AreEqual("value", error.ParamName);
        }

        /// <summary>
        /// The ExpandString() method should expand a string using ZIP compression.
        /// </summary>
        [Test]
        public void ExpandStringExpandsAString()
        {
            var service = new ZipCompressionService();
            var inputString = "eJxNxEEKwCAMBMCvrPf6m34gtGICRsWs9PvtsTDMqRb4CILLegUHruFzlQhkPEaF70abreAWyvE7vbhUFbg=";
            var expected = "This is a string to compress - with multiple data, data, data!";
            var actual = service.ExpandString(inputString);
            ClassicAssert.AreEqual(expected, actual);
        }
        #endregion
        #endregion
    }
}
