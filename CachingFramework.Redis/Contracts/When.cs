namespace CachingFramework.Redis.Contracts
{
    /// <summary>
    /// Indicates when this operation should be performed (only some variations are legal in a given context)
    /// </summary>
    public enum When
    {
        /// <summary>
        /// The operation should occur whether or not there is an existing value  
        /// </summary>
        Always = 0,
        /// <summary>
        /// The operation should only occur when there is an existing value
        /// </summary>
        Exists = 1,
        /// <summary>
        /// The operation should only occur when there is not an existing value
        /// </summary>
        NotExists = 2
    }
}
