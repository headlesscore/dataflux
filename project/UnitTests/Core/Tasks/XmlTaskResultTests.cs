namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
    using CruiseControl.Core.Tasks;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using System;

    public class XmlTaskResultTests
    {
        #region Tests
        [Test]
        public void CheckIfSuccessReturnsSuccessProperty()
        {
            var result = new XmlTaskResult
                             {
                                 Success = true
                             };
            ClassicAssert.IsTrue(result.CheckIfSuccess());
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void DataCanBeWrittenViaTheWriter()
        {
            var result = new XmlTaskResult();
            var writer = result.GetWriter();
            writer.WriteElementString("key", "value");
            var actual = result.Data;
            var expected = "<key>value</key>";
            ClassicAssert.AreEqual(expected, actual);
        }

        [Test]
        public void CachedDataWillBeReturnedOnSubsequentReads()
        {
            var result = new XmlTaskResult();
            var writer = result.GetWriter();
            writer.WriteElementString("key", "value");
            var first = result.Data;
            var second = result.Data;
            ClassicAssert.AreEqual(first, second);
        }

        [Test]
        public void DataReturnsErrorIfWriterNotInitialised()
        {
            var result = new XmlTaskResult();
            string data = null;
            ClassicAssert.Throws<InvalidOperationException>(() => data = result.Data);
            ClassicAssert.IsNull(data);
        }

        [Test]
        public void WriterCannotBeStartedAfterDataHasBeenAccessed()
        {
            var result = new XmlTaskResult();
            result.GetWriter();
            var data = result.Data;
            ClassicAssert.AreEqual(string.Empty, data);
            ClassicAssert.Throws<InvalidOperationException>(() => result.GetWriter());
        }
        #endregion
    }
}
