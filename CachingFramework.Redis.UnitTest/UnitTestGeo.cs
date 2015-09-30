using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CachingFramework.Redis.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GoogleMaps.LocationServices;

namespace CachingFramework.Redis.UnitTest
{
    public static class TempExtensions
    {
        public static GeoCoordinate ToGeoCoord(this MapPoint coord)
        {
            return new GeoCoordinate(coord.Latitude, coord.Longitude);
        }
    }

    [TestClass]
    public class UnitTestGeo
    {
        private GeoCoordinate _coordZapopan;
        private GeoCoordinate _coordLondon;
        private CacheContext _context;
        private string _defaultConfig;
        private GoogleLocationService _locationSvc;
        [TestInitialize]
        public void Initialize()
        {
            _locationSvc = new GoogleLocationService();
            _coordZapopan = _locationSvc.GetLatLongFromAddress("Zapopan").ToGeoCoord();
            _coordLondon = _locationSvc.GetLatLongFromAddress("London").ToGeoCoord();
            // Config doc: https://github.com/StackExchange/StackExchange.Redis/blob/master/Docs/Configuration.md
            _defaultConfig = "192.168.15.15:7001, 192.168.15.15:7006, 192.168.15.15:7002, 192.168.15.15:7003, 192.168.15.15:7004, 192.168.15.15:7005, 192.168.15.15:7000, connectRetry=10, syncTimeout=10000, abortConnect=false, keepAlive=10, allowAdmin=true";
            _context = new CacheContext(_defaultConfig);
            _context.FlushAll();
        }

        [TestMethod]
        public void UT_Geo_GeoAdd()
        {
            var key = "UT_Geo_GeoAdd";
            var users = GetUsers();
            var cnt = _context.GeoAdd(key, _coordZapopan, users[0]);
            cnt += _context.GeoAdd(key, _coordLondon, users[1]);
            var coord = _context.GeoPosition(key, users[0]);
            Assert.AreEqual(2, cnt);
            Assert.AreEqual(_coordZapopan.Latitude, coord.Latitude, 0.00001);
            Assert.AreEqual(_coordZapopan.Longitude, coord.Longitude, 0.00001);
        }

        [TestMethod]
        public void UT_Geo_GeoPos()
        {
            var key = "UT_Geo_GeoPos";
            var cnt = _context.GeoAdd(key, _coordZapopan, "Zapopan");
            var coordGet = _context.GeoPosition(key, "Zapopan");
            var coordErr = _context.GeoPosition(key, "not exists");
            Assert.IsNull(coordErr);
            Assert.AreEqual(1, cnt);
            Assert.AreEqual(_coordZapopan.Latitude, coordGet.Latitude, 0.00001);
            Assert.AreEqual(_coordZapopan.Longitude, coordGet.Longitude, 0.00001);
        }

        [TestMethod]
        public void UT_Geo_GeoPosMultiple()
        {
            var key = "UT_Geo_GeoPosMultiple";
            var cnt = _context.GeoAdd(key, new [] { _coordZapopan, _coordLondon }, new[] {"Zapopan", "London"});
            var coords = _context.GeoPositions(key, new[] { "London", "not exists", "Zapopan" }).ToArray();
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

        [TestMethod]
        public void UT_Geo_GeoDistance()
        {
            var key = "UT_Geo_GeoDistance";
            var cnt = _context.GeoAdd(key, new [] { _coordZapopan, _coordLondon }, new[] {"Zapopan", "London"});
            var kmzz = _context.GeoDistance(key, "Zapopan", "Zapopan", Unit.Kilometers);
            var kmzl = _context.GeoDistance(key, "Zapopan", "London", Unit.Kilometers);
            var kmlz = _context.GeoDistance(key, "London", "Zapopan", Unit.Kilometers);
            var err = _context.GeoDistance(key, "London", "not exists", Unit.Kilometers);
            Assert.AreEqual(-1, err);
            Assert.AreEqual(2, cnt);
            Assert.AreEqual(0, kmzz, 0.00001);
            Assert.AreEqual(kmlz, kmzl, 0.00001);
            Assert.AreEqual(9100, kmlz, 100);
        }

        [TestMethod]
        public void UT_Geo_GeoDistanceDirect()
        {
            var key = "UT_Geo_GeoDistanceDirect";
            var mdq = _locationSvc.GetLatLongFromAddress("Mar del Plata").ToGeoCoord();
            var bue = _locationSvc.GetLatLongFromAddress("Buenos Aires").ToGeoCoord();
            _context.GeoAdd(key, new GeoCoordinate[] {mdq, bue}, new[] {"mdq", "bue"});
            var km = _context.GeoDistance(key, "mdq", "bue", Unit.Kilometers);
            Assert.AreEqual(385, km, 15);
        }

        [TestMethod]
        public void UT_Geo_GeoHash()
        {
            var key = "UT_Geo_GeoHash";
            _context.GeoAdd(key, _coordZapopan, "zapopan");
            var hash = _context.GeoHash(key, "zapopan");
            var hashErr = _context.GeoHash(key, "not exists");
            Assert.IsNull(hashErr);
            Assert.IsTrue(hash.StartsWith("9ewmwenq"));
        }

        [TestMethod]
        public void UT_Geo_GeoRadius()
        {
            var key = "UT_Geo_GeoRadius";
            _context.GeoAdd(key, _coordZapopan, "zapopan");
            _context.GeoAdd(key, _coordLondon, "london");
            var coordMx = _locationSvc.GetLatLongFromAddress("Mexico DF").ToGeoCoord();
            _context.GeoAdd(key, coordMx, "mexico");
            var coordZam = _locationSvc.GetLatLongFromAddress("Zamora, Michoacan").ToGeoCoord();
            _context.GeoAdd(key, coordZam, "zamora");
            var coordMor = _locationSvc.GetLatLongFromAddress("Morelia, Michoacan").ToGeoCoord();
            var results500 = _context.GeoRadius<string>(key, coordMor, 500, Unit.Kilometers).ToList();
            var results200 = _context.GeoRadius<string>(key, coordMor, 200, Unit.Kilometers).ToList();
            var results500_count1 = _context.GeoRadius<string>(key, coordMor, 500, Unit.Kilometers, 1).ToList();
            var results0 = _context.GeoRadius<string>(key, coordMor, 1, Unit.Kilometers).ToList();
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
}
