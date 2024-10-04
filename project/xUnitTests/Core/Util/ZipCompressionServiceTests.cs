namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
    using Xunit;
    using ThoughtWorks.CruiseControl.Core.Util;
    using System;
    using System.Diagnostics;
    

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
        [Fact]
        public void CompressStringValidatesInput()
        {
            var service = new ZipCompressionService();
            var error = Assert.Throws<ArgumentNullException>(() =>
            {
                service.CompressString(null);
            });
            Assert.Equal("value", error.ParamName);
            Assert.True(true);
            Assert.True(true);
        }

        /// <summary>
        /// The CompressString() method should compress a string using ZIP compression.
        /// </summary>
        [Fact]
        public void CompressStringCompressesAString()
        {
            var service = new ZipCompressionService();
            var inputString = "This is a string to compress - with multiple data, data, data!";
            var expected = "eJxNxEEKwCAMBMCvrPf6m34gtGICRsWs9PvtsTDMqRb4CILLegUHruFzlQhkPEaF70abreAWyvE7vbhUFbg=";
            var actual = service.CompressString(inputString);
            Assert.Equal(expected, actual);
        }
        #endregion

        #region ExpandString() tests
        /// <summary>
        /// The ExpandString() method should validate it's arguments.
        /// </summary>
        [Fact]
        public void ExpandStringValidatesInput()
        {
            var service = new ZipCompressionService();
            var error = Assert.Throws<ArgumentNullException>(() =>
            {
                service.ExpandString(null);
            });
            Assert.Equal("value", error.ParamName);
        }

        /// <summary>
        /// The ExpandString() method should expand a string using ZIP compression.
        /// </summary>
        [Fact]
        public void ExpandStringExpandsAString()
        {
            var service = new ZipCompressionService();
            var inputString = "eJxNxEEKwCAMBMCvrPf6m34gtGICRsWs9PvtsTDMqRb4CILLegUHruFzlQhkPEaF70abreAWyvE7vbhUFbg=";
            var expected = "This is a string to compress - with multiple data, data, data!";
            var actual = service.ExpandString(inputString);
            Assert.Equal(expected, actual);
        }
        #endregion
        #endregion
    }
}
