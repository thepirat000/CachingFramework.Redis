.NET adapted Redis collections
=====
The following are the .NET objects provided to handle Redis collections:

| Redis object | Common interface | Interface name | CacheContext method | Description |
| ------------ | ---------------- | -------------- | ------------------- | ----------- |
| List | ```IList``` | ```ICachedList<T>``` | ```GetCachedList()``` | Double-linked list of objects |
| Hash | ```IDictionary``` | ```ICachedDictionary<TK, TV>``` | ```GetCachedDictionary()``` | Dictionary of values |
| Set | ```ISet``` | ```ICachedSet<T>``` | ```GetCachedSet()``` | Set of unique objects |
| Sorted Set | ```ICollection<T>``` | ```ICachedSortedSet``` | ```GetCachedSortedSet()``` | Set of objects sorted by score |
| Bitmap | ```ICollection<bool>``` | ```ICachedBitmap``` | ```GetCachedBitmap()``` | Binary value |
| Sorted Set | ```ICollection<string>``` | ```ICachedLexicographicSet``` | ```GetCachedLexicographicSet()``` | Set of strings lexicographically sorted |
| String | ```IEnumerable<byte>``` | ```ICachedString``` | ```GetCachedString()``` | Binary-safe string |

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

# Redis Bitmaps

To obtain a new (or existing) Redis bitmap implementing a .NET `ICollection<bool>`, use the ```GetCachedBitmap()``` method of the ```CacheContext``` class:

```c#
ICachedBitmap bitmap = context.GetCachedBitmap("users:visit");
```

To get or set bits, use the `GetBit` or `SetBit` methods:

```c#
bitmap.SetBit(0, false); // Set the first bit to 0

bool bit = bitmap.GetBit(8); // Get the 9th bit
```

To count bits within a range, use the `Count` method:
```c#
long count = bitmap.Count(0, 1); // Count the bits in 1 within the first two bytes
```

To get the position of the first bit within a range, use the `BitPosition` method:
```c#
bitmap.BitPosition(true, -1, -1); // Return the position of the first 1 in the last byte
```

## Bitmap example: count unique users logged per day
Set up a bitmap where the key is a function of the day, and each user is identified by an offset value. 

When a user logs in, set the bit to 1 at the offset representing user id:
```c#
void Login(int userId)
{
    var key = "visits:" + DateTime.Now.ToString("yyyy-MM-dd");
    var bitmap = _context.GetCachedBitmap(key);
    bitmap.SetBit(userId, true);
}
```

Get a count of the unique visits for a given date:
```c#
long CountVisits(DateTime date)
{
    var key = "visits:" + date.ToString("yyyy-MM-dd");
    var bitmap = _context.GetCachedBitmap(key);
    return bitmap.Count();
}
```

Determine if a user has logged in a given date:
```c#
bool HasVisited(int userId, DateTime date)
{
    var key = "visits:" + date.ToString("yyyy-MM-dd");
    var bitmap = _context.GetCachedBitmap(key);
    return bitmap.GetBit(userId);
}
```

## ICachedBitmap mapping to Redis bitmap

Mapping between `ICachedBitmap` methods/properties to the Redis commands used:

|ICachedBitmap interface|Redis command|Time complexity|
|------|------|-------|
|`Add(bool value)`|[APPEND](http://redis.io/commands/append)|O(1)|
|`SetBit(long offset, bool bit)`|[SETBIT](http://redis.io/commands/setbit)|O(1)|
|`GetBit(long offset)`|[GETBIT](http://redis.io/commands/getbit)|O(1)|
|`BitPosition(bool bit, long start, long stop)`|[BITPOS](http://redis.io/commands/bitpos)|O(N)|
|`Contains(bool bit, long start, long stop)`|[BITPOS](http://redis.io/commands/bitpos)+[STRLEN](http://redis.io/commands/strlen)|O(N)|
|`Count`|[BITCOUNT](http://redis.io/commands/bitcount)|O(N)|

# Redis lexicographical Sorted Set

To obtain a new (or existing) Redis lexicographical sorted set implementing a .NET `ICollection<string>`, use the ```GetCachedLexicographicSet()``` method of the ```CacheContext``` class:

```c#
ICachedLexicographicSet lex = context.GetCachedLexicographicSet("autocomplete");
```

To add elements to the lex sorted set, use `Add` / `AddRange` methods:

```c#
lex.Add("zero");
lex.AddRange(new [] { "one", "two", "three" });
```

To get a suggestion list from a partial match, like an autocomplete suggestions:
```c#
IEnumerable<string> suggestions = lex.AutoComplete("t");
```
Will return an `IEnumerable<string>` alphabetically sorted with the matches (in this case "two" and "three").

## ICachedLexicographicSet mapping to Redis Sorted Set

Mapping between `ICachedLexicographicSet` methods/properties to the Redis commands used:

|ICachedLexicographicSet interface|Redis command|Time complexity|
|------|------|-------|
|`Add(string item)`|[ZADD](http://redis.io/commands/zadd)|O(log(N))|
|`AddRange(IEnu<string> items)`|[ZADD](http://redis.io/commands/zadd)|O(log(N))|
|`AutoComplete(string partial, long take)`|[ZRANGEBYLEX](http://redis.io/commands/zrangebylex)|O(log(N)+M) : M number of elements being returned|
|`Contains(string item)`|[ZRANGEBYLEX](http://redis.io/commands/zrangebylex)|O(log(N))|
|`Remove(string item)`|[ZREM](http://redis.io/commands/zrem)|O(log(N))|
|`Count`|[ZCARD](http://redis.io/commands/zcard)|O(1)|

# Redis String

To obtain a new (or existing) Redis String implementing a .NET `IEnumerable<byte>`, use the ```GetCachedString()``` method of the ```CacheContext``` class:

```c#
ICachedString cstr = context.GetCachedString("key");
```

To write to the string use the `SetRange` method:

```c#
cstr.SetRange(0, "Some text");
```

To write starting at a specific position:
```c#
cstr.SetRange(5, "T");
```

To read from the string, use the `GetRange` method or the indexed property:
```c#
string s = cstr.GetRange(0, -1);   // This will return the entire string: "Some Text"
```

```c#
string s = cstr[0, -1];   // This will return the entire string: "Some Text"
```

## ICachedString mapping to Redis String

Mapping between `ICachedString` methods/properties to the Redis commands used:

|ICachedString interface|Redis command|Time complexity|
|------|------|-------|
|`GetRange(long start, long stop)`|[GETRANGE](http://redis.io/commands/getrange)|O(M) : M is the length of the returned string |
|`SetRange(long offset, string item)`|[SETRANGE](http://redis.io/commands/setrange)|O(1)|
|`Length`|[STRLEN](http://redis.io/commands/strlen)|O(1)|



