using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Exortech.NetReflector;
using Xunit;

using ThoughtWorks.CruiseControl.Core.Security;
using Exortech.NetReflector;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Security
{
    
    public class FileBasedSessionCacheTests
    {
        private readonly string cacheLocation = Path.Combine(Path.GetTempPath(), "Sessions");

        // [SetUp]
        public void SetUpForTest()
        {
            if (!Directory.Exists(cacheLocation)) Directory.CreateDirectory(cacheLocation);
        }

        // [TearDown]
        public void CleanUpFromTest()
        {
            if (Directory.Exists(cacheLocation)) Directory.Delete(cacheLocation, true);
        }

        [Fact]
        public void InitialiseNoSessions()
        {
            FileBasedSessionCache cache = new FileBasedSessionCache();
            cache.StoreLocation = cacheLocation;
            cache.Initialise();
        }

        [Fact]
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
            Assert.Equal(value.ToString(), result);
        }

        [Fact]
        public void AddToCacheReturnsGuid()
        {
            FileBasedSessionCache cache = new FileBasedSessionCache();
            cache.StoreLocation = cacheLocation;
            string sessionToken = cache.AddToCache("johndoe");
            string userName = cache.RetrieveFromCache(sessionToken);
            Guid sessionGuid = new Guid(sessionToken);
            Assert.Equal("johndoe", userName);
            
        }

        [Fact]
        public void RemoveFromCacheRemovesSession()
        {
            FileBasedSessionCache cache = new FileBasedSessionCache();
            cache.StoreLocation = cacheLocation;
            string sessionToken = cache.AddToCache("johndoe");
            cache.RemoveFromCache(sessionToken);
            string userName = cache.RetrieveFromCache(sessionToken);
            Assert.Null(userName);
        }

        [Fact]
        public void LoadsFromXml()
        {
		    NetReflectorTypeTable typeTable = new NetReflectorTypeTable();
            typeTable.Add(AppDomain.CurrentDomain);
            NetReflectorReader reader = new NetReflectorReader(typeTable);

            object result = reader.Read("<fileBasedCache duration=\"5\" mode=\"Fixed\"/>");
            Assert.IsType<FileBasedSessionCache>(result);
            FileBasedSessionCache cache = result as FileBasedSessionCache;
            Assert.Equal(5, cache.Duration);
            Assert.Equal(SessionExpiryMode.Fixed, cache.ExpiryMode);
        }

        [Fact]
        public void StoreSessionValueIsStored()
        {
            FileBasedSessionCache cache = new FileBasedSessionCache();
            cache.StoreLocation = cacheLocation;
            string sessionToken = cache.AddToCache("johndoe");
            string key = "An item";
            object value = Guid.NewGuid();

            cache.StoreSessionValue(sessionToken, key, value);
            object result = cache.RetrieveSessionValue(sessionToken, key);
            Assert.Equal(value, result);
        }

        [Fact]
        public void NonStoredValueReturnsNull()
        {
            FileBasedSessionCache cache = new FileBasedSessionCache();
            cache.StoreLocation = cacheLocation;
            string sessionToken = cache.AddToCache("johndoe");
            string key = "An item";

            object result = cache.RetrieveSessionValue(sessionToken, key);
            Assert.Null(result);
        }

        [Fact]
        public void InvalidSessionValueReturnsNull()
        {
            FileBasedSessionCache cache = new FileBasedSessionCache();
            cache.StoreLocation = cacheLocation;
            string sessionToken = "Non-existant";
            string key = "An item";

            object result = cache.RetrieveSessionValue(sessionToken, key);
            Assert.Null(result);
        }
    }
}
