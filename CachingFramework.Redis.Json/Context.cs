using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CachingFramework.Redis.Json
{
    public class Context : Redis.Context
    {
        public Context()
            : base("localhost:6379", new JsonSerializer()) 
        { }
        public Context(string configuration)
            : base(configuration, new JsonSerializer())
        { }
        public Context(string configuration, JsonSerializerSettings jsonSettings)
        : base(configuration, new JsonSerializer(jsonSettings))
        { }
    }
}
