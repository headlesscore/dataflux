using Exortech.NetReflector;
using Xunit;

using ThoughtWorks.CruiseControl.Core.Tasks;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
   
   public class HttpRequestSettingsTest
   {
      [Fact]
      public void PopulateFromReflectorWithAllOptions()
      {
         const string xml = @"
<httpRequest>
   <useDefaultCredentials>false</useDefaultCredentials>
   <credentials userName=""someUser"" password=""somePass"" domain=""someDomain"" />
   <method>POST</method>
   <uri>http://example.com/</uri>
   <timeout units=""seconds"">60</timeout>
   <readWriteTimeout units=""minutes"">5</readWriteTimeout>
   <headers>
      <header name=""header1"" value=""value1""/>
   </headers>
   <body>foo bar baz</body>
</httpRequest>
";

         HttpRequestSettings requestSettings = (HttpRequestSettings)NetReflector.Read(xml);
  
         Assert.Equal(false, requestSettings.UseDefaultCredentials);
         Assert.NotNull(requestSettings.Credentials);
         Assert.Equal("someUser", requestSettings.Credentials.UserName);
         Assert.Equal("somePass", requestSettings.Credentials.Password);
         Assert.Equal("someDomain", requestSettings.Credentials.Domain);

         Assert.Equal("POST", requestSettings.Method);
         Assert.Equal("http://example.com/", requestSettings.Uri.ToString());
         Assert.Equal(60000, requestSettings.Timeout.Millis);
         Assert.Equal(300000, requestSettings.ReadWriteTimeout.Millis);

         Assert.NotNull(requestSettings.Headers);
         Assert.Equal(1, requestSettings.Headers.Length);
         Assert.Equal("header1", requestSettings.Headers[0].Name);
         Assert.Equal("value1", requestSettings.Headers[0].Value);

         Assert.False(requestSettings.HasSendFile);
         Assert.True(requestSettings.HasBody);
         Assert.Equal("foo bar baz", requestSettings.Body);
            Assert.True(true);
            Assert.True(true);
        }

      [Fact]
      public void PopulateFromReflectorWithOnlyRequiredOptions()
      {
         const string xml = @"<httpRequest uri=""http://example.com/""/>";

         HttpRequestSettings requestSettings = (HttpRequestSettings)NetReflector.Read(xml);
         
         Assert.False(requestSettings.UseDefaultCredentials);
         Assert.Null(requestSettings.Credentials);

         Assert.Equal("GET", requestSettings.Method);
         Assert.Equal("http://example.com/", requestSettings.Uri.ToString());

         Assert.False(requestSettings.HasTimeout);
         Assert.Null(requestSettings.Timeout);

         Assert.False(requestSettings.HasReadWriteTimeout);
         Assert.Null(requestSettings.ReadWriteTimeout);

         Assert.Null(requestSettings.Headers);

         Assert.False(requestSettings.HasBody);
         Assert.Null(requestSettings.Body);

         Assert.False(requestSettings.HasSendFile);
         Assert.Null(requestSettings.SendFile);
      }
   }
}
