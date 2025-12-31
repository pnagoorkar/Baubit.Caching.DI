
using Baubit.Caching;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace gRPC.Client
{
    /// <summary>
    /// Async enumerator for gRPC server streaming responses.
    /// Reads EntryResponse messages from gRPC stream and converts them to IEntry instances.
    /// </summary>
    /// <typeparam name="TId">The type of the entry identifier.</typeparam>
    /// <typeparam name="TValue">The type of values stored in the cache.</typeparam>
    internal class GrpcAsyncEnumerator<TId, TValue> : IAsyncEnumerator<IEntry<TId, TValue>> where TId : struct, IComparable<TId>, IEquatable<TId>
    {
        private readonly AsyncServerStreamingCall<EntryResponse> _streamingCall;
        private readonly ISerializer _serializer;
        private readonly CancellationToken _cancellationToken;
        private IEntry<TId, TValue> _current;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrpcAsyncEnumerator{TId, TValue}"/> class.
        /// </summary>
        /// <param name="streamingCall">The gRPC server streaming call.</param>
        /// <param name="serializer">Serializer for deserializing values.</param>
        /// <param name="cancellationToken">Cancellation token for the enumeration.</param>
        public GrpcAsyncEnumerator(AsyncServerStreamingCall<EntryResponse> streamingCall,
                                   ISerializer serializer,
                                   CancellationToken cancellationToken)
        {
            _streamingCall = streamingCall ?? throw new ArgumentNullException(nameof(streamingCall));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _cancellationToken = cancellationToken;
        }

        /// <inheritdoc />
        public IEntry<TId, TValue> Current => _current;

        /// <inheritdoc />
        public async ValueTask<bool> MoveNextAsync()
        {
            try
            {
                if (_disposed || _cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                // Read next entry from gRPC stream
                if (!await _streamingCall.ResponseStream.MoveNext(_cancellationToken))
                {
                    return false;
                }

                var response = _streamingCall.ResponseStream.Current;

                if (!response.HasValue)
                {
                    return false;
                }

                // Deserialize the Value from byte[] back to TValue
                if (!_serializer.TryDeserialize<TValue>(response.Value.ToByteArray(), out var value))
                {
                    return false;
                }

                _current = new Baubit.Caching.InMemory.Entry<TId, TValue>((TId)(object)response.Id, value) { CreatedOnUTC = new DateTime(response.CreatedOnUtcTicks, DateTimeKind.Utc) };

                return true;
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                // Client cancelled the operation
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                _streamingCall?.Dispose();
                _disposed = true;
            }
            return default;
        }
    }
}
