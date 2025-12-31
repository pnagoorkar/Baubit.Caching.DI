namespace gRPC.Client.DI
{
    public class Configuration : Baubit.Caching.DI.Configuration
    {
        public string GrpcChannelAddress { get; set; } = "http://localhost:49971";
    }
}
