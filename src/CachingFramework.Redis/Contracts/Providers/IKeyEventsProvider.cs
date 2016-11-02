using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CachingFramework.Redis.Contracts.Providers
{
    /// <summary>
    /// Key/Event space notifications API.
    /// </summary>
    public interface IKeyEventsProvider
    {
        /// <summary>
        /// Subscribes to the specified type of events.
        /// </summary>
        /// <param name="subscriptionType">The subscription type.</param>
        /// <param name="action">Operation to be performed when the event occurs. 
        /// First parameter is the key affected. Second parameter is the event type.</param>
        void Subscribe(KeyEventSubscriptionType subscriptionType, Action<string, KeyEvent> action);
        /// <summary>
        /// Subscribes to the key-space events for the given key.
        /// </summary>
        /// <param name="key">The redis key.</param>
        /// <param name="action">Operation to be performed when the event occurs. 
        /// First parameter is the key affected. Second parameter is the event type.</param>
        void Subscribe(string key, Action<string, KeyEvent> action);
        /// <summary>
        /// Subscribes the specified key event type.
        /// </summary>
        /// <param name="keyEvent">The key event.</param>
        /// <param name="action">Operation to be performed when the event occurs. 
        /// First parameter is the key affected. Second parameter is the event type.</param>
        void Subscribe(KeyEvent keyEvent, Action<string, KeyEvent> action);
        /// <summary>
        /// Unsubscribes from the specified type of events.
        /// </summary>
        /// <param name="subscriptionType">The subscription type.</param>
        void Unsubscribe(KeyEventSubscriptionType subscriptionType);
        /// <summary>
        /// Unsubscribes from the key-space events for the given key.
        /// </summary>
        /// <param name="key">The redis key.</param>
        void Unsubscribe(string key);
        /// <summary>
        /// Unsubscribes from the specified key event type.
        /// </summary>
        /// <param name="keyEvent">The key event.</param>
        void Unsubscribe(KeyEvent keyEvent);
    }
}
