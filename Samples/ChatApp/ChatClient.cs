using Baubit.Caching;
using Microsoft.Extensions.Hosting;

namespace ChatApp
{
    public class ChatClient : IHostedService
    {
        private IOrderedCache<long, ChatMessage> cache;
        private Task<bool> listener;
        private Task<bool> poster;
        private CancellationTokenSource cts;
        public ChatClient(IOrderedCache<long, ChatMessage> cache)
        {
            this.cache = cache;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            Console.WriteLine("Enter name: ");
            var userName = Console.ReadLine();
            listener = StartListeningAsync(userName, cts.Token);

            poster = Task.Run(() => RunPostingLoop(userName, cts.Token), cts.Token);

            return Task.CompletedTask;
        }

        private bool RunPostingLoop(string currentUserName, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine("Enter message: ");
                var str = Console.ReadLine();
                cache.Add(new ChatMessage { Message = str, UserName = currentUserName, Timestamp = DateTime.UtcNow }, out _);
            }
            return true;
        }

        private async Task<bool> StartListeningAsync(string currentUserName, 
                                                     CancellationToken cancellationToken)
        {
            var enumerator = cache.GetFutureAsyncEnumerator(cancellationToken);

            while (await enumerator.MoveNextAsync())
            {
                if (enumerator.Current.Value.UserName != currentUserName || true)
                {
                    Console.WriteLine(enumerator.Current.Value.Message);
                }
            }
            return true;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            cts.Cancel();
            await listener;
            await poster;
        }
    }

    public class ChatMessage
    {
        public string UserName { get; set; } = "";
        public string Message { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
}
