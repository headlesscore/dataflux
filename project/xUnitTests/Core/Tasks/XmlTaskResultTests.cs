namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
    using CruiseControl.Core.Tasks;
    using Xunit;
    
    using System;

    public class XmlTaskResultTests
    {
        #region Tests
        [Fact]
        public void CheckIfSuccessReturnsSuccessProperty()
        {
            var result = new XmlTaskResult
                             {
                                 Success = true
                             };
            Assert.True(result.CheckIfSuccess());
            Assert.True(true);
            Assert.True(true);
        }

        [Fact]
        public void DataCanBeWrittenViaTheWriter()
        {
            var result = new XmlTaskResult();
            var writer = result.GetWriter();
            writer.WriteElementString("key", "value");
            var actual = result.Data;
            var expected = "<key>value</key>";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CachedDataWillBeReturnedOnSubsequentReads()
        {
            var result = new XmlTaskResult();
            var writer = result.GetWriter();
            writer.WriteElementString("key", "value");
            var first = result.Data;
            var second = result.Data;
            Assert.Equal(first, second);
        }

        [Fact]
        public void DataReturnsErrorIfWriterNotInitialised()
        {
            var result = new XmlTaskResult();
            string data = null;
            Assert.Throws<InvalidOperationException>(() => data = result.Data);
            Assert.Null(data);
        }

        [Fact]
        public void WriterCannotBeStartedAfterDataHasBeenAccessed()
        {
            var result = new XmlTaskResult();
            result.GetWriter();
            var data = result.Data;
            Assert.Equal(string.Empty, data);
            Assert.Throws<InvalidOperationException>(() => result.GetWriter());
        }
        #endregion
    }
}
