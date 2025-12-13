using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// A simple interactive TCP client for the Counter Server.
/// </summary>
/// <remarks>
/// Connects to the Counter Server running on <c>127.0.0.1:8080</c> and allows the user
/// to interactively send commands such as:
/// <list type="bullet">
///   <item><description><c>get</c> — retrieve the current counter value</description></item>
///   <item><description><c>add N</c> — add an integer <c>N</c> to the counter and receive the updated value</description></item>
///   <item><description><c>quit</c> — close the connection</description></item>
/// </list>
/// <para>
/// The client reads server responses line by line and displays them to the user.
/// It terminates gracefully when the server sends the "Bye!" message.
/// </para>
/// </remarks>
public class Program
{
    /// <summary>
    /// Entry point of the TCP counter client application.
    /// Establishes a connection to the server, displays the welcome message,
    /// and enters an interactive loop to read user input and display server responses.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous execution of the client.</returns>
    /// <exception cref="Exception">
    /// Catches and displays any exception that occurs during connection or communication
    /// (e.g., server not running, network error, etc.).
    /// </exception>
    public static async Task Main()
    {
        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", 8080);
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            // Display server welcome message
            Console.WriteLine(await reader.ReadLineAsync());

            while (true)
            {
                Console.Write("> ");
                var input = Console.ReadLine();
                if (string.IsNullOrEmpty(input)) continue;

                await writer.WriteLineAsync(input);

                var response = await reader.ReadLineAsync();
                if (response == "Bye!")
                {
                    Console.WriteLine(response);
                    break;
                }

                Console.WriteLine(response);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}