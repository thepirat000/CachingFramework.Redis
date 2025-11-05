#if (NET462)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace CachingFramework.Redis.UnitTest
{
    [TestFixture]
    public class RedisStressTest
    {
        [Test, TestCaseSource(typeof(Common), "Raw")]
        public void UT_RedisStress_BigAddDelete(RedisContext context)
        {
            string key = "UT_RedisStress_BigAddDelete";
            int total = 1000;
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < total; i++)
            {
                context.Cache.SetObject(key + i, new User() { Id = i });
            }
            var secsSet = sw.Elapsed.TotalSeconds;
            sw = Stopwatch.StartNew();
            for (int i = 0; i < total; i++)
            {
                var user = context.Cache.GetObject<User>(key + i);
                ClassicAssert.AreEqual(user.Id, i);
            }
            var secsGet = sw.Elapsed.TotalSeconds;
            sw = Stopwatch.StartNew();
            for (int i = 0; i < total; i++)
            {
                var removed = context.Cache.Remove(key + i);
                ClassicAssert.IsTrue(removed);
            }
            var secsRem = sw.Elapsed.TotalSeconds;
        }

        [Test, TestCaseSource(typeof(Common), "Bin")]
        public void UT_CacheBigRemoveByTag(RedisContext context)
        {
            string key = "UT_CacheBigRemoveByTag";
            string tag = "mytag";
            int total = 1000;
            for (int i = 0; i < total; i++)
            {
                context.Cache.SetObject(key + i, new User() { Id = i }, new[] { tag });
            }
            var keys = context.Cache.GetKeysByTag(new [] {tag});
            var sw = Stopwatch.StartNew();
            context.Cache.InvalidateKeysByTag(tag);
            var secs = sw.Elapsed.TotalSeconds;
            var nokeys = context.Cache.GetKeysByTag(new[] { tag });
            ClassicAssert.AreEqual(total, keys.Count());
            ClassicAssert.AreEqual(0, nokeys.Count());
        }

        [Test, TestCaseSource(typeof(Common), "Bin")]
        public void UT_RedisBomb(RedisContext context)
        {
            const string test = "UT_RedisBomb";
            Stress(100, test, context);
            Stress(1000, test, context);
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

        [Test, TestCaseSource(typeof(Common), "Bin")]
        public void UT_RedisStress_GetAllTags(RedisContext context)
        {
            const string test = "UT_RedisStress_GetAllTags";
            const int keyCount = 1500;
            var realTags = new HashSet<string>();
            for (int mod = 1; mod <= 216; mod++)
            {
                var tag = GetTag(mod, test);
                realTags.Add(tag);
                context.Cache.InvalidateKeysByTag(tag);
            }
            RemoveKeys(keyCount, test, context);
            CreateKeys(keyCount, test, context);
            var tags = context.Cache.GetAllTags();
            Debug.WriteLine("{0} {1}", tags.Count(), realTags.Count);
            ClassicAssert.IsTrue(realTags.IsSubsetOf(tags));
        }

        private void Stress(int keyCount, string test, RedisContext context)
        {
            int toCreate = 0, toConsume = 0;
            RemoveKeys(keyCount, test, context);
            var sw = Stopwatch.StartNew();
            Parallel.Invoke(() =>
            {
                toCreate = CreateKeys(keyCount, test, context);
            }, () =>
            {
                toConsume = ConsumeValues(keyCount, test, context);
            });
            var createAndConsumeSeconds = sw.Elapsed.TotalSeconds;
            int timeouts = toCreate + toConsume;
            sw = Stopwatch.StartNew();
            ConsumeValues(keyCount, test, context);
            var consumeSeconds = sw.Elapsed.TotalSeconds;
            sw = Stopwatch.StartNew();
            RemoveKeys(keyCount, test, context);
            var removeSeconds = sw.Elapsed.TotalSeconds;

            Debug.WriteLine(
                "{0} keys.\nParallel Create and Consume time {1} secs.\nParallel Consume time {2} secs.\nRemoval time {3} secs.\nTimeouts: {4}.",
                keyCount, createAndConsumeSeconds, consumeSeconds, removeSeconds, timeouts);
        }

        [Test, TestCaseSource(typeof(Common), "MsgPack")]
        public void UT_RedisStress_GetKeysByTag(RedisContext context)
        {
            const string test = "UT_RedisStress_GetKeysByTag";
            const int keyCount = 1500;
            for (int mod = 1; mod <= 216; mod++)
            {
                context.Cache.InvalidateKeysByTag(GetTag(mod, test));
            }
            RemoveKeys(keyCount, test, context);
            CreateKeys(keyCount, test, context);
            ConsumeValues(keyCount, test, context);

            var hash = context.Cache.GetKeysByTag(new [] {GetTag(1, test)});
            ClassicAssert.AreEqual(keyCount, hash.Count());

            for (int mod = 2; mod <= 216; mod++)
            {
                hash = context.Cache.GetKeysByTag(new [] {GetTag(mod, test)});
                //assert all are multiple of mod
                ClassicAssert.IsFalse(hash.Any(s => int.Parse(s.Split(':')[0]) % mod != 0));
            }
            RemoveKeys(keyCount, test, context);
            for (int mod = 1; mod <= 216; mod++)
            {
                context.Cache.InvalidateKeysByTag(GetTag(mod, test));
            }
        }

        [Test, TestCaseSource(typeof(Common), "Json")]
        public void UT_CleanupTags(RedisContext context)
        {
            // using the cleanup option
            const string key = "UT_CleanupTags";
            const string tag = "UT_CleanupTags-Tag";
            context.Cache.InvalidateKeysByTag(tag);
            context.Cache.Remove(key);
            context.Cache.SetObject(key, "value", new [] { tag });
            var keys = context.Cache.GetKeysByTag(new [] {tag}, true);
            ClassicAssert.IsTrue(keys.Contains(key));
            context.Cache.Remove(key);
            keys = context.Cache.GetKeysByTag(new [] {tag}, true);
            ClassicAssert.IsFalse(keys.Contains(key));
        }

        [Test, TestCaseSource(typeof(Common), "Raw")]
        public void UT_RedisStress_RemoveKeysByTags(RedisContext context)
        {
            const string test = "UT_RedisStress_RemoveKeysByTags";
            const int keyCount = 3000;
            RemoveKeys(keyCount, test, context);
            for (int mod = 1; mod <= 216; mod++)
            {
                context.Cache.InvalidateKeysByTag(GetTag(mod, test));
            }

            CreateKeys(keyCount, test, context);
            ConsumeValues(keyCount, test, context);

            var user = context.Cache.GetObject<User>(GeyKey(1, test));
            ClassicAssert.IsNotNull(user);

            var hash = context.Cache.GetKeysByTag(new [] { GetTag(1, test) });
            var dict = new Dictionary<int, int>() { { 1, hash.Count() } };
            for (int mod = 2; mod <= 36; mod++)
            {
                hash = context.Cache.GetKeysByTag(new [] { GetTag(mod, test) });
                dict.Add(mod, hash.Count());
            }
            var keys = context.Cache.GetKeysByTag(new[] { GetTag(2, test), GetTag(3, test) });
            context.Cache.InvalidateKeysByTag(new[] { GetTag(2, test), GetTag(3, test) });

            keys = context.Cache.GetKeysByTag(new[] { GetTag(2, test), GetTag(3, test) });
            ClassicAssert.AreEqual(0, keys.Count());
            
            context.Cache.InvalidateKeysByTag(new[] { GetTag(6, test) });

            context.Cache.InvalidateKeysByTag(new[] { GetTag(5, test), GetTag(7, test) });

            context.Cache.InvalidateKeysByTag(new[] { GetTag(1, test) });

            keys = context.Cache.GetKeysByTag(new[] { GetTag(1, test), GetTag(2, test) });
            ClassicAssert.AreEqual(0, keys.Count());

            user = context.Cache.GetObject<User>(GeyKey(1, test));
            ClassicAssert.IsNull(user);
            RemoveKeys(keyCount, test, context);
        }


        [Test, TestCaseSource(typeof(Common), "Raw")]
        public void UT_CacheString_BigString(RedisContext context)
        {
            var key = "UT_CacheString_BigString";
            int i = 999999;
            context.Cache.Remove(key);
            var cs = context.Collections.GetRedisString(key);
            cs.SetRange(i, "test");
            ClassicAssert.AreEqual(i + 4, cs.Length);
            ClassicAssert.AreEqual("\0", cs[0, 0]);
            ClassicAssert.AreEqual("test", cs[i, -1]);
            var big = cs[0, -1];
            ClassicAssert.IsTrue(big.EndsWith("test"));
            ClassicAssert.AreEqual(i + 4, big.Length);
            cs.Clear();
            ClassicAssert.AreEqual(0, cs.Length);
        }

        private void RemoveKeys(int count, string test, RedisContext context)
        {
            foreach(int i in Enumerable.Range(1, count))
            {
                context.Cache.Remove(GeyKey(i, test));
            };
        }
        private int CreateKeys(int count, string test, RedisContext context)
        {
            int timeouts = 0;
            Debug.WriteLine("CreateKeys started. {0} {1} {2}.", test, count, DateTime.Now);
            Parallel.ForEach(Enumerable.Range(1, count), i =>
            {
            again:
                try
                {
                    context.Cache.SetObject(GeyKey(i, test), GetValue(i), GetTags(i, test), TimeSpan.FromMinutes(15));
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

        private int ConsumeValues(int count, string test, RedisContext context)
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
                    user = context.Cache.GetObject<User>(GeyKey(i, test));
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
                        user = context.Cache.GetObject<User>(GeyKey(i, test));
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
#endif
