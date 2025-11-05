using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Serializers;
using NUnit.Framework;
using System.Diagnostics;
using NUnit.Framework.Legacy;

namespace CachingFramework.Redis.UnitTest
{
    [TestFixture]
    public class UnitTestRedis
    {
#if NET8_0_OR_GREATER
        [Test, TestCaseSource(typeof(Common), nameof(Common.MsgPack))]
        public void UT_MessagePack_DateOnly(RedisContext ctx)
        {
            var x = new 
            {
                SomeDate = DateOnly.FromDateTime(DateTime.Now),
                TimeOnly = TimeOnly.FromDateTime(DateTime.Now.AddHours(1))
            };

            ctx.Cache.SetObject("UT_MessagePack_DateOnly_DO", x.SomeDate);
            ctx.Cache.SetObject("UT_MessagePack_DateOnly_TO", x.TimeOnly);

            var dateOnly = ctx.Cache.GetObject<DateOnly>("UT_MessagePack_DateOnly_DO");
            var timeOnly = ctx.Cache.GetObject<TimeOnly>("UT_MessagePack_DateOnly_TO");

            ClassicAssert.AreEqual(x.SomeDate, dateOnly);
            ClassicAssert.AreEqual(x.TimeOnly, timeOnly);
        }
#endif

        [Test]
        public void Test_KeyPrefix_Multiple()
        {
            var key = nameof(Test_KeyPrefix_Multiple);
            var tag = nameof(Test_KeyPrefix_Multiple) + "_TAG";
            var prefix1 = "P1";
            var prefix2 = "P2";
            using (var ctx1 = new RedisContext(Common.Config, new DatabaseOptions { KeyPrefix = prefix1 }))
            using (var ctx2 = new RedisContext(Common.Config, new DatabaseOptions { KeyPrefix = prefix2 }))
            {
                ctx1.Cache.SetObject(key, "ctx1", new[] { tag } );
                ctx2.Cache.SetObject(key, "ctx2", new[] { tag } );

                ClassicAssert.AreEqual("ctx1", ctx1.Cache.GetObject<string>(key));
                ClassicAssert.AreEqual("ctx2", ctx2.Cache.GetObject<string>(key));

                var byTag1 = ctx1.Cache.GetObjectsByTag<string>(tag).ToList();
                var byTag2 = ctx2.Cache.GetObjectsByTag<string>(tag).ToList();
                
                ClassicAssert.AreEqual(1, byTag1.Count);
                ClassicAssert.AreEqual(1, byTag2.Count);
                ClassicAssert.AreEqual("ctx1", byTag1[0]);
                ClassicAssert.AreEqual("ctx2", byTag2[0]);
            }
        }

        [Test]
        public void Test_KeyPrefix()
        {
            var key = nameof(Test_KeyPrefix);
            var prefix = "TEST-PREFIX-";
            using (var ctx = new RedisContext(Common.Config, new DatabaseOptions { KeyPrefix = prefix }))
            {
                ctx.Cache.SetObject(key, "value");
                
                var x = ctx.Cache.GetObject<string>(key);
                
                ClassicAssert.AreEqual("value", x);
            }
            using (var ctx = new RedisContext("localhost:6379"))
            {
                var y = ctx.Cache.GetObject<string>(prefix + key);

                ClassicAssert.AreEqual("value", y);
            }
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CustomTagPostfix(RedisContext ctx)
        {
            // Arrange
            var serializer = ctx.GetSerializer();
            var ori = new[] { serializer.TagPrefix, serializer.TagPostfix };
            serializer.TagPrefix = null;
            serializer.TagPostfix = "{tag}";
            
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{ctx.GetSerializer().GetType().Name}";
            var tag = $"{key}-Tag1";
            ctx.Cache.Remove(key);
            ctx.Cache.InvalidateKeysByTag(tag);
            var location = new Location() { Id = 1, Name = Guid.NewGuid().ToString() };

            // Act
            ctx.Cache.SetObject(key, location, new[] { tag });

            var locationByKey = ctx.Cache.GetObject<Location>(key);
            var locationByTag = ctx.Cache.GetObjectsByTag<Location>(tag).FirstOrDefault();
            var keyFromTag = ctx.Cache.GetKeysByTag(new[] { tag }).FirstOrDefault();
            var allTags = ctx.Cache.GetAllTags().ToList();
            var tagSetMembers = ctx.GetConnectionMultiplexer().GetDatabase().SetMembers($"{serializer.TagPrefix}{tag}{serializer.TagPostfix}");

            serializer.TagPrefix = ori[0];
            serializer.TagPostfix = ori[1];

            // Assert
            ClassicAssert.IsNotNull(locationByKey);
            ClassicAssert.IsNotNull(locationByTag);
            ClassicAssert.IsNotNull(keyFromTag);
            ClassicAssert.AreEqual(key, keyFromTag);
            ClassicAssert.AreEqual(location.Id, locationByKey.Id);
            ClassicAssert.AreEqual(location.Name, locationByKey.Name);
            ClassicAssert.AreEqual(location.Id, locationByTag.Id);
            ClassicAssert.AreEqual(location.Name, locationByTag.Name);
            ClassicAssert.IsTrue(allTags.Contains(tag));
            ClassicAssert.AreEqual(1, tagSetMembers.Length);
            ClassicAssert.AreEqual(key, tagSetMembers[0].ToString());
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CustomTagPrefixPostfix(RedisContext ctx)
        {
            // Arrange
            var serializer = ctx.GetSerializer();
            var ori = new[] { serializer.TagPrefix, serializer.TagPostfix };
            serializer.TagPrefix = "{";
            serializer.TagPostfix = "}";

            var key = $"{TestContext.CurrentContext.Test.MethodName}-{ctx.GetSerializer().GetType().Name}";
            var tag = $"{key}-Tag1";
            ctx.Cache.Remove(key);
            ctx.Cache.InvalidateKeysByTag(tag);
            var location = new Location() { Id = 1, Name = Guid.NewGuid().ToString() };

            // Act
            ctx.Cache.SetObject(key, location, new[] {tag});

            var locationByKey = ctx.Cache.GetObject<Location>(key);
            var locationByTag = ctx.Cache.GetObjectsByTag<Location>(tag).FirstOrDefault();
            var keyFromTag = ctx.Cache.GetKeysByTag(new[] { tag }).FirstOrDefault();
            var allTags = ctx.Cache.GetAllTags().ToList();
            var tagSetMembers = ctx.GetConnectionMultiplexer().GetDatabase().SetMembers($"{serializer.TagPrefix}{tag}{serializer.TagPostfix}");

            serializer.TagPrefix = ori[0];
            serializer.TagPostfix = ori[1];

            // Assert
            ClassicAssert.IsNotNull(locationByKey);
            ClassicAssert.IsNotNull(locationByTag);
            ClassicAssert.IsNotNull(keyFromTag);
            ClassicAssert.AreEqual(key, keyFromTag);
            ClassicAssert.AreEqual(location.Id, locationByKey.Id);
            ClassicAssert.AreEqual(location.Name, locationByKey.Name);
            ClassicAssert.AreEqual(location.Id, locationByTag.Id);
            ClassicAssert.AreEqual(location.Name, locationByTag.Name);
            ClassicAssert.IsTrue(allTags.Contains(tag));
            ClassicAssert.AreEqual(1, tagSetMembers.Length);
            ClassicAssert.AreEqual(key, tagSetMembers[0].ToString());
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.Json))]
        public void UT_KeyTaggedTTL(RedisContext ctx)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{ctx.GetSerializer().GetType().Name}";
            var tag = $"{key}-Tag1";
            ctx.Cache.Remove(key);
            ctx.Cache.InvalidateKeysByTag(tag);
            ctx.Cache.SetObject(key, "the value", new[] { tag }, TimeSpan.FromSeconds(1));
            ctx.Cache.KeyTimeToLive(key, new[] { tag }, TimeSpan.FromHours(24));

