using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CachingFramework.Redis.NewtonsoftJson
{
    public class Context : Redis.RedisContext
    {
        public Context()
            : base("localhost:6379", new NewtonsoftJsonSerializer())
        { }
        public Context(string configuration)
            : base(configuration, new NewtonsoftJsonSerializer())
        { }
    }
}
