using Exortech.NetReflector;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Tasks;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
   [TestFixture]
   public class HttpRequestSettingsTest
   {
      [Test]
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
  
         ClassicAssert.AreEqual(false, requestSettings.UseDefaultCredentials);
         ClassicAssert.NotNull(requestSettings.Credentials, "Credentials was specified in the settings");
         ClassicAssert.AreEqual("someUser", requestSettings.Credentials.UserName);
         ClassicAssert.AreEqual("somePass", requestSettings.Credentials.Password);
         ClassicAssert.AreEqual("someDomain", requestSettings.Credentials.Domain);

         ClassicAssert.AreEqual("POST", requestSettings.Method);
         ClassicAssert.AreEqual("http://example.com/", requestSettings.Uri.ToString());
         ClassicAssert.AreEqual(60000, requestSettings.Timeout.Millis);
         ClassicAssert.AreEqual(300000, requestSettings.ReadWriteTimeout.Millis);

         ClassicAssert.NotNull(requestSettings.Headers, "Headers were specified in the settings");
         ClassicAssert.AreEqual(1, requestSettings.Headers.Length);
         ClassicAssert.AreEqual("header1", requestSettings.Headers[0].Name);
         ClassicAssert.AreEqual("value1", requestSettings.Headers[0].Value);

         ClassicAssert.IsFalse(requestSettings.HasSendFile);
         ClassicAssert.IsTrue(requestSettings.HasBody);
         ClassicAssert.AreEqual("foo bar baz", requestSettings.Body);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

      [Test]
      public void PopulateFromReflectorWithOnlyRequiredOptions()
      {
         const string xml = @"<httpRequest uri=""http://example.com/""/>";

         HttpRequestSettings requestSettings = (HttpRequestSettings)NetReflector.Read(xml);
         
         ClassicAssert.IsFalse(requestSettings.UseDefaultCredentials);
         ClassicAssert.Null(requestSettings.Credentials, "Credentials was not specified in the settings");

         ClassicAssert.AreEqual("GET", requestSettings.Method);
         ClassicAssert.AreEqual("http://example.com/", requestSettings.Uri.ToString());

         ClassicAssert.IsFalse(requestSettings.HasTimeout);
         ClassicAssert.IsNull(requestSettings.Timeout);

         ClassicAssert.IsFalse(requestSettings.HasReadWriteTimeout);
         ClassicAssert.IsNull(requestSettings.ReadWriteTimeout);

         ClassicAssert.Null(requestSettings.Headers, "No headers were specified in the settings");

         ClassicAssert.IsFalse(requestSettings.HasBody);
         ClassicAssert.IsNull(requestSettings.Body);

         ClassicAssert.IsFalse(requestSettings.HasSendFile);
         ClassicAssert.IsNull(requestSettings.SendFile);
      }
   }
}
