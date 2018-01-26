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

namespace CachingFramework.Redis.UnitTest
{
    [TestFixture]
    public class UnitTestRedisObjects
    {
        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheGeo_WithTags(Context context)
        {
            string key = "UT_CacheGeo_WithTags";
            context.Cache.Remove(key);
            context.Cache.InvalidateKeysByTag("tag1", "tag2", "common");

            var geo1 = context.GeoSpatial.GeoAdd(key, 12.34, 23.45, "value1", new[] { "tag1", "common" });
            var geo2 = context.GeoSpatial.GeoAdd(key, 33.34, 11.45, "value2", new[] { "tag2", "common" });

            var t1 = context.Cache.GetObjectsByTag<string>("tag1").ToList();
            var t2 = context.Cache.GetObjectsByTag<string>("tag2").ToList();
            var x = context.Cache.GetObjectsByTag<string>("common").ToList();

            Assert.AreEqual(2, x.Count);
            Assert.AreEqual(1, t1.Count);
            Assert.AreEqual(1, t2.Count);
            Assert.IsTrue(x.Contains("value1"));
            Assert.IsTrue(x.Contains("value2"));
            Assert.IsTrue(t1.Contains("value1"));
            Assert.IsTrue(t2.Contains("value2"));

            context.Cache.RemoveTagsFromSetMember(key, "value1", new[] { "tag1" });
            Assert.AreEqual(0, context.Cache.GetObjectsByTag<string>("tag1").Count());

            context.Cache.InvalidateKeysByTag("common");

            Assert.IsNull(context.GeoSpatial.GeoPosition(key, "value1"));
            Assert.IsNull(context.GeoSpatial.GeoPosition(key, "value2"));
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheSortedSet_WithTags(Context context)
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

            Assert.AreEqual(2, x.Count);
            Assert.AreEqual(1, t1.Count);
            Assert.AreEqual(1, t2.Count);
            Assert.IsTrue(x.Contains("value1"));
            Assert.IsTrue(x.Contains("value2"));
            Assert.IsTrue(t1.Contains("value1"));
            Assert.IsTrue(t2.Contains("value2"));

            context.Cache.InvalidateKeysByTag("common");

            Assert.AreEqual(0, sset.Count);
        }


        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheSet_Mix_WithTags(Context context)
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

            Assert.AreEqual(3, x.Count);
            Assert.IsTrue(x.Contains("geo2"));
            Assert.IsTrue(x.Contains("s2"));
            Assert.IsTrue(x.Contains("ss2"));

            context.Cache.InvalidateKeysByTag("tag");

            x = context.Cache.GetObjectsByTag<string>("tag").ToList();
            var pos = context.GeoSpatial.GeoPosition(geokey, "geo2");

            Assert.AreEqual(0, x.Count);
            Assert.IsNull(pos);
            Assert.IsFalse(sset.Contains("ss2"));
            Assert.IsTrue(sset.Contains("ss1"));
            Assert.IsTrue(set.Contains("s1"));
            Assert.IsFalse(set.Contains("s2"));
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheSet_WithTags(Context context)
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

            Assert.AreEqual(0, xxx.Count);
            Assert.AreEqual(5, commonValues.Count);
            Assert.AreEqual(1, t1.Count);
            Assert.AreEqual(2, t2.Count);
            Assert.AreEqual(1, t3.Count);

            Assert.AreEqual("item1", t1[0]);
            Assert.IsTrue(t2.Contains("item2"));
            Assert.IsTrue(t2.Contains("item222"));
            Assert.AreEqual("item3", t3[0]);

            context.Cache.RemoveTagsFromSetMember(key, "item222", new [] { "tag2", "common" });
            commonValues = context.Cache.GetObjectsByTag<string>("common").ToList();
            t2 = context.Cache.GetObjectsByTag<string>("tag2").ToList();

            Assert.AreEqual(4, commonValues.Count);
            Assert.AreEqual(1, t2.Count);
            Assert.AreEqual("item2", t2[0]);
        }

