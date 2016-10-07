using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Serializers;
using NUnit.Framework;
using System.Threading.Tasks;

namespace CachingFramework.Redis.UnitTest
{
    [TestFixture]
    public class UnitTestRedis_Async
    {
        [Test, TestCaseSource(typeof(Common), "Raw")]
        public async Task UT_Context_Dispose_Async(Context context)
        {
            var ctx = new Context(context.GetConnectionMultiplexer().Configuration, context.GetSerializer());
            await ctx.Cache.SetObjectAsync("key", "value");
            ctx.Dispose();
            await context.Cache.RemoveAsync("key");
            Assert.ThrowsAsync<ObjectDisposedException>(async () => await ctx.Cache.SetObjectAsync("key", "value2"));
        }

        [Test, TestCaseSource(typeof (Common), "Raw")]
        public async Task UT_CacheNull_Async(Context context)
        {
            Assert.ThrowsAsync<ArgumentException>(async () => await context.Cache.SetObjectAsync(null, "this should fail"));
        }

        [Test, TestCaseSource(typeof (Common), "Raw")]
        public async Task UT_CacheSet_When_Async(Context context)
        {
            var key = "UT_CacheSet_When_Async";
            await context.Cache.RemoveAsync(key);
            await context.Cache.SetObjectAsync(key, "value", null, When.Exists);
            Assert.IsNull(context.Cache.GetObject<string>(key));
            await context.Cache.SetObjectAsync(key, "value", null, When.NotExists);
            Assert.AreEqual("value", context.Cache.GetObject<string>(key));
            await context.Cache.SetObjectAsync(key, "new", null, When.NotExists);
            Assert.AreEqual("value", context.Cache.GetObject<string>(key));
            await context.Cache.SetObjectAsync(key, "new", null, When.Exists);
            Assert.AreEqual("new", context.Cache.GetObject<string>(key));
            await context.Cache.RemoveAsync(key);
        }

        [Test, TestCaseSource(typeof(Common), "Raw")]
        public async Task UT_CacheSetHashed_When_Async(Context context)
        {
            var key = "UT_CacheSetHashed_When_Async";
            var field = "F1";
            await context.Cache.RemoveAsync(key);
            context.Cache.SetHashed(key, field, "value", null, When.NotExists);
            Assert.AreEqual("value", context.Cache.GetHashed<string>(key, field));
            context.Cache.SetHashed(key, field, "new", null, When.NotExists);
            Assert.AreEqual("value", context.Cache.GetHashed<string>(key, field));
            context.Cache.SetHashed(key, field, "new", null, When.Always);
            Assert.AreEqual("new", context.Cache.GetHashed<string>(key, field));
            await context.Cache.RemoveAsync(key);
        }

        [Test, TestCaseSource(typeof (Common), "Raw")]
        public async Task UT_CacheHackTag_Async(Context context)
        {
            var key = "UT_CacheHackTag_Async";
            await context.Cache.InvalidateKeysByTagAsync("tag1");
            await context.Cache.SetObjectAsync(key, "some value", new [] {"tag1"});
            var keys = (await context.Cache.GetKeysByTagAsync(new [] { "tag1" })).ToList();
            Assert.AreEqual(1, keys.Count);
            Assert.AreEqual(key, keys[0]);
            var tagset = context.Collections.GetRedisSet<string>(":$_tag_$:tag1");
            tagset.Add("FakeKey:$_->_$:FakeValue");
            var knc = (await context.Cache.GetKeysByTagAsync(new [] { "tag1" })).ToList();
            var k = (await context.Cache.GetKeysByTagAsync(new [] { "tag1" }, true)).ToList();
            var v = context.Cache.GetObjectsByTag<string>("tag1").ToList();
            Assert.AreEqual(2, knc.Count);
            Assert.AreEqual(1, k.Count);
            Assert.AreEqual(1, v.Count);
        }

