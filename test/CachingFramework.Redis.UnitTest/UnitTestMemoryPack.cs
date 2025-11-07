#if !NET462
using System;

using MemoryPack;

using NUnit.Framework;

namespace CachingFramework.Redis.UnitTest
{
    [TestFixture]
    public class UnitTestMemoryPack
    {
        [Test]
        public void TestSerialization()
        {
            SUTContext context = new SUTContext();
            var person = new Person { Age = 18, Name = "Joe" };
            context.Cache.SetObject("JMem18", person, TimeSpan.FromSeconds(3));
            var cachePerson = context.Cache.GetObject<Person>("JMem18");
            Assert.AreEqual(person.Age, cachePerson.Age);
            Assert.AreEqual(person.Name, cachePerson.Name);
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
