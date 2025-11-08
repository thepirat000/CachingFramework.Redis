# Summary

|||
|:---|:---|
| Generated on: | 11/08/2025 - 03:38:30 |
| Coverage date: | 11/08/2025 - 03:34:31 - 11/08/2025 - 03:38:04 |
| Parser: | MultiReport (5x Cobertura) |
| Assemblies: | 4 |
| Classes: | 37 |
| Files: | 37 |
| **Line coverage:** | 85.3% (1647 of 1930) |
| Covered lines: | 1647 |
| Uncovered lines: | 283 |
| Coverable lines: | 1930 |
| Total lines: | 6618 |
| **Branch coverage:** | 76.4% (520 of 680) |
| Covered branches: | 520 |
| Total branches: | 680 |
| **Method coverage:** | [Feature is only available for sponsors](https://reportgenerator.io/pro) |

# Risk Hotspots

| **Assembly** | **Class** | **Method** | **Crap Score** | **Cyclomatic complexity** |
|:---|:---|:---|---:|---:|
| CachingFramework.Redis | CachingFramework.Redis.Providers.RedisCacheProvider | IsHashFieldInTagAsync() | 72 | 8 || CachingFramework.Redis | CachingFramework.Redis.Providers.RedisCacheProvider | IsSetMemberInTagAsync() | 72 | 8 || CachingFramework.Redis | CachingFramework.Redis.Providers.RedisCacheProvider | IsStringKeyInTagAsync() | 72 | 8 || CachingFramework.Redis | CachingFramework.Redis.Providers.RedisCacheProvider | GetObjectsByTag() | 24 | 24 || CachingFramework.Redis | CachingFramework.Redis.Providers.RedisCacheProvider | GetTaggedItemsWithCleanup(...) | 20 | 16 || CachingFramework.Redis | CachingFramework.Redis.Providers.RedisCacheProvider | GetTaggedItemsWithCleanupAsync() | 20 | 16 |
# Coverage

| **Name** | **Covered** | **Uncovered** | **Coverable** | **Total** | **Line coverage** | **Covered** | **Total** | **Branch coverage** |
|:---|---:|---:|---:|---:|---:|---:|---:|---:|
| **CachingFramework.Redis** | **1621** | **267** | **1888** | **6402** | **85.8%** | **512** | **668** | **76.6%** |
| CachingFramework.Redis.Contracts.GeoCoordinate | 6 | 1 | 7 | 40 | 85.7% | 0 | 0 |  |
| CachingFramework.Redis.Contracts.GeoMember<T> | 12 | 0 | 12 | 57 | 100% | 0 | 0 |  |
| CachingFramework.Redis.Contracts.RedisObjects.TryGetValueResult<T1, T2> | 3 | 0 | 3 | 9 | 100% | 0 | 0 |  |
| CachingFramework.Redis.Contracts.SerializerBase | 2 | 0 | 2 | 29 | 100% | 0 | 0 |  |
| CachingFramework.Redis.Contracts.SortedMember<T> | 6 | 0 | 6 | 30 | 100% | 0 | 0 |  |
| CachingFramework.Redis.Contracts.TagMember | 8 | 1 | 9 | 41 | 88.8% | 2 | 4 | 50% |
| CachingFramework.Redis.Contracts.TextAttribute | 7 | 0 | 7 | 35 | 100% | 0 | 0 |  |
| CachingFramework.Redis.Contracts.TextAttributeCache<T> | 18 | 0 | 18 | 55 | 100% | 7 | 8 | 87.5% |
| CachingFramework.Redis.DatabaseOptions | 3 | 0 | 3 | 26 | 100% | 1 | 2 | 50% |
| CachingFramework.Redis.LuaScriptResource | 2 | 0 | 2 | 18 | 100% | 0 | 0 |  |
| CachingFramework.Redis.Providers.RedisCacheProvider | 789 | 120 | 909 | 2373 | 86.7% | 343 | 440 | 77.9% |
| CachingFramework.Redis.Providers.RedisCollectionProvider | 10 | 0 | 10 | 97 | 100% | 0 | 0 |  |
| CachingFramework.Redis.Providers.RedisGeoProvider | 45 | 2 | 47 | 219 | 95.7% | 26 | 28 | 92.8% |
| CachingFramework.Redis.Providers.RedisKeyEventsProvider | 32 | 2 | 34 | 94 | 94.1% | 14 | 18 | 77.7% |
| CachingFramework.Redis.Providers.RedisProviderBase | 7 | 0 | 7 | 52 | 100% | 0 | 0 |  |
| CachingFramework.Redis.Providers.RedisProviderContext | 15 | 5 | 20 | 83 | 75% | 4 | 6 | 66.6% |
| CachingFramework.Redis.Providers.RedisPubSubProvider | 35 | 14 | 49 | 146 | 71.4% | 0 | 0 |  |
| CachingFramework.Redis.RedisContext | 41 | 45 | 86 | 311 | 47.6% | 2 | 12 | 16.6% |
| CachingFramework.Redis.RedisObjects.RedisBaseObject | 16 | 3 | 19 | 109 | 84.2% | 5 | 8 | 62.5% |
| CachingFramework.Redis.RedisObjects.RedisBitmap | 64 | 6 | 70 | 272 | 91.4% | 15 | 16 | 93.7% |
| CachingFramework.Redis.RedisObjects.RedisDictionary<T1, T2> | 76 | 13 | 89 | 367 | 85.3% | 10 | 14 | 71.4% |
| CachingFramework.Redis.RedisObjects.RedisLexicographicSet | 18 | 4 | 22 | 132 | 81.8% | 2 | 2 | 100% |
| CachingFramework.Redis.RedisObjects.RedisList<T> | 135 | 9 | 144 | 496 | 93.7% | 35 | 38 | 92.1% |
| CachingFramework.Redis.RedisObjects.RedisSet<T> | 41 | 6 | 47 | 234 | 87.2% | 6 | 10 | 60% |
| CachingFramework.Redis.RedisObjects.RedisSortedSet<T> | 77 | 25 | 102 | 481 | 75.4% | 26 | 42 | 61.9% |
| CachingFramework.Redis.RedisObjects.RedisString | 57 | 3 | 60 | 204 | 95% | 6 | 10 | 60% |
| CachingFramework.Redis.Serializers.BinarySerializer | 23 | 0 | 23 | 110 | 100% | 0 | 0 |  |
| CachingFramework.Redis.Serializers.HandleSpecialDoublesAsStrings | 8 | 1 | 9 | 38 | 88.8% | 3 | 4 | 75% |
| CachingFramework.Redis.Serializers.JsonSerializer | 9 | 3 | 12 | 59 | 75% | 0 | 0 |  |
| CachingFramework.Redis.Serializers.RawSerializer | 54 | 3 | 57 | 146 | 94.7% | 5 | 6 | 83.3% |
| CachingFramework.Redis.TaskExtensions | 2 | 1 | 3 | 39 | 66.6% | 0 | 0 |  |
| **CachingFramework.Redis.MemoryPack** | **9** | **6** | **15** | **69** | **60%** | **4** | **6** | **66.6%** |
| CachingFramework.Redis.MemoryPack.Context | 0 | 4 | 4 | 12 | 0% | 0 | 0 |  |
| CachingFramework.Redis.MemoryPack.MemoryPackSerializer | 9 | 2 | 11 | 57 | 81.8% | 4 | 6 | 66.6% |
| **CachingFramework.Redis.MsgPack** | **10** | **6** | **16** | **74** | **62.5%** | **4** | **6** | **66.6%** |
| CachingFramework.Redis.MsgPack.Context | 0 | 4 | 4 | 18 | 0% | 0 | 0 |  |
| CachingFramework.Redis.MsgPack.MsgPackSerializer | 10 | 2 | 12 | 56 | 83.3% | 4 | 6 | 66.6% |
| **CachingFramework.Redis.NewtonsoftJson** | **7** | **4** | **11** | **73** | **63.6%** | **0** | **0** | **** |
| CachingFramework.Redis.NewtonsoftJson.Context | 0 | 4 | 4 | 18 | 0% | 0 | 0 |  |
| CachingFramework.Redis.NewtonsoftJson.NewtonsoftJsonSerializer | 7 | 0 | 7 | 55 | 100% | 0 | 0 |  |

