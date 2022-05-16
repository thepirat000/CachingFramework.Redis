
using System.Collections.Generic;
using System.Linq;
using CachingFramework.Redis.Contracts;
using NUnit.Framework;

namespace CachingFramework.Redis.UnitTest
{
    [TestFixture]
    public class UnitTestGeo
    {
        private static GeoCoordinate _coordZapopan;
        private static GeoCoordinate _coordLondon;

        [OneTimeSetUp]
        public static void ClassInitialize()
        {
            if (Common.VersionInfo[0] < 3)
            {
                Assert.Ignore($"Geospatial tests ignored for version {string.Join(".", Common.VersionInfo)}\n");
            }
            _coordZapopan = new GeoCoordinate(20.6719563, -103.416501);
            _coordLondon = new GeoCoordinate(51.5073509, -0.1277583);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_Geo_GeoAdd(RedisContext context)
        {
            var key = "UT_Geo_GeoAdd";
            context.Cache.Remove(key);
            var users = GetUsers();
            var cnt = context.GeoSpatial.GeoAdd(key, _coordZapopan, users[0]);
            cnt += context.GeoSpatial.GeoAdd(key, _coordLondon.Latitude, _coordLondon.Longitude, users[1]);
            var coord = context.GeoSpatial.GeoPosition(key, users[0]);
            Assert.AreEqual(2, cnt);
            Assert.AreEqual(_coordZapopan.Latitude, coord.Latitude, 0.00001);
            Assert.AreEqual(_coordZapopan.Longitude, coord.Longitude, 0.00001);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_Geo_GeoPos(RedisContext context)
        {
            var key = "UT_Geo_GeoPos";
            context.Cache.Remove(key);
            var cnt = context.GeoSpatial.GeoAdd(key, _coordZapopan, "Zapopan");
            var coordGet = context.GeoSpatial.GeoPosition(key, "Zapopan");
            var coordErr = context.GeoSpatial.GeoPosition(key, "not exists");
            Assert.IsNull(coordErr);
            Assert.AreEqual(1, cnt);
            Assert.AreEqual(_coordZapopan.Latitude, coordGet.Latitude, 0.00001);
            Assert.AreEqual(_coordZapopan.Longitude, coordGet.Longitude, 0.00001);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_Geo_GeoPosMultiple(RedisContext context)
        {
            var key = "UT_Geo_GeoPosMultiple";
            context.Cache.Remove(key);
            var cnt = context.GeoSpatial.GeoAdd(key, new[] { 
                new GeoMember<string>(_coordZapopan, "Zapopan"),
                new GeoMember<string>(_coordLondon, "London") });
            var coords = context.GeoSpatial.GeoPosition(key, new[] { "London", "not exists", "Zapopan" }).ToArray();
            Assert.AreEqual(2, cnt);
            Assert.AreEqual(3, coords.Length);
            Assert.AreEqual("London", coords[0].Value);
            Assert.AreEqual("Zapopan", coords[2].Value);
            Assert.AreEqual(_coordLondon.Latitude, coords[0].Position.Latitude, 0.00001);
            Assert.AreEqual(_coordLondon.Longitude, coords[0].Position.Longitude, 0.00001);
            Assert.AreEqual(_coordZapopan.Latitude, coords[2].Position.Latitude, 0.00001);
            Assert.AreEqual(_coordZapopan.Longitude, coords[2].Position.Longitude, 0.00001);
            Assert.AreEqual(null, coords[1]);
        }

        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_Geo_GeoDistance(RedisContext context)
        {
            var key = "UT_Geo_GeoDistance";
            context.Cache.Remove(key);
            var cnt = context.GeoSpatial.GeoAdd(key, new[] { 
                new GeoMember<string>(_coordZapopan, "Zapopan"),
                new GeoMember<string>(_coordLondon, "London") });
            var kmzz = context.GeoSpatial.GeoDistance(key, "Zapopan", "Zapopan", Unit.Kilometers);
            var kmzl = context.GeoSpatial.GeoDistance(key, "Zapopan", "London", Unit.Kilometers);
            var kmlz = context.GeoSpatial.GeoDistance(key, "London", "Zapopan", Unit.Kilometers);
            var err = context.GeoSpatial.GeoDistance(key, "London", "not exists", Unit.Kilometers);
            Assert.AreEqual(-1, err);
            Assert.AreEqual(2, cnt);
            Assert.AreEqual(0, kmzz, 0.00001);
            Assert.AreEqual(kmlz, kmzl, 0.00001);
            Assert.AreEqual(9100, kmlz, 100);
        }

#if (NET461)
        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_Geo_GeoDistanceDirect(RedisContext context)
        {
            var key = "UT_Geo_GeoDistanceDirect";
            context.Cache.Remove(key);
            var mdq = new GeoCoordinate(38.0055, -57.5426);
            var bue = new GeoCoordinate(34.6037, -58.3816);

            context.GeoSpatial.GeoAdd(key, new[] { 
                new GeoMember<string>(mdq, "mdq"),
                new GeoMember<string>(bue, "bue") });
            var km = context.GeoSpatial.GeoDistance(key, "mdq", "bue", Unit.Kilometers);
            Assert.AreEqual(385, km, 15);
        }
#endif
        [Test, TestCaseSource(typeof(Common), "All")]
        public void UT_Geo_GeoHash(RedisContext context)
        {
            var key = "UT_Geo_GeoHash";
            context.Cache.Remove(key);
            context.GeoSpatial.GeoAdd(key, _coordZapopan, "zapopan");
            var hash = context.GeoSpatial.GeoHash(key, "zapopan");
            var hashErr = context.GeoSpatial.GeoHash(key, "not exists");
            Assert.IsNull(hashErr);
            Assert.IsTrue(hash.StartsWith("9ewmwenq"));
        }

#if (NET461)
        [Test, TestCaseSource(typeof(Common), "Json")]
        public void UT_Geo_GeoRadius(RedisContext context)
        {
            var key = "UT_Geo_GeoRadius";
            context.Cache.Remove(key);
            context.GeoSpatial.GeoAdd(key, _coordZapopan, "zapopan");
            context.GeoSpatial.GeoAdd(key, _coordLondon, "london");
            var coordMx = new GeoCoordinate(19.4326, -99.1332);
            context.GeoSpatial.GeoAdd(key, coordMx, "mexico");
            var coordZam = new GeoCoordinate(19.9902, -102.2834);
            context.GeoSpatial.GeoAdd(key, coordZam, "zamora");
            var coordMor = new GeoCoordinate(19.7060, -101.1950);
            var results500 = context.GeoSpatial.GeoRadius<string>(key, coordMor, 750, Unit.Kilometers).ToList();
            var results200 = context.GeoSpatial.GeoRadius<string>(key, coordMor, 200, Unit.Kilometers).ToList();
            var results500_count1 = context.GeoSpatial.GeoRadius<string>(key, coordMor, 500, Unit.Kilometers, 1).ToList();
            var results0 = context.GeoSpatial.GeoRadius<string>(key, coordMor, 1, Unit.Kilometers).ToList();
            Assert.AreEqual(0, results0.Count);
            Assert.AreEqual(3, results500.Count);
            Assert.AreEqual(1, results200.Count);
            Assert.AreEqual(1, results500_count1.Count);
            Assert.AreEqual("zamora", results500_count1[0].Value);
            Assert.AreEqual("zamora", results200[0].Value);
            Assert.AreEqual("zamora", results500[0].Value);
            Assert.AreEqual(118, results500[0].DistanceToCenter, 2);
            Assert.AreEqual(coordZam.Latitude, results500[0].Position.Latitude, 0.00001);
            Assert.AreEqual(coordZam.Longitude, results500[0].Position.Longitude, 0.00001);
            Assert.AreEqual("mexico", results500[1].Value);
            Assert.AreEqual("zapopan", results500[2].Value);
        }
#endif

        private List<User> GetUsers()
        {
            var loc1 = new Location()
            {
                Id = 1,
                Name = "one"
            };
            var loc2 = new Location()
            {
                Id = 2,
                Name = "two"
            };
            var user1 = new User()
            {
                Id = 1,
                Deparments = new List<Department>()
                {
                    new Department() {Id = 1, Distance = 123.45m, Size = 2, Location = loc1},
                    new Department() {Id = 2, Distance = 400, Size = 1, Location = loc2}
                }
            };
            var user2 = new User()
            {
                Id = 2,
                Deparments = new List<Department>()
                {
                    new Department() {Id = 3, Distance = 500, Size = 1, Location = loc2},
                    new Department() {Id = 4, Distance = 125.5m, Size = 3, Location = loc1}
                }
            };
            var user3 = new User()
            {
                Id = 3,
                Deparments = new List<Department>()
                {
                    new Department() {Id = 5, Distance = 5, Size = 5, Location = loc2},
                }
            };
            var user4 = new User()
            {
                Id = 4,
                Deparments = new List<Department>()
                {
                    new Department() {Id = 6, Distance = 100, Size = 10, Location = loc1},
                }
            };
            return new List<User>() { user1, user2, user3, user4 };
        }
    }
#if (NET461)
    public static class TempExtensions
    {
        public static GeoCoordinate ToGeoCoord(double lat, double lon)
        {
            return new GeoCoordinate(lat / 10000000, lon / 10000000);
        }
    }
#endif
}
