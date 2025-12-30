using MessagePack;
using System;
using System.Threading;

namespace gRPC
{
    /// <summary>
    /// MessagePack-based serializer implementation.
    /// Uses typeless serialization to support arbitrary object types.
    /// Also provides contractless serialization for DTOs.
    /// </summary>
    public class Serializer : ISerializer
    {
        private MessagePackSerializerOptions _messagePackSerializerOptions;
        private static readonly MessagePackSerializerOptions ContractlessOptions = global::MessagePack.Resolvers.ContractlessStandardResolver.Options;

        /// <summary>
        /// Initializes a new instance of the <see cref="Serializer"/> class.
        /// </summary>
        /// <param name="messagePackSerializerOptions">The MessagePack serializer options to use.</param>
        public Serializer(MessagePackSerializerOptions messagePackSerializerOptions)
        {
            _messagePackSerializerOptions = messagePackSerializerOptions;
        }

        /// <summary>
        /// Attempts to deserialize a byte array to an object using MessagePack.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="bytes">The byte array to deserialize.</param>
        /// <param name="obj">When this method returns, contains the deserialized object if successful.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>
        public bool TryDeserialize<T>(byte[] bytes, out T obj, CancellationToken cancellationToken = default)
        {
            try
            {
                obj = (T)MessagePackSerializer.Typeless.Deserialize(bytes, cancellationToken: cancellationToken)!;
            }
            catch
            {
                obj = default!;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Attempts to serialize an object to a byte array using MessagePack.
        /// </summary>
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        /// <param name="value">The object to serialize.</param>
        /// <param name="serializedValue">When this method returns, contains the serialized bytes if successful.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>True if serialization succeeded; otherwise, false.</returns>
        public bool TrySerialize<T>(T value, out byte[] serializedValue, CancellationToken cancellationToken = default)
        {
            try
            {
                serializedValue = MessagePackSerializer.Typeless.Serialize(value, cancellationToken: cancellationToken);
            }
            catch
            {
                serializedValue = Array.Empty<byte>();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Serializes a DTO object using contractless serialization.
        /// Used for HTTP DTOs that don't have MessagePack attributes.
        /// </summary>
        /// <typeparam name="T">The type of DTO to serialize.</typeparam>
        /// <param name="dto">The DTO to serialize.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>Serialized bytes, or empty array on failure.</returns>
        public byte[] SerializeDto<T>(T dto, CancellationToken cancellationToken = default)
        {
            try
            {
                return MessagePackSerializer.Serialize(dto, ContractlessOptions, cancellationToken);
            }
            catch
            {
                return Array.Empty<byte>();
            }
        }

        /// <summary>
        /// Deserializes a DTO object using contractless serialization.
        /// Used for HTTP DTOs that don't have MessagePack attributes.
        /// </summary>
        /// <typeparam name="T">The type of DTO to deserialize to.</typeparam>
        /// <param name="bytes">The byte array to deserialize.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>Deserialized DTO, or default value on failure.</returns>
        public T DeserializeDto<T>(byte[] bytes, CancellationToken cancellationToken = default)
        {
            try
            {
                return MessagePackSerializer.Deserialize<T>(bytes, ContractlessOptions, cancellationToken);
            }
            catch
            {
                return default!;
            }
        }
    }
}
