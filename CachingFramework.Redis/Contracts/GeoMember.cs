namespace CachingFramework.Redis.Contracts
{
    /// <summary>
    /// Represents a member of type T with geospatial information (coordinates and distance)
    /// </summary>
    /// <typeparam name="T">The member type.</typeparam>
    public class GeoMember<T>
    {
        /// <summary>
        /// Gets or sets the distance to center.
        /// </summary>
        /// <value>The distance to center.</value>
        public double DistanceToCenter { get; set; }
        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>The position.</value>
        public GeoCoordinate Position { get; set; }
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public T Value { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoMember{T}"/> class.
        /// </summary>
        /// <param name="coordinate">The coordinate.</param>
        /// <param name="value">The value.</param>
        public GeoMember(GeoCoordinate coordinate, T value)
            : this(coordinate.Latitude, coordinate.Longitude, value, -1)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoMember{T}"/> class.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="value">The value.</param>
        public GeoMember(double latitude, double longitude, T value)
            :this(latitude, longitude, value, -1)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoMember{T}"/> class.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="value">The value.</param>
        /// <param name="distance">The distance.</param>
        public GeoMember(double latitude, double longitude, T value, double distance)
        {
            Value = value;
            Position = new GeoCoordinate(latitude, longitude);
            DistanceToCenter = distance;
        }
    }
}