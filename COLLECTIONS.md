.NET adapted Redis collections
=====

| Redis object | Common interface | Interface name | CacheContext method |
| ------------ | ---------------- | -------------- | ------------------- |
| List | ```IList``` | ```ICachedList``` | ```GetCachedList()``` |
| Hash | ```IDictionary``` | ```ICachedDictionary``` | ```GetCachedDictionary()``` |
| Set | ```ISet``` | ```ICachedSet``` | ```GetCachedSet()``` |
| Sorted Set | ```ICollection``` | ```ICachedSortedSet``` | ```GetCachedSortedSet()``` |

## Redis List

To obtain a new (or existing) Redis list implementing a .NET IList, use the ```GetCachedList()``` method of the ```CacheContext``` class.

```c#

```


```c#
```

```c#
```
