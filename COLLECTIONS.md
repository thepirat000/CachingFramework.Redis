.NET adapted Redis collections
=====
The following are the .NET objects provided to access Redis objects:

| Object type | CacheContext method | Description | Common interface |
| ------------ | ---------------- | -------------- | ------------------- |
| [List](#redis-lists--) | ```GetRedisList()``` | Doubly-linked list of objects | ```IList<T>``` |
| [Set](#redis-sets--) | ```GetRedisSet()``` | Set of unique objects | ```ICollection<T>``` |
| [Hash](#redis-hashes--) | ```GetRedisDictionary()``` | Dictionary of values | ```IDictionary<TK, TV>``` |
| [Sorted Set](#redis-sorted-sets--) | ```GetRedisSortedSet()``` | Set of unique objects sorted by score | ```ICollection<T>``` |
| [Bitmap](#redis-bitmaps--) | ```GetRedisBitmap()``` | Binary value / Bit fields | ```ICollection<byte>``` |
| [Lex. Sorted Set](#redis-lexicographical-sorted-set) | ```GetRedisLexicographicSet()``` | Set of strings lexicographically sorted | ```ICollection<string>``` |
| [String](#redis-string) | ```GetRedisString()``` | Binary-safe string | ```IEnumerable<byte>``` |

For example, to create/get a Redis Sorted Set of type `User`, you should do:
```c#
var context = new Context();
var sortedSet = context.Collections.GetRedisSortedSet<User>("key");
```

All the collections exposes the properties `TimeToLive` and `Expiration` (TimeSpan and DateTime) to get/set the expiration of the entire collection:
```c#
sortedSet.TimeToLive = TimeSpan.FromMinutes(60);
```

--------------
# Redis Lists &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; ![Image of hash](http://i.imgur.com/rN5QqoS.png)


To obtain a new (or existing) Redis List that implements `IList`, use the ```GetRedisList()``` method:

```c#
IRedisList<User> list = context.Collections.GetRedisList<User>("user:list");
```

To push elements to the list use `PushFirst` / `PushLast` methods:
```c#
list.PushFirst(new User() { Id = 0 });
```

To pop elements from the list use `PopFirst` / `PopLast` methods:
```c#
var user = list.PopFirst();
```

You can also add elements to the list by using `Add` / `AddRange` / `Insert` methods:
```c#
list.Add(new User() { Id = 1 });
list.AddRange(new [] { new User() { Id = 2 }, new User() { Id = 3 } });
list.Insert(2, new User() { Id = 4 });
```

To get a range of elements from the list, use the `GetRange` method.
It accepts a start and stop zero-based indexes that can also be negative numbers indicating offsets starting at the end of the list. -1 is the last element of the list, -2 the penultimate, and so on.
For example, this will return all the elements except the first and the last element:
```c#
IEnumerable<User> range = users.GetRange(1, -2);
```

## IRedisList mapping to Redis List

Mapping between `IRedisList` methods/properties to the Redis commands used:

|IRedisList interface|Redis command|Time complexity|
|------|------|-------|
|`Add(T item)`|[RPUSH](http://redis.io/commands/rpush)|O(1)|
|`AddRange(IEnumerable<T> collection)`|[RPUSH](http://redis.io/commands/rpush)|O(M) : M the number of elements to add|
|`AddFirst(T item)`|[LPUSH](http://redis.io/commands/lpush)|O(1)|
|`AddLast(T item)`|[RPUSH](http://redis.io/commands/rpush)|O(1)|
|`Insert(long index, T item)`|[LSET](http://redis.io/commands/lset) + [LINSERT](http://redis.io/commands/linsert)|O(N)|
|`GetRange(long start, long stop)`|[LRANGE](http://redis.io/commands/lrange)|O(S+M) : S is dist. from HEAD/TAIL|
|`FirstOrDefault()`|[LINDEX](http://redis.io/commands/lindex)|O(1)|
|`LastOrDefault()`|[LINDEX](http://redis.io/commands/lindex)|O(1)|
|`PopFirst()`|[LPOP](http://redis.io/commands/lpop)|O(1)|
|`PopLast()`|[RPOP](http://redis.io/commands/rpop)|O(1)|
|`Remove(T item, long count)`|[LREM](http://redis.io/commands/lrem)|O(N)|
|`RemoveAt(long index)`|[LSET](http://redis.io/commands/lset) + [LREM](http://redis.io/commands/lrem)|O(N)|
|`Contains(T item)`|[LRANGE](http://redis.io/commands/lrange)|O(N)|
|`Clear()`|[DEL](http://redis.io/commands/del)|O(1)|
|`this[] get`|[LINDEX](http://redis.io/commands/lindex)|O(N)|
|`this[] set`|[LSET](http://redis.io/commands/lset)|O(N)|
|`IndexOf(T item)`|[LINDEX](http://redis.io/commands/lindex)|O(M) : M is the # of elements to traverse|
|`Trim(long start, long stop)`|[LTRIM](http://redis.io/commands/ltrim)|O(M) : M is the # of elements to remove|
|`Count`|[LLEN](http://redis.io/commands/llen)|O(1)|

--------------

# Redis Sets &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; ![Image of sets](http://i.imgur.com/HYHjpbX.png)

To obtain a new (or existing) Redis Set that implements `ICollection`, use the ```GetRedisSet()``` method:

```c#
IRedisSet<User> set = context.Collections.GetRedisSet<User>("user:set");
```

To insert elements to the set, use `Add` or `AddRange` methods:

```c#
set.Add(new User() { Id = 1 });
set.AddRange(new [] { new User() { Id = 2 }, new User() { Id = 3 } });
```

To check if an element exists, use the `Contains` method:
```c#
bool exists = set.Contains(user);
```

To get a random element use the `GetRandomMember`method:
```c#
User user = set.GetRandomMember();
```

To get and remove a random element use the `Pop`method:
```c#
User user = set.Pop();
```

## IRedisSet mapping to Redis Set

Mapping between `IRedisSet` methods/properties to the Redis commands used:

|IRedisSet interface|Redis command|Time complexity|
|------|------|-------|
|`Add(T item)`|[SADD](http://redis.io/commands/sadd)|O(1)|
|`AddRange(IEnumerable<T> collection)`|[SADD](http://redis.io/commands/sadd)|O(M) : M the number of elements to add|
|`GetRandomMember()`|[SRANDMEMBER](http://redis.io/commands/srandmember)|O(1)|
|`Pop()`|[SPOP](http://redis.io/commands/spop)|O(1)|
|`Remove(T item)`|[SREM](http://redis.io/commands/srem)|O(1)|
|`Contains(T item)`|[SISMEMBER](http://redis.io/commands/sismember)|O(1)|
|`Count`|[SCARD](http://redis.io/commands/scard)|O(1)|

--------------

# Redis Hashes &nbsp;&nbsp;&nbsp;&nbsp; ![Image of hash](http://i.imgur.com/5HeN9VX.png)

To obtain a new (or existing) Redis Hash that implements `IDictionary`, use the ```GetRedisDictionary()``` method:

```c#
IRedisDictionary<int, User> hash = context.Collections.GetRedisDictionary<int, User>("user:hash");
```

To add elements to the hash, use `Add` / `AddRange`  methods or the indexed property:

```c#
hash.Add(1, new User() { Id = 1 });
hash.AddRange(usersQuery.ToDictionary(k => k.Id));
hash[2] = new User() { Id = 1 };
```

To check if a hash element exists, use `ContainsKey` method:
```c#
bool exists = hash.ContainsKey(1);
```

To access a hash element by key, use the indexed property:
```c#
User u = hash[1];
```


## IRedisDictionary mapping to Redis Hash

Mapping between `IRedisDictionary` methods/properties to the Redis commands used:

|IRedisDictionary interface|Redis command|Time complexity|
|------|------|-------|
|`Add(TK key, TV value)`|[HSET](http://redis.io/commands/hset)|O(1)|
|`AddRange(IEnum<KVP<TK, TV>> items)`|[HMSET](http://redis.io/commands/hmset)|O(M) where M is the number of fields being added|
|`this[] get`|[HGET](http://redis.io/commands/hget)|O(1)|
|`this[] set`|[HSET](http://redis.io/commands/hget)|O(1)|
|`Remove(TK key)`|[HDEL](http://redis.io/commands/hdel)|O(1)|
|`Clear()`|[DEL](http://redis.io/commands/del)|O(1)|
|`Contains(KeyValuePair<TK, TV> item)`|[HEXISTS](http://redis.io/commands/hexists)|O(1)|
|`ContainsKey(TK key)`|[HEXISTS](http://redis.io/commands/hexists)|O(1)|
|`Count`|[HLEN](http://redis.io/commands/hlen)|O(1)|

--------------

# Redis Sorted Sets &nbsp;&nbsp; ![Image of sorted set](http://i.imgur.com/HOklZQg.png)

To obtain a new (or existing) Redis Sorted Set that implements `ICollection`, use the ```GetRedisSortedSet()``` method:

```c#
IRedisSortedSet<User> sortedSet = context.Collections.GetRedisSortedSet<User>("user:sset");
```

To add elements to the sorted set, use `Add` or `AddRange` methods providing the score of the items as a `double`:

```c#
sortedSet.Add(12.34, new User() { Id = 1 });
```

To increment/decrement a score for an item, use the `IncrementScore` method:
```c#
double incremented = sortedSet.IncrementScore(user, 10.00);
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

You can remove a range by rank or by score.
For example to remove the first two elements sorted by score:
```c#
sortedSet.RemoveRangeByRank(0, 1);
```

Remove the elements whose score is between 0 and 100:
```c#
sortedSet.RemoveRangeByScore(0.00, 100.00);
```


## IRedisSortedSet mapping to Redis Sorted Set

Mapping between `IRedisSortedSet` methods/properties to the Redis commands used:

|IRedisSortedSet interface|Redis command|Time complexity|
|------|------|-------|
|`Add(T item, double score)`|[ZADD](http://redis.io/commands/zadd)|O(log(N))
|`AddRange(IEnu<SortedMember<T>> items)`|[ZADD](http://redis.io/commands/zadd)|O(log(N))
|`GetRangeByScore(double min, double max, bool desc, long skip, long)`|[ZRANGEBYSCORE](http://redis.io/commands/zrangebyscore) / [ZREVRANGEBYSCORE](http://redis.io/commands/zrevrangebyscore)|O(log(N)+M) : M the number of elements being returned|
|`GetRangeByRank(long start, long stop, bool desc)`|[ZRANGE](http://redis.io/commands/zrange) / [ZREVRANGE](http://redis.io/commands/zrevrange)|O(log(N)+M)
|`RemoveRangeByScore(double min, double max)`|[ZREMRANGEBYSCORE](http://redis.io/commands/zremrangebyscore)|O(log(N)+M)
|`RemoveRangeByRank(long start, long stop)`|[ZREMRANGEBYRANK](http://redis.io/commands/zremrangebyrank)|O(log(N)+M)
|`CountByScore(double min, double max)`|[ZCOUNT](http://redis.io/commands/zcount)|O(log(N))
|`IncrementScore(T item, double value)`|[ZINCRBY](http://redis.io/commands/zincrby)|O(log(N))
|`RankOf(T item, bool desc)`|[ZRANK](http://redis.io/commands/zrank)|O(log(N))
|`ScoreOf(T item)`|[ZSCORE](http://redis.io/commands/zscore)|O(1)
|`Count`|[ZCARD](http://redis.io/commands/zcard)|O(1)|

--------------

# Redis Bitmaps &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; ![Image of bitmap](http://i.imgur.com/2NxSq56.png)

To obtain a new (or existing) Redis bitmap that implements `ICollection<byte>`, use the ```GetRedisBitmap()``` method:

```c#
IRedisBitmap bitmap = context.Collections.GetRedisBitmap("users:visit");
```

To get or set bits, use the `GetBit` or `SetBit` methods:

```c#
bitmap.SetBit(0, 1); // Set the first bit to 1

bool bit = bitmap.GetBit(8); // Get the 9th bit
```

To count bits within a range, use the `Count` method:
```c#
long count = bitmap.Count(0, 2); // Count the bits in 1 within the first three bytes
```

To get the position of the first bit within a range, use the `BitPosition` method:
```c#
bitmap.BitPosition(1, -1, -1); // Return the position of the first 1 in the last byte
```

When enumerating a bitmap, each *byte* returned will have a value of 0 or 1, and will correspond to a bit in the bitmap, starting from the most significant bit. For example:

```c#
var sb = new StringBuilder();
foreach (byte bit in bitmap)
{
    sb.Append(bit);
}
```

## Bitmap example: count unique users logged per day
Set up a bitmap where the key is a function of the day, and each user is identified by an offset value. 

When a user logs in, set the bit to 1 at the offset representing user id:
```c#
void OnLogin(int userId)
{
    var key = "visits:" + DateTime.Now.ToString("yyyy-MM-dd");
    var bitmap = _context.GetRedisBitmap(key);
    bitmap.SetBit(userId, 1);
}
```

Get a count of the unique visits for a given date:
```c#
long CountVisits(DateTime date)
{
    var key = "visits:" + date.ToString("yyyy-MM-dd");
    var bitmap = _context.GetRedisBitmap(key);
    return bitmap.Count();
}
```

Determine if a user has logged in a given date:
```c#
bool HasVisited(int userId, DateTime date)
{
    var key = "visits:" + date.ToString("yyyy-MM-dd");
    var bitmap = _context.GetRedisBitmap(key);
    return bitmap.GetBit(userId) == 1;
}
```

## Bitfields

*: Bitfield operations are not yet available in a stable version of Redis. Download [unstable](https://github.com/antirez/redis/archive/unstable.tar.gz) if you want to test these commands.

[Bitfields](http://www.antirez.com/news/103) are arbitrary sized integers at arbitrary offsets stored on a Redis string.
This allows to handle groups of consecutive bits on a bitmap, instead of handling each bit separately.

There are three **commands** to handle bitfields:
- BitfieldGet(FieldType, Offset)
- BitfieldSet(FieldType, Offset, Value)
- BitfieldIncrementBy(FieldType, Offset, Value)

The **FieldType** indicates how many bits the integer will take and if it will be interpereted as a signed or unsigned integer. 
For example: 
 - u2 means a 2-bit unsigned integer [0 .. 3]
 - i9 means a 9-bit signed integer [-256 .. 255]
 - u16 means a 16-bit unsigned integer [0 .. 65535]

You also need to specify an **Offset** from which the bitmap will be read/written.
This offset can be specified in two ways:
- As a number of **bits** from the beginning,
- As a number of **fields** from the beginning, in order to say: "handle the bitmap as an array of counters of the specified size, and set the N-th counter".

Some examples:

Set the value 255 to an unsigned 8-bit integer stored at offset 8 (from the 9th bit in the bitmap):
```c#
bitmap.BitfieldSet(BitfieldType.u8, 8, 0xFF);
```
Set the value 255 to an unsigned 8-bit integer stored at position #1 (from 9th bit in the bitmap):
```c#
bitmap.BitfieldSet(BitfieldType.u8, 1, 0xFF, offsetIsOrdinal: true);
```

Set the value -1 to a signed 4-bit integer stored at offset 2 (from 3rd bit in the bitmap):
```c#
bitmap.BitfieldSet(BitfieldType.i4, 2, -1);
```
            
Get the value of a signed 4-bit integer at offset 2 (from 3rd bit in the bitmap):
```c#
int value = bitmap.BitfieldGet<int>(BitfieldType.i4, 2);
```

Get the value of a signed 4-bit integer at position #2 (9th bit in the bitmap):
```c#
int value = bitmap.BitfieldGet<int>(BitfieldType.i4, 2, offsetIsOrdinal:true);   
```


## IRedisBitmap mapping to Redis bitmap

Mapping between `IRedisBitmap` methods/properties to the Redis commands used:

|IRedisBitmap interface|Redis command|Time complexity|
|------|------|-------|
|`Add(byte value)`|[APPEND](http://redis.io/commands/append)|O(1)|
|`SetBit(long offset, byte bit)`|[SETBIT](http://redis.io/commands/setbit)|O(1)|
|`GetBit(long offset)`|[GETBIT](http://redis.io/commands/getbit)|O(1)|
|`BitPosition(byte bit, long start, long stop)`|[BITPOS](http://redis.io/commands/bitpos)|O(N)|
|`Contains(byte bit, long start, long stop)`|[BITPOS](http://redis.io/commands/bitpos)+[STRLEN](http://redis.io/commands/strlen)|O(N)|
|`Count()`|[BITCOUNT](http://redis.io/commands/bitcount)|O(N)|
|`BitFieldGet(FieldType type, long offset, bool offsetIsOrdinal)` *|BITFIELD|O(1)|
|`BitFieldSet(FieldType type, long offset, T value, bool offsetIsOrdinal)` *|BITFIELD|O(1)|
|`BitFieldIncrementBy(FieldType type, long offset, T increment)` *|BITFIELD|O(1)|



--------------

# Redis lexicographical Sorted Set

To obtain a new (or existing) Redis lexicographical sorted set that implements `ICollection<string>`, use the ```GetRedisLexicographicSet()``` method:

```c#
IRedisLexicographicSet lex = context.Collections.GetRedisLexicographicSet("autocomplete");
```

To add elements to the lex sorted set, use `Add` / `AddRange` methods:

```c#
lex.Add("zero");
lex.AddRange(new [] { "one", "two", "three" });
```

To get a suggestion list from a partial match, like an autocomplete suggestions:
```c#
IEnumerable<string> suggestions = lex.AutoComplete("t", 10);
```
Will return at most 10 elements as an `IEnumerable<string>` alphabetically sorted with the strings that starts with 't'.

To match any glob-style pattern, use the `Match` method:
```c#
IEnumerable<string> matches = lex.Match("*o");
```
Will return the strings that ends with 'o'.

## IRedisLexicographicSet mapping to Redis Sorted Set

Mapping between `IRedisLexicographicSet` methods/properties to the Redis commands used:

|IRedisLexicographicSet interface|Redis command|Time complexity|
|------|------|-------|
|`Add(string item)`|[ZADD](http://redis.io/commands/zadd)|O(log(N))|
|`AddRange(IEnu<string> items)`|[ZADD](http://redis.io/commands/zadd)|O(log(N))|
|`AutoComplete(string partial, long take)`|[ZRANGEBYLEX](http://redis.io/commands/zrangebylex)|O(log(N)+M) : M number of elements being returned|
|`Match(pattern)`|[ZSCAN](http://redis.io/commands/zscan)|O(1)|
|`Remove(string item)`|[ZREM](http://redis.io/commands/zrem)|O(log(N))|
|`Contains(string item)`|[ZRANGEBYLEX](http://redis.io/commands/zrangebylex)|O(log(N))|
|`Count`|[ZCARD](http://redis.io/commands/zcard)|O(1)|

--------------

# Redis String

To obtain a new (or existing) Redis String that implements `IEnumerable<byte>`, use the ```GetRedisString()``` method:

```c#
IRedisString cstr = context.Collections.GetRedisString("key");
```

To append to the string use the `Append` method:

```c#
cstr.Append("Hello world!");
```

To write starting at a specific position use the `SetRange` method:
```c#
cstr.SetRange(6, "WORLD");
```

To read from the string, use the `GetRange` method or the indexed property:
```c#
string s = cstr.GetRange(0, -1);   // will return the entire string.
```

```c#
string s = cstr[6, 8];   // will return the string "WOR".
```

### Redis string as a number

Overloads of the `Set` and `GetSet` methods are provided for setting the string value as an integer or a floating point number. The value can be changed in one operation with the `IncrementBy` / `IncrementByFloat` methods. Use the `AsInteger` and `AsFloat` to get the long/double value represented by the string.

For example to maintain a real-time counter of the users online.

When a user connects, increment the redis string value:

```c#
void OnConnect()
{
    var rs = context.Collections.GetRedisString("online:count");
    rs.IncrementBy(1);
}
```

Upon disconnection decrement:
```c#
void OnDisconnect()
{
    var rs = context.Collections.GetRedisString("online:count");
    rs.IncrementBy(-1);
}
```

To get the online counter use the `AsInteger` method:
```c#
long GetOnlineCount()
{
    var rs = context.Collections.GetRedisString("online:count");
    return rs.AsInteger();
}
```

Use the `GetSet` method to atomically overwrite the value and return the old value.

This can be used, for example, to atomically reset a counter:
```c#
long ResetCount()
{
    var rs = context.Collections.GetRedisString("online:count");
    return rs.GetSet(0);  // return the last value before reset
}
```

There is no need to initialize the string value to "0" when using increment functions, since a zero-value is assumed when the key does not exists. 

## IRedisString mapping to Redis String

Mapping between `IRedisString` methods/properties to the Redis commands used:

|IRedisString interface|Redis command|Time complexity|
|------|------|-------|
|`Append(string value)`|[APPEND](http://redis.io/commands/append)|O(1)|
|`Set(string value)`|[SET](http://redis.io/commands/set)|O(1)|
|`GetSet(string value)`|[GETSET](http://redis.io/commands/getset)|O(1)|
|`SetRange(long offset, string value)`|[SETRANGE](http://redis.io/commands/setrange)|O(1)|
|`GetRange(long start, long stop)`|[GETRANGE](http://redis.io/commands/getrange)|O(M) : M is the length of the returned string |
|`Length`|[STRLEN](http://redis.io/commands/strlen)|O(1)|
|`IncrementBy(long increment)`|[INCRBY](http://redis.io/commands/incrby)|O(1)|
|`IncrementByFloat(double increment)`|[INCRBYFLOAT](http://redis.io/commands/incrbyfloat)|O(1)|
