using System;
using CachingFramework.Redis.Contracts;

namespace CachingFramework.Redis.Providers
{
    /// <summary>
    /// Pub/Sub provider implementation using Redis.
    /// </summary>
    internal class RedisPubSubProvider : RedisProviderBase, IPubSubProvider
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisProviderBase" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public RedisPubSubProvider(RedisProviderContext context)
            : base(context)
        {
        }
        #endregion

        #region IPubSubProvider implementation
        /// <summary>
        /// Subscribes to a specified channel for a speficied type.
        /// </summary>
        /// <typeparam name="T">The item type</typeparam>
        /// <param name="channel">The channel name.</param>
        /// <param name="action">The action where the first parameter is the channel name and the second is the object message.</param>
        public void Subscribe<T>(string channel, Action<string, T> action)
        {
            var sub = RedisConnection.GetSubscriber();
            sub.Subscribe(channel, (ch, value) =>
            {
                var obj = Serializer.Deserialize<object>(value);
                if (obj is T)
                {
                    action(ch, (T)obj);
                }
            });
        }
        /// <summary>
        /// Unsubscribes from the specified channel.
        /// </summary>
        /// <param name="channel">The channel name.</param>
        public void Unsubscribe(string channel)
        {
            var sub = RedisConnection.GetSubscriber();
            sub.Unsubscribe(channel);
        }
        /// <summary>
        /// Publishes an object to the specified channel.
        /// </summary>
        /// <typeparam name="T">The type of item to publish</typeparam>
        /// <param name="channel">The channel name.</param>
        /// <param name="item">The object message to send.</param>
        public void Publish<T>(string channel, T item)
        {
            var sub = RedisConnection.GetSubscriber();
            sub.Publish(channel, Serializer.Serialize(item));
        }
        #endregion
    }
}