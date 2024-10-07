using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
    [TestFixture]
    public class SystemPathTest
    {
        [Test]
        public void ShouldConvertPathSeparatorForWindows()
        {
            var mockWindows = new Mock<IExecutionEnvironment>();
            mockWindows.SetupGet(env => env.DirectorySeparator).Returns('\\');
            ClassicAssert.AreEqual(@"c:\temp\files", new SystemPath("c:/temp/files", (IExecutionEnvironment) mockWindows.Object).ToString());
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void ShouldConvertPathSeparatorForMono()
        {
            var mockMono = new Mock<IExecutionEnvironment>();
            mockMono.SetupGet(env => env.DirectorySeparator).Returns('/');
            ClassicAssert.AreEqual(@"/home/build/files", new SystemPath(@"\home\build\files", (IExecutionEnvironment) mockMono.Object).ToString());
        }
    }
}
