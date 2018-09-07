using System;
using System.Diagnostics;

namespace CachingFramework.Redis.Contracts
{
    /// <summary>
    /// Class representing a position in Latitude/Longitude terms.
    /// </summary>
#if (NET461)
    [Serializable]
#endif
    public class GeoCoordinate
    {
        /// <summary>
        /// Gets the latitude.
        /// </summary>
        public double Latitude { get; private set; }
        /// <summary>
        /// Gets the longitude.
        /// </summary>
        public double Longitude { get; private set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoCoordinate"/> class.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        public GeoCoordinate(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            return $"{Latitude},{Longitude}";
        }
    }
}