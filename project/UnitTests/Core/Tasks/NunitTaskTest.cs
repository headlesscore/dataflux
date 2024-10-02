using System.IO;
using Exortech.NetReflector;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
	[TestFixture]
	public class NUnitTaskTest : ProcessExecutorTestFixtureBase
	{
		private NUnitTask task;

		[Test]
		public void LoadWithSingleAssemblyNunitPath()
		{
			const string xml = @"<nunit>
	<path>d:\temp\nunit-console.exe</path>
	<assemblies>
		<assembly>foo.dll</assembly>
	</assemblies>
	<outputfile>c:\testfile.xml</outputfile>
	<timeout>50</timeout>
</nunit>";
			task = (NUnitTask) NetReflector.Read(xml);
			ClassicAssert.AreEqual(@"d:\temp\nunit-console.exe", task.NUnitPath);
			ClassicAssert.AreEqual(1, task.Assemblies.Length);
			ClassicAssert.AreEqual("foo.dll", task.Assemblies[0]);
			ClassicAssert.AreEqual(@"c:\testfile.xml", task.OutputFile);
			ClassicAssert.AreEqual(50, task.Timeout);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

		[Test]
		public void LoadWithMultipleAssemblies()
		{
			const string xml = @"<nunit>
							 <path>d:\temp\nunit-console.exe</path>
				             <assemblies>
			                     <assembly>foo.dll</assembly>
								 <assembly>bar.dll</assembly>
							</assemblies>
						 </nunit>";

			task = (NUnitTask) NetReflector.Read(xml);
			AssertEqualArrays(new string[] {"foo.dll", "bar.dll"}, task.Assemblies);
		}

        [Test]
        public void LoadWithExcludedCategories()
        {
            const string xml = @"<nunit>
							 <path>d:\temp\nunit-console.exe</path>
				             <assemblies>
			                     <assembly>foo.dll</assembly>
								 <assembly>bar.dll</assembly>
							</assemblies>
                            <excludedCategories>
				                <excludedCategory>Category1</excludedCategory>
				                <excludedCategory>Category 2</excludedCategory>				
				                <excludedCategory> </excludedCategory>				
			                </excludedCategories>
						 </nunit>";

            task = (NUnitTask) NetReflector.Read(xml);
            ClassicAssert.AreEqual(3, task.ExcludedCategories.Length);
        }

		[Test]
		public void HandleNUnitTaskFailure()
		{
			CreateProcessExecutorMock(NUnitTask.DefaultPath);
			ExpectToExecuteAndReturn(SuccessfulProcessResult());
			IIntegrationResult result = IntegrationResult();
			result.ArtifactDirectory = Path.GetTempPath();

			task = new NUnitTask((ProcessExecutor) mockProcessExecutor.Object);
			task.Assemblies = new string[] {"foo.dll"};
			task.Run(result);

			ClassicAssert.AreEqual(1, result.TaskResults.Count);
		    ClassicAssert.That(result.TaskOutput, Is.Empty);
			Verify();
		}
	}
}
