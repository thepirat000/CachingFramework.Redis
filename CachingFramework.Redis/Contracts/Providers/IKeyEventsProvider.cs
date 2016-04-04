using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CachingFramework.Redis.Contracts.Providers
{
    public interface IKeyEventsProvider
    {
        void Subscribe(KeyEventSubscriptionType subscriptionType, Action<string, KeyEvent> action);
        void Subscribe(string key, Action<string, KeyEvent> action);
        void Subscribe(KeyEvent keyEvent, Action<string, KeyEvent> action);

        void UnSubscribe(KeyEventSubscriptionType subscriptionType);
        void UnSubscribe(string key);
        void UnSubscribe(KeyEvent keyEvent);
    }
}
