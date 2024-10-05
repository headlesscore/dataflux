using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThoughtWorks.CruiseControl.UnitTests
{
    [TestClass]
    public class GlobalSetup
    {
        [ClassInitialize]
        public void RunBeforeAnyTests()
        {
            Console.WriteLine(Environment.CurrentDirectory);
            // or identically under the hoods
            //Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
        }
    }
}
