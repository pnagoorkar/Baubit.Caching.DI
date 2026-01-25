using Baubit.Caching;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace gRPC.Server
{
    public class OrderedCacheService : gRPC.OrderedCacheService.OrderedCacheServiceBase
    {
        private readonly IOrderedCache<long, byte[]> _cache;
        private readonly ISerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedCacheServiceImpl"/> class.
        /// </summary>
        /// <param name="cache">The underlying cache instance.</param>
        /// <param name="serializer">Serializer for MessagePack operations.</param>
        public OrderedCacheService(IOrderedCache<long, byte[]> cache, ISerializer serializer)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        public override async Task<EntryResponse> Add(AddRequest request, ServerCallContext context)
        {
            var success = _cache.Add(request.Value.ToByteArray(), out var entry);

            if (!success || entry == null) return new EntryResponse { HasValue = false };

            return new EntryResponse
            {
                Id = entry.Id,
                CreatedOnUtcTicks = entry.CreatedOnUTC.Ticks,
                Value = Google.Protobuf.ByteString.CopyFrom(entry.Value),
                HasValue = true
            };
        }

        public override Task<BoolResponse> Update(UpdateRequest request, ServerCallContext context)
        {
            var success = _cache.Update(request.Id, request.Value.ToByteArray());
            return Task.FromResult(new BoolResponse { Success = success });
        }

        public override Task<EntryResponse> Remove(RemoveRequest request, ServerCallContext context)
        {
            var success = _cache.Remove(request.Id, out var entry);

            if (!success || entry == null) return Task.FromResult(new EntryResponse { HasValue = false });

            return Task.FromResult(new EntryResponse
            {
                Id = entry.Id,
                CreatedOnUtcTicks = entry.CreatedOnUTC.Ticks,
                Value = Google.Protobuf.ByteString.CopyFrom(entry.Value),
                HasValue = true
            });
        }

        public override Task<BoolResponse> Clear(EmptyRequest request, ServerCallContext context)
        {
            var success = _cache.Clear();
            return Task.FromResult(new BoolResponse { Success = success });
        }

        public override Task<CountResponse> GetCount(EmptyRequest request, ServerCallContext context)
        {
            return Task.FromResult(new CountResponse { Count = _cache.Count });
        }

        public override Task<EntryResponse> GetEntry(GetEntryRequest request, ServerCallContext context)
        {
            long? id = request.HasId ? request.Id : null;
            var success = _cache.GetEntryOrDefault(id, out var entry);

            if (!success)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Failed to get entry"));
            }

            if (entry == null)
            {
                return Task.FromResult(new EntryResponse { HasValue = false });
            }

            return Task.FromResult(new EntryResponse
            {
                Id = entry.Id,
                CreatedOnUtcTicks = entry.CreatedOnUTC.Ticks,
                Value = Google.Protobuf.ByteString.CopyFrom(entry.Value),
                HasValue = true
            });
        }

        public override Task<EntryResponse> GetFirst(EmptyRequest request, ServerCallContext context)
        {
            var success = _cache.GetFirstOrDefault(out var entry);

            if (!success)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Failed to get first entry"));
            }

            if (entry == null)
            {
                return Task.FromResult(new EntryResponse { HasValue = false });
            }

            return Task.FromResult(new EntryResponse
            {
                Id = entry.Id,
                CreatedOnUtcTicks = entry.CreatedOnUTC.Ticks,
                Value = Google.Protobuf.ByteString.CopyFrom(entry.Value),
                HasValue = true
            });
        }

        public override Task<IdResponse> GetFirstId(EmptyRequest request, ServerCallContext context)
        {
            var success = _cache.GetFirstIdOrDefault(out var id);

            if (!success)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Failed to get first ID"));
            }

            return Task.FromResult(new IdResponse
            {
                Id = id ?? 0,
                HasValue = id.HasValue
            });
        }

        public override Task<EntryResponse> GetLast(EmptyRequest request, ServerCallContext context)
        {
            var success = _cache.GetLastOrDefault(out var entry);

            if (!success)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Failed to get last entry"));
            }

            if (entry == null)
            {
                return Task.FromResult(new EntryResponse { HasValue = false });
            }

            return Task.FromResult(new EntryResponse
            {
                Id = entry.Id,
                CreatedOnUtcTicks = entry.CreatedOnUTC.Ticks,
                Value = Google.Protobuf.ByteString.CopyFrom(entry.Value),
                HasValue = true
            });
        }

        public override Task<IdResponse> GetLastId(EmptyRequest request, ServerCallContext context)
        {
            var success = _cache.GetLastIdOrDefault(out var id);

            if (!success)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Failed to get last ID"));
            }

            return Task.FromResult(new IdResponse
            {
                Id = id ?? 0,
                HasValue = id.HasValue
            });
        }

        public override Task<EntryResponse> GetNext(GetNextRequest request, ServerCallContext context)
        {
            long? id = request.HasId ? request.Id : null;
            var success = _cache.GetNextOrDefault(id, out var entry);

            if (!success)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Failed to get next entry"));
            }

            if (entry == null)
            {
                return Task.FromResult(new EntryResponse { HasValue = false });
            }

            return Task.FromResult(new EntryResponse
            {
                Id = entry.Id,
                CreatedOnUtcTicks = entry.CreatedOnUTC.Ticks,
                Value = Google.Protobuf.ByteString.CopyFrom(entry.Value),
                HasValue = true
            });
        }

        public override async Task<EntryResponse> GetNextAwaiting(GetNextRequest request, ServerCallContext context)
        {
            long? id = request.HasId ? request.Id : null;
            var entry = await _cache.GetNextAsync(id, context.CancellationToken);

            if (entry == null)
            {
                return new EntryResponse { HasValue = false };
            }

            return new EntryResponse
            {
                Id = entry.Id,
                CreatedOnUtcTicks = entry.CreatedOnUTC.Ticks,
                Value = Google.Protobuf.ByteString.CopyFrom(entry.Value),
                HasValue = true
            };
        }

        public override async Task<EntryResponse> GetFutureFirst(EmptyRequest request, ServerCallContext context)
        {
            var entry = await _cache.GetFutureFirstOrDefaultAsync(context.CancellationToken);

            if (entry == null)
            {
                return new EntryResponse { HasValue = false };
            }

            return new EntryResponse
            {
                Id = entry.Id,
                CreatedOnUtcTicks = entry.CreatedOnUTC.Ticks,
                Value = Google.Protobuf.ByteString.CopyFrom(entry.Value),
                HasValue = true
            };
        }

        public override async Task Enumerate(EmptyRequest request, IServerStreamWriter<EntryResponse> responseStream, ServerCallContext context)
        {
            // Collect all entries first to avoid issues with streaming from the underlying cache
            var entries = new List<IEntry<long, byte[]>>();

            // Use a separate cancellation token source to ensure we complete enumeration
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                var enumerator = _cache.GetAsyncEnumerator(cts.Token);
                try
                {
                    while (await enumerator.MoveNextAsync().ConfigureAwait(false))
                    {
                        entries.Add(enumerator.Current);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Timeout - return what we have
                }
                finally
                {
                    await enumerator.DisposeAsync().ConfigureAwait(false);
                }
            }

            // Now stream the collected entries to the client
            foreach (var entry in entries)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var response = new EntryResponse
                {
                    Id = entry.Id,
                    CreatedOnUtcTicks = entry.CreatedOnUTC.Ticks,
                    Value = Google.Protobuf.ByteString.CopyFrom(entry.Value),
                    HasValue = true
                };

                await responseStream.WriteAsync(response).ConfigureAwait(false);
            }
        }

        public override async Task EnumerateFuture(EnumerateFutureRequest request, IServerStreamWriter<EntryResponse> responseStream, ServerCallContext context)
        {
            string id = request.HasId ? request.Id : null;
            var enumerator = _cache.GetFutureAsyncEnumerator(id, context.CancellationToken);
            try
            {
                while (await enumerator.MoveNextAsync())
                {
                    if (context.CancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    var entry = enumerator.Current;
                    var response = new EntryResponse
                    {
                        Id = entry.Id,
                        CreatedOnUtcTicks = entry.CreatedOnUTC.Ticks,
                        Value = Google.Protobuf.ByteString.CopyFrom(entry.Value),
                        HasValue = true
                    };

                    await responseStream.WriteAsync(response);
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }
        }
    }
}
