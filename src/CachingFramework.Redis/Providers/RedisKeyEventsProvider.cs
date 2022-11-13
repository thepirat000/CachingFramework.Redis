using System;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Contracts.Providers;

namespace CachingFramework.Redis.Providers
{
    internal class RedisKeyEventsProvider : RedisProviderBase, IKeyEventsProvider
    {
        public RedisKeyEventsProvider(RedisProviderContext context) : base(context) { }

        public void Subscribe(KeyEventSubscriptionType subscriptionType, Action<string, KeyEvent> action)
        {
            SubscribeToRedis(GetEventChannelName(subscriptionType), action);
        }

        public void Subscribe(string key, Action<string, KeyEvent> action)
        {
            SubscribeToRedis(GetEventChannelName(KeyEventSubscriptionType.KeySpace, key), action);
        }

        public void Subscribe(KeyEvent keyEvent, Action<string, KeyEvent> action)
        {
            SubscribeToRedis(GetEventChannelName(KeyEventSubscriptionType.KeyEvent, eventType: keyEvent), action);
        }

        public void Unsubscribe(KeyEventSubscriptionType subscriptionType)
        {
            UnsubscribeFromRedis(GetEventChannelName(subscriptionType));
        }

        public void Unsubscribe(string key)
        {
            UnsubscribeFromRedis(GetEventChannelName(KeyEventSubscriptionType.KeySpace, key));
        }

        public void Unsubscribe(KeyEvent keyEvent)
        {
            UnsubscribeFromRedis(GetEventChannelName(KeyEventSubscriptionType.KeyEvent, eventType: keyEvent));
        }

        private void SubscribeToRedis(string channel, Action<string, KeyEvent> action)
        {
            var sub = RedisConnection.GetSubscriber();
            sub.Subscribe(channel, (ch, value) => { ProcessNotification(ch, value, action); });
        }

        private void UnsubscribeFromRedis(string channel)
        {
            var sub = RedisConnection.GetSubscriber();
            sub.Unsubscribe(channel);
        }


        private void ProcessNotification(string channel, string value, Action<string, KeyEvent> action)
        {
            string eventType, key;

            if (channel.StartsWith("__keyevent@"))
            {
                eventType = channel.Split(':')[1];
                key = value;
            }
            else if (channel.StartsWith("__keyspace@"))
            {
                key = channel.Split(':')[1];
                eventType = value;
            }
            else
            {
                throw new NotImplementedException(string.Format("Parsing not implemented for event channel {0}", channel));
            }
            if (action != null)
            {
                action(key, TextAttributeCache<KeyEvent>.Instance.GetEnumValue(eventType));
            }
        }

        private string GetEventChannelName(KeyEventSubscriptionType subscriptionType, string key = null, KeyEvent? eventType = null)
        {
            var keyPrefix = _context.DatabaseOptions?.KeyPrefix ?? "";
            switch (subscriptionType)
            {
                case KeyEventSubscriptionType.All:
                    return "__key*__:*";
                case KeyEventSubscriptionType.KeyEvent:
                    return string.Format("__keyevent@*__:{0}", eventType.HasValue ? TextAttributeCache<KeyEvent>.Instance.GetEnumText(eventType.Value) : "*");
                case KeyEventSubscriptionType.KeySpace:
                    return string.Format("__keyspace@*__:{0}", keyPrefix + (key ?? "*"));
                default:
                    throw new NotImplementedException(string.Format("Subscription not implemented for type: {0}", subscriptionType));
            }
        }
    }
}
