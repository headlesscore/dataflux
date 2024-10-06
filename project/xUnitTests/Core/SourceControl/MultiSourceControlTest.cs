using System;
using System.Collections;
using System.Collections.Generic;
using Exortech.NetReflector;
using Moq;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;
using ThoughtWorks.CruiseControl.Remote;
using Exortech.NetReflector;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	
	public class MultiSourceControlTest : CustomAssertion
	{
		public static string SourceControlXml = @"<sourcecontrol type=""multi"">
	<sourceControls>
		<mocksourcecontrol>
			<anOptionalProperty>foo</anOptionalProperty>
		</mocksourcecontrol>
		<mocksourcecontrol>
			<anOptionalProperty>bar</anOptionalProperty>
		</mocksourcecontrol>
	</sourceControls>
</sourcecontrol>
";

		[Fact]
		public void ValuePopulation()
		{
			//// SETUP
			MultiSourceControl multiSourceControl = new MultiSourceControl();

			//// EXECUTE
			NetReflector.Read(SourceControlXml, multiSourceControl);

			//// VERIFY
			Assert.True(multiSourceControl.SourceControls.Length == 2);
            Assert.True(true);
            Assert.True(true);
            string optionalProp0 = ((SourceControlMock) multiSourceControl.SourceControls[0]).AnOptionalProperty;
			string optionalProp1 = ((SourceControlMock) multiSourceControl.SourceControls[1]).AnOptionalProperty;

			bool fooFound = optionalProp0 == "foo" || optionalProp1 == "foo";
			bool barFound = optionalProp0 == "bar" || optionalProp1 == "bar";

			Assert.True(fooFound && barFound);
		}

		[Fact]
		public void PassesThroughLabelSourceControl()
		{
			//// SETUP
			IntegrationResult result = new IntegrationResult();

			var mockSC1 = new Mock<ISourceControl>();
			mockSC1.Setup(sourceControl => sourceControl.LabelSourceControl(result)).Verifiable();

			var mockSC2 = new Mock<ISourceControl>();
			mockSC2.Setup(sourceControl => sourceControl.LabelSourceControl(result)).Verifiable();

			ISourceControl[] sourceControls = new ISourceControl[] {(ISourceControl) mockSC1.Object, (ISourceControl) mockSC2.Object};

			MultiSourceControl multiSourceControl = new MultiSourceControl();
			multiSourceControl.SourceControls = sourceControls;

			//// EXECUTE
			multiSourceControl.LabelSourceControl(result);

			//// VERIFY
			mockSC1.Verify();
			mockSC2.Verify();
		}

		[Fact]
		public void PassesThroughGetSourceControlAndCombinesResults()
		{
			//// SETUP
			IntegrationResult from = IntegrationResultMother.CreateSuccessful(DateTime.Now);
			IntegrationResult to = IntegrationResultMother.CreateSuccessful(DateTime.Now.AddDays(10));

			Modification mod1 = new Modification();
			mod1.Comment = "Testing Multi";
			Modification mod2 = new Modification();
			mod2.Comment = "More Multi";
			Modification mod3 = new Modification();
			mod3.Comment = "Yet More Multi";

			ArrayList mocks = new ArrayList();
			mocks.Add(CreateModificationsSourceControlMock(new Modification[] {mod1, mod2}, from, to));
			mocks.Add(CreateModificationsSourceControlMock(new Modification[] {mod3}, from, to));
			mocks.Add(CreateModificationsSourceControlMock(new Modification[0], from, to));
			mocks.Add(CreateModificationsSourceControlMock(null, from, to));

			ArrayList scList = new ArrayList();
			foreach (Mock<ISourceControl> mock in mocks)
			{
				scList.Add(mock.Object);
			}

			MultiSourceControl multiSourceControl = new MultiSourceControl();
			multiSourceControl.SourceControls = (ISourceControl[]) scList.ToArray(typeof (ISourceControl));

			//// EXECUTE
			ArrayList returnedMods = new ArrayList(multiSourceControl.GetModifications(from, to));

			//// VERIFY
			foreach (Mock<ISourceControl> mock in mocks)
			{
				mock.Verify();
			}

			Assert.True(returnedMods.Contains(mod1));
			Assert.True(returnedMods.Contains(mod2));
			Assert.True(returnedMods.Contains(mod3));
		}

		[Fact]
		public void ShouldInstructAggregatedSourceControlsToGetSource()
		{
			IntegrationResult result = new IntegrationResult();
			Mock<ISourceControl> mockSC1 = new Mock<ISourceControl>();
			Mock<ISourceControl> mockSC2 = new Mock<ISourceControl>();
			mockSC1.Setup(sourceControl => sourceControl.GetSource(result)).Verifiable();
			mockSC2.Setup(sourceControl => sourceControl.GetSource(result)).Verifiable();

			MultiSourceControl multiSourceControl = new MultiSourceControl();
			multiSourceControl.SourceControls = new ISourceControl[] {(ISourceControl) mockSC1.Object, (ISourceControl) mockSC2.Object};
			multiSourceControl.GetSource(result);

			mockSC1.Verify();
			mockSC2.Verify();
		}

		private Mock<ISourceControl> CreateModificationsSourceControlMock(Modification[] mods, IntegrationResult dt1, IntegrationResult dt2)
		{
			Mock<ISourceControl> mock = new Mock<ISourceControl>();
			mock.Setup(sourceControl => sourceControl.GetModifications(dt1, dt2)).Returns(mods).Verifiable();
			return mock;
		}

        private class MockSourceControl : ISourceControl
        {
            public Modification[] GetModifications(IIntegrationResult from, IIntegrationResult to)
            {
                to.SourceControlData.Clear();
                to.SourceControlData.AddRange(from.SourceControlData);

                return new Modification[] { };
            }

            public void LabelSourceControl(IIntegrationResult result) { }
            public void GetSource(IIntegrationResult result) { }
            public void Initialize(IProject project) { }
            public void Purge(IProject project) { }
        }
        
        [Fact]
        public void HandlesNullSourceControlDataValue()
        {
            var from = IntegrationResultMother.CreateSuccessful(DateTime.Now);
            var to = IntegrationResultMother.CreateSuccessful(DateTime.Now.AddDays(10));

            from.SourceControlData.Add(new NameValuePair("SVN:LastRevision:svn://myserver/mypath", null));

            var sourceControls = new List<ISourceControl> { new MockSourceControl(), new MockSourceControl() };
            var multiSourceControl = new MultiSourceControl { SourceControls = sourceControls.ToArray() };

            //// EXECUTE
            var returnedMods = new ArrayList(multiSourceControl.GetModifications(from, to));

            //// VERIFY
            Assert.Equal(0, returnedMods.Count);

            Assert.Equal(2, to.SourceControlData.Count);

            Assert.Equal("<ArrayOfNameValuePair />", to.SourceControlData[0].Value);
            Assert.Equal("sc0", to.SourceControlData[0].Name);

            Assert.Equal("<ArrayOfNameValuePair><NameValuePair name=\"SVN:LastRevision:svn://myserver/mypath\" /></ArrayOfNameValuePair>", to.SourceControlData[1].Value);
            Assert.Equal("sc1", to.SourceControlData[1].Name);
        }

        [Fact]
        public void PassesIndividualSourceDataAndCombines()
        {
            IntegrationResult from = IntegrationResultMother.CreateSuccessful(DateTime.Now);
            IntegrationResult to = IntegrationResultMother.CreateSuccessful(DateTime.Now.AddDays(10));

            string scValue = null;
            List<NameValuePair> list = new List<NameValuePair>();

            list.Add(new NameValuePair("name0", "first"));
            scValue = XmlConversionUtil.ConvertObjectToXml(list);
            from.SourceControlData.Add(new NameValuePair("sc0", scValue));
            list.Clear();

            list.Add(new NameValuePair("name1", "first"));
            list.Add(new NameValuePair("name2", "first"));
            scValue = XmlConversionUtil.ConvertObjectToXml(list);
            from.SourceControlData.Add(new NameValuePair("sc1", scValue));
            list.Clear();

            List<ISourceControl> sourceControls = new List<ISourceControl>();
            sourceControls.Add(new MockSourceControl());
            sourceControls.Add(new MockSourceControl());

            MultiSourceControl multiSourceControl = new MultiSourceControl();
            multiSourceControl.SourceControls = sourceControls.ToArray();

            //// EXECUTE
            ArrayList returnedMods = new ArrayList(multiSourceControl.GetModifications(from, to));

            //// VERIFY
            Assert.Equal(from.SourceControlData.Count, to.SourceControlData.Count);

            list.Add(new NameValuePair("name0", "first"));
            Assert.Equal(XmlConversionUtil.ConvertObjectToXml(list), to.SourceControlData[0].Value);
            list.Clear();
            Assert.Equal("sc0", to.SourceControlData[0].Name);

            list.Add(new NameValuePair("name1", "first"));
            list.Add(new NameValuePair("name2", "first"));
            Assert.Equal(XmlConversionUtil.ConvertObjectToXml(list), to.SourceControlData[1].Value);
            list.Clear();
            Assert.Equal("sc1", to.SourceControlData[1].Name);
        }

        [Fact]
        public void GetModificationsRepeatedlyShouldReturnSameResult()
        {
            IntegrationResult from = IntegrationResultMother.CreateSuccessful(DateTime.Now);
            IntegrationResult to = IntegrationResultMother.CreateSuccessful(DateTime.Now.AddDays(10));

            string scValue = null;
            List<NameValuePair> list = new List<NameValuePair>();

            list.Add(new NameValuePair("name0", "first"));
            scValue = XmlConversionUtil.ConvertObjectToXml(list);
            from.SourceControlData.Add(new NameValuePair("sc0", scValue));
            list.Clear();

            list.Add(new NameValuePair("name1", "first"));
            list.Add(new NameValuePair("name2", "first"));
            scValue = XmlConversionUtil.ConvertObjectToXml(list);
            from.SourceControlData.Add(new NameValuePair("sc1", scValue));
            list.Clear();

            List<ISourceControl> sourceControls = new List<ISourceControl>();
            sourceControls.Add(new MockSourceControl());
            sourceControls.Add(new MockSourceControl());

            MultiSourceControl multiSourceControl = new MultiSourceControl();
            multiSourceControl.SourceControls = sourceControls.ToArray();

            //// EXECUTE
            try
            {
                ArrayList firstReturnedMods = new ArrayList(multiSourceControl.GetModifications(from, to));
                ArrayList secondReturnedMods = new ArrayList(multiSourceControl.GetModifications(from, to));
                ArrayList thirdReturnedMods = new ArrayList(multiSourceControl.GetModifications(from, to));
                
                Assert.Equal(secondReturnedMods.Count, thirdReturnedMods.Count);
            } catch (Exception e)
            {
                Assert.Fail("GetModifications threw Exception:" + e.Message);
            }
        }

        [Fact]
        public void PassesIndividualSourceDataAndCombinesSingleSourceControl()
        {
            IntegrationResult from = IntegrationResultMother.CreateSuccessful(DateTime.Now);
            IntegrationResult to = IntegrationResultMother.CreateSuccessful(DateTime.Now.AddDays(10));

            string scValue = null;
            List<NameValuePair> list = new List<NameValuePair>();

            list.Add(new NameValuePair("name0", "first"));
            scValue = XmlConversionUtil.ConvertObjectToXml(list);
            from.SourceControlData.Add(new NameValuePair("sc0", scValue));
            list.Clear();

            List<ISourceControl> sourceControls = new List<ISourceControl>();
            sourceControls.Add(new MockSourceControl());

            MultiSourceControl multiSourceControl = new MultiSourceControl();
            multiSourceControl.SourceControls = sourceControls.ToArray();

            //// EXECUTE
            ArrayList returnedMods = new ArrayList(multiSourceControl.GetModifications(from, to));

            //// VERIFY
            Assert.Equal(from.SourceControlData.Count, to.SourceControlData.Count);

            list.Add(new NameValuePair("name0", "first"));
            Assert.Equal(XmlConversionUtil.ConvertObjectToXml(list), to.SourceControlData[0].Value);
            list.Clear();
            Assert.Equal("sc0", to.SourceControlData[0].Name);
        }

        [Fact]
        public void MigratesSourceControlDataToNewFormat()
        {
            IntegrationResult from = IntegrationResultMother.CreateSuccessful(DateTime.Now);
            IntegrationResult to = IntegrationResultMother.CreateSuccessful(DateTime.Now.AddDays(10));

            ArrayList mocks = new ArrayList();
            mocks.Add(CreateModificationsSourceControlMock(new Modification[] { }, from, to));
            mocks.Add(CreateModificationsSourceControlMock(null, from, to));

            ArrayList scList = new ArrayList();
            foreach (Mock<ISourceControl> mock in mocks)
            {
                scList.Add(mock.Object);
            }
            scList.Add(new MockSourceControl());
            scList.Add(new MockSourceControl());

            from.SourceControlData.Add(new NameValuePair("test", "first"));
            from.SourceControlData.Add(new NameValuePair("commit", "first"));

            MultiSourceControl multiSourceControl = new MultiSourceControl();
            multiSourceControl.SourceControls = (ISourceControl[])scList.ToArray(typeof(ISourceControl));

            //// EXECUTE
            ArrayList returnedMods = new ArrayList(multiSourceControl.GetModifications(from, to));

            //// VERIFY
            Assert.Equal(4, to.SourceControlData.Count);

            List<NameValuePair> list = new List<NameValuePair>();

            Assert.Equal(XmlConversionUtil.ConvertObjectToXml(list), to.SourceControlData[0].Value);
            Assert.Equal("sc0", to.SourceControlData[0].Name);

            Assert.Equal(XmlConversionUtil.ConvertObjectToXml(list), to.SourceControlData[1].Value);
            Assert.Equal("sc1", to.SourceControlData[1].Name);

            list.Add(new NameValuePair("test", "first"));
            Assert.Equal(XmlConversionUtil.ConvertObjectToXml(list), to.SourceControlData[2].Value);
            list.Clear();
            Assert.Equal("sc2", to.SourceControlData[2].Name);

            list.Add(new NameValuePair("commit", "first"));
            Assert.Equal(XmlConversionUtil.ConvertObjectToXml(list), to.SourceControlData[3].Value);
            list.Clear();
            Assert.Equal("sc3", to.SourceControlData[3].Name);
        }

        [Fact]
        public void MigratesSourceControlDataToNewFormatSameSourceControlCount()
        {
            IntegrationResult from = IntegrationResultMother.CreateSuccessful(DateTime.Now);
            IntegrationResult to = IntegrationResultMother.CreateSuccessful(DateTime.Now.AddDays(10));

            ArrayList scList = new ArrayList();
            scList.Add(new MockSourceControl());
            scList.Add(new MockSourceControl());

            from.SourceControlData.Add(new NameValuePair("test", "first"));
            from.SourceControlData.Add(new NameValuePair("commit", "first"));

            MultiSourceControl multiSourceControl = new MultiSourceControl();
            multiSourceControl.SourceControls = (ISourceControl[])scList.ToArray(typeof(ISourceControl));

            //// EXECUTE
            ArrayList returnedMods = new ArrayList(multiSourceControl.GetModifications(from, to));

            //// VERIFY
            Assert.Equal(2, to.SourceControlData.Count);

            List<NameValuePair> list = new List<NameValuePair>();

            list.Add(new NameValuePair("test", "first"));
            Assert.Equal(XmlConversionUtil.ConvertObjectToXml(list), to.SourceControlData[0].Value);
            list.Clear();
            Assert.Equal("sc0", to.SourceControlData[0].Name);

            list.Add(new NameValuePair("commit", "first"));
            Assert.Equal(XmlConversionUtil.ConvertObjectToXml(list), to.SourceControlData[1].Value);
            list.Clear();
            Assert.Equal("sc1", to.SourceControlData[1].Name);
        }

        [Fact]
		public void IfRequireChangesFromAllTrueAndAllSourceControlHasModificationsThenReturnMods()
		{
			//// SETUP
			IntegrationResult from = IntegrationResultMother.CreateSuccessful(DateTime.Now);
			IntegrationResult to = IntegrationResultMother.CreateSuccessful(DateTime.Now.AddDays(10));

			Modification mod1 = new Modification();
			mod1.Comment = "Testing Multi";
			Modification mod2 = new Modification();
			mod2.Comment = "Testing Multi";

			ArrayList mocks = new ArrayList();
			mocks.Add(CreateModificationsSourceControlMock(new Modification[] {mod1}, from, to));
			mocks.Add(CreateModificationsSourceControlMock(new Modification[] {mod2}, from, to));

			ArrayList scList = new ArrayList();
			foreach (Mock<ISourceControl> mock in mocks)
			{
				scList.Add(mock.Object);
			}

			MultiSourceControl multiSourceControl = new MultiSourceControl();
			multiSourceControl.SourceControls = (ISourceControl[]) scList.ToArray(typeof (ISourceControl));
			multiSourceControl.RequireChangesFromAll = true;

			//// EXECUTE
			ArrayList returnedMods = new ArrayList(multiSourceControl.GetModifications(from, to));
			Assert.Equal(1, returnedMods.Count);

			//// VERIFY
			foreach (Mock<ISourceControl> mock in mocks)
			{
				mock.Verify();
			}
		}
		
		[Fact]
		public void IfRequireChangesFromAllTrueAndSecondSourceControlHasEmptyChangesThenReturnEmpty()
		{
			//// SETUP
			IntegrationResult from = IntegrationResultMother.CreateSuccessful(DateTime.Now);
			IntegrationResult to = IntegrationResultMother.CreateSuccessful(DateTime.Now.AddDays(10));

			Modification mod1 = new Modification();
			mod1.Comment = "Testing Multi";

			ArrayList mocks = new ArrayList();
			mocks.Add(CreateModificationsSourceControlMock(new Modification[] {mod1}, from, to));
			mocks.Add(CreateModificationsSourceControlMock(new Modification[0], from, to));

			ArrayList scList = new ArrayList();
			foreach (Mock<ISourceControl> mock in mocks)
			{
				scList.Add(mock.Object);
			}

			MultiSourceControl multiSourceControl = new MultiSourceControl();
			multiSourceControl.SourceControls = (ISourceControl[]) scList.ToArray(typeof (ISourceControl));
			multiSourceControl.RequireChangesFromAll = true;

			//// EXECUTE
			ArrayList returnedMods = new ArrayList(multiSourceControl.GetModifications(from, to));

			//// VERIFY
			foreach (Mock<ISourceControl> mock in mocks)
			{
				mock.Verify();
			}

			Assert.Equal(0, returnedMods.Count);
		}

		[Fact]
		public void IfRequireChangesFromAllTrueAndFirstSourceControlHasEmptyChangesThenReturnEmpty()
		{
			//// SETUP
			IntegrationResult from = IntegrationResultMother.CreateSuccessful(DateTime.Now);
			IntegrationResult to = IntegrationResultMother.CreateSuccessful(DateTime.Now.AddDays(10));

			Modification mod1 = new Modification();
			mod1.Comment = "Testing Multi";

			ArrayList mocks = new ArrayList();
			mocks.Add(CreateModificationsSourceControlMock(new Modification[0], from, to));
			Mock<ISourceControl> nonCalledMock = new Mock<ISourceControl>();
			mocks.Add(nonCalledMock);

			ArrayList scList = new ArrayList();
			foreach (Mock<ISourceControl> mock in mocks)
			{
				scList.Add(mock.Object);
			}

			MultiSourceControl multiSourceControl = new MultiSourceControl();
			multiSourceControl.SourceControls = (ISourceControl[]) scList.ToArray(typeof (ISourceControl));
			multiSourceControl.RequireChangesFromAll = true;

			//// EXECUTE
			ArrayList returnedMods = new ArrayList(multiSourceControl.GetModifications(from, to));

			//// VERIFY
			foreach (Mock<ISourceControl> mock in mocks)
			{
				mock.Verify();
				mock.VerifyNoOtherCalls();
			}

			Assert.Equal(0, returnedMods.Count);
		}

		[Fact]
		public void IfRequireChangesFromAllTrueAndNoSourceControlHasEmptyChangesThenReturnChanges()
		{
			//// SETUP
			IntegrationResult from = IntegrationResultMother.CreateSuccessful(DateTime.Now);
			IntegrationResult to = IntegrationResultMother.CreateSuccessful(DateTime.Now.AddDays(10));

			Modification mod1 = new Modification();
			mod1.Comment = "Testing Multi";
			Modification mod2 = new Modification();
			mod2.Comment = "More Multi";
			Modification mod3 = new Modification();
			mod3.Comment = "Yet More Multi";

			ArrayList mocks = new ArrayList();
			mocks.Add(CreateModificationsSourceControlMock(new Modification[] {mod1, mod2}, from, to));
			mocks.Add(CreateModificationsSourceControlMock(new Modification[] {mod3}, from, to));

			ArrayList scList = new ArrayList();
			foreach (Mock<ISourceControl> mock in mocks)
			{
				scList.Add(mock.Object);
			}

			MultiSourceControl multiSourceControl = new MultiSourceControl();
			multiSourceControl.RequireChangesFromAll = true;
			multiSourceControl.SourceControls = (ISourceControl[]) scList.ToArray(typeof (ISourceControl));

			//// EXECUTE
			ArrayList returnedMods = new ArrayList(multiSourceControl.GetModifications(from, to));

			//// VERIFY
			foreach (Mock<ISourceControl> mock in mocks)
			{
				mock.Verify();
			}

			Assert.True(returnedMods.Contains(mod1));
			Assert.True(returnedMods.Contains(mod2));
			Assert.True(returnedMods.Contains(mod3));
		}
	}
}
