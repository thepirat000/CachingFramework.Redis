using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CachingFramework.Redis.UnitTest
{
    [TestClass]
    public class RedisStressTest
    {
        private static CacheContext _context;
        private string defaultConfig;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            _context = Common.GetContextAndFlush();
        }

        [TestMethod]
        public void UT_RedisStress_BigAddDelete()
        {
            string key = "UT_RedisStress_BigAddDelete";
            string tag = "UT_RedisStress_BigAddDelete-tag1";
            int total = 32000;
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < total; i++)
            {
                _context.SetObject(key + i, new User() { Id = i });
            }
            var secsSet = sw.Elapsed.TotalSeconds;
            sw = Stopwatch.StartNew();
            for (int i = 0; i < total; i++)
            {
                var user = _context.GetObject<User>(key + i);
                Assert.AreEqual(user.Id, i);
            }
            var secsGet = sw.Elapsed.TotalSeconds;
            sw = Stopwatch.StartNew();
            for (int i = 0; i < total; i++)
            {
                var removed = _context.Remove(key + i);
                Assert.IsTrue(removed);
            }
            var secsRem = sw.Elapsed.TotalSeconds;
        }

        [TestMethod]
        public void UT_CacheBigRemoveByTag()
        {
            string key = "UT_CacheBigRemoveByTag";
            string tag = "mytag";
            int total = 16000;
            for (int i = 0; i < total; i++)
            {
                _context.SetObject(key + i, new User() { Id = i }, new[] { tag });
            }
            var keys = _context.GetKeysByTag(new [] {tag});
            var sw = Stopwatch.StartNew();
            _context.InvalidateKeysByTag(tag);
            var secs = sw.Elapsed.TotalSeconds;
            var nokeys = _context.GetKeysByTag(new[] { tag });
            Assert.AreEqual(total, keys.Count);
            Assert.AreEqual(0, nokeys.Count);
        }

        [TestMethod]
        //[Ignore]
        public void UT_RedisBomb()
        {
            const string test = "UT_RedisBomb";
            Stress(100, test);
            Stress(1000, test);
            /*
            Stress(10000, test);
            Stress(100000, test);
            Stress(200000, test);
             Stress(300000, test);
             Stress(400000, test);
             Stress(500000, test);
             Stress(1000000, test);
            */
        }

        private void Stress(int keyCount, string test)
        {
            int toCreate = 0, toConsume = 0;
            RemoveKeys(keyCount, test);
            var sw = Stopwatch.StartNew();
            Parallel.Invoke(() =>
            {
                toCreate = CreateKeys(keyCount, test);
            }, () =>
            {
                toConsume = ConsumeValues(keyCount, test);
            });
            var createAndConsumeSeconds = sw.Elapsed.TotalSeconds;
            int timeouts = toCreate + toConsume;
            sw = Stopwatch.StartNew();
            ConsumeValues(keyCount, test);
            var consumeSeconds = sw.Elapsed.TotalSeconds;
            sw = Stopwatch.StartNew();
            RemoveKeys(keyCount, test);
            var removeSeconds = sw.Elapsed.TotalSeconds;

            Debug.WriteLine(
                "{0} keys.\nParallel Create and Consume time {1} secs.\nParallel Consume time {2} secs.\nRemoval time {3} secs.\nTimeouts: {4}.",
                keyCount, createAndConsumeSeconds, consumeSeconds, removeSeconds, timeouts);
        }

        [TestMethod]
        public void UT_RedisStress_GetKeysByTag()
        {
            const string test = "UT_RedisStress_GetKeysByTag";
            const int keyCount = 1500;
            for (int mod = 1; mod <= 216; mod++)
            {
                _context.InvalidateKeysByTag(GetTag(mod, test));
            }
            RemoveKeys(keyCount, test);
            CreateKeys(keyCount, test);
            ConsumeValues(keyCount, test);

            var hash = _context.GetKeysByTag(new [] {GetTag(1, test)});
            Assert.AreEqual(keyCount, hash.Count);

            for (int mod = 2; mod <= 216; mod++)
            {
                hash = _context.GetKeysByTag(new [] {GetTag(mod, test)});
                //assert all are multiple of mod
                Assert.IsFalse(hash.Any(s => int.Parse(s.Split(':')[0]) % mod != 0));
            }
            RemoveKeys(keyCount, test);
            for (int mod = 1; mod <= 216; mod++)
            {
                _context.InvalidateKeysByTag(GetTag(mod, test));
            }
        }


        [TestMethod]
        public void UT_RedisStress_GetAllTags()
        {
            const string test = "UT_RedisStress_GetAllTags";
            const int keyCount = 1500;
            var realTags = new HashSet<string>();
            for (int mod = 1; mod <= 216; mod++)
            {
                var tag = GetTag(mod, test);
                realTags.Add(tag);
                _context.InvalidateKeysByTag(tag);
            }
            RemoveKeys(keyCount, test);
            CreateKeys(keyCount, test);
            var tags = _context.GetAllTags();
            Debug.WriteLine("{0} {1}", tags.Count, realTags.Count);
            Assert.IsTrue(realTags.IsSubsetOf(tags));
        }

        [TestMethod]
        public void UT_CleanupTags()
        {
            // using the cleanup option
            const string key = "UT_CleanupTags";
            const string tag = "UT_CleanupTags-Tag";
            _context.InvalidateKeysByTag(tag);
            _context.Remove(key);
            _context.SetObject(key, "value", new [] { tag });
            var keys = _context.GetKeysByTag(new [] {tag}, true);
            Assert.IsTrue(keys.Contains(key));
            _context.Remove(key);
            keys = _context.GetKeysByTag(new [] {tag}, true);
            Assert.IsFalse(keys.Contains(key));
        }

        [TestMethod]
        public void UT_RedisStress_RemoveKeysByTags()
        {
            const string test = "UT_RedisStress_RemoveKeysByTags";
            const int keyCount = 3000;
            RemoveKeys(keyCount, test);
            for (int mod = 1; mod <= 216; mod++)
            {
                _context.InvalidateKeysByTag(GetTag(mod, test));
            }

            CreateKeys(keyCount, test);
            ConsumeValues(keyCount, test);

            var user = _context.GetObject<User>(GeyKey(1, test));
            Assert.IsNotNull(user);

            var hash = _context.GetKeysByTag(GetTag(1, test));
            var dict = new Dictionary<int, int>() { { 1, hash.Count } };
            for (int mod = 2; mod <= 36; mod++)
            {
                hash = _context.GetKeysByTag(GetTag(mod, test));
                dict.Add(mod, hash.Count);
            }
            var keys = _context.GetKeysByTag(new[] { GetTag(2, test), GetTag(3, test) });
            _context.InvalidateKeysByTag(new[] { GetTag(2, test), GetTag(3, test) });

            keys = _context.GetKeysByTag(new[] { GetTag(2, test), GetTag(3, test) });
            Assert.AreEqual(0, keys.Count);
            
            _context.InvalidateKeysByTag(new[] { GetTag(6, test) });

            _context.InvalidateKeysByTag(new[] { GetTag(5, test), GetTag(7, test) });

            _context.InvalidateKeysByTag(new[] { GetTag(1, test) });

            keys = _context.GetKeysByTag(new[] { GetTag(1, test), GetTag(2, test) });
            Assert.AreEqual(0, keys.Count);

            user = _context.GetObject<User>(GeyKey(1, test));
            Assert.IsNull(user);
            RemoveKeys(keyCount, test);
        }


        private void RemoveKeys(int count, string test)
        {
            Parallel.ForEach(Enumerable.Range(1, count), i =>
            {
                _context.Remove(GeyKey(i, test));
            });
        }
        private int CreateKeys(int count, string test)
        {
            int timeouts = 0;
            Debug.WriteLine("CreateKeys started. {0} {1} {2}.", test, count, DateTime.Now);
            Parallel.ForEach(Enumerable.Range(1, count), i =>
            {
            again:
                try
                {
                    _context.SetObject(GeyKey(i, test), GetValue(i), GetTags(i, test), TimeSpan.FromMinutes(15));
                }
                catch (TimeoutException)
                {
                    Debug.WriteLine("TIMEOUT when Setting {0} {1} ", i, DateTime.Now);
                    timeouts++;
                    Thread.Sleep(100);
                    goto again;
                }
            });
            Debug.WriteLine("----->>>>>>>>>>> CreateKeys FINISHED. {0} {1} {2}.", test, count, DateTime.Now);
            return timeouts;
        }

        private string[] GetTags(int i, string test)
        {
            var tags = new List<string>();
            for (int mod = 1; mod <= 216; mod++)
            {
                if (i % mod == 0)
                {
                    tags.Add(GetTag(mod, test));
                }
            }
            return tags.ToArray();
        }

        private string GetTag(int mod, string test)
        {
            return "This Is Tag " + mod + " for test " + test;
        }

        private int ConsumeValues(int count, string test)
        {
            int timeouts = 0;
            Debug.WriteLine("ComsumeValues started. {0} {1} {2}.", test, count, DateTime.Now);
            Parallel.ForEach(Enumerable.Range(1, count), i =>
            {
                int j = 1;
                User user;
            again:
                try
                {
                    user = _context.GetObject<User>(GeyKey(i, test));
                }
                catch (TimeoutException)
                {
                    Debug.WriteLine("TIMEOUT when Getting {0} {1}", i, DateTime.Now);
                    timeouts++;
                    Thread.Sleep(100);
                    goto again;
                }
                while (user == null)
                {
                    j++;
                again2:
                    try
                    {
                        user = _context.GetObject<User>(GeyKey(i, test));
                    }
                    catch (TimeoutException)
                    {
                        Debug.WriteLine("TIMEOUT when Getting {0} {1} {2}", i, j, DateTime.Now);
                        timeouts++;
                        Thread.Sleep(100);
                        goto again2;
                    }

                    if (j > 1000000)
                    {
                        throw new Exception("1000000 tries exceeded");
                    }
                }
            });
            Debug.WriteLine("----->>>>>>>>>>> ComsumeValues FINISHED. {0} {1} {2}.", test, count, DateTime.Now);
            return timeouts;
        }

        private string GeyKey(int i, string test)
        {
            return string.Format("{0}:For Test " + test, i);
        }
        private User GetValue(int i)
        {
            var user = new User()
            {
                Id = i,
                Deparments = new List<Department>()
            };
            for (int j = 0; j < 10; j++)
            {
                user.Deparments.Add(new Department()
                {
                    Id = j + 1,
                    Distance = (j + 1) * (decimal)Math.PI,
                    Location = new Location()
                    {
                        Id = (j + 1) * 10,
                        Name = string.Format("{0}", j + 1)
                    },
                    Size = (j + 1) * 23
                });
            }
            return user;
        }
    }

}
