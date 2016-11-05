using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Serializers;
using NUnit.Framework;
using System.Diagnostics;

namespace CachingFramework.Redis.UnitTest
{
    [TestFixture]
    public class UnitTestRedis
    {
        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_Cache_SetHashed_TK_TV(Context context)
        {
            var key = "UT_Cache_SetHashed_TK_TV";
            context.Cache.Remove(key);
            var users = GetUsers();

            context.Cache.SetHashed<User, User>(key, users[0], users[1]);
            context.Cache.SetHashed<User, User>(key, users[1], users[0]);

            var u1 = context.Cache.GetHashed<User, User>(key, users[0]);
            var u0 = context.Cache.GetHashed<User, User>(key, users[1]);

            Assert.AreEqual(users[0].Id, u0.Id);
            Assert.AreEqual(users[1].Id, u1.Id);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_Cache_SetHashed_TK_TV_WithTags(Context context)
        {
            var key = "UT_Cache_SetHashed_TK_TV_WithTags";
            context.Cache.Remove(key);
            var users = GetUsers();
            var dict = context.Collections.GetRedisDictionary<User, User>(key);

            context.Cache.SetHashed<User, User>(key, users[0], users[1], new[] { "tag 0->1", "common" });
            dict.Add(users[1], users[0], new[] { "tag 1->0", "common" });
            context.Cache.SetHashed<User>(key, "string field", users[0], new[] { "tag S->0", "common" });

            var u1 = context.Cache.GetHashed<User, User>(key, users[0]);
            var u0 = context.Cache.GetHashed<User, User>(key, users[1]);

            var all = context.Cache.GetObjectsByTag<User>("common").ToList();
            var t01 = context.Cache.GetObjectsByTag<User>("tag 0->1").ToList();
            var t10 = context.Cache.GetObjectsByTag<User>("tag 1->0").ToList();
            var tS0 = context.Cache.GetObjectsByTag<User>("tag S->0").ToList();

            Assert.AreEqual(users[0].Id, u0.Id);
            Assert.AreEqual(users[1].Id, u1.Id);

            Assert.AreEqual(3, all.Count);
            Assert.AreEqual(users[1].Id, t01[0].Id);
            Assert.AreEqual(users[0].Id, t10[0].Id);
            Assert.AreEqual(users[0].Id, tS0[0].Id);

            Assert.AreEqual(users[1].Id, dict[users[0]].Id);
        }

        [Test, TestCaseSource(typeof (Common), "All")]
        public void UT_Cache_Hash_Scan(Context context)
        {
            var key = "UT_Hash_Scan";
            int total = 10000;
            context.Cache.Remove(key);
            var fields = Enumerable.Range(1, total)
                .Select(i => new KeyValuePair<string, string>(i.ToString(), Guid.NewGuid().ToString()+ Guid.NewGuid().ToString()+ Guid.NewGuid().ToString()+ Guid.NewGuid().ToString()))
                .ToDictionary(k => k.Key, v => v.Value);
            context.Cache.SetHashed(key, fields);

            var dict = context.Collections.GetRedisDictionary<string, string>(key);
            Assert.AreEqual(total, dict.Count);

            var stp = Stopwatch.StartNew();
            var all = context.Cache.GetHashedAll<string>(key).Take(5).ToList();
            var allTime = stp.Elapsed.TotalMilliseconds;

            stp = Stopwatch.StartNew();
            var some = context.Cache.ScanHashed<string>(key, "*").Take(5).ToList();
            var someTime = stp.Elapsed.TotalMilliseconds;


            var c1 = context.Cache.ScanHashed<string>(key, "").Count();
            var c2 = context.Cache.ScanHashed<string>(key, null).Count();


            // Assert the HSCAN lasted at most the 0.5 times of the time of the HGETALL
            Assert.IsTrue((someTime / (double)allTime) < 0.5);
            Assert.AreEqual(total, c1);
            Assert.AreEqual(total, c2);
            context.Cache.Remove(key);
        }

        [Test, TestCaseSource(typeof(Common), "Raw")]
        public void UT_Context_Dispose(Context context)
        {
            var ctx = new Context(context.GetConnectionMultiplexer().Configuration, context.GetSerializer());
            ctx.Cache.SetObject("key", "value");
            ctx.Dispose();
            context.Cache.Remove("key");
            Assert.Throws<ObjectDisposedException>(() => ctx.Cache.SetObject("key", "value2"));
        }

        [Test, TestCaseSource(typeof (Common), "Raw")]
        public void UT_CacheNull(Context context)
        {
            Assert.Throws<ArgumentException>(() => context.Cache.SetObject(null, "this should fail"));
        }

        [Test, TestCaseSource(typeof (Common), "Raw")]
        public void UT_CacheSet_When(Context context)
        {
            var key = "UT_CacheSet_When";
            context.Cache.Remove(key);
            context.Cache.SetObject(key, "value", null, When.Exists);
            Assert.IsNull(context.Cache.GetObject<string>(key));
            context.Cache.SetObject(key, "value", null, When.NotExists);
            Assert.AreEqual("value", context.Cache.GetObject<string>(key));
            context.Cache.SetObject(key, "new", null, When.NotExists);
            Assert.AreEqual("value", context.Cache.GetObject<string>(key));
            context.Cache.SetObject(key, "new", null, When.Exists);
            Assert.AreEqual("new", context.Cache.GetObject<string>(key));
            context.Cache.Remove(key);
        }

        [Test, TestCaseSource(typeof(Common), "Raw")]
        public void UT_CacheSetHashed_When(Context context)
        {
            var key = "UT_CacheSetHashed_When";
            var field = "F1";
            context.Cache.Remove(key);
            context.Cache.SetHashed(key, field, "value", null, When.NotExists);
            Assert.AreEqual("value", context.Cache.GetHashed<string>(key, field));
            context.Cache.SetHashed(key, field, "new", null, When.NotExists);
            Assert.AreEqual("value", context.Cache.GetHashed<string>(key, field));
            context.Cache.SetHashed(key, field, "new", null, When.Always);
            Assert.AreEqual("new", context.Cache.GetHashed<string>(key, field));
            context.Cache.Remove(key);
        }

        [Test, TestCaseSource(typeof (Common), "Raw")]
        public void UT_CacheHackTag(Context context)
        {
            var key = "UT_CacheHackTag";
            context.Cache.InvalidateKeysByTag("tag1");
            context.Cache.SetObject(key, "some value", new [] {"tag1"});
            var keys = context.Cache.GetKeysByTag(new [] { "tag1" }).ToList();
            Assert.AreEqual(1, keys.Count);
            Assert.AreEqual(key, keys[0]);
            var tagset = context.Collections.GetRedisSet<string>(":$_tag_$:tag1");
            tagset.Add("FakeKey:$_->_$:FakeValue");
            var knc = context.Cache.GetKeysByTag(new [] { "tag1" }).ToList();
            var k = context.Cache.GetKeysByTag(new [] { "tag1" }, true).ToList();
            var v = context.Cache.GetObjectsByTag<string>("tag1").ToList();
            Assert.AreEqual(2, knc.Count);
            Assert.AreEqual(1, k.Count);
            Assert.AreEqual(1, v.Count);
        }

        [Test, TestCaseSource(typeof(Common), "BinAndRawAndJson")]
        public void UT_CacheSerializer(Context context)
        {
            var kss = "short:string";
            var kch = "char";
            var kds = "decimal";
            var kls = "long:string";
            var kpBool = "primitive:bool";
            var kpInt = "primitive:int";
            var kpLong = "primitive:long";
            var kpSingle = "primitive:single";
            var kpIntPtr = "primitive:intptr";
            var kpUInt16 = "primitive:uint16";
            var kpUInt32 = "primitive:uint32";
            var kpUInt64 = "primitive:uint64";
            var kby = "primitive:byte";
            var ksby = "primitive:sbyte";
            var ki16 = "primitive:int16";
            var ki32 = "primitive:int32";
            var kuip = "primitive:uintptr";
            var kdbl = "primitive:double";
            var kdt = "datetime";

            context.Cache.Remove(new[] { kss, kls, kpBool, kpInt, kpLong, kpSingle, kpIntPtr, kpUInt16, kpUInt32, kpUInt64, 
                kch, kds, kdt, kby, ksby, ki16, ki32, kuip, kdbl });
            var ss = "this is a short string";
            var ls = @"UTF-8 is a character encoding capable of encoding all possible characters, or code points, in Unicode.
                       The encoding is variable-length and uses 8-bit code units. It was designed for backward compatibility with ASCII, and to avoid the complications of endianness and byte order marks in the alternative UTF-16 and UTF-32 encodings. The name is derived from: Universal Coded Character Set + Transformation Format—8-bit.";
            context.Cache.SetObject(kss, ss);
            context.Cache.SetObject(kls, ls);
            context.Cache.SetObject<char>(kch, 'c');
            context.Cache.SetObject<Byte>(kby, Byte.MaxValue);
            context.Cache.SetObject<SByte>(ksby, SByte.MaxValue);
            context.Cache.SetObject<Int16>(ki16, Int16.MaxValue);
            context.Cache.SetObject<Int32>(ki32, Int32.MaxValue);
            context.Cache.SetObject<UIntPtr>(kuip, UIntPtr.Zero);
            context.Cache.SetObject<Double>(kdbl, Double.NegativeInfinity);
            context.Cache.SetObject<bool>(kpBool, true);
            context.Cache.SetObject<int>(kpInt, int.MaxValue);
            context.Cache.SetObject<Int64>(kpLong, Int64.MaxValue);
            var now = DateTime.Now;
            context.Cache.SetObject<DateTime>(kdt, now);
            context.Cache.SetObject<Single>(kpSingle, Single.MaxValue);
            context.Cache.SetObject<IntPtr>(kpIntPtr, new IntPtr(int.MaxValue));
            context.Cache.SetObject<UInt16>(kpUInt16, UInt16.MaxValue);
            context.Cache.SetObject<UInt32>(kpUInt32, UInt32.MaxValue);
            context.Cache.SetObject<UInt64>(kpUInt64, UInt64.MaxValue);
            context.Cache.SetObject<decimal>(kds, decimal.MaxValue);

            var ss_ = context.Cache.GetObject<string>(kss);
            var ls_ = context.Cache.GetObject<string>(kls);
            var pInt_ = context.Cache.GetObject<int>(kpInt);
            var pLong_ = context.Cache.GetObject<Int64>(kpLong);
            var pSingle_ = context.Cache.GetObject<Single>(kpSingle);
            var pUint16_ = context.Cache.GetObject<UInt16>(kpUInt16);
            var pUint32_ = context.Cache.GetObject<UInt32>(kpUInt32);
            UInt64 pUint64_ = 0;
            if (context.GetSerializer().GetType() != typeof(JsonSerializer))
            {
                pUint64_ = context.Cache.GetObject<UInt64>(kpUInt64);
            }
            var kch_ = context.Cache.GetObject<char>(kch);
            var kds_ = context.Cache.GetObject<decimal>(kds);
            var kdt_ = context.Cache.GetObject<DateTime>(kdt);
            var kby_ = context.Cache.GetObject<Byte>(kby);
            var ksby_ = context.Cache.GetObject<SByte>(ksby);
            var ki16_ = context.Cache.GetObject<Int16>(ki16);
            var ki32_ = context.Cache.GetObject<Int32>(ki32);
            var kuip_ = context.Cache.GetObject<UIntPtr>(kuip);
            var kdbl_ = context.Cache.GetObject<Double>(kdbl);
            var kpBool_ = context.Cache.GetObject<bool>(kpBool);

            Assert.AreEqual(ss, ss_);
            Assert.AreEqual(ls, ls_);
            Assert.AreEqual('c', kch_);
            Assert.IsTrue((now - kdt_).TotalMilliseconds < 0.001);
            Assert.AreEqual(decimal.MaxValue, kds_);
            Assert.AreEqual(Byte.MaxValue, kby_);
            Assert.AreEqual(SByte.MaxValue, ksby_);
            Assert.AreEqual(Int16.MaxValue, ki16_);
            Assert.AreEqual(Int32.MaxValue, ki32_);
            Assert.AreEqual(UIntPtr.Zero, kuip_);
            Assert.AreEqual(Double.NegativeInfinity, kdbl_);
            Assert.AreEqual(int.MaxValue, pInt_);
            Assert.AreEqual(long.MaxValue, pLong_);
            Assert.AreEqual(Single.Parse(Single.MaxValue.ToString("F")), Single.Parse(pSingle_.ToString("F")));
            Assert.AreEqual(UInt16.MaxValue, pUint16_);
            Assert.AreEqual(UInt32.MaxValue, pUint32_);
            if (context.GetSerializer().GetType() != typeof(JsonSerializer))
            {
                Assert.AreEqual(UInt64.MaxValue, pUint64_);
            }
            Assert.AreEqual(true, kpBool_);
            context.Cache.Remove(new[] { kss, kls, kpBool, kpInt, kpLong, kpSingle, kpIntPtr, kpUInt16, kpUInt32, kpUInt64, 
                kch, kds, kdt, kby, ksby, ki16, ki32, kuip, kdbl });
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_Cache_GetAllTags(Context context)
        {
            string key = "UT_Cache_GetAllTags";
            context.Cache.SetHashed(key, "1", "some value", new[] {"tag1", "tag2"});
            var tags = context.Cache.GetAllTags();
            Assert.IsTrue(tags.Contains("tag1"));
            Assert.IsTrue(tags.Contains("tag2"));
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_Cache_GetKeysByPattern(Context context)
        {
            string key = "UT_Cache_GetKeysByPattern";
            string key2 = "UT_Cache_GetKeys2ByPattern";
            context.Cache.FlushAll();
            context.Cache.SetObject(key, "some value", new[] { "tag3", "tag4" });
            context.Cache.SetObject(key2, "some value2");
            var keys = context.Cache.GetKeysByPattern("*");
            Assert.AreEqual(2, keys.Count());
            Assert.IsTrue(keys.Contains(key));
            Assert.IsTrue(keys.Contains(key2));
        }

        [Test]
        public void UT_Cache_RawOverrideSerializer()
        {
            var raw = new RawSerializer();
            raw.SetSerializerFor<User>(u => Encoding.UTF8.GetBytes(u.Id.ToString()),
                b => new User() {Id = int.Parse(Encoding.UTF8.GetString(b))});
            var ctx = new Context(Common.Config, raw);
            Thread.Sleep(1000);
            var users = GetUsers();
            string key = "UT_Cache_RawOverrideSerializer";
            string key2 = "UT_Cache_RawOverrideSerializer2";
            ctx.Cache.Remove(new[] {key, key2});
            ctx.Cache.SetObject(key, users[0]);
            ctx.Cache.SetHashed(key2, "X", users[1]);
            var v = ctx.Cache.GetObject<User>(key);
            var v2 = ctx.Cache.GetHashed<User>(key2, "X");
            var v3 = ctx.Cache.GetObject<int>(key);
            Assert.AreEqual(users[0].Id, v.Id);
            Assert.AreEqual(users[1].Id, v2.Id);
            Assert.AreEqual(users[0].Id, v3);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheByteArray(Context context)
        {
            context.Cache.SetObject("key", "jpeg");
            var o = context.Cache.GetObject<string>("key");
            string key = "UT_CacheByteArray";
            Jpeg jpeg = new Jpeg()
            {
                Data = Enumerable.Range(0, 200000)
                       .Select(i => (byte)((i * 223) % 256)).ToArray()
            };
            context.Cache.Remove(key);
            context.Cache.SetObject(key, jpeg);
            var jpeg2 = context.Cache.GetObject<Jpeg>(key);
            Assert.IsTrue(Enumerable.SequenceEqual(jpeg.Data, jpeg2.Data));
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheAddGet(Context context)
        {
            // Test the Add and Get methods
            var users = GetUsers();
            string key = "UT_CacheAddGet";
            context.Cache.Remove(key);
            context.Cache.SetObject(key, users[1]);
            context.Cache.SetObject(key, users[0], new string[]{});
            var user = context.Cache.GetObject<User>(key);
            Assert.AreEqual(1, user.Id);
            Assert.AreEqual(2, user.Deparments[0].Size);
            Assert.AreEqual("one", user.Deparments[0].Location.Name);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheFetch(Context context)
        {
            // Test the Fetch method
            string key = "UT_CacheFetch";
            int count = 0;
            context.Cache.Remove(key);
            var a = context.Cache.FetchObject(key, () => { count++; return GetUsers(); });
            var b = context.Cache.FetchObject(key, () => { count++; return GetUsers(); });
            context.Cache.FetchObject(key, () => { count++; return GetUsers(); });
            Assert.AreEqual(1, count);
            Assert.AreEqual(a[0].Id, b[0].Id);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheFetch_TTL(Context context)
        {
            // Test the Fetch method
            string key = "UT_CacheFetch_TTL";
            int count = 0;
            context.Cache.Remove(key);
            context.Cache.FetchObject(key, () => { count++; return GetUsers(); }, TimeSpan.FromSeconds(2));
            context.Cache.FetchObject(key, () => { count++; return GetUsers(); });
            Assert.AreEqual(1, count);
            Thread.Sleep(2200);
            context.Cache.FetchObject(key, () => { count++; return GetUsers(); });
            Assert.AreEqual(2, count);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheFetchHashed(Context context)
        {
            // Test the FetchHashed method
            string key = "UT_CacheFetchHashed";
            bool r = context.Cache.Remove(key);
            var users = GetUsers();
            var returnedUser1 = context.Cache.FetchHashed<User>(key, users[0].Id.ToString(), () => users[0]);
            var returnedUser2 = context.Cache.FetchHashed<User>(key, users[0].Id.ToString(), () => null);
            Assert.AreEqual(users[0].Id, returnedUser1.Id);
            Assert.AreEqual(users[0].Id, returnedUser2.Id);
        }

        [Test, TestCaseSource(typeof (Common), "All")]
        public void UT_CacheFetch_Nulls(Context context)
        {
            string key = "UT_CacheFetch_Nulls";
            context.Cache.Remove(key);
            string str = context.Cache.FetchObject<string>(key, () => null);
            Assert.IsNull(str);
            Assert.IsFalse(context.Cache.KeyExists(key));
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheFetchHashed_Nulls(Context context)
        {
            string key = "UT_CacheFetchHashed_Nulls";
            context.Cache.Remove(key);
            string str = context.Cache.FetchHashed<string>(key, "1", () => null);
            Assert.IsNull(str);
            Assert.IsFalse(context.Cache.KeyExists(key));
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheTryGetObject(Context context)
        {
            // Test the TryGetObject method
            string key = "UT_CacheTryGetObject";
            context.Cache.Remove(key);
            var users = GetUsers();
            User u1;
            context.Cache.SetObject(key, users[0]);
            bool b = context.Cache.TryGetObject(key + "x7rz9a", out u1);
            Assert.IsFalse(b);
            Assert.IsNull(u1);
            b = context.Cache.TryGetObject(key, out u1);
            Assert.IsTrue(b);
            Assert.IsNotNull(u1);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheTryGetHashed(Context context)
        {
            // Test the TryGetHashed method
            string key = "UT_CacheTryGetHashed";
            context.Cache.Remove(key);
            var users = GetUsers();
            User u1;
            context.Cache.SetHashed(key, users[0].Id.ToString(), users[0]);
            bool b = context.Cache.TryGetHashed(key, "a", out u1);
            Assert.IsFalse(b);
            Assert.IsNull(u1);
            b = context.Cache.TryGetHashed(key, users[0].Id.ToString(), out u1);
            Assert.IsTrue(b);
            Assert.IsNotNull(u1);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheGetSetObject(Context context)
        {
            // Test the GetSetObject method
            string key = "UT_CacheGetSetObject";
            context.Cache.Remove(key);
            var str = context.Cache.GetSetObject<string>(key, "1");
            Assert.IsNull(str);
            str = context.Cache.GetSetObject<string>(key, "2");
            Assert.AreEqual("1", str);
            str = context.Cache.GetObject<string>(key);
            Assert.AreEqual("2", str);
            context.Cache.Remove(key);
            var integer = context.Cache.GetSetObject<int>(key, 1);
            Assert.AreEqual(0, integer);
            integer = context.Cache.GetSetObject<int>(key, 2);
            Assert.AreEqual(1, integer);
            integer = context.Cache.GetObject<int>(key);
            Assert.AreEqual(2, integer);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheGetHashAll(Context context)
        {
            // Test the GetHashAll method
            string key = "UT_CacheGetHashAll";
            bool r = context.Cache.Remove(key);
            var users = GetUsers();
            foreach (var u in users)
            {
                User user = u;
                context.Cache.SetHashed(key, user.Id.ToString(), user);
            }
            var dict = context.Cache.GetHashedAll<User>(key);
            Assert.AreEqual(users.Count, dict.Count);
            Assert.AreEqual(1, dict["1"].Id);
            Assert.AreEqual(2, dict["2"].Id);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheRemove(Context context)
        {
            // Test the Remove method
            string key = "UT_CacheRemove";
            bool r = context.Cache.Remove(key);
            var users = GetUsers();
            context.Cache.SetObject(key, users[0]);

            r = context.Cache.Remove(key);
            Assert.IsTrue(r);
            Thread.Sleep(500);
            r = context.Cache.Remove(key);
            Assert.IsFalse(r);
            var returnedUser = context.Cache.GetObject<User>(key);
            Assert.IsNull(returnedUser);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheRemoveMultiple(Context context)
        {
            string key = "UT_CacheRemoveMultiple";
            for (int i = 0; i < 255; i++)
            {
                context.Cache.SetObject(key + i, new User() { Id = i });
            }
            for (int i = 0; i < 255; i++)
            {
                Assert.IsNotNull(context.Cache.GetObject<User>(key + i));
            }
            context.Cache.Remove(Enumerable.Range(0, 255).Select(i => key + i).ToArray());
            for (int i = 0; i < 255; i++)
            {
                Assert.IsNull(context.Cache.GetObject<User>(key + i));
            }
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheRemoveHashed(Context context)
        {
            // Test the Remove method for a complete hash set
            string key = "UT_CacheRemove_PreviouslyHashed";
            bool r = context.Cache.Remove(key);
            var users = GetUsers();
            foreach (var u in users)
            {
                User user = u;
                context.Cache.SetHashed(key, user.Id.ToString(), user);
            }
            r = context.Cache.RemoveHashed(key, "1");
            Assert.IsTrue(r);
            Thread.Sleep(200);
            r = context.Cache.RemoveHashed(key, "1");
            Assert.IsFalse(r);

            var returnedUser1 = context.Cache.GetHashed<User>(key, 1.ToString());
            var returnedUser2 = context.Cache.GetHashed<User>(key, 2.ToString());

            Assert.IsNull(returnedUser1);
            Assert.AreEqual(2, returnedUser2.Id);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheRemove_PreviouslyHashed(Context context)
        {
            // Test the Remove hashed method
            string key = "UT_CacheRemoveHashed";
            bool r = context.Cache.Remove(key);
            var users = GetUsers();
            foreach (var u in users)
            {
                User user = u;
                context.Cache.SetHashed(key, user.Id.ToString(), user);
            }
            r = context.Cache.Remove(key);
            Assert.IsTrue(r);
            Thread.Sleep(200);
            r = context.Cache.Remove(key);
            Assert.IsFalse(r);

            var returnedUser1 = context.Cache.GetHashed<User>(key, 1.ToString());
            var returnedUser2 = context.Cache.GetHashed<User>(key, 2.ToString());

            Assert.IsNull(returnedUser1);
            Assert.IsNull(returnedUser2);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheAdd_Expiration(Context context)
        {
            // Test the expiration of the Add method
            var users = GetUsers();
            string key = "SomeKey";
            context.Cache.Remove(key);
            context.Cache.SetObject(key, users[0], TimeSpan.FromMilliseconds(1000));
            var user = context.Cache.GetObject<User>(key);
            Assert.AreEqual(1, user.Id);
            Assert.AreEqual(2, user.Deparments[0].Size);
            Assert.AreEqual("one", user.Deparments[0].Location.Name);
            Thread.Sleep(1500);
            user = context.Cache.GetObject<User>(key);
            Assert.IsNull(user);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheAddHashed_Expiration_1(Context context)
        {
            // Test the expiration of the AddHashed method (MAX ttl applies)
            var users = GetUsers();
            string key = "SomeKey";
            context.Cache.Remove(key);

            context.Cache.SetHashed(key, "2", users[1], TimeSpan.FromMilliseconds(10000));
            context.Cache.SetHashed(key, "1", users[0], TimeSpan.FromMilliseconds(1000));
            Thread.Sleep(1200);

            var user1 = context.Cache.GetHashed<User>(key, "1");
            var user2 = context.Cache.GetHashed<User>(key, "2");
            Assert.IsNotNull(user1);
            Assert.IsNotNull(user2);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheAddHashed_Expiration_2(Context context)
        {
            // Test the expiration of the Fetch method (last larger expiration applies)
            var users = GetUsers();
            string key = "SomeKey";
            context.Cache.Remove(key);

            context.Cache.SetHashed(key, "1", users[0], TimeSpan.FromMilliseconds(1000));
            context.Cache.SetHashed(key, "2", users[1], TimeSpan.FromMilliseconds(10000));
            Thread.Sleep(1200);

            var user1 = context.Cache.GetHashed<User>(key, "1");
            var user2 = context.Cache.GetHashed<User>(key, "2");
            Assert.IsNotNull(user1);
            Assert.IsNotNull(user2);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheAddHashed_Expiration_3(Context context)
        {
            // Test the expiration of the Fetch method (last no-expiration applies)
            var users = GetUsers();
            string key = "SomeKey";
            context.Cache.Remove(key);
            var ms = 1000;
            context.Cache.SetHashed(key, "1", users[0], TimeSpan.FromMilliseconds(ms));
            context.Cache.SetHashed(key, "2", users[1]);
            Thread.Sleep(ms + 200);

            var user1 = context.Cache.GetHashed<User>(key, "1");
            var user2 = context.Cache.GetHashed<User>(key, "2");
            Assert.IsNotNull(user1);
            Assert.IsNotNull(user2);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheSetHashed_Tags(Context context)
        {
            string key = "UT_CacheSetHashed_Tags";
            var users = GetUsers();
            context.Cache.Remove(key);
            context.Cache.InvalidateKeysByTag("common", "tagA", "tagB", "whole");
            context.Cache.SetHashed(key, "A", users[0], new[] { "common", "tagA" });
            context.Cache.SetHashed(key, "B", users[0], new[] { "tagB" });
            context.Cache.AddTagsToHashField(key, "B", new[] {"common"});
            context.Cache.SetHashed(key, "C", users[1], new[] { "common", "tagC" });        
            context.Cache.AddTagsToKey(key, new [] { "whole" });
            var kwhole = context.Cache.GetKeysByTag(new [] { "whole" });
            var kcmn = context.Cache.GetKeysByTag(new [] { "common" });
            var ka = context.Cache.GetKeysByTag(new [] { "tagA" });
            var kb = context.Cache.GetKeysByTag(new[] { "tagB" });
            var kc = context.Cache.GetKeysByTag(new[] { "tagC" });
            var kab = context.Cache.GetKeysByTag(new[] { "tagA", "tagB" });
            Assert.AreEqual(3, kcmn.Count());
            context.Cache.InvalidateKeysByTag("tagA");
            ka = context.Cache.GetKeysByTag(new[] { "tagA" });
            kcmn = context.Cache.GetKeysByTag(new[] { "common" }, true);
            Assert.IsFalse(ka.Any());
            Assert.AreEqual(2, kcmn.Count());
            var objs = context.Cache.GetObjectsByTag<User>("common").ToList();
            Assert.AreEqual(2, objs.Count);
            context.Cache.RemoveTagsFromHashField(key, "B", new [] { "common" });
            objs = context.Cache.GetObjectsByTag<User>("common").ToList();
            Assert.AreEqual(1, objs.Count);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheFetchHashed_Tags(Context context)
        {
            string key = "UT_CacheFetchHashed_Tags";
            var users = GetUsers();
            context.Cache.InvalidateKeysByTag("common", "tag0", "tag1", "miss");
            context.Cache.Remove(key);
            var u1 = context.Cache.FetchHashed(key, users[0].Id.ToString(), () => users[0], new[] { "common", "tag0"});
            var u2 = context.Cache.FetchHashed(key, users[1].Id.ToString(), () => users[1], new[] { "common", "tag1" });
            var u1t = context.Cache.GetObjectsByTag<User>("tag1").ToList();
            var ust = context.Cache.GetObjectsByTag<User>("common").ToList();
            Assert.AreEqual(1, u1t.Count);
            Assert.AreEqual(2, ust.Count);
            Assert.AreEqual(users[1].Id, u1t[0].Id);
            int i = 0;
            var u = context.Cache.FetchHashed(key, users[1].Id.ToString(), () => { i++; return new User(); }, new[] { "miss" });
            Assert.AreEqual(0, context.Cache.GetKeysByTag(new[] {"miss"}).Count());
            Assert.AreEqual(0, i);
            Assert.AreEqual(users[1].Id, u.Id);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheSetWithTags(Context context)
        {
            string key = "miset"; //UT_CacheSetWithTags
            context.Cache.Remove(key);
            var users = GetUsers();
            context.Cache.SetObject(key, users[0], new[] { "user:" + users[0].Id });
            var keys = context.Cache.GetKeysByTag(new[] { "user:" + users[0].Id });
            var value = context.Cache.GetObject<User>(keys.First());
            Assert.IsTrue(keys.Contains(key));
            Assert.IsNotNull(value);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheFetchWithTags(Context context)
        {
            string key = "UT_CacheFetchWithTags";
            context.Cache.Remove(key);
            string tag1 = "UT_CacheFetchWithTags-Tag1";
            string tag2 = "UT_CacheFetchWithTags-Tag2";
            context.Cache.FetchObject(key, () => "test value 1", new[] {tag1});
            context.Cache.FetchObject(key, () => "should not be updated", new[] { tag2 });
            var keys = context.Cache.GetKeysByTag(new [] {tag1}).ToList();
            var value = context.Cache.GetObject<string>(keys.First()).ToList();
            Assert.IsTrue(keys.Contains(key));
            Assert.IsNotNull(value);
            keys = context.Cache.GetKeysByTag(new [] {tag2}).ToList();
            Assert.IsFalse(keys.Contains(key));
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheSetWithTags_PersistentOverridesExpiration(Context context)
        {
            string key1 = "UT_CacheSetWithTags_Special1";
            string key2 = "UT_CacheSetWithTags_Special2";
            string tag = "UT_CacheSetWithTags_Special-Tag1";
            context.Cache.InvalidateKeysByTag(tag);
            context.Cache.SetObject(key1, "test value 1", new[] { tag }, TimeSpan.FromSeconds(1));
            context.Cache.SetObject(key2, "test value 2", new[] { tag });
            Thread.Sleep(2000);
            var keys = context.Cache.GetKeysByTag(new[] { tag }).ToList();
            var keysCleaned = context.Cache.GetKeysByTag(new[] { tag }, true).ToList();
            Assert.AreEqual(2, keys.Count);
            Assert.AreEqual(1, keysCleaned.Count);
            Assert.IsTrue(keys.Contains(key1));
            Assert.IsTrue(keys.Contains(key2));
            Assert.IsTrue(keysCleaned.Contains(key2));
            context.Cache.InvalidateKeysByTag(tag);
        }


        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheSetWithTags_Expiration(Context context)
        {
            string key = "UT_CacheSetWithTags_Expiration";
            string key2 = "UT_CacheSetWithTags_Expiration2";
            context.Cache.Remove(key);
            context.Cache.Remove(key2);
            var users = GetUsers();
            context.Cache.InvalidateKeysByTag("user:" + users[0].Id, "user:" + users[1].Id, "user-info");

            context.Cache.SetObject(key, users[0], new[] { "user:" + users[0].Id, "user-info" }, TimeSpan.FromSeconds(1));
            context.Cache.SetObject(key2, users[1], new[] { "user:" + users[1].Id, "user-info" }, TimeSpan.FromSeconds(5));
            var keys = context.Cache.GetKeysByTag(new[] { "user:" + users[0].Id }).ToList();
            Assert.IsTrue(keys.Contains(key));
            var value = context.Cache.GetObject<User>(keys.First());
            Assert.IsNotNull(value);
            Thread.Sleep(1200);
            var keys2 = context.Cache.GetKeysByTag(new[] { "user:" + users[0].Id });
            Assert.IsFalse(keys2.Contains(key));
            value = context.Cache.GetObject<User>(key);
            Assert.IsNull(value);
            var keys3 = context.Cache.GetKeysByTag(new[] { "user-info" });
            Assert.IsTrue(keys3.Contains(key2));
            Thread.Sleep(4000);
            var keys4 = context.Cache.GetKeysByTag(new[] { "user-info" });
            Assert.IsFalse(keys4.Contains(key2));
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheSetWithTags_Removal(Context context)
        {
            string key = "UT_CacheSetWithTags_Removal";
            context.Cache.Remove(key);
            var users = GetUsers();
            context.Cache.InvalidateKeysByTag("user:" + users[0].Id);

            context.Cache.SetObject(key, users[0], new[] { "user:" + users[0].Id });
            var keys = context.Cache.GetKeysByTag(new[] { "user:" + users[0].Id }, true);
            Assert.IsTrue(keys.Contains(key));
            context.Cache.Remove(key);
            var keys2 = context.Cache.GetKeysByTag(new[] { "user:" + users[0].Id }, true);
            Assert.IsFalse(keys2.Contains(key));
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheSetWithTags_Multiple(Context context)
        {
            string key0 = "UT_CacheSetWithTags_Multiple0";
            string key1 = "UT_CacheSetWithTags_Multiple1";
            context.Cache.Remove(key0);
            context.Cache.Remove(key1);
            var users = GetUsers();
            context.Cache.InvalidateKeysByTag("user:" + users[0].Id, "user:" + users[1].Id, "user-info");

            context.Cache.SetObject(key0, users[0], new[] { "user:" + users[0].Id, "user-info" });
            context.Cache.SetObject(key1, users[1], new[] { "user:" + users[1].Id, "user-info" });
            var keys0 = context.Cache.GetKeysByTag(new[] { "user:" + users[0].Id });
            var keys1 = context.Cache.GetKeysByTag(new[] { "user:" + users[1].Id });
            var keys = context.Cache.GetKeysByTag(new[] { "user-info" }).ToList();
            Assert.IsTrue(keys0.Contains(key0));
            Assert.IsTrue(keys1.Contains(key1));
            Assert.IsTrue(keys.Contains(key0) && keys.Contains(key1));
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheRemoveByTags(Context context)
        {
            string key1 = "UT_CacheRemoveByTags1";
            string key2 = "UT_CacheRemoveByTags2";
            context.Cache.Remove(key1);
            context.Cache.Remove(key2);
            var users = GetUsers();
            string tag1 = "user:" + users[0].Id;
            string tag2 = "user:" + users[1].Id;
            context.Cache.InvalidateKeysByTag(tag1, tag2);
            context.Cache.SetObject(key1, users[0], new[] { tag1 });
            context.Cache.SetObject(key2, users[1], new[] { tag2 });
            context.Cache.InvalidateKeysByTag(tag1, tag2);
            var keys = context.Cache.GetKeysByTag(new [] {tag1, tag2});
            var user = context.Cache.GetObject<User>(key1);
            Assert.IsNull(user);
            Assert.AreEqual(0, keys.Count());
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheGetObjectsByTag(Context context)
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
                context.Cache.SetObject(string.Format(key, i), users[users.Count - 1], new[] { i % 3 == 0 ? tag1 : i % 3 == 1 ? tag2 : tag3 });
            }
            var t1Users = context.Cache.GetObjectsByTag<User>(tag1).ToList();
            var t2Users = context.Cache.GetObjectsByTag<User>(tag2).ToList();
            var t3Users = context.Cache.GetObjectsByTag<User>(tag3).ToList();
            Assert.IsTrue(t1Users.TrueForAll(u => u.Id % 3 == 0));
            Assert.IsTrue(t2Users.TrueForAll(u => u.Id % 3 == 1));
            Assert.IsTrue(t3Users.TrueForAll(u => u.Id % 3 == 2));
            context.Cache.InvalidateKeysByTag(tag1, tag2, tag3);
            t1Users = context.Cache.GetObjectsByTag<User>(tag1).ToList();
            Assert.AreEqual(0, t1Users.Count);
            Assert.IsNull(context.Cache.GetObject<User>(string.Format(key, 1)));
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheAddRemoveTagToKey(Context context)
        {
            string key = "UT_CacheAddRemoveTagToKey";
            string tag = "UT_CacheAddRemoveTagToKey_Tag";
            context.Cache.Remove(key);
            context.Cache.SetObject(key, "value");
            context.Cache.AddTagsToKey(key, new[] { tag });
            var keys = context.Cache.GetKeysByTag(new [] { tag }).ToList();
            Assert.IsTrue(keys.Contains(key));
            context.Cache.RemoveTagsFromKey(key, new[] { tag });
            keys = context.Cache.GetKeysByTag(new [] { tag }).ToList();
            Assert.IsFalse(keys.Contains(key));

        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheSetHashedAll(Context context)
        {
            string key = "UT_CacheSetHashedAll";
            context.Cache.Remove(key);
            var users = GetUsers();
            IDictionary<string, User> allUsers = users.ToDictionary(k => k.Id.ToString());
            context.Cache.SetHashed(key, allUsers);
            var response = context.Cache.GetHashedAll<User>(key);
            Assert.AreEqual(users.Count, response.Count);
            Assert.IsTrue(users.All(x => response.ContainsKey(x.Id.ToString())));
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_Cache_HllAddCount(Context context)
        {
            string key = "UT_Cache_HllAddCount";
            context.Cache.Remove(key);
            context.Cache.HyperLogLogAdd(key, new[] { 1, 2, 3, 4, 5, 6 });
            context.Cache.HyperLogLogAdd(key, new[] { 4, 5, 6, 7, 8, 9 });
            context.Cache.HyperLogLogAdd(key, 10);

            var cnt = context.Cache.HyperLogLogCount(key);
            Assert.AreEqual(10, cnt);
        }

#if (NET45 || NET461)
        [Test, TestCaseSource(typeof(Common), "Bin")]
        public void UT_CacheSerialization(Context context)
        {
            string key = "UT_CacheSerialization";
            context.Cache.Remove(key);
            Exception exItem = null;
            try
            {
                throw new Exception("this is a test exception to test serialization", new ArgumentException("this is an inner exception", "param"));
            }
            catch (Exception ex)
            {
                ex.Data.Add("some data", "to test");
                exItem = ex;
            }
            context.Cache.SetObject(key, exItem);
            var exFinal = context.Cache.GetObject<Exception>(key);
            Assert.AreEqual(exItem.Data.Count, exFinal.Data.Count);
            Assert.AreEqual(exItem.InnerException.Message, exFinal.InnerException.Message);
            Assert.AreEqual(exItem.StackTrace, exFinal.StackTrace);
        }

        [Test, TestCaseSource(typeof(Common), "Bin")]
        public void UT_CacheSetHashed_MultipleFieldsDistinctTypes(Context context)
        {
            string key = "UT_CacheSetHashed_MultipleFieldsDistinctTypes";
            context.Cache.Remove(key);
            var dict = new Dictionary<string, object>()
            {
                { "a", new User { Id = 222 }},
                { "2", new Department { Id = 3 }}
            };
            context.Cache.SetHashed(key, "a", dict["a"]);
            context.Cache.SetHashed(key, "2", dict["2"]);
            context.Cache.SetHashed(key, "D", new Location() { Id = 444 });

            var user = context.Cache.GetHashed<User>(key, "a");
            var dept = context.Cache.GetHashed<Department>(key, "2");
            var loc = context.Cache.GetHashed<Location>(key, "D");
            var all = context.Cache.GetHashedAll<object>(key);

            Assert.AreEqual(222, user.Id);
            Assert.AreEqual(3, dept.Id);
            Assert.AreEqual(444, loc.Id);

            Assert.AreEqual(3, all.Count);

            Assert.AreEqual(222, ((User)all["a"]).Id);
            Assert.AreEqual(3, ((Department)all["2"]).Id);
            Assert.AreEqual(444, ((Location)all["D"]).Id);
        }

        [Test, TestCaseSource(typeof(Common), "Bin")]
        public void UT_CacheFetch_TagsBuilder(Context context)
        {
            string key = "UT_CacheFetch_TagsBuilder";
            var users = GetUsers();
            var user = users[0];
            context.Cache.Remove(key);
            context.Cache.InvalidateKeysByTag("user-id-tag:" + user.Id);
            context.Cache.FetchObject(key, () => user, u => new[] { "user-id-tag:" + u.Id });
            context.Cache.FetchObject(key, () => (User)null, u => new[] { "wrong" });
            Assert.AreEqual(0, context.Cache.GetKeysByTag(new[] { "wrong" }).Count());
            var result = context.Cache.GetObjectsByTag<User>("user-id-tag:" + user.Id).First();
            Assert.AreEqual(0, context.Cache.GetKeysByTag(new[] { "wrong" }).Count());
            Assert.AreEqual(user.Id, result.Id);
        }

        [Test, TestCaseSource(typeof(Common), "Bin")]
        public void UT_CacheFetchHashed_TagsBuilder(Context context)
        {
            string key = "UT_CacheFetchHashed_TagsBuilder";
            string field = "field";
            var users = GetUsers();
            var user = users[0];
            context.Cache.Remove(key);
            context.Cache.InvalidateKeysByTag("user-id-tag:" + user.Id);
            context.Cache.FetchHashed(key, field, () => user, u => new[] { "user-id-tag:" + u.Id });
            context.Cache.FetchHashed(key, field, () => (User)null, u => new[] { "wrong" });
            Assert.AreEqual(0, context.Cache.GetKeysByTag(new[] { "wrong" }).Count());
            var result = context.Cache.GetObjectsByTag<User>(new[] { "user-id-tag:" + user.Id }).First();
            Assert.AreEqual(0, context.Cache.GetKeysByTag(new[] { "wrong" }).Count());
            Assert.AreEqual(user.Id, result.Id);
        }
        [Test, TestCaseSource(typeof (Common), "Bin")]
        public void UT_CacheTagRename(Context context)
        {
            string key = "UT_CacheTagRename";
            context.Cache.Remove(key);
            string tag1 = "UT_CacheTagRename-Tag1";
            string tag2 = "UT_CacheTagRename-Tag2";
            context.Cache.InvalidateKeysByTag(tag1, tag2);
            var user = GetUsers()[0];
            context.Cache.SetObject(key, user, new [] { tag1 });
            Assert.AreEqual(1, context.Cache.GetKeysByTag(new [] { tag1 }).Count());
            context.Cache.RenameTagForKey(key, tag1, tag2);
            Assert.AreEqual(0, context.Cache.GetKeysByTag(new[] { tag1 }).Count());
            Assert.AreEqual(1, context.Cache.GetKeysByTag(new[] { tag2 }).Count());
            context.Cache.RemoveTagsFromKey(key, new [] { tag2 });
            context.Cache.RenameTagForKey(key, tag2, tag1);
            Assert.AreEqual(0, context.Cache.GetKeysByTag(new[] { tag1 }).Count());
            Assert.AreEqual(0, context.Cache.GetKeysByTag(new[] { tag2 }).Count());
        }

        [Test, TestCaseSource(typeof(Common), "Bin")]
        public void UT_CacheFieldTagRename(Context context)
        {
            string key = "UT_CacheFieldTagRename";
            string field = "field";
            context.Cache.Remove(key);
            string tag1 = "UT_CacheFieldTagRename-Tag1";
            string tag2 = "UT_CacheFieldTagRename-Tag2";
            context.Cache.InvalidateKeysByTag(tag1, tag2);
            var user = GetUsers()[0];
            context.Cache.SetHashed(key, field, user, new[] { tag1 });
            Assert.AreEqual(1, context.Cache.GetKeysByTag(new[] { tag1 }).Count());
            context.Cache.RenameTagForHashField(key, field, tag1, tag2);
            Assert.AreEqual(0, context.Cache.GetKeysByTag(new[] { tag1 }).Count());
            Assert.AreEqual(1, context.Cache.GetKeysByTag(new[] { tag2 }).Count());
            context.Cache.RemoveTagsFromHashField(key, field, new[] { tag2 });
            context.Cache.RemoveTagsFromHashField(key, field, new [] { tag2, tag1 });
            Assert.AreEqual(0, context.Cache.GetKeysByTag(new[] { tag1 }).Count());
            Assert.AreEqual(0, context.Cache.GetKeysByTag(new[] { tag2 }).Count());
        }

        [Test]
        public void UT_Cache_RawOverrideSerializer_object()
        {
            var raw = new RawSerializer();
            raw.SetSerializerFor<object>(o => Encoding.UTF8.GetBytes(o.GetHashCode().ToString()),
                b => int.Parse(Encoding.UTF8.GetString(b)));
            var ctx = new Context(Common.Config, raw);
            Thread.Sleep(1000);
            string key = "UT_Cache_RawOverrideSerializer_object";
            ctx.Cache.Remove(new[] { key });
            User usr = new User();
            ctx.Cache.SetObject<object>(key, usr);
            var v = ctx.Cache.GetObject<object>(key);
            Assert.AreEqual(usr.GetHashCode(), v);
        }
#endif

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
