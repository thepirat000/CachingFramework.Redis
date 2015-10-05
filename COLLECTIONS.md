.NET adapted Redis collections
=====

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

## Redis List

To obtain a new (or existing) Redis List implementing a .NET IList, use the ```GetCachedList()``` method of the ```CacheContext``` class:

```c#
ICachedList<User> userList = context.GetCachedList<User>("user:list");
```

To add elements to the list, use `Add` / `AddRange` / `Insert` / `AddFirst` or `AddLast` methods:

```c#
userList.Add(new User() { Id = 1 });
userList.AddRange(new [] { new User() { Id = 2 }, new User() { Id = 3 } });
userList.AddFirst(new User() { Id = 0 });
```

To get a range of elements from the list, use the `GetRange` method.
It accepts a start and stop zero-based indexes that can also be negative numbers indicating offsets starting at the end of the list. -1 is the last element of the list, -2 the penultimate, and so on.
For example, this will return all the elements except the first and the last element:
```c#
IEnumerable<User> range = users.GetRange(1, -2);
```

## ICachedList mapping to Redis

|ICachedList interface|Redis command|Time complexity|
|------|------|-------|
|AddRange(IEnumerable<T> collection)|[RPUSH](http://redis.io/commands/rpush)|O(M) : M the number of elements to add|
|Add(T item)|[RPUSH](http://redis.io/commands/rpush)|O(1)|
|Contains(T item)|[LRANGE](http://redis.io/commands/lrange)|O(N)|
|GetRange(long start, long stop)|[LRANGE](http://redis.io/commands/lrange)|O(S+M) : S is dist. from HEAD/TAIL|
|Insert(long index, T item)|[LINDEX](http://redis.io/commands/lindex) + [LINSERT](http://redis.io/commands/linsert)|O(N)|
|RemoveAt(long index)|[LINDEX](http://redis.io/commands/lindex) + [LREM](http://redis.io/commands/lrem)|O(N)|
|this[] get|[LINDEX](http://redis.io/commands/lindex)|O(N)|
|this[] set|[LSET](http://redis.io/commands/lset)|O(N)|
|IndexOf(T item)|[LINDEX](http://redis.io/commands/lindex)|O(M) : M is the # of elements to traverse|
|Remove(T item)|[LREM](http://redis.io/commands/lrem)|O(N)|
|Clear()|[DEL](http://redis.io/commands/del)|O(1)|
|AddFirst(T item)|[LPUSH](http://redis.io/commands/lpush)|O(1)|
|AddLast(T item)|[RPUSH](http://redis.io/commands/rpush)|O(1)|
|RemoveFirst()|[LPOP](http://redis.io/commands/lpop)|O(1)|
|RemoveLast()|[RPOP](http://redis.io/commands/rpop)|O(1)|
|Count|[LLEN](http://redis.io/commands/llen)|O(1)|
|First|[LINDEX](http://redis.io/commands/lindex)|O(1)|
|Last|[LINDEX](http://redis.io/commands/lindex)|O(1)|

## Redis Set

To obtain a new (or existing) Redis Set implementing a .NET ISet, use the ```GetCachedSet()``` method of the ```CacheContext``` class:

```c#
ICachedSet<User> userSet = context.GetCachedSet<User>("user:set");
```

To insert elements to the set, use `Add` or `AddRange` methods:

```c#
userSet.Add(new User() { Id = 1 });
userSet.AddRange(new [] { new User() { Id = 2 }, new User() { Id = 3 } });
```

To check if an element exists:
```c#
bool contains = userSet.Contains(user);
```

## ICachedSet mapping to Redis

|ICachedSet interface|Redis command|Time complexity|
|------|------|-------|
|AddRange(IEnumerable<T> collection)|[SADD](http://redis.io/commands/sadd)|O(M) : M the number of elements to add|
|RemoveWhere(Predicate<T> match)|[SMEMBERS](http://redis.io/commands/smembers) + [SREM](http://redis.io/commands/srem)|O(N)|
|Add(T item)|[SADD](http://redis.io/commands/sadd)|O(1)|
|Contains(T item)|[SISMEMBER](http://redis.io/commands/sismember)|O(1)|
|Remove(T item)|[SREM](http://redis.io/commands/srem)|O(1)|
|Count|[SCARD](http://redis.io/commands/scard)|O(1)|
