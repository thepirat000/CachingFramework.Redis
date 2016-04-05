using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CachingFramework.Redis.Contracts
{
    public enum KeyEventSubscriptionType
    {
        All = 0,
        KeySpace,
        KeyEvent
    }
}
