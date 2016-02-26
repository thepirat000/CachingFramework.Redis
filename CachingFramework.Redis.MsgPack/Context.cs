using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CachingFramework.Redis.MsgPack
{
    public class Context : Redis.Context
    {
        public Context()
            : base("localhost:6379", new MsgPackSerializer())
        { }
        public Context(string configuration)
            : base(configuration, new MsgPackSerializer())
        { }
    }
}
