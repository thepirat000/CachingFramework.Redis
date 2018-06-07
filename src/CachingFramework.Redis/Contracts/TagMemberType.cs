namespace CachingFramework.Redis.Contracts
{
    /// <summary>
    /// Tag member type
    /// </summary>
    public enum TagMemberType
    {
        /// <summary>Member of type Redis String (single key)</summary>
        StringKey = 0,
        /// <summary>Member of type Redis Hash (single hash field)</summary>
        HashField = 1,
        /// <summary>Member of type Redis Set (single set member)</summary>
        SetMember = 2,
        /// <summary>Member of type Redis SortedSet (single sorted set member)</summary>
        SortedSetMember = 3
    }
}
