using System.ComponentModel;

namespace CachingFramework.Redis.Contracts
{
    /// <summary>
    /// Distance unit
    /// </summary>
    public enum Unit
    {
        [Unit("m")] Meters = 0,
        [Unit("km")] Kilometers = 1,
        [Unit("mi")] Miles = 2,
        [Unit("ft")] Feet = 3
    }
}