using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Contracts.RedisObjects;
using CachingFramework.Redis.RedisObjects;
using CachingFramework.Redis.Serializers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CachingFramework.Redis.UnitTest
{
    [TestClass]
    public class UnitTestRedisObjects
    {
        private static CacheContext _cache;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            _cache = Common.GetContextAndFlush();
        }

        [TestMethod]
        public void UT_CacheListObject()
        {
            string key1 = "UT_CacheListObject1";
            _cache.Remove(key1);
            var users = GetUsers();
            var rl = _cache.GetCachedList<int>(key1);
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
            _cache.Remove(key);
            var users = GetUsers();
            var rl = _cache.GetCachedList<User>(key);
            rl.AddRange(users);
            rl.AddFirst(new User() { Id = 0 });
            rl.AddLast(new User() { Id = 666 });
            Assert.AreEqual(0, rl[0].Id);
            Assert.AreEqual(0, rl.First.Id);
            Assert.AreEqual(666, rl.Last.Id);
            var remf = rl.RemoveFirst();
            Assert.AreEqual(0, remf.Id);
            var reml = rl.RemoveLast();
            Assert.AreEqual(666, reml.Id);
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
            _cache.Remove(key1);
            var users = GetUsers();
            var rd = _cache.GetCachedDictionary<int, User>(key1);
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
            _cache.Remove(key1);
            var users = GetUsers();
            var rl = _cache.GetCachedDictionary<int, User>(key1);
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
            _cache.Remove(key);
            var set = _cache.GetCachedSet<string>(key);
            set.AddRange(new [] { "test1", "test2", "test3" });
            set.Expiration = DateTime.Now.AddSeconds(2);
            var startedOn = DateTime.Now;
            Assert.AreEqual(3, set.Count);
            while (_cache.KeyExists(key) && DateTime.Now < startedOn.AddSeconds(10))
            {
                Thread.Sleep(100);
            }
            Assert.IsFalse(_cache.KeyExists(key));
            Assert.AreEqual(0, set.Count);
            var stoppedOn = DateTime.Now;
            var span = stoppedOn - startedOn;
            Assert.AreEqual(2, span.TotalSeconds, 1);
        }


        [TestMethod]
        public void UT_CacheSetObject()
        {
            string key1 = "UT_CacheSetObject1";
            _cache.Remove(key1);
            var users = GetUsers();
            var rs = _cache.GetCachedSet<User>(key1);
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
        }

        [TestMethod]
        public void UT_CacheSetObject_TTL()
        {
            string key1 = "UT_CacheSetObject_TTL";
            _cache.Remove(key1);
            var users = GetUsers();
            var rl = _cache.GetCachedSet<User>(key1);
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

            _cache.Remove(keyAbc);
            _cache.Remove(keyCde);
            var abcSet = _cache.GetCachedSet<char>(keyAbc);
            abcSet.AddRange("ABC");
            
            var cdeSet = _cache.GetCachedSet<char>(keyCde);
            cdeSet.AddRange("CDE");

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
            abcSet.AddRange("AB");

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

        [TestMethod]
        public void UT_CacheSortedSet_GetRange()
        {
            var key = "UT_CacheSortedSet_GetRange";
            _cache.Remove(key);
            var ss = _cache.GetCachedSortedSet<User>(key);
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
            // This seems to be a StackExhange.Redis issue: https://github.com/StackExchange/StackExchange.Redis/issues/287
            //Assert.AreEqual(double.PositiveInfinity, byRank[3].Score);

            var byScore = ss.GetRangeByScore(12.34, 23.449).ToList();
            Assert.AreEqual(1, byScore.Count);
            Assert.AreEqual(users[0].Id, byScore[0].Value.Id);

            byScore = ss.GetRangeByScore(12.34, 23.45).ToList();
            Assert.AreEqual(2, byScore.Count);
            Assert.AreEqual(users[1].Id, byScore[1].Value.Id);
        }

        [TestMethod]
        public void UT_CacheSortedSet_GetRangeByRankNegative()
        {
            var key = "UT_CacheSortedSet_GetRangeByRankNegative";
            _cache.Remove(key);
            var ss = _cache.GetCachedSortedSet<string>(key);
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
            var ss = _cache.GetCachedSortedSet<string>(key);
            _cache.Remove(key);
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
            _cache.Remove(key);
            var bm = _cache.GetCachedBitmap(key);
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
            _cache.Remove(key);
            var bm = _cache.GetCachedLexicographicSet(key);

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