            Thread.Sleep(1200);

            var keys = ctx.Cache.GetKeysByTag(new[] { tag }, true);
            var value = ctx.Cache.GetObject<string>(key);
            var ttlKey = ctx.Cache.KeyTimeToLive(key);
            var tagKey = ctx.Cache.GetAllTags().FirstOrDefault(k => k.Contains(tag));
            ClassicAssert.IsNotNull(tagKey);
            var ttlTag = ctx.Cache.KeyTimeToLive(":$_tag_$:" + tagKey); 

            ClassicAssert.IsNotNull(ttlKey);
            ClassicAssert.IsTrue(ttlKey.Value.TotalHours > 23 && ttlKey.Value.TotalHours < 25);
            ClassicAssert.IsTrue(ttlTag.Value.TotalHours > 23 && ttlTag.Value.TotalHours < 25);
            
            ClassicAssert.IsTrue(keys.Contains(key));
            ClassicAssert.AreEqual("the value", value);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_MultipleAddHashedWithTags(RedisContext ctx)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{ctx.GetSerializer().GetType().Name}";
            var tags = new[] { $"{key}_TAG_1", $"{key}_TAG_2" };
            ctx.Cache.Remove(key);
            ctx.Cache.InvalidateKeysByTag(tags);

            var dict = new Dictionary<string, string>()
            {
                {"1one", "VALUE 1" },
                {"2two", "VALUE 2" }
            };

            ctx.Cache.SetHashed<string, string>(key, "3three", "VALUE 3", tags);
            ctx.Cache.SetHashed(key, dict, tags: tags);

            var ser = ctx.GetSerializer();
            var members0 = ctx.Cache.GetMembersByTag(tags[0]).OrderBy(x => ser.Deserialize<string>(x.MemberValue)).ToList();
            var members1 = ctx.Cache.GetMembersByTag(tags[1]).OrderBy(x => ser.Deserialize<string>(x.MemberValue)).ToList();

            ClassicAssert.AreEqual(members0.Count, members1.Count);
            ClassicAssert.AreEqual(3, members1.Count);
            ClassicAssert.AreEqual(key, members0[0].Key);
            ClassicAssert.AreEqual(TagMemberType.HashField, members0[1].MemberType);
            ClassicAssert.AreEqual("1one", ser.Deserialize<string>(members0[0].MemberValue));
            ClassicAssert.AreEqual("2two", ser.Deserialize<string>(members0[1].MemberValue));
            ClassicAssert.AreEqual("3three", ser.Deserialize<string>(members0[2].MemberValue));
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_HashedWithFieldTypes(RedisContext ctx)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{ctx.GetSerializer().GetType().Name}";
            var tag = $"{key}-tag";
            ctx.Cache.Remove(key);
            var users = GetUsers();
            var loc1 = new Location() { Id = 1, Name = "One" };
            var loc2 = new Location() { Id = 2, Name = "Two" };
            ctx.Cache.FetchHashed<Location, User>(key, loc1, () => users[0]);
            ctx.Cache.FetchHashed<Location, User>(key, loc1, () => new User() { Id = 99999 }); // should not affect (ignored)
            ctx.Cache.FetchHashed<Location, User>(key, loc2, () => users[1], new[] { tag });

            ctx.Cache.SetHashed<Location, User>(key, new Location() { Name = "DELETEME" }, new User() { Id = 666 });
            ctx.Cache.TryGetHashed<Location, User>(key, new Location() { Name = "DELETEME" }, out User deleted);
            bool removed = ctx.Cache.RemoveHashed(key, new Location() { Name = "DELETEME" });

            var all = ctx.Cache.GetHashedAll<Location, User>(key);

            ClassicAssert.IsNotNull(deleted);
            ClassicAssert.AreEqual(666, deleted.Id);
            ClassicAssert.IsTrue(removed);
            ClassicAssert.AreEqual(2, all.Count);
            ClassicAssert.IsTrue(all.Any(_ => _.Key.Id == 1 && _.Key.Name == "One" && _.Value.Id == users[0].Id));
            ClassicAssert.IsTrue(all.Any(_ => _.Key.Id == 2 && _.Key.Name == "Two" && _.Value.Id == users[1].Id));
        }


        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_SetGetHashedMultiple(RedisContext ctx)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{ctx.GetSerializer().GetType().Name}";
            ctx.Cache.Remove(key);
            ctx.Cache.SetHashed(key, Enumerable.Range(1, 20).ToDictionary(i => $"k{i}", i => i));
            var result = ctx.Cache.GetHashed<int>(key, "k1", "k5", "kXXX", "k10").ToList();

            ClassicAssert.AreEqual(4, result.Count);
            ClassicAssert.AreEqual(1, result[0]);
            ClassicAssert.AreEqual(5, result[1]);
            ClassicAssert.AreEqual(0, result[2]);
            ClassicAssert.AreEqual(10, result[3]);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_SetGetHashedMultiple_Generic(RedisContext ctx)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{ctx.GetSerializer().GetType().Name}";
            ctx.Cache.Remove(key);
            ctx.Cache.SetHashed<KeyValuePair<int, int>, int>(key, Enumerable.Range(1, 20).ToDictionary(i => new KeyValuePair<int, int>(1, i), i => i));
            var result = ctx.Cache.GetHashed<KeyValuePair<int, int>, int>(key, new KeyValuePair<int, int>(1, 1), new KeyValuePair<int, int>(1, 11), new KeyValuePair<int, int>(0, 0)).ToList();

            ClassicAssert.AreEqual(3, result.Count);
            ClassicAssert.AreEqual(1, result[0]);
            ClassicAssert.AreEqual(11, result[1]);
            ClassicAssert.AreEqual(0, result[2]);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.Json))]
        public void UT_DefaultSerializer(RedisContext context)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            var prev = RedisContext.DefaultSerializer;

            RedisContext.DefaultSerializer = new RawSerializer().SetSerializerFor<string>(s => new byte[] { 1, 2, 3 }, b => "123");

            var ctx = new RedisContext(Common.Config);
            ctx.Cache.Remove(key);
            ctx.Cache.SetObject<string>("xxx", "value");

            var value = ctx.Cache.GetObject<string>("xxx");
            ClassicAssert.AreEqual("123", value);

#if (NET462)
            ClassicAssert.IsTrue(prev is BinarySerializer);
#else
            ClassicAssert.IsTrue(prev is JsonSerializer);
