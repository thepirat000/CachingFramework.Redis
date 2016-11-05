# CachingFramework.Redis

[![Gitter](https://badges.gitter.im/CachingFramework-Redis/Lobby.svg)](https://gitter.im/CachingFramework-Redis/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=body_badge)

.NET Redis client library based on [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis/) adding some interesting features like an **extensible serialization** strategy, a **tagging mechanism** to group keys, hash fields and set members, and a **fetching mechanism** to support atomic add/get operations, all being cluster-compatible.

## Features
 * .NET Framework and .NET Core support (Net Standard 1.5)
 * [**Typed cache**](#typed-cache): any serializable object can be used as a cache value.
 * [**Fetching mechanism**](#fetching-mechanism): shortcut cache methods for atomic add/get operations (cache-aside pattern).
 * [**Tagging mechanism**](#tagging-mechanism): cache items can be tagged allowing to retrieve or invalidate keys or members by tag.
 * [**Time-To-Live mechanism**](#add-a-single-object-with-ttl): each key can be associated to a value defining its time-to-live.
 * [**Lexicographically sorted sets**](https://github.com/thepirat000/CachingFramework.Redis/blob/master/COLLECTIONS.md#redis-lexicographical-sorted-set): for fast string matching and auto-complete suggestion. 
 * [**Pub/Sub support**](#pubsub-api): Publish-Subscribe implementation with strongly typed messages.
 * [**Geospatial indexes**](#geospatial-api): with radius queries support.
 * [**HyperLogLog support**](#hyperloglog-api): to count unique things.
 * [**Configurable Serialization**](#serialization): a compressed binary serializer by default, or provide your own serialization. 
 * [**Redis data types as .NET collections**](https://github.com/thepirat000/CachingFramework.Redis/blob/master/COLLECTIONS.md): List, Set, Sorted Set, Hash and Bitmap support as managed collections.
 * [**Redis Keyspace Notifications**](#keyspace-notifications-api): Subscribe to Pub/Sub channels in order to receive events affecting the Redis data set.
 * [**Fully compatible with Redis Cluster**](#why-some-redis-commands-are-not-implemented): all commands are cluster-safe.
 
## Usage

### [NuGet](https://www.nuget.org/packages/CachingFramework.Redis/)
```
PM> Install-Package CachingFramework.Redis
```

### Context
The `Context` class provides all the functionality divided into five categories, each of which is exposed as a property with the following names:
- Cache
- Collections
- GeoSpatial
- PubSub
- KeyEvents
 
#### Default configuration
Connect to Redis on localhost port 6379:
```c#
var context = new Context();
```
#### Custom configuration
```c#
var context = new Context("10.0.0.1:7000, 10.0.0.2:7000, connectRetry=10, abortConnect=false, allowAdmin=true");
```

The constructor parameter must be a valid StackExchange.Redis connection string. See [this](https://github.com/StackExchange/StackExchange.Redis/blob/master/Docs/Configuration.md) for StackExchange.Redis configuration options.

--------------

Typed cache
=====
Any primitive type or serializable class can be used as a cache value.

For example:
```c#
[Serializable]
public class User
{
    public int Id { get; set; }
    public string UserName { get; set; }
	   ...
}
```

* Note: The *Serializable* attribute is needed by the default serialization method [binary serializer](https://github.com/thepirat000/CachingFramework.Redis#serialization). If you use, for example, a [JSon Serializer](https://www.nuget.org/packages/CachingFramework.Redis.Json/) the Serializable attribute becomes unnecessary.

### Add a single object to the cache:
```c#
string redisKey = "user:1";
User value = new User() { Id = 1 };  // any serializable object 
context.Cache.SetObject(redisKey, value);
```

### Add a single object with TTL
Add a single object to the cache with a Time-To-Live of 1 day:
```c#
context.Cache.SetObject(redisKey, value, TimeSpan.FromDays(1));
```
### Get a single object
```c#
User user = context.Cache.GetObject<User>(redisKey);
```
### Remove a key
```c#
context.Cache.Remove(redisKey);
```

--------------

Fetching mechanism
=====
Shortcut methods are provided for atomic add/get operations (see [Cache-Aside pattern](https://msdn.microsoft.com/en-us/library/dn589799.aspx)).
![Image of Fetching Mechanism](http://i.imgur.com/Kb9OBlK.png)

#### Fetch an object
Try to get an object from the cache, inserting it to the cache if it does not exists:
```c#
var user = context.Cache.FetchObject<User>(redisKey, () => GetUserFromDatabase(id));
```
The method `GetUserFromDatabase` will only be called when the value is not present on the cache, in which case will be added to the cache before returning it.

Fetch an object with a time-to-live:
```c#
var user = context.Cache.FetchObject<User>(redisKey, () => GetUserFromDatabase(id), TimeSpan.FromDays(1));
```

The TTL value is set only when the value is not present on the cache. 

--------------

Hashes
=====

Hashes are maps composed of fields associated with values, like .NET dictionaries.

![Image of hashes](http://i.imgur.com/B6Wz7es.png)

#### Set hashed objects
Set an object on a redis key indexed by a field key (sub-key):
```c#
void InsertUser(User user)
{
    var redisKey = "users:hash";
    var fieldKey = "user:id:" + user.Id;
    context.Cache.SetHashed(redisKey, fieldKey, user);
}
```
#### Get hashed object
Get an object by the redis key and a field key:
```c#
User u = context.Cache.GetHashed<User>("users:hash", "user:id:1");
```
#### Get all the objects in a hash 
```c#
IDictionary<string, User> users = context.Cache.GetHashedAll<User>("users:hash");
```
Objects within a hash can be of different types. 

#### Scan fields by pattern
Incrementally iterate over the hash members by matching a glob-style pattern with the field names.

For example, to iterate over the members of a hash whose field names starts with "user:".

```c#
var scan = context.Cache.ScanHashed<User>("users:hash", "user:*");
foreach (var item in scan)
{
    string key = item.Key;
    User value = item.Value;
    // ...
}
```

#### Remove object from hash
```c#
context.Cache.RemoveHashed("users:hash", "user:id:1");
```

#### Fetch a hashed object
```c#
var user = context.Cache.FetchHashed<User>("users:hash", "user:id:1", () => GetUser(1));
```
The method `GetUser` will only be called when the value is not present on the hash, in which case will be added to the hash before returning it.

#### Hash as .NET Dictionary

Hashes can be handled as .NET Dictionaries by using the `GetRedisDictionary` method on `Context.Collections`, for example:

```c#
var dict = context.Collections.GetRedisDictionary<string, User>("users:hash");
dict.Add("user:id:1", user);
```

For more information about collections, please see [COLLECTIONS.md](https://github.com/thepirat000/CachingFramework.Redis/blob/master/COLLECTIONS.md).

--------------

Tagging mechanism
=====

Cluster compatible tagging mechanism where tags are used to group keys, hash fields, set members, sorted set members and geospatial members, so they can be retrieved or invalidated at the same time. 
A tag can be related to any number of keys, hash fields, or set members.

![Image of Tagging Mechanism](http://i.imgur.com/CLGr0Pg.jpg)

#### Add a **single object** related to a tag
Add a single object to the cache and associate it with tags *red* and *blue*:
```c#
context.Cache.SetObject("user:1", user, new[] { "red", "blue" });
```

#### Add a **hashed object** related to a tag
Tags can point to a field in a hash.
```c#
context.Cache.SetHashed("users:hash", "user:id:1", value, new[] { "red" });
```

#### Add a member to a **redis set** related to a tag:
Add a single member to a redis set and associate the member to the tag *red*:
```c#
context.Cache.AddToSet("users:set", value, new[] { "red" });
```

#### Add a member to a **redis sorted set** related to a tag:
Add a single member to a redis sorted set and associate the member to the tag *blue*:
```c#
context.Cache.AddToSortedSet("users:sortedset", 100.00, value, new[] { "blue" });
```

#### Relate an existing **key** to a tag
Relate the key to the *green* tag:
```c#
context.Cache.AddTagsToKey("user:1", new [] { "green" });
```

#### Relate an existing **hash field** to a tag
Relate the hash field to the *green* tag:
```c#
context.Cache.AddTagsToHashField("users:hash", "user:id:1", new[] {"green"});
```

#### Relate an existing **member of a redis set** to a tag
Relate a set member to the *blue* tag:
```c#
context.Cache.AddTagsToSetMember("users:set", "user:id:1", new[] { "blue" });
```
The same method can be used to relate tags to Sorted Set members and GeoSpatial index members.

#### Remove a tag from a key
Remove the relation between the key and the tag *green*:
```c#
context.Cache.RemoveTagsFromKey("user:1", new [] { "green" });
```

#### Remove a tag from a hash field
Remove the relation between the hash field and the tag *green*:
```c#
context.Cache.RemoveTagsFromHashField("users:hash", "user:id:1", new [] { "green" });
```

#### Remove a tag from a redis set
Remove the relation between a set member and the tag *green*:
```c#
context.Cache.RemoveTagsFromSetMember("users:set", "user:id:1", new[] { "green" });
```
The same method can be used to remove tags from Sorted Set members and GeoSpatial index members.

#### Get objects by tag
Get all the objects related to *red* and/or *green*. Assuming all the keys related to the tags are of the same type:
```c#
IEnumerable<User> users = context.Cache.GetObjectsByTag<User>("red", "green");
```



#### Invalidate keys by tags
Remove all the keys, hash fields, set members and sorted set members related to *blue* and/or *green* tags:
```c#
context.Cache.InvalidateKeysByTag("blue", "green");
```

#### Get keys by tag
Get all the keys, hash fields and set members related to the given tags:
```c#
ISet<string> keys = context.Cache.GetKeysByTag(new [] { "green" });
```

If the tag is related to a _hash field_, the string returned will be in the form: 

`{hashKey}:$_->_$:{field}`

If the tag is related to a _set_, _sorted set_ or _geospatial index_ the string returned will be in the form: 

`{setKey}:$_-S>_$:{member}`

For example:

`users:hash:$_->_$:user:id:1` 
means the field `user:id:1` of hash `users:hash`.

`users:set:$_-S>_$:user:id:2` 
means the member `user:id:2` of set `users:set`.

--------------

Pub/Sub API
=====

A strongly typed Publish/Subscribe mechanism is provided.

#### Subscribe to a channel
Listen for messages of type `User` on the channel *users*:
```c#
context.PubSub.Subscribe<User>("users", user => Console.WriteLine(user.Id));
```

#### Publish to a channel
Publishes a messages of type `User` to the channel *users*:
```c#
context.PubSub.Publish<User>("users", new User() { Id = 1 });
```

#### Unsubscribe from a channel
```c#
context.PubSub.Unsubscribe("users");
```

### Pattern-matching subscriptions
Redis Pub/Sub supports pattern matching in which clients may subscribe to glob-style patterns to receive all the messages sent to channel names matching a given pattern.

#### Subscribe using channel pattern 
```c#
context.PubSub.Subscribe<User>("users.*", user => Console.WriteLine(user.Id));
```
This will listen to any channel whose name starts with "*users.*".

#### Unsubscribe using channel pattern 
```c#
context.PubSub.Unsubscribe("users.*");
```

#### Naïve 4-lines chat application
```c#
static void Main()
{
    var context = new Context("10.0.0.1:7000");
    context.PubSub.Subscribe<string>("chat", m => Console.WriteLine(m));
    while (true)
    {
        context.PubSub.Publish("chat", Console.ReadLine());
    }
}
```

--------------

Geospatial API
=====
The Geospatial Redis API consists of a set of commands that add support for storing and querying pairs of longitude/latitude coordinates into Redis keys.

> The Geospatial API is available from Redis version >= 3.2.0.

### Add a Geospatial item
Add a user to a geospatial index by its coordinates:
```c#
string redisKey = "users:geo";
context.GeoSpatial.GeoAdd<User>(redisKey, new GeoCoordinate(20.637, -103.402), user);
```

### Add a Geospatial item related to a tag
Add a user to a geospatial index and relate it to a tag:
```c#
string redisKey = "users:geo";
context.GeoSpatial.GeoAdd<User>(redisKey, new GeoCoordinate(20.637, -103.402), user, new[] { "tag" });
```

### Get the coordinates for an item
```c#
GeoCoordinate coord = context.GeoSpatial.GeoPosition(redisKey, user);
var lat = coord.Latitude;
var lon = coord.Longitude;
```

### Get the distance between two items
Get the distance in Kilometers between two user items:
```c#
double dist = context.GeoSpatial.GeoDistance(redisKey, user1, user2, Unit.Kilometers);
```

### Get the items within the radius of a center location
Get the users within a 100 Km radius: 
```c#
string redisKey = "users:geo";
var center = new GeoCoordinate(20.553, -102.925);
double radius = 100;
var results = context.GeoSpatial.GeoRadius<User>(redisKey, center, radius, Unit.Kilometers);
```
The results includes the position and the distance from the center:
```c#
foreach (var r in results)
{
    double dist = r.DistanceToCenter;
    GeoCoordinate pos = r.Position;
    User user = r.Value;
    ...
}
```

### Get the distance between two cities

Get the distance (in kilometers) between two addresses by using [GoogleMaps.LocationServices](https://github.com/sethwebster/GoogleMaps.LocationServices):
```c#
private Context _context = new Context();
private GoogleLocationService _location = new GoogleLocationService();
        
public double Distance(string address1, string address2)
{
    var redisKey = "dist";
    var loc1 = _location.GetLatLongFromAddress(address1);
    var loc2 = _location.GetLatLongFromAddress(address2);
    _context.GeoSpatial.GeoAdd(redisKey, new[]
    {
      new GeoMember<string>(loc1.Latitude, loc1.Longitude, address1),
      new GeoMember<string>(loc2.Latitude, loc2.Longitude, address2)
    });
    return _context.GeoSpatial.GeoDistance(redisKey, address1, address2, Unit.Kilometers);
}
```
For example:
```c#
double km = Distance("London", "Buenos Aires");
```

--------------

HyperLogLog API
=====
The [Redis HyperLogLog implementation](http://antirez.com/news/75) provides a very good approximation of the cardinality of a set using a very small amount of memory.

### Add elements
To add elements to the HLL, use the `HyperLogLogAdd` method:
```c#
bool result = context.Cache.HyperLogLogAdd<string>("key", "10.0.0.1");
```
The method returns `True` if the underlying HLL count was modified.

To get the cardinality (the count of unique elements) use the `HyperLogLogCount` method:
```c#
long count = context.Cache.HyperLogLogCount("key");
```

### Count unique logins per day
Considering a unique login as the Username + IP address combination.

Each time a user login, add the element to the HLL with the `HyperLogLogAdd` method:
```c#
public void OnLogin(string userName, string ipAddress)
{
    var info = new LoginInfo(userName, ipAddress);
    var key = "logins:" + DateTime.Now.ToString("yyyyMMdd");
    context.Cache.HyperLogLogAdd(key, info);
}
```

To get the unique login count for a specific date, use the `HyperLogLogCount` method:
```c#
public long GetLoginCount(DateTime date)
{
    var key = "logins:" + date.ToString("yyyyMMdd");
    return context.Cache.HyperLogLogCount(key);
}
```

--------------

Serialization
=====

To provide your own serialization mechanism implement the `ISerializer` interface.

For example, a custom serializer for simple types:
```c#
public class MySerializer : ISerializer
{
    public byte[] Serialize<T>(T value)
    {
        return Encoding.UTF8.GetBytes(value.ToString());
    }
    public T Deserialize<T>(byte[] value)
    {
        return (T)Convert.ChangeType(Encoding.UTF8.GetString(value), typeof(T));
    }
}
```

The `Context` class has constructor overloads to supply the serialization mechanism, for example:

```c#
var context = new Context("localhost:6379", new MySerializer());
```

Different serialization mechanisms are provided:

- **Binary Serializer** (default for NET45 and NET461):
All types are serialized using the .NET `BinaryFormatter` from `System.Runtime.Serialization` and compressed using GZIP from `System.IO.Compression`.

- **Json Serializer** (default for NET CORE):
All types are serialized using the [JSON.NET](https://www.nuget.org/packages/Newtonsoft.Json/) library. 

- **Raw Serializer**:
The [simple types](https://msdn.microsoft.com/en-us/library/ya5y69ds.aspx) are serialized as strings (UTF-8 encoded).
Any other type is serialized using the default serializer.

| | **Inheritance** | **Data** | **Configuration** |
| ----------- | ----------------------- | -------------------------- | ------------------ |
|**BinarySerializer** | Full inheritance support | Data is compressed and not human readable | Serialization cannot be configured (except NonSerialized attribute)| 
|**JsonSerializer** | Limited inheritance support | Data is stored as Json | Serialization can be configured with JsonSerializerSettings | 
|**RawSerializer** | Limited inheritance, only for types serialized with BinaryFormatter | Simple types are stored as strings and are human readable | Serialization can be set-up per type using SetSerializerFor | 

The `RawSerializer` allows to dynamically override the serialization/deserialization logic per type with the method `SetSerializerFor<T>()`.

For example, to allow the serialization of a `StringBuilder` as an UTF-8 encoded string:

```c#
var raw = new RawSerializer();
raw.SetSerializerFor<StringBuilder>
(
    sb => Encoding.UTF8.GetBytes(sb.ToString()), 
    b => new StringBuilder(Encoding.UTF8.GetString(b))
);
var context = new Context("localhost:6379", raw);
```
--------------

Keyspace Notifications API
=====

Subscribe to keyspace events to receive events affecting the Redis data.
See the Redis notification [documentation](http://redis.io/topics/notifications).

### Server configuration

By default keyspace events notifications are disabled. To enable notifications use the notify-keyspace-events of redis.conf or via the CONFIG SET, for example:
```
redis> CONFIG SET notify-keyspace-events KEA
```

### Usage
To access the Keyspace Notifications API, use the `Subscribe`/`Unsubscribe` methods on the context's `KeyEvents` property.

The subscribe method callback in an `Action<string, KeyEvent>` where the first parameter is the Redis key affected, and the second is the operation performed. 

### Examples 

Receive all the commands affecting a specific key:
```c#
context.KeyEvents.Subscribe("user:1", (string key, KeyEvent cmd) =>
{
    if (cmd == KeyEvent.Delete)
    {
        //Key "user:1" was deleted
    }
    Console.WriteLine("command " + cmd);
});
```

Receive a specific command affecting any key:
```c#
context.KeyEvents.Subscribe(KeyEvent.PushLeft, (key, cmd) =>
{
    Console.WriteLine("key {0} received an LPUSH", key);
});
```

Receive any command affecting any key:
```c#
context.KeyEvents.Subscribe(KeyEventSubscriptionType.All, (key, cmd) =>
{
    Console.WriteLine("key {0} - command {1}", key, cmd);
});
```

Stop receiving all commands affecting the given key:
```c#
context.KeyEvents.Unsubscribe("user:1");
```

Stop receiving LPUSH commands affecting any key:
```c#
context.KeyEvents.Unsubscribe(KeyEvent.PushLeft);
```

--------------

Why some Redis commands are not implemented?
=====

Some Redis commands were omitted by design falling into these two categories:

- Commands that operates on multiple keys are not included because they are incompatible with a cluster topology.
(i.e. MGET, SINTER, SUNION)

- Commands that assumes a format on the Redis value were omitted because the library doesn't make assumptions on the serialization method. (i.e. INCRBY, APPEND.) (Except for the collections 
[RedisBitmap](https://github.com/thepirat000/CachingFramework.Redis/blob/master/COLLECTIONS.md#redis-bitmaps--), 
[RedisLexicographicSet](https://github.com/thepirat000/CachingFramework.Redis/blob/master/COLLECTIONS.md#redis-lexicographical-sorted-set) 
and [RedisString](https://github.com/thepirat000/CachingFramework.Redis/blob/master/COLLECTIONS.md#redis-string)) 

You can still call these commands via `StackExchange.Redis` API, accesing the `ConnectionMultiplexer` by calling the `GetConnectionMultiplexer()` method on the `Context` (see next section).


--------------

StackExchange.Redis API
=====

To use the `StackExchange.Redis` API, call the `GetConnectionMultiplexer()` method on the `Context`.

For example:
```c#
var context = new Context();
var multiplexer = context.GetConnectionMultiplexer();	// SE.Redis Connection Multiplexer
multiplexer.GetDatabase().StringIncrement("key", 1);    // SE.Redis API
```


[.NET Collections](https://github.com/thepirat000/CachingFramework.Redis/blob/master/COLLECTIONS.md)
=====

You can handle **Redis Lists** as `IList<T>`, **Hashes** as `IDictionary<K, V>`, **Sets**, **Lex Sets** and **Bitmaps** as `ICollection<T>`, and more.

Access these objects by the `Collections` property on `Context`.

For example:
```c#
IList<User> redisList = context.Collections.GetRedisList<User>("users:list");
redisList.Add(user);
```

**For details please see [COLLECTIONS.md](https://github.com/thepirat000/CachingFramework.Redis/blob/master/COLLECTIONS.md) documentation file**
