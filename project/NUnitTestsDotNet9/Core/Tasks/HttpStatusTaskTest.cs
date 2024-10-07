using System;
using System.Collections.Generic;
using System.Text;
using Exortech.NetReflector;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Tasks;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
   [TestFixture]
   class HttpStatusTaskTest
   {

		[SetUp]
		public void SetUp()
		{
		}

		[Test]
		public void PopulateFromReflectorWithAllOptions()
		{
			const string xml = @"
<checkHttpStatus>
   <description>ADesc</description>
      <httpRequest uri=""http://example.com/""/>
      
      <successStatusCodes>200,203</successStatusCodes>
      <retries>7</retries>
      <retryDelay units=""seconds"">5</retryDelay>
      <taskTimeout units=""minutes"">5</taskTimeout>
      <includeContent>true</includeContent>
</checkHttpStatus>";

		   HttpStatusTask task = (HttpStatusTask)NetReflector.Read(xml);
         ClassicAssert.AreEqual("ADesc", task.Description);
		   ClassicAssert.NotNull(task.RequestSettings, "Request settings are required");

         ClassicAssert.AreEqual("200,203", task.SuccessStatusCodes);
         ClassicAssert.AreEqual(7, task.Retries);
         ClassicAssert.AreEqual(5000, task.RetryDelay.Millis);
         ClassicAssert.IsTrue(task.HasTimeout);
         ClassicAssert.AreEqual(300000, task.Timeout.Millis);
         ClassicAssert.AreEqual(true, task.IncludeContent);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

      [Test]
      public void PopulateFromReflectorWithOnlyRequiredOptions()
      {
         const string xml = @"
<checkHttpStatus>
      <httpRequest uri=""http://example.com/""/>
</checkHttpStatus>";

         HttpStatusTask task = (HttpStatusTask)NetReflector.Read(xml);
         ClassicAssert.IsNull(task.Description);
         ClassicAssert.NotNull(task.RequestSettings, "Request settings are required");

         ClassicAssert.AreEqual("200", task.SuccessStatusCodes);
         ClassicAssert.AreEqual(3, task.Retries);
         
         ClassicAssert.IsFalse(task.HasTimeout);
         ClassicAssert.IsNull(task.Timeout);
         
         ClassicAssert.IsFalse(task.IncludeContent);
      }

	}
}
