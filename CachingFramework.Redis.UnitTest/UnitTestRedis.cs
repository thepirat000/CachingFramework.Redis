using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CachingFramework.Redis.Serializers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CachingFramework.Redis.UnitTest
{
    [TestClass]
    public class UnitTestRedis
    {
        private static Context _context;
        private string _defaultConfig;
        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            _context = Common.GetContextAndFlush();
        }

        [TestMethod]
        public void UT_CacheSerializer()
        {
            var kss = "short:string";
            var kls = "long:string";
            var kpBool = "primitive:bool";
            var kpInt = "primitive:int";
            var kpLong = "primitive:long";
            var kpSingle = "primitive:single";
            var kpIntPtr = "primitive:intptr";
            var kpUInt16 = "primitive:uint16";
            var kpUInt32 = "primitive:uint32";
            var kpUInt64 = "primitive:uint64";
            _context.Cache.Remove(new [] { kss, kls, kpBool, kpInt, kpLong, kpSingle, kpIntPtr, kpUInt16, kpUInt32, kpUInt64 });
            var ss = "this is a short string";
            var ls = @"UTF-8 is a character encoding capable of encoding all possible characters, or code points, in Unicode.
The encoding is variable-length and uses 8-bit code units. It was designed for backward compatibility with ASCII, and to avoid the complications of endianness and byte order marks in the alternative UTF-16 and UTF-32 encodings. The name is derived from: Universal Coded Character Set + Transformation Format—8-bit.";
            _context.Cache.SetObject(kss, ss);
            _context.Cache.SetObject(kls, ls);

            _context.Cache.SetObject<bool>(kpBool, true);
            _context.Cache.SetObject<int>(kpInt, int.MaxValue);
            _context.Cache.SetObject<long>(kpLong, long.MaxValue);
            _context.Cache.SetObject<Single>(kpSingle, Single.MaxValue);
            _context.Cache.SetObject<IntPtr>(kpIntPtr, new IntPtr(int.MaxValue));
            _context.Cache.SetObject<UInt16>(kpUInt16, UInt16.MaxValue);
            _context.Cache.SetObject<UInt32>(kpUInt32, UInt32.MaxValue);
            _context.Cache.SetObject<UInt64>(kpUInt64, UInt64.MaxValue);

            var ss_ = _context.Cache.GetObject<string>(kss);
            var ls_ = _context.Cache.GetObject<string>(kls);
            var pInt_ = _context.Cache.GetObject<int>(kpInt);
            var pLong_ = _context.Cache.GetObject<long>(kpLong);
            var pSingle_ = _context.Cache.GetObject<Single>(kpSingle);
            var pIntPtr_ = _context.Cache.GetObject<IntPtr>(kpIntPtr);
            var pUint16_ = _context.Cache.GetObject<UInt16>(kpUInt16);
            var pUint32_ = _context.Cache.GetObject<UInt32>(kpUInt32);
            var pUint64_ = _context.Cache.GetObject<UInt64>(kpUInt64);


            Assert.AreEqual(ss, ss_);
            Assert.AreEqual(int.MaxValue, pInt_);
            Assert.AreEqual(long.MaxValue, pLong_);
            Assert.AreEqual(Single.MaxValue, pSingle_);
            Assert.AreEqual(new IntPtr(int.MaxValue), pIntPtr_);
            Assert.AreEqual(UInt16.MaxValue, pUint16_);
            Assert.AreEqual(UInt32.MaxValue, pUint32_);
            Assert.AreEqual(UInt64.MaxValue, pUint64_);
            _context.Cache.Remove(new[] { kss, kls, kpBool, kpInt, kpLong, kpSingle, kpIntPtr, kpUInt16, kpUInt32, kpUInt64 });
        }


        [TestMethod]
        public void UT_CacheByteArray()
        {
            _context.Cache.SetObject("key", "jpeg");
            var o = _context.Cache.GetObject<string>("key");


            string key = "UT_CacheByteArray";
            Jpeg jpeg = new Jpeg()
            {
                Data = Enumerable.Range(0, 200000)
                       .Select(i => (byte)((i * 223) % 256)).ToArray()
            };
            _context.Cache.Remove(key);
            _context.Cache.SetObject(key, jpeg);
            var jpeg2 = _context.Cache.GetObject<Jpeg>(key);
            Assert.IsTrue(Enumerable.SequenceEqual(jpeg.Data, jpeg2.Data));
        }

        [TestMethod]
        public void UT_CacheAddGet()
        {
            // Test the Add and Get methods
            var users = GetUsers();
            string key = "UT_CacheAddGet";
            _context.Cache.Remove(key);
            _context.Cache.SetObject(key, users[1]);
            _context.Cache.SetObject(key, users[0], new string[]{});
            var user = _context.Cache.GetObject<User>(key);
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
            _context.Cache.Remove(key);
            var a = _context.Cache.FetchObject(key, () => { count++; return GetUsers(); });
            var b = _context.Cache.FetchObject(key, () => { count++; return GetUsers(); });
            _context.Cache.FetchObject(key, () => { count++; return GetUsers(); });
            Assert.AreEqual(1, count);
            Assert.AreEqual(a[0].Id, b[0].Id);
        }

        [TestMethod]
        public void UT_CacheFetch_TTL()
        {
            // Test the Fetch method
            string key = "UT_CacheFetch_TTL";
            int count = 0;
            _context.Cache.Remove(key);
            _context.Cache.FetchObject(key, () => { count++; return GetUsers(); }, TimeSpan.FromSeconds(2));
            _context.Cache.FetchObject(key, () => { count++; return GetUsers(); });
            Assert.AreEqual(1, count);
            Thread.Sleep(2200);
            _context.Cache.FetchObject(key, () => { count++; return GetUsers(); });
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public void UT_CacheFetchHashed()
        {
            // Test the FetchHashed method
            string key = "UT_CacheFetchHashed";
            bool r = _context.Cache.Remove(key);
            var users = GetUsers();
            var returnedUser1 = _context.Cache.FetchHashed<User>(key, users[0].Id.ToString(), () => users[0]);
            var returnedUser2 = _context.Cache.FetchHashed<User>(key, users[0].Id.ToString(), () => null);
            Assert.AreEqual(users[0].Id, returnedUser1.Id);
            Assert.AreEqual(users[0].Id, returnedUser2.Id);
        }

        [TestMethod]
        public void UT_CacheTryGetObject()
        {
            // Test the TryGetObject method
            string key = "UT_CacheTryGetObject";
            _context.Cache.Remove(key);
            var users = GetUsers();
            User u1;
            _context.Cache.SetObject(key, users[0]);
            bool b = _context.Cache.TryGetObject(key + "x7rz9a", out u1);
            Assert.IsFalse(b);
            Assert.IsNull(u1);
            b = _context.Cache.TryGetObject(key, out u1);
            Assert.IsTrue(b);
            Assert.IsNotNull(u1);
        }

        [TestMethod]
        public void UT_CacheTryGetHashed()
        {
            // Test the TryGetHashed method
            string key = "UT_CacheTryGetHashed";
            _context.Cache.Remove(key);
            var users = GetUsers();
            User u1;
            _context.Cache.SetHashed(key, users[0].Id.ToString(), users[0]);
            bool b = _context.Cache.TryGetHashed(key, "a", out u1);
            Assert.IsFalse(b);
            Assert.IsNull(u1);
            b = _context.Cache.TryGetHashed(key, users[0].Id.ToString(), out u1);
            Assert.IsTrue(b);
            Assert.IsNotNull(u1);
        }

        [TestMethod]
        public void UT_CacheGetSetObject()
        {
            // Test the GetSetObject method
            string key = "UT_CacheGetSetObject";
            _context.Cache.Remove(key);
            var str = _context.Cache.GetSetObject<string>(key, "1");
            Assert.IsNull(str);
            str = _context.Cache.GetSetObject<string>(key, "2");
            Assert.AreEqual("1", str);
            str = _context.Cache.GetObject<string>(key);
            Assert.AreEqual("2", str);
            _context.Cache.Remove(key);
            var integer = _context.Cache.GetSetObject<int>(key, 1);
            Assert.AreEqual(0, integer);
            integer = _context.Cache.GetSetObject<int>(key, 2);
            Assert.AreEqual(1, integer);
            integer = _context.Cache.GetObject<int>(key);
            Assert.AreEqual(2, integer);
        }

        [TestMethod]
        public void UT_CacheGetHashAll()
        {
            // Test the GetHashAll method
            string key = "UT_CacheGetHashAll";
            bool r = _context.Cache.Remove(key);
            var users = GetUsers();
            foreach (var u in users)
            {
                User user = u;
                _context.Cache.SetHashed(key, user.Id.ToString(), user);
            }
            var dict = _context.Cache.GetHashedAll<User>(key);
            Assert.AreEqual(users.Count, dict.Count);
            Assert.AreEqual(1, dict["1"].Id);
            Assert.AreEqual(2, dict["2"].Id);
        }

        [TestMethod]
        public void UT_CacheSetHashed_MultipleFieldsDistinctTypes()
        {
            string key = "UT_CacheSetHashed_MultipleFieldsDistinctTypes";
            _context.Cache.Remove(key);
            var dict = new Dictionary<string, object>()
            {
                { "a", new User { Id = 222 }},
                { "2", new Department { Id = 3 }}
            };
            _context.Cache.SetHashed(key, "a", dict["a"]);
            _context.Cache.SetHashed(key, "2", dict["2"]);
            _context.Cache.SetHashed(key, "D", new Location() { Id = 444 });

            var user = _context.Cache.GetHashed<User>(key, "a");
            var dept = _context.Cache.GetHashed<Department>(key, "2");
            var loc = _context.Cache.GetHashed<Location>(key, "D");
            var all = _context.Cache.GetHashedAll<object>(key);

            Assert.AreEqual(222, user.Id);
            Assert.AreEqual(3, dept.Id);
            Assert.AreEqual(444, loc.Id);

            Assert.AreEqual(3, all.Count);

            Assert.AreEqual(222, ((User)all["a"]).Id);
            Assert.AreEqual(3, ((Department)all["2"]).Id);
            Assert.AreEqual(444, ((Location)all["D"]).Id);
        }

        [TestMethod]
        public void UT_CacheRemove()
        {
            // Test the Remove method
            string key = "UT_CacheRemove";
            bool r = _context.Cache.Remove(key);
            var users = GetUsers();
            _context.Cache.SetObject(key, users[0]);

            r = _context.Cache.Remove(key);
            Assert.IsTrue(r);
            Thread.Sleep(500);
            r = _context.Cache.Remove(key);
            Assert.IsFalse(r);
            var returnedUser = _context.Cache.GetObject<User>(key);
            Assert.IsNull(returnedUser);
        }

        [TestMethod]
        public void UT_CacheRemoveMultiple()
        {
            string key = "UT_CacheRemoveMultiple";
            for (int i = 0; i < 255; i++)
            {
                _context.Cache.SetObject(key + i, new User() { Id = i });
            }
            for (int i = 0; i < 255; i++)
            {
                Assert.IsNotNull(_context.Cache.GetObject<User>(key + i));
            }
            _context.Cache.Remove(Enumerable.Range(0, 255).Select(i => key + i).ToArray());
            for (int i = 0; i < 255; i++)
            {
                Assert.IsNull(_context.Cache.GetObject<User>(key + i));
            }
        }

        [TestMethod]
        public void UT_CacheRemoveHashed()
        {
            // Test the Remove method for a complete hash set
            string key = "UT_CacheRemove_PreviouslyHashed";
            bool r = _context.Cache.Remove(key);
            var users = GetUsers();
            foreach (var u in users)
            {
                User user = u;
                _context.Cache.SetHashed(key, user.Id.ToString(), user);
            }
            r = _context.Cache.RemoveHashed(key, "1");
            Assert.IsTrue(r);
            Thread.Sleep(200);
            r = _context.Cache.RemoveHashed(key, "1");
            Assert.IsFalse(r);

            var returnedUser1 = _context.Cache.GetHashed<User>(key, 1.ToString());
            var returnedUser2 = _context.Cache.GetHashed<User>(key, 2.ToString());

            Assert.IsNull(returnedUser1);
            Assert.AreEqual(2, returnedUser2.Id);
        }

        [TestMethod]
        public void UT_CacheRemove_PreviouslyHashed()
        {
            // Test the Remove hashed method
            string key = "UT_CacheRemoveHashed";
            bool r = _context.Cache.Remove(key);
            var users = GetUsers();
            foreach (var u in users)
            {
                User user = u;
                _context.Cache.SetHashed(key, user.Id.ToString(), user);
            }
            r = _context.Cache.Remove(key);
            Assert.IsTrue(r);
            Thread.Sleep(200);
            r = _context.Cache.Remove(key);
            Assert.IsFalse(r);

            var returnedUser1 = _context.Cache.GetHashed<User>(key, 1.ToString());
            var returnedUser2 = _context.Cache.GetHashed<User>(key, 2.ToString());

            Assert.IsNull(returnedUser1);
            Assert.IsNull(returnedUser2);
        }

        [TestMethod]
        public void UT_CacheAdd_Expiration()
        {
            // Test the expiration of the Add method
            var users = GetUsers();
            string key = "SomeKey";
            _context.Cache.Remove(key);
            _context.Cache.SetObject(key, users[0], TimeSpan.FromMilliseconds(1000));
            var user = _context.Cache.GetObject<User>(key);
            Assert.AreEqual(1, user.Id);
            Assert.AreEqual(2, user.Deparments[0].Size);
            Assert.AreEqual("one", user.Deparments[0].Location.Name);
            Thread.Sleep(1500);
            user = _context.Cache.GetObject<User>(key);
            Assert.IsNull(user);
        }

        [TestMethod]
        public void UT_CacheAddHashed_Expiration_1()
        {
            // Test the expiration of the AddHashed method (MAX ttl applies)
            var users = GetUsers();
            string key = "SomeKey";
            _context.Cache.Remove(key);

            _context.Cache.SetHashed(key, "2", users[1], TimeSpan.FromMilliseconds(10000));
            _context.Cache.SetHashed(key, "1", users[0], TimeSpan.FromMilliseconds(1000));
            Thread.Sleep(1200);

            var user1 = _context.Cache.GetHashed<User>(key, "1");
            var user2 = _context.Cache.GetHashed<User>(key, "2");
            Assert.IsNotNull(user1);
            Assert.IsNotNull(user2);
        }

        [TestMethod]
        public void UT_CacheAddHashed_Expiration_2()
        {
            // Test the expiration of the Fetch method (last larger expiration applies)
            var users = GetUsers();
            string key = "SomeKey";
            _context.Cache.Remove(key);

            _context.Cache.SetHashed(key, "1", users[0], TimeSpan.FromMilliseconds(1000));
            _context.Cache.SetHashed(key, "2", users[1], TimeSpan.FromMilliseconds(10000));
            Thread.Sleep(1200);

            var user1 = _context.Cache.GetHashed<User>(key, "1");
            var user2 = _context.Cache.GetHashed<User>(key, "2");
            Assert.IsNotNull(user1);
            Assert.IsNotNull(user2);
        }

        [TestMethod]
        public void UT_CacheAddHashed_Expiration_3()
        {
            // Test the expiration of the Fetch method (last no-expiration applies)
            var users = GetUsers();
            string key = "SomeKey";
            _context.Cache.Remove(key);
            var ms = 1000;
            _context.Cache.SetHashed(key, "1", users[0], TimeSpan.FromMilliseconds(ms));
            _context.Cache.SetHashed(key, "2", users[1]);
            Thread.Sleep(ms + 200);

            var user1 = _context.Cache.GetHashed<User>(key, "1");
            var user2 = _context.Cache.GetHashed<User>(key, "2");
            Assert.IsNotNull(user1);
            Assert.IsNotNull(user2);
        }

        [TestMethod]
        public void UT_CacheSetWithTags()
        {
            string key = "miset"; //UT_CacheSetWithTags
            _context.Cache.Remove(key);
            var users = GetUsers();
            _context.Cache.SetObject(key, users[0], new[] { "user:" + users[0].Id });
            var keys = _context.Cache.GetKeysByTag(new[] { "user:" + users[0].Id });
            var value = _context.Cache.GetObject<User>(keys.First());
            Assert.IsTrue(keys.Contains(key));
            Assert.IsNotNull(value);
        }

        [TestMethod]
        public void UT_CacheFetchWithTags()
        {
            string key = "UT_CacheFetchWithTags";
            _context.Cache.Remove(key);
            string tag1 = "UT_CacheFetchWithTags-Tag1";
            string tag2 = "UT_CacheFetchWithTags-Tag2";
            _context.Cache.FetchObject(key, () => "test value 1", new[] {tag1});
            _context.Cache.FetchObject(key, () => "should not be updated", new[] { tag2 });
            var keys = _context.Cache.GetKeysByTag(new [] {tag1});
            var value = _context.Cache.GetObject<string>(keys.First());
            Assert.IsTrue(keys.Contains(key));
            Assert.IsNotNull(value);
            keys = _context.Cache.GetKeysByTag(new [] {tag2});
            Assert.IsFalse(keys.Contains(key));
        }

        [TestMethod]
        public void UT_CacheSetWithTags_PersistentOverridesExpiration()
        {
            string key1 = "UT_CacheSetWithTags_Special1";
            string key2 = "UT_CacheSetWithTags_Special2";
            string tag = "UT_CacheSetWithTags_Special-Tag1";
            _context.Cache.InvalidateKeysByTag(tag);
            _context.Cache.SetObject(key1, "test value 1", new[] { tag }, TimeSpan.FromSeconds(1));
            _context.Cache.SetObject(key2, "test value 2", new[] { tag });
            Thread.Sleep(2000);
            var keys = _context.Cache.GetKeysByTag(new[] { tag });
            var keysCleaned = _context.Cache.GetKeysByTag(new[] { tag }, true);
            Assert.AreEqual(2, keys.Count);
            Assert.AreEqual(1, keysCleaned.Count);
            Assert.IsTrue(keys.Contains(key1));
            Assert.IsTrue(keys.Contains(key2));
            Assert.IsTrue(keysCleaned.Contains(key2));
            _context.Cache.InvalidateKeysByTag(tag);
        }


        [TestMethod]
        public void UT_CacheSetWithTags_Expiration()
        {
            string key = "UT_CacheSetWithTags_Expiration";
            string key2 = "UT_CacheSetWithTags_Expiration2";
            _context.Cache.Remove(key);
            _context.Cache.Remove(key2);
            var users = GetUsers();
            _context.Cache.InvalidateKeysByTag("user:" + users[0].Id, "user:" + users[1].Id, "user-info");

            _context.Cache.SetObject(key, users[0], new[] { "user:" + users[0].Id, "user-info" }, TimeSpan.FromSeconds(1));
            _context.Cache.SetObject(key2, users[1], new[] { "user:" + users[1].Id, "user-info" }, TimeSpan.FromSeconds(5));
            var keys = _context.Cache.GetKeysByTag(new[] { "user:" + users[0].Id });
            Assert.IsTrue(keys.Contains(key));
            var value = _context.Cache.GetObject<User>(keys.First());
            Assert.IsNotNull(value);
            Thread.Sleep(1200);
            var keys2 = _context.Cache.GetKeysByTag(new[] { "user:" + users[0].Id });
            Assert.IsFalse(keys2.Contains(key));
            value = _context.Cache.GetObject<User>(key);
            Assert.IsNull(value);
            var keys3 = _context.Cache.GetKeysByTag(new[] { "user-info" });
            Assert.IsTrue(keys3.Contains(key2));
            Thread.Sleep(4000);
            var keys4 = _context.Cache.GetKeysByTag(new[] { "user-info" });
            Assert.IsFalse(keys4.Contains(key2));
        }

        [TestMethod]
        public void UT_CacheSetWithTags_Removal()
        {
            string key = "UT_CacheSetWithTags_Removal";
            _context.Cache.Remove(key);
            var users = GetUsers();
            _context.Cache.InvalidateKeysByTag("user:" + users[0].Id);

            _context.Cache.SetObject(key, users[0], new[] { "user:" + users[0].Id });
            var keys = _context.Cache.GetKeysByTag(new[] { "user:" + users[0].Id }, true);
            Assert.IsTrue(keys.Contains(key));
            _context.Cache.Remove(key);
            var keys2 = _context.Cache.GetKeysByTag(new[] { "user:" + users[0].Id }, true);
            Assert.IsFalse(keys2.Contains(key));
        }

        [TestMethod]
        public void UT_CacheSetWithTags_Multiple()
        {
            string key0 = "UT_CacheSetWithTags_Multiple0";
            string key1 = "UT_CacheSetWithTags_Multiple1";
            _context.Cache.Remove(key0);
            _context.Cache.Remove(key1);
            var users = GetUsers();
            _context.Cache.InvalidateKeysByTag("user:" + users[0].Id, "user:" + users[1].Id, "user-info");

            _context.Cache.SetObject(key0, users[0], new[] { "user:" + users[0].Id, "user-info" });
            _context.Cache.SetObject(key1, users[1], new[] { "user:" + users[1].Id, "user-info" });
            var keys0 = _context.Cache.GetKeysByTag(new[] { "user:" + users[0].Id });
            var keys1 = _context.Cache.GetKeysByTag(new[] { "user:" + users[1].Id });
            var keys = _context.Cache.GetKeysByTag(new[] { "user-info" });
            Assert.IsTrue(keys0.Contains(key0));
            Assert.IsTrue(keys1.Contains(key1));
            Assert.IsTrue(keys.Contains(key0) && keys.Contains(key1));
        }

        [TestMethod]
        public void UT_CacheRemoveByTags()
        {
            string key1 = "UT_CacheRemoveByTags1";
            string key2 = "UT_CacheRemoveByTags2";
            _context.Cache.Remove(key1);
            _context.Cache.Remove(key2);
            var users = GetUsers();
            string tag1 = "user:" + users[0].Id;
            string tag2 = "user:" + users[1].Id;
            _context.Cache.InvalidateKeysByTag(tag1, tag2);
            _context.Cache.SetObject(key1, users[0], new[] { tag1 });
            _context.Cache.SetObject(key2, users[1], new[] { tag2 });
            _context.Cache.InvalidateKeysByTag(tag1, tag2);
            var keys = _context.Cache.GetKeysByTag(new [] {tag1, tag2});
            var user = _context.Cache.GetObject<User>(key1);
            Assert.IsNull(user);
            Assert.AreEqual(0, keys.Count);
        }

        [TestMethod]
        public void UT_CacheGetObjectsByTag()
        {
            string key = "UT_CacheGetObjectsByTag{0}";
            string tag1 = "UT_CacheGetObjectsByTag_Tag1";
            string tag2 = "UT_CacheGetObjectsByTag_Tag2";
            string tag3 = "UT_CacheGetObjectsByTag_Tag3";
            var users = new List<User>();
            for (int i = 0; i < 100; i++)
            {
                users.Add(new User()
                {
                    Id = i, Deparments = new List<Department>() { new Department() {Id = i, Distance = i, Location = new Location() {Id = i}, Size = i} }
                });
                _context.Cache.SetObject(string.Format(key, i), users[users.Count - 1], new[] { i % 3 == 0 ? tag1 : i % 3 == 1 ? tag2 : tag3 });
            }
            var t1Users = _context.Cache.GetObjectsByTag<User>(tag1).ToList();
            var t2Users = _context.Cache.GetObjectsByTag<User>(tag2).ToList();
            var t3Users = _context.Cache.GetObjectsByTag<User>(tag3).ToList();
            Assert.IsTrue(t1Users.TrueForAll(u => u.Id % 3 == 0));
            Assert.IsTrue(t2Users.TrueForAll(u => u.Id % 3 == 1));
            Assert.IsTrue(t3Users.TrueForAll(u => u.Id % 3 == 2));
            _context.Cache.InvalidateKeysByTag(tag1, tag2, tag3);
            t1Users = _context.Cache.GetObjectsByTag<User>(tag1).ToList();
            Assert.AreEqual(0, t1Users.Count);
            Assert.IsNull(_context.Cache.GetObject<User>(string.Format(key, 1)));
        }

        [TestMethod]
        public void UT_CacheAddRemoveTagToKey()
        {
            string key = "UT_CacheAddRemoveTagToKey";
            string tag = "UT_CacheAddRemoveTagToKey_Tag";
            _context.Cache.Remove(key);
            _context.Cache.SetObject(key, "value");
            _context.Cache.AddTagsToKey(key, new[] { tag });
            var keys = _context.Cache.GetKeysByTag(new [] { tag }).ToList();
            Assert.IsTrue(keys.Contains(key));
            _context.Cache.RemoveTagsFromKey(key, new[] { tag });
            keys = _context.Cache.GetKeysByTag(new [] { tag }).ToList();
            Assert.IsFalse(keys.Contains(key));

        }

        [TestMethod]
        public void UT_CacheSetHashedAll()
        {
            string key = "UT_CacheSetHashedAll";
            _context.Cache.Remove(key);
            var users = GetUsers();
            IDictionary<string, User> allUsers = users.ToDictionary(k => k.Id.ToString());
            _context.Cache.SetHashed(key, allUsers);
            var response = _context.Cache.GetHashedAll<User>(key);
            Assert.AreEqual(users.Count, response.Count);
            Assert.IsTrue(users.All(x => response.ContainsKey(x.Id.ToString())));
        }

        [TestMethod]
        public void UT_CacheSerialization()
        {
            string key = "UT_CacheSerialization";
            _context.Cache.Remove(key);
            Exception exItem = null;
            try
            {
                throw new ApplicationException("this is a test exception to test serialization", new ArgumentException("this is an inner exception", "param"));
            }
            catch (Exception ex)
            {
                ex.Data.Add("some data", "to test");
                exItem = ex;
            }
            _context.Cache.SetObject(key, exItem);
            var exFinal = _context.Cache.GetObject<Exception>(key);
            Assert.AreEqual(exItem.Data.Count, exFinal.Data.Count);
            Assert.AreEqual(exItem.InnerException.Message, exFinal.InnerException.Message);
            Assert.AreEqual(exItem.StackTrace, exFinal.StackTrace);
        }

        [TestMethod]
        public void UT_Cache_HllAddCount()
        {
            string key = "UT_Cache_HllAddCount";
            _context.Cache.Remove(key);
            _context.Cache.HyperLogLogAdd(key, new[] { 1, 2, 3, 4, 5, 6 });
            _context.Cache.HyperLogLogAdd(key, new[] { 4, 5, 6, 7, 8, 9 });
            _context.Cache.HyperLogLogAdd(key, 10);

            var cnt = _context.Cache.HyperLogLogCount(key);
            Assert.AreEqual(10, cnt);
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
