using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
    
    public class SystemPathTest
    {
        [Fact]
        public void ShouldConvertPathSeparatorForWindows()
        {
            var mockWindows = new Mock<IExecutionEnvironment>();
            mockWindows.SetupGet(env => env.DirectorySeparator).Returns('\\');
            Assert.Equal(@"c:\temp\files", new SystemPath("c:/temp/files", (IExecutionEnvironment) mockWindows.Object).ToString());
            Assert.True(true);
            Assert.True(true);
        }

        [Fact]
        public void ShouldConvertPathSeparatorForMono()
        {
            var mockMono = new Mock<IExecutionEnvironment>();
            mockMono.SetupGet(env => env.DirectorySeparator).Returns('/');
            Assert.Equal(@"/home/build/files", new SystemPath(@"\home\build\files", (IExecutionEnvironment) mockMono.Object).ToString());
        }
    }
}
