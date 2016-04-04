﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CachingFramework.Redis.Contracts;
using NUnit.Framework;

namespace CachingFramework.Redis.UnitTest
{
    [TestFixture]
    public class UnitTestKeyEvents
    {
        [Test, TestCaseSource(typeof(Common), "Json")]
        public void UT_SubscribeToAllEvents(Context context)
        {
            UT_SubscribeToEvents(context, new[] { KeyEvent.Set, KeyEvent.Set, KeyEvent.Delete, KeyEvent.Delete }, KeyEventSubscriptionType.All);
        }

        [Test, TestCaseSource(typeof(Common), "Json")]
        public void UT_SubscribeToSpaceEvents(Context context)
        {
            UT_SubscribeToEvents(context, new[] { KeyEvent.Set, KeyEvent.Delete }, KeyEventSubscriptionType.KeySpace);
        }

        [Test, TestCaseSource(typeof(Common), "Json")]
        public void UT_SubscribeToKeyEvents(Context context)
        {
            UT_SubscribeToEvents(context, new[] { KeyEvent.Set, KeyEvent.Delete }, KeyEventSubscriptionType.KeyEvent);
        }

        [Test, TestCaseSource(typeof(Common), "Json")]
        public void UT_SubscribeToSpecificEvent(Context context)
        {
            UT_SubscribeToEvents(context, new[] { KeyEvent.Delete }, eventType: KeyEvent.Delete);
        }

        [Test, TestCaseSource(typeof(Common), "Json")]
        public void UT_SubscribeToSpecificKey(Context context)
        {
            UT_SubscribeToEvents(context, new[] { KeyEvent.Set, KeyEvent.Delete }, key: "myKey");
        }

        private void UT_SubscribeToEvents(Context context, IList<KeyEvent> expectedEvents, KeyEventSubscriptionType? eventSubscriptionType = null, string key = null, KeyEvent? eventType = null)
        {
            ConcurrentQueue<KeyValuePair<string, KeyEvent>> result = new ConcurrentQueue<KeyValuePair<string, KeyEvent>>();
            CountdownEvent handle = new CountdownEvent(expectedEvents.Count);
            Action<string, KeyEvent> action = (k, e) =>
            {
                result.Enqueue(new KeyValuePair<string, KeyEvent>(k, e));
                handle.Signal();
            };

            Action unsubscribeAction = () => { };

            if (key == null && !eventType.HasValue && eventSubscriptionType.HasValue)
            {
                context.KeyEvents.Subscribe(eventSubscriptionType.Value, action);
                unsubscribeAction = () => context.KeyEvents.UnSubscribe(eventSubscriptionType.Value);
            }
            else if (key != null)
            {
                context.KeyEvents.Subscribe(key, action);
                unsubscribeAction = () => context.KeyEvents.UnSubscribe(key);
            }
            else if (eventType.HasValue)
            {
                context.KeyEvents.Subscribe(eventType.Value, action);
                unsubscribeAction = () => context.KeyEvents.UnSubscribe(eventType.Value);
            }

            var objectKey = key ?? Guid.NewGuid().ToString();

            context.Cache.SetObject(objectKey, new { Name = "alex", Created = DateTime.UtcNow });
            context.Cache.Remove(objectKey);

            Assert.IsTrue(handle.Wait(5000));
            Assert.AreEqual(expectedEvents.Count, result.Count);

            foreach (var expectedEvent in expectedEvents)
            {
                KeyValuePair<string, KeyEvent> e;
                Assert.IsTrue(result.TryDequeue(out e));
                Assert.AreEqual(expectedEvent, e.Value);
                Assert.AreEqual(objectKey, e.Key);
            }

            //Now test Unsubscribe. No more events should be received in queue and handle will timeout.
            handle.Reset(1);
            unsubscribeAction();
            context.Cache.SetObject(objectKey, new { Name = "alex", Created = DateTime.UtcNow }, TimeSpan.FromMilliseconds(500));
            Assert.IsFalse(handle.Wait(1000));
            Assert.IsTrue(result.IsEmpty);

            context.KeyEvents.UnSubscribe(KeyEventSubscriptionType.All);
        }


    }
}
