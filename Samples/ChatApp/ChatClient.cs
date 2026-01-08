using Baubit.Caching;
using Microsoft.Extensions.Hosting;

namespace ChatApp
{
    /// <summary>
    /// A distributed chat client that demonstrates real-time messaging using Baubit.Caching
    /// with gRPC as the transport layer. Messages are synchronized across multiple chat clients
    /// connected to the same server.
    /// </summary>
    public class ChatClient : IHostedService
    {
        private readonly IOrderedCache<long, ChatMessage> cache;
        private Task<bool>? listener;
        private Task<bool>? poster;
        private CancellationTokenSource? cts;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatClient"/> class.
        /// </summary>
        /// <param name="cache">The distributed cache instance used for message synchronization.</param>
        public ChatClient(IOrderedCache<long, ChatMessage> cache)
        {
            this.cache = cache;
        }

        /// <summary>
        /// Starts the chat client by displaying the banner, prompting for username,
        /// and starting the listener and message posting tasks.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to stop the client.</param>
        /// <returns>A completed task.</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Display the banner
            DisplayBanner();

            Console.WriteLine("Enter your name: ");
            var userName = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(userName))
            {
                Console.WriteLine("Name cannot be empty. Using 'Anonymous'.");
                userName = "Anonymous";
            }

            Console.WriteLine($"\nWelcome, {userName}! You can start chatting now.");
            Console.WriteLine("Type your messages and press Enter to send.\n");
            
            // userName is guaranteed to be non-null here
            listener = StartListeningAsync(userName, cts.Token);
            poster = Task.Run(() => RunPostingLoop(userName, cts.Token), cts.Token);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Displays the Baubit Chat ASCII art banner.
        /// </summary>
        private void DisplayBanner()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                                                                  ║");
            Console.WriteLine("║ ██████╗  █████╗ ██╗   ██╗██████╗ ██╗████████╗   ██████╗██╗  ██╗ █████╗ ████████╗ ║");
            Console.WriteLine("║ ██╔══██╗██╔══██╗██║   ██║██╔══██╗██║╚══██╔══╝  ██╔════╝██║  ██║██╔══██╗╚══██╔══╝ ║");
            Console.WriteLine("║ ██████╔╝███████║██║   ██║██████╔╝██║   ██║     ██║     ███████║███████║   ██║    ║");
            Console.WriteLine("║ ██╔══██╗██╔══██║██║   ██║██╔══██╗██║   ██║     ██║     ██╔══██║██╔══██║   ██║    ║");
            Console.WriteLine("║ ██████╔╝██║  ██║╚██████╔╝██████╔╝██║   ██║     ╚██████╗██║  ██║██║  ██║   ██║    ║");
            Console.WriteLine("║ ╚═════╝ ╚═╝  ╚═╝ ╚═════╝ ╚═════╝ ╚═╝   ╚═╝      ╚═════╝╚═╝  ╚═╝╚═╝  ╚═╝   ╚═╝    ║");
            Console.WriteLine("║                                                                                  ║");
            Console.WriteLine("║                         via Baubit.Caching                                       ║");
            Console.WriteLine("║                                                                                  ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        /// <summary>
        /// Runs the message posting loop, continuously reading user input and adding messages
        /// to the distributed cache.
        /// </summary>
        /// <param name="currentUserName">The current user's name.</param>
        /// <param name="cancellationToken">Cancellation token to stop the loop.</param>
        /// <returns>True when the loop completes normally.</returns>
        private bool RunPostingLoop(string currentUserName, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var str = Console.ReadLine();
                if (str == null)
                {
                    // EOF or input closed - exit gracefully
                    break;
                }
                
                if (!string.IsNullOrWhiteSpace(str))
                {
                    cache.Add(new ChatMessage 
                    { 
                        Message = str, 
                        UserName = currentUserName, 
                        Timestamp = DateTime.UtcNow 
                    }, out _);
                }
            }
            return true;
        }

        /// <summary>
        /// Listens for incoming messages from other users via the distributed cache's
        /// future enumeration feature, which provides real-time message streaming.
        /// </summary>
        /// <param name="currentUserName">The current user's name (to filter out own messages).</param>
        /// <param name="cancellationToken">Cancellation token to stop listening.</param>
        /// <returns>True when listening completes normally.</returns>
        private async Task<bool> StartListeningAsync(string currentUserName, 
                                                     CancellationToken cancellationToken)
        {
            var enumerator = cache.GetFutureAsyncEnumerator(null, cancellationToken);

            while (await enumerator.MoveNextAsync())
            {
                // Display messages from other users
                if (enumerator.Current.Value.UserName != currentUserName)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[{enumerator.Current.Value.UserName}]: {enumerator.Current.Value.Message}");
                    Console.ResetColor();
                }
            }
            return true;
        }

        /// <summary>
        /// Stops the chat client gracefully by cancelling tasks and awaiting their completion.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token (unused).</param>
        /// <returns>A task representing the asynchronous stop operation.</returns>
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

    /// <summary>
    /// Represents a chat message with sender information and timestamp.
    /// Messages are serialized using MessagePack for efficient gRPC transport.
    /// </summary>
    [MessagePack.MessagePackObject]
    public class ChatMessage
    {
        /// <summary>
        /// Gets or sets the name of the user who sent the message.
        /// </summary>
        [MessagePack.Key(0)]
        public string UserName { get; set; } = "";

        /// <summary>
        /// Gets or sets the message content.
        /// </summary>
        [MessagePack.Key(1)]
        public string Message { get; set; } = "";

        /// <summary>
        /// Gets or sets the UTC timestamp when the message was created.
        /// </summary>
        [MessagePack.Key(2)]
        public DateTime Timestamp { get; set; }
    }
}
