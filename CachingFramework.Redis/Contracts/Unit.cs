using System.ComponentModel;

namespace CachingFramework.Redis.Contracts
{
    /// <summary>
    /// Distance unit
    /// </summary>
    public enum Unit
    {
        [Text("m")] Meters = 0,
        [Text("km")] Kilometers = 1,
        [Text("mi")] Miles = 2,
        [Text("ft")] Feet = 3
    }
}