#endif

            ctx.Cache.Remove(key);
            RedisContext.DefaultSerializer = prev;
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_Cache_MembersByTag(RedisContext context)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            var keyHash = $"{key}_HASH";
            var keySet = $"{key}_SET";
            var keySortedset = $"{key}_SSET";
            var tag1 = $"{key}_TAG1";
            var tag2 = $"{key}_TAG2";
            context.Cache.Remove(new[] { key, keyHash, keySet, keySortedset });
            context.Cache.InvalidateKeysByTag(tag1, tag2);

            context.Cache.SetObject(key, "test", new[] { tag1 });
            // duplicated call with same tag and key
            context.Cache.SetObject(key, "test2", new[] { tag1 });

            var hash = context.Collections.GetRedisDictionary<string, string>(keyHash);
            hash.Add("hx", "one", new[] { tag1 });
            hash.Add("hy", "two", new[] { tag1, tag2 });
            hash.Add("hz", "three", new[] { tag2 });

            var set = context.Collections.GetRedisSet<string>(keySet);
            set.Add("sx", new[] { tag1 });
            set.Add("sy", new[] { tag1, tag2 });
            set.Add("sz", new[] { tag2 });

            var sortedSet = context.Collections.GetRedisSortedSet<string>(keySortedset);
            sortedSet.Add(1, "ssx", new[] { tag1 });
            sortedSet.Add(2, "ssy", new[] { tag1, tag2 });
            sortedSet.Add(3, "ssz", new[] { tag2 });

            var t1Members = context.Cache.GetMembersByTag(tag1).ToList();
            var t2Members = context.Cache.GetMembersByTag(tag2).ToList();
            var noMembers = context.Cache.GetMembersByTag("does not exists").ToList();

            ClassicAssert.AreEqual(0, noMembers.Count);
            ClassicAssert.AreEqual(7, t1Members.Count);
            ClassicAssert.AreEqual(6, t2Members.Count);

            ClassicAssert.IsTrue(t1Members.Any(x => x.MemberType == TagMemberType.StringKey && x.Key == key && x.MemberValue == null));
            ClassicAssert.IsTrue(t1Members.Any(x => x.MemberType == TagMemberType.HashField && x.Key == keyHash && x.GetMemberAs<string>() == "hx" ));
            ClassicAssert.IsTrue(t1Members.Any(x => x.MemberType == TagMemberType.HashField && x.Key == keyHash && x.GetMemberAs<string>() == "hy"));

            ClassicAssert.IsTrue(t1Members.Any(x => x.MemberType == TagMemberType.SetMember && x.Key == keySet && x.GetMemberAs<string>() == "sx"));
            ClassicAssert.IsTrue(t1Members.Any(x => x.MemberType == TagMemberType.SetMember && x.Key == keySet && x.GetMemberAs<string>() == "sy"));

            ClassicAssert.IsTrue(t1Members.Any(x => x.MemberType == TagMemberType.SortedSetMember && x.Key == keySortedset && x.GetMemberAs<string>() == "ssx"));
            ClassicAssert.IsTrue(t1Members.Any(x => x.MemberType == TagMemberType.SortedSetMember && x.Key == keySortedset && x.GetMemberAs<string>() == "ssy"));

            ClassicAssert.IsTrue(t2Members.Any(x => x.MemberType == TagMemberType.HashField && x.Key == keyHash && x.GetMemberAs<string>() == "hy"));
            ClassicAssert.IsTrue(t2Members.Any(x => x.MemberType == TagMemberType.HashField && x.Key == keyHash && x.GetMemberAs<string>() == "hz"));

            ClassicAssert.IsTrue(t2Members.Any(x => x.MemberType == TagMemberType.SetMember && x.Key == keySet && x.GetMemberAs<string>() == "sy"));
            ClassicAssert.IsTrue(t2Members.Any(x => x.MemberType == TagMemberType.SetMember && x.Key == keySet && x.GetMemberAs<string>() == "sz"));

            ClassicAssert.IsTrue(t2Members.Any(x => x.MemberType == TagMemberType.SortedSetMember && x.Key == keySortedset && x.GetMemberAs<string>() == "ssy"));
            ClassicAssert.IsTrue(t2Members.Any(x => x.MemberType == TagMemberType.SortedSetMember && x.Key == keySortedset && x.GetMemberAs<string>() == "ssz"));

            context.Cache.InvalidateKeysByTag(tag1, tag2);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_Cache_IsOnTagMethods(RedisContext context)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            var keyHash = $"{key}_HASH";
            var keySet = $"{key}_SET";
            var keySortedset = $"{key}_SSET";
            var tag1 = $"{key}_TAG1";
            var tag2 = $"{key}_TAG2";
            context.Cache.Remove(new [] { key, keyHash, keySet, keySortedset });
            context.Cache.InvalidateKeysByTag(tag1, tag2);

            context.Cache.SetObject(key, "test", new [] { tag1 });

            var hash = context.Collections.GetRedisDictionary<string, string>(keyHash);
            hash.Add("hx", "one", new[] {tag1});
            hash.Add("hy", "two", new[] {tag1, tag2});
            hash.Add("hz", "three", new[] { tag2 });

            var set = context.Collections.GetRedisSet<string>(keySet);
            set.Add("sx", new[] {tag1});
            set.Add("sy", new[] {tag1, tag2});
            set.Add("sz", new[] {tag2 });

            var sortedSet = context.Collections.GetRedisSortedSet<string>(keySortedset);
            sortedSet.Add(1, "ssx", new[] {tag1});
            sortedSet.Add(2, "ssy", new[] {tag1, tag2});
            sortedSet.Add(3, "ssz", new[] {tag2});

            ClassicAssert.AreEqual(true, context.Cache.IsStringKeyInTag(key, tag1));
            ClassicAssert.AreEqual(false, context.Cache.IsStringKeyInTag(key, tag2));
            ClassicAssert.AreEqual(true, context.Cache.IsStringKeyInTag(key, "xyyxx", tag1));
            ClassicAssert.AreEqual(false, context.Cache.IsStringKeyInTag("does not exists", tag1));

            ClassicAssert.AreEqual(true, context.Cache.IsHashFieldInTag(keyHash, "hx", tag1));
            ClassicAssert.AreEqual(false, context.Cache.IsHashFieldInTag(keyHash, "hx", tag2));
            ClassicAssert.AreEqual(true, context.Cache.IsHashFieldInTag(keyHash, "hy", tag1, tag2));
            ClassicAssert.AreEqual(true, context.Cache.IsHashFieldInTag(keyHash, "hz", tag1, tag2));
            ClassicAssert.AreEqual(false, context.Cache.IsHashFieldInTag(keyHash, "does not exists", tag1, tag2));

            ClassicAssert.AreEqual(true, context.Cache.IsSetMemberInTag(keySet, "sx", tag1));
            ClassicAssert.AreEqual(false, context.Cache.IsSetMemberInTag(keySet, "sx", tag2));
            ClassicAssert.AreEqual(true, context.Cache.IsSetMemberInTag(keySet, "sy", tag1));
            ClassicAssert.AreEqual(true, context.Cache.IsSetMemberInTag(keySet, "sy", tag2));
            ClassicAssert.AreEqual(false, context.Cache.IsSetMemberInTag(keySet, "sz", tag1));
            ClassicAssert.AreEqual(true, context.Cache.IsSetMemberInTag(keySet, "sz", tag2));

            ClassicAssert.AreEqual(true, context.Cache.IsSetMemberInTag(keySortedset, "ssx", tag1));
            ClassicAssert.AreEqual(false, context.Cache.IsSetMemberInTag(keySortedset, "ssx", tag2));
            ClassicAssert.AreEqual(true, context.Cache.IsSetMemberInTag(keySortedset, "ssy", tag1));
            ClassicAssert.AreEqual(true, context.Cache.IsSetMemberInTag(keySortedset, "ssy", tag2));
            ClassicAssert.AreEqual(false, context.Cache.IsSetMemberInTag(keySortedset, "ssz", tag1));
            ClassicAssert.AreEqual(true, context.Cache.IsSetMemberInTag(keySortedset, "ssz", tag2));

            context.Cache.InvalidateKeysByTag(tag1, tag2);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_Cache_SetHashed_TK_TV(RedisContext context)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            context.Cache.Remove(key);
            var users = GetUsers();

            context.Cache.SetHashed<User, User>(key, users[0], users[1]);
            context.Cache.SetHashed<User, User>(key, users[1], users[0]);

            var u1 = context.Cache.GetHashed<User, User>(key, users[0]);
            var u0 = context.Cache.GetHashed<User, User>(key, users[1]);

            ClassicAssert.AreEqual(users[0].Id, u0.Id);
            ClassicAssert.AreEqual(users[1].Id, u1.Id);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_Cache_SetHashed_TK_TV_WithTags(RedisContext context)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            context.Cache.Remove(key);
            context.Cache.InvalidateKeysByTag("tag 0->1", "tag 1->0", "tag S->0", "common");
            var users = GetUsers();
            var dict = context.Collections.GetRedisDictionary<User, User>(key, 4);

            context.Cache.SetHashed<User, User>(key, users[0], users[1], new[] { "tag 0->1", "common" });
            dict.Add(users[1], users[0], new[] { "tag 1->0", "common" });
            context.Cache.SetHashed<User>(key, "string field", users[0], new[] { "tag S->0", "common" });

            var u1 = context.Cache.GetHashed<User, User>(key, users[0]);
            var u0 = context.Cache.GetHashed<User, User>(key, users[1]);

            var all = context.Cache.GetObjectsByTag<User>("common").ToList();
            var t01 = context.Cache.GetObjectsByTag<User>("tag 0->1").ToList();
            var t10 = context.Cache.GetObjectsByTag<User>("tag 1->0").ToList();
            var tS0 = context.Cache.GetObjectsByTag<User>("tag S->0").ToList();

            ClassicAssert.AreEqual(users[0].Id, u0.Id);
            ClassicAssert.AreEqual(users[1].Id, u1.Id);

            ClassicAssert.AreEqual(3, all.Count);
            ClassicAssert.AreEqual(users[1].Id, t01[0].Id);
            ClassicAssert.AreEqual(users[0].Id, t10[0].Id);
            ClassicAssert.AreEqual(users[0].Id, tS0[0].Id);

            ClassicAssert.AreEqual(users[1].Id, dict[users[0]].Id);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_Cache_Hash_Scan(RedisContext context)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            int total = 10000;
            context.Cache.Remove(key);
            var fields = Enumerable.Range(1, total)
                .Select(i => new KeyValuePair<string, string>(i.ToString(), Guid.NewGuid().ToString()+ Guid.NewGuid().ToString()+ Guid.NewGuid().ToString()+ Guid.NewGuid().ToString()))
                .ToDictionary(k => k.Key, v => v.Value);
            context.Cache.SetHashed(key, fields);

            var dict = context.Collections.GetRedisDictionary<string, string>(key);
            ClassicAssert.AreEqual(total, dict.Count);

            var stp = Stopwatch.StartNew();
            var all = context.Cache.GetHashedAll<string>(key).Take(5).ToList();
            var allTime = stp.Elapsed.TotalMilliseconds;

            stp = Stopwatch.StartNew();
            var some = context.Cache.ScanHashed<string>(key, "*", 5).Take(5).ToList();
            var someTime = stp.Elapsed.TotalMilliseconds;


            var c1 = context.Cache.ScanHashed<string>(key, "", 20).Count();
            var c2 = context.Cache.ScanHashed<string>(key, null).Count();


            // Assert the HSCAN lasted at most the 0.5 times of the time of the HGETALL
            ClassicAssert.IsTrue((someTime / (double)allTime) < 0.5);
            ClassicAssert.AreEqual(total, c1);
            ClassicAssert.AreEqual(total, c2);
            context.Cache.Remove(key);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.Raw))]
        public void UT_Context_Dispose(RedisContext context)
        {
            var ctx = new RedisContext(context.GetConnectionMultiplexer().Configuration, context.GetSerializer());
            ctx.Cache.SetObject("key", "value");
            ctx.Dispose();
            context.Cache.Remove("key");
            Assert.Throws<ObjectDisposedException>(() => ctx.Cache.SetObject("key", "value2"));
        }

        [Test, TestCaseSource(typeof (Common), nameof(Common.Raw))]
        public void UT_CacheNull(RedisContext context)
        {
            Assert.Throws<ArgumentException>(() => context.Cache.SetObject(null, "this should fail"));
        }

        [Test, TestCaseSource(typeof (Common), nameof(Common.Raw))]
        public void UT_CacheSet_When(RedisContext context)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            context.Cache.Remove(key);
            context.Cache.SetObject(key, "value", null, When.Exists);
            ClassicAssert.IsNull(context.Cache.GetObject<string>(key));
            context.Cache.SetObject(key, "value", null, When.NotExists);
            ClassicAssert.AreEqual("value", context.Cache.GetObject<string>(key));
            context.Cache.SetObject(key, "new", null, When.NotExists);
            ClassicAssert.AreEqual("value", context.Cache.GetObject<string>(key));
            context.Cache.SetObject(key, "new", null, When.Exists);
            ClassicAssert.AreEqual("new", context.Cache.GetObject<string>(key));
            context.Cache.Remove(key);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.Raw))]
        public void UT_CacheSetHashed_When(RedisContext context)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            var field = "F1";
            context.Cache.Remove(key);
            context.Cache.SetHashed(key, field, "value", null, When.NotExists);
            ClassicAssert.AreEqual("value", context.Cache.GetHashed<string>(key, field));
            context.Cache.SetHashed(key, field, "new", null, When.NotExists);
            ClassicAssert.AreEqual("value", context.Cache.GetHashed<string>(key, field));
            context.Cache.SetHashed(key, field, "new", null, When.Always);
            ClassicAssert.AreEqual("new", context.Cache.GetHashed<string>(key, field));
            context.Cache.Remove(key);
        }

        [Test, TestCaseSource(typeof (Common), nameof(Common.Raw))]
        public void UT_CacheHackTag(RedisContext context)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            context.Cache.InvalidateKeysByTag("tag1");
            context.Cache.SetObject(key, "some value", new [] {"tag1"});
            var keys = context.Cache.GetKeysByTag(new [] { "tag1" }).ToList();
            ClassicAssert.AreEqual(1, keys.Count);
            ClassicAssert.AreEqual(key, keys[0]);
            var tagset = context.Collections.GetRedisSet<string>(":$_tag_$:tag1");
            tagset.Add("FakeKey:$_->_$:FakeValue");
            var knc = context.Cache.GetKeysByTag(new [] { "tag1" }).ToList();
            var k = context.Cache.GetKeysByTag(new [] { "tag1" }, true).ToList();
            var v = context.Cache.GetObjectsByTag<string>("tag1").ToList();
            ClassicAssert.AreEqual(2, knc.Count);
            ClassicAssert.AreEqual(1, k.Count);
            ClassicAssert.AreEqual(1, v.Count);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.BinAndRawAndJson))]
        public void UT_CacheSerializer(RedisContext context)
        {
            System.Diagnostics.Debug.WriteLine($"Using the {context.GetSerializer().GetType().Name} as serializer");

            var kss = "short:string:sync";
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
            // TODO: System.Text.JSon cannot serialize this.
            if (context.GetSerializer().GetType() != typeof(JsonSerializer))
            {
                context.Cache.SetObject<UIntPtr>(kuip, UIntPtr.Zero);
            }
            context.Cache.SetObject<Double>(kdbl, Double.NegativeInfinity);
            context.Cache.SetObject<bool>(kpBool, true);
            context.Cache.SetObject<int>(kpInt, int.MaxValue);
            context.Cache.SetObject<Int64>(kpLong, Int64.MaxValue);
            var now = DateTime.Now;
            context.Cache.SetObject<DateTime>(kdt, now);
            context.Cache.SetObject<Single>(kpSingle, Single.MaxValue);
            // TODO: same as above. not supported in System.Text.Json
            if (context.GetSerializer().GetType() != typeof(JsonSerializer))
            {
                context.Cache.SetObject<IntPtr>(kpIntPtr, new IntPtr(int.MaxValue));
            }
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

            ClassicAssert.AreEqual(ss, ss_);
            ClassicAssert.AreEqual(ls, ls_);
            ClassicAssert.AreEqual('c', kch_);
            ClassicAssert.IsTrue((now - kdt_).TotalMilliseconds < 0.001);
            ClassicAssert.AreEqual(decimal.MaxValue, kds_);
            ClassicAssert.AreEqual(Byte.MaxValue, kby_);
            ClassicAssert.AreEqual(SByte.MaxValue, ksby_);
            ClassicAssert.AreEqual(Int16.MaxValue, ki16_);
            ClassicAssert.AreEqual(Int32.MaxValue, ki32_);
            ClassicAssert.AreEqual(UIntPtr.Zero, kuip_);
            ClassicAssert.AreEqual(Double.NegativeInfinity, kdbl_);
            ClassicAssert.AreEqual(int.MaxValue, pInt_);
            ClassicAssert.AreEqual(long.MaxValue, pLong_);
            ClassicAssert.AreEqual(Single.Parse(Single.MaxValue.ToString("F")), Single.Parse(pSingle_.ToString("F")));
            ClassicAssert.AreEqual(UInt16.MaxValue, pUint16_);
            ClassicAssert.AreEqual(UInt32.MaxValue, pUint32_);
            if (context.GetSerializer().GetType() != typeof(JsonSerializer))
            {
                ClassicAssert.AreEqual(UInt64.MaxValue, pUint64_);
            }
            ClassicAssert.AreEqual(true, kpBool_);
            context.Cache.Remove(new[] { kss, kls, kpBool, kpInt, kpLong, kpSingle, kpIntPtr, kpUInt16, kpUInt32, kpUInt64, 
                kch, kds, kdt, kby, ksby, ki16, ki32, kuip, kdbl });
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_Cache_GetAllTags(RedisContext context)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            context.Cache.SetHashed(key, "1", "some value", new[] {"tag1", "tag2"});
            var tags = context.Cache.GetAllTags();
            ClassicAssert.IsTrue(tags.Contains("tag1"));
            ClassicAssert.IsTrue(tags.Contains("tag2"));
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_Cache_GetKeysByPattern(RedisContext context)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            var key2 = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}-2";
            context.Cache.FlushAll();
            context.Cache.SetObject(key, "some value", new[] { "tag3", "tag4" });
            context.Cache.SetObject(key2, "some value2");
            var keyPrefix = context.GetDatabaseOptions().KeyPrefix;
            var keys = context.Cache.GetKeysByPattern("*");
            ClassicAssert.AreEqual(2, keys.Count());
            ClassicAssert.IsTrue(keys.Contains(keyPrefix + key));
            ClassicAssert.IsTrue(keys.Contains(keyPrefix + key2));
        }

        [Test]
        public void UT_Cache_RawOverrideSerializer()
        {
            var raw = new RawSerializer();
            raw.SetSerializerFor<User>(u => Encoding.UTF8.GetBytes(u.Id.ToString()),
                b => new User() {Id = int.Parse(Encoding.UTF8.GetString(b))});
            var ctx = new RedisContext(Common.Config, raw);
            Thread.Sleep(1000);
            var users = GetUsers();
            var key = $"{TestContext.CurrentContext.Test.MethodName}";
            var key2 = $"{TestContext.CurrentContext.Test.MethodName}-2";
            ctx.Cache.Remove(new[] {key, key2});
            ctx.Cache.SetObject(key, users[0]);
            ctx.Cache.SetHashed(key2, "X", users[1]);
            var v = ctx.Cache.GetObject<User>(key);
            var v2 = ctx.Cache.GetHashed<User>(key2, "X");
            var v3 = ctx.Cache.GetObject<int>(key);
            ClassicAssert.AreEqual(users[0].Id, v.Id);
            ClassicAssert.AreEqual(users[1].Id, v2.Id);
            ClassicAssert.AreEqual(users[0].Id, v3);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CacheByteArray(RedisContext context)
        {
            context.Cache.SetObject("key", "jpeg");
            var o = context.Cache.GetObject<string>("key");
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            Jpeg jpeg = new Jpeg()
            {
                Data = Enumerable.Range(0, 200000)
                       .Select(i => (byte)((i * 223) % 256)).ToArray()
            };
            context.Cache.Remove(key);
            context.Cache.SetObject(key, jpeg);
            var jpeg2 = context.Cache.GetObject<Jpeg>(key);
            ClassicAssert.IsTrue(Enumerable.SequenceEqual(jpeg.Data, jpeg2.Data));
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CacheAddGet(RedisContext context)
        {
            // Test the Add and Get methods
            var users = GetUsers();
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            context.Cache.Remove(key);
            context.Cache.SetObject(key, users[1]);
            context.Cache.SetObject(key, users[0], new string[]{});
            var user = context.Cache.GetObject<User>(key);
            ClassicAssert.AreEqual(1, user.Id);
            ClassicAssert.AreEqual(2, user.Deparments[0].Size);
            ClassicAssert.AreEqual("one", user.Deparments[0].Location.Name);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CacheFetch(RedisContext context)
        {
            // Test the Fetch method
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            int count = 0;
            context.Cache.Remove(key);
            var a = context.Cache.FetchObject(key, () => { count++; return GetUsers(); });
            var b = context.Cache.FetchObject(key, () => { count++; return GetUsers(); });
            context.Cache.FetchObject(key, () => { count++; return GetUsers(); });
            ClassicAssert.AreEqual(1, count);
            ClassicAssert.AreEqual(a[0].Id, b[0].Id);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CacheFetch_TTL(RedisContext context)
        {
            // Test the Fetch method
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            int count = 0;
            context.Cache.Remove(key);
            context.Cache.FetchObject(key, () => { count++; return GetUsers(); }, TimeSpan.FromSeconds(2));
            context.Cache.FetchObject(key, () => { count++; return GetUsers(); });
            ClassicAssert.AreEqual(1, count);
            Thread.Sleep(2200);
            context.Cache.FetchObject(key, () => { count++; return GetUsers(); });
            ClassicAssert.AreEqual(2, count);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CacheFetchHashed(RedisContext context)
        {
            // Test the FetchHashed method
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            bool r = context.Cache.Remove(key);
            var users = GetUsers();
            var returnedUser1 = context.Cache.FetchHashed<User>(key, users[0].Id.ToString(), () => users[0]);
            var returnedUser2 = context.Cache.FetchHashed<User>(key, users[0].Id.ToString(), () => null);
            ClassicAssert.AreEqual(users[0].Id, returnedUser1.Id);
            ClassicAssert.AreEqual(users[0].Id, returnedUser2.Id);
        }

        [Test, TestCaseSource(typeof (Common), nameof(Common.All))]
        public void UT_CacheFetch_Nulls(RedisContext context)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            context.Cache.Remove(key);
            string str = context.Cache.FetchObject<string>(key, () => null);
            ClassicAssert.IsNull(str);
            ClassicAssert.IsFalse(context.Cache.KeyExists(key));
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CacheFetchHashed_Nulls(RedisContext context)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            context.Cache.Remove(key);
            string str = context.Cache.FetchHashed<string>(key, "1", () => null);
            ClassicAssert.IsNull(str);
            ClassicAssert.IsFalse(context.Cache.KeyExists(key));
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CacheTryGetObject(RedisContext context)
        {
            // Test the TryGetObject method
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            context.Cache.Remove(key);
            var users = GetUsers();
            User u1;
            context.Cache.SetObject(key, users[0]);
            bool b = context.Cache.TryGetObject(key + "x7rz9a", out u1);
            ClassicAssert.IsFalse(b);
            ClassicAssert.IsNull(u1);
            b = context.Cache.TryGetObject(key, out u1);
            ClassicAssert.IsTrue(b);
            ClassicAssert.IsNotNull(u1);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CacheTryGetHashed(RedisContext context)
        {
            // Test the TryGetHashed method
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            context.Cache.Remove(key);
            var users = GetUsers();
            User u1;
            context.Cache.SetHashed(key, users[0].Id.ToString(), users[0]);
            bool b = context.Cache.TryGetHashed(key, "a", out u1);
            ClassicAssert.IsFalse(b);
            ClassicAssert.IsNull(u1);
            b = context.Cache.TryGetHashed(key, users[0].Id.ToString(), out u1);
            ClassicAssert.IsTrue(b);
            ClassicAssert.IsNotNull(u1);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CacheGetSetObject(RedisContext context)
        {
            // Test the GetSetObject method
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            context.Cache.Remove(key);
            var str = context.Cache.GetSetObject<string>(key, "1");
            ClassicAssert.IsNull(str);
            str = context.Cache.GetSetObject<string>(key, "2");
            ClassicAssert.AreEqual("1", str);
            str = context.Cache.GetObject<string>(key);
            ClassicAssert.AreEqual("2", str);
            context.Cache.Remove(key);
            var integer = context.Cache.GetSetObject<int>(key, 1);
            ClassicAssert.AreEqual(0, integer);
            integer = context.Cache.GetSetObject<int>(key, 2);
            ClassicAssert.AreEqual(1, integer);
            integer = context.Cache.GetObject<int>(key);
            ClassicAssert.AreEqual(2, integer);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CacheGetHashAll(RedisContext context)
        {
            // Test the GetHashAll method
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            bool r = context.Cache.Remove(key);
            var users = GetUsers();
            foreach (var u in users)
            {
                User user = u;
                context.Cache.SetHashed(key, user.Id.ToString(), user);
            }
            var dict = context.Cache.GetHashedAll<User>(key);
            ClassicAssert.AreEqual(users.Count, dict.Count);
            ClassicAssert.AreEqual(1, dict["1"].Id);
            ClassicAssert.AreEqual(2, dict["2"].Id);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CacheRemove(RedisContext context)
        {
            // Test the Remove method
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            bool r = context.Cache.Remove(key);
            var users = GetUsers();
            context.Cache.SetObject(key, users[0]);

            r = context.Cache.Remove(key);
            ClassicAssert.IsTrue(r);
            Thread.Sleep(500);
            r = context.Cache.Remove(key);
            ClassicAssert.IsFalse(r);
            var returnedUser = context.Cache.GetObject<User>(key);
            ClassicAssert.IsNull(returnedUser);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CacheRemoveMultiple(RedisContext context)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            for (int i = 0; i < 255; i++)
            {
                context.Cache.SetObject(key + i, new User() { Id = i });
            }
            for (int i = 0; i < 255; i++)
            {
                ClassicAssert.IsNotNull(context.Cache.GetObject<User>(key + i));
            }
            context.Cache.Remove(Enumerable.Range(0, 255).Select(i => key + i).ToArray());
            for (int i = 0; i < 255; i++)
            {
                ClassicAssert.IsNull(context.Cache.GetObject<User>(key + i));
            }
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CacheRemoveHashed(RedisContext context)
        {
            // Test the Remove method for a complete hash set
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            bool r = context.Cache.Remove(key);
            var users = GetUsers();
            foreach (var u in users)
            {
                User user = u;
                context.Cache.SetHashed(key, user.Id.ToString(), user);
            }
            r = context.Cache.RemoveHashed(key, "1");
            ClassicAssert.IsTrue(r);
            Thread.Sleep(200);
            r = context.Cache.RemoveHashed(key, "1");
            ClassicAssert.IsFalse(r);

            var returnedUser1 = context.Cache.GetHashed<User>(key, 1.ToString());
            var returnedUser2 = context.Cache.GetHashed<User>(key, 2.ToString());

            ClassicAssert.IsNull(returnedUser1);
            ClassicAssert.AreEqual(2, returnedUser2.Id);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CacheRemove_PreviouslyHashed(RedisContext context)
        {
            // Test the Remove hashed method
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            bool r = context.Cache.Remove(key);
            var users = GetUsers();
            foreach (var u in users)
            {
                User user = u;
                context.Cache.SetHashed(key, user.Id.ToString(), user);
            }
            r = context.Cache.Remove(key);
            ClassicAssert.IsTrue(r);
            Thread.Sleep(200);
            r = context.Cache.Remove(key);
            ClassicAssert.IsFalse(r);

            var returnedUser1 = context.Cache.GetHashed<User>(key, 1.ToString());
            var returnedUser2 = context.Cache.GetHashed<User>(key, 2.ToString());

            ClassicAssert.IsNull(returnedUser1);
            ClassicAssert.IsNull(returnedUser2);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CacheAdd_Expiration(RedisContext context)
        {
            // Test the expiration of the Add method
            var users = GetUsers();
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            context.Cache.Remove(key);
            context.Cache.SetObject(key, users[0], TimeSpan.FromMilliseconds(1000));
            var user = context.Cache.GetObject<User>(key);
            ClassicAssert.AreEqual(1, user.Id);
            ClassicAssert.AreEqual(2, user.Deparments[0].Size);
            ClassicAssert.AreEqual("one", user.Deparments[0].Location.Name);
            Thread.Sleep(1500);
            user = context.Cache.GetObject<User>(key);
            ClassicAssert.IsNull(user);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CacheAddHashed_Expiration_1(RedisContext context)
        {
            // Test the expiration of the AddHashed method (MAX ttl applies)
            var users = GetUsers();
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            context.Cache.Remove(key);

            context.Cache.SetHashed(key, "2", users[1], TimeSpan.FromMilliseconds(10000));
            context.Cache.SetHashed(key, "1", users[0], TimeSpan.FromMilliseconds(1000));
            Thread.Sleep(1200);

            var user1 = context.Cache.GetHashed<User>(key, "1");
            var user2 = context.Cache.GetHashed<User>(key, "2");
            ClassicAssert.IsNotNull(user1);
            ClassicAssert.IsNotNull(user2);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CacheAddHashed_Expiration_2(RedisContext context)
        {
            // Test the expiration of the Fetch method (last larger expiration applies)
            var users = GetUsers();
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            context.Cache.Remove(key);

            context.Cache.SetHashed(key, "1", users[0], TimeSpan.FromMilliseconds(1000));
            context.Cache.SetHashed(key, "2", users[1], TimeSpan.FromMilliseconds(10000));
            Thread.Sleep(1200);

            var user1 = context.Cache.GetHashed<User>(key, "1");
            var user2 = context.Cache.GetHashed<User>(key, "2");
            ClassicAssert.IsNotNull(user1);
            ClassicAssert.IsNotNull(user2);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CacheAddHashed_Expiration_3(RedisContext context)
        {
            // Test the expiration of the Fetch method (last no-expiration applies)
            var users = GetUsers();
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            context.Cache.Remove(key);
            var ms = 1000;
            context.Cache.SetHashed(key, "1", users[0], TimeSpan.FromMilliseconds(ms));
            context.Cache.SetHashed(key, "2", users[1], TimeSpan.MaxValue);
            Thread.Sleep(ms + 200);

            var user1 = context.Cache.GetHashed<User>(key, "1");
            var user2 = context.Cache.GetHashed<User>(key, "2");
            ClassicAssert.IsNotNull(user1);
            ClassicAssert.IsNotNull(user2);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.Json))]
        public void UT_CacheSetHashed_KeyTimeToLive(RedisContext context)
        {
            // Test the expiration of the Fetch method (last no-expiration applies)
            var users = GetUsers();
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            context.Cache.Remove(key);
            var ms = 10000;
            context.Cache.SetHashed(key, "1", users[0], TimeSpan.FromMilliseconds(ms));
            var ttl = context.Cache.KeyTimeToLive(key);

            ClassicAssert.IsTrue(ttl.Value.Seconds >= 8);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CacheSetHashed_Tags(RedisContext context)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
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
            ClassicAssert.AreEqual(3, kcmn.Count());
            context.Cache.InvalidateKeysByTag("tagA");
            ka = context.Cache.GetKeysByTag(new[] { "tagA" });
            kcmn = context.Cache.GetKeysByTag(new[] { "common" }, true);
            ClassicAssert.IsFalse(ka.Any());
            ClassicAssert.AreEqual(2, kcmn.Count());
            var objs = context.Cache.GetObjectsByTag<User>("common").ToList();
            ClassicAssert.AreEqual(2, objs.Count);
            context.Cache.RemoveTagsFromHashField(key, "B", new [] { "common" });
            objs = context.Cache.GetObjectsByTag<User>("common").ToList();
            ClassicAssert.AreEqual(1, objs.Count);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CacheFetchHashed_Tags(RedisContext context)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            var users = GetUsers();
            context.Cache.InvalidateKeysByTag("common", "tag0", "tag1", "miss");
            context.Cache.Remove(key);
            var u1 = context.Cache.FetchHashed(key, users[0].Id.ToString(), () => users[0], new[] { "common", "tag0"});
            var u2 = context.Cache.FetchHashed(key, users[1].Id.ToString(), () => users[1], new[] { "common", "tag1" });
            var u1t = context.Cache.GetObjectsByTag<User>("tag1").ToList();
            var ust = context.Cache.GetObjectsByTag<User>("common").ToList();
            ClassicAssert.AreEqual(1, u1t.Count);
            ClassicAssert.AreEqual(2, ust.Count);
            ClassicAssert.AreEqual(users[1].Id, u1t[0].Id);
            int i = 0;
            var u = context.Cache.FetchHashed(key, users[1].Id.ToString(), () => { i++; return new User(); }, new[] { "miss" });
            ClassicAssert.AreEqual(0, context.Cache.GetKeysByTag(new[] {"miss"}).Count());
            ClassicAssert.AreEqual(0, i);
            ClassicAssert.AreEqual(users[1].Id, u.Id);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CacheSetWithTags_Default(RedisContext context)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            context.Cache.Remove(key);
            var users = GetUsers();
            context.Cache.SetObject(key, users[0], new[] { $"{key}-user:" + users[0].Id });
            var keys = context.Cache.GetKeysByTag(new[] { $"{key}-user:" + users[0].Id });
            var value = context.Cache.GetObject<User>(keys.First());
            ClassicAssert.IsTrue(keys.Contains(key));
            ClassicAssert.IsNotNull(value);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CacheFetchWithTags(RedisContext context)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            context.Cache.Remove(key);
            string tag1 = $"{key}-Tag1";
            string tag2 = $"{key}-Tag2";
            context.Cache.FetchObject(key, () => "test value 1", new[] {tag1});
            context.Cache.FetchObject(key, () => "should not be updated", new[] { tag2 });
            var keys = context.Cache.GetKeysByTag(new [] {tag1}).ToList();
            var value = context.Cache.GetObject<string>(keys.First()).ToList();
            ClassicAssert.IsTrue(keys.Contains(key));
            ClassicAssert.IsNotNull(value);
            keys = context.Cache.GetKeysByTag(new [] {tag2}).ToList();
            ClassicAssert.IsFalse(keys.Contains(key));
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CacheSetWithTags_PersistentOverridesExpiration(RedisContext context)
        {
            var key1 = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            var key2 = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}-2";
            string tag = $"{key1}-Tag1";
            context.Cache.InvalidateKeysByTag(tag);
            context.Cache.SetObject(key1, "test value 1", new[] { tag }, TimeSpan.FromSeconds(1));
            context.Cache.SetObject(key2, "test value 2", new[] { tag }, TimeSpan.MaxValue);
            Thread.Sleep(2000);
            var keys = context.Cache.GetKeysByTag(new[] { tag }).ToList();
            var keysCleaned = context.Cache.GetKeysByTag(new[] { tag }, true).ToList();
            ClassicAssert.AreEqual(2, keys.Count);
            ClassicAssert.AreEqual(1, keysCleaned.Count);
            ClassicAssert.IsTrue(keys.Contains(key1));
            ClassicAssert.IsTrue(keys.Contains(key2));
            ClassicAssert.IsTrue(keysCleaned.Contains(key2));
            context.Cache.InvalidateKeysByTag(tag);
        }


        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CacheSetWithTags_Expiration(RedisContext context)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            var key2 = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}-2";
            context.Cache.Remove(key);
            context.Cache.Remove(key2);
            var users = GetUsers();
            context.Cache.InvalidateKeysByTag("user:" + users[0].Id, "user:" + users[1].Id, "user-info");

            context.Cache.SetObject(key, users[0], new[] { "user:" + users[0].Id, "user-info" }, TimeSpan.FromSeconds(1));
            context.Cache.SetObject(key2, users[1], new[] { "user:" + users[1].Id, "user-info" }, TimeSpan.FromSeconds(5));
            var keys = context.Cache.GetKeysByTag(new[] { "user:" + users[0].Id }).ToList();
            ClassicAssert.IsTrue(keys.Contains(key));
            var value = context.Cache.GetObject<User>(keys.First());
            ClassicAssert.IsNotNull(value);
            Thread.Sleep(1200);
            var keys2 = context.Cache.GetKeysByTag(new[] { "user:" + users[0].Id });
            ClassicAssert.IsFalse(keys2.Contains(key));
            value = context.Cache.GetObject<User>(key);
            ClassicAssert.IsNull(value);
            var keys3 = context.Cache.GetKeysByTag(new[] { "user-info" });
            ClassicAssert.IsTrue(keys3.Contains(key2));
            Thread.Sleep(4000);
            var keys4 = context.Cache.GetKeysByTag(new[] { "user-info" });
            ClassicAssert.IsFalse(keys4.Contains(key2));
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CacheSetWithTags_Removal(RedisContext context)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            context.Cache.Remove(key);
            var users = GetUsers();
            context.Cache.InvalidateKeysByTag("user:" + users[0].Id);

            context.Cache.SetObject(key, users[0], new[] { "user:" + users[0].Id });
            var keys = context.Cache.GetKeysByTag(new[] { "user:" + users[0].Id }, true);
            ClassicAssert.IsTrue(keys.Contains(key));
            context.Cache.Remove(key);
            var keys2 = context.Cache.GetKeysByTag(new[] { "user:" + users[0].Id }, true);
            ClassicAssert.IsFalse(keys2.Contains(key));
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CacheSetWithTags_Multiple(RedisContext context)
        {
            var key0 = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}-0";
            var key1 = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}-1";
            context.Cache.Remove(key0);
            context.Cache.Remove(key1);
            var users = GetUsers();
            context.Cache.InvalidateKeysByTag("user:" + users[0].Id, "user:" + users[1].Id, "user-info");

            context.Cache.SetObject(key0, users[0], new[] { "user:" + users[0].Id, "user-info" });
            context.Cache.SetObject(key1, users[1], new[] { "user:" + users[1].Id, "user-info" });
            var keys0 = context.Cache.GetKeysByTag(new[] { "user:" + users[0].Id });
            var keys1 = context.Cache.GetKeysByTag(new[] { "user:" + users[1].Id });
            var keys = context.Cache.GetKeysByTag(new[] { "user-info" }).ToList();
            ClassicAssert.IsTrue(keys0.Contains(key0));
            ClassicAssert.IsTrue(keys1.Contains(key1));
            ClassicAssert.IsTrue(keys.Contains(key0) && keys.Contains(key1));
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CacheRemoveByTags(RedisContext context)
        {
            var key1 = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}-1";
            var key2 = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}-2";
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
            ClassicAssert.IsNull(user);
            ClassicAssert.AreEqual(0, keys.Count());
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CacheGetObjectsByTag(RedisContext context)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}" + "{0}";
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
            ClassicAssert.IsTrue(t1Users.TrueForAll(u => u.Id % 3 == 0));
            ClassicAssert.IsTrue(t2Users.TrueForAll(u => u.Id % 3 == 1));
            ClassicAssert.IsTrue(t3Users.TrueForAll(u => u.Id % 3 == 2));
            context.Cache.InvalidateKeysByTag(tag1, tag2, tag3);
            t1Users = context.Cache.GetObjectsByTag<User>(tag1).ToList();
            ClassicAssert.AreEqual(0, t1Users.Count);
            ClassicAssert.IsNull(context.Cache.GetObject<User>(string.Format(key, 1)));
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CacheAddRemoveTagToKey(RedisContext context)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            string tag = $"{key}_Tag";
            context.Cache.Remove(key);
            context.Cache.SetObject(key, "value");
            context.Cache.AddTagsToKey(key, new[] { tag });
            var keys = context.Cache.GetKeysByTag(new [] { tag }).ToList();
            ClassicAssert.IsTrue(keys.Contains(key));
            context.Cache.RemoveTagsFromKey(key, new[] { tag });
            keys = context.Cache.GetKeysByTag(new [] { tag }).ToList();
            ClassicAssert.IsFalse(keys.Contains(key));

        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_CacheSetHashedAll(RedisContext context)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            context.Cache.Remove(key);
            var users = GetUsers();
            IDictionary<string, User> allUsers = users.ToDictionary(k => k.Id.ToString());
            context.Cache.SetHashed(key, allUsers);
            var response = context.Cache.GetHashedAll<User>(key);
            ClassicAssert.AreEqual(users.Count, response.Count);
            ClassicAssert.IsTrue(users.All(x => response.ContainsKey(x.Id.ToString())));
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.All))]
        public void UT_Cache_HllAddCount(RedisContext context)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            context.Cache.Remove(key);
            context.Cache.HyperLogLogAdd(key, new[] { 1, 2, 3, 4, 5, 6 });
            context.Cache.HyperLogLogAdd(key, new[] { 4, 5, 6, 7, 8, 9 });
            context.Cache.HyperLogLogAdd(key, 10);

            var cnt = context.Cache.HyperLogLogCount(key);
            ClassicAssert.AreEqual(10, cnt);
        }

#if (NET462)
        [Test, TestCaseSource(typeof(Common), nameof(Common.Bin))]
        public void UT_CacheSerialization(RedisContext context)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
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
            ClassicAssert.AreEqual(exItem.Data.Count, exFinal.Data.Count);
            ClassicAssert.AreEqual(exItem.InnerException.Message, exFinal.InnerException.Message);
            ClassicAssert.AreEqual(exItem.StackTrace, exFinal.StackTrace);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.Bin))]
        public void UT_CacheSetHashed_MultipleFieldsDistinctTypes(RedisContext context)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
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

            ClassicAssert.AreEqual(222, user.Id);
            ClassicAssert.AreEqual(3, dept.Id);
            ClassicAssert.AreEqual(444, loc.Id);

            ClassicAssert.AreEqual(3, all.Count);

            ClassicAssert.AreEqual(222, ((User)all["a"]).Id);
            ClassicAssert.AreEqual(3, ((Department)all["2"]).Id);
            ClassicAssert.AreEqual(444, ((Location)all["D"]).Id);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.Bin))]
        public void UT_CacheFetch_TagsBuilder(RedisContext context)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            var users = GetUsers();
            var user = users[0];
            context.Cache.Remove(key);
            context.Cache.InvalidateKeysByTag("user-id-tag:" + user.Id);
            context.Cache.FetchObject(key, () => user, u => new[] { "user-id-tag:" + u.Id });
            context.Cache.FetchObject(key, () => (User)null, u => new[] { "wrong" });
            ClassicAssert.AreEqual(0, context.Cache.GetKeysByTag(new[] { "wrong" }).Count());
            var result = context.Cache.GetObjectsByTag<User>("user-id-tag:" + user.Id).First();
            ClassicAssert.AreEqual(0, context.Cache.GetKeysByTag(new[] { "wrong" }).Count());
            ClassicAssert.AreEqual(user.Id, result.Id);
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.Bin))]
        public void UT_CacheFetchHashed_TagsBuilder(RedisContext context)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            string field = "field";
            var users = GetUsers();
            var user = users[0];
            context.Cache.Remove(key);
            context.Cache.InvalidateKeysByTag("user-id-tag:" + user.Id);
            context.Cache.FetchHashed(key, field, () => user, u => new[] { "user-id-tag:" + u.Id });
            context.Cache.FetchHashed(key, field, () => (User)null, u => new[] { "wrong" });
            ClassicAssert.AreEqual(0, context.Cache.GetKeysByTag(new[] { "wrong" }).Count());
            var result = context.Cache.GetObjectsByTag<User>(new[] { "user-id-tag:" + user.Id }).First();
            ClassicAssert.AreEqual(0, context.Cache.GetKeysByTag(new[] { "wrong" }).Count());
            ClassicAssert.AreEqual(user.Id, result.Id);
        }
        [Test, TestCaseSource(typeof (Common), nameof(Common.Bin))]
        public void UT_CacheTagRename(RedisContext context)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            context.Cache.Remove(key);
            string tag1 = $"{key}-Tag1";
            string tag2 = $"{key}-Tag2";
            context.Cache.InvalidateKeysByTag(tag1, tag2);
            var user = GetUsers()[0];
            context.Cache.SetObject(key, user, new [] { tag1 });
            ClassicAssert.AreEqual(1, context.Cache.GetKeysByTag(new [] { tag1 }).Count());
            context.Cache.RenameTagForKey(key, tag1, tag2);
            ClassicAssert.AreEqual(0, context.Cache.GetKeysByTag(new[] { tag1 }).Count());
            ClassicAssert.AreEqual(1, context.Cache.GetKeysByTag(new[] { tag2 }).Count());
            context.Cache.RemoveTagsFromKey(key, new [] { tag2 });
            context.Cache.RenameTagForKey(key, tag2, tag1);
            ClassicAssert.AreEqual(0, context.Cache.GetKeysByTag(new[] { tag1 }).Count());
            ClassicAssert.AreEqual(0, context.Cache.GetKeysByTag(new[] { tag2 }).Count());
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.Bin))]
        public void UT_CacheFieldTagRename(RedisContext context)
        {
            var key = $"{TestContext.CurrentContext.Test.MethodName}-{context.GetSerializer().GetType().Name}";
            string field = "field";
            context.Cache.Remove(key);
            string tag1 = $"{key}-Tag1";
            string tag2 = $"{key}-Tag2";
            context.Cache.InvalidateKeysByTag(tag1, tag2);
            var user = GetUsers()[0];
            context.Cache.SetHashed(key, field, user, new[] { tag1 });
            ClassicAssert.AreEqual(1, context.Cache.GetKeysByTag(new[] { tag1 }).Count());
            context.Cache.RenameTagForHashField(key, field, tag1, tag2);
            ClassicAssert.AreEqual(0, context.Cache.GetKeysByTag(new[] { tag1 }).Count());
            ClassicAssert.AreEqual(1, context.Cache.GetKeysByTag(new[] { tag2 }).Count());
            context.Cache.RemoveTagsFromHashField(key, field, new[] { tag2 });
            context.Cache.RemoveTagsFromHashField(key, field, new [] { tag2, tag1 });
            ClassicAssert.AreEqual(0, context.Cache.GetKeysByTag(new[] { tag1 }).Count());
            ClassicAssert.AreEqual(0, context.Cache.GetKeysByTag(new[] { tag2 }).Count());
        }

        [Test]
        public void UT_Cache_RawOverrideSerializer_object()
        {
            var raw = new RawSerializer();
            raw.SetSerializerFor<object>(o => Encoding.UTF8.GetBytes(o.GetHashCode().ToString()),
                b => int.Parse(Encoding.UTF8.GetString(b)));
            var ctx = new RedisContext(Common.Config, raw);
            Thread.Sleep(1000);
            var key = $"{TestContext.CurrentContext.Test.MethodName}";
            ctx.Cache.Remove(new[] { key });
            User usr = new User();
            ctx.Cache.SetObject<object>(key, usr);
            var v = ctx.Cache.GetObject<object>(key);
            ClassicAssert.AreEqual(usr.GetHashCode(), v);
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
