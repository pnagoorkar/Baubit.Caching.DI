using System.Threading;

namespace gRPC
{
    /// <summary>
    /// Defines a contract for serializing and deserializing objects to/from byte arrays.
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Attempts to serialize an object to a byte array.
        /// </summary>
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        /// <param name="value">The object to serialize.</param>
        /// <param name="serializedValue">When this method returns, contains the serialized bytes if successful.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>True if serialization succeeded; otherwise, false.</returns>
        public bool TrySerialize<T>(T value, out byte[] serializedValue, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to deserialize a byte array to an object.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="bytes">The byte array to deserialize.</param>
        /// <param name="obj">When this method returns, contains the deserialized object if successful.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>
        public bool TryDeserialize<T>(byte[] bytes, out T obj, CancellationToken cancellationToken = default);

        /// <summary>
        /// Serializes a DTO object using contractless serialization.
        /// Used for HTTP DTOs that don't have MessagePack attributes.
        /// </summary>
        /// <typeparam name="T">The type of DTO to serialize.</typeparam>
        /// <param name="dto">The DTO to serialize.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>Serialized bytes, or empty array on failure.</returns>
        public byte[] SerializeDto<T>(T dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deserializes a DTO object using contractless serialization.
        /// Used for HTTP DTOs that don't have MessagePack attributes.
        /// </summary>
        /// <typeparam name="T">The type of DTO to deserialize to.</typeparam>
        /// <param name="bytes">The byte array to deserialize.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>Deserialized DTO, or default value on failure.</returns>
        public T DeserializeDto<T>(byte[] bytes, CancellationToken cancellationToken = default);
    }
}