        [Test, TestCaseSource(typeof(Common), "BinAndRawAndJson")]
        public async Task UT_CacheSerializer_Async(Context context)
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
            await context.Cache.SetObjectAsync(kss, ss);
            await context.Cache.SetObjectAsync(kls, ls);
            await context.Cache.SetObjectAsync<char>(kch, 'c');
            await context.Cache.SetObjectAsync<Byte>(kby, Byte.MaxValue);
            await context.Cache.SetObjectAsync<SByte>(ksby, SByte.MaxValue);
            await context.Cache.SetObjectAsync<Int16>(ki16, Int16.MaxValue);
            await context.Cache.SetObjectAsync<Int32>(ki32, Int32.MaxValue);
            await context.Cache.SetObjectAsync<UIntPtr>(kuip, UIntPtr.Zero);
            await context.Cache.SetObjectAsync<Double>(kdbl, Double.NegativeInfinity);
            await context.Cache.SetObjectAsync<bool>(kpBool, true);
            await context.Cache.SetObjectAsync<int>(kpInt, int.MaxValue);
            await context.Cache.SetObjectAsync<Int64>(kpLong, Int64.MaxValue);
            var now = DateTime.Now;
            await context.Cache.SetObjectAsync<DateTime>(kdt, now);
            await context.Cache.SetObjectAsync<Single>(kpSingle, Single.MaxValue);
            await context.Cache.SetObjectAsync<IntPtr>(kpIntPtr, new IntPtr(int.MaxValue));
            await context.Cache.SetObjectAsync<UInt16>(kpUInt16, UInt16.MaxValue);
            await context.Cache.SetObjectAsync<UInt32>(kpUInt32, UInt32.MaxValue);
            await context.Cache.SetObjectAsync<UInt64>(kpUInt64, UInt64.MaxValue);
            await context.Cache.SetObjectAsync<decimal>(kds, decimal.MaxValue);

            var ss_ = context.Cache.GetObject<string>(kss);
            var ls_ = context.Cache.GetObject<string>(kls);
            var pInt_ = context.Cache.GetObject<int>(kpInt);
            var pLong_ = context.Cache.GetObject<Int64>(kpLong);
            var pSingle_ = context.Cache.GetObject<Single>(kpSingle);
            var pIntPtr_ = context.Cache.GetObject<IntPtr>(kpIntPtr);
            var pUint16_ = context.Cache.GetObject<UInt16>(kpUInt16);
            var pUint32_ = context.Cache.GetObject<UInt32>(kpUInt32);
            var pUint64_ = context.Cache.GetObject<UInt64>(kpUInt64);
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
            Assert.AreEqual(new IntPtr(int.MaxValue), pIntPtr_);
            Assert.AreEqual(UInt16.MaxValue, pUint16_);
            Assert.AreEqual(UInt32.MaxValue, pUint32_);
            Assert.AreEqual(UInt64.MaxValue, pUint64_);
            Assert.AreEqual(true, kpBool_);
            context.Cache.Remove(new[] { kss, kls, kpBool, kpInt, kpLong, kpSingle, kpIntPtr, kpUInt16, kpUInt32, kpUInt64, 
                kch, kds, kdt, kby, ksby, ki16, ki32, kuip, kdbl });
        }

        [Test]
        public async Task UT_Cache_RawOverrideSerializer_Async()
        {
            var raw = new RawSerializer();
            raw.SetSerializerFor<User>(u => Encoding.UTF8.GetBytes(u.Id.ToString()),
                b => new User() {Id = int.Parse(Encoding.UTF8.GetString(b))});
            var ctx = new Context(Common.Config, raw);
            Thread.Sleep(100);
            var users = await GetUsersAsync();
            string key = "UT_Cache_RawOverrideSerializer_Async";
            string key2 = "UT_Cache_RawOverrideSerializer_Async2";
            ctx.Cache.Remove(new[] {key, key2});
            await ctx.Cache.SetObjectAsync(key, users[0]);
            ctx.Cache.SetHashed(key2, "X", users[1]);
            var v = await ctx.Cache.GetObjectAsync<User>(key);
            var v2 = await ctx.Cache.GetHashedAsync<User>(key2, "X");
            var v3 = await ctx.Cache.GetObjectAsync<int>(key);
            Assert.AreEqual(users[0].Id, v.Id);
            Assert.AreEqual(users[1].Id, v2.Id);
            Assert.AreEqual(users[0].Id, v3);
        }

