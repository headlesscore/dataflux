using System;
using Xunit;

using ThoughtWorks.CruiseControl.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	
	public class NullSourceControlTest
	{
		private NullSourceControl sourceControl;

		// [SetUp]
		public void Setup()
		{
			sourceControl = new NullSourceControl();
		}

		[Fact]
		public void ShouldReturnEmptyListOfModifications()
		{
			Assert.Empty(sourceControl.GetModifications(IntegrationResultMother.CreateSuccessful(DateTime.MinValue), IntegrationResultMother.CreateSuccessful(DateTime.MaxValue)));
        }

		[Fact]
		public void ShouldReturnSilentlyForOtherOperations()
		{
			sourceControl.GetSource(null);
			sourceControl.Initialize(null);
			sourceControl.Purge(null);
			sourceControl.LabelSourceControl(null);
		}

        [Fact]
        public void ShouldFailGetModsWhenFailModsIsTrue()
        {
            sourceControl.FailGetModifications = true;
            Assert.Equal("Failing GetModifications",
                Assert.Throws<Exception>(() => { sourceControl.GetModifications(null, null); }).Message);
        }

        [Fact]
        public void ShouldFailGetSourceWhenFailGetSourceIsTrue()
        {
            sourceControl.FailGetSource = true;
            Assert.Equal("Failing getting the source",
                Assert.Throws<Exception>(()=> { sourceControl.GetSource(null); }).Message);
        }

        [Fact]
        public void ShouldFailLabelSourceWhenFailLabelSourceIsTrue()
        {
            sourceControl.FailLabelSourceControl = true;
            Assert.Equal("Failing label source control",
                Assert.Throws<Exception>(()=> { sourceControl.LabelSourceControl(null); }).Message);
        }

        [Fact]
        public void ShouldReturnNonEmptyListOfModificationsWhenAlwaysModifiedIsTrue()
        {
            sourceControl.AlwaysModified = true;
            Assert.NotEmpty(sourceControl.GetModifications(IntegrationResultMother.CreateSuccessful(DateTime.MinValue), IntegrationResultMother.CreateSuccessful(DateTime.MaxValue)));
        }

    }
}
