.NET adapted Redis collections
=====
The following are the four .NET generic collections provided to handle Redis collections:

| Redis object | Common interface | Interface name | CacheContext method |
| ------------ | ---------------- | -------------- | ------------------- |
| List | ```IList``` | ```ICachedList``` | ```GetCachedList()``` |
| Hash | ```IDictionary``` | ```ICachedDictionary``` | ```GetCachedDictionary()``` |
| Set | ```ISet``` | ```ICachedSet``` | ```GetCachedSet()``` |
| Sorted Set | ```ICollection``` | ```ICachedSortedSet``` | ```GetCachedSortedSet()``` |

For example, to create/get a Redis Sorted Set of type `User`, you should do:
```c#
var context = new CacheContext();
ICachedSortedSet<User> sortedSet = context.GetCachedSortedSet<User>("some:key");
```

# Redis Lists

To obtain a new (or existing) Redis List implementing a .NET `IList`, use the ```GetCachedList()``` method of the ```CacheContext``` class:

```c#
ICachedList<User> list = context.GetCachedList<User>("user:list");
```

To add elements to the list, use `Add` / `AddRange` / `Insert` / `AddFirst` or `AddLast` methods:

```c#
list.Add(new User() { Id = 1 });
list.AddRange(new [] { new User() { Id = 2 }, new User() { Id = 3 } });
list.AddFirst(new User() { Id = 0 });
```

To get a range of elements from the list, use the `GetRange` method.
It accepts a start and stop zero-based indexes that can also be negative numbers indicating offsets starting at the end of the list. -1 is the last element of the list, -2 the penultimate, and so on.
For example, this will return all the elements except the first and the last element:
```c#
IEnumerable<User> range = users.GetRange(1, -2);
```

## ICachedList mapping to Redis List

Mapping between `ICachedList` methods/properties to the Redis commands used:

