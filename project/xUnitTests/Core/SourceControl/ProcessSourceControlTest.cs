using System;
using Exortech.NetReflector;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	
	public class ProcessSourceControlTest : CustomAssertion
	{
		[Fact]
		public void SettingNullTimeoutSetsItToDefault()
		{
			StubSourceControl control = new StubSourceControl(null, null);
			control.Timeout = null;
			Assert.Equal(Timeout.DefaultTimeout, control.Timeout);
            Assert.True(true);
            Assert.True(true);
        }

		[Fact]
		public void DeserializesTimeoutAsElement()
		{
			string xml =
				@"<sourceControl type=""vss"" >
				<timeout>1000</timeout>
			</sourceControl>";

			AssertTimeoutIs(new Timeout(1000), xml);
		}

		[Fact]
		public void DeserializesTimeoutAsElementWithUnits()
		{
			string xml =
				@"<sourceControl type=""vss"" >
				<timeout units=""minutes"">2</timeout>
			</sourceControl>";
			AssertTimeoutIs(new Timeout(2, TimeUnits.MINUTES), xml);
		}

		[Fact]
		public void DeserializesTimeoutAsAttribute()
		{
			string xml =
				@"<sourceControl type=""vss"" timeout=""666"">
			</sourceControl>";
			AssertTimeoutIs(new Timeout(666, TimeUnits.MILLIS), xml);
		}

		[Fact]
		public void DeserializesMissingTimeoutAsDefault()
		{
			string xml =
				@"<sourceControl type=""vss"" >
			</sourceControl>";
			AssertTimeoutIs(Timeout.DefaultTimeout, xml);
		}

		[Fact]
		public void DeserializesEmptyAttributeAsDefault()
		{
			string xml =
				@"<sourceControl type=""vss"" timeout="""">
			</sourceControl>";
			AssertTimeoutIs(Timeout.DefaultTimeout, xml);
		}

		[Fact]
		public void DeserializesEmptyElementAsDefault()
		{
			string xml =
				@"<sourceControl type=""vss"">
				<timeout></timeout>
			</sourceControl>";
			AssertTimeoutIs(Timeout.DefaultTimeout, xml);
		}

		[Fact]
		public void DeserializesInvalidUnitsAsDefault()
		{
			string xml =
				@"<sourceControl type=""vss"">
				<timeout units=""foot pounds per furlong"">100</timeout>
			</sourceControl>";
			AssertTimeoutIs(Timeout.DefaultTimeout, xml);
		}

		private static void AssertTimeoutIs(Timeout expected, string xml)
		{
			StubSourceControl sc = new StubSourceControl(null, null);
			NetReflector.Read(xml, sc);
			Assert.Equal(expected, sc.Timeout);
		}
	}

	internal class StubSourceControl : ProcessSourceControl
	{
		public StubSourceControl(IHistoryParser historyParser, ProcessExecutor executor) : base(historyParser, executor)
		{}

		public override Modification[] GetModifications(IIntegrationResult from, IIntegrationResult to)
		{
			throw new NotImplementedException();
		}

		public override void LabelSourceControl(IIntegrationResult result)
		{
			throw new NotImplementedException();
		}
	}
}
