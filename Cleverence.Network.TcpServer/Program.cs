using Cleverence.Network;
using System.Net;
using System.Net.Sockets;
using System.Text;

/// <summary>
/// Provides a TCP-based interactive server that exposes thread-safe counter operations
/// from the <see cref="Server"/> class over a simple text protocol.
/// </summary>
/// <remarks>
/// The server listens on <c>127.0.0.1:8080</c> and supports the following commands:
/// <list type="bullet">
///   <item><description><c>get</c> — returns the current counter value</description></item>
///   <item><description><c>add N</c> — adds integer <c>N</c> to the counter and returns the new value</description></item>
///   <item><description><c>quit</c> — closes the connection</description></item>
/// </list>
/// <para>
/// In <c>DEBUG</c> builds, artificial delays are introduced in counter operations
/// to facilitate concurrency testing and lock validation.
/// </para>
/// </remarks>
public class Program
{
#if DEBUG
    /// <summary>
    /// Artificial delay applied to counter operations in DEBUG builds to simulate
    /// slow critical sections and expose potential concurrency issues.
    /// </summary>
    private static readonly TimeSpan _delay = TimeSpan.FromMilliseconds(5000);
#endif

    /// <summary>
    /// Entry point of the TCP counter server application.
    /// Starts listening for incoming client connections on localhost:8080
    /// and spawns a background task to handle each client.
    /// </summary>
    /// <returns>A <see cref="Task"/> that never completes (server runs indefinitely).</returns>
    public static async Task Main()
    {
        var listener = new TcpListener(IPAddress.Loopback, 8080);
        listener.Start();

#if DEBUG
        Console.WriteLine($"Server started on port 8080 with delay {_delay.Milliseconds}ms. Waiting for clients...");
#else
        Console.WriteLine("Server started on port 8080. Waiting for clients...");
#endif

        while (true)
        {
            var client = await listener.AcceptTcpClientAsync();
            _ = Task.Run(() => HandleClient(client));
        }
    }

    /// <summary>
    /// Retrieves the current value of the shared counter.
    /// </summary>
    /// <returns>The current counter value.</returns>
    /// <remarks>
    /// In DEBUG builds, this method includes an artificial delay inside the read lock
    /// to help expose race conditions during testing.
    /// </remarks>
    private static int ServerGetCount()
    {
#if DEBUG
        return Server.GetCountDelay(_delay);
#else
        return Server.GetCount();
#endif
    }

    /// <summary>
    /// Atomically adds the specified value to the shared counter.
    /// </summary>
    /// <param name="value">The integer value to add (can be negative).</param>
    /// <remarks>
    /// In DEBUG builds, this method includes an artificial delay inside the write lock
    /// to simulate long-running write operations and validate lock fairness/exclusivity.
    /// </remarks>
    private static void ServerAddToCount(int value)
    {
#if DEBUG
        Server.AddToCountDelay(value, _delay);
#else
        Server.AddToCount(value);
#endif
    }

    /// <summary>
    /// Handles communication with a connected TCP client.
    /// Processes commands line-by-line and responds with counter values or error messages.
    /// </summary>
    /// <param name="client">The connected <see cref="TcpClient"/>.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous handling of the client.</returns>
    /// <exception cref="Exception">Propagates any I/O or protocol errors during communication.</exception>
    private static async Task HandleClient(TcpClient client)
    {
        using (client)
        using (var stream = client.GetStream())
        using (var reader = new StreamReader(stream, Encoding.UTF8))
        using (var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
        {
            await writer.WriteLineAsync("Connected to Counter Server. Type 'get', 'add N', or 'quit'");

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                var input = line.Trim();
                if (string.IsNullOrEmpty(input)) continue;

                if (input.Equals("quit", StringComparison.OrdinalIgnoreCase))
                {
                    await writer.WriteLineAsync("Bye!");
                    break;
                }

                if (input.Equals("get", StringComparison.OrdinalIgnoreCase))
                {
                    var count = ServerGetCount();
                    await writer.WriteLineAsync(count.ToString());
                }
                else if (input.StartsWith("add ", StringComparison.OrdinalIgnoreCase))
                {
                    var arg = input[4..].Trim();
                    if (int.TryParse(arg, out int value))
                    {
                        ServerAddToCount(value);
                        var newCount = ServerGetCount();
                        await writer.WriteLineAsync(newCount.ToString());
                    }
                    else
                    {
                        await writer.WriteLineAsync("ERROR: invalid number");
                    }
                }
                else
                {
                    await writer.WriteLineAsync("ERROR: unknown command. Use 'get', 'add N', or 'quit'");
                }
            }
        }
    }
}