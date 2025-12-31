using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace gRPC.Client.DI
{
    public class Module<TValue> : Baubit.Caching.DI.Module<long, TValue, Configuration>
    {
        public Module(IConfiguration configuration) : base(configuration)
        {
        }

        public Module(Configuration configuration, List<Baubit.DI.IModule> nestedModules = null) : base(configuration, nestedModules)
        {
        }

        protected override Baubit.Caching.IOrderedCache<long, TValue> BuildOrderedCache(IServiceProvider serviceProvider)
        {
            var channel = GrpcChannel.ForAddress(Configuration.GrpcChannelAddress);
            var options = MessagePack.Resolvers.ContractlessStandardResolver.Options;
            var serializer = new Serializer(options);
            return new OrderedCache<long, TValue>(channel, serializer);
        }

        protected override Baubit.Caching.IStore<long, TValue> BuildL1DataStore(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException(); // Not required for this implementation
        }

        protected override Baubit.Caching.IStore<long, TValue> BuildL2DataStore(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException(); // Not required for this implementation
        }

        protected override Baubit.Caching.IMetadata<long> BuildMetadata(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException(); // Not required for this implementation
        }
    }
}