        [Test, TestCaseSource(typeof (Common), "Raw")]
        public void UT_CacheSortedSet_When(Context context)
        {
            string key = "UT_CacheSortedSet_When";
            context.Cache.Remove(key);
            var lst = context.Collections.GetRedisSortedSet<string>(key);

            lst.Add(12.345, "item1", When.Exists);
            var rank = lst.RankOf("item1");
            Assert.IsNull(rank);

            lst.Add(12.345, "item1", When.NotExists);
            rank = lst.RankOf("item1");
            Assert.AreEqual(0, rank.Value);

            lst.Add(34.567, "item1", When.NotExists);
            var score = lst.ScoreOf("item1");
            Assert.AreEqual(12.345, score.Value, 0.0001);

            lst.Add(34.567, "itemXXX", When.Exists);
            score = lst.ScoreOf("itemXXX");
            Assert.IsNull(score);

            lst.Add(34.567, "item1", When.Exists);
            score = lst.ScoreOf("item1");
            Assert.AreEqual(34.567, score.Value, 0.0001);

            lst.AddRange(new [] { new SortedMember<string>(56.789, "item1"), new SortedMember<string>(77.888, "itemXXX") }, When.Exists);
            score = lst.ScoreOf("item1");
            Assert.AreEqual(56.789, score.Value, 0.0001);
            score = lst.ScoreOf("itemXXX");
            Assert.IsNull(score);

            lst.AddRange(new[] { new SortedMember<string>(99.999, "item1"), new SortedMember<string>(88.999, "itemXXX") }, When.NotExists);
            score = lst.ScoreOf("item1");
            Assert.AreEqual(56.789, score.Value, 0.0001);
            score = lst.ScoreOf("itemXXX");
            Assert.AreEqual(88.999, score.Value, 0.0001);

            context.Cache.Remove(key);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheList_Remove(Context context)
        {
            string key = "UT_CacheList_Remove";
            context.Cache.Remove(key);
            var lst = context.Collections.GetRedisList<string>(key);
            lst.AddRange(new [] { "test", "test", "anothertest" });
            Assert.AreEqual(3, lst.Count);
            lst.RemoveAt(0);
            Assert.AreEqual(2, lst.Count);
            Assert.AreEqual("test", lst[0]);
            Assert.AreEqual("anothertest", lst[1]);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheList_Insert(Context context)
        {
            string key = "UT_CacheList_Insert";
            context.Cache.Remove(key);
            var rl = context.Collections.GetRedisList<string>(key);
            rl.Insert(0, "test");
            rl.Insert(0, "test");
            rl.Insert(0, "test");
            Assert.AreEqual(3, rl.Count);
            rl.Insert(2, "test2");
            Assert.AreEqual(4, rl.Count);
            Assert.AreEqual("test", rl[0]);
            Assert.AreEqual("test2", rl[2]);
            rl.Insert(rl.Count, "LAST");
            Assert.AreEqual(5, rl.Count);
            Assert.AreEqual("LAST", rl[rl.Count - 1]);
            rl[rl.Count - 1] = "NEW LAST";
            Assert.AreEqual(5, rl.Count);
            Assert.AreEqual("NEW LAST", rl[rl.Count - 1]);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheList_Trim(Context context)
        {
            string key = "UT_CacheList_Trim";
            context.Cache.Remove(key);
            var rl = context.Collections.GetRedisList<int>(key);
            rl.AddRange(Enumerable.Range(1, 100));
            Assert.AreEqual(100, rl.Count);
            rl.Trim(0, 9);
            Assert.AreEqual(10, rl.Count);
            Assert.AreEqual(1, rl[0]);
            Assert.AreEqual(10, rl[-1]);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheListObject(Context context)
        {
            string key1 = "UT_CacheListObject1";
            context.Cache.Remove(key1);
            var users = GetUsers();
            var rl = context.Collections.GetRedisList<int>(key1);
            rl.AddRange(users.Select(u => u.Id));
            // Test GetEnumerator
            foreach (var item in rl)
            {
                Assert.IsTrue(users.Any(u => u.Id == item));
            }
            // Test Count
            Assert.AreEqual(users.Count, rl.Count);
            // Test First and Last
            var first = rl.First();
            Assert.AreEqual(users.First().Id, first);
            var firstod = rl.FirstOrDefault();
            Assert.AreEqual(users.First().Id, firstod);
            Assert.AreEqual(users.Last().Id, rl.Last());
            Assert.AreEqual(users.Last().Id, rl.LastOrDefault());
            // Test Contains
            Assert.IsTrue(rl.Contains(users[2].Id));
            // Test Insert
            rl.Insert(0, 0);
            Assert.AreEqual(0, rl[0]);
            // Test RemoveAt
            rl.RemoveAt(0);
            Assert.AreEqual(1, rl[0]);
            // Test Add
            rl.Add(5);
            Assert.AreEqual(5, rl.Last());
            // Test IndexOf
            Assert.AreEqual(2, rl.IndexOf(users[2].Id));
            // Test Remove
            rl.Remove(users[2].Id);
            Assert.IsFalse(rl.Contains(users[2].Id));
            // Test CopyTo
            int[] array = new int[50];
            rl.CopyTo(array, 10);
            Assert.AreEqual(1, array[10]);
            // Test Clear
            rl.Clear();
            Assert.AreEqual(0, rl.Count);
            Assert.AreEqual(0, rl.LastOrDefault());
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheListPushPop(Context context)
        {
            string key = "UT_CacheListPushPop";
            context.Cache.Remove(key);
            var users = GetUsers();
            var rl = context.Collections.GetRedisList<User>(key);
            Assert.IsNull(rl.FirstOrDefault());
            Assert.IsNull(rl.LastOrDefault());
            rl.AddRange(users);
            rl.PushFirst(new User() { Id = 0 });
            rl.PushLast(new User() { Id = 666 });
            Assert.AreEqual(0, rl[0].Id);
            Assert.AreEqual(0, rl.FirstOrDefault().Id);
            Assert.AreEqual(666, rl.LastOrDefault().Id);
            Assert.AreEqual(users.Count + 2, rl.Count);
            var remf = rl.PopFirst();
            Assert.AreEqual(users.Count + 1, rl.Count);
            Assert.AreEqual(0, remf.Id);
            var reml = rl.PopLast();
            Assert.AreEqual(users.Count, rl.Count);
            Assert.AreEqual(666, reml.Id);
        }

        [Test, TestCaseSource(typeof(Common), "Raw")]
        public void UT_CacheListRemoveAt(Context context)
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
            Assert.AreEqual(4, rl.Count);
            Assert.AreEqual("test 2", rl[0]);

            rl.RemoveAt(rl.Count - 1);
            Assert.AreEqual(3, rl.Count);
            Assert.AreEqual("test 4", rl.LastOrDefault());

            rl.RemoveAt(1);
            Assert.AreEqual(2, rl.Count);
            Assert.AreEqual("test 2", rl.FirstOrDefault());
            Assert.AreEqual("test 4", rl.LastOrDefault());
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheListObjectTTL(Context context)
        {
            string key1 = "UT_CacheListObject_TTL1";
            context.Cache.Remove(key1);
            var users = GetUsers();
            var rl = context.Collections.GetRedisList<User>(key1);
            rl.AddRange(users);
            rl.TimeToLive = TimeSpan.FromMilliseconds(1500);
            Assert.AreEqual(users.Count, rl.Count);
            Thread.Sleep(2000);
            Assert.AreEqual(0, rl.Count);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheListObject_GetRange(Context context)
        {
            string key = "UT_CacheListObject_GetRange";
            int total = 100;
            context.Cache.Remove(key);
            var rl = context.Collections.GetRedisList<User>(key);
            rl.AddRange(Enumerable.Range(1, total).Select(i => new User() {Id = i}));

            var range = rl.GetRange().ToList();
            Assert.AreEqual(total, rl.Count);

            range = rl.GetRange(3, 10).ToList();
            Assert.AreEqual(8, range.Count);
            Assert.AreEqual(4, range[0].Id);

            range = rl.GetRange(10, -10).ToList();
            Assert.AreEqual(11, range[0].Id);
            Assert.AreEqual(91, range[range.Count - 1].Id);
        }


        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheList_Remove_Async(Context context)
        {
            string key = "UT_CacheList_Remove_Async";
            await context.Cache.RemoveAsync(key);
            var lst = context.Collections.GetRedisList<string>(key);
            await lst.AddRangeAsync(new[] { "test", "test", "anothertest" });
            Assert.AreEqual(3, lst.Count);
            await lst.RemoveAtAsync(0);
            Assert.AreEqual(2, lst.Count);
            Assert.AreEqual("test", lst[0]);
            Assert.AreEqual("anothertest", lst[1]);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheList_Insert_Async(Context context)
        {
            string key = "UT_CacheList_Insert_Async";
            await context.Cache.RemoveAsync(key);
            var rl = context.Collections.GetRedisList<string>(key);
            await rl.InsertAsync(0, "test");
            await rl.InsertAsync(0, "test");
            await rl.InsertAsync(0, "test");
            Assert.AreEqual(3, rl.Count);
            await rl.InsertAsync(2, "test2");
            Assert.AreEqual(4, rl.Count);
            Assert.AreEqual("test", rl[0]);
            Assert.AreEqual("test2", rl[2]);
            await rl.InsertAsync(rl.Count, "LAST");
            Assert.AreEqual(5, rl.Count);
            Assert.AreEqual("LAST", rl[rl.Count - 1]);
            rl[rl.Count - 1] = "NEW LAST";
            Assert.AreEqual(5, rl.Count);
            Assert.AreEqual("NEW LAST", rl[rl.Count - 1]);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheList_Trim_Async(Context context)
        {
            string key = "UT_CacheList_Trim_Async";
            await context.Cache.RemoveAsync(key);
            var rl = context.Collections.GetRedisList<int>(key);
            await rl.AddRangeAsync(Enumerable.Range(1, 100));
            Assert.AreEqual(100, rl.Count);
            await rl.TrimAsync(0, 9);
            Assert.AreEqual(10, rl.Count);
            Assert.AreEqual(1, rl[0]);
            Assert.AreEqual(10, rl[-1]);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheListObject_Async(Context context)
        {
            string key1 = "UT_CacheListObject_Async";
            await context.Cache.RemoveAsync(key1);
            var users = GetUsers();
            var rl = context.Collections.GetRedisList<int>(key1);
            await rl.AddRangeAsync(users.Select(u => u.Id));
            // Test GetEnumerator
            foreach (var item in rl)
            {
                Assert.IsTrue(users.Any(u => u.Id == item));
            }
            // Test Count
            Assert.AreEqual(users.Count, rl.Count);
            // Test First and Last
            var first = rl.First();
            Assert.AreEqual(users.First().Id, first);
            var firstod = rl.FirstOrDefault();
            Assert.AreEqual(users.First().Id, firstod);
            Assert.AreEqual(users.Last().Id, rl.Last());
            Assert.AreEqual(users.Last().Id, rl.LastOrDefault());
            // Test Contains
            Assert.IsTrue(rl.Contains(users[2].Id));
            // Test Insert
            await rl.InsertAsync(0, 0);
            Assert.AreEqual(0, rl[0]);
            // Test RemoveAt
            await rl.RemoveAtAsync(0);
            Assert.AreEqual(1, rl[0]);
            // Test Add
            await rl.AddAsync(5);
            Assert.AreEqual(5, rl.Last());
            // Test IndexOf
            var i = await rl.IndexOfAsync(users[2].Id);
            Assert.AreEqual(2, i);
            // Test Remove
            await rl.RemoveAsync(users[2].Id, 1);
            var b = await rl.ContainsAsync(users[2].Id);
            Assert.IsFalse(b);
            // Test CopyTo
            int[] array = new int[50];
            rl.CopyTo(array, 10);
            Assert.AreEqual(1, array[10]);
            // Test Clear
            await rl.ClearAsync();
            Assert.AreEqual(0, rl.Count);
            Assert.AreEqual(0, rl.LastOrDefault());
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheListPushPop_Async(Context context)
        {
            string key = "UT_CacheListPushPop_Async";
            await context.Cache.RemoveAsync(key);
            var users = GetUsers();
            var rl = context.Collections.GetRedisList<User>(key);
            Assert.IsNull(rl.FirstOrDefault());
            Assert.IsNull(rl.LastOrDefault());
            await rl.AddRangeAsync(users);
            await rl.PushFirstAsync(new User() { Id = 0 });
            await rl.PushLastAsync(new User() { Id = 666 });
            Assert.AreEqual(0, rl[0].Id);
            Assert.AreEqual(0, rl.FirstOrDefault().Id);
            Assert.AreEqual(666, rl.LastOrDefault().Id);
            Assert.AreEqual(users.Count + 2, rl.Count);
            var remf = await rl.PopFirstAsync();
            Assert.AreEqual(users.Count + 1, rl.Count);
            Assert.AreEqual(0, remf.Id);
            var reml = await rl.PopLastAsync();
            Assert.AreEqual(users.Count, rl.Count);
            Assert.AreEqual(666, reml.Id);
        }

        [Test, TestCaseSource(typeof(Common), "Raw")]
        public async Task UT_CacheListRemoveAt_Async(Context context)
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
            Assert.AreEqual(4, rl.Count);
            Assert.AreEqual("test 2", rl[0]);

            await rl.RemoveAtAsync(rl.Count - 1);
            Assert.AreEqual(3, rl.Count);
            Assert.AreEqual("test 4", rl.LastOrDefault());

            await rl.RemoveAtAsync(1);
            Assert.AreEqual(2, rl.Count);
            Assert.AreEqual("test 2", rl.FirstOrDefault());
            Assert.AreEqual("test 4", rl.LastOrDefault());
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheListObjectTTL_Async(Context context)
        {
            string key1 = "UT_CacheListObjectTTL_Async";
            await context.Cache.RemoveAsync(key1);
            var users = GetUsers();
            var rl = context.Collections.GetRedisList<User>(key1);
            await rl.AddRangeAsync(users);
            rl.TimeToLive = TimeSpan.FromMilliseconds(1500);
            Assert.AreEqual(users.Count, rl.Count);
            Thread.Sleep(2000);
            Assert.AreEqual(0, rl.Count);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheListObject_GetRange_Async(Context context)
        {
            string key = "UT_CacheListObject_GetRange_Async";
            int total = 100;
            await context.Cache.RemoveAsync(key);
            var rl = context.Collections.GetRedisList<User>(key);
            await rl.AddRangeAsync(Enumerable.Range(1, total).Select(i => new User() { Id = i }));

            var r = await rl.GetRangeAsync();
            var range = r.ToList();
            Assert.AreEqual(total, rl.Count);

            r = await rl.GetRangeAsync(3, 10);
            range = r.ToList();

            Assert.AreEqual(8, range.Count);
            Assert.AreEqual(4, range[0].Id);

            r = await rl.GetRangeAsync(10, -10);
            range = r.ToList();
            Assert.AreEqual(11, range[0].Id);
            Assert.AreEqual(91, range[range.Count - 1].Id);
        }


        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheDictionaryObject(Context context)
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
                Assert.IsTrue(users.Any(u => u.Id == item.Key));
            }
            // Test Count
            Assert.AreEqual(users.Count, rd.Count);
            // Test ContainsKey
            Assert.IsTrue(rd.ContainsKey(users[1].Id));
            // Test Contains
            Assert.IsTrue(rd.Contains(new KeyValuePair<int, User>(users.Last().Id, users.Last())));
            // Test Add
            rd.Add(0, new User() {Id = 0});
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

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheDictionaryObject_TTL(Context context)
        {
            string key1 = "UT_CacheDictionaryObjectTTL1";
            context.Cache.Remove(key1);
            var users = GetUsers();
            var rl = context.Collections.GetRedisDictionary<int, User>(key1);
            rl.AddRange(users.ToDictionary(k => k.Id));
            rl.TimeToLive = TimeSpan.FromMilliseconds(1500);
            Assert.AreEqual(users.Count, rl.Count);
            Thread.Sleep(2000);
            Assert.AreEqual(0, rl.Count);
        }

        [Test, TestCaseSource(typeof(Common), "JsonAndRaw")]
        public void UT_CacheDictionaryIncrement(Context context)
        {
            string key = "UT_CacheDictionaryIncrement";
            context.Cache.Remove(key);
            var rl = context.Collections.GetRedisDictionary<string, int>(key);
            var r1 = rl.IncrementBy("h1", 1);
            var r2 = rl.IncrementBy("h2", -2);
            var r3 = rl.IncrementBy("h1", -2);

            var v1 = rl["h1"]; 
            var v2 = rl.GetValue("h2");

            Assert.AreEqual(1, r1);
            Assert.AreEqual(-2, r2);
            Assert.AreEqual(-1, r3);
            Assert.AreEqual(-1, v1);
            Assert.AreEqual(-2, v2);
        }


        [Test, TestCaseSource(typeof(Common), "JsonAndRaw")]
        public void UT_CacheDictionaryIncrementFloat(Context context)
        {
            string key = "UT_CacheDictionaryIncrementFloat";
            context.Cache.Remove(key);
            var rl = context.Collections.GetRedisDictionary<string, double>(key);
            var r1 = rl.IncrementByFloat("h1", (double)1.23);
            var r2 = rl.IncrementByFloat("h2", (double)-2.23);
            var r3 = rl.IncrementByFloat("h1", (double)-2.01);
                                   
            var v1 = rl["h1"];
            var v2 = rl.GetValue("h2");

            Assert.AreEqual(1.23, r1, 0.0001);
            Assert.AreEqual(-2.23, r2, 0.0001);
            Assert.AreEqual(1.23-2.01, r3, 0.0001);
            Assert.AreEqual(1.23-2.01, v1, 0.0001);
            Assert.AreEqual(-2.23, v2, 0.0001);
        }

        [Test, TestCaseSource(typeof(Common), "JsonAndRaw")]
        public async Task UT_CacheDictionaryIncrementAsync(Context context)
        {
            string key = "UT_CacheDictionaryIncrementAsync";
            await context.Cache.RemoveAsync(key);
            var rl = context.Collections.GetRedisDictionary<string, int>(key);
            var r1 = await rl.IncrementByAsync("h1", 1);
            var r2 = await rl.IncrementByAsync("h2", -2);
            var r3 = await rl.IncrementByAsync("h1", -2);

            var v1 = rl["h1"];
            var v2 = await rl.GetValueAsync("h2");

            Assert.AreEqual(1, r1);
            Assert.AreEqual(-2, r2);
            Assert.AreEqual(-1, r3);
            Assert.AreEqual(-1, v1);
            Assert.AreEqual(-2, v2);
        }

        [Test, TestCaseSource(typeof(Common), "JsonAndRaw")]
        public async Task UT_CacheDictionaryIncrementFloatAsync(Context context)
        {
            string key = "UT_CacheDictionaryIncrementFloatAsync";
            context.Cache.Remove(key);
            var rl = context.Collections.GetRedisDictionary<string, double>(key);
            var r1 = await rl.IncrementByFloatAsync("h1", (double)1.23);
            var r2 = await rl.IncrementByFloatAsync("h2", (double)-2.23);
            var r3 = await rl.IncrementByFloatAsync("h1", (double)-2.01);

            var v1 = rl["h1"];
            var v2 = await rl.GetValueAsync("h2");

            Assert.AreEqual(1.23, r1, 0.0001);
            Assert.AreEqual(-2.23, r2, 0.0001);
            Assert.AreEqual(1.23 - 2.01, r3, 0.0001);
            Assert.AreEqual(1.23 - 2.01, v1, 0.0001);
            Assert.AreEqual(-2.23, v2, 0.0001);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheDictionaryObjectAsync(Context context)
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
                Assert.IsTrue(users.Any(u => u.Id == item.Key));
            }
            // Test Count
            Assert.AreEqual(users.Count, await rd.GetCountAsync());
            // Test ContainsKey
            Assert.IsTrue(await rd.ContainsKeyAsync(users[1].Id));
            // Test Contains
            Assert.IsTrue(await rd.ContainsAsync(new KeyValuePair<int, User>(users.Last().Id, users.Last())));
            // Test Add
            await rd.AddAsync(0, new User() { Id = 0 });
            Assert.AreEqual(users.Count + 1, await rd.GetCountAsync());
            Assert.AreEqual(0, rd[0].Id);
            // Test Remove
            await rd.RemoveAsync(0);
            Assert.IsFalse(await rd.ContainsKeyAsync(0));
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
            await rd.ClearAsync();
            Assert.AreEqual(0, await rd.GetCountAsync());
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheDictionaryObject_TTLAsync(Context context)
        {
            string key1 = "UT_CacheDictionaryObject_TTLAsync";
            context.Cache.Remove(key1);
            var users = GetUsers();
            var rl = context.Collections.GetRedisDictionary<int, User>(key1);
            await rl.AddRangeAsync(users.ToDictionary(k => k.Id));
            rl.TimeToLive = TimeSpan.FromMilliseconds(1500);
            Assert.AreEqual(users.Count, await rl.GetCountAsync());
            Thread.Sleep(2000);
            Assert.AreEqual(0, await rl.GetCountAsync());
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheDictionaryObject_AddAsyncWithTags(Context context)
        {
            string key1 = "UT_CacheDictionaryObject_AddAsyncWithTags";
            string tag1 = "UT_CacheDictionaryObject_AddAsyncWithTags_TAG1";
            context.Cache.Remove(key1);
            context.Cache.InvalidateKeysByTag(tag1);
            var users = GetUsers();
            var rl = context.Collections.GetRedisDictionary<int, User>(key1);
            await rl.AddAsync(1, users[0], new[] { tag1 });
            var keys = context.Cache.GetKeysByTag(new[] { tag1 }, true).ToList();
            Assert.AreEqual(1, keys.Count);
            var val = Encoding.UTF8.GetString(context.GetSerializer().Serialize(1));
            Assert.AreEqual("UT_CacheDictionaryObject_AddAsyncWithTags:$_->_$:" + val, keys[0]);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheList_EXP(Context context)
        {
            string key = "UT_CacheList_EXP";
            context.Cache.Remove(key);
            var set = context.Collections.GetRedisSet<string>(key);
            set.AddRange(new [] { "test1", "test2", "test3" });
            var realLocalExp = DateTime.UtcNow.AddSeconds(2);
            set.Expiration = realLocalExp;
            var exp = set.Expiration.Value.ToUniversalTime();
            var startedOn = DateTime.Now;
            Assert.AreEqual(3, set.Count);
            while (context.Cache.KeyExists(key) && DateTime.Now < startedOn.AddSeconds(10))
            {
                Thread.Sleep(100);
            }
            Assert.IsFalse(context.Cache.KeyExists(key));
            Assert.AreEqual(0, set.Count);
            var stoppedOn = DateTime.Now;
            var span = stoppedOn - startedOn;
            Assert.AreEqual(2, span.TotalSeconds, 1);
            Assert.AreEqual(0, (exp - realLocalExp).TotalSeconds, 1);
        }

#if (NET45 || NET461)
        [Test, TestCaseSource(typeof(Common), "Bin")]
        public void UT_CacheList_StrObj(Context context)
        {
            string key = "UT_CacheList_StrObj";
            context.Cache.Remove(key);
            context.Cache.SetObject<string>(key, "test value 1");
            var obj = context.Cache.GetObject<object>(key);
            Assert.IsTrue(obj is string);
            context.Cache.SetObject<string>(key, Encoding.UTF8.GetString(new byte[] { 0x1f, 0x8b }));
            obj = context.Cache.GetObject<object>(key);
            Assert.IsTrue(obj is string);
        }
#endif

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheSetObject(Context context)
        {
            string key1 = "UT_CacheSetObject1";
            context.Cache.Remove(key1);
            var users = GetUsers();
            var rs = context.Collections.GetRedisSet<User>(key1);
            rs.AddRange(users);
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
            rs.AddRange(new []{  new User() {Id = 3},  new User() {Id = 1},  new User() {Id = 2} });
            Assert.AreEqual(3, rs.Count);
            rs.Remove(new User() { Id = 1 });
            rs.Remove(new User() { Id = 2 });
            Assert.AreEqual(1, rs.Count);
            Assert.IsTrue(rs.Contains(new User() { Id = 3 }));
            // Test GetRandomMember
            var user = rs.GetRandomMember();
            Assert.AreEqual(3, user.Id);
            // Test Pop
            user = rs.Pop();
            Assert.AreEqual(3, user.Id);
            user = rs.Pop();
            Assert.IsNull(user);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheSetObject_TTL(Context context)
        {
            string key1 = "UT_CacheSetObject_TTL";
            context.Cache.Remove(key1);
            var users = GetUsers();
            var rl = context.Collections.GetRedisSet<User>(key1);
            rl.AddRange(users);
            rl.TimeToLive = TimeSpan.FromMilliseconds(1500);
            Assert.AreEqual(users.Count, rl.Count);
            Thread.Sleep(2000);
            Assert.AreEqual(0, rl.Count);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheSetObject_SetModifiers(Context context)
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
            Assert.AreEqual(3, abcSet.Count);
            Assert.AreEqual(3, cdeSet.Count);

            abcSet.Clear();
            cdeSet.Clear();
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheSortedSet_GetRange(Context context)
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
            Assert.AreEqual(4, count);
            Assert.AreEqual(count, byRank.Count);
            Assert.AreEqual(12.34, byRank[1].Score);
            Assert.AreEqual(double.NegativeInfinity, byRank[0].Score);

            var byScore = ss.GetRangeByScore(12.34, 23.449).ToList();
            Assert.AreEqual(1, byScore.Count);
            Assert.AreEqual(users[0].Id, byScore[0].Value.Id);

            byScore = ss.GetRangeByScore(12.34, 23.45).ToList();
            Assert.AreEqual(2, byScore.Count);
            Assert.AreEqual(users[1].Id, byScore[1].Value.Id);
        }

        [Test, TestCaseSource(typeof(Common), "Json")]
        public void UT_CacheSortedSet_SE_Issue287(Context context)
        {
            var key = "UT_CacheSortedSet_SE_Issue287";
            context.Cache.Remove(key);
            var ss = context.Collections.GetRedisSortedSet<User>(key);
            var users = GetUsers();

            ss.Add(double.NegativeInfinity, users[3]);
            ss.Add(double.PositiveInfinity, users[2]);

            var byRank = ss.GetRangeByRank().ToList();
            Assert.AreEqual(double.NegativeInfinity, byRank[0].Score);
            // This is a StackExhange.Redis bug: https://github.com/StackExchange/StackExchange.Redis/issues/287
            // This was corrected and should be included in the next SE.Redis version
            Assert.AreEqual(double.NegativeInfinity, byRank[0].Score);
            Assert.AreEqual(double.PositiveInfinity, byRank[1].Score);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheSortedSet_GetRangeByRankNegative(Context context)
        {
            var key = "UT_CacheSortedSet_GetRangeByRankNegative";
            context.Cache.Remove(key);
            var ss = context.Collections.GetRedisSortedSet<string>(key);
            ss.AddRange(new[] { new SortedMember<string>(33, "c"), new SortedMember<string>(0, "a"), new SortedMember<string>(22, "b") });

            var byRank = ss.GetRangeByRank(-2, -1).ToList();
            var byRankRev = ss.GetRangeByRank(-2, -1, true).ToList();

            Assert.AreEqual(2, byRank.Count);
            Assert.AreEqual("b", byRank[0].Value);
            Assert.AreEqual("c", byRank[1].Value);
            Assert.AreEqual("a", byRankRev[1].Value);
            Assert.AreEqual("b", byRankRev[0].Value);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheSortedSet_etc(Context context)
        {
            var key = "UT_CacheSortedSet_etc";
            var ss = context.Collections.GetRedisSortedSet<string>(key);
            context.Cache.Remove(key);
            for (int i = 0; i < 255; i++)
            {
                ss.Add(i, "member " + i);
            }
            Assert.AreEqual(10, ss.CountByScore(1, 10));
            var incremented = ss.IncrementScore("member 10", 1000);
            Assert.AreEqual(1010, incremented);
            Assert.AreEqual(255, ss.Count);

            int x = 0;
            foreach (var item in ss)
            {
                x++;
            }
            Assert.AreEqual(255, x);

            var r0 = ss.RankOf("member 0");
            var r9 = ss.RankOf("member 9");
            var r10 = ss.RankOf("member 10");
            var r11 = ss.RankOf("member 11");
            var r254 = ss.RankOf("member 254");
            var r255 = ss.RankOf("member 255");
            var r0Rev = ss.RankOf("member 0", true);

            Assert.AreEqual(0, r0);
            Assert.AreEqual(9, r9);
            Assert.AreEqual(10, r11);
            Assert.AreEqual(254, r10);
            Assert.AreEqual(253, r254);
            Assert.AreEqual(254, r0Rev);
            Assert.IsNull(r255);

            var s0 = ss.ScoreOf("member 0");
            var s254 = ss.ScoreOf("member 254");
            var s255 = ss.ScoreOf("member 255");
            Assert.AreEqual(0, s0);
            Assert.AreEqual(254, s254);
            Assert.IsNull(s255);

            ss.RemoveRangeByRank(0, 2);
            Assert.AreEqual(252, ss.Count);

            ss.RemoveRangeByScore(double.NegativeInfinity, 9);
            Assert.AreEqual(245, ss.Count);

            Assert.IsTrue(ss.Contains("member 100"));
            Assert.IsFalse(ss.Contains("member 0"));

            ss.Remove("member 100");
            Assert.IsFalse(ss.Contains("member 100"));

            ss.Clear();
            Assert.AreEqual(0, ss.Count);


        }

        [Test, TestCaseSource(typeof(Common), "Raw")]
        public void UT_CacheBitmap(Context context)
        {
            var key = "UT_CacheBitmap";
            context.Cache.Remove(key);
            var bm = context.Collections.GetRedisBitmap(key);
            bm.Add(0xff);           // 11111111 
            Assert.IsFalse(bm.Contains(0));
            bm.Add(0x00);          // 11111111 00000000
            Assert.IsTrue(bm.Contains(0));
            bm.Add(0xff);           // 11111111 00000000 11111111
            bm.SetBit(24, 1);    // 11111111 00000000 11111111 10000000
            bm.SetBit(25, 0);   // 11111111 00000000 11111111 10000000
            bm.SetBit(26, 1);    // 11111111 00000000 11111111 10100000
            bm.SetBit(27, 1);    // 11111111 00000000 11111111 10110000
            Assert.AreEqual(19, bm.Count());
            Assert.IsFalse(bm.Contains(0, 0, 0));
            Assert.IsFalse(bm.Contains(1, 1, 1));
            Assert.IsTrue(bm.Contains(0, -1, -1));
            Assert.IsTrue(bm.Contains(1, -1, -1));
            Assert.AreEqual(0, bm.BitPosition(1));
            Assert.AreEqual(8, bm.BitPosition(0));
            Assert.AreEqual(24+1, bm.BitPosition(0, -1, -1));
            Assert.AreEqual(24+0, bm.BitPosition(1, -1, -1));
            Assert.AreEqual(0, bm.GetBit(25));
            Assert.AreEqual(1, bm.GetBit(26));
            Assert.AreEqual(1, bm.GetBit(27));
            Assert.AreEqual(3, bm.Count(-1, -1));
            Assert.AreEqual(11, bm.Count(-2, -1));
            Assert.AreEqual(11, bm.Count(2, -1));
            Assert.AreEqual(19, bm.Count());
            Assert.AreEqual(8, bm.Count(0, 1));
            var sb = new StringBuilder();
            foreach (var bit in bm)
            {
                sb.Append(bit);
            }
            Assert.AreEqual("11111111000000001111111110110000", sb.ToString());
            Assert.AreEqual("11111111000000001111111110110000", bm.ToString());
            bm.SetBit(9999999, 1);
            Assert.AreEqual(20, bm.Count());
            Assert.AreEqual(0, bm.GetBit(9999998));
            Assert.AreEqual(1, bm.GetBit(9999999));    
            bm.Clear();
            Assert.AreEqual(0, bm.Count());
            bm.Remove(0);
            Assert.AreEqual(1, bm.Count());
            Assert.AreEqual(1, bm.GetBit(0));
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheLexSet(Context context)
        {
            var key = "UT_CacheLexSet";
            context.Cache.Remove(key);
            var bm = context.Collections.GetRedisLexicographicSet(key);
            bm.Add("zero");
            bm.AddRange(new [] { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve" });

            Assert.AreEqual(13, bm.Count);
            var suggestions = bm.AutoComplete("t", 3).ToList();
            Assert.AreEqual(3, suggestions.Count);
            Assert.AreEqual("ten", suggestions[0]);
            Assert.AreEqual("three", suggestions[1]);
            Assert.AreEqual("twelve", suggestions[2]);

            suggestions = bm.AutoComplete("tw").ToList();
            Assert.AreEqual(2, suggestions.Count);
            Assert.AreEqual("twelve", suggestions[0]);
            Assert.AreEqual("two", suggestions[1]);

            var lst = new List<string>();
            foreach (var s in bm)
            {
                lst.Add(s);
            }
            Assert.AreEqual(13, lst.Count);
            Assert.AreEqual("eight", lst[0]);
            Assert.AreEqual("eleven", lst[1]);
            Assert.AreEqual("two", lst[11]);
            Assert.AreEqual("zero", lst[12]);

            Assert.IsTrue(bm.Contains("zero"));
            bm.Remove("zero");
            Assert.IsFalse(bm.Contains("zero"));
            Assert.AreEqual(12, bm.Count);
        }

        // See this: https://github.com/StackExchange/StackExchange.Redis/issues/458
        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheLexSet_Match(Context context)
        {
            var key = "UT_CacheLexSet_Match";
            context.Cache.Remove(key);
            var bm = context.Collections.GetRedisLexicographicSet(key);
            bm.AddRange(new[] { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve" });
            Assert.AreEqual(12, bm.Match("*").Count());

            var lst = bm.Match("*eve*").ToList();
            Assert.AreEqual(2, lst.Count);
            Assert.AreEqual("eleven", lst[0]);
            Assert.AreEqual("seven", lst[1]);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheString(Context context)
        {
            var key = "UT_CacheString";
            context.Cache.Remove(key);
            var cs = context.Collections.GetRedisString(key);

            cs.SetRange(3, "Test");
            Assert.AreEqual(7, cs.Length);
            Assert.AreEqual("\0", cs.GetRange(0, 0));
            Assert.AreEqual("\0", cs[0, 0]);
            Assert.AreEqual("T", cs[3, 3]);
            Assert.AreEqual("t", cs[6, 6]);
            Assert.AreEqual("Test", cs[3, -1]);
            Assert.AreEqual("\0\0\0Test", cs[0, -1]);
            Assert.AreEqual("\0\0\0Test", cs[0, 999]);

            var len = cs.SetRange(0, "123");
            Assert.AreEqual(7, len);
            Assert.AreEqual(7, cs.Length);
            Assert.AreEqual("123Test", cs.GetRange());
            
            cs.SetRange(0, "abc");

            var lst = new List<byte>();
            foreach (byte b in cs)
            {
                lst.Add(b);
            }
            Assert.AreEqual("abcTest", Encoding.UTF8.GetString(lst.ToArray()));

            Assert.AreEqual(10, cs.Append("def"));
            Assert.AreEqual("abcTestdef", cs.ToString());

            cs.Set("new string");
            Assert.AreEqual("new string", cs.ToString());

            cs.Set(123.45);
            Assert.AreEqual("123.45", cs.ToString());

            cs.Set(12345);
            Assert.AreEqual("12345", cs.ToString());
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheStringAsync(Context context)
        {
            var key = "UT_CacheStringAsync";
            await context.Cache.RemoveAsync(key);
            var cs = context.Collections.GetRedisString(key);

            await cs.SetRangeAsync(3, "Test");
            Assert.AreEqual(7, cs.Length);
            Assert.AreEqual("\0", await cs.GetRangeAsync(0, 0));
            Assert.AreEqual("\0", cs[0, 0]);
            Assert.AreEqual("T", cs[3, 3]);
            Assert.AreEqual("t", cs[6, 6]);
            Assert.AreEqual("Test", cs[3, -1]);
            Assert.AreEqual("\0\0\0Test", cs[0, -1]);
            Assert.AreEqual("\0\0\0Test", cs[0, 999]);

            var len = await cs.SetRangeAsync(0, "123");
            Assert.AreEqual(7, len);
            Assert.AreEqual(7, cs.Length);
            Assert.AreEqual("123Test", await cs.GetRangeAsync());

            await cs.SetRangeAsync(0, "abc");

            var lst = new List<byte>();
            foreach (byte b in cs)
            {
                lst.Add(b);
            }
            Assert.AreEqual("abcTest", Encoding.UTF8.GetString(lst.ToArray()));

            Assert.AreEqual(10, await cs.AppendAsync("def"));
            Assert.AreEqual("abcTestdef", await cs.ToStringAsync());

            await cs.SetAsync("new string");
            Assert.AreEqual("new string", await cs.ToStringAsync());

            await cs.SetAsync(123.45);
            Assert.AreEqual("123.45", await cs.ToStringAsync());

            await cs.SetAsync(12345);
            Assert.AreEqual("12345", await cs.ToStringAsync());
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheStringGetSet(Context context)
        {
            var key = "UT_CacheStringGetSet";
            context.Cache.Remove(key);
            var cs = context.Collections.GetRedisString(key);
            var str = cs.GetSet("value");
            Assert.IsNull(str);
            str = cs.GetSet("new value");
            Assert.AreEqual("value", str);
            context.Cache.Remove(key);
            var integer = cs.GetSet(456);
            Assert.AreEqual(0, integer);
            Assert.AreEqual(456, cs.AsInteger());
            var fp = cs.GetSet(789.12);
            Assert.AreEqual(456.0, fp);
            Assert.AreEqual(789.12, cs.AsFloat());
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheStringGetSetAsync(Context context)
        {
            var key = "UT_CacheStringGetSetAsync";
            await context.Cache.RemoveAsync(key);
            var cs = context.Collections.GetRedisString(key);
            var str = await cs.GetSetAsync("value");
            Assert.IsNull(str);
            str = await cs.GetSetAsync("new value");
            Assert.AreEqual("value", str);
            await context.Cache.RemoveAsync(key);
            var integer = await cs.GetSetAsync(456);
            Assert.AreEqual(0, integer);
            Assert.AreEqual(456, await cs.AsIntegerAsync());
            var fp = cs.GetSet(789.12);
            Assert.AreEqual(456.0, fp);
            Assert.AreEqual(789.12, await cs.AsFloatAsync());
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheString_Unicode(Context context)
        {
            var key = "UT_CacheString_Unicode";
            context.Cache.Remove(key);
            var cs = context.Collections.GetRedisString(key);
            Assert.AreEqual(0, cs.Length);
            var str = "元来は有力貴族や諸大";
            cs.SetRange(0, str);
            Assert.AreEqual(10*3, cs.Length);
            var g = cs.GetRange(0, 2);
            Assert.AreEqual("元", cs[0, 2]);
            Assert.AreEqual("大", cs[-3, -1]);
            var lst = new List<byte>();
            foreach (byte b in cs)
            {
                lst.Add(b);
            }
            Assert.AreEqual(str, Encoding.UTF8.GetString(lst.ToArray()));
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheString_UnicodeAsync(Context context)
        {
            var key = "UT_CacheString_UnicodeAsync";
            await context.Cache.RemoveAsync(key);
            var cs = context.Collections.GetRedisString(key);
            Assert.AreEqual(0, cs.Length);
            var str = "元来は有力貴族や諸大";
            await cs.SetRangeAsync(0, str);
            Assert.AreEqual(10 * 3, cs.Length);
            var g = await cs.GetRangeAsync(0, 2);
            Assert.AreEqual("元", cs[0, 2]);
            Assert.AreEqual("大", cs[-3, -1]);
            var lst = new List<byte>();
            foreach (byte b in cs)
            {
                lst.Add(b);
            }
            Assert.AreEqual(str, Encoding.UTF8.GetString(lst.ToArray()));
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheString_AsInteger(Context context)
        {
            var key = "UT_CacheString_AsInteger";
            context.Cache.Remove(key);
            var str = context.Collections.GetRedisString(key);
            str.Set((long.MaxValue - 1).ToString());
            var value = str.IncrementBy(1);
            Assert.AreEqual(long.MaxValue, value);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheString_AsIntegerAsync(Context context)
        {
            var key = "UT_CacheString_AsIntegerAsync";
            await context.Cache.RemoveAsync(key);
            var str = context.Collections.GetRedisString(key);
            await str.SetAsync((long.MaxValue - 1).ToString());
            var value = await str.IncrementByAsync(1);
            Assert.AreEqual(long.MaxValue, value);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheString_AsFloat(Context context)
        {
            var key = "UT_CacheString_AsFloat";
            context.Cache.Remove(key);
            var str = context.Collections.GetRedisString(key);
            str.Append(Math.PI.ToString(CultureInfo.InvariantCulture));
            var fract = (double) 1/3;
            var value = str.IncrementByFloat(fract);
            Assert.AreEqual(Math.PI + fract, value, 0.000000001);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheString_AsFloatAsync(Context context)
        {
            var key = "UT_CacheString_AsFloatAsync";
            await context.Cache.RemoveAsync(key);
            var str = context.Collections.GetRedisString(key);
            await str.AppendAsync(Math.PI.ToString(CultureInfo.InvariantCulture));
            var fract = (double)1 / 3;
            var value = await str.IncrementByFloatAsync(fract);
            Assert.AreEqual(Math.PI + fract, value, 0.000000001);
        }

        [Test, TestCaseSource(typeof(Common), "Raw")]
        public void UT_CacheHash_Mix(Context context)
        {
            var key = "UT_CacheHash_Mix";
            context.Cache.Remove(key);
            var users = GetUsers();
            var redisDict = context.Collections.GetRedisDictionary<string, User>(key);
            redisDict.AddRange(users.ToDictionary(k => k.Id.ToString()));
            context.Cache.AddTagsToKey(redisDict.RedisKey, new[] { "tag1" });
            var returnedDict = context.Cache.GetHashedAll<User>(key);
            Assert.AreEqual(returnedDict["1"].Id, redisDict["1"].Id);
            context.Cache.InvalidateKeysByTag("tag1");
            Assert.AreEqual(0, redisDict.Count);
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
        public async Task UT_CacheSetObjectAsync(Context context)
        {
            string key1 = "UT_CacheSetObjectAsync";
            await context.Cache.RemoveAsync(key1);
            var users = GetUsers();
            var rs = context.Collections.GetRedisSet<User>(key1);
            await rs.AddRangeAsync(users);
            // Test GetEnumerator
            foreach (var item in rs)
            {
                Assert.IsTrue(users.Any(u => u.Id == item.Id));
            }
            // Test Count
            Assert.AreEqual(users.Count, (await rs.GetCountAsync()));
            // Test Contains
            Assert.IsTrue(await (rs.ContainsAsync(users[2])));
            // Test Add
            var newUser = new User() { Id = 5 };
            await rs.AddAsync(newUser);
            Assert.IsTrue(await (rs.ContainsAsync(newUser)));
            // Test Remove
            await rs.RemoveAsync(users[2]);
            Assert.IsFalse(await (rs.ContainsAsync(users[2])));
            // Test CopyTo
            User[] array = new User[50];
            rs.CopyTo(array, 10);
            Assert.AreEqual(users.Count, array.Count(x => x != null));
            // Test Clear
            await rs.ClearAsync();
            Assert.AreEqual(0, await rs.GetCountAsync());
            await rs.AddRangeAsync(new[] { new User() { Id = 3 }, new User() { Id = 1 }, new User() { Id = 2 } });
            Assert.AreEqual(3, await rs.GetCountAsync());
            await rs.RemoveAsync(new User() { Id = 1 });
            await rs.RemoveAsync(new User() { Id = 2 });
            Assert.AreEqual(1, await rs.GetCountAsync());
            Assert.IsTrue(await rs.ContainsAsync(new User() { Id = 3 }));
            // Test GetRandomMember
            var user = await rs.GetRandomMemberAsync();
            Assert.AreEqual(3, user.Id);
            // Test Pop
            user = await rs.PopAsync();
            Assert.AreEqual(3, user.Id);
            user = await rs.PopAsync();
            Assert.IsNull(user);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheSetObject_TTLAsync(Context context)
        {
            string key1 = "UT_CacheSetObject_TTLAsync";
            context.Cache.Remove(key1);
            var users = GetUsers();
            var rl = context.Collections.GetRedisSet<User>(key1);
            await rl.AddRangeAsync(users);
            rl.TimeToLive = TimeSpan.FromMilliseconds(1500);
            Assert.AreEqual(users.Count, await rl.GetCountAsync());
            Thread.Sleep(2000);
            Assert.AreEqual(0, await rl.GetCountAsync());
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public async Task UT_CacheSetObject_SetModifiersAsync(Context context)
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
            Assert.AreEqual(3, await abcSet.GetCountAsync());
            Assert.AreEqual(3, await cdeSet.GetCountAsync());
            abcSet.Clear();
            cdeSet.Clear();
        }


    }
}
