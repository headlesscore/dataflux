using Exortech.NetReflector;
using Xunit;

using System;
using ThoughtWorks.CruiseControl.Core.Security;

namespace ThoughtWorks.CruiseControl.xUnitTests.Core.Security
{
    
    public class InMemorySessionCacheTest
    {
        [Fact]
        public void InitialiseDoesNothing()
        {
            InMemorySessionCache cache = new InMemorySessionCache();
            cache.Initialise();
        }

        [Fact]
        public void AddToCacheReturnsGuid()
        {
            InMemorySessionCache cache = new InMemorySessionCache();
            string sessionToken = cache.AddToCache("johndoe");
            string userName = cache.RetrieveFromCache(sessionToken);
            Guid sessionGuid = new Guid(sessionToken);
            Assert.Equal("johndoe", userName);
        }

        [Fact]
        public void RemoveFromCacheRemovesSession()
        {
            InMemorySessionCache cache = new InMemorySessionCache();
            string sessionToken = cache.AddToCache("johndoe");
            cache.RemoveFromCache(sessionToken);
            string userName = cache.RetrieveFromCache(sessionToken);
            Assert.Null(userName);
            
        }

        [Fact]
        public void FixedExpiryTimeExpires()
        {
            TestClock clock = new TestClock {Now = DateTime.Now};
            InMemorySessionCache cache = new InMemorySessionCache(clock);
            cache.Duration = 1;
            cache.ExpiryMode = SessionExpiryMode.Fixed;
            string sessionToken = cache.AddToCache("johndoe");
            clock.TimePasses(TimeSpan.FromSeconds(61));
            string userName = cache.RetrieveFromCache(sessionToken);
            Assert.Null(userName);
        }

        [Fact]
        public void SlidingExpiryTimeDoesntExpire()
        {
            TestClock clock = new TestClock {Now = DateTime.Now};
            InMemorySessionCache cache = new InMemorySessionCache(clock);
            cache.Duration = 1;
            cache.ExpiryMode = SessionExpiryMode.Sliding;
            string sessionToken = cache.AddToCache("johndoe");
            clock.TimePasses(TimeSpan.FromSeconds(31));
            string userName = cache.RetrieveFromCache(sessionToken);
            Assert.Equal("johndoe", userName);
            clock.TimePasses(TimeSpan.FromSeconds(31));
            userName = cache.RetrieveFromCache(sessionToken);
            Assert.Equal("johndoe", userName);
        }

        [Fact]
        public void LoadsFromXml()
        {
		    NetReflectorTypeTable typeTable = new NetReflectorTypeTable();
            typeTable.Add(AppDomain.CurrentDomain);
            NetReflectorReader reader = new NetReflectorReader(typeTable);

            object result = reader.Read("<inMemoryCache duration=\"5\" mode=\"Fixed\"/>");
            Assert.IsType<InMemorySessionCache>(result);
            InMemorySessionCache cache = result as InMemorySessionCache;
            Assert.Equal(5, cache.Duration);
            Assert.Equal(SessionExpiryMode.Fixed, cache.ExpiryMode);
        }

        [Fact]
        public void StoreSessionValueIsStored()
        {
            InMemorySessionCache cache = new InMemorySessionCache();
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
            InMemorySessionCache cache = new InMemorySessionCache();
            string sessionToken = cache.AddToCache("johndoe");
            string key = "An item";

            object result = cache.RetrieveSessionValue(sessionToken, key);
            Assert.Null(result);
        }

        [Fact]
        public void InvalidSessionValueReturnsNull()
        {
            InMemorySessionCache cache = new InMemorySessionCache();
            string sessionToken = "Non-existant";
            string key = "An item";

            object result = cache.RetrieveSessionValue(sessionToken, key);
            Assert.Null(result);
        }
    }
}
