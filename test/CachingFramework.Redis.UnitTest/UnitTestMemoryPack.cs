#if !NET462
using MemoryPack;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace CachingFramework.Redis.UnitTest
{
    [TestFixture]
    public class UnitTestMemoryPack
    {
        [Test]
        public void TestSerialization()
        {
            var key = Common.GetUId();
            SUTContext context = new SUTContext();
            var person = new Person { Age = 18, Name = "Joe" };
            context.Cache.SetObject(key, person, TimeSpan.FromSeconds(3));
            var cachePerson = context.Cache.GetObject<Person>(key);
            Assert.That(cachePerson.Age, Is.EqualTo(person.Age));
            Assert.That(cachePerson.Name, Is.EqualTo(person.Name));
        }
    }
    [MemoryPackable(SerializeLayout.Explicit)]
    public partial class Person
    {
        [MemoryPackOrder(0)]
        public int Age { get; set; }
        [MemoryPackOrder(1)]
        public string Name { get; set; }
    }
    public class SUTContext : Redis.RedisContext
    {
        public SUTContext()
            : base("localhost:6379", new MemoryPack.MemoryPackSerializer())
        {
        }
        public SUTContext(string configuration)
            : base(configuration, new MemoryPack.MemoryPackSerializer())
        { }
    }
}
#endif
