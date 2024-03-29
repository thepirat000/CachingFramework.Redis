﻿using System;
using StackExchange.Redis;

namespace CachingFramework.Redis.Contracts
{
    /// <summary>
    /// Interface that defines serialization/deserialization generic methods to be used by the cache engine
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns>System.String.</returns>
        RedisValue Serialize<T>(T value);
        /// <summary>
        /// Deserializes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        T Deserialize<T>(RedisValue value);
        /// <summary>
        /// The prefix for the keys that represents tags
        /// </summary>
        string TagPrefix { get; set; }
        /// <summary>
        /// The postfix for the keys that represents tags
        /// </summary>
        string TagPostfix { get; set; }
    }
}
