using System;
using System.Collections.Generic;

namespace CachingFramework.Redis.UnitTest
{
    // DTO dummy classes to be used in the tests
    public interface IDto
    {
    }
    [Serializable]
    public class User : IDto
    {
        public int Id { get; set; }
        public List<Department> Deparments { get; set; }
    }
    [Serializable]
    public class Department : IDto
    {
        public int Id { get; set; }
        public Location Location { get; set; }
        public int Size { get; set; }
        public decimal Distance { get; set; }
    }
    [Serializable]
    public class Location : IDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    [Serializable]
    public class Jpeg
    {
        public byte[] Data { get; set; }
    }
    [Serializable]
    public class DistributorInfo
    {
        public DistributorInfo Parent { get; set; }
        public string DistributorId { get; set; }
        public string Subtype { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompleteName { get { return LastName + ";" + FirstName; } }
    }
    [Serializable]
    public class DistributorCompReview
    {
        public string DistributorId { get; set; }
        public bool MatchingVolumePenalty { get; set; }
        public bool RetiredSupervisor { get; set; }
    }
}
