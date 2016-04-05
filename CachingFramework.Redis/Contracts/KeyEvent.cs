using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CachingFramework.Redis.Contracts
{
    public enum KeyEvent
    {
        [Text("del")]
        Delete = 0,
        [Text("rename_from")]
        RenameFrom,
        [Text("rename_to")]
        RenameTo,
        [Text("expire")]
        ExpirationSet,
        [Text("expired")]
        Expire,
        [Text("sortstore")]
        SortStore,
        [Text("set")]
        Set,
        [Text("setrange")]
        RangeSet,
        [Text("incrby")]
        Increment,
        [Text("incrbyfloat")]
        IncrementByFloat,
        [Text("append")]
        Append,
        [Text("lpush")]
        PushLeft,
        [Text("lpop")]
        PopLeft,
        [Text("rpush")]
        PushRight,
        [Text("rpop")]
        PopRight,
        [Text("linsert")]
        ListInsert,
        [Text("lset")]
        ListSet,
        [Text("lrem")]
        ListRemove,
        [Text("ltrim")]
        ListTrim,
        [Text("hset")]
        HashSet,
        [Text("hincrby")]
        HashIncrement,
        [Text("hincrbyfloat")]
        HashIncrementByFloat,
        [Text("hdel")]
        HashDelete,
        [Text("sadd")]
        SetAdd,
        [Text("srem")]
        SetRemove,
        [Text("spop")]
        SetPop,
        [Text("sinterstore")]
        SetIntersectStore,
        [Text("sunionstore")]
        SetUnionStore,
        [Text("sdiffstore")]
        SetDiffStore,
        [Text("zincr")]
        SortedIncrementScore,
        [Text("zadd")]
        SortedAddScore,
        [Text("zrem")]
        SortedRemoveScore,
        [Text("zrembyscore")]
        SortedRemoveByScore,
        [Text("zrembyrank")]
        SortedRemoveByRank,
        [Text("zinterstore")]
        SortedIntersectStore,
        [Text("zunionstore")]
        SortedUnionStore,
        [Text("evicted")]
        Evicted
    }
}
