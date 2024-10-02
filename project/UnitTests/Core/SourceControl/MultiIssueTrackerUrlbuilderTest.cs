using System;
using Exortech.NetReflector;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{

    [TestFixture]
    public class MultiIssueTrackerUrlbuilderTest
    {

        const string UrlFromConfigFile = "http://jira.public.thoughtworks.org/browse/CCNET-{0}";
        const string searchString = "$";
        const string replacementString = "EndOfLine";


        private string defaultIssueConfig = string.Format(System.Globalization.CultureInfo.CurrentCulture,"<defaultIssueTracker><url>{0}</url></defaultIssueTracker>", UrlFromConfigFile);
        private string regexIssueConfig = string.Format(System.Globalization.CultureInfo.CurrentCulture,"<regexIssueTracker><find>{0}</find><replace>{1}</replace></regexIssueTracker>", searchString, replacementString);

 
        [Test]
        public void ValuePopulation_BadEmptySet()
        {
            string configFile = "<issueUrlBuilder type=\"multiIssueTracker\"></issueUrlBuilder>";
            
            MultiIssueTrackerUrlBuilder multiIssue = new MultiIssueTrackerUrlBuilder();
            ClassicAssert.That(delegate { NetReflector.Read(configFile, multiIssue); },
                        Throws.TypeOf<NetReflectorException>().With.Message.EqualTo(
                            "Missing Xml node (issueTrackers) for required member (ThoughtWorks.CruiseControl.Core.Sourcecontrol.MultiIssueTrackerUrlBuilder.IssueTrackers)." + Environment.NewLine + "Xml: <issueUrlBuilder type=\"multiIssueTracker\"></issueUrlBuilder>"));
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }


        [Test]
        public void ValuePopulation_EmptySet()
        {
            string configFile = "<issueUrlBuilder type=\"multiIssueTracker\"><issueTrackers/></issueUrlBuilder>";

            MultiIssueTrackerUrlBuilder multiIssue = new MultiIssueTrackerUrlBuilder();
            NetReflector.Read(configFile, multiIssue);

        }

        [Test]
        public void ValuePopulation_DefaultIssue()
        {
            string configFile = string.Format(System.Globalization.CultureInfo.CurrentCulture,"<issueUrlBuilder type=\"multiIssueTracker\"><issueTrackers>{0}</issueTrackers></issueUrlBuilder>", defaultIssueConfig);

            MultiIssueTrackerUrlBuilder multiIssue = new MultiIssueTrackerUrlBuilder();
            NetReflector.Read(configFile, multiIssue);

            ClassicAssert.AreEqual(1, multiIssue.IssueTrackers.Length);
            ClassicAssert.That(multiIssue.IssueTrackers[0], Is.InstanceOf<DefaultIssueTrackerUrlBuilder>());
        }

        [Test]
        public void ValuePopulation_RegExIssue()
        {
            string configFile = string.Format(System.Globalization.CultureInfo.CurrentCulture,"<issueUrlBuilder type=\"multiIssueTracker\"><issueTrackers>{0}</issueTrackers></issueUrlBuilder>", regexIssueConfig);

            MultiIssueTrackerUrlBuilder multiIssue = new MultiIssueTrackerUrlBuilder();
            NetReflector.Read(configFile, multiIssue);

            ClassicAssert.AreEqual(1, multiIssue.IssueTrackers.Length);
            ClassicAssert.That(multiIssue.IssueTrackers[0], Is.InstanceOf<RegExIssueTrackerUrlBuilder>());
        }


        [Test]
        public void ValuePopulation_DefaultissueAndRegExIssue()
        {
            string configFile = string.Format(System.Globalization.CultureInfo.CurrentCulture,"<issueUrlBuilder type=\"multiIssueTracker\"><issueTrackers>{0}{1}</issueTrackers></issueUrlBuilder>",defaultIssueConfig, regexIssueConfig);

            MultiIssueTrackerUrlBuilder multiIssue = new MultiIssueTrackerUrlBuilder();
            NetReflector.Read(configFile, multiIssue);

            ClassicAssert.AreEqual(2, multiIssue.IssueTrackers.Length);
            ClassicAssert.That(multiIssue.IssueTrackers[0], Is.InstanceOf<DefaultIssueTrackerUrlBuilder>());
            ClassicAssert.That(multiIssue.IssueTrackers[1], Is.InstanceOf<RegExIssueTrackerUrlBuilder>());
        }
    }
}
