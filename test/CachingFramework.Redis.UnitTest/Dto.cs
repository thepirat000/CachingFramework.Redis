using MessagePack;
using System;
using System.Collections.Generic;

namespace CachingFramework.Redis.UnitTest
{
    // DTO dummy classes to be used in the tests
    public interface IDto
    {
    }
#if NET462 
    [Serializable]
#endif
    [MessagePackObject]
    public class User : IDto
    {
        [Key(1)]
        public int Id { get; set; }
        [Key(2)]
        public List<Department> Deparments { get; set; }
    }
#if NET462 
    [Serializable]
#endif
    [MessagePackObject]
    public class Department : IDto
    {
        [Key(1)]
        public int Id { get; set; }
        [Key(2)]
        public Location Location { get; set; }
        [Key(3)]
        public int Size { get; set; }
        [Key(4)]
        public decimal Distance { get; set; }
    }
#if NET462 
    [Serializable]
#endif
    [MessagePackObject]
    public class Location : IDto
    {
        [Key(1)]
        public int Id { get; set; }
        [Key(2)]
        public string Name { get; set; }
    }
#if NET462 
    [Serializable]
#endif
    [MessagePackObject]
    public class Jpeg
    {
        [Key(1)]
        public byte[] Data { get; set; }
    }
#if NET462 
    [Serializable]
#endif
    [MessagePackObject]
    public class DistributorInfo
    {
        [Key(1)]
        public DistributorInfo Parent { get; set; }
        [Key(2)]
        public string DistributorId { get; set; }
        [Key(3)]
        public string Subtype { get; set; }
        [Key(4)]
        public string FirstName { get; set; }
        [Key(5)]
        public string LastName { get; set; }
        [Key(6)]
        public string CompleteName { get { return LastName + ";" + FirstName; } }
    }
#if NET462 
    [Serializable]
#endif
    [MessagePackObject]
    public class DistributorCompReview
    {
        [Key(1)]
        public string DistributorId { get; set; }
        [Key(2)]
        public bool MatchingVolumePenalty { get; set; }
        [Key(3)]
        public bool RetiredSupervisor { get; set; }
    }
}
