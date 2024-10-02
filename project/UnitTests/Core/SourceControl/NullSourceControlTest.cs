using System;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	[TestFixture]
	public class NullSourceControlTest
	{
		private NullSourceControl sourceControl;

		[SetUp]
		public void Setup()
		{
			sourceControl = new NullSourceControl();
		}

		[Test]
		public void ShouldReturnEmptyListOfModifications()
		{
			ClassicAssert.AreEqual(0, sourceControl.GetModifications(IntegrationResultMother.CreateSuccessful(DateTime.MinValue), IntegrationResultMother.CreateSuccessful(DateTime.MaxValue)).Length);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

		[Test]
		public void ShouldReturnSilentlyForOtherOperations()
		{
			sourceControl.GetSource(null);
			sourceControl.Initialize(null);
			sourceControl.Purge(null);
			sourceControl.LabelSourceControl(null);
		}

        [Test]
        public void ShouldFailGetModsWhenFailModsIsTrue()
        {
            sourceControl.FailGetModifications = true;
            ClassicAssert.That(delegate { sourceControl.GetModifications(null, null); },
                        Throws.TypeOf<Exception>().With.Message.EqualTo("Failing GetModifications"));
        }

        [Test]
        public void ShouldFailGetSourceWhenFailGetSourceIsTrue()
        {
            sourceControl.FailGetSource = true;
            ClassicAssert.That(delegate { sourceControl.GetSource(null); },
                        Throws.TypeOf<Exception>().With.Message.EqualTo("Failing getting the source"));
        }

        [Test]
        public void ShouldFailLabelSourceWhenFailLabelSourceIsTrue()
        {
            sourceControl.FailLabelSourceControl = true;
            ClassicAssert.That(delegate { sourceControl.LabelSourceControl(null); },
                        Throws.TypeOf<Exception>().With.Message.EqualTo("Failing label source control"));
        }

        [Test]
        public void ShouldReturnNonEmptyListOfModificationsWhenAlwaysModifiedIsTrue()
        {
            sourceControl.AlwaysModified = true;
            ClassicAssert.AreNotEqual(0, sourceControl.GetModifications(IntegrationResultMother.CreateSuccessful(DateTime.MinValue), IntegrationResultMother.CreateSuccessful(DateTime.MaxValue)).Length);
        }

    }
}
