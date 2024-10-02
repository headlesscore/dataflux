using System;
using Exortech.NetReflector;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Label;
using ThoughtWorks.CruiseControl.Remote;
using ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Label
{
	[TestFixture]
	public class AssemblyVersionLabellerTest : IntegrationFixture
	{
		private AssemblyVersionLabeller labeller;

		private static IntegrationResult CreateIntegrationResult()
		{
			IntegrationResult result = IntegrationResultMother.CreateSuccessful();
			return result;
		}

		private static void AddModifications(IntegrationResult result)
		{
			result.Modifications = new Modification[3];
			result.Modifications[0] = ModificationMother.CreateModification("fileName", "folderName",
																			new DateTime(2009, 1, 1), "userName",
																			"comment", "10", "email@address",
																			"http://url");
			result.Modifications[1] = ModificationMother.CreateModification("fileName", "folderName",
																			new DateTime(2009, 1, 3), "userName",
																			"comment", "30", "email@address",
																			"http://url");
			result.Modifications[2] = ModificationMother.CreateModification("fileName", "folderName",
																			new DateTime(2009, 1, 2), "userName",
																			"comment", "20", "email@address",
																			"http://url");
		}

		[SetUp]
		public void SetUp()
		{
			labeller = new AssemblyVersionLabeller();
		}

		[Test]
		public void GenerateLabel()
		{
			IntegrationResult result = CreateIntegrationResult();
			AddModifications(result);
			ClassicAssert.AreEqual(new Version(0, 0, 1, 30).ToString(), labeller.Generate(result));
		}


        [Test]
        public void GenerateLabelWithLabelFormats()
        {
            labeller.MajorLabelFormat = "00";
            labeller.MinorLabelFormat = "000";
            labeller.BuildLabelFormat = "0000";
            labeller.RevisionLabelFormat = "00000";

            IntegrationResult result = CreateIntegrationResult();
            AddModifications(result);
            ClassicAssert.AreEqual("00.000.0001.00030", labeller.Generate(result));
        }



		[Test]
		public void GenerateLabelFromNoMods()
		{
			IntegrationResult result = CreateIntegrationResult();
			ClassicAssert.AreEqual(new Version(0, 0, 1, 0).ToString(), labeller.Generate(result));
		}

		[Test]
		public void PopulateFromConfiguration()
		{
			string xml = @"
				<labeller type='assemblyVersionLabeller'>
					<major>1</major>
					<minor>2</minor>
					<build>1234</build>
					<revision>123456</revision>
					<incrementOnFailure>false</incrementOnFailure>
				</labeller>";
			
			NetReflector.Read(xml, labeller);
			ClassicAssert.AreEqual(1, labeller.Major);
			ClassicAssert.AreEqual(2, labeller.Minor);
			ClassicAssert.AreEqual(1234, labeller.Build);
			ClassicAssert.AreEqual(123456, labeller.Revision);
			ClassicAssert.AreEqual(false, labeller.IncrementOnFailure);
		}

		[Test]
		public void PopulateFromConfigurationUsingOnlyRequiredElementsAndCheckDefaultValues()
		{
			string xml = @"<labeller type='assemblyVersionLabeller' />";

			NetReflector.Read(xml, labeller);
			ClassicAssert.AreEqual(0, labeller.Major);
			ClassicAssert.AreEqual(0, labeller.Minor);
            ClassicAssert.AreEqual(0, labeller.Minor);
            ClassicAssert.AreEqual(-1, labeller.Build);
			ClassicAssert.AreEqual(-1, labeller.Revision);
			ClassicAssert.AreEqual(false, labeller.IncrementOnFailure);
		}

		[Test]
		public void GenerateLabelIterative()
		{
			ClassicAssert.AreEqual(new Version(0, 0, 1, 0).ToString(), labeller.Generate(SuccessfulResult("unknown")));

			IntegrationResult result = SuccessfulResult(new Version(0, 0, 1, 30).ToString());
			AddModifications(result);
			ClassicAssert.AreEqual(new Version(0, 0, 2, 30).ToString(), labeller.Generate(result));

			result.BuildCondition = BuildCondition.ForceBuild;
			ClassicAssert.AreEqual(new Version(0, 0, 2, 30).ToString(), labeller.Generate(result));

			result = SuccessfulResult(new Version(0, 0, 2, 30).ToString());
			AddModifications(result);
			labeller.Major++;
			ClassicAssert.AreEqual(new Version(1, 0, 3, 30).ToString(), labeller.Generate(result));

			result = SuccessfulResult(new Version(0, 0, 3, 30).ToString());
			AddModifications(result);
			labeller.Minor++;
			ClassicAssert.AreEqual(new Version(1, 1, 4, 30).ToString(), labeller.Generate(result));

			result = SuccessfulResult(new Version(0, 0, 4, 30).ToString());
			labeller.Revision = 40;
			ClassicAssert.AreEqual(new Version(1, 1, 5, 40).ToString(), labeller.Generate(result));

			result = SuccessfulResult(new Version(0, 0, 1, 30).ToString());
			AddModifications(result);
			labeller.Major = 5;
			labeller.Minor = 3;
			labeller.Revision = 5467;
			ClassicAssert.AreEqual(new Version(5, 3, 2, 5467).ToString(), labeller.Generate(result));

			result = SuccessfulResult(new Version(5, 0, 1, 30).ToString());
			AddModifications(result);
			labeller.Major = 5;
			labeller.Minor = 3;
			labeller.Build = 1234;
			labeller.Revision = 5467;
			ClassicAssert.AreEqual(new Version(5, 3, 1234, 5467).ToString(), labeller.Generate(result));
		}
	}
}
