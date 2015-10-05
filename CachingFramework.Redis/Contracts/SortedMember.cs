namespace CachingFramework.Redis.Contracts
{
    /// <summary>
    /// Class SortedMember.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SortedMember<T>
    {
        /// <summary>
        /// Gets or sets the score.
        /// </summary>
        /// <value>The score.</value>
        public double Score { get; set; }
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public T Value { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="SortedMember{T}"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="score">The score.</param>
        public SortedMember(double score, T value)
        {
            Value = value;
            Score = score;
        }
    }
}