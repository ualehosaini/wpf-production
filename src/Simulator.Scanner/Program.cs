using System.Net;
using System.Net.Sockets;
using System.Text;

const int Port = 5001;
const double FailureRate = 0.10;

int totalScanned = 0;
int goodReads = 0;
int badReads = 0;
int counter = 0;

var consoleLock = new object();
var cts = new CancellationTokenSource();
var random = new Random();

PrintBanner();

var listener = new TcpListener(IPAddress.Any, Port);
listener.Start();
Log($"Listening on port {Port}");
PrintSeparator();

_ = Task.Run(AcceptClientsAsync, cts.Token);

await WaitForExitAsync();

listener.Stop();
Console.WriteLine("\nShutdown complete.");

/// <summary>
/// Continuously accepts incoming TCP client connections and spawns a handler task for each.
/// </summary>
async Task AcceptClientsAsync()
{
    while (!cts.IsCancellationRequested)
    {
        try
        {
            var tcp = await listener.AcceptTcpClientAsync(cts.Token);
            tcp.NoDelay = true;
            var endpoint = tcp.Client.RemoteEndPoint?.ToString() ?? "unknown";
            Log($"[+] Client connected: {endpoint}");
            _ = Task.Run(() => HandleClientAsync(tcp, endpoint), cts.Token);
        }
        catch (OperationCanceledException) { break; }
        catch (SocketException) { break; }
    }
}

/// <summary>
/// Handles all SCAN requests from a single connected client, returning barcode strings or READ_ERROR responses.
/// </summary>
async Task HandleClientAsync(TcpClient tcp, string endpoint)
{
    try
    {
        await using var stream = tcp.GetStream();
        using var reader = new StreamReader(stream, new UTF8Encoding(false), leaveOpen: true);
        await using var writer = new StreamWriter(stream, new UTF8Encoding(false), leaveOpen: true)
        {
            AutoFlush = true
        };

        while (!cts.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cts.Token);

            if (IsClientDisconnected(line))
                break;

            var seq = Interlocked.Increment(ref counter);
            var isBadRead = random.NextDouble() < FailureRate;

            string response;
            if (isBadRead)
            {
                response = "READ_ERROR";
                Interlocked.Increment(ref badReads);
                Log($"  SCAN #{seq,-6}  READ_ERROR");
            }
            else
            {
                response = $"BN{DateTime.UtcNow:yyyyMMdd}-{seq:D6}";
                Interlocked.Increment(ref goodReads);
                Log($"  SCAN #{seq,-6}  {response}");
            }

            Interlocked.Increment(ref totalScanned);
            await writer.WriteLineAsync(response);
        }
    }
    catch (OperationCanceledException) { }
    catch (Exception ex)
    {
        Log($"  [!] Client {endpoint} error: {ex.Message}");
    }
    finally
    {
        Log($"[-] Client disconnected: {endpoint}");
        tcp.Dispose();
    }

    static bool IsClientDisconnected(string? line) => line is null;
}

/// <summary>
/// Blocks until the user presses Q, then signals the cancellation token to stop all tasks.
/// </summary>
async Task WaitForExitAsync()
{
    Log("Press [Q] to quit.");
    while (!cts.IsCancellationRequested)
    {
        if (Console.KeyAvailable)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Q)
            {
                cts.Cancel();
                return;
            }
        }
        await Task.Delay(50);
    }
}

/// <summary>
/// Writes a timestamped message to the console with current scan statistics.
/// </summary>
void Log(string message)
{
    lock (consoleLock)
    {
        var stats = totalScanned > 0
            ? $"  [scanned:{totalScanned} good:{goodReads} error:{badReads} rate:{FailureRate:P0}]"
            : string.Empty;
        Console.WriteLine($"  {DateTime.Now:HH:mm:ss.fff}  {message}{stats}");
    }
}

/// <summary>
/// Prints the simulator banner header to the console.
/// </summary>
void PrintBanner()
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine();
    Console.WriteLine("  ╔══════════════════════════════════════════════════════╗");
    Console.WriteLine("  ║          SCANNER SIMULATOR  -  TCP :5001             ║");
    Console.WriteLine("  ║    Simulates a barcode / RFID scanner device         ║");
    Console.WriteLine("  ╚══════════════════════════════════════════════════════╝");
    Console.ResetColor();
    Console.WriteLine();
}

/// <summary>
/// Prints a horizontal separator line to the console.
/// </summary>
void PrintSeparator()
{
    lock (consoleLock)
        Console.WriteLine("  " + new string('─', 60));
}
