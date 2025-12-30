using Baubit.Caching;
using Microsoft.Extensions.Hosting;

namespace ChatApp
{
    public class ChatClient : IHostedService
    {
        private IOrderedCache<long, ChatMessage> cache;
        private Task<bool>? listener;
        private Task<bool>? poster;
        private CancellationTokenSource? cts;
        public ChatClient(IOrderedCache<long, ChatMessage> cache)
        {
            this.cache = cache;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            Console.WriteLine("Enter name: ");
            var userName = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(userName))
            {
                Console.WriteLine("Name cannot be empty. Using 'Anonymous'.");
                userName = "Anonymous";
            }
            
            // userName is guaranteed to be non-null here
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
                if (str == null)
                {
                    // EOF or input closed - exit gracefully
                    break;
                }
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
                if (enumerator.Current.Value.UserName != currentUserName)
                {
                    Console.WriteLine($"[{enumerator.Current.Value.UserName}]: {enumerator.Current.Value.Message}");
                }
            }
            return true;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            cts?.Cancel();
            
            if (listener != null)
            {
                try
                {
                    await listener;
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation token is triggered
                }
            }
            
            if (poster != null)
            {
                try
                {
                    await poster;
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation token is triggered
                }
            }
        }
    }

    [MessagePack.MessagePackObject]
    public class ChatMessage
    {
        [MessagePack.Key(0)]
        public string UserName { get; set; } = "";
        [MessagePack.Key(1)]
        public string Message { get; set; } = "";
        [MessagePack.Key(2)]
        public DateTime Timestamp { get; set; }
    }
}
