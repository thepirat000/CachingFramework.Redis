using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CachingFramework.Redis.UnitTest
{
    [TestClass]
    public class UnitTestRedisObjects
    {
        private CacheContext _cache;
        [TestInitialize]
        public void Initialize()
        {
            // Config doc: https://github.com/StackExchange/StackExchange.Redis/blob/master/Docs/Configuration.md
            var config = "192.168.15.15:7001,192.168.15.15:7006,192.168.15.15:7002,192.168.15.15:7003,192.168.15.15:7004,192.168.15.15:7005,192.168.15.15:7000,connectRetry=10,syncTimeout=5000,abortConnect=false,keepAlive=10, allowAdmin=true";
            _cache = new CacheContext(config);
            _cache.FlushAll();
        }

        [TestMethod]
        public void UT_CacheListObject()
        {
            string key1 = "UT_CacheListObject1";
            _cache.Remove(key1);
            var users = GetUsers();
            var rl = _cache.GetCachedList<User>(key1);
            rl.AddRange(users);
            // Test GetEnumerator
            foreach (var item in rl)
            {
                Assert.IsTrue(users.Any(u => u.Id == item.Id));
            }
            // Test Count
            Assert.AreEqual(users.Count, rl.Count);
            // Test Contains
            Assert.IsTrue(rl.Contains(users[2]));
            // Test Insert
            rl.Insert(0, new User() { Id = 0 });
            Assert.AreEqual(0, rl[0].Id);
            // Test RemoveAt
            rl.RemoveAt(0);
            Assert.AreEqual(1, rl[0].Id);
            // Test Add
            rl.Add(new User() { Id = 5 });
            Assert.AreEqual(5, rl.Last().Id);
            // Test IndexOf
            Assert.AreEqual(2, rl.IndexOf(users[2]));
            // Test Remove
            rl.Remove(users[2]);
            Assert.IsFalse(rl.Contains(users[2]));
            // Test CopyTo
            User[] array = new User[50];
            rl.CopyTo(array, 10);
            Assert.AreEqual(1, array[10].Id);
            // Test Clear
            rl.Clear();
            Assert.AreEqual(0, rl.Count);
        }

        [TestMethod]
        public void UT_CacheListObjectTTL()
        {
            string key1 = "UT_CacheListObject_TTL1";
            _cache.Remove(key1);
            var users = GetUsers();
            var rl = _cache.GetCachedList<User>(key1);
            rl.AddRange(users);
            rl.TimeToLive = TimeSpan.FromMilliseconds(1500);
            Assert.AreEqual(users.Count, rl.Count);
            Thread.Sleep(2000);
            Assert.AreEqual(0, rl.Count);
        }

        [TestMethod]
        public void UT_CacheListObject_GetRange()
        {
            string key = "UT_CacheListObject_GetRange";
            int total = 100;
            _cache.Remove(key);
            var rl = _cache.GetCachedList<User>(key);
            rl.AddRange(Enumerable.Range(1, total).Select(i => new User() { Id = i }));

            var range = rl.GetRange();
            Assert.AreEqual(total, rl.Count);

            range = rl.GetRange(3, 10);
            Assert.AreEqual(8, range.Count);
            Assert.AreEqual(4, range[0].Id);

            range = rl.GetRange(10, -10);
            Assert.AreEqual(11, range[0].Id);
            Assert.AreEqual(91, range[range.Count - 1].Id);
        }

        [TestMethod]
        public void UT_CacheDictionaryObject()
        {
            string key1 = "UT_CacheDictionaryObject1";
            _cache.Remove(key1);
            var users = GetUsers();
            var rd = _cache.GetCachedDictionary<int, User>(key1);
            // Test AddMultiple
            var usersKv = users.Select(x => new KeyValuePair<int, User>(x.Id, x));
            rd.AddMultiple(usersKv);
            // Test GetEnumerator
            foreach (var item in rd)
            {
                Assert.IsTrue(users.Any(u => u.Id == item.Key));
            }
            // Test Count
            Assert.AreEqual(users.Count, rd.Count);
            // Test ContainsKey
            Assert.IsTrue(rd.ContainsKey(users[1].Id));
            // Test Contains
            Assert.IsTrue(rd.Contains(new KeyValuePair<int, User>(users.Last().Id, users.Last())));
            // Test Add
            rd.Add(0, new User() { Id = 0 });
            Assert.AreEqual(users.Count + 1, rd.Count);
            Assert.AreEqual(0, rd[0].Id);
            // Test Remove
            rd.Remove(0);
            Assert.IsFalse(rd.ContainsKey(0));
            // Test Keys
            foreach (var k in rd.Keys)
            {
                Assert.IsTrue(users.Any(u => u.Id == k));
            }
            // Test Values
            foreach (var u in rd.Values)
            {
                Assert.IsTrue(users.Any(user => user.Id == u.Id));
            }
            // Test TryGetValue
            User userTest = new User();
            bool b = rd.TryGetValue(999, out userTest);
            Assert.IsFalse(b);
            Assert.IsNull(userTest);
            b = rd.TryGetValue(1, out userTest);
            Assert.IsTrue(b);
            Assert.AreEqual(1, userTest.Id);
            // Test CopyTo
            var array = new KeyValuePair<int, User>[50];
            rd.CopyTo(array, 10);
            Assert.AreEqual(users.Count, array.Count(x => x.Value != null));
            // Test Clear
            rd.Clear();
            Assert.AreEqual(0, rd.Count);
        }

        [TestMethod]
        public void UT_CacheDictionaryObject_TTL()
        {
            string key1 = "UT_CacheDictionaryObjectTTL1";
            _cache.Remove(key1);
            var users = GetUsers();
            var rl = _cache.GetCachedDictionary<int, User>(key1);
            rl.AddMultiple(users.ToDictionary(k => k.Id));
            rl.TimeToLive = TimeSpan.FromMilliseconds(1500);
            Assert.AreEqual(users.Count, rl.Count);
            Thread.Sleep(2000);
            Assert.AreEqual(0, rl.Count);
        }

        [TestMethod]
        public void UT_CacheSetObject()
        {
            string key1 = "UT_CacheSetObject1";
            _cache.Remove(key1);
            var users = GetUsers();
            var rs = _cache.GetCachedSet<User>(key1);
            rs.AddMultiple(users);
            // Test GetEnumerator
            foreach (var item in rs)
            {
                Assert.IsTrue(users.Any(u => u.Id == item.Id));
            }
            // Test Count
            Assert.AreEqual(users.Count, rs.Count);
            // Test Contains
            Assert.IsTrue(rs.Contains(users[2]));
            // Test Add
            var newUser = new User() {Id = 5};
            rs.Add(newUser);
            Assert.IsTrue(rs.Contains(newUser));
            // Test Remove
            rs.Remove(users[2]);
            Assert.IsFalse(rs.Contains(users[2]));
            // Test CopyTo
            User[] array = new User[50];
            rs.CopyTo(array, 10);
            Assert.AreEqual(users.Count, array.Count(x => x != null));
            // Test Clear
            rs.Clear();
            Assert.AreEqual(0, rs.Count);
        }

        [TestMethod]
        public void UT_CacheSetObject_TTL()
        {
            string key1 = "UT_CacheSetObject_TTL";
            _cache.Remove(key1);
            var users = GetUsers();
            var rl = _cache.GetCachedSet<User>(key1);
            rl.AddMultiple(users);
            rl.TimeToLive = TimeSpan.FromMilliseconds(1500);
            Assert.AreEqual(users.Count, rl.Count);
            Thread.Sleep(2000);
            Assert.AreEqual(0, rl.Count);
        }

        [TestMethod]
        public void UT_CacheSetObject_SetModifiers()
        {
            string keyAbc = "UT_CacheSetObject_SetModifiers_ABC";
            string keyCde = "UT_CacheSetObject_SetModifiers_CDE";

            _cache.Remove(keyAbc);
            _cache.Remove(keyCde);
            var abcSet = _cache.GetCachedSet<char>(keyAbc);
            abcSet.AddMultiple("ABC");
            
            var cdeSet = _cache.GetCachedSet<char>(keyCde);
            cdeSet.AddMultiple("CDE");

            // Test Count
            Assert.AreEqual(3, abcSet.Count);
            Assert.AreEqual(3, cdeSet.Count);

            // Test ExceptWith
            abcSet.ExceptWith("AXYZ".ToCharArray());
            Assert.AreEqual(2, abcSet.Count);
            Assert.IsFalse(abcSet.Contains('A'));
            Assert.IsFalse(abcSet.Contains('X'));
            abcSet.Add('A');

            // Test IntersectWith
            abcSet.IntersectWith(cdeSet);
            Assert.AreEqual(1, abcSet.Count);
            Assert.IsTrue(abcSet.Contains('C'));
            abcSet.AddMultiple("AB");

            // Test IsProperSubsetOf
            Assert.IsFalse(abcSet.IsProperSubsetOf(cdeSet));
            Assert.IsFalse(abcSet.IsProperSubsetOf(abcSet));
            Assert.IsTrue(abcSet.IsProperSubsetOf("ABCD"));

            // Test IsProperSupersetOf
            Assert.IsFalse(abcSet.IsProperSupersetOf(cdeSet));
            Assert.IsFalse(abcSet.IsProperSupersetOf(abcSet));
            Assert.IsTrue(abcSet.IsProperSupersetOf("AB"));
            Assert.IsTrue(abcSet.IsProperSupersetOf("C"));

            // Test IsSubsetOf
            Assert.IsFalse(abcSet.IsSubsetOf(cdeSet));
            Assert.IsTrue(abcSet.IsSubsetOf(abcSet));
            Assert.IsTrue(abcSet.IsSubsetOf("ABCD"));

            // Test IsSupersetOf
            Assert.IsFalse(abcSet.IsSupersetOf(cdeSet));
            Assert.IsTrue(abcSet.IsSupersetOf(abcSet));
            Assert.IsTrue(abcSet.IsSupersetOf("AB"));
            Assert.IsTrue(abcSet.IsSupersetOf("C"));

            // Test Overlaps
            Assert.IsTrue(abcSet.Overlaps(cdeSet));
            Assert.IsTrue(abcSet.Overlaps(abcSet));
            Assert.IsFalse(abcSet.Overlaps("XYZ"));

            // Test SetEquals
            Assert.IsFalse(abcSet.SetEquals(cdeSet));
            Assert.IsTrue(abcSet.SetEquals(abcSet));
            Assert.IsFalse(abcSet.SetEquals("ABCD"));

            // Test SymmetricExceptWith 
            abcSet.SymmetricExceptWith(cdeSet);
            Assert.IsTrue(abcSet.OrderBy(x => x).ToArray().SequenceEqual("ABDE"));
            abcSet.RemoveWhere(x => "DE".Contains(x));
            abcSet.Add('C');
            Assert.AreEqual(3, abcSet.Count);
            Assert.IsTrue(abcSet.OrderBy(x => x).ToArray().SequenceEqual("ABC"));

            // Test UnionWith
            abcSet.UnionWith(cdeSet);
            Assert.IsTrue(abcSet.OrderBy(x => x).ToArray().SequenceEqual("ABCDE"));

            abcSet.Clear();
            cdeSet.Clear();
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
            var user3 = new User()
            {
                Id = 3,
                Deparments = new List<Department>()
                {
                    new Department() {Id = 5, Distance = 5, Size = 5, Location = loc2},
                }
            };
            var user4 = new User()
            {
                Id = 4,
                Deparments = new List<Department>()
                {
                    new Department() {Id = 6, Distance = 100, Size = 10, Location = loc1},
                }
            };
            return new List<User>() { user1, user2, user3, user4 };
        }

    }
}