        [Test]
        public async Task UT_Cache_RawOverrideSerializer_object_Async()
        {
            var raw = new RawSerializer();
            raw.SetSerializerFor<object>(o => Encoding.UTF8.GetBytes(o.GetHashCode().ToString()),
                b => int.Parse(Encoding.UTF8.GetString(b)));
            var ctx = new Context(Common.Config, raw);
            Thread.Sleep(100);
            string key = "UT_Cache_RawOverrideSerializer_object_Async";
            ctx.Cache.Remove(new[] { key });
            await ctx.Cache.SetObjectAsync(key, this);
            var v = await ctx.Cache.GetObjectAsync<int>(key);
            Assert.AreEqual(this.GetHashCode(), v);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheByteArray_Async(Context context)
        {
            context.Cache.SetObject("key", "jpeg");
            var o = await context.Cache.GetObjectAsync<string>("key");
            string key = "UT_CacheByteArray_Async";
            Jpeg jpeg = new Jpeg()
            {
                Data = Enumerable.Range(0, 200000)
                       .Select(i => (byte)((i * 223) % 256)).ToArray()
            };
            context.Cache.Remove(key);
            await context.Cache.SetObjectAsync(key, jpeg);
            var jpeg2 = await context.Cache.GetObjectAsync<Jpeg>(key);
            Assert.IsTrue(Enumerable.SequenceEqual(jpeg.Data, jpeg2.Data));
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheAddGet_Async(Context context)
        {
            // Test the Add and Get methods
            var users = await GetUsersAsync();
            string key = "UT_CacheAddGet_Async";
            await context.Cache.RemoveAsync(key);
            await context.Cache.SetObjectAsync(key, users[1]);
            await context.Cache.SetObjectAsync(key, users[0], new string[]{});
            var user = await context.Cache.GetObjectAsync<User>(key);
            Assert.AreEqual(1, user.Id);
            Assert.AreEqual(2, user.Deparments[0].Size);
            Assert.AreEqual("one", user.Deparments[0].Location.Name);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheFetch_Async(Context context)
        {
            // Test the Fetch method
            string key = "UT_CacheFetch_Async";
            int count = 0;
            context.Cache.Remove(key);
            var a = await context.Cache.FetchObjectAsync(key, async () => { count++; return await GetUsersAsync(); });
            var b = await context.Cache.FetchObjectAsync(key, async () => { count++; return await GetUsersAsync(); });
            await context.Cache.FetchObjectAsync(key, async  () => { count++; return await GetUsersAsync(); });
            Assert.AreEqual(1, count);
            Assert.AreEqual(a[0].Id, b[0].Id);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheFetch_TTL_Async(Context context)
        {
            // Test the Fetch method
            string key = "UT_CacheFetch_TTL_Async";
            int count = 0;
            context.Cache.Remove(key);
            await context.Cache.FetchObjectAsync(key, async () => { count++; return await GetUsersAsync(); }, TimeSpan.FromSeconds(2));
            await context.Cache.FetchObjectAsync(key, async () => { count++; return await GetUsersAsync(); });
            Assert.AreEqual(1, count);
            Thread.Sleep(2200);
            await context.Cache.FetchObjectAsync(key, async () => { count++; return await GetUsersAsync(); });
            Assert.AreEqual(2, count);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheFetchHashed_Async(Context context)
        {
            // Test the FetchHashed method
            string key = "UT_CacheFetchHashed_Async";
            bool r = await context.Cache.RemoveAsync(key);
            var users = await GetUsersAsync();
            var returnedUser1 = await context.Cache.FetchHashedAsync<User>(key, users[0].Id.ToString(), async () => await Task.FromResult(users[0]));
            var returnedUser2 = await context.Cache.FetchHashedAsync<User>(key, users[0].Id.ToString(), () => null);
            Assert.AreEqual(users[0].Id, returnedUser1.Id);
            Assert.AreEqual(users[0].Id, returnedUser2.Id);
        }

        [Test, TestCaseSource(typeof (Common), "All")]
        public async Task UT_CacheFetch_Nulls_Async(Context context)
        {
            string key = "UT_CacheFetch_Nulls_Async";
            context.Cache.Remove(key);
            string str = await context.Cache.FetchObjectAsync<string>(key, () => null);
            Assert.IsNull(str);
            Assert.IsFalse(await context.Cache.KeyExistsAsync(key));
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheFetchHashed_Nulls_Async(Context context)
        {
            string key = "UT_CacheFetchHashed_Nulls_Async";
            context.Cache.Remove(key);
            string str = await context.Cache.FetchHashedAsync<string>(key, "1", () => null);
            Assert.IsNull(str);
            Assert.IsFalse(context.Cache.KeyExists(key));
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheGetSetObject_Async(Context context)
        {
            // Test the GetSetObject method
            string key = "UT_CacheGetSetObject_Async";
            context.Cache.Remove(key);
            var str = await context.Cache.GetSetObjectAsync<string>(key, "1");
            Assert.IsNull(str);
            str = await context.Cache.GetSetObjectAsync<string>(key, "2");
            Assert.AreEqual("1", str);
            str = await context.Cache.GetObjectAsync<string>(key);
            Assert.AreEqual("2", str);
            context.Cache.Remove(key);
            var integer = await context.Cache.GetSetObjectAsync<int>(key, 1);
            Assert.AreEqual(0, integer);
            integer = await context.Cache.GetSetObjectAsync<int>(key, 2);
            Assert.AreEqual(1, integer);
            integer = await context.Cache.GetObjectAsync<int>(key);
            Assert.AreEqual(2, integer);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheGetHashAll_Async(Context context)
        {
            // Test the GetHashAll method
            string key = "UT_CacheGetHashAll_Async";
            bool r = context.Cache.Remove(key);
            var users = await GetUsersAsync();
            foreach (var u in users)
            {
                User user = u;
                context.Cache.SetHashed(key, user.Id.ToString(), user);
            }
            var dict = await context.Cache.GetHashedAllAsync<User>(key);
            Assert.AreEqual(users.Count, dict.Count);
            Assert.AreEqual(1, dict["1"].Id);
            Assert.AreEqual(2, dict["2"].Id);
        }

        [Test, TestCaseSource(typeof(Common), "BinAndRaw")]
        public async Task UT_CacheSetHashed_MultipleFieldsDistinctTypes_Async(Context context)
        {
            string key = "UT_CacheSetHashed_MultipleFieldsDistinctTypes_Async";
            context.Cache.Remove(key);
            var dict = new Dictionary<string, object>()
            {
                { "a", new User { Id = 222 }},
                { "2", new Department { Id = 3 }}
            };
            context.Cache.SetHashed(key, "a", dict["a"]);
            context.Cache.SetHashed(key, "2", dict["2"]);
            context.Cache.SetHashed(key, "D", new Location() { Id = 444 });

            var user = await context.Cache.GetHashedAsync<User>(key, "a");
            var dept = await context.Cache.GetHashedAsync<Department>(key, "2");
            var loc = await context.Cache.GetHashedAsync<Location>(key, "D");
            var all = await context.Cache.GetHashedAllAsync<object>(key);

            Assert.AreEqual(222, user.Id);
            Assert.AreEqual(3, dept.Id);
            Assert.AreEqual(444, loc.Id);

            Assert.AreEqual(3, all.Count);

            Assert.AreEqual(222, ((User)all["a"]).Id);
            Assert.AreEqual(3, ((Department)all["2"]).Id);
            Assert.AreEqual(444, ((Location)all["D"]).Id);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheRemove_Async(Context context)
        {
            // Test the Remove method
            string key = "UT_CacheRemove_Async";
            bool r = context.Cache.Remove(key);
            var users = await GetUsersAsync();
            await context.Cache.SetObjectAsync(key, users[0]);

            r = await context.Cache.RemoveAsync(key);
            Assert.IsTrue(r);
            Thread.Sleep(500);
            r = await context.Cache.RemoveAsync(key);
            Assert.IsFalse(r);
            var returnedUser = await context.Cache.GetObjectAsync<User>(key);
            Assert.IsNull(returnedUser);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheRemoveMultiple_Async(Context context)
        {
            string key = "UT_CacheRemoveMultiple_Async";
            for (int i = 0; i < 255; i++)
            {
                await context.Cache.SetObjectAsync(key + i, new User() { Id = i });
            }
            for (int i = 0; i < 255; i++)
            {
                Assert.IsNotNull(await context.Cache.GetObjectAsync<User>(key + i));
            }
            context.Cache.Remove(Enumerable.Range(0, 255).Select(i => key + i).ToArray());
            for (int i = 0; i < 255; i++)
            {
                Assert.IsNull(await context.Cache.GetObjectAsync<User>(key + i));
            }
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheRemoveHashed_Async(Context context)
        {
            // Test the Remove method for a complete hash set
            string key = "UT_CacheRemoveHashed_Async";
            bool r = context.Cache.Remove(key);
            var users = await GetUsersAsync();
            foreach (var u in users)
            {
                User user = u;
                context.Cache.SetHashed(key, user.Id.ToString(), user);
            }
            r = await context.Cache.RemoveHashedAsync(key, "1");
            Assert.IsTrue(r);
            Thread.Sleep(200);
            r = await context.Cache.RemoveHashedAsync(key, "1");
            Assert.IsFalse(r);

            var returnedUser1 = await context.Cache.GetHashedAsync<User>(key, 1.ToString());
            var returnedUser2 = await context.Cache.GetHashedAsync<User>(key, 2.ToString());

            Assert.IsNull(returnedUser1);
            Assert.AreEqual(2, returnedUser2.Id);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheRemove_PreviouslyHashed_Async(Context context)
        {
            // Test the Remove hashed method
            string key = "UT_CacheRemove_PreviouslyHashed_Async";
            bool r = context.Cache.Remove(key);
            var users = await GetUsersAsync();
            foreach (var u in users)
            {
                User user = u;
                context.Cache.SetHashed(key, user.Id.ToString(), user);
            }
            r = await context.Cache.RemoveAsync(key);
            Assert.IsTrue(r);
            Thread.Sleep(200);
            r = await context.Cache.RemoveAsync(key);
            Assert.IsFalse(r);

            var returnedUser1 = await context.Cache.GetHashedAsync<User>(key, 1.ToString());
            var returnedUser2 = await context.Cache.GetHashedAsync<User>(key, 2.ToString());

            Assert.IsNull(returnedUser1);
            Assert.IsNull(returnedUser2);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheAdd_Expiration_Async(Context context)
        {
            // Test the expiration of the Add method
            var users = await GetUsersAsync();
            string key = "UT_CacheAdd_Expiration_Async";
            context.Cache.Remove(key);
            await context.Cache.SetObjectAsync(key, users[0], TimeSpan.FromMilliseconds(1000));
            var user = await context.Cache.GetObjectAsync<User>(key);
            Assert.AreEqual(1, user.Id);
            Assert.AreEqual(2, user.Deparments[0].Size);
            Assert.AreEqual("one", user.Deparments[0].Location.Name);
            Thread.Sleep(1500);
            user = await context.Cache.GetObjectAsync<User>(key);
            Assert.IsNull(user);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheSetHashed_Tags_Async(Context context)
        {
            string key = "UT_CacheSetHashed_Tags_Async";
            var users = await GetUsersAsync();
            context.Cache.Remove(key);
            await context.Cache.InvalidateKeysByTagAsync("common1", "tagA1", "tagB1", "whole1");
            context.Cache.SetHashed(key, "A", users[0], new[] { "common1", "tagA1" });
            context.Cache.SetHashed(key, "B", users[0], new[] { "tagB1" });
            context.Cache.AddTagsToHashField(key, "B", new[] {"common1"});
            context.Cache.SetHashed(key, "C", users[1], new[] { "common1", "tagC" });        
            context.Cache.AddTagsToKey(key, new [] { "whole1" });
            var kwhole = await context.Cache.GetKeysByTagAsync(new [] { "whole1" });
            var kcmn = await context.Cache.GetKeysByTagAsync(new [] { "common1" });
            var ka = await context.Cache.GetKeysByTagAsync(new [] { "tagA1" });
            var kb = await context.Cache.GetKeysByTagAsync(new[] { "tagB1" });
            var kc = await context.Cache.GetKeysByTagAsync(new[] { "tagC" });
            var kab = await context.Cache.GetKeysByTagAsync(new[] { "tagA1", "tagB1" });
            Assert.AreEqual(3, kcmn.Count());
            await context.Cache.InvalidateKeysByTagAsync("tagA1");
            ka = await context.Cache.GetKeysByTagAsync(new[] { "tagA1" });
            kcmn = await context.Cache.GetKeysByTagAsync(new[] { "common1" }, true);
            Assert.IsFalse(ka.Any());
            Assert.AreEqual(2, kcmn.Count());
            var objs = context.Cache.GetObjectsByTag<User>("common1").ToList();
            Assert.AreEqual(2, objs.Count);
            context.Cache.RemoveTagsFromHashField(key, "B", new [] { "common1" });
            objs = context.Cache.GetObjectsByTag<User>("common1").ToList();
            Assert.AreEqual(1, objs.Count);
        }

        [Test, TestCaseSource(typeof (Common), "Bin")]
        public async Task UT_CacheFetch_TagsBuilder_Async(Context context)
        {
            string key = "UT_CacheFetch_TagsBuilder_Async";
            var users = await GetUsersAsync();
            var user = users[0];
            context.Cache.Remove(key);
            await context.Cache.InvalidateKeysByTagAsync("user-id-tag:" + user.Id);
            await context.Cache.FetchObjectAsync(key, async () => await Task.FromResult(user), u => new [] { "user-id-tag:" + u.Id });
            await context.Cache.FetchObjectAsync(key, async () => await Task.FromResult((User)null), u => new[] { "wrong" });
            Assert.AreEqual(0, (await context.Cache.GetKeysByTagAsync(new [] { "wrong" })).Count);
            var result = context.Cache.GetObjectsByTag<User>("user-id-tag:" + user.Id).First();
            Assert.AreEqual(0, (await context.Cache.GetKeysByTagAsync(new [] { "wrong" })).Count);
            Assert.AreEqual(user.Id, result.Id);
        }

        [Test, TestCaseSource(typeof(Common), "Bin")]
        public async Task UT_CacheFetchHashed_TagsBuilder_Async(Context context)
        {
            string key = "UT_CacheFetchHashed_TagsBuilder_Async";
            string field = "field";
            var users = await GetUsersAsync();
            var user = users[0];
            context.Cache.Remove(key);
            await context.Cache.InvalidateKeysByTagAsync("user-id-tag:" + user.Id);
            await context.Cache.FetchHashedAsync(key, field, async () => await Task.FromResult(user), u => new[] { "user-id-tag:" + u.Id });
            await context.Cache.FetchHashedAsync(key, field, async () => await Task.FromResult((User)null), u => new[] { "wrong" });
            Assert.AreEqual(0, (await context.Cache.GetKeysByTagAsync(new[] { "wrong" })).Count);
            var result = context.Cache.GetObjectsByTag<User>(new[] { "user-id-tag:" + user.Id }).First();
            Assert.AreEqual(0, (await context.Cache.GetKeysByTagAsync(new[] { "wrong" })).Count);
            Assert.AreEqual(user.Id, result.Id);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheFetchHashed_Tags_Async(Context context)
        {
            string key = "UT_CacheFetchHashed_Tags_Async";
            var users = await GetUsersAsync();
            await context.Cache.InvalidateKeysByTagAsync("common", "tag0", "tag1", "miss");
            context.Cache.Remove(key);
            var u1 = await context.Cache.FetchHashedAsync(key, users[0].Id.ToString(), async () => await Task.FromResult(users[0]), new[] { "common", "tag0"});
            var u2 = await context.Cache.FetchHashedAsync(key, users[1].Id.ToString(), async () => await Task.FromResult(users[1]), new[] { "common", "tag1" });
            var u1t = context.Cache.GetObjectsByTag<User>("tag1").ToList();
            var ust = context.Cache.GetObjectsByTag<User>("common").ToList();
            Assert.AreEqual(1, u1t.Count);
            Assert.AreEqual(2, ust.Count);
            Assert.AreEqual(users[1].Id, u1t[0].Id);
            int i = 0;
            var u = await context.Cache.FetchHashedAsync(key, users[1].Id.ToString(), async () => { i++; return await Task.FromResult(new User()); }, new[] { "miss" });
            Assert.AreEqual(0, context.Cache.GetKeysByTag(new[] {"miss"}).Count());
            Assert.AreEqual(0, i);
            Assert.AreEqual(users[1].Id, u.Id);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UtCacheSetWithTagsAsyncTask_Async(Context context)
        {
            string key = "UtCacheSetWithTagsAsyncTask_Async"; //UT_CacheSetWithTags
            context.Cache.Remove(key);
            var users = await GetUsersAsync();
            await context.Cache.SetObjectAsync(key, users[0], new[] { "user:" + users[0].Id });
            var keys = await context.Cache.GetKeysByTagAsync(new[] { "user:" + users[0].Id });
            var value = await context.Cache.GetObjectAsync<User>(keys.First());
            Assert.IsTrue(keys.Contains(key));
            Assert.IsNotNull(value);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheFetchWithTags_Async(Context context)
        {
            string key = "UT_CacheFetchWithTags_Async";
            context.Cache.Remove(key);
            string tag1 = "UT_CacheFetchWithTags_Async-Tag1";
            string tag2 = "UT_CacheFetchWithTags_Async-Tag2";
            await context.Cache.FetchObjectAsync(key, async () => await Task.FromResult("test value 1"), new[] {tag1});
            await context.Cache.FetchObjectAsync(key, async () => await Task.FromResult("should not be updated"), new[] { tag2 });
            var keys = (await context.Cache.GetKeysByTagAsync(new [] {tag1})).ToList();
            var value = (await context.Cache.GetObjectAsync<string>(keys.First())).ToList();
            Assert.IsTrue(keys.Contains(key));
            Assert.IsNotNull(value);
            keys = (await context.Cache.GetKeysByTagAsync(new [] {tag2})).ToList();
            Assert.IsFalse(keys.Contains(key));
        }


        [Test, TestCaseSource(typeof (Common), "Bin")]
        public async Task UT_CacheTagRename_Async(Context context)
        {
            string key = "UT_CacheTagRename_Async";
            context.Cache.Remove(key);
            string tag1 = "UT_CacheTagRename_Async-Tag1";
            string tag2 = "UT_CacheTagRename_Async-Tag2";
            await context.Cache.InvalidateKeysByTagAsync(tag1, tag2);
            var user = (await GetUsersAsync())[0];
            await context.Cache.SetObjectAsync(key, user, new [] { tag1 });
            Assert.AreEqual(1, context.Cache.GetKeysByTag(new [] { tag1 }).Count);
            await context.Cache.RenameTagForKeyAsync(key, tag1, tag2);
            Assert.AreEqual(0, context.Cache.GetKeysByTag(new[] { tag1 }).Count);
            Assert.AreEqual(1, context.Cache.GetKeysByTag(new[] { tag2 }).Count);
            context.Cache.RemoveTagsFromKey(key, new [] { tag2 });
            await context.Cache.RenameTagForKeyAsync(key, tag2, tag1);
            Assert.AreEqual(0, context.Cache.GetKeysByTag(new[] { tag1 }).Count);
            Assert.AreEqual(0, context.Cache.GetKeysByTag(new[] { tag2 }).Count);
        }

        [Test, TestCaseSource(typeof(Common), "Bin")]
        public async Task UT_CacheFieldTagRename_Async(Context context)
        {
            string key = "UT_CacheFieldTagRename_Async";
            string field = "field";
            await context.Cache.RemoveAsync(key);
            string tag1 = "UT_CacheFieldTagRename_Async-Tag1";
            string tag2 = "UT_CacheFieldTagRename_Async-Tag2";
            await context.Cache.InvalidateKeysByTagAsync(tag1, tag2);
            var user = (await GetUsersAsync())[0];
            context.Cache.SetHashed(key, field, user, new[] { tag1 });
            Assert.AreEqual(1, context.Cache.GetKeysByTag(new[] { tag1 }).Count);
            await context.Cache.RenameTagForHashFieldAsync(key, field, tag1, tag2);
            Assert.AreEqual(0, context.Cache.GetKeysByTag(new[] { tag1 }).Count);
            Assert.AreEqual(1, context.Cache.GetKeysByTag(new[] { tag2 }).Count);
            context.Cache.RemoveTagsFromHashField(key, field, new[] { tag2 });
            context.Cache.RemoveTagsFromHashField(key, field, new [] { tag2, tag1 });
            Assert.AreEqual(0, context.Cache.GetKeysByTag(new[] { tag1 }).Count);
            Assert.AreEqual(0, context.Cache.GetKeysByTag(new[] { tag2 }).Count);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheSetWithTags_PersistentOverridesExpiration_Async(Context context)
        {
            string key1 = "UT_CacheSetWithTags_Special1";
            string key2 = "UT_CacheSetWithTags_Special2";
            string tag = "UT_CacheSetWithTags_Special-Tag1";
            await context.Cache.InvalidateKeysByTagAsync(tag);
            await context.Cache.SetObjectAsync(key1, "test value 1", new[] { tag }, TimeSpan.FromSeconds(1));
            await context.Cache.SetObjectAsync(key2, "test value 2", new[] { tag });
            Thread.Sleep(1500);
            var keys = (await context.Cache.GetKeysByTagAsync(new[] { tag })).ToList();
            var keysCleaned = (await context.Cache.GetKeysByTagAsync(new[] { tag }, true)).ToList();
            Assert.AreEqual(2, keys.Count);
            Assert.AreEqual(1, keysCleaned.Count);
            Assert.IsTrue(keys.Contains(key1));
            Assert.IsTrue(keys.Contains(key2));
            Assert.IsTrue(keysCleaned.Contains(key2));
            await context.Cache.InvalidateKeysByTagAsync(tag);
        }


        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheSetWithTags_Expiration_Async(Context context)
        {
            string key = "UT_CacheSetWithTags_Expiration_Async";
            string key2 = "UT_CacheSetWithTags_Expiration_Async2";
            await context.Cache.RemoveAsync(key);
            await context.Cache.RemoveAsync(key2);
            var users = await GetUsersAsync();
            await context.Cache.InvalidateKeysByTagAsync("user:" + users[0].Id, "user:" + users[1].Id, "user-info");

            await context.Cache.SetObjectAsync(key, users[0], new[] { "user:" + users[0].Id, "user-info" }, TimeSpan.FromSeconds(1));
            await context.Cache.SetObjectAsync(key2, users[1], new[] { "user:" + users[1].Id, "user-info" }, TimeSpan.FromSeconds(5));
            var keys = (await context.Cache.GetKeysByTagAsync(new[] { "user:" + users[0].Id })).ToList();
            Assert.IsTrue(keys.Contains(key));
            var value = await context.Cache.GetObjectAsync<User>(keys.First());
            Assert.IsNotNull(value);
            Thread.Sleep(1200);
            var keys2 = await context.Cache.GetKeysByTagAsync(new[] { "user:" + users[0].Id });
            Assert.IsFalse(keys2.Contains(key));
            value = await context.Cache.GetObjectAsync<User>(key);
            Assert.IsNull(value);
            var keys3 = await context.Cache.GetKeysByTagAsync(new[] { "user-info" });
            Assert.IsTrue(keys3.Contains(key2));
            Thread.Sleep(4000);
            var keys4 = await context.Cache.GetKeysByTagAsync(new[] { "user-info" });
            Assert.IsFalse(keys4.Contains(key2));
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheSetWithTags_Removal_Async(Context context)
        {
            string key = "UT_CacheSetWithTags_Removal_Async";
            await context.Cache.RemoveAsync(key);
            var users = await GetUsersAsync();
            await context.Cache.InvalidateKeysByTagAsync("user:" + users[0].Id);

            await context.Cache.SetObjectAsync(key, users[0], new[] { "user:" + users[0].Id });
            var keys = await context.Cache.GetKeysByTagAsync(new[] { "user:" + users[0].Id }, true);
            Assert.IsTrue(keys.Contains(key));
            await context.Cache.RemoveAsync(key);
            var keys2 = await context.Cache.GetKeysByTagAsync(new[] { "user:" + users[0].Id }, true);
            Assert.IsFalse(keys2.Contains(key));
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheSetWithTags_Multiple_Async(Context context)
        {
            string key0 = "UT_CacheSetWithTags_Multiple_Async0";
            string key1 = "UT_CacheSetWithTags_Multiple_Async1";
            await context.Cache.RemoveAsync(key0);
            await context.Cache.RemoveAsync(key1);
            var users = await GetUsersAsync();
            await context.Cache.InvalidateKeysByTagAsync("user:" + users[0].Id, "user:" + users[1].Id, "user-info");

            await context.Cache.SetObjectAsync(key0, users[0], new[] { "user:" + users[0].Id, "user-info" });
            await context.Cache.SetObjectAsync(key1, users[1], new[] { "user:" + users[1].Id, "user-info" });
            var keys0 = await context.Cache.GetKeysByTagAsync(new[] { "user:" + users[0].Id });
            var keys1 = await context.Cache.GetKeysByTagAsync(new[] { "user:" + users[1].Id });
            var keys = (await context.Cache.GetKeysByTagAsync(new[] { "user-info" })).ToList();
            Assert.IsTrue(keys0.Contains(key0));
            Assert.IsTrue(keys1.Contains(key1));
            Assert.IsTrue(keys.Contains(key0) && keys.Contains(key1));
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheRemoveByTags_Async(Context context)
        {
            string key1 = "UT_CacheRemoveByTags_Async1";
            string key2 = "UT_CacheRemoveByTags_Async2";
            await context.Cache.RemoveAsync(key1);
            await context.Cache.RemoveAsync(key2);
            var users = await GetUsersAsync();
            string tag1 = "user:" + users[0].Id;
            string tag2 = "user:" + users[1].Id;
            await context.Cache.InvalidateKeysByTagAsync(tag1, tag2);
            await context.Cache.SetObjectAsync(key1, users[0], new[] { tag1 });
            await context.Cache.SetObjectAsync(key2, users[1], new[] { tag2 });
            await context.Cache.InvalidateKeysByTagAsync(tag1, tag2);
            var keys = await context.Cache.GetKeysByTagAsync(new [] {tag1, tag2});
            var user = await context.Cache.GetObjectAsync<User>(key1);
            Assert.IsNull(user);
            Assert.AreEqual(0, keys.Count());
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheSetHashedAll_Async(Context context)
        {
            string key = "UT_CacheSetHashedAll_Async";
            context.Cache.Remove(key);
            var users = await GetUsersAsync();
            IDictionary<string, User> allUsers = users.ToDictionary(k => k.Id.ToString());
            context.Cache.SetHashed(key, allUsers);
            var response = await context.Cache.GetHashedAllAsync<User>(key);
            Assert.AreEqual(users.Count, response.Count);
            Assert.IsTrue(users.All(x => response.ContainsKey(x.Id.ToString())));
        }

        [Test, TestCaseSource(typeof(Common), "BinAndRaw")]
        public async Task UT_CacheSerialization_Async(Context context)
        {
            string key = "UT_CacheSerialization_Async";
            context.Cache.Remove(key);
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
            await context.Cache.SetObjectAsync(key, exItem);
            var exFinal = await context.Cache.GetObjectAsync<Exception>(key);
            Assert.AreEqual(exItem.Data.Count, exFinal.Data.Count);
            Assert.AreEqual(exItem.InnerException.Message, exFinal.InnerException.Message);
            Assert.AreEqual(exItem.StackTrace, exFinal.StackTrace);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_Cache_HllAddCount_Async(Context context)
        {
            string key = "UT_Cache_HllAddCount_Async";
            context.Cache.Remove(key);
            await context.Cache.HyperLogLogAddAsync(key, new[] { 1, 2, 3, 4, 5, 6 });
            await context.Cache.HyperLogLogAddAsync(key, new[] { 4, 5, 6, 7, 8, 9 });
            await context.Cache.HyperLogLogAddAsync(key, 10);

            var cnt = await context.Cache.HyperLogLogCountAsync(key);
            Assert.AreEqual(10, cnt);
        }

        private async Task<List<User>> GetUsersAsync()
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
            return await Task.FromResult<List<User>>(new List<User>() { user1, user2 });
        }
    }
}
