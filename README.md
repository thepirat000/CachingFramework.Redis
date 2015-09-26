# CachingFramework.Redis
.NET Redis Distributed Cache library.

##Features
 * **Tagging mechanism**
 to store cache items with tags allowing to retrieve or invalidate items by tag.
 * **Fetching mechanism** as shortcut methods for atomic Add/Get operations.
 * **Time-To-Live mechanism**.
 * **Fully compatible with Redis Cluster**.
 * **Compressed binary serialization** to minimize network and memory load.
 * **Handle Redis List, Sets and Hashes** from common interfaces IList<T>, ISet<T> and IDictionary<K, V>.
 * **Pub/Sub with typed messages**.
 
## Usage

### NuGet
```
PM> Install-Package CachingFramework.Redis
```

### Configuration
#### Default configuration
Connect to Redis on localhost port 6379:
```c#
var cache = new CacheContext();
```

#### Custom configuration
```c#
var cache = new CacheContext("10.0.0.1:7000, 10.0.0.2:7000, 10.0.0.3:7000, connectRetry=10, syncTimeout=5000, abortConnect=false, allowAdmin=true");
```
See https://github.com/StackExchange/StackExchange.Redis/blob/master/Docs/Configuration.md for StackExchange.Redis configuration options.

### Adding objects

#### Add a single object 
Add a single object to the cache:
```c#
string redisKey = "user:1";
User value = new User() { Id = 1 };  // any serializable object 
cache.SetObject(redisKey, value);
```

#### Add a single object with tags
Add a single object to the cache and associate it with tags *tag1* and *tag2*:
```c#
cache.SetObject(redisKey, value, new[] { "tag1", "tag2" });
```

#### Add a single object with TTL
Add a single object to the cache with a Time-To-Live of 1 day:
```c#
cache.SetObject(redisKey, value, TimeSpan.FromDays(1));
```

### Getting objects

#### Get a single object
```c#
User user = cache.GetObject<User>(redisKey);
```

### Removing objects

#### Remove a key
```c#
cache.Remove(redisKey);
```

#### Invalidate by tag
Remove all the keys related to *tag1*:
```c#
cache.InvalidateKeysByTag("tag1");
```

#### Get objects by tag
Get all the objects related to *tag1*. Assuming all the keys related to the tag are of type `User`:
```c#
IEnumerable<User> users = cache.GetObjectsByTag<User>("tag1");
```

### Fetching objects

#### Fetch an object
Try to get an object from the cache, inserting it to the cache if it does not exists:
```c#
var user = cache.FetchObject<User>(redisKey, () => GetUserFromDatabase(id));
```
The method `GetUserFromDatabase` will only be called when the value is not present on the cache, in which case will be added to the cache before returning it.

### Hashes
Hashes are maps composed of fields associated with values, like .NET dictionaries.

#### Set hashed objects
Set an object on a redis key indexed by a field key (sub-key):
```c#
void InsertUser(User user)
{
    var redisKey = "users:hash";
    var fieldKey = "user:id:" + user.Id;
    cache.SetHashed(redisKey, fieldKey, user);
}
```
#### Get hashed object
Get an object by the redis key and a field key:
```c#
User u = cache.GetHashed<User>(redisKey, "user:id:1");
```
#### Get all the objects in a hash 
```c#
IDictionary<string, User> users = cache.GetHashedAll<User>(redisKey);
```
Objects within a hash can be of different types. 

#### Remove object from hash
```c#
cache.RemoveHashed(redisKey, "user:id:1");
```

### .NET Collections
Implementations of .NET IList, ISet and IDictionary that internally uses Redis as storage are provided.

#### Get a .NET IList stored as a Redis List
```c#
IList<User> users = cache.GetCachedList<User>(redisKey);
```

#### Get a .NET ISet stored as a Redis Set
```c#
ISet<User> users = cache.GetCachedSet<User>(redisKey);
```

#### Get a .NET IDictionary stored as a Redis Hash
```c#
IDictionary<string, User> users = cache.GetCachedDictionary<string, User>(redisKey);
```

Pub/Sub
=====

A typed Publish/Subscribe mechanism is provided.

#### Subscribe to a channel
Listen for messages of type `User` on the channel *users*:
```c#
cache.Subscribe<User>("users", user => Console.WriteLine(user.Id));
```

#### Publish to a channel
Publishes a messages of type `User` to the channel *users*:
```c#
cache.Publish<User>("users", new User() { Id = 1 });
```

#### Unsubscribe from a channel
```c#
cache.Unsubscribe("users");
```

#### Messages types
Each subscription listen to messages of the specified type (or inherited from it):
```c#
cache.Subscribe<User>("entities", user => Console.WriteLine(user.Id));
cache.Subscribe<Manager>("entities", mgr => Console.WriteLine(mgr.Id));
```
Subscription of type object will listen to all types:
```c#
cache.Subscribe<object>("entities", obj => Console.WriteLine(obj));
```

### Pattern-matching subscriptions
Redis Pub/Sub supports pattern matching in which clients may subscribe to glob-style patterns to receive all the messages sent to channel names matching a given pattern.

#### Subscribe using channel pattern 
```c#
cache.Subscribe<User>("users.*", user => Console.WriteLine(user.Id));
```
This will listen to any channel whose name starts with "*users.*".

#### Unsubscribe using channel pattern 
```c#
cache.Unsubscribe("users.*");
```

#### Naïve 4-lines chat application
```c#
static void Main()
{
    var cache = new CacheContext("10.0.0.1:7000");
    cache.Subscribe<string>("chat", m => Console.WriteLine(m));
    while (true)
    {
        cache.Publish("chat", Console.ReadLine());
    }
}
```








