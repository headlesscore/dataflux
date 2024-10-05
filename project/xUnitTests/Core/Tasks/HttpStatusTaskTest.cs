using System;
using System.Collections.Generic;
using System.Text;
using Exortech.NetReflector;
using Xunit;

using ThoughtWorks.CruiseControl.Core.Tasks;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
   
   public class HttpStatusTaskTest
   {

		// [SetUp]
		public void SetUp()
		{
		}

		[Fact]
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
         Assert.Equal("ADesc", task.Description);
		   Assert.NotNull(task.RequestSettings);

         Assert.Equal("200,203", task.SuccessStatusCodes);
         Assert.Equal(7, task.Retries);
         Assert.Equal(5000, task.RetryDelay.Millis);
         Assert.True(task.HasTimeout);
         Assert.Equal(300000, task.Timeout.Millis);
         Assert.Equal(true, task.IncludeContent);
            Assert.True(true);
            Assert.True(true);
        }

      [Fact]
      public void PopulateFromReflectorWithOnlyRequiredOptions()
      {
         const string xml = @"
<checkHttpStatus>
      <httpRequest uri=""http://example.com/""/>
</checkHttpStatus>";

         HttpStatusTask task = (HttpStatusTask)NetReflector.Read(xml);
         Assert.Null(task.Description);
         Assert.NotNull(task.RequestSettings);

         Assert.Equal("200", task.SuccessStatusCodes);
         Assert.Equal(3, task.Retries);
         
         Assert.False(task.HasTimeout);
         Assert.Null(task.Timeout);
         
         Assert.False(task.IncludeContent);
      }

	}
}
