using System;

namespace CachingFramework.Redis.Contracts.Providers
{
    /// <summary>
    /// A typed PubSub Provider interface
    /// </summary>
    internal interface IPubSubProvider
    {
        /// <summary>
        /// Subscribes to a specified channel for a speficied type.
        /// </summary>
        /// <typeparam name="T">The item type</typeparam>
        /// <param name="channel">The channel name.</param>
        /// <param name="action">The action where the first parameter is the channel name and the second is the object message.</param>
        void Subscribe<T>(string channel, Action<string, T> action);
        /// <summary>
        /// Unsubscribes from the specified channel.
        /// </summary>
        /// <param name="channel">The channel name.</param>
        void Unsubscribe(string channel);
        /// <summary>
        /// Publishes an object to the specified channel.
        /// </summary>
        /// <typeparam name="T">The type of item to publish</typeparam>
        /// <param name="channel">The channel name.</param>
        /// <param name="item">The item.</param>
        void Publish<T>(string channel, T item);
    }
}
