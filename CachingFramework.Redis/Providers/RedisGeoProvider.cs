using System;
using System.Collections.Generic;
using System.Linq;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Contracts.Providers;
using StackExchange.Redis;

namespace CachingFramework.Redis.Providers
{
    /// <summary>
    /// geospatial index provider implementation using Redis.
    /// </summary>
    internal class RedisGeoProvider : RedisProviderBase, IGeoProvider
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisGeoProvider"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public RedisGeoProvider(RedisProviderContext context)
            : base(context)
        {
        }
        #endregion

        #region IGeoProvider implementation
        /// <summary>
        /// Adds the specified geospatial members (latitude, longitude, object) to the specified key.
        /// </summary>
        /// <typeparam name="T">The member type</typeparam>
        /// <param name="key">The redis key.</param>
        /// <param name="members">The members to add.</param>
        /// <returns>The number of elements added to the sorted set, not including elements already existing.</returns>
        public int GeoAdd<T>(string key, GeoMember<T>[] members)
        {
            var db = RedisConnection.GetDatabase();
            var values = new List<RedisValue>();
            foreach(var member in members)
            {
                values.AddRange(new RedisValue[] 
                { 
                    member.Position.Longitude, 
                    member.Position.Latitude, 
                    Serializer.Serialize(member.Value) 
                });
            }
            return (int)db.ScriptEvaluate(LuaScriptResource.GeoAdd, new RedisKey[] { key }, values.ToArray());
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
            var results = (RedisResult[])db.ScriptEvaluate(LuaScriptResource.GeoHash, new RedisKey[] { key }, new RedisValue[] { Serializer.Serialize(member) });
            return results[0].ToString();
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
            var results = ((RedisResult[])db.ScriptEvaluate(LuaScriptResource.GeoPos, new RedisKey[] { key }, redisMembers))
                .Select(r => (RedisValue[])r)
                .ToArray();
            for (int i = 0; i < results.Length; i++)
            {
                var values = results[i];
                if (values[0].IsNull)
                {
                    yield return null;
                }
                else
                {
                    yield return new GeoMember<T>((double)values[1], (double)values[0], members[i]);
                }
            }
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
            var result = db.ScriptEvaluate(LuaScriptResource.GeoDist, new RedisKey[] { key },
                new RedisValue[] { Serializer.Serialize(member1), Serializer.Serialize(member2), UnitAttribute.GetEnumUnit(unit) });
            return result.IsNull ? -1 : (double)result;
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
            var redisValues = new List<RedisValue>()
            {
                center.Longitude,
                center.Latitude,
                radius,
                UnitAttribute.GetEnumUnit(unit),
                "WITHDIST",
                "WITHCOORD"
            };
            if (count > 0)
            {
                redisValues.Add("COUNT");
                redisValues.Add(count);
            }
            redisValues.Add("ASC");
            var results = ((RedisResult[])db.ScriptEvaluate(LuaScriptResource.GeoRadius, new RedisKey[] { key }, redisValues.ToArray()))
                .Select(r => (RedisResult[])r);
            foreach (var values in results)
            {
                var distance = (double)values[1];
                var coords = (RedisValue[])values[2];
                var member = Serializer.Deserialize<T>((Byte[])values[0]);
                yield return new GeoMember<T>((double)coords[1], (double)coords[0], member, distance);
            }
        }
        #endregion
    }
}