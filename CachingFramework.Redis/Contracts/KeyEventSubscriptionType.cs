using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CachingFramework.Redis.Contracts
{
    public enum KeyEventSubscriptionType
    {
        [Text("__key*__:*")]
        All = 0,
        [Text("__keyspace@__:*")]
        KeySpace,
        [Text("__keyevent@__:*")]
        KeyEvent
    }
}
