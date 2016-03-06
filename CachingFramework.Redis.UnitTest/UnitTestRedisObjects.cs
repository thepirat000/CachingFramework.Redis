using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.RedisObjects;
using CachingFramework.Redis.Serializers;
using NUnit.Framework;

namespace CachingFramework.Redis.UnitTest
{
    [TestFixture]
    public class UnitTestRedisObjects
    {
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

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheListRemoveAt(Context context)
        {
            string key = "UT_CacheListRemoveAt";
            context.Cache.Remove(key);
            var rl = context.Collections.GetRedisList<string>(key, new RawSerializer());
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

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_CacheList_EXP(Context context)
        {
            string key = "UT_CacheList_EXP";
            context.Cache.Remove(key);
            var set = context.Collections.GetRedisSet<string>(key);
            set.AddRange(new [] { "test1", "test2", "test3" });
            set.Expiration = Common.ServerNow.AddSeconds(2);
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
        }

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

        [Test, TestCaseSource(typeof(Common), "All")]
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
                sb.Append(bit.ToString());
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

        [Test, TestCaseSource(typeof (Common), "Raw")]
        public void UT_CacheBitmapBitField(Context context)
        {
            var key = "UT_CacheBitmapBitField";
            context.Cache.Remove(key);
            var rb = context.Collections.GetRedisBitmap(key);
            var ex = rb.BitfieldSet(BitfieldType.u4, 0, 14, false, OverflowType.Fail);
            var n1 = rb.BitfieldGet<decimal>(BitfieldType.u4, 0);
            Assert.AreEqual(0, ex);
            Assert.AreEqual(14, n1);
            rb.BitfieldSet(BitfieldType.u16, 0, 0xb525);
            Assert.AreEqual(0xb5, rb.BitfieldGet<int>(BitfieldType.u8, 0));
            Assert.AreEqual(0x25, rb.BitfieldGet<uint>(BitfieldType.u8, 1, true));

            Assert.AreEqual(0x2, rb.BitfieldGet<byte>(BitfieldType.u3, 4));
            Assert.AreEqual(0x12, rb.BitfieldGet<sbyte>(BitfieldType.u5, 7));
            Assert.AreEqual(0x2, rb.BitfieldGet<long>(BitfieldType.u3, 2, true));
        }

        [Test, TestCaseSource(typeof(Common), "Raw")]
        public void UT_CacheBitmapBitField_Overflow(Context context)
        {
            var key = "UT_CacheBitmapBitField_Overflow";
            context.Cache.Remove(key);
            var rb = context.Collections.GetRedisBitmap(key);
            Assert.Throws<OverflowException>(() => rb.BitfieldSet(BitfieldType.u1, 0, -2, false, OverflowType.Fail));
            Assert.DoesNotThrow(() => rb.BitfieldSet(BitfieldType.u1, 0, -2));
            Assert.DoesNotThrow(() => rb.BitfieldSet(BitfieldType.u1, 0, -2, false, OverflowType.Saturation));
        }

        [Test, TestCaseSource(typeof(Common), "Raw")]
        public void UT_CacheBitmapBitField_WrapSaturation(Context context)
        {
            var key = "UT_CacheBitmapBitField_WrapSaturation";
            context.Cache.Remove(key);
            var rb = context.Collections.GetRedisBitmap(key);
            rb.BitfieldSet(BitfieldType.u13, 10, 8191, true);
            Assert.AreEqual(8191, rb.BitfieldGet<int>(BitfieldType.u13, 10, true));
            rb.BitfieldIncrementBy(BitfieldType.u13, 10, 4, true);
            Assert.AreEqual(3, rb.BitfieldGet<UInt16>(BitfieldType.u13, 10, true));
            rb.BitfieldSet(BitfieldType.u13, 4, 8191);
            rb.BitfieldIncrementBy(BitfieldType.u13, 4, 999, false, OverflowType.Saturation);
            Assert.AreEqual(8191, rb.BitfieldGet<UInt32>(BitfieldType.u13, 4));
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
        public void UT_CacheString_AsFloat(Context context)
        {
            var key = "UT_CacheString_AsFloat";
            context.Cache.Remove(key);
            var str = context.Collections.GetRedisString(key);
            str.Append(Math.PI.ToString());
            var fract = (double) 1/3;
            var value = str.IncrementByFloat(fract);
            Assert.AreEqual(Math.PI + fract, value, 0.000000001);
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
