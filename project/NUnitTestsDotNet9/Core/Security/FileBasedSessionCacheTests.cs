using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Exortech.NetReflector;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Security;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Security
{
    [TestFixture]
    public class FileBasedSessionCacheTests
    {
        private readonly string cacheLocation = Path.Combine(Path.GetTempPath(), "Sessions");

        [SetUp]
        public void SetUpForTest()
        {
            if (!Directory.Exists(cacheLocation)) Directory.CreateDirectory(cacheLocation);
        }

        [TearDown]
        public void CleanUpFromTest()
        {
            if (Directory.Exists(cacheLocation)) Directory.Delete(cacheLocation, true);
        }

        [Test]
        public void InitialiseNoSessions()
        {
            FileBasedSessionCache cache = new FileBasedSessionCache();
            cache.StoreLocation = cacheLocation;
            cache.Initialise();
        }

        [Test]
        public void InitialiseWithSessions()
        {
            // Generate a cache
            FileBasedSessionCache cache = new FileBasedSessionCache();
            cache.StoreLocation = cacheLocation;
            cache.Initialise();
            string sessionToken = cache.AddToCache("johndoe");
            string key = "An item";
            object value = Guid.NewGuid();
            cache.StoreSessionValue(sessionToken, key, value);

            // Reload it
            cache = new FileBasedSessionCache();
            cache.StoreLocation = cacheLocation;
            cache.Initialise();
            object result = cache.RetrieveSessionValue(sessionToken, key);
            ClassicAssert.AreEqual(value.ToString(), result);
        }

        [Test]
        public void AddToCacheReturnsGuid()
        {
            FileBasedSessionCache cache = new FileBasedSessionCache();
            cache.StoreLocation = cacheLocation;
            string sessionToken = cache.AddToCache("johndoe");
            string userName = cache.RetrieveFromCache(sessionToken);
            Guid sessionGuid = new Guid(sessionToken);
            ClassicAssert.AreEqual("johndoe", userName);
            
        }

        [Test]
        public void RemoveFromCacheRemovesSession()
        {
            FileBasedSessionCache cache = new FileBasedSessionCache();
            cache.StoreLocation = cacheLocation;
            string sessionToken = cache.AddToCache("johndoe");
            cache.RemoveFromCache(sessionToken);
            string userName = cache.RetrieveFromCache(sessionToken);
            ClassicAssert.IsNull(userName);
        }

        [Test]
        public void LoadsFromXml()
        {
		    NetReflectorTypeTable typeTable = new NetReflectorTypeTable();
            typeTable.Add(AppDomain.CurrentDomain);
            NetReflectorReader reader = new NetReflectorReader(typeTable);

            object result = reader.Read("<fileBasedCache duration=\"5\" mode=\"Fixed\"/>");
            ClassicAssert.That(result, Is.InstanceOf<FileBasedSessionCache>());
            FileBasedSessionCache cache = result as FileBasedSessionCache;
            ClassicAssert.AreEqual(5, cache.Duration);
            ClassicAssert.AreEqual(SessionExpiryMode.Fixed, cache.ExpiryMode);
        }

        [Test]
        public void StoreSessionValueIsStored()
        {
            FileBasedSessionCache cache = new FileBasedSessionCache();
            cache.StoreLocation = cacheLocation;
            string sessionToken = cache.AddToCache("johndoe");
            string key = "An item";
            object value = Guid.NewGuid();

            cache.StoreSessionValue(sessionToken, key, value);
            object result = cache.RetrieveSessionValue(sessionToken, key);
            ClassicAssert.AreEqual(value, result);
        }

        [Test]
        public void NonStoredValueReturnsNull()
        {
            FileBasedSessionCache cache = new FileBasedSessionCache();
            cache.StoreLocation = cacheLocation;
            string sessionToken = cache.AddToCache("johndoe");
            string key = "An item";

            object result = cache.RetrieveSessionValue(sessionToken, key);
            ClassicAssert.IsNull(result);
        }

        [Test]
        public void InvalidSessionValueReturnsNull()
        {
            FileBasedSessionCache cache = new FileBasedSessionCache();
            cache.StoreLocation = cacheLocation;
            string sessionToken = "Non-existant";
            string key = "An item";

            object result = cache.RetrieveSessionValue(sessionToken, key);
            ClassicAssert.IsNull(result);
        }
    }
}
