using Baubit.Caching;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace gRPC.Client
{
    public class OrderedCache<TId, TValue> : IOrderedCache<TId, TValue> where TId : struct, IComparable<TId>, IEquatable<TId>
    {
        private readonly GrpcChannel _channel;
        private readonly OrderedCacheService.OrderedCacheServiceClient _client;
        private readonly ISerializer _serializer;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedCache{TId, TValue}"/> class.
        /// </summary>
        /// <param name="channel">gRPC channel for communication with the server.</param>
        /// <param name="serializer">Serializer for MessagePack serialization.</param>
        public OrderedCache(GrpcChannel channel, ISerializer serializer)
        {
            _channel = channel ?? throw new ArgumentNullException(nameof(channel));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _client = new OrderedCacheService.OrderedCacheServiceClient(_channel);
        }

        /// <inheritdoc />
        public long Count
        {
            get
            {
                try
                {
                    var response = _client.GetCount(new EmptyRequest());
                    return response.Count;
                }
                catch
                {
                    return 0;
                }
            }
        }

        /// <inheritdoc />
        public bool Add(TValue value, out IEntry<TId, TValue> entry)
        {
            entry = null;
            try
            {
                // Serialize TValue to byte[]
                if (!_serializer.TrySerialize(value, out var serializedValue))
                {
                    return false;
                }

                var request = new AddRequest { Value = Google.Protobuf.ByteString.CopyFrom(serializedValue) };
                var response = _client.Add(request);

                if (!response.HasValue)
                {
                    return false;
                }

                entry = new Baubit.Caching.InMemory.Entry<TId, TValue>((TId)(object)response.Id, value) { CreatedOnUTC = new DateTime(response.CreatedOnUtcTicks, DateTimeKind.Utc) };
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc />
        public bool Clear()
        {
            try
            {
                var response = _client.Clear(new EmptyRequest());
                return response.Success;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_disposed)
            {
                _channel?.Dispose();
                _disposed = true;
            }
        }

        /// <inheritdoc />
        public IAsyncEnumerator<IEntry<TId, TValue>> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new GrpcAsyncEnumerator<TId, TValue>(_client.Enumerate(new EmptyRequest(), cancellationToken: cancellationToken),
                                                        _serializer,
                                                        cancellationToken);
        }

        /// <inheritdoc />
        public bool GetEntryOrDefault(TId? id, out IEntry<TId, TValue> entry)
        {
            entry = null;
            try
            {
                var request = new GetEntryRequest();
                if (id.HasValue)
                {
                    request.Id = (long)(object)id.Value;
                }

                var response = _client.GetEntry(request);

                if (!response.HasValue)
                {
                    return true; // Successful lookup, but entry not found
                }

                // Deserialize the Value from byte[] back to TValue
                if (!_serializer.TryDeserialize<TValue>(response.Value.ToByteArray(), out var value))
                {
                    return false;
                }

                entry = new Baubit.Caching.InMemory.Entry<TId, TValue>((TId)(object)response.Id, value) { CreatedOnUTC = new DateTime(response.CreatedOnUtcTicks, DateTimeKind.Utc) };
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc />
        public bool GetFirstIdOrDefault(out TId? id)
        {
            id = null;
            try
            {
                var response = _client.GetFirstId(new EmptyRequest());

                if (!response.HasValue)
                {
                    return true; // Successful lookup, but no entry
                }

                id = (TId)(object)response.Id;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc />
        public bool GetFirstOrDefault(out IEntry<TId, TValue> entry)
        {
            entry = null;
            try
            {
                var response = _client.GetFirst(new EmptyRequest());

                if (!response.HasValue)
                {
                    return true; // Successful lookup, but no entry
                }

                // Deserialize the Value from byte[] back to TValue
                if (!_serializer.TryDeserialize<TValue>(response.Value.ToByteArray(), out var value))
                {
                    return false;
                }

                entry = new Baubit.Caching.InMemory.Entry<TId, TValue>((TId)(object)response.Id, value) { CreatedOnUTC = new DateTime(response.CreatedOnUtcTicks, DateTimeKind.Utc) };

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc />
        public IAsyncEnumerator<IEntry<TId, TValue>> GetFutureAsyncEnumerator(string id = null, CancellationToken cancellationToken = default)
        {
            var request = new EnumerateFutureRequest();
            if (!string.IsNullOrEmpty(id))
            {
                request.Id = id;
            }
            return new GrpcAsyncEnumerator<TId, TValue>(_client.EnumerateFuture(request, cancellationToken: cancellationToken),
                                                        _serializer,
                                                        cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEntry<TId, TValue>> GetFutureFirstOrDefaultAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _client.GetFutureFirstAsync(new EmptyRequest(), cancellationToken: cancellationToken);

                if (!response.HasValue) return null;

                // Deserialize the Value from byte[] back to TValue
                if (!_serializer.TryDeserialize<TValue>(response.Value.ToByteArray(), out var value)) return null;


                return new Baubit.Caching.InMemory.Entry<TId, TValue>((TId)(object)response.Id, value) { CreatedOnUTC = new DateTime(response.CreatedOnUtcTicks, DateTimeKind.Utc) };
            }
            catch { return null; }
        }

        /// <inheritdoc />
        public bool GetLastIdOrDefault(out TId? id)
        {
            id = null;
            try
            {
                var response = _client.GetLastId(new EmptyRequest());

                if (!response.HasValue)
                {
                    return true; // Successful lookup, but no entry
                }

                id = (TId)(object)response.Id;
                return true;
            }
            catch { return false; }
        }

        /// <inheritdoc />
        public bool GetLastOrDefault(out IEntry<TId, TValue> entry)
        {
            entry = null;
            try
            {
                var response = _client.GetLast(new EmptyRequest());

                if (!response.HasValue)
                {
                    return true; // Successful lookup, but no entry
                }

                // Deserialize the Value from byte[] back to TValue
                if (!_serializer.TryDeserialize<TValue>(response.Value.ToByteArray(), out var value))
                {
                    return false;
                }

                entry = new Baubit.Caching.InMemory.Entry<TId, TValue>((TId)(object)response.Id, value) { CreatedOnUTC = new DateTime(response.CreatedOnUtcTicks, DateTimeKind.Utc) };

                return true;
            }
            catch { return false; }
        }

        /// <inheritdoc />
        public async Task<IEntry<TId, TValue>> GetNextAsync(TId? id = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new GetNextRequest();
                if (id.HasValue)
                {
                    request.Id = (long)(object)id.Value;
                }

                var response = await _client.GetNextAwaitingAsync(request, cancellationToken: cancellationToken);

                if (!response.HasValue)
                {
                    return null;
                }

                // Deserialize the Value from byte[] back to TValue
                if (!_serializer.TryDeserialize<TValue>(response.Value.ToByteArray(), out var value))
                {
                    return null;
                }

                return new Baubit.Caching.InMemory.Entry<TId, TValue>((TId)(object)response.Id, value) { CreatedOnUTC = new DateTime(response.CreatedOnUtcTicks, DateTimeKind.Utc) };
            }
            catch
            {
                return null;
            }
        }

        /// <inheritdoc />
        public bool GetNextOrDefault(TId? id, out IEntry<TId, TValue> entry)
        {
            entry = null;
            try
            {
                var request = new GetNextRequest();
                if (id.HasValue)
                {
                    request.Id = (long)(object)id.Value;
                }

                var response = _client.GetNext(request);

                if (!response.HasValue)
                {
                    return true; // Successful lookup, but no next entry
                }

                // Deserialize the Value from byte[] back to TValue
                if (!_serializer.TryDeserialize<TValue>(response.Value.ToByteArray(), out var value))
                {
                    return false;
                }

                entry = new Baubit.Caching.InMemory.Entry<TId, TValue>((TId)(object)response.Id, value) { CreatedOnUTC = new DateTime(response.CreatedOnUtcTicks, DateTimeKind.Utc) };

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc />
        public bool Remove(TId id, out IEntry<TId, TValue> entry)
        {
            entry = null;
            try
            {
                var request = new RemoveRequest { Id = (long)(object)id };
                var response = _client.Remove(request);

                if (!response.HasValue)
                {
                    return false;
                }

                // Deserialize the Value from byte[] back to TValue
                if (!_serializer.TryDeserialize<TValue>(response.Value.ToByteArray(), out var value))
                {
                    return false;
                }

                entry = new Baubit.Caching.InMemory.Entry<TId, TValue>((TId)(object)response.Id, value) { CreatedOnUTC = new DateTime(response.CreatedOnUtcTicks, DateTimeKind.Utc) };

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc />
        public bool Update(TId id, TValue value)
        {
            try
            {
                // Serialize TValue to byte[]
                if (!_serializer.TrySerialize(value, out var serializedValue))
                {
                    return false;
                }

                var request = new UpdateRequest
                {
                    Id = (long)(object)id,
                    Value = Google.Protobuf.ByteString.CopyFrom(serializedValue)
                };

                var response = _client.Update(request);
                return response.Success;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<(TId, T)> EnumerateAsync<T>([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default) where T : TValue
        {
            var enumerator = GetAsyncEnumerator(cancellationToken);
            await using (enumerator.ConfigureAwait(false))
            {
                while (await enumerator.MoveNextAsync().ConfigureAwait(false))
                {
                    var entry = enumerator.Current;
                    if (entry.Value is T typedValue)
                    {
                        yield return (entry.Id, typedValue);
                    }
                }
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<(TId, T)> EnumerateFutureAsync<T>([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default) where T : TValue
        {
            var enumerator = GetFutureAsyncEnumerator(null, cancellationToken);
            await using (enumerator.ConfigureAwait(false))
            {
                while (await enumerator.MoveNextAsync().ConfigureAwait(false))
                {
                    var entry = enumerator.Current;
                    if (entry.Value is T typedValue)
                    {
                        yield return (entry.Id, typedValue);
                    }
                }
            }
        }

        /// <inheritdoc />
        public async Task<bool> OnNextAsync<T>(Func<(TId, T), object, CancellationToken, Task<bool>> handler, object state = null, CancellationToken cancellationToken = default) where T : TValue
        {
            await foreach (var tuple in EnumerateFutureAsync<T>(cancellationToken))
            {
                await handler?.Invoke(tuple, state, cancellationToken);
            }
            return true;
        }
    }
}

