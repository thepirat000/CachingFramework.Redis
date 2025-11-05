using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.RedisObjects;
using CachingFramework.Redis.Serializers;
using NUnit.Framework;
using System.Threading.Tasks;
using CachingFramework.Redis.Contracts.RedisObjects;
using System.Runtime.InteropServices.ComTypes;
using Nito.AsyncEx;
using NUnit.Framework.Legacy;

namespace CachingFramework.Redis.UnitTest
{
    [TestFixture]
    public class UnitTestRedisObjects
    {
        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_SetGetDictionaryFieldsMultiple(RedisContext ctx)
        {
            var key = "UT_SetGetDictionaryFieldsMultiple";
            ctx.Cache.Remove(key);

            var dict = ctx.Collections.GetRedisDictionary<string, int>(key);
            dict.AddRange(Enumerable.Range(1, 20).ToDictionary(i => $"k{i}", i => i));

            var result = dict.GetRange("k1", "k5", "kXXX", "k10").ToList();

            ClassicAssert.AreEqual(4, result.Count);
            ClassicAssert.AreEqual(1, result[0]);
            ClassicAssert.AreEqual(5, result[1]);
            ClassicAssert.AreEqual(0, result[2]);
            ClassicAssert.AreEqual(10, result[3]);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheGeo_WithTags(RedisContext context)
        {
            if (Common.VersionInfo[0] < 3)
            {
                Assert.Ignore($"Geospatial tests ignored for version {string.Join(".", Common.VersionInfo)}\n");
                return;
            }

            string key = "UT_CacheGeo_WithTags";
            context.Cache.Remove(key);
            context.Cache.InvalidateKeysByTag("tag1", "tag2", "common");

            var geo1 = context.GeoSpatial.GeoAdd(key, 12.34, 23.45, "value1", new[] { "tag1", "common" });
            var geo2 = context.GeoSpatial.GeoAdd(key, 33.34, 11.45, "value2", new[] { "tag2", "common" });

            var t1 = context.Cache.GetObjectsByTag<string>("tag1").ToList();
            var t2 = context.Cache.GetObjectsByTag<string>("tag2").ToList();
            var x = context.Cache.GetObjectsByTag<string>("common").ToList();

            ClassicAssert.AreEqual(2, x.Count);
            ClassicAssert.AreEqual(1, t1.Count);
            ClassicAssert.AreEqual(1, t2.Count);
            ClassicAssert.IsTrue(x.Contains("value1"));
            ClassicAssert.IsTrue(x.Contains("value2"));
            ClassicAssert.IsTrue(t1.Contains("value1"));
            ClassicAssert.IsTrue(t2.Contains("value2"));

            context.Cache.RemoveTagsFromSetMember(key, "value1", new[] { "tag1" });
            ClassicAssert.AreEqual(0, context.Cache.GetObjectsByTag<string>("tag1").Count());

            context.Cache.InvalidateKeysByTag("common");

            ClassicAssert.IsNull(context.GeoSpatial.GeoPosition(key, "value1"));
            ClassicAssert.IsNull(context.GeoSpatial.GeoPosition(key, "value2"));
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheSortedSet_WithTags(RedisContext context)
        {
            string key = "UT_CacheSortedSet_WithTags";
            context.Cache.Remove(key);
            var set = context.Collections.GetRedisSet<string>(key);
            context.Cache.InvalidateKeysByTag("tag1", "tag2", "common");

            var sset = context.Collections.GetRedisSortedSet<string>(key);

            sset.Add(.1, "value1", new[] { "tag1", "common" });
            sset.Add(.2, "value2", new[] { "tag2" });

            context.Cache.AddTagsToSetMemberAsync(key, "value2", new[] { "common" }).Wait();

            var t1 = context.Cache.GetObjectsByTag<string>("tag1").ToList();
            var t2 = context.Cache.GetObjectsByTag<string>("tag2").ToList();
            var x = context.Cache.GetObjectsByTag<string>("common").ToList();

            ClassicAssert.AreEqual(2, x.Count);
            ClassicAssert.AreEqual(1, t1.Count);
            ClassicAssert.AreEqual(1, t2.Count);
            ClassicAssert.IsTrue(x.Contains("value1"));
            ClassicAssert.IsTrue(x.Contains("value2"));
            ClassicAssert.IsTrue(t1.Contains("value1"));
            ClassicAssert.IsTrue(t2.Contains("value2"));

            context.Cache.InvalidateKeysByTag("common");

            ClassicAssert.AreEqual(0, sset.Count);
        }


        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheSet_Mix_WithTags(RedisContext context)
        {
            string key = "UT_CacheSet_Mix_WithTags_Set";
            string sskey = "UT_CacheSet_Mix_WithTags_SortedSet";
            string geokey = "UT_CacheSet_Mix_WithTags_Geo";
            context.Cache.InvalidateKeysByTag("tag");
            context.Cache.Remove(key);
            context.Cache.Remove(sskey);
            var set = context.Collections.GetRedisSet<string>(key);
            var sset = context.Collections.GetRedisSortedSet<string>(sskey);
            context.GeoSpatial.GeoAdd(geokey, 12.34, 23.45, "geo2");
            context.Cache.AddTagsToSetMember(geokey, "geo2", new[] { "tag" });

            set.Add("s1");
            set.Add("s2", new[] { "tag" });
            set.Add("s3");

            sset.Add(0.1, "ss1");
            sset.Add(0.2, "ss2");
            context.Cache.AddTagsToSetMember(sskey, "ss2", new[] { "tag" });
            sset.Add(0.3, "ss3");

            var x = context.Cache.GetObjectsByTag<string>("tag").ToList();

            ClassicAssert.AreEqual(3, x.Count);
            ClassicAssert.IsTrue(x.Contains("geo2"));
            ClassicAssert.IsTrue(x.Contains("s2"));
            ClassicAssert.IsTrue(x.Contains("ss2"));

            context.Cache.InvalidateKeysByTag("tag");

            x = context.Cache.GetObjectsByTag<string>("tag").ToList();
            var pos = context.GeoSpatial.GeoPosition(geokey, "geo2");

            ClassicAssert.AreEqual(0, x.Count);
            ClassicAssert.IsNull(pos);
            ClassicAssert.IsFalse(sset.Contains("ss2"));
            ClassicAssert.IsTrue(sset.Contains("ss1"));
            ClassicAssert.IsTrue(set.Contains("s1"));
            ClassicAssert.IsFalse(set.Contains("s2"));
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheSet_WithTags(RedisContext context)
        {
            string key = "UT_CacheSet_WithTags";
            string otherKey = "UT_CacheSet_WithTags_Other";
            context.Cache.Remove(key);
            context.Cache.Remove(otherKey);
            var set = context.Collections.GetRedisSet<string>(key);
            context.Cache.InvalidateKeysByTag("tag1", "tag2", "tag3", "tagXXX", "common");

            set.Add("item1", new[] { "tag1", "common" });
            set.Add("item2", new[] { "tag2", "common" });
            set.Add("item222", new[] { "tag2", "common" });
            set.Add("item3", new[] { "tagXXX" });
            context.Cache.RenameTagForSetMember(key, "item3", "tagXXX", "tag3");
            context.Cache.AddTagsToSetMember(key, "item3", new[] { "common" });
            context.Cache.SetHashed(otherKey, "field", "other", new[] { "common" });

            var commonValues = context.Cache.GetObjectsByTag<string>("common").ToList();
            var xxx = context.Cache.GetObjectsByTag<string>("tagXXX").ToList();
            var t1 = context.Cache.GetObjectsByTag<string>("tag1").ToList();
            var t2 = context.Cache.GetObjectsByTag<string>("tag2").ToList();
            var t3 = context.Cache.GetObjectsByTag<string>("tag3").ToList();

            ClassicAssert.AreEqual(0, xxx.Count);
            ClassicAssert.AreEqual(5, commonValues.Count);
            ClassicAssert.AreEqual(1, t1.Count);
            ClassicAssert.AreEqual(2, t2.Count);
            ClassicAssert.AreEqual(1, t3.Count);

            ClassicAssert.AreEqual("item1", t1[0]);
            ClassicAssert.IsTrue(t2.Contains("item2"));
            ClassicAssert.IsTrue(t2.Contains("item222"));
            ClassicAssert.AreEqual("item3", t3[0]);

            context.Cache.RemoveTagsFromSetMember(key, "item222", new [] { "tag2", "common" });
            commonValues = context.Cache.GetObjectsByTag<string>("common").ToList();
            t2 = context.Cache.GetObjectsByTag<string>("tag2").ToList();

            ClassicAssert.AreEqual(4, commonValues.Count);
            ClassicAssert.AreEqual(1, t2.Count);
            ClassicAssert.AreEqual("item2", t2[0]);
        }

        [Test, TestCaseSource(typeof (Common), "Raw")]
        public void UT_CacheSortedSet_When(RedisContext context)
        {
            string key = "UT_CacheSortedSet_When";
            context.Cache.Remove(key);
            var lst = context.Collections.GetRedisSortedSet<string>(key);

            lst.Add(12.345, "item1", When.Exists);
            var rank = lst.RankOf("item1");
            ClassicAssert.IsNull(rank);

            lst.Add(12.345, "item1", When.NotExists);
            rank = lst.RankOf("item1");
            ClassicAssert.AreEqual(0, rank.Value);

            lst.Add(34.567, "item1", When.NotExists);
            var score = lst.ScoreOf("item1");
            ClassicAssert.AreEqual(12.345, score.Value, 0.0001);

            lst.Add(34.567, "itemXXX", When.Exists);
            score = lst.ScoreOf("itemXXX");
            ClassicAssert.IsNull(score);

            lst.Add(34.567, "item1", When.Exists);
            score = lst.ScoreOf("item1");
            ClassicAssert.AreEqual(34.567, score.Value, 0.0001);

            lst.AddRange(new [] { new SortedMember<string>(56.789, "item1"), new SortedMember<string>(77.888, "itemXXX") }, When.Exists);
            score = lst.ScoreOf("item1");
            ClassicAssert.AreEqual(56.789, score.Value, 0.0001);
            score = lst.ScoreOf("itemXXX");
            ClassicAssert.IsNull(score);

            lst.AddRange(new[] { new SortedMember<string>(99.999, "item1"), new SortedMember<string>(88.999, "itemXXX") }, When.NotExists);
            score = lst.ScoreOf("item1");
            ClassicAssert.AreEqual(56.789, score.Value, 0.0001);
            score = lst.ScoreOf("itemXXX");
            ClassicAssert.AreEqual(88.999, score.Value, 0.0001);

            context.Cache.Remove(key);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheList_Remove(RedisContext context)
        {
            string key = "UT_CacheList_Remove";
            context.Cache.Remove(key);
            var lst = context.Collections.GetRedisList<string>(key);
            lst.AddRange(new [] { "test", "test", "anothertest" });
            ClassicAssert.AreEqual(3, lst.Count);
            lst.RemoveAt(0);
            ClassicAssert.AreEqual(2, lst.Count);
            ClassicAssert.AreEqual("test", lst[0]);
            ClassicAssert.AreEqual("anothertest", lst[1]);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheList_Insert(RedisContext context)
        {
            string key = "UT_CacheList_Insert";
            context.Cache.Remove(key);
            var rl = context.Collections.GetRedisList<string>(key);
            rl.Insert(0, "test");
            rl.Insert(0, "test");
            rl.Insert(0, "test");
            ClassicAssert.AreEqual(3, rl.Count);
            rl.Insert(2, "test2");
            ClassicAssert.AreEqual(4, rl.Count);
            ClassicAssert.AreEqual("test", rl[0]);
            ClassicAssert.AreEqual("test2", rl[2]);
            rl.Insert(rl.Count, "LAST");
            ClassicAssert.AreEqual(5, rl.Count);
            ClassicAssert.AreEqual("LAST", rl[rl.Count - 1]);
            rl[rl.Count - 1] = "NEW LAST";
            ClassicAssert.AreEqual(5, rl.Count);
            ClassicAssert.AreEqual("NEW LAST", rl[rl.Count - 1]);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheList_Trim(RedisContext context)
        {
            string key = "UT_CacheList_Trim";
            context.Cache.Remove(key);
            var rl = context.Collections.GetRedisList<int>(key);
            rl.AddRange(Enumerable.Range(1, 100));
            ClassicAssert.AreEqual(100, rl.Count);
            rl.Trim(0, 9);
            ClassicAssert.AreEqual(10, rl.Count);
            ClassicAssert.AreEqual(1, rl[0]);
            ClassicAssert.AreEqual(10, rl[-1]);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheListObject(RedisContext context)
        {
            string key1 = "UT_CacheListObject1";
            context.Cache.Remove(key1);
            var users = GetUsers();
            var rl = context.Collections.GetRedisList<int>(key1);
            rl.AddRange(users.Select(u => u.Id));
            // Test GetEnumerator
            foreach (var item in rl)
            {
                ClassicAssert.IsTrue(users.Any(u => u.Id == item));
            }
            // Test Count
            ClassicAssert.AreEqual(users.Count, rl.Count);
            // Test First and Last
            var first = rl.First();
            ClassicAssert.AreEqual(users.First().Id, first);
            var firstod = rl.FirstOrDefault();
            ClassicAssert.AreEqual(users.First().Id, firstod);
            ClassicAssert.AreEqual(users.Last().Id, rl.Last());
            ClassicAssert.AreEqual(users.Last().Id, rl.LastOrDefault());
            // Test Contains
            ClassicAssert.IsTrue(rl.Contains(users[2].Id));
            // Test Insert
            rl.Insert(0, 0);
            ClassicAssert.AreEqual(0, rl[0]);
            // Test RemoveAt
            rl.RemoveAt(0);
            ClassicAssert.AreEqual(1, rl[0]);
            // Test Add
            rl.Add(5);
            ClassicAssert.AreEqual(5, rl.Last());
            // Test IndexOf
            ClassicAssert.AreEqual(2, rl.IndexOf(users[2].Id));
            // Test Remove
            rl.Remove(users[2].Id);
            ClassicAssert.IsFalse(rl.Contains(users[2].Id));
            // Test CopyTo
            int[] array = new int[50];
            rl.CopyTo(array, 10);
            ClassicAssert.AreEqual(1, array[10]);
            // Test Clear
            rl.Clear();
            ClassicAssert.AreEqual(0, rl.Count);
            ClassicAssert.AreEqual(0, rl.LastOrDefault());
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheListPushPop(RedisContext context)
        {
            string key = "UT_CacheListPushPop";
            context.Cache.Remove(key);
            var users = GetUsers();
            var rl = context.Collections.GetRedisList<User>(key);
            ClassicAssert.IsNull(rl.FirstOrDefault());
            ClassicAssert.IsNull(rl.LastOrDefault());
            rl.AddRange(users);
            rl.PushFirst(new User() { Id = 0 });
            rl.PushLast(new User() { Id = 666 });
            ClassicAssert.AreEqual(0, rl[0].Id);
            ClassicAssert.AreEqual(0, rl.FirstOrDefault().Id);
            ClassicAssert.AreEqual(666, rl.LastOrDefault().Id);
            ClassicAssert.AreEqual(users.Count + 2, rl.Count);
            var remf = rl.PopFirst();
            ClassicAssert.AreEqual(users.Count + 1, rl.Count);
            ClassicAssert.AreEqual(0, remf.Id);
            var reml = rl.PopLast();
            ClassicAssert.AreEqual(users.Count, rl.Count);
            ClassicAssert.AreEqual(666, reml.Id);
        }

        [Test, TestCaseSource(typeof(Common), "Raw")]
        public void UT_CacheListRemoveAt(RedisContext context)
        {
            string key = "UT_CacheListRemoveAt";
            context.Cache.Remove(key);
            var rl = context.Collections.GetRedisList<string>(key);
            rl.RemoveAt(0);
            rl.PushLast("test 1");
            rl.PushLast("test 2");
            rl.PushLast("test 3");
            rl.PushLast("test 4");
            rl.PushLast("test 5");

            rl.RemoveAt(0);
            ClassicAssert.AreEqual(4, rl.Count);
            ClassicAssert.AreEqual("test 2", rl[0]);

            rl.RemoveAt(rl.Count - 1);
            ClassicAssert.AreEqual(3, rl.Count);
            ClassicAssert.AreEqual("test 4", rl.LastOrDefault());

            rl.RemoveAt(1);
            ClassicAssert.AreEqual(2, rl.Count);
            ClassicAssert.AreEqual("test 2", rl.FirstOrDefault());
            ClassicAssert.AreEqual("test 4", rl.LastOrDefault());
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheListObjectTTL(RedisContext context)
        {
            string key1 = "UT_CacheListObject_TTL1";
            context.Cache.Remove(key1);
            var users = GetUsers();
            var rl = context.Collections.GetRedisList<User>(key1);
            rl.AddRange(users);
            rl.TimeToLive = TimeSpan.FromMilliseconds(1500);
            ClassicAssert.AreEqual(users.Count, rl.Count);
            Thread.Sleep(2000);
            ClassicAssert.AreEqual(0, rl.Count);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheListObject_GetRange(RedisContext context)
        {
            string key = "UT_CacheListObject_GetRange";
            int total = 100;
            context.Cache.Remove(key);
            var rl = context.Collections.GetRedisList<User>(key);
            rl.AddRange(Enumerable.Range(1, total).Select(i => new User() {Id = i}));

            var range = rl.GetRange().ToList();
            ClassicAssert.AreEqual(total, rl.Count);

            range = rl.GetRange(3, 10).ToList();
            ClassicAssert.AreEqual(8, range.Count);
            ClassicAssert.AreEqual(4, range[0].Id);

            range = rl.GetRange(10, -10).ToList();
            ClassicAssert.AreEqual(11, range[0].Id);
            ClassicAssert.AreEqual(91, range[range.Count - 1].Id);
        }


        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheList_Remove_Async(RedisContext context)
        {
            string key = "UT_CacheList_Remove_Async";
            await context.Cache.RemoveAsync(key);
            var lst = context.Collections.GetRedisList<string>(key);
            await lst.AddRangeAsync(new[] { "test", "test", "anothertest" });
            ClassicAssert.AreEqual(3, lst.Count);
            await lst.RemoveAtAsync(0);
            ClassicAssert.AreEqual(2, lst.Count);
            ClassicAssert.AreEqual("test", lst[0]);
            ClassicAssert.AreEqual("anothertest", lst[1]);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheList_Insert_Async(RedisContext context)
        {
            string key = "UT_CacheList_Insert_Async";
            await context.Cache.RemoveAsync(key);
            var rl = context.Collections.GetRedisList<string>(key);
            await rl.InsertAsync(0, "test");
            await rl.InsertAsync(0, "test");
            await rl.InsertAsync(0, "test");
            ClassicAssert.AreEqual(3, rl.Count);
            await rl.InsertAsync(2, "test2");
            ClassicAssert.AreEqual(4, rl.Count);
            ClassicAssert.AreEqual("test", rl[0]);
            ClassicAssert.AreEqual("test2", rl[2]);
            await rl.InsertAsync(rl.Count, "LAST");
            ClassicAssert.AreEqual(5, rl.Count);
            ClassicAssert.AreEqual("LAST", rl[rl.Count - 1]);
            rl[rl.Count - 1] = "NEW LAST";
            ClassicAssert.AreEqual(5, rl.Count);
            ClassicAssert.AreEqual("NEW LAST", rl[rl.Count - 1]);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheList_Trim_Async(RedisContext context)
        {
            string key = "UT_CacheList_Trim_Async";
            await context.Cache.RemoveAsync(key);
            var rl = context.Collections.GetRedisList<int>(key);
            await rl.AddRangeAsync(Enumerable.Range(1, 100));
            ClassicAssert.AreEqual(100, rl.Count);
            await rl.TrimAsync(0, 9);
            ClassicAssert.AreEqual(10, rl.Count);
            ClassicAssert.AreEqual(1, rl[0]);
            ClassicAssert.AreEqual(10, rl[-1]);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheListObject_Async(RedisContext context)
        {
            string key1 = "UT_CacheListObject_Async";
            await context.Cache.RemoveAsync(key1);
            var users = GetUsers();
            var rl = context.Collections.GetRedisList<int>(key1);
            await rl.AddRangeAsync(users.Select(u => u.Id));
            // Test GetEnumerator
            foreach (var item in rl)
            {
                ClassicAssert.IsTrue(users.Any(u => u.Id == item));
            }
            // Test Count
            ClassicAssert.AreEqual(users.Count, rl.Count);
            // Test First and Last
            var first = rl.First();
            ClassicAssert.AreEqual(users.First().Id, first);
            var firstod = rl.FirstOrDefault();
            ClassicAssert.AreEqual(users.First().Id, firstod);
            ClassicAssert.AreEqual(users.Last().Id, rl.Last());
            ClassicAssert.AreEqual(users.Last().Id, rl.LastOrDefault());
            // Test Contains
            ClassicAssert.IsTrue(rl.Contains(users[2].Id));
            // Test Insert
            await rl.InsertAsync(0, 0);
            ClassicAssert.AreEqual(0, rl[0]);
            // Test RemoveAt
            await rl.RemoveAtAsync(0);
            ClassicAssert.AreEqual(1, rl[0]);
            // Test Add
            await rl.AddAsync(5);
            ClassicAssert.AreEqual(5, rl.Last());
            // Test IndexOf
            var i = await rl.IndexOfAsync(users[2].Id);
            ClassicAssert.AreEqual(2, i);
            // Test Remove
            await rl.RemoveAsync(users[2].Id, 1);
            var b = await rl.ContainsAsync(users[2].Id);
            ClassicAssert.IsFalse(b);
            // Test CopyTo
            int[] array = new int[50];
            rl.CopyTo(array, 10);
            ClassicAssert.AreEqual(1, array[10]);
            // Test Clear
            await rl.ClearAsync();
            ClassicAssert.AreEqual(0, rl.Count);
            ClassicAssert.AreEqual(0, rl.LastOrDefault());
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheListObject_Async_NoDeadlocks(RedisContext context)
        {
            string key1 = "UT_CacheListObject_Async_NoDeadlocks";

            await Common.TestDeadlock(() =>
            {
                context.Cache.RemoveAsync(key1).Wait();
            });



            var users = GetUsers();
            var rl = context.Collections.GetRedisList<int>(key1);


            await Common.TestDeadlock(() =>
            {
                rl.AddRangeAsync(users.Select(u => u.Id)).Wait();
            });

            await Common.TestDeadlock(() =>
            {
                rl.InsertAsync(0, 0).Wait();
            });

            // Test RemoveAt
            await Common.TestDeadlock(() =>
            {
                rl.RemoveAtAsync(0).Wait();
            });

            await Common.TestDeadlock(() =>
            {
                // Test Add
                rl.AddAsync(5).Wait();
            });

            await Common.TestDeadlock(() =>
            {
                // Test IndexOf
                _ = rl.IndexOfAsync(users[2].Id).Result;
            });

            await Common.TestDeadlock(() =>
            {
                // Test Remove
                rl.RemoveAsync(users[2].Id, 1).Wait();
            });

            await Common.TestDeadlock(() =>
            {
                // Test Contains
                rl.ContainsAsync(users[2].Id).Wait();
            });

            await Common.TestDeadlock(() =>
            {
                // Test Clear
                rl.ClearAsync().Wait();
            });

        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheListPushPop_Async(RedisContext context)
        {
            string key = "UT_CacheListPushPop_Async";
            await context.Cache.RemoveAsync(key);
            var users = GetUsers();
            var rl = context.Collections.GetRedisList<User>(key);
            ClassicAssert.IsNull(rl.FirstOrDefault());
            ClassicAssert.IsNull(rl.LastOrDefault());
            await rl.AddRangeAsync(users);
            await rl.PushFirstAsync(new User() { Id = 0 });
            await rl.PushLastAsync(new User() { Id = 666 });
            ClassicAssert.AreEqual(0, rl[0].Id);
            ClassicAssert.AreEqual(0, rl.FirstOrDefault().Id);
            ClassicAssert.AreEqual(666, rl.LastOrDefault().Id);
            ClassicAssert.AreEqual(users.Count + 2, rl.Count);
            var remf = await rl.PopFirstAsync();
            ClassicAssert.AreEqual(users.Count + 1, rl.Count);
            ClassicAssert.AreEqual(0, remf.Id);
            var reml = await rl.PopLastAsync();
            ClassicAssert.AreEqual(users.Count, rl.Count);
            ClassicAssert.AreEqual(666, reml.Id);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheListPushPop_Async_NoDeadlock(RedisContext context)
        {
            string key = "UT_CacheListPushPop_Async";
            await Common.TestDeadlock(() =>
            {
                _ = context.Cache.RemoveAsync(key).Result;
            });

            var users = GetUsers();
            var rl = context.Collections.GetRedisList<User>(key);

            await Common.TestDeadlock(() =>
            {
                rl.AddRangeAsync(users).Wait();
            });
            await Common.TestDeadlock(() =>
            {
                rl.PushFirstAsync(new User() { Id = 0 }).Wait();
            });

            await Common.TestDeadlock(() =>
            {
                rl.PushLastAsync(new User() { Id = 666 }).Wait();
            });

            await Common.TestDeadlock(() =>
            {
                _ = rl.PopFirstAsync().Result;
            });

            await Common.TestDeadlock(() =>
            {
                rl.PopLastAsync().Wait();
            });
        }

        [Test, TestCaseSource(typeof(Common), "Raw")]
        public async Task UT_CacheListRemoveAt_Async(RedisContext context)
        {
            string key = "UT_CacheListRemoveAt_Async";
            await context.Cache.RemoveAsync(key);
            var rl = context.Collections.GetRedisList<string>(key);
            await rl.RemoveAtAsync(0);
            await rl.PushLastAsync("test 1");
            await rl.PushLastAsync("test 2");
            await rl.PushLastAsync("test 3");
            await rl.PushLastAsync("test 4");
            await rl.PushLastAsync("test 5");

            await rl.RemoveAtAsync(0);
            ClassicAssert.AreEqual(4, rl.Count);
            ClassicAssert.AreEqual("test 2", rl[0]);

            await rl.RemoveAtAsync(rl.Count - 1);
            ClassicAssert.AreEqual(3, rl.Count);
            ClassicAssert.AreEqual("test 4", rl.LastOrDefault());

            await rl.RemoveAtAsync(1);
            ClassicAssert.AreEqual(2, rl.Count);
            ClassicAssert.AreEqual("test 2", rl.FirstOrDefault());
            ClassicAssert.AreEqual("test 4", rl.LastOrDefault());
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheListObjectTTL_Async(RedisContext context)
        {
            string key1 = "UT_CacheListObjectTTL_Async";
            await context.Cache.RemoveAsync(key1);
            var users = GetUsers();
            var rl = context.Collections.GetRedisList<User>(key1);
            await rl.AddRangeAsync(users);
            rl.TimeToLive = TimeSpan.FromMilliseconds(1500);
            ClassicAssert.AreEqual(users.Count, rl.Count);
            Thread.Sleep(2000);
            ClassicAssert.AreEqual(0, rl.Count);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheListObject_GetRange_Async(RedisContext context)
        {
            string key = "UT_CacheListObject_GetRange_Async";
            int total = 100;
            await context.Cache.RemoveAsync(key);
            var rl = context.Collections.GetRedisList<User>(key);
            await rl.AddRangeAsync(Enumerable.Range(1, total).Select(i => new User() { Id = i }));

            var r = await rl.GetRangeAsync();
            var range = r.ToList();
            ClassicAssert.AreEqual(total, rl.Count);

            r = await rl.GetRangeAsync(3, 10);
            range = r.ToList();

            ClassicAssert.AreEqual(8, range.Count);
            ClassicAssert.AreEqual(4, range[0].Id);

            r = await rl.GetRangeAsync(10, -10);
            range = r.ToList();
            ClassicAssert.AreEqual(11, range[0].Id);
            ClassicAssert.AreEqual(91, range[range.Count - 1].Id);
        }


        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheDictionaryObject(RedisContext context)
        {
            string key1 = "UT_CacheDictionaryObject1";
            context.Cache.Remove(key1);
            var users = GetUsers();
            var rd = context.Collections.GetRedisDictionary<int, User>(key1);
            // Test AddMultiple
            var usersKv = users.Select(x => new KeyValuePair<int, User>(x.Id, x));
            rd.AddRange(usersKv);
            // Test GetEnumerator
            foreach (var item in rd)
            {
                ClassicAssert.IsTrue(users.Any(u => u.Id == item.Key));
            }
            // Test Count
            ClassicAssert.AreEqual(users.Count, rd.Count);
            // Test ContainsKey
            ClassicAssert.IsTrue(rd.ContainsKey(users[1].Id));
            // Test Contains
            ClassicAssert.IsTrue(rd.Contains(new KeyValuePair<int, User>(users.Last().Id, users.Last())));
            // Test Add
            rd.Add(0, new User() {Id = 0});
            ClassicAssert.AreEqual(users.Count + 1, rd.Count);
            ClassicAssert.AreEqual(0, rd[0].Id);
            // Test Remove
            rd.Remove(0);
            ClassicAssert.IsFalse(rd.ContainsKey(0));
            // Test Keys
            foreach (var k in rd.Keys)
            {
                ClassicAssert.IsTrue(users.Any(u => u.Id == k));
            }
            // Test Values
            foreach (var u in rd.Values)
            {
                ClassicAssert.IsTrue(users.Any(user => user.Id == u.Id));
            }
            // Test TryGetValue
            User userTest = new User();
            bool b = rd.TryGetValue(999, out userTest);
            ClassicAssert.IsFalse(b);
            ClassicAssert.IsNull(userTest);
            b = rd.TryGetValue(1, out userTest);
            ClassicAssert.IsTrue(b);
            ClassicAssert.AreEqual(1, userTest.Id);
            // Test CopyTo
            var array = new KeyValuePair<int, User>[50];
            rd.CopyTo(array, 10);
            ClassicAssert.AreEqual(users.Count, array.Count(x => x.Value != null));
            // Test Clear
            rd.Clear();
            ClassicAssert.AreEqual(0, rd.Count);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheDictionaryTryGetAsync(RedisContext context)
        {
            // Arrange
            string key = "UT_CacheDictionaryTryGetAsync";
            await context.Cache.RemoveAsync(key);
            var users = GetUsers();
            var rd = context.Collections.GetRedisDictionary<int, User>(key);

            // Act
            var usersKv = users.Select(x => new KeyValuePair<int, User>(x.Id, x));
            await rd.AddRangeAsync(usersKv);
            var tryGet1 = await rd.TryGetValueAsync(users[0].Id);
            var tryGet2 = await rd.TryGetValueAsync(-567);

            // Assert 
            ClassicAssert.AreEqual(users.Count, rd.Count);
            ClassicAssert.IsTrue(tryGet1.Found);
            ClassicAssert.AreEqual(users[0].Id, tryGet1.Key);
            ClassicAssert.AreEqual(users[0].Id, tryGet1.Value.Id);
            ClassicAssert.IsFalse(tryGet2.Found);
            ClassicAssert.AreEqual(-567, tryGet2.Key);
            ClassicAssert.IsNull(tryGet2.Value);
        }
        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheDictionaryTryGetAsync_NoDeadlocks(RedisContext context)
        {
            // Arrange
            string key = "UT_CacheDictionaryTryGetAsync_NoDeadlocks";
            await context.Cache.RemoveAsync(key);
            var users = GetUsers();
            var rd = context.Collections.GetRedisDictionary<int, User>(key);

            // Act
            var usersKv = users.Select(x => new KeyValuePair<int, User>(x.Id, x));
            await rd.AddRangeAsync(usersKv);

            var t = Task.Run(() =>
            {
                var singleThreadedSyncCtx = new AsyncContext().SynchronizationContext;
                SynchronizationContext.SetSynchronizationContext(singleThreadedSyncCtx);
                _ = rd.TryGetValueAsync(users[0].Id).Result;
            });

            await Task.Delay(TimeSpan.FromSeconds(1));
            if (!t.IsCompleted)
                throw new Exception("Code has deadlocked."); //usually means you have forgotten a ConfigureAwait(false)
        }


        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheDictionaryObject_TTL(RedisContext context)
        {
            string key1 = "UT_CacheDictionaryObjectTTL1";
            context.Cache.Remove(key1);
            var users = GetUsers();
            var rl = context.Collections.GetRedisDictionary<int, User>(key1);
            rl.AddRange(users.ToDictionary(k => k.Id));
            rl.TimeToLive = TimeSpan.FromMilliseconds(1500);
            ClassicAssert.AreEqual(users.Count, rl.Count);
            Thread.Sleep(2000);
            ClassicAssert.AreEqual(0, rl.Count);
        }

        [Test, TestCaseSource(typeof(Common), "JsonAndRaw")]
        public void UT_CacheDictionaryIncrement(RedisContext context)
        {
            string key = "UT_CacheDictionaryIncrement";
            context.Cache.Remove(key);
            var rl = context.Collections.GetRedisDictionary<string, int>(key);
            var r1 = rl.IncrementBy("h1", 1);
            var r2 = rl.IncrementBy("h2", -2);
            var r3 = rl.IncrementBy("h1", -2);

            var v1 = rl["h1"]; 
            var v2 = rl.GetValue("h2");

            ClassicAssert.AreEqual(1, r1);
            ClassicAssert.AreEqual(-2, r2);
            ClassicAssert.AreEqual(-1, r3);
            ClassicAssert.AreEqual(-1, v1);
            ClassicAssert.AreEqual(-2, v2);
        }


        [Test, TestCaseSource(typeof(Common), "JsonAndRaw")]
        public void UT_CacheDictionaryIncrementFloat(RedisContext context)
        {
            string key = "UT_CacheDictionaryIncrementFloat";
            context.Cache.Remove(key);
            var rl = context.Collections.GetRedisDictionary<string, double>(key);
            var r1 = rl.IncrementByFloat("h1", (double)1.23);
            var r2 = rl.IncrementByFloat("h2", (double)-2.23);
            var r3 = rl.IncrementByFloat("h1", (double)-2.01);
                                   
            var v1 = rl["h1"];
            var v2 = rl.GetValue("h2");

            ClassicAssert.AreEqual(1.23, r1, 0.0001);
            ClassicAssert.AreEqual(-2.23, r2, 0.0001);
            ClassicAssert.AreEqual(1.23-2.01, r3, 0.0001);
            ClassicAssert.AreEqual(1.23-2.01, v1, 0.0001);
            ClassicAssert.AreEqual(-2.23, v2, 0.0001);
        }

        [Test, TestCaseSource(typeof(Common), "JsonAndRaw")]
        public async Task UT_CacheDictionaryIncrementAsync(RedisContext context)
        {
            string key = "UT_CacheDictionaryIncrementAsync";
            await context.Cache.RemoveAsync(key);
            var rl = context.Collections.GetRedisDictionary<string, int>(key);
            var r1 = await rl.IncrementByAsync("h1", 1);
            var r2 = await rl.IncrementByAsync("h2", -2);
            var r3 = await rl.IncrementByAsync("h1", -2);

            var v1 = rl["h1"];
            var v2 = await rl.GetValueAsync("h2");

            ClassicAssert.AreEqual(1, r1);
            ClassicAssert.AreEqual(-2, r2);
            ClassicAssert.AreEqual(-1, r3);
            ClassicAssert.AreEqual(-1, v1);
            ClassicAssert.AreEqual(-2, v2);
        }

        [Test, TestCaseSource(typeof(Common), "JsonAndRaw")]
        public async Task UT_CacheDictionaryIncrementFloatAsync(RedisContext context)
        {
            string key = "UT_CacheDictionaryIncrementFloatAsync";
            context.Cache.Remove(key);
            var rl = context.Collections.GetRedisDictionary<string, double>(key);
            var r1 = await rl.IncrementByFloatAsync("h1", (double)1.23);
            var r2 = await rl.IncrementByFloatAsync("h2", (double)-2.23);
            var r3 = await rl.IncrementByFloatAsync("h1", (double)-2.01);

            var v1 = rl["h1"];
            var v2 = await rl.GetValueAsync("h2");

            ClassicAssert.AreEqual(1.23, r1, 0.0001);
            ClassicAssert.AreEqual(-2.23, r2, 0.0001);
            ClassicAssert.AreEqual(1.23 - 2.01, r3, 0.0001);
            ClassicAssert.AreEqual(1.23 - 2.01, v1, 0.0001);
            ClassicAssert.AreEqual(-2.23, v2, 0.0001);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheDictionaryObjectAsync_NoDeadlocks(RedisContext context)
        {
            string key1 = "UT_CacheDictionaryObjectAsync_NoDeadlocks";
            context.Cache.Remove(key1);
            var users = GetUsers();
            var rd = context.Collections.GetRedisDictionary<int, User>(key1);

            // Test AddMultiple
            await Common.TestDeadlock(() =>
            {
                var usersKv = users.Select(x => new KeyValuePair<int, User>(x.Id, x));
                rd.AddRangeAsync(usersKv).Wait();
            });

            // Test Count
            await Common.TestDeadlock(() =>
            {
                _ = rd.GetCountAsync().Result;
            });

            // Test ContainsKey
            await Common.TestDeadlock(() =>
            {
                _ = rd.ContainsKeyAsync(users[1].Id).Result;
            });

            // Test Contains
            await Common.TestDeadlock(() =>
            {
                _ = rd.ContainsAsync(new KeyValuePair<int, User>(users.Last().Id, users.Last())).Result;
            });

            // Test Add
            await Common.TestDeadlock(() =>
            {
                _ = rd.AddAsync(0, new User() { Id = 0 });
            });

            // Test Remove
            await Common.TestDeadlock(() =>
            {
                _ = rd.RemoveAsync(0).Result;
            });
            // Test Clear
            await Common.TestDeadlock(() =>
            {
                rd.ClearAsync().Wait();
            });
        }
        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheDictionaryAddRangeAsync_NoDeadlocks(RedisContext context)
        {
            string key1 = "UT_CacheDictionaryAddRangeAsync_NoDeadlocks";
            context.Cache.Remove(key1);
            var users = GetUsers();
            var rd = context.Collections.GetRedisDictionary<int, User>(key1);

            // Test AddMultiple
            await Common.TestDeadlock(() =>
            {
                var usersKv = users.Select(x => new KeyValuePair<int, User>(x.Id, x));
                rd.AddRangeAsync(usersKv).Wait();
            });
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheDictionaryGetCountAsync_NoDeadlocks(RedisContext context)
        {
            string key1 = "UT_CacheDictionaryGetCountAsync_NoDeadlocks";
            context.Cache.Remove(key1);
            var users = GetUsers();
            var rd = context.Collections.GetRedisDictionary<int, User>(key1);

            

            // Test Count
            await Common.TestDeadlock(() =>
            {
                _ = rd.GetCountAsync().Result;
            });
        }
        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheDictionaryContainsKeyAsync_NoDeadlocks(RedisContext context)
        {
            string key1 = "UT_CacheDictionaryObjectAsync";
            context.Cache.Remove(key1);
            var users = GetUsers();
            var rd = context.Collections.GetRedisDictionary<int, User>(key1);


            // Test ContainsKey
            await Common.TestDeadlock(() =>
            {
                _ = rd.ContainsKeyAsync(users[1].Id).Result;
            });
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheDictionaryContainsAsync_NoDeadlocks(RedisContext context)
        {
            string key1 = "UT_CacheDictionaryContainsAsync_NoDeadlocks";
            context.Cache.Remove(key1);
            var users = GetUsers();
            var rd = context.Collections.GetRedisDictionary<int, User>(key1);


            // Test Contains
            await Common.TestDeadlock(() =>
            {
                _ = rd.ContainsAsync(new KeyValuePair<int, User>(users.Last().Id, users.Last())).Result;
            });

        }

        

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheDictionaryAddAsync_NoDeadlocks(RedisContext context)
        {
            string key1 = "UT_CacheDictionaryAddAsync_NoDeadlocks";
            context.Cache.Remove(key1);
            var users = GetUsers();
            var rd = context.Collections.GetRedisDictionary<int, User>(key1);

            // Test Add
            await Common.TestDeadlock(() =>
            {
                rd.AddAsync(0, new User() { Id = 0 }).Wait();
            });
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheDictionaryRemoveAsync_NoDeadlocks(RedisContext context)
        {
            string key1 = "UT_CacheDictionaryRemoveAsync_NoDeadlocks";
            context.Cache.Remove(key1);
            var users = GetUsers();
            var rd = context.Collections.GetRedisDictionary<int, User>(key1);

            // Test Remove
            await Common.TestDeadlock(() =>
            {
                _ = rd.RemoveAsync(0).Result;
            });
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheDictionaryClearAsync_NoDeadlocks(RedisContext context)
        {
            string key1 = "UT_CacheDictionaryClearAsync_NoDeadlocks";
            context.Cache.Remove(key1);
            var users = GetUsers();
            var rd = context.Collections.GetRedisDictionary<int, User>(key1);

            // Test Clear
            await Common.TestDeadlock(() =>
            {
                rd.ClearAsync().Wait();
            });
        }

        

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheDictionaryObjectAsync(RedisContext context)
        {
            string key1 = "UT_CacheDictionaryObjectAsync";
            context.Cache.Remove(key1);
            var users = GetUsers();
            var rd = context.Collections.GetRedisDictionary<int, User>(key1);
            // Test AddMultiple
            var usersKv = users.Select(x => new KeyValuePair<int, User>(x.Id, x));
            await rd.AddRangeAsync(usersKv);
            // Test GetEnumerator
            foreach (var item in rd)
            {
                ClassicAssert.IsTrue(users.Any(u => u.Id == item.Key));
            }
            // Test Count
            ClassicAssert.AreEqual(users.Count, await rd.GetCountAsync());
            // Test ContainsKey
            ClassicAssert.IsTrue(await rd.ContainsKeyAsync(users[1].Id));
            // Test Contains
            ClassicAssert.IsTrue(await rd.ContainsAsync(new KeyValuePair<int, User>(users.Last().Id, users.Last())));
            // Test Add
            await rd.AddAsync(0, new User() { Id = 0 });
            ClassicAssert.AreEqual(users.Count + 1, await rd.GetCountAsync());
            ClassicAssert.AreEqual(0, rd[0].Id);
            // Test Remove
            await rd.RemoveAsync(0);
            ClassicAssert.IsFalse(await rd.ContainsKeyAsync(0));
            // Test Keys
            foreach (var k in rd.Keys)
            {
                ClassicAssert.IsTrue(users.Any(u => u.Id == k));
            }
            // Test Values
            foreach (var u in rd.Values)
            {
                ClassicAssert.IsTrue(users.Any(user => user.Id == u.Id));
            }
            // Test TryGetValue
            User userTest = new User();
            bool b = rd.TryGetValue(999, out userTest);
            ClassicAssert.IsFalse(b);
            ClassicAssert.IsNull(userTest);
            b = rd.TryGetValue(1, out userTest);
            ClassicAssert.IsTrue(b);
            ClassicAssert.AreEqual(1, userTest.Id);
            // Test CopyTo
            var array = new KeyValuePair<int, User>[50];
            rd.CopyTo(array, 10);
            ClassicAssert.AreEqual(users.Count, array.Count(x => x.Value != null));
            // Test Clear
            await rd.ClearAsync();
            ClassicAssert.AreEqual(0, await rd.GetCountAsync());
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheDictionaryObject_TTLAsync(RedisContext context)
        {
            string key1 = "UT_CacheDictionaryObject_TTLAsync";
            context.Cache.Remove(key1);
            var users = GetUsers();
            var rl = context.Collections.GetRedisDictionary<int, User>(key1);
            await rl.AddRangeAsync(users.ToDictionary(k => k.Id));
            rl.TimeToLive = TimeSpan.FromMilliseconds(1500);
            ClassicAssert.AreEqual(users.Count, await rl.GetCountAsync());
            Thread.Sleep(2000);
            ClassicAssert.AreEqual(0, await rl.GetCountAsync());
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheDictionaryObject_AddAsyncWithTags(RedisContext context)
        {
            string key1 = "UT_CacheDictionaryObject_AddAsyncWithTags";
            string tag1 = "UT_CacheDictionaryObject_AddAsyncWithTags_TAG1";
            context.Cache.Remove(key1);
            context.Cache.InvalidateKeysByTag(tag1);
            var users = GetUsers();
            var rl = context.Collections.GetRedisDictionary<int, User>(key1);
            await rl.AddAsync(1, users[0], new[] { tag1 });
            var keys = context.Cache.GetKeysByTag(new[] { tag1 }, true).ToList();
            ClassicAssert.AreEqual(1, keys.Count);
            var val = Encoding.UTF8.GetString(context.GetSerializer().Serialize(1));
            ClassicAssert.AreEqual("UT_CacheDictionaryObject_AddAsyncWithTags:$_->_$:" + val, keys[0]);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheDictionaryObject_AddAsyncWithTags_NoDeadlocks(RedisContext context)
        {
            string key1 = "UT_CacheDictionaryObject_AddAsyncWithTags_NoDeadlocks";
            string tag1 = "UT_CacheDictionaryObject_AddAsyncWithTags_NoDeadlocks_TAG1";
            context.Cache.Remove(key1);
            context.Cache.InvalidateKeysByTag(tag1);
            var users = GetUsers();
            var rl = context.Collections.GetRedisDictionary<int, User>(key1);

            await Common.TestDeadlock(() =>
            {
                rl.AddAsync(1, users[0], new[] { tag1 }).Wait();
            });
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheDictionaryObject_AddRangeWithTags(RedisContext context)
        {
            var key = "UT_CacheDictionaryObject_AddRangeWithTags";
            var tags = new[] { "UT_CacheDictionaryObject_AddRangeWithTags_TAG1" };
            context.Cache.Remove(key);
            context.Cache.InvalidateKeysByTag(tags);

            var redisDict = context.Collections.GetRedisDictionary<int, User>(key);
            var users = GetUsers();
            redisDict.AddRange(users.ToDictionary(k => k.Id), tags);

            var ser = context.GetSerializer();
            var members = context.Cache.GetMembersByTag(tags[0]).OrderBy(x => ser.Deserialize<int>(x.MemberValue)).ToList();

            ClassicAssert.AreEqual(users.Count, members.Count);
            ClassicAssert.AreEqual(key, members[0].Key);
            ClassicAssert.AreEqual(TagMemberType.HashField, members[0].MemberType);
            ClassicAssert.AreEqual(1, ser.Deserialize<int>(members[0].MemberValue));
            ClassicAssert.AreEqual(2, ser.Deserialize<int>(members[1].MemberValue));
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheDictionaryObject_AddRangeWithTags_Async(RedisContext context)
        {
            var key = "UT_CacheDictionaryObject_AddRangeWithTags_Async";
            var tags = new[] { "UT_CacheDictionaryObject_AddRangeWithTags_Async_TAG1" };
            await context.Cache.RemoveAsync(key);
            await context.Cache.InvalidateKeysByTagAsync(tags);

            var redisDict = context.Collections.GetRedisDictionary<int, User>(key);
            var users = GetUsers();
            await redisDict.AddRangeAsync(users.ToDictionary(k => k.Id), tags);

            var ser = context.GetSerializer();
            var members = context.Cache.GetMembersByTag(tags[0]).OrderBy(x => ser.Deserialize<int>(x.MemberValue)).ToList();

            ClassicAssert.AreEqual(users.Count, members.Count);
            ClassicAssert.AreEqual(key, members[0].Key);
            ClassicAssert.AreEqual(TagMemberType.HashField, members[0].MemberType);
            ClassicAssert.AreEqual(1, ser.Deserialize<int>(members[0].MemberValue));
            ClassicAssert.AreEqual(2, ser.Deserialize<int>(members[1].MemberValue));
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheDictionaryObject_AddRangeWithTags_Async_NoDeadlocks(RedisContext context)
        {
            var key = "UT_CacheDictionaryObject_AddRangeWithTags_Async_NoDeadlocks";
            var tags = new[] { "UT_CacheDictionaryObject_AddRangeWithTags_Async_NoDeadlocksTAG1" };
            await Common.TestDeadlock(() =>
            {
                _ = context.Cache.RemoveAsync(key).Result;
            });

            await Common.TestDeadlock(() =>
            {
                context.Cache.InvalidateKeysByTagAsync(tags).Wait();
            });

            var redisDict = context.Collections.GetRedisDictionary<int, User>(key);
            var users = GetUsers();
            await Common.TestDeadlock(() =>
            {
                redisDict.AddRangeAsync(users.ToDictionary(k => k.Id), tags).Wait();
            });            
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheList_EXP(RedisContext context)
        {
            string key = "UT_CacheList_EXP";
            context.Cache.Remove(key);
            var set = context.Collections.GetRedisSet<string>(key);
            set.AddRange(new [] { "test1", "test2", "test3" });
            var realLocalExp = DateTime.UtcNow.AddSeconds(2);
            set.Expiration = realLocalExp;
            var exp = set.Expiration.Value.ToUniversalTime();
            var startedOn = DateTime.Now;
            ClassicAssert.AreEqual(3, set.Count);
            while (context.Cache.KeyExists(key) && DateTime.Now < startedOn.AddSeconds(10))
            {
                Thread.Sleep(100);
            }
            ClassicAssert.IsFalse(context.Cache.KeyExists(key));
            ClassicAssert.AreEqual(0, set.Count);
            var stoppedOn = DateTime.Now;
            var span = stoppedOn - startedOn;
            ClassicAssert.AreEqual(2, span.TotalSeconds, 1);
            ClassicAssert.AreEqual(0, (exp - realLocalExp).TotalSeconds, 1);
        }

        [Test, TestCaseSource(typeof(Common), "Json")]
        public void UT_CacheList_EXP_Expired(RedisContext context)
        {
            string key = "UT_CacheList_EXP_Expired";
            context.Cache.Remove(key);
            var set = context.Collections.GetRedisSet<string>(key);
            set.AddRange(new[] { "test1", "test2", "test3" });
            ClassicAssert.AreEqual(3, set.Count);
            var realLocalExp = DateTime.UtcNow.AddDays(-4);
            set.Expiration = realLocalExp;
            Thread.Sleep(250);
            ClassicAssert.IsFalse(context.Cache.KeyExists(key));
            ClassicAssert.AreEqual(0, set.Count);
        }

#if (NET462)
        [Test, TestCaseSource(typeof(Common), "Bin")]
        public void UT_CacheList_StrObj(RedisContext context)
        {
            string key = "UT_CacheList_StrObj";
            context.Cache.Remove(key);
            context.Cache.SetObject<string>(key, "test value 1");
            var obj = context.Cache.GetObject<object>(key);
            ClassicAssert.IsTrue(obj is string);
            context.Cache.SetObject<string>(key, Encoding.UTF8.GetString(new byte[] { 0x1f, 0x8b }));
            obj = context.Cache.GetObject<object>(key);
            ClassicAssert.IsTrue(obj is string);
        }
#endif

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheSetObject(RedisContext context)
        {
            string key1 = "UT_CacheSetObject1";
            context.Cache.Remove(key1);
            var users = GetUsers();
            var rs = context.Collections.GetRedisSet<User>(key1);
            rs.AddRange(users);
            // Test GetEnumerator
            foreach (var item in rs)
            {
                ClassicAssert.IsTrue(users.Any(u => u.Id == item.Id));
            }
            // Test Count
            ClassicAssert.AreEqual(users.Count, rs.Count);
            // Test Contains
            ClassicAssert.IsTrue(rs.Contains(users[2]));
            // Test Add
            var newUser = new User() {Id = 5};
            rs.Add(newUser);
            ClassicAssert.IsTrue(rs.Contains(newUser));
            // Test Remove
            rs.Remove(users[2]);
            ClassicAssert.IsFalse(rs.Contains(users[2]));
            // Test CopyTo
            User[] array = new User[50];
            rs.CopyTo(array, 10);
            ClassicAssert.AreEqual(users.Count, array.Count(x => x != null));
            // Test Clear
            rs.Clear();
            ClassicAssert.AreEqual(0, rs.Count);
            rs.AddRange(new []{  new User() {Id = 3},  new User() {Id = 1},  new User() {Id = 2} });
            ClassicAssert.AreEqual(3, rs.Count);
            rs.Remove(new User() { Id = 1 });
            rs.Remove(new User() { Id = 2 });
            ClassicAssert.AreEqual(1, rs.Count);
            ClassicAssert.IsTrue(rs.Contains(new User() { Id = 3 }));
            // Test GetRandomMember
            var user = rs.GetRandomMember();
            ClassicAssert.AreEqual(3, user.Id);
            // Test Pop
            user = rs.Pop();
            ClassicAssert.AreEqual(3, user.Id);
            user = rs.Pop();
            ClassicAssert.IsNull(user);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheSetObject_TTL(RedisContext context)
        {
            string key1 = "UT_CacheSetObject_TTL";
            context.Cache.Remove(key1);
            var users = GetUsers();
            var rl = context.Collections.GetRedisSet<User>(key1);
            rl.AddRange(users);
            rl.TimeToLive = TimeSpan.FromMilliseconds(1500);
            ClassicAssert.AreEqual(users.Count, rl.Count);
            Thread.Sleep(2000);
            ClassicAssert.AreEqual(0, rl.Count);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheSetObject_SetModifiers(RedisContext context)
        {
            string keyAbc = "UT_CacheSetObject_SetModifiers_ABC";
            string keyCde = "UT_CacheSetObject_SetModifiers_CDE";

            context.Cache.Remove(keyAbc);
            context.Cache.Remove(keyCde);
            var abcSet = context.Collections.GetRedisSet<char>(keyAbc);
            abcSet.AddRange("ABC");
            
            var cdeSet = context.Collections.GetRedisSet<char>(keyCde);
            cdeSet.AddRange("CDE");

            // Test Count
            ClassicAssert.AreEqual(3, abcSet.Count);
            ClassicAssert.AreEqual(3, cdeSet.Count);

            abcSet.Clear();
            cdeSet.Clear();
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheSortedSet_GetRange(RedisContext context)
        {
            var key = "UT_CacheSortedSet_GetRange";
            context.Cache.Remove(key);
            var ss = context.Collections.GetRedisSortedSet<User>(key);
            var users = GetUsers();

            ss.Add(double.NegativeInfinity, users[3]);
            ss.Add(double.PositiveInfinity, users[2]);
            ss.Add(12.34, users[0]);
            ss.Add(23.45, users[1]);

            var count = ss.Count();
            var byRank = ss.GetRangeByRank().ToList();
            ClassicAssert.AreEqual(4, count);
            ClassicAssert.AreEqual(count, byRank.Count);
            ClassicAssert.AreEqual(12.34, byRank[1].Score);
            ClassicAssert.AreEqual(double.NegativeInfinity, byRank[0].Score);

            var byScore = ss.GetRangeByScore(12.34, 23.449).ToList();
            ClassicAssert.AreEqual(1, byScore.Count);
            ClassicAssert.AreEqual(users[0].Id, byScore[0].Value.Id);

            byScore = ss.GetRangeByScore(12.34, 23.45).ToList();
            ClassicAssert.AreEqual(2, byScore.Count);
            ClassicAssert.AreEqual(users[1].Id, byScore[1].Value.Id);
        }

        [Test, TestCaseSource(typeof(Common), "Json")]
        public void UT_CacheSortedSet_SE_Issue287(RedisContext context)
        {
            var key = "UT_CacheSortedSet_SE_Issue287";
            context.Cache.Remove(key);
            var ss = context.Collections.GetRedisSortedSet<User>(key);
            var users = GetUsers();

            ss.Add(double.NegativeInfinity, users[3]);
            ss.Add(double.PositiveInfinity, users[2]);

            var byRank = ss.GetRangeByRank().ToList();
            ClassicAssert.AreEqual(double.NegativeInfinity, byRank[0].Score);
            // This is a StackExhange.Redis bug: https://github.com/StackExchange/StackExchange.Redis/issues/287
            // This was corrected and should be included in the next SE.Redis version
            ClassicAssert.AreEqual(double.NegativeInfinity, byRank[0].Score);
            ClassicAssert.AreEqual(double.PositiveInfinity, byRank[1].Score);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheSortedSet_GetRangeByRankNegative(RedisContext context)
        {
            var key = "UT_CacheSortedSet_GetRangeByRankNegative";
            context.Cache.Remove(key);
            var ss = context.Collections.GetRedisSortedSet<string>(key);
            ss.AddRange(new[] { new SortedMember<string>(33, "c"), new SortedMember<string>(0, "a"), new SortedMember<string>(22, "b") });

            var byRank = ss.GetRangeByRank(-2, -1).ToList();
            var byRankRev = ss.GetRangeByRank(-2, -1, true).ToList();

            ClassicAssert.AreEqual(2, byRank.Count);
            ClassicAssert.AreEqual("b", byRank[0].Value);
            ClassicAssert.AreEqual("c", byRank[1].Value);
            ClassicAssert.AreEqual("a", byRankRev[1].Value);
            ClassicAssert.AreEqual("b", byRankRev[0].Value);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheSortedSet_etc(RedisContext context)
        {
            var key = "UT_CacheSortedSet_etc";
            var ss = context.Collections.GetRedisSortedSet<string>(key);
            context.Cache.Remove(key);
            for (int i = 0; i < 255; i++)
            {
                ss.Add(i, "member " + i);
            }
            ClassicAssert.AreEqual(10, ss.CountByScore(1, 10));
            var incremented = ss.IncrementScore("member 10", 1000);
            ClassicAssert.AreEqual(1010, incremented);
            ClassicAssert.AreEqual(255, ss.Count);

            int x = 0;
            foreach (var item in ss)
            {
                x++;
            }
            ClassicAssert.AreEqual(255, x);

            var r0 = ss.RankOf("member 0");
            var r9 = ss.RankOf("member 9");
            var r10 = ss.RankOf("member 10");
            var r11 = ss.RankOf("member 11");
            var r254 = ss.RankOf("member 254");
            var r255 = ss.RankOf("member 255");
            var r0Rev = ss.RankOf("member 0", true);

            ClassicAssert.AreEqual(0, r0);
            ClassicAssert.AreEqual(9, r9);
            ClassicAssert.AreEqual(10, r11);
            ClassicAssert.AreEqual(254, r10);
            ClassicAssert.AreEqual(253, r254);
            ClassicAssert.AreEqual(254, r0Rev);
            ClassicAssert.IsNull(r255);

            var s0 = ss.ScoreOf("member 0");
            var s254 = ss.ScoreOf("member 254");
            var s255 = ss.ScoreOf("member 255");
            ClassicAssert.AreEqual(0, s0);
            ClassicAssert.AreEqual(254, s254);
            ClassicAssert.IsNull(s255);

            ss.RemoveRangeByRank(0, 2);
            ClassicAssert.AreEqual(252, ss.Count);

            ss.RemoveRangeByScore(double.NegativeInfinity, 9);
            ClassicAssert.AreEqual(245, ss.Count);

            ClassicAssert.IsTrue(ss.Contains("member 100"));
            ClassicAssert.IsFalse(ss.Contains("member 0"));

            ss.Remove("member 100");
            ClassicAssert.IsFalse(ss.Contains("member 100"));

            ss.Clear();
            ClassicAssert.AreEqual(0, ss.Count);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheSortedSet_GetRangeAsync(RedisContext context)
        {
            var key = "UT_CacheSortedSet_GetRangeAsync";
            await context.Cache.RemoveAsync(key);
            var ss = context.Collections.GetRedisSortedSet<User>(key);
            var users = GetUsers();

            await ss.AddAsync(double.NegativeInfinity, users[3]);
            await ss.AddAsync(double.PositiveInfinity, users[2]);
            await ss.AddAsync(12.34, users[0]);
            await ss.AddAsync(23.45, users[1]);

            var count = await ss.CountAsync();
            var byRank = (await ss.GetRangeByRankAsync()).ToList();
            ClassicAssert.AreEqual(4, count);
            ClassicAssert.AreEqual(count, byRank.Count);
            ClassicAssert.AreEqual(12.34, byRank[1].Score);
            ClassicAssert.AreEqual(double.NegativeInfinity, byRank[0].Score);

            var byScore = (await ss.GetRangeByScoreAsync(12.34, 23.449)).ToList();
            ClassicAssert.AreEqual(1, byScore.Count);
            ClassicAssert.AreEqual(users[0].Id, byScore[0].Value.Id);

            byScore = (await ss.GetRangeByScoreAsync(12.34, 23.45)).ToList();
            ClassicAssert.AreEqual(2, byScore.Count);
            ClassicAssert.AreEqual(users[1].Id, byScore[1].Value.Id);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheSortedSet_GetRangeByRankNegativeAsync(RedisContext context)
        {
            var key = "UT_CacheSortedSet_GetRangeByRankNegativeAsync";
            await context.Cache.RemoveAsync(key);
            var ss = context.Collections.GetRedisSortedSet<string>(key);
            await ss.AddRangeAsync(new[] { new SortedMember<string>(33, "c"), new SortedMember<string>(0, "a"), new SortedMember<string>(22, "b") });

            var byRank = (await ss.GetRangeByRankAsync(-2, -1)).ToList();
            var byRankRev = (await ss.GetRangeByRankAsync(-2, -1, true)).ToList();

            ClassicAssert.AreEqual(2, byRank.Count);
            ClassicAssert.AreEqual("b", byRank[0].Value);
            ClassicAssert.AreEqual("c", byRank[1].Value);
            ClassicAssert.AreEqual("a", byRankRev[1].Value);
            ClassicAssert.AreEqual("b", byRankRev[0].Value);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheSortedSet_etcAsync(RedisContext context)
        {
            var key = "UT_CacheSortedSet_etcAsync";
            var ss = context.Collections.GetRedisSortedSet<string>(key);
            await context.Cache.RemoveAsync(key);
            for (int i = 0; i < 255; i++)
            {
                await ss.AddAsync(i, "member " + i);
            }
            ClassicAssert.AreEqual(10, await ss.CountByScoreAsync(1, 10));
            var incremented = await ss.IncrementScoreAsync("member 10", 1000);
            ClassicAssert.AreEqual(1010, incremented);
            ClassicAssert.AreEqual(255, ss.Count);

            int x = 0;
            foreach (var item in ss)
            {
                x++;
            }
            ClassicAssert.AreEqual(255, x);

            var r0 = await ss.RankOfAsync("member 0");
            var r9 = await ss.RankOfAsync("member 9");
            var r10 = await ss.RankOfAsync("member 10");
            var r11 = await ss.RankOfAsync("member 11");
            var r254 = await ss.RankOfAsync("member 254");
            var r255 = await ss.RankOfAsync("member 255");
            var r0Rev = await ss.RankOfAsync("member 0", true);

            ClassicAssert.AreEqual(0, r0);
            ClassicAssert.AreEqual(9, r9);
            ClassicAssert.AreEqual(10, r11);
            ClassicAssert.AreEqual(254, r10);
            ClassicAssert.AreEqual(253, r254);
            ClassicAssert.AreEqual(254, r0Rev);
            ClassicAssert.IsNull(r255);

            var s0 = await ss.ScoreOfAsync("member 0");
            var s254 = await ss.ScoreOfAsync("member 254");
            var s255 = await ss.ScoreOfAsync("member 255");
            ClassicAssert.AreEqual(0, s0);
            ClassicAssert.AreEqual(254, s254);
            ClassicAssert.IsNull(s255);

            await ss.RemoveRangeByRankAsync(0, 2);
            ClassicAssert.AreEqual(252, await ss.CountAsync());

            await ss.RemoveRangeByScoreAsync(double.NegativeInfinity, 9);
            ClassicAssert.AreEqual(245, await ss.CountAsync());

            ClassicAssert.IsTrue(await ss.ContainsAsync("member 100"));
            ClassicAssert.IsFalse(await ss.ContainsAsync("member 0"));

            await ss.RemoveAsync("member 100");
            ClassicAssert.IsFalse(await ss.ContainsAsync("member 100"));

            await ((RedisBaseObject)ss).ClearAsync();
            ClassicAssert.AreEqual(0, await ss.CountAsync());
        }

        [Test, TestCaseSource(typeof(Common), "Raw")]
        public void UT_CacheBitmap(RedisContext context)
        {
            var key = "UT_CacheBitmap";
            context.Cache.Remove(key);
            var bm = context.Collections.GetRedisBitmap(key);
            bm.Add(0xff);           // 11111111 
            ClassicAssert.IsFalse(bm.Contains(0));
            bm.Add(0x00);          // 11111111 00000000
            ClassicAssert.IsTrue(bm.Contains(0));
            bm.Add(0xff);           // 11111111 00000000 11111111
            bm.SetBit(24, 1);    // 11111111 00000000 11111111 10000000
            bm.SetBit(25, 0);   // 11111111 00000000 11111111 10000000
            bm.SetBit(26, 1);    // 11111111 00000000 11111111 10100000
            bm.SetBit(27, 1);    // 11111111 00000000 11111111 10110000
            ClassicAssert.AreEqual(19, bm.Count());
            ClassicAssert.IsFalse(bm.Contains(0, 0, 0));
            ClassicAssert.IsFalse(bm.Contains(1, 1, 1));
            ClassicAssert.IsTrue(bm.Contains(0, -1, -1));
            ClassicAssert.IsTrue(bm.Contains(1, -1, -1));
            ClassicAssert.AreEqual(0, bm.BitPosition(1));
            ClassicAssert.AreEqual(8, bm.BitPosition(0));
            ClassicAssert.AreEqual(24+1, bm.BitPosition(0, -1, -1));
            ClassicAssert.AreEqual(24+0, bm.BitPosition(1, -1, -1));
            ClassicAssert.AreEqual(0, bm.GetBit(25));
            ClassicAssert.AreEqual(1, bm.GetBit(26));
            ClassicAssert.AreEqual(1, bm.GetBit(27));
            ClassicAssert.AreEqual(3, bm.Count(-1, -1));
            ClassicAssert.AreEqual(11, bm.Count(-2, -1));
            ClassicAssert.AreEqual(11, bm.Count(2, -1));
            ClassicAssert.AreEqual(19, bm.Count());
            ClassicAssert.AreEqual(8, bm.Count(0, 1));
            var sb = new StringBuilder();
            foreach (var bit in bm)
            {
                sb.Append(bit);
            }
            ClassicAssert.AreEqual("11111111000000001111111110110000", sb.ToString());
            ClassicAssert.AreEqual("11111111000000001111111110110000", bm.ToString());
            bm.SetBit(9999999, 1);
            ClassicAssert.AreEqual(20, bm.Count());
            ClassicAssert.AreEqual(0, bm.GetBit(9999998));
            ClassicAssert.AreEqual(1, bm.GetBit(9999999));    
            bm.Clear();
            ClassicAssert.AreEqual(0, bm.Count());
            bm.Remove(0);
            ClassicAssert.AreEqual(1, bm.Count());
            ClassicAssert.AreEqual(1, bm.GetBit(0));
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheLexSet(RedisContext context)
        {
            var key = "UT_CacheLexSet";
            context.Cache.Remove(key);
            var bm = context.Collections.GetRedisLexicographicSet(key);
            bm.Add("zero");
            bm.AddRange(new [] { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve" });

            ClassicAssert.AreEqual(13, bm.Count);
            var suggestions = bm.AutoComplete("t", 3).ToList();
            ClassicAssert.AreEqual(3, suggestions.Count);
            ClassicAssert.AreEqual("ten", suggestions[0]);
            ClassicAssert.AreEqual("three", suggestions[1]);
            ClassicAssert.AreEqual("twelve", suggestions[2]);

            suggestions = bm.AutoComplete("tw").ToList();
            ClassicAssert.AreEqual(2, suggestions.Count);
            ClassicAssert.AreEqual("twelve", suggestions[0]);
            ClassicAssert.AreEqual("two", suggestions[1]);

            var lst = new List<string>();
            foreach (var s in bm)
            {
                lst.Add(s);
            }
            ClassicAssert.AreEqual(13, lst.Count);
            ClassicAssert.AreEqual("eight", lst[0]);
            ClassicAssert.AreEqual("eleven", lst[1]);
            ClassicAssert.AreEqual("two", lst[11]);
            ClassicAssert.AreEqual("zero", lst[12]);

            ClassicAssert.IsTrue(bm.Contains("zero"));
            bm.Remove("zero");
            ClassicAssert.IsFalse(bm.Contains("zero"));
            ClassicAssert.AreEqual(12, bm.Count);
        }

        // See this: https://github.com/StackExchange/StackExchange.Redis/issues/458
        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheLexSet_Match(RedisContext context)
        {
            var key = "UT_CacheLexSet_Match";
            context.Cache.Remove(key);
            var bm = context.Collections.GetRedisLexicographicSet(key);
            bm.AddRange(new[] { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve" });
            ClassicAssert.AreEqual(12, bm.Match("*").Count());

            var lst = bm.Match("*eve*").ToList();
            ClassicAssert.AreEqual(2, lst.Count);
            ClassicAssert.AreEqual("eleven", lst[0]);
            ClassicAssert.AreEqual("seven", lst[1]);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheString(RedisContext context)
        {
            var key = "UT_CacheString";
            context.Cache.Remove(key);
            var cs = context.Collections.GetRedisString(key);

            cs.SetRange(3, "Test");
            ClassicAssert.AreEqual(7, cs.Length);
            ClassicAssert.AreEqual("\0", cs.GetRange(0, 0));
            ClassicAssert.AreEqual("\0", cs[0, 0]);
            ClassicAssert.AreEqual("T", cs[3, 3]);
            ClassicAssert.AreEqual("t", cs[6, 6]);
            ClassicAssert.AreEqual("Test", cs[3, -1]);
            ClassicAssert.AreEqual("\0\0\0Test", cs[0, -1]);
            ClassicAssert.AreEqual("\0\0\0Test", cs[0, 999]);

            var len = cs.SetRange(0, "123");
            ClassicAssert.AreEqual(7, len);
            ClassicAssert.AreEqual(7, cs.Length);
            ClassicAssert.AreEqual("123Test", cs.GetRange());
            
            cs.SetRange(0, "abc");

            var lst = new List<byte>();
            foreach (byte b in cs)
            {
                lst.Add(b);
            }
            ClassicAssert.AreEqual("abcTest", Encoding.UTF8.GetString(lst.ToArray()));

            ClassicAssert.AreEqual(10, cs.Append("def"));
            ClassicAssert.AreEqual("abcTestdef", cs.ToString());

            cs.Set("new string");
            ClassicAssert.AreEqual("new string", cs.ToString());

            cs.Set(123.45);
            ClassicAssert.AreEqual("123.45", cs.ToString());

            cs.Set(12345);
            ClassicAssert.AreEqual("12345", cs.ToString());
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheStringAsync(RedisContext context)
        {
            var key = "UT_CacheStringAsync";
            await context.Cache.RemoveAsync(key);
            var cs = context.Collections.GetRedisString(key);

            await cs.SetRangeAsync(3, "Test");
            ClassicAssert.AreEqual(7, cs.Length);
            ClassicAssert.AreEqual("\0", await cs.GetRangeAsync(0, 0));
            ClassicAssert.AreEqual("\0", cs[0, 0]);
            ClassicAssert.AreEqual("T", cs[3, 3]);
            ClassicAssert.AreEqual("t", cs[6, 6]);
            ClassicAssert.AreEqual("Test", cs[3, -1]);
            ClassicAssert.AreEqual("\0\0\0Test", cs[0, -1]);
            ClassicAssert.AreEqual("\0\0\0Test", cs[0, 999]);

            var len = await cs.SetRangeAsync(0, "123");
            ClassicAssert.AreEqual(7, len);
            ClassicAssert.AreEqual(7, cs.Length);
            ClassicAssert.AreEqual("123Test", await cs.GetRangeAsync());

            await cs.SetRangeAsync(0, "abc");

            var lst = new List<byte>();
            foreach (byte b in cs)
            {
                lst.Add(b);
            }
            ClassicAssert.AreEqual("abcTest", Encoding.UTF8.GetString(lst.ToArray()));

            ClassicAssert.AreEqual(10, await cs.AppendAsync("def"));
            ClassicAssert.AreEqual("abcTestdef", await cs.ToStringAsync());

            await cs.SetAsync("new string");
            ClassicAssert.AreEqual("new string", await cs.ToStringAsync());

            await cs.SetAsync(123.45);
            ClassicAssert.AreEqual("123.45", await cs.ToStringAsync());

            await cs.SetAsync(12345);
            ClassicAssert.AreEqual("12345", await cs.ToStringAsync());
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheStringGetSet(RedisContext context)
        {
            var key = "UT_CacheStringGetSet";
            context.Cache.Remove(key);
            var cs = context.Collections.GetRedisString(key);
            var str = cs.GetSet("value");
            ClassicAssert.IsNull(str);
            str = cs.GetSet("new value");
            ClassicAssert.AreEqual("value", str);
            context.Cache.Remove(key);
            var integer = cs.GetSet(456);
            ClassicAssert.AreEqual(0, integer);
            ClassicAssert.AreEqual(456, cs.AsInteger());
            var fp = cs.GetSet(789.12);
            ClassicAssert.AreEqual(456.0, fp);
            ClassicAssert.AreEqual(789.12, cs.AsFloat());
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheStringGetSetAsync(RedisContext context)
        {
            var key = "UT_CacheStringGetSetAsync";
            await context.Cache.RemoveAsync(key);
            var cs = context.Collections.GetRedisString(key);
            var str = await cs.GetSetAsync("value");
            ClassicAssert.IsNull(str);
            str = await cs.GetSetAsync("new value");
            ClassicAssert.AreEqual("value", str);
            await context.Cache.RemoveAsync(key);
            var integer = await cs.GetSetAsync(456);
            ClassicAssert.AreEqual(0, integer);
            ClassicAssert.AreEqual(456, await cs.AsIntegerAsync());
            var fp = cs.GetSet(789.12);
            ClassicAssert.AreEqual(456.0, fp);
            ClassicAssert.AreEqual(789.12, await cs.AsFloatAsync());
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheString_Unicode(RedisContext context)
        {
            var key = "UT_CacheString_Unicode";
            context.Cache.Remove(key);
            var cs = context.Collections.GetRedisString(key);
            ClassicAssert.AreEqual(0, cs.Length);
            var str = "元来は有力貴族や諸大";
            cs.SetRange(0, str);
            ClassicAssert.AreEqual(10*3, cs.Length);
            var g = cs.GetRange(0, 2);
            ClassicAssert.AreEqual("元", cs[0, 2]);
            ClassicAssert.AreEqual("大", cs[-3, -1]);
            var lst = new List<byte>();
            foreach (byte b in cs)
            {
                lst.Add(b);
            }
            ClassicAssert.AreEqual(str, Encoding.UTF8.GetString(lst.ToArray()));
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheString_UnicodeAsync(RedisContext context)
        {
            var key = "UT_CacheString_UnicodeAsync";
            await context.Cache.RemoveAsync(key);
            var cs = context.Collections.GetRedisString(key);
            ClassicAssert.AreEqual(0, cs.Length);
            var str = "元来は有力貴族や諸大";
            await cs.SetRangeAsync(0, str);
            ClassicAssert.AreEqual(10 * 3, cs.Length);
            var g = await cs.GetRangeAsync(0, 2);
            ClassicAssert.AreEqual("元", cs[0, 2]);
            ClassicAssert.AreEqual("大", cs[-3, -1]);
            var lst = new List<byte>();
            foreach (byte b in cs)
            {
                lst.Add(b);
            }
            ClassicAssert.AreEqual(str, Encoding.UTF8.GetString(lst.ToArray()));
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheString_AsInteger(RedisContext context)
        {
            var key = "UT_CacheString_AsInteger";
            context.Cache.Remove(key);
            var str = context.Collections.GetRedisString(key);
            str.Set((long.MaxValue - 1).ToString());
            var value = str.IncrementBy(1);
            ClassicAssert.AreEqual(long.MaxValue, value);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheString_AsIntegerAsync(RedisContext context)
        {
            var key = "UT_CacheString_AsIntegerAsync";
            await context.Cache.RemoveAsync(key);
            var str = context.Collections.GetRedisString(key);
            await str.SetAsync((long.MaxValue - 1).ToString());
            var value = await str.IncrementByAsync(1);
            ClassicAssert.AreEqual(long.MaxValue, value);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheString_AsFloat(RedisContext context)
        {
            var key = "UT_CacheString_AsFloat";
            context.Cache.Remove(key);
            var str = context.Collections.GetRedisString(key);
            str.Append(Math.PI.ToString(CultureInfo.InvariantCulture));
            var fract = (double) 1/3;
            var value = str.IncrementByFloat(fract);
            ClassicAssert.AreEqual(Math.PI + fract, value, 0.000000001);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheString_AsFloatAsync(RedisContext context)
        {
            var key = "UT_CacheString_AsFloatAsync";
            await context.Cache.RemoveAsync(key);
            var str = context.Collections.GetRedisString(key);
            await str.AppendAsync(Math.PI.ToString(CultureInfo.InvariantCulture));
            var fract = (double)1 / 3;
            var value = await str.IncrementByFloatAsync(fract);
            ClassicAssert.AreEqual(Math.PI + fract, value, 0.000000001);
        }

        [Test, TestCaseSource(typeof(Common), "Raw")]
        public void UT_CacheHash_Mix(RedisContext context)
        {
            var key = "UT_CacheHash_Mix";
            context.Cache.Remove(key);
            var users = GetUsers();
            var redisDict = context.Collections.GetRedisDictionary<string, User>(key);
            redisDict.AddRange(users.ToDictionary(k => k.Id.ToString()));
            context.Cache.AddTagsToKey(redisDict.RedisKey, new[] { "tag1" });
            var returnedDict = context.Cache.GetHashedAll<User>(key);
            ClassicAssert.AreEqual(returnedDict["1"].Id, redisDict["1"].Id);
            context.Cache.InvalidateKeysByTag("tag1");
            ClassicAssert.AreEqual(0, redisDict.Count);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheHash_Mix_WithType(RedisContext context)
        {
            var key = "UT_CacheHash_Mix_WithType";
            var tag = "tag1-UT_CacheHash_Mix_WithType";
            context.Cache.Remove(key);
            var users = GetUsers();
            var redisDict = context.Collections.GetRedisDictionary<Location, User>(key);
            redisDict.AddRange(users.ToDictionary(k => new Location() { Id = k.Id, Name = k.Id.ToString() }));
            context.Cache.AddTagsToKey(redisDict.RedisKey, new[] { tag });
            var returnedDict = context.Cache.GetHashedAll<Location, User>(key);
            ClassicAssert.AreEqual(users.Count, returnedDict.Count);
            foreach (var u in users)
            {
                ClassicAssert.IsTrue(returnedDict.Keys.Any(k => k.Id == u.Id && k.Name == u.Id.ToString()));
            }
            ClassicAssert.AreEqual(1, redisDict[new Location() { Id = 1, Name = "1" }].Id);
            context.Cache.InvalidateKeysByTag(tag);
            ClassicAssert.AreEqual(0, redisDict.Count);
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

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheSetObjectAsync(RedisContext context)
        {
            string key1 = "UT_CacheSetObjectAsync";
            await context.Cache.RemoveAsync(key1);
            var users = GetUsers();
            var rs = context.Collections.GetRedisSet<User>(key1);
            await rs.AddRangeAsync(users);
            // Test GetEnumerator
            foreach (var item in rs)
            {
                ClassicAssert.IsTrue(users.Any(u => u.Id == item.Id));
            }
            // Test Count
            ClassicAssert.AreEqual(users.Count, (await rs.GetCountAsync()));
            // Test Contains
            ClassicAssert.IsTrue(await (rs.ContainsAsync(users[2])));
            // Test Add
            var newUser = new User() { Id = 5 };
            await rs.AddAsync(newUser);
            ClassicAssert.IsTrue(await (rs.ContainsAsync(newUser)));
            // Test Remove
            await rs.RemoveAsync(users[2]);
            ClassicAssert.IsFalse(await (rs.ContainsAsync(users[2])));
            // Test CopyTo
            User[] array = new User[50];
            rs.CopyTo(array, 10);
            ClassicAssert.AreEqual(users.Count, array.Count(x => x != null));
            // Test Clear
            await rs.ClearAsync();
            ClassicAssert.AreEqual(0, await rs.GetCountAsync());
            await rs.AddRangeAsync(new[] { new User() { Id = 3 }, new User() { Id = 1 }, new User() { Id = 2 } });
            ClassicAssert.AreEqual(3, await rs.GetCountAsync());
            await rs.RemoveAsync(new User() { Id = 1 });
            await rs.RemoveAsync(new User() { Id = 2 });
            ClassicAssert.AreEqual(1, await rs.GetCountAsync());
            ClassicAssert.IsTrue(await rs.ContainsAsync(new User() { Id = 3 }));
            // Test GetRandomMember
            var user = await rs.GetRandomMemberAsync();
            ClassicAssert.AreEqual(3, user.Id);
            // Test Pop
            user = await rs.PopAsync();
            ClassicAssert.AreEqual(3, user.Id);
            user = await rs.PopAsync();
            ClassicAssert.IsNull(user);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheSetObjectAsync_NoDeadlocks(RedisContext context)
        {
            string key1 = "UT_CacheSetObjectAsync_NoDeadlocks";
            await Common.TestDeadlock(() =>
            {
                _ = context.Cache.RemoveAsync(key1).Result;
            });
            
            var users = GetUsers();
            var rs = context.Collections.GetRedisSet<User>(key1);

            await Common.TestDeadlock(() =>
            {
                rs.AddRangeAsync(users).Wait();
            });


            // Test Count
            await Common.TestDeadlock(() =>
            {
                rs.GetCountAsync().Wait();
            });

            // Test Contains
            await Common.TestDeadlock(() =>
            {
                rs.ContainsAsync(users[2]).Wait();
            });
            
            // Test Add
            var newUser = new User() { Id = 5 };
            await Common.TestDeadlock(() =>
            {
                rs.AddAsync(newUser).Wait();
            });

            await Common.TestDeadlock(() =>
            {
                // Test Remove
                rs.RemoveAsync(users[2]).Wait();
            });

            // Test Clear
            await Common.TestDeadlock(() =>
            {
                rs.ClearAsync().Wait();
            });

            // Test GetRandomMember
            await Common.TestDeadlock(() =>
            {
                rs.GetRandomMemberAsync().Wait();
            });

            // Test Pop
            await Common.TestDeadlock(() =>
            {
                _ = rs.PopAsync().Result;
            });
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheSetObject_TTLAsync(RedisContext context)
        {
            string key1 = "UT_CacheSetObject_TTLAsync";
            context.Cache.Remove(key1);
            var users = GetUsers();
            var rl = context.Collections.GetRedisSet<User>(key1);
            await rl.AddRangeAsync(users);
            rl.TimeToLive = TimeSpan.FromMilliseconds(1500);
            ClassicAssert.AreEqual(users.Count, await rl.GetCountAsync());
            Thread.Sleep(2000);
            ClassicAssert.AreEqual(0, await rl.GetCountAsync());
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheSetObject_SetModifiersAsync(RedisContext context)
        {
            string keyAbc = "UT_CacheSetObject_SetModifiersAsync_ABC";
            string keyCde = "UT_CacheSetObject_SetModifiersAsync_CDE";

            context.Cache.Remove(keyAbc);
            context.Cache.Remove(keyCde);
            var abcSet = context.Collections.GetRedisSet<char>(keyAbc);
            await abcSet.AddRangeAsync("ABC");

            var cdeSet = context.Collections.GetRedisSet<char>(keyCde);
            await cdeSet.AddRangeAsync("CDE");

            // Test Count
            ClassicAssert.AreEqual(3, await abcSet.GetCountAsync());
            ClassicAssert.AreEqual(3, await cdeSet.GetCountAsync());
            abcSet.Clear();
            cdeSet.Clear();
        }


    }
}