|ICachedList interface|Redis command|Time complexity|
|------|------|-------|
|`AddRange(IEnumerable<T> collection)`|[RPUSH](http://redis.io/commands/rpush)|O(M) : M the number of elements to add|
|`Add(T item)`|[RPUSH](http://redis.io/commands/rpush)|O(1)|
|`Contains(T item)`|[LRANGE](http://redis.io/commands/lrange)|O(N)|
|`GetRange(long start, long stop)`|[LRANGE](http://redis.io/commands/lrange)|O(S+M) : S is dist. from HEAD/TAIL|
|`Insert(long index, T item)`|[LINDEX](http://redis.io/commands/lindex) + [LINSERT](http://redis.io/commands/linsert)|O(N)|
|`RemoveAt(long index)`|[LINDEX](http://redis.io/commands/lindex) + [LREM](http://redis.io/commands/lrem)|O(N)|
|`this[] get`|[LINDEX](http://redis.io/commands/lindex)|O(N)|
|`this[] set`|[LSET](http://redis.io/commands/lset)|O(N)|
|`IndexOf(T item)`|[LINDEX](http://redis.io/commands/lindex)|O(M) : M is the # of elements to traverse|
|`Remove(T item)`|[LREM](http://redis.io/commands/lrem)|O(N)|
|`Clear()`|[DEL](http://redis.io/commands/del)|O(1)|
|`AddFirst(T item)`|[LPUSH](http://redis.io/commands/lpush)|O(1)|
|`AddLast(T item)`|[RPUSH](http://redis.io/commands/rpush)|O(1)|
|`RemoveFirst()`|[LPOP](http://redis.io/commands/lpop)|O(1)|
|`RemoveLast()`|[RPOP](http://redis.io/commands/rpop)|O(1)|
|`Count`|[LLEN](http://redis.io/commands/llen)|O(1)|
|`First`|[LINDEX](http://redis.io/commands/lindex)|O(1)|
|`Last`|[LINDEX](http://redis.io/commands/lindex)|O(1)|

# Redis Sets

To obtain a new (or existing) Redis Set implementing a .NET `ISet`, use the ```GetCachedSet()``` method of the ```CacheContext``` class:

```c#
ICachedSet<User> set = context.GetCachedSet<User>("user:set");
```

To insert elements to the set, use `Add` or `AddRange` methods:

```c#
set.Add(new User() { Id = 1 });
set.AddRange(new [] { new User() { Id = 2 }, new User() { Id = 3 } });
```

To check if an element exists, use the `Contains` methos:
```c#
bool exists = set.Contains(user);
```

## ICachedSet mapping to Redis Set

Mapping between `ICachedSet` methods/properties to the Redis commands used:

|ICachedSet interface|Redis command|Time complexity|
|------|------|-------|
|`AddRange(IEnumerable<T> collection)`|[SADD](http://redis.io/commands/sadd)|O(M) : M the number of elements to add|
|`RemoveWhere(Predicate<T> match)`|[SMEMBERS](http://redis.io/commands/smembers) + [SREM](http://redis.io/commands/srem)|O(N)|
|`Add(T item)`|[SADD](http://redis.io/commands/sadd)|O(1)|
|`Contains(T item)`|[SISMEMBER](http://redis.io/commands/sismember)|O(1)|
|`Remove(T item)`|[SREM](http://redis.io/commands/srem)|O(1)|
|`Count`|[SCARD](http://redis.io/commands/scard)|O(1)|

# Redis Hashes

To obtain a new (or existing) Redis Hash implementing a .NET `IDictionary`, use the ```GetCachedDictionary()``` method of the ```CacheContext``` class:

```c#
ICachedDictionary<int, User> hash = context.GetCachedDictionary<int, User>("user:hash");
```

To add elements to the list, use `Add` or `AddRange` methods:

```c#
hash.Add(1, new User() { Id = 1 });
hash.AddRange(usersQuery.ToDictionary(k => k.Id));
```

To check if a hash element exists, use `ContainsKey` method:
```c#
bool exists = hash.ContainsKey(1);
```

## ICachedDictionary mapping to Redis Hash

Mapping between `ICachedDictionary` methods/properties to the Redis commands used:

|ICachedDictionary interface|Redis command|Time complexity|
|------|------|-------|
|`AddRange(IEnum<KVP<TK, TV>> items)`|[HMSET](http://redis.io/commands/hmset)|O(M) where M is the number of fields being added|
|`Add(TK key, TV value)`|[HSET](http://redis.io/commands/hset)|O(1)|
|`ContainsKey(TK key)`|[HEXISTS](http://redis.io/commands/hexists)|O(1)|
|`Remove(TK key)`|[HDEL](http://redis.io/commands/hdel)|O(1)|
|`this[] get`|[HGET](http://redis.io/commands/hget)|O(1)|
|`this[] set`|[HSET](http://redis.io/commands/hget)|O(1)|
|`Contains(KeyValuePair<TK, TV> item)`|[HEXISTS](http://redis.io/commands/hexists)|O(1)|
|`Count`|[HLEN](http://redis.io/commands/hlen)|O(1)|
|`Clear()`|[DEL](http://redis.io/commands/del)|O(1)|

# Redis Sorted Sets

To obtain a new (or existing) Redis Sorted Set implementing a .NET `ICollection`, use the ```GetCachedSortedSet()``` method of the ```CacheContext``` class:

```c#
ICachedSortedSet<User> sortedSet = context.GetCachedSortedSet<User>("user:sset");
```

To add elements to the sorted set, use `Add` or `AddRange` methods prividing the score of the items as a `double`:

```c#
sortedSet.Add(12.34, new User() { Id = 1 });
```

To get a range of elements by rank or by score, use the `GetRangeByScore` and `GetRangeByRank` methods.
For example to get all the elements with the exception of the top and the bottom ranked values:
```c#
var byRank = sortedSet.GetRangeByRank(1, -2);
```

For example to get elements with score less than or equal to 100:
```c#
var byScore = sortedSet.GetRangeByScore(double.NegativeInfinity, 100.00);
```

## ICachedSortedSet mapping to Redis Sorted Set

Mapping between `ICachedSortedSet` methods/properties to the Redis commands used:

|ICachedSortedSet interface|Redis command|Time complexity|
|------|------|-------|
|`Add(T item, double score)`|[ZADD](http://redis.io/commands/zadd)|O(log(N))
|`AddRange(IEnu<SortedMember<T>> items)`|[ZADD](http://redis.io/commands/zadd)|O(log(N))
|`CountByScore(double min, double max)`|[ZCOUNT](http://redis.io/commands/zcount)|O(log(N))
|`GetRangeByScore(double min, double max, bool desc, long skip, long)`|[ZRANGEBYSCORE](http://redis.io/commands/zrangebyscore) / [ZREVRANGEBYSCORE](http://redis.io/commands/zrevrangebyscore)|O(log(N)+M) : M the number of elements being returned|
|`GetRangeByRank(long start, long stop, bool desc)`|[ZRANGE](http://redis.io/commands/zrange) / [ZREVRANGE](http://redis.io/commands/zrevrange)|O(log(N)+M)
|`RemoveRangeByScore(double min, double max)`|[ZREMRANGEBYSCORE](http://redis.io/commands/zremrangebyscore)|O(log(N)+M)
|`RemoveRangeByRank(long start, long stop)`|[ZREMRANGEBYRANK](http://redis.io/commands/zremrangebyrank)|O(log(N)+M)
|`IncrementScore(T item, double value)`|[ZINCRBY](http://redis.io/commands/zincrby)|O(log(N))
|`RankOf(T item, bool desc)`|[ZRANK](http://redis.io/commands/zrank)|O(log(N))
|`ScoreOf(T item)`|[ZSCORE](http://redis.io/commands/zscore)|O(1)
|`Count`|[ZCARD](http://redis.io/commands/zcard)|O(1)|

