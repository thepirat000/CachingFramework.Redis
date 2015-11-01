using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Serializers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CachingFramework.Redis.UnitTest
{
    [TestClass]
    public class UnitTestRedisObjects
    {
        private static Context _context;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            _context = Common.GetContextAndFlush();
        }

        [TestMethod]
        public void UT_CacheList_Remove()
        {
            string key = "UT_CacheList_Remove";
            _context.Cache.Remove(key);
            var lst = _context.Collections.GetRedisList<string>(key);
            lst.AddRange(new [] { "test", "test", "anothertest" });
            Assert.AreEqual(3, lst.Count);
            lst.RemoveAt(0);
            Assert.AreEqual(2, lst.Count);
            Assert.AreEqual("test", lst[0]);
            Assert.AreEqual("anothertest", lst[1]);
        }

        [TestMethod]
        public void UT_CacheList_Insert()
        {
            string key = "UT_CacheList_Insert";
            _context.Cache.Remove(key);
            var rl = _context.Collections.GetRedisList<string>(key);
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

        [TestMethod]
        public void UT_CacheList_Trim()
        {
            string key = "UT_CacheList_Trim";
            _context.Cache.Remove(key);
            var rl = _context.Collections.GetRedisList<int>(key);
            rl.AddRange(Enumerable.Range(1, 100));
            Assert.AreEqual(100, rl.Count);
            rl.Trim(0, 9);
            Assert.AreEqual(10, rl.Count);
            Assert.AreEqual(1, rl[0]);
            Assert.AreEqual(10, rl[-1]);
        }


        [TestMethod]
        public void UT_CacheListObject()
        {
            string key1 = "UT_CacheListObject1";
            _context.Cache.Remove(key1);
            var users = GetUsers();
            var rl = _context.Collections.GetRedisList<int>(key1);
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

        [TestMethod]
        public void UT_CacheListPushPop()
        {
            string key = "UT_CacheListPushPop";
            _context.Cache.Remove(key);
            var users = GetUsers();
            var rl = _context.Collections.GetRedisList<User>(key);
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

        [TestMethod]
        public void UT_CacheListRemoveAt()
        {
            string key = "UT_CacheListRemoveAt";
            _context.Cache.Remove(key);
            var rl = _context.Collections.GetRedisList<string>(key);
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

        [TestMethod]
        public void UT_CacheListObjectTTL()
        {
            string key1 = "UT_CacheListObject_TTL1";
            _context.Cache.Remove(key1);
            var users = GetUsers();
            var rl = _context.Collections.GetRedisList<User>(key1);
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
            _context.Cache.Remove(key);
            var rl = _context.Collections.GetRedisList<User>(key);
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

        [TestMethod]
        public void UT_CacheDictionaryObject()
        {
            string key1 = "UT_CacheDictionaryObject1";
            _context.Cache.Remove(key1);
            var users = GetUsers();
            var rd = _context.Collections.GetRedisDictionary<int, User>(key1);
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

        [TestMethod]
        public void UT_CacheDictionaryObject_TTL()
        {
            string key1 = "UT_CacheDictionaryObjectTTL1";
            _context.Cache.Remove(key1);
            var users = GetUsers();
            var rl = _context.Collections.GetRedisDictionary<int, User>(key1);
            rl.AddRange(users.ToDictionary(k => k.Id));
            rl.TimeToLive = TimeSpan.FromMilliseconds(1500);
            Assert.AreEqual(users.Count, rl.Count);
            Thread.Sleep(2000);
            Assert.AreEqual(0, rl.Count);
        }

        [TestMethod]
        public void UT_CacheList_EXP()
        {
            string key = "UT_CacheList_EXP";
            _context.Cache.Remove(key);
            var set = _context.Collections.GetRedisSet<string>(key);
            set.AddRange(new [] { "test1", "test2", "test3" });
            set.Expiration = DateTime.Now.AddSeconds(2);
            var startedOn = DateTime.Now;
            Assert.AreEqual(3, set.Count);
            while (_context.Cache.KeyExists(key) && DateTime.Now < startedOn.AddSeconds(10))
            {
                Thread.Sleep(100);
            }
            Assert.IsFalse(_context.Cache.KeyExists(key));
            Assert.AreEqual(0, set.Count);
            var stoppedOn = DateTime.Now;
            var span = stoppedOn - startedOn;
            Assert.AreEqual(2, span.TotalSeconds, 1);
        }

        [TestMethod]
        public void UT_CacheList_StrObj()
        {
            string key = "UT_CacheList_StrObj";
            _context.Cache.Remove(key);
            _context.Cache.SetObject<string>(key, "test value 1");
            var obj = _context.Cache.GetObject<object>(key);
            Assert.IsTrue(obj is string);
            _context.Cache.SetObject<string>(key, Encoding.UTF8.GetString(new byte[] { 0x1f, 0x8b }));
            obj = _context.Cache.GetObject<object>(key);
            Assert.IsTrue(obj is string);
        }

        [TestMethod]
        public void UT_CacheSetObject()
        {
            string key1 = "UT_CacheSetObject1";
            _context.Cache.Remove(key1);
            var users = GetUsers();
            var rs = _context.Collections.GetRedisSet<User>(key1);
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
            // Test Remove Where
            rs.AddRange(new []{  new User() {Id = 3},  new User() {Id = 1},  new User() {Id = 2} });
            Assert.AreEqual(3, rs.Count);
            rs.RemoveWhere(u => u.Id <= 2);
            Assert.AreEqual(1, rs.Count);
            Assert.IsTrue(rs.Contains(new User() { Id = 3 }));
        }

        [TestMethod]
        public void UT_CacheSetObject_TTL()
        {
            string key1 = "UT_CacheSetObject_TTL";
            _context.Cache.Remove(key1);
            var users = GetUsers();
            var rl = _context.Collections.GetRedisSet<User>(key1);
            rl.AddRange(users);
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

            _context.Cache.Remove(keyAbc);
            _context.Cache.Remove(keyCde);
            var abcSet = _context.Collections.GetRedisSet<char>(keyAbc);
            abcSet.AddRange("ABC");
            
            var cdeSet = _context.Collections.GetRedisSet<char>(keyCde);
            cdeSet.AddRange("CDE");

            // Test Count
            Assert.AreEqual(3, abcSet.Count);
            Assert.AreEqual(3, cdeSet.Count);

            abcSet.Clear();
            cdeSet.Clear();
        }

        [TestMethod]
        public void UT_CacheSortedSet_GetRange()
        {
            var key = "UT_CacheSortedSet_GetRange";
            _context.Cache.Remove(key);
            var ss = _context.Collections.GetRedisSortedSet<User>(key);
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

        [TestMethod]
        public void UT_CacheSortedSet_SE_Issue287()
        {
            var key = "UT_CacheSortedSet_SE_Issue287";
            _context.Cache.Remove(key);
            var ss = _context.Collections.GetRedisSortedSet<User>(key);
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

        [TestMethod]
        public void UT_CacheSortedSet_GetRangeByRankNegative()
        {
            var key = "UT_CacheSortedSet_GetRangeByRankNegative";
            _context.Cache.Remove(key);
            var ss = _context.Collections.GetRedisSortedSet<string>(key);
            ss.AddRange(new[] { new SortedMember<string>(33, "c"), new SortedMember<string>(0, "a"), new SortedMember<string>(22, "b") });

            var byRank = ss.GetRangeByRank(-2, -1).ToList();
            var byRankRev = ss.GetRangeByRank(-2, -1, true).ToList();

            Assert.AreEqual(2, byRank.Count);
            Assert.AreEqual("b", byRank[0].Value);
            Assert.AreEqual("c", byRank[1].Value);
            Assert.AreEqual("a", byRankRev[1].Value);
            Assert.AreEqual("b", byRankRev[0].Value);
        }

        [TestMethod]
        public void UT_CacheSortedSet_etc()
        {
            var key = "UT_CacheSortedSet_etc";
            var ss = _context.Collections.GetRedisSortedSet<string>(key);
            _context.Cache.Remove(key);
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

        [TestMethod]
        public void UT_CacheBitmap()
        {
            var key = "UT_CacheBitmap";
            _context.Cache.Remove(key);
            var bm = _context.Collections.GetRedisBitmap(key);
            bm.Add(true);           // 11111111 
            Assert.IsFalse(bm.Contains(false));
            bm.Add(false);          // 11111111 00000000
            Assert.IsTrue(bm.Contains(false));
            bm.Add(true);           // 11111111 00000000 11111111
            bm.SetBit(24, true);    // 11111111 00000000 11111111 10000000
            bm.SetBit(25, false);   // 11111111 00000000 11111111 10000000
            bm.SetBit(26, true);    // 11111111 00000000 11111111 10100000
            bm.SetBit(27, true);    // 11111111 00000000 11111111 10110000
            Assert.IsFalse(bm.Contains(false, 0, 0));
            Assert.IsFalse(bm.Contains(true, 1, 1));
            Assert.IsTrue(bm.Contains(false, -1, -1));
            Assert.IsTrue(bm.Contains(true, -1, -1));
            Assert.AreEqual(0, bm.BitPosition(true));
            Assert.AreEqual(8, bm.BitPosition(false));
            Assert.AreEqual(24+1, bm.BitPosition(false, -1, -1));
            Assert.AreEqual(24+0, bm.BitPosition(true, -1, -1));
            Assert.AreEqual(false, bm.GetBit(25));
            Assert.AreEqual(true, bm.GetBit(26));
            Assert.AreEqual(true, bm.GetBit(27));
            Assert.AreEqual(3, bm.Count(-1, -1));
            Assert.AreEqual(11, bm.Count(-2, -1));
            Assert.AreEqual(11, bm.Count(2, -1));
            Assert.AreEqual(19, bm.Count());
            Assert.AreEqual(8, bm.Count(0, 1));
            var sb = new StringBuilder();
            foreach (var bit in bm)
            {
                sb.Append(bit ? '1' : '0');
            }
            Assert.AreEqual("11111111000000001111111110110000", sb.ToString());
            bm.SetBit(9999999, true);
            Assert.AreEqual(20, bm.Count());
            Assert.AreEqual(true, bm.GetBit(9999999));    
            bm.Clear();
            Assert.AreEqual(0, bm.Count());
            bm.Remove(false);
            Assert.AreEqual(1, bm.Count());
            Assert.AreEqual(true, bm.GetBit(0));
        }

        [TestMethod]
        public void UT_CacheLexSet()
        {
            var key = "UT_CacheLexSet";
            _context.Cache.Remove(key);
            var bm = _context.Collections.GetRedisLexicographicSet(key);
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

            Assert.AreEqual(12, bm.Match("*").Count());

            lst = bm.Match("*eve*").ToList();
            Assert.AreEqual(2, lst.Count);
            Assert.AreEqual("eleven", lst[0]);
            Assert.AreEqual("seven", lst[1]);
        }

        [TestMethod]
        public void UT_CacheString()
        {
            var key = "UT_CacheString";
            _context.Cache.Remove(key);
            var cs = _context.Collections.GetRedisString(key);

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

        }

        [TestMethod]
        public void UT_CacheString_Unicode()
        {
            var key = "UT_CacheString_Unicode";
            _context.Cache.Remove(key);
            var cs = _context.Collections.GetRedisString(key);
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

        [TestMethod]
        public void UT_CacheString_BigString()
        {
            var key = "UT_CacheString_BigString";
            int i = 9999999;
            _context.Cache.Remove(key);
            var cs = _context.Collections.GetRedisString(key);
            cs.SetRange(i, "test");
            Assert.AreEqual(i + 4, cs.Length);
            Assert.AreEqual("\0", cs[0, 0]);
            Assert.AreEqual("test", cs[i, -1]);
            var big = cs[0, -1];
            Assert.IsTrue(big.EndsWith("test"));
            Assert.AreEqual(i + 4, big.Length);
            cs.Clear();
            Assert.AreEqual(0, cs.Length);
        }

        [TestMethod]
        public void UT_CacheString_AsInteger()
        {
            var key = "UT_CacheString_AsInteger";
            _context.Cache.Remove(key);
            var str = _context.Collections.GetRedisString(key);
            str.Append((long.MaxValue - 1).ToString());
            var value = str.IncrementBy(1);
            Assert.AreEqual(long.MaxValue, value);
        }

        [TestMethod]
        public void UT_CacheString_AsFloat()
        {
            var key = "UT_CacheString_AsFloat";
            _context.Cache.Remove(key);
            var str = _context.Collections.GetRedisString(key);
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
