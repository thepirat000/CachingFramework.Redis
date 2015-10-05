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

To obtain a new (or existing) Redis list implementing a .NET IList, use the ```GetCachedList()``` method of the ```CacheContext``` class:

```c#
ICachedList<User> users = context.GetCachedList<User>("user:list");
```

You can call `Add` / `AddRange` / `Insert` / `AddFirst` / `AddLast` methods to add elements to the list:

```c#
users.Add(new User() { Id = 1 });
users.AddRange(new [] { new User() { Id = 2 }, new User() { Id = 3 } });
users.AddFirst(new User() { Id = 0 });
```

You can get a range of elements from the list by calling `GetRange` method.
It accepts a start and stop zero-based indexes that can also be negative numbers indicating offsets starting at the end of the list. -1 is the last element of the list, -2 the penultimate, and so on.
For example, this will return all the elements except the first and the last element:
```c#
IEnumerable<User> range = users.GetRange(1, -2);
```

## RedisList map

|ICachedList|Redis command|Time complexity|
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

```c#
```
