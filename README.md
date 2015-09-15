# CachingFramework.Redis
.NET Redis Distributed Cache library.

##Features
 * **Tagging mechanism**
 to store cache items with tags allowing to retrieve or invalidate items by tag.
 * **Fetching mechanism**
 as shortcut methods for atomic Add/Get operations.
 * **Items Expiration**
 * **Fully compatible with Redis Cluster**
 * **Configurable serialization**
 * **Handle Redis List, Sets and Hashes** from common interfaces IList<T>, ISet<T> and IDictionary<K, V>.
 
## Usage

### NuGet
```
PM> Install-Package CachingFramework.Redis
```

### Configuration
#### Default configuration
```c#
var cache = new CacheContext();
```

#### Custom configuration
```c#
var cache = new CacheContext("10.0.0.1:7000, 10.0.0.1:7001, connectRetry=10, syncTimeout=5000, abortConnect=false, allowAdmin=true");
```
See https://github.com/StackExchange/StackExchange.Redis/blob/master/Docs/Configuration.md for StackExchange.Redis configuration options.

### Adding objects

#### Add a single object to the cache
```c#
string redisKey = "user:1";
User value = new User() { Id = 1 };  // any serializable object 
cache.SetObject(redisKey, value);
```

#### Add a single object with tags
```c#
cache.SetObject(redisKey, value, new[] { "tag1", "tag2" });
```

#### Add a single object with tags and TTL
```c#
cache.SetObject(redisKey, value, TimeSpan.FromDays(1));
```

### Getting objects

#### Get a single object
```c#
User user = cache.GetObject<User>(redisKey);
```

### Removing objects

#### Remove a single object
```c#
cache.Remove(redisKey);
```

#### Invalidate an entire tag
```c#
cache.InvalidateKeysByTag("tag1");
```

#### Get objects by tag
```c#
IEnumerable<User> users = cache.GetObjectsByTag<User>("tag1");
```

### Fetching objects

#### Get an object, inserting it to the cache if it does not exists
```c#
var user = cache.FetchObject<User>(redisKey, () => GetUserFromDatabase(id));
```
Note the method *GetUserFromDatabase* will only be called if the value is not present on the cache, in such case it will add it to the cache.

















