using System;
using System.Threading.Tasks;
using CachingFramework.Redis.Contracts.Providers;

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

        #region IPubSubProviderAsync implementation
        /// <summary>
        /// Subscribes to a specified channel for a speficied type.
        /// </summary>
        /// <typeparam name="T">The item type</typeparam>
        /// <param name="channel">The channel name.</param>
        /// <param name="action">The action where the first parameter is the channel name and the second is the object message.</param>
        public async Task SubscribeAsync<T>(string channel, Action<string, T> action)
        {
            var sub = RedisConnection.GetSubscriber();
            await sub.SubscribeAsync(GetChannel(channel), (ch, value) =>
            {
                var obj = Serializer.Deserialize<T>(value);
                action(ch, obj);
            }).ForAwait();
        }
        /// <summary>
        /// Subscribes to a specified channel for a speficied type.
        /// </summary>
        /// <typeparam name="T">The item type</typeparam>
        /// <param name="channel">The channel name.</param>
        /// <param name="action">The action where the first parameter is the object message.</param>
        public async Task SubscribeAsync<T>(string channel, Action<T> action)
        {
            var sub = RedisConnection.GetSubscriber();
            await sub.SubscribeAsync(GetChannel(channel), (ch, value) =>
            {
                var obj = Serializer.Deserialize<T>(value);
                action(obj);
            }).ForAwait();
        }
        /// <summary>
        /// Unsubscribes from the specified channel.
        /// </summary>
        /// <param name="channel">The channel name.</param>
        public async Task UnsubscribeAsync(string channel)
        {
            var sub = RedisConnection.GetSubscriber();
            await sub.UnsubscribeAsync(GetChannel(channel)).ForAwait();
        }
        /// <summary>
        /// Publishes an object to the specified channel.
        /// </summary>
        /// <typeparam name="T">The type of item to publish</typeparam>
        /// <param name="channel">The channel name.</param>
        /// <param name="item">The object message to send.</param>
        public async Task PublishAsync<T>(string channel, T item)
        {
            var sub = RedisConnection.GetSubscriber();
            await sub.PublishAsync(GetChannel(channel), Serializer.Serialize(item)).ForAwait();
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
            sub.Subscribe(GetChannel(channel), (ch, value) =>
            {
                var obj = Serializer.Deserialize<T>(value);
                action(ch, obj);
            });
        }
        /// <summary>
        /// Subscribes to a specified channel for a speficied type.
        /// </summary>
        /// <typeparam name="T">The item type</typeparam>
        /// <param name="channel">The channel name.</param>
        /// <param name="action">The action where the first parameter is the object message.</param>
        public void Subscribe<T>(string channel, Action<T> action)
        {
            var sub = RedisConnection.GetSubscriber();
            sub.Subscribe(GetChannel(channel), (ch, value) =>
            {
                var obj = Serializer.Deserialize<T>(value);
                action(obj);
            });
        }
        /// <summary>
        /// Unsubscribes from the specified channel.
        /// </summary>
        /// <param name="channel">The channel name.</param>
        public void Unsubscribe(string channel)
        {
            var sub = RedisConnection.GetSubscriber();
            sub.Unsubscribe(GetChannel(channel));
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
            sub.Publish(GetChannel(channel), Serializer.Serialize(item));
        }
        #endregion
    }
}