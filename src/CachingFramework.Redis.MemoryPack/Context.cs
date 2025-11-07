namespace CachingFramework.Redis.MemoryPack
{
    public class Context : Redis.RedisContext
    {
        public Context()
            : base("localhost:6379", new MemoryPackSerializer())
        { }
        public Context(string configuration)
            : base(configuration, new MemoryPackSerializer())
        { }
    }
}
