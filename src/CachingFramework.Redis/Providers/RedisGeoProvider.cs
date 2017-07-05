using System;
using System.Collections.Generic;
using System.Linq;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Contracts.Providers;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace CachingFramework.Redis.Providers
{
    /// <summary>
    /// geospatial index provider implementation using Redis.
    /// </summary>
    internal class RedisGeoProvider : RedisProviderBase, IGeoProvider
    {
        #region Fields
        private readonly ICacheProvider _cacheProvider;
        #endregion
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisGeoProvider"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="cacheProvider">The cache provider.</param>
        public RedisGeoProvider(RedisProviderContext context, ICacheProvider cacheProvider)
            : base(context)
        {
            _cacheProvider = cacheProvider;
        }
        #endregion

        #region IGeoProvider implementation

        /// <summary>
        /// Adds the specified geospatial members (latitude, longitude, object) to the specified key.
        /// </summary>
        /// <typeparam name="T">The member type</typeparam>
        /// <param name="key">The redis key.</param>
        /// <param name="members">The members to add.</param>
        /// <param name="tags">The tags to relate to the members.</param>
        /// <returns>The number of elements added to the sorted set, not including elements already existing.</returns>
        public int GeoAdd<T>(string key, GeoMember<T>[] members, string[] tags = null)
        {
            var db = RedisConnection.GetDatabase();
            var values = new GeoEntry[members.Length];
            for(int i = 0; i < values.Length; i++)
            {
                var member = members[i];
                values[i] = new GeoEntry(member.Position.Longitude, member.Position.Latitude, Serializer.Serialize(member.Value));
            }
            int result = (int)db.GeoAdd(key, values);
            // Relate the tags (if any)
            if (tags != null && tags.Length > 0)
            {
                foreach (var member in members)
                {
                    _cacheProvider.AddTagsToSetMember(key, member.Value, tags);
                }
            }
            return result;
        }
        /// <summary>
        /// Adds the specified members to a geospatial index.
        /// </summary>
        /// <typeparam name="T">The member type</typeparam>
        /// <param name="key">The redis key.</param>
        /// <param name="latitude">The member latitude coordinate.</param>
        /// <param name="longitude">The member longitude coordinate.</param>
        /// <param name="member">The member to add.</param>
        /// <param name="tags">The tags to relate to the members.</param>
        /// <returns>The number of elements added to the sorted set, not including elements already existing.</returns>
        public int GeoAdd<T>(string key, double latitude, double longitude, T member, string[] tags = null)
        {
            return GeoAdd(key, new GeoCoordinate(latitude, longitude), member, tags);
        }
        /// <summary>
        /// Adds the specified members to a geospatial index.
        /// </summary>
        /// <typeparam name="T">The member type</typeparam>
        /// <param name="key">The redis key.</param>
        /// <param name="coordinate">The member coordinates.</param>
        /// <param name="member">The member to add.</param>
        /// <param name="tags">The tags to relate to the members.</param>
        /// <returns>The number of elements added to the sorted set, not including elements already existing.</returns>
        public int GeoAdd<T>(string key, GeoCoordinate coordinate, T member, string[] tags = null)
        {
            return GeoAdd(key, new[] { new GeoMember<T>(coordinate, member) }, tags);
        }

        /// <summary>
        /// Return Geohash strings representing the position of a member in a geospatial index (where elements were added using GEOADD).
        /// </summary>
        /// <typeparam name="T">The member type</typeparam>
        /// <param name="key">The redis key.</param>
        /// <param name="member">The member.</param>
        /// <returns>System.String.</returns>
        public string GeoHash<T>(string key, T member)
        {
            var db = RedisConnection.GetDatabase();
            return db.GeoHash(key, Serializer.Serialize(member));
        }
        /// <summary>
        /// Return the positions (longitude,latitude) of all the specified members of the geospatial index at key.
        /// </summary>
        /// <typeparam name="T">The member type</typeparam>
        /// <param name="key">The redis key.</param>
        /// <param name="members">The members.</param>
        /// <returns>IEnumerable{GeoMember{``0}}.</returns>
        public IEnumerable<GeoMember<T>> GeoPosition<T>(string key, T[] members)
        {
            var db = RedisConnection.GetDatabase();
            var redisMembers = members.Select(m => (RedisValue)Serializer.Serialize<T>(m)).ToArray();
            var results = db.GeoPosition(key, redisMembers);
            if (results != null)
            {
                for(int i = 0; i < results.Length; i++)
                {
                    var result = results[i];
                    if (!result.HasValue)
                    {
                        yield return null;
                    }
                    else
                    {
                        yield return new GeoMember<T>(result.Value.Latitude, result.Value.Longitude, members[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Return the position (longitude,latitude) of the specified member of the geospatial index at key.
        /// </summary>
        /// <typeparam name="T">The member type</typeparam>
        /// <param name="key">The redis key.</param>
        /// <param name="member">The member.</param>
        /// <returns>NULL if the member does not exists</returns>
        public GeoCoordinate GeoPosition<T>(string key, T member)
        {
            var pos = GeoPosition(key, new[] { member }).FirstOrDefault();
            return pos?.Position;
        }
        /// <summary>
        /// Return the distance between two members in the geospatial index at key.
        /// </summary>
        /// <typeparam name="T">The member type</typeparam>
        /// <param name="key">The redis key.</param>
        /// <param name="member1">The first member.</param>
        /// <param name="member2">The second member.</param>
        /// <param name="unit">The result unit.</param>
        /// <returns>The distance in the given unit or -1 in case of a non-existing member.</returns>
        public double GeoDistance<T>(string key, T member1, T member2, Unit unit)
        {
            var db = RedisConnection.GetDatabase();
            var dist = db.GeoDistance(key, Serializer.Serialize(member1), Serializer.Serialize(member2), (GeoUnit)unit);
            return dist.HasValue ? dist.Value : -1;
        }
        /// <summary>
        /// Return the members of a geospatial index, which are within the borders of the area specified with the center location and the maximum distance from the center (the radius).
        /// </summary>
        /// <typeparam name="T">The member type</typeparam>
        /// <param name="key">The redis key.</param>
        /// <param name="center">The center.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="unit">The unit.</param>
        /// <param name="count">If greater than 0, limit the results to the first N matching items.</param>
        /// <returns>IEnumerable{GeoMember{``0}}.</returns>
        public IEnumerable<GeoMember<T>> GeoRadius<T>(string key, GeoCoordinate center, double radius, Unit unit, int count = 0)
        {
            var db = RedisConnection.GetDatabase();
            var results = db.GeoRadius(key, center.Longitude, center.Latitude, radius, (GeoUnit)unit, count, Order.Ascending);
            foreach(var result in results)
            {
                if (result.Position.HasValue)
                {
                    yield return new GeoMember<T>
                    (
                        result.Position.Value.Latitude,
                        result.Position.Value.Longitude,
                        Serializer.Deserialize<T>((Byte[])result.Member),
                        result.Distance.HasValue ? result.Distance.Value : -1
                    ); 
                }
                else
                {
                    yield return null;
                }
                
            }
        }
        /// <summary>
        /// Return the members of a geospatial index, which are within the borders of the area specified with the center location and the maximum distance from the center (the radius).
        /// </summary>
        /// <typeparam name="T">The member type</typeparam>
        /// <param name="key">The redis key.</param>
        /// <param name="latitude">The latitude of the center.</param>
        /// <param name="longitude">The latitude of the center.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="unit">The unit.</param>
        public IEnumerable<GeoMember<T>> GeoRadius<T>(string key, double latitude, double longitude, double radius, Unit unit)
        {
            return GeoRadius<T>(key, new GeoCoordinate(latitude, longitude), radius, unit, -1);
        }
        /// <summary>
        /// Return the members of a geospatial index, which are within the borders of the area specified with the center location and the maximum distance from the center (the radius).
        /// </summary>
        /// <typeparam name="T">The member type</typeparam>
        /// <param name="key">The redis key.</param>
        /// <param name="center">The center.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="unit">The unit.</param>
        public IEnumerable<GeoMember<T>> GeoRadius<T>(string key, GeoCoordinate center, double radius, Unit unit)
        {
            return GeoRadius<T>(key, center, radius, unit, -1);
        }
        #endregion
    }
}