using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace CachingFramework.Redis.UnitTest
{
    [TestClass]
    public class UnitTestRedis
    {
        private CacheContext _cache;
        private string _defaultConfig;
        [TestInitialize]
        public void Initialize()
        {
            // Config doc: https://github.com/StackExchange/StackExchange.Redis/blob/master/Docs/Configuration.md
            _defaultConfig = "192.168.15.15:7001,192.168.15.15:7006,192.168.15.15:7002,192.168.15.15:7003,192.168.15.15:7004,192.168.15.15:7005,192.168.15.15:7000,connectRetry=10,syncTimeout=5000,abortConnect=false,keepAlive=10";
            _cache = new CacheContext(_defaultConfig);
        }

        [TestMethod]
        public void UT_CacheByteArray()
        {
            string key = "UT_CacheByteArray";
            Jpeg jpeg = new Jpeg()
            {
                Data = Enumerable.Range(0, 200000)
                       .Select(i => (byte)((i * 223) % 256)).ToArray()
            };
            _cache.Remove(key);
            _cache.SetObject(key, jpeg);
            var jpeg2 = _cache.GetObject<Jpeg>(key);
            Assert.IsTrue(Enumerable.SequenceEqual(jpeg.Data, jpeg2.Data));
        }

        [TestMethod]
        public void UT_CacheAddGet()
        {
            // Test the Add and Get methods
            var users = GetUsers();
            string key = "UT_CacheAddGet";
            _cache.Remove(key);

            _cache.SetObject(key, users[1]);
            _cache.SetObject(key, users[0]);
            var user = _cache.GetObject<User>(key);
            Assert.AreEqual(1, user.Id);
            Assert.AreEqual(2, user.Deparments[0].Size);
            Assert.AreEqual("one", user.Deparments[0].Location.Name);
        }

        [TestMethod]
        public void UT_CacheFetch()
        {
            // Test the Fetch method
            string key = "UT_CacheFetch";
            int count = 0;
            _cache.Remove(key);
            _cache.FetchObject(key, () => { count++; return GetUsers(); });
            _cache.FetchObject(key, () => { count++; return GetUsers(); });
            _cache.FetchObject(key, () => { count++; return GetUsers(); });
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void UT_CacheFetchHashed()
        {
            // Test the FetchHashed method
            string key = "UT_CacheFetchHashed";
            bool r = _cache.Remove(key);
            var users = GetUsers();
            foreach (var u in users)
            {
                User user = u;
                _cache.SetHashed(key, user.Id.ToString(), user);
            }
            var returnedUser1 = _cache.GetHashed<User>(key, "1");
            var returnedUser2 = _cache.GetHashed<User>(key, "2");

            Assert.AreEqual(1, returnedUser1.Id);
            Assert.AreEqual(2, returnedUser2.Id);
        }

        [TestMethod]
        public void UT_CacheGetHashAll()
        {
            // Test the GetHashAll method
            string key = "UT_CacheGetHashAll";
            bool r = _cache.Remove(key);
            var users = GetUsers();
            foreach (var u in users)
            {
                User user = u;
                _cache.SetHashed(key, user.Id.ToString(), user);
            }
            var dict = _cache.GetHashedAll<User>(key);
            Assert.AreEqual(users.Count, dict.Count);
            Assert.AreEqual(1, dict["1"].Id);
            Assert.AreEqual(2, dict["2"].Id);
        }

        [TestMethod]
        public void UT_CacheSetHashed_MultipleFieldsDistinctTypes()
        {
            string key = "UT_CacheSetHashed_MultipleFieldsDistinctTypes";
            _cache.Remove(key);
            var dict = new Dictionary<string, object>()
            {
                { "a", new User { Id = 222 }},
                { "2", new Department { Id = 3 }}
            };
            _cache.SetHashed(key, "a", dict["a"]);
            _cache.SetHashed(key, "2", dict["2"]);
            _cache.SetHashed(key, "D", new Location() { Id = 444 });

            var user = _cache.GetHashed<User>(key, "a");
            var dept = _cache.GetHashed<Department>(key, "2");
            var loc = _cache.GetHashed<Location>(key, "D");
            var all = _cache.GetHashedAll<object>(key);

            Assert.AreEqual(222, user.Id);
            Assert.AreEqual(3, dept.Id);
            Assert.AreEqual(444, loc.Id);

            Assert.AreEqual(3, all.Count);

            Assert.AreEqual(222, JObject.Parse(all["a"].ToString()).ToObject<User>().Id);
            Assert.AreEqual(3, JObject.Parse(all["2"].ToString()).ToObject<Department>().Id);
            Assert.AreEqual(444, JObject.Parse(all["D"].ToString()).ToObject<Location>().Id);
        }

        [TestMethod]
        public void UT_CacheRemove()
        {
            // Test the Remove method
            string key = "UT_CacheRemove";
            bool r = _cache.Remove(key);
            var users = GetUsers();
            _cache.SetObject(key, users[0]);

            r = _cache.Remove(key);
            Assert.IsTrue(r);
            Thread.Sleep(500);
            r = _cache.Remove(key);
            Assert.IsFalse(r);

            var returnedUser = _cache.GetObject<User>(key);

            Assert.IsNull(returnedUser);
        }

        [TestMethod]
        public void UT_CacheRemoveHashed()
        {
            // Test the Remove method for a complete hash set
            string key = "UT_CacheRemove_PreviouslyHashed";
            bool r = _cache.Remove(key);
            var users = GetUsers();
            foreach (var u in users)
            {
                User user = u;
                _cache.SetHashed(key, user.Id.ToString(), user);
            }
            r = _cache.RemoveHashed(key, "1");
            Assert.IsTrue(r);
            Thread.Sleep(200);
            r = _cache.RemoveHashed(key, "1");
            Assert.IsFalse(r);

            var returnedUser1 = _cache.GetHashed<User>(key, 1.ToString());
            var returnedUser2 = _cache.GetHashed<User>(key, 2.ToString());

            Assert.IsNull(returnedUser1);
            Assert.AreEqual(2, returnedUser2.Id);
        }

        [TestMethod]
        public void UT_CacheRemove_PreviouslyHashed()
        {
            // Test the Remove hashed method
            string key = "UT_CacheRemoveHashed";
            bool r = _cache.Remove(key);
            var users = GetUsers();
            foreach (var u in users)
            {
                User user = u;
                _cache.SetHashed(key, user.Id.ToString(), user);
            }
            r = _cache.Remove(key);
            Assert.IsTrue(r);
            Thread.Sleep(200);
            r = _cache.Remove(key);
            Assert.IsFalse(r);

            var returnedUser1 = _cache.GetHashed<User>(key, 1.ToString());
            var returnedUser2 = _cache.GetHashed<User>(key, 2.ToString());

            Assert.IsNull(returnedUser1);
            Assert.IsNull(returnedUser2);
        }

        [TestMethod]
        public void UT_CacheAdd_Expiration()
        {
            // Test the expiration of the Add method
            var users = GetUsers();
            string key = "SomeKey";
            _cache.Remove(key);
            _cache.SetObject(key, users[0], TimeSpan.FromMilliseconds(1000));
            var user = _cache.GetObject<User>(key);
            Assert.AreEqual(1, user.Id);
            Assert.AreEqual(2, user.Deparments[0].Size);
            Assert.AreEqual("one", user.Deparments[0].Location.Name);
            Thread.Sleep(1500);
            user = _cache.GetObject<User>(key);
            Assert.IsNull(user);
        }

        [TestMethod]
        public void UT_CacheAddHashed_Expiration_1()
        {
            // Test the expiration of the AddHashed method (MAX ttl applies)
            var users = GetUsers();
            string key = "SomeKey";
            _cache.Remove(key);

            _cache.SetHashed(key, "2", users[1], TimeSpan.FromMilliseconds(10000));
            _cache.SetHashed(key, "1", users[0], TimeSpan.FromMilliseconds(1000));
            Thread.Sleep(1200);

            var user1 = _cache.GetHashed<User>(key, "1");
            var user2 = _cache.GetHashed<User>(key, "2");
            Assert.IsNotNull(user1);
            Assert.IsNotNull(user2);
        }

        [TestMethod]
        public void UT_CacheAddHashed_Expiration_2()
        {
            // Test the expiration of the Fetch method (last larger expiration applies)
            var users = GetUsers();
            string key = "SomeKey";
            _cache.Remove(key);

            _cache.SetHashed(key, "1", users[0], TimeSpan.FromMilliseconds(1000));
            _cache.SetHashed(key, "2", users[1], TimeSpan.FromMilliseconds(10000));
            Thread.Sleep(1200);

            var user1 = _cache.GetHashed<User>(key, "1");
            var user2 = _cache.GetHashed<User>(key, "2");
            Assert.IsNotNull(user1);
            Assert.IsNotNull(user2);
        }

        [TestMethod]
        public void UT_CacheAddHashed_Expiration_3()
        {
            // Test the expiration of the Fetch method (last no-expiration applies)
            var users = GetUsers();
            string key = "SomeKey";
            _cache.Remove(key);
            var ms = 1000;
            _cache.SetHashed(key, "1", users[0], TimeSpan.FromMilliseconds(ms));
            _cache.SetHashed(key, "2", users[1]);
            Thread.Sleep(ms + 200);

            var user1 = _cache.GetHashed<User>(key, "1");
            var user2 = _cache.GetHashed<User>(key, "2");
            Assert.IsNotNull(user1);
            Assert.IsNotNull(user2);
        }

        [TestMethod]
        public void UT_CacheSetWithTags()
        {
            string key = "miset"; //UT_CacheSetWithTags
            _cache.Remove(key);
            var users = GetUsers();
            _cache.SetObject(key, users[0], new[] { "user:" + users[0].Id });
            var keys = _cache.GetKeysByTag(new[] { "user:" + users[0].Id });
            var value = _cache.GetObject<User>(keys.First());
            Assert.IsTrue(keys.Contains(key));
            Assert.IsNotNull(value);
        }

        [TestMethod]
        public void UT_CacheFetchWithTags()
        {
            
            string key = "UT_CacheFetchWithTags";
            string tag1 = "UT_CacheFetchWithTags-Tag1";
            string tag2 = "UT_CacheFetchWithTags-Tag2";
            _cache.FetchObject(key, () => "test value 1", new[] {tag1});
            _cache.FetchObject(key, () => "should not be updated", new[] { tag2 });
            var keys = _cache.GetKeysByTag(new [] {tag1});
            var value = _cache.GetObject<string>(keys.First());
            Assert.IsTrue(keys.Contains(key));
            Assert.IsNotNull(value);
            keys = _cache.GetKeysByTag(new [] {tag2});
            Assert.IsFalse(keys.Contains(key));
        }

        [TestMethod]
        public void UT_CacheSetWithTags_PersistentOverridesExpiration()
        {
            string key1 = "UT_CacheSetWithTags_Special1";
            string key2 = "UT_CacheSetWithTags_Special2";
            string tag = "UT_CacheSetWithTags_Special-Tag1";
            _cache.InvalidateKeysByTag(tag);
            _cache.SetObject(key1, "test value 1", new[] { tag }, TimeSpan.FromSeconds(1));
            _cache.SetObject(key2, "test value 2", new[] { tag });
            Thread.Sleep(2000);
            var keys = _cache.GetKeysByTag(new[] { tag });
            var keysCleaned = _cache.GetKeysByTag(new[] { tag }, true);
            Assert.AreEqual(2, keys.Count);
            Assert.AreEqual(1, keysCleaned.Count);
            Assert.IsTrue(keys.Contains(key1));
            Assert.IsTrue(keys.Contains(key2));
            Assert.IsTrue(keysCleaned.Contains(key2));
            _cache.InvalidateKeysByTag(tag);
        }


        [TestMethod]
        public void UT_CacheSetWithTags_Expiration()
        {
            string key = "UT_CacheSetWithTags_Expiration";
            string key2 = "UT_CacheSetWithTags_Expiration2";
            _cache.Remove(key);
            _cache.Remove(key2);
            var users = GetUsers();
            _cache.InvalidateKeysByTag("user:" + users[0].Id, "user:" + users[1].Id, "user-info");

            _cache.SetObject(key, users[0], new[] { "user:" + users[0].Id, "user-info" }, TimeSpan.FromSeconds(1));
            _cache.SetObject(key2, users[1], new[] { "user:" + users[1].Id, "user-info" }, TimeSpan.FromSeconds(5));
            var keys = _cache.GetKeysByTag(new[] { "user:" + users[0].Id });
            Assert.IsTrue(keys.Contains(key));
            var value = _cache.GetObject<User>(keys.First());
            Assert.IsNotNull(value);
            Thread.Sleep(1200);
            var keys2 = _cache.GetKeysByTag(new[] { "user:" + users[0].Id });
            Assert.IsFalse(keys2.Contains(key));
            value = _cache.GetObject<User>(key);
            Assert.IsNull(value);
            var keys3 = _cache.GetKeysByTag(new[] { "user-info" });
            Assert.IsTrue(keys3.Contains(key2));
            Thread.Sleep(4000);
            var keys4 = _cache.GetKeysByTag(new[] { "user-info" });
            Assert.IsFalse(keys4.Contains(key2));
        }

        [TestMethod]
        public void UT_CacheSetWithTags_Removal()
        {
            string key = "UT_CacheSetWithTags_Removal";
            _cache.Remove(key);
            var users = GetUsers();
            _cache.InvalidateKeysByTag("user:" + users[0].Id);

            _cache.SetObject(key, users[0], new[] { "user:" + users[0].Id });
            var keys = _cache.GetKeysByTag(new[] { "user:" + users[0].Id }, true);
            Assert.IsTrue(keys.Contains(key));
            _cache.Remove(key);
            var keys2 = _cache.GetKeysByTag(new[] { "user:" + users[0].Id }, true);
            Assert.IsFalse(keys2.Contains(key));
        }

        [TestMethod]
        public void UT_CacheSetWithTags_Multiple()
        {
            string key0 = "UT_CacheSetWithTags_Multiple0";
            string key1 = "UT_CacheSetWithTags_Multiple1";
            _cache.Remove(key0);
            _cache.Remove(key1);
            var users = GetUsers();
            _cache.InvalidateKeysByTag("user:" + users[0].Id, "user:" + users[1].Id, "user-info");

            _cache.SetObject(key0, users[0], new[] { "user:" + users[0].Id, "user-info" });
            _cache.SetObject(key1, users[1], new[] { "user:" + users[1].Id, "user-info" });
            var keys0 = _cache.GetKeysByTag(new[] { "user:" + users[0].Id });
            var keys1 = _cache.GetKeysByTag(new[] { "user:" + users[1].Id });
            var keys = _cache.GetKeysByTag(new[] { "user-info" });
            Assert.IsTrue(keys0.Contains(key0));
            Assert.IsTrue(keys1.Contains(key1));
            Assert.IsTrue(keys.Contains(key0) && keys.Contains(key1));
        }

        [TestMethod]
        public void UT_CacheRemoveByTags()
        {
            string key1 = "UT_CacheRemoveByTags1";
            string key2 = "UT_CacheRemoveByTags2";
            _cache.Remove(key1);
            _cache.Remove(key2);
            var users = GetUsers();
            string tag1 = "user:" + users[0].Id;
            string tag2 = "user:" + users[1].Id;
            _cache.InvalidateKeysByTag(tag1, tag2);
            _cache.SetObject(key1, users[0], new[] { tag1 });
            _cache.SetObject(key2, users[1], new[] { tag2 });
            _cache.InvalidateKeysByTag(tag1, tag2);
            var keys = _cache.GetKeysByTag(new [] {tag1, tag2});
            var user = _cache.GetObject<User>(key1);
            Assert.IsNull(user);
            Assert.AreEqual(0, keys.Count);
        }

        private List<User> GetUsers()
        {
            var loc1 = new Location()
            {
                Id = 1,
                Name = "one"
            };
            var loc2 = new Location()
            {
                Id = 2,
                Name = "two"
            };
            var user1 = new User()
            {
                Id = 1,
                Deparments = new List<Department>()
                {
                    new Department() {Id = 1, Distance = 123.45m, Size = 2, Location = loc1},
                    new Department() {Id = 2, Distance = 400, Size = 1, Location = loc2}
                }
            };
            var user2 = new User()
            {
                Id = 2,
                Deparments = new List<Department>()
                {
                    new Department() {Id = 3, Distance = 500, Size = 1, Location = loc2},
                    new Department() {Id = 4, Distance = 125.5m, Size = 3, Location = loc1}
                }
            };
            return new List<User>() { user1, user2 };
        }

    }


}
