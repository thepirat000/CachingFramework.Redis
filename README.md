# CachingFramework.Redis
.NET Redis Distributed Cache library based on [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis/)

##Features
 * **Tagging mechanism** to store cache items with tags allowing to retrieve or invalidate items by tag.
 * **Fetching mechanism** as shortcut methods for atomic Add/Get operations.
 * **Time-To-Live mechanism**.
 * **Fully compatible with Redis Cluster**.
 * **Compressed binary serialization** (or implement your own serialization).
 * **Redis Strings, List, Sets, Sorted Sets, Hashes and Bitmaps support** as managed [collections](https://github.com/thepirat000/CachingFramework.Redis/blob/master/COLLECTIONS.md).
 * **Lexicographically sorted sets** for fast auto-complete suggestion. 
 * **Pub/Sub support with typed messages**.
 * **Geospatial indexes support**.
 * **HyperLogLog algorithm support**.
 
 
## Usage

### [NuGet](https://www.nuget.org/packages/CachingFramework.Redis/)
```
PM> Install-Package CachingFramework.Redis
```

### Context
The `Context` class provides all the functionality divided into four categories, each of which is exposed as a property with the following names:
- Cache
- Collections
- GeoSpatial
- PubSub
 
#### Default configuration
Connect to Redis on localhost port 6379:
```c#
var context = new Context();
```
#### Custom configuration
```c#
var context = new Context("10.0.0.1:7000, 10.0.0.2:7000, 10.0.0.3:7000, connectRetry=10, syncTimeout=5000, abortConnect=false, allowAdmin=true");
```
See [this](https://github.com/StackExchange/StackExchange.Redis/blob/master/Docs/Configuration.md) for StackExchange.Redis configuration options.

### Adding objects to the cache

#### Add a single object to the cache
Add a single object to the cache:
```c#
string redisKey = "user:1";
User value = new User() { Id = 1 };  // any serializable object 
context.Cache.SetObject(redisKey, value);
```

#### Add a single object related to multiple tags
Add a single object to the cache and associate it with tags *tag1* and *tag2*:
```c#
context.Cache.SetObject(redisKey, value, new[] { "tag1", "tag2" });
```

#### Add a single object with TTL
Add a single object to the cache with a Time-To-Live of 1 day:
```c#
context.Cache.SetObject(redisKey, value, TimeSpan.FromDays(1));
```

### Getting objects

#### Get a single object
```c#
User user = context.Cache.GetObject<User>(redisKey);
```

### Removing objects

#### Remove a key
```c#
context.Cache.Remove(redisKey);
```

#### Invalidate keys by multiple tags
Remove all the keys related to *tag1* and/or *tag2*:
```c#
context.Cache.InvalidateKeysByTag("tag1", "tag2");
```

#### Get objects by tag
Get all the objects related to *tag1* and/or *tag2*. Assuming all the keys related to the tags are of type `User`:
```c#
IEnumerable<User> users = context.Cache.GetObjectsByTag<User>("tag1", "tag2");
```

### Fetching objects

#### Fetch an object
Try to get an object from the cache, inserting it to the cache if it does not exists:
```c#
var user = context.Cache.FetchObject<User>(redisKey, () => GetUserFromDatabase(id));
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
    context.Cache.SetHashed(redisKey, fieldKey, user);
}
```
#### Get hashed object
Get an object by the redis key and a field key:
```c#
User u = context.Cache.GetHashed<User>(redisKey, "user:id:1");
```
#### Get all the objects in a hash 
```c#
IDictionary<string, User> users = context.Cache.GetHashedAll<User>(redisKey);
```
Objects within a hash can be of different types. 

#### Remove object from hash
```c#
context.Cache.RemoveHashed(redisKey, "user:id:1");
```

### [.NET Collections](https://github.com/thepirat000/CachingFramework.Redis/blob/master/COLLECTIONS.md)
Implementations of .NET IList, ISet and IDictionary that internally uses Redis as storage are provided.

#### Get a .NET IList stored as a Redis List
```c#
IList<User> users = context.Collections.GetRedisList<User>(redisKey);
```

#### Get a .NET ISet stored as a Redis Set
```c#
ISet<User> users = context.Collections.GetRedisSet<User>(redisKey);
```

#### Get a .NET ICollection stored as a Redis Sorted Set
```c#
ICollection<User> users = context.Collections.GetRedisSortedSet<User>(redisKey);
```

#### Get a .NET IDictionary stored as a Redis Hash
```c#
IDictionary<string, User> users = context.Collections.GetRedisDictionary<string, User>(redisKey);
```

#### Get a .NET ICollection<bool> stored as a Redis Bitmap
```c#
ICollection<bool> bitmap = context.Collections.GetRedisBitmap(redisKey);
```

**For more details please see [COLLECTIONS.md](https://github.com/thepirat000/CachingFramework.Redis/blob/master/COLLECTIONS.md) documentation file**

Pub/Sub API
=====

A typed Publish/Subscribe mechanism is provided.

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

#### Messages types
Each subscription listen to messages of the specified type (or inherited from it):
```c#
context.PubSub.Subscribe<User>("entities", user => Console.WriteLine(user.Id));
context.PubSub.Subscribe<Manager>("entities", mgr => Console.WriteLine(mgr.Id));
```
Subscription of type `object` will listen to all types:
```c#
context.PubSub.Subscribe<object>("entities", obj => Console.WriteLine(obj));
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

Geospatial API
=====
The Geospatial Redis API is not yet available in a stable version of Redis. Download unstable if you want to test these commands.

### Add a Geospatial item
Add a user to a geospatial index by its coordinates:
```c#
string redisKey = "users:geo";
context.GeoSpatial.GeoAdd<User>(redisKey, new GeoCoordinate(20.637, -103.402), user);
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
