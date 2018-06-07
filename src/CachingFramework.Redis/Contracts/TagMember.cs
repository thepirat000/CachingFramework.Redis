namespace CachingFramework.Redis.Contracts
{
    /// <summary>
    /// Represents a tagged element that could be a redis string, a hash field or a set member
    /// </summary>
    public class TagMember
    {
        private readonly ISerializer _serializer;

        public TagMember(ISerializer serializer)
        {
            _serializer = serializer;
        }

        /// <summary>
        /// The redis key pointed by this tag member
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// The redis member type pointed by this tag member
        /// </summary>
        public TagMemberType MemberType { get; set; }
        /// <summary>
        /// The redis member value pointed by this tag member.
        /// </summary>
        public byte[] MemberValue { get; set; }

        /// <summary>
        /// Returns the deserialized member value
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        public T GetMemberAs<T>()
        {
            if (MemberValue == null || _serializer == null)
            {
                return default(T);
            }
            return _serializer.Deserialize<T>(MemberValue);
        }
    }
}
