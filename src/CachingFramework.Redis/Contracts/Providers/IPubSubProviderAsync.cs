using System;
using System.Threading.Tasks;

namespace CachingFramework.Redis.Contracts.Providers
{
    /// <summary>
    /// A typed PubSub Provider Async interface
    /// </summary>
    public interface IPubSubProviderAsync
    {
        /// <summary>
        /// Subscribes to a specified channel for a speficied type.
        /// </summary>
        /// <typeparam name="T">The item type</typeparam>
        /// <param name="channel">The channel name.</param>
        /// <param name="action">The action where the first parameter is the channel name and the second is the object message.</param>
        Task SubscribeAsync<T>(string channel, Action<string, T> action);
        /// <summary>
        /// Subscribes to a specified channel for a speficied type.
        /// </summary>
        /// <typeparam name="T">The item type</typeparam>
        /// <param name="channel">The channel name.</param>
        /// <param name="action">The action where the first parameter is the object message.</param>
        Task SubscribeAsync<T>(string channel, Action<T> action);
        /// <summary>
        /// Unsubscribes from the specified channel.
        /// </summary>
        /// <param name="channel">The channel name.</param>
        Task UnsubscribeAsync(string channel);
        /// <summary>
        /// Publishes an object to the specified channel.
        /// </summary>
        /// <typeparam name="T">The type of item to publish</typeparam>
        /// <param name="channel">The channel name.</param>
        /// <param name="item">The item.</param>
        Task PublishAsync<T>(string channel, T item);
    }
}
