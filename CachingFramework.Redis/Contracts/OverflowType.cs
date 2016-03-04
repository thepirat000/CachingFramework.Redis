namespace CachingFramework.Redis.Contracts
{
    /// <summary>
    /// Enum OverflowType
    /// </summary>
    public enum OverflowType
    {
        /// <summary>
        /// Wrap around, this also works for signed integers, by wrapping towards the most negative or most positive values.
        /// </summary>
        [Text("wrap")]
        Wrap = 0,
        /// <summary>
        /// Saturation, so that overflowing in one direction or the other, will saturate the integer to its maximum value in the direction of the overflow.
        /// </summary>
        [Text("sat")]
        Saturation = 1,
        /// <summary>
        /// The operation is not performed if the value would overflow.
        /// </summary>
        [Text("fail")]
        Fail = 2
    }
}