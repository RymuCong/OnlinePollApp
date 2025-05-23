﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T3H.Poll.CrossCuttingConcerns.Cache.RedisCache
{
    public interface ISerializer
    {
        /// <summary>
        /// Serializes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        byte[] Serialize(object item);

        /// <summary>
        /// Serializes the asynchronous.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        Task<byte[]> SerializeAsync(object item);

        /// <summary>
        /// Deserializes the specified bytes.
        /// </summary>
        /// <param name="serializedObject">The serialized object.</param>
        /// <returns>
        /// The instance of the specified Item
        /// </returns>
        object Deserialize(byte[] serializedObject);

        /// <summary>
        /// Deserializes the specified bytes.
        /// </summary>
        /// <param name="serializedObject">The serialized object.</param>
        /// <returns>
        /// The instance of the specified Item
        /// </returns>
        Task<object> DeserializeAsync(byte[] serializedObject);

        /// <summary>
        /// Deserializes the specified bytes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializedObject">The serialized object.</param>
        /// <returns>
        /// The instance of the specified Item
        /// </returns>
        T Deserialize<T>(byte[] serializedObject) where T : class;

        /// <summary>
        /// Deserializes the specified bytes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializedObject">The serialized object.</param>
        /// <returns>
        /// The instance of the specified Item
        /// </returns>
        Task<T> DeserializeAsync<T>(byte[] serializedObject) where T : class;
    }
}
