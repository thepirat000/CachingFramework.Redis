namespace CachingFramework.Redis.Contracts.RedisObjects
{
    public class TryGetValueResult<TK, TV>
    {
        public bool Found { get; set; }
        public TK Key { get; set; }
        public TV Value { get; set; }
    }
}
