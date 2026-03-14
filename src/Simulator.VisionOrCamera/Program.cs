using System.Net;
using System.Net.Sockets;
using System.Text;

const int Port = 5002;
const int ItemsPerMinute = 30;
const double FailureRate = 0.30;

int totalInspected = 0;
int totalPassed = 0;
int totalFailed = 0;

var consoleLock = new object();
var cts = new CancellationTokenSource();
var random = new Random();

string[] RealisticFailureReasons =
[
    "Ink smear - reverse face",
    "Misaligned serial print",
    "Incomplete security thread",
    "Substrate defect detected",
    "Serial number font deviation",
    "Ink void - portrait area",
    "Overprint registration error",
    "UV feature absent"
];

PrintBanner();

var listener = new TcpListener(IPAddress.Any, Port);
listener.Start();
Log($"Listening on port {Port}");
PrintSeparator();

_ = Task.Run(AcceptClientsAsync, cts.Token);

_ = Task.Run(PeriodicInspectionTriggerAsync, cts.Token);

await WaitForExitAsync();

/// <summary>
/// Fires a periodic trigger at the configured inspection rate to pace simulated throughput.
/// </summary>
async Task PeriodicInspectionTriggerAsync()
{
    while (!cts.IsCancellationRequested)
    {
        var intervalMs = (int)(60_000.0 / Math.Max(1, ItemsPerMinute));
        await Task.Delay(intervalMs, cts.Token).ContinueWith(_ => { });
    }
}

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
/// Handles inspection requests from a single connected client, returning PASS or FAIL responses.
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
            {
                break;
            }

            var serial = line.Trim();
            var isFail = random.NextDouble() < FailureRate;

            string response;
            if (isFail)
            {
                var reason = RealisticFailureReasons[random.Next(RealisticFailureReasons.Length)];
                response = $"FAIL:{reason}";
                Interlocked.Increment(ref totalFailed);
                Log($"  {serial,-24}  FAIL  {reason}");
            }
            else
            {
                response = "PASS";
                Interlocked.Increment(ref totalPassed);
                Log($"  {serial,-24}  PASS");
            }

            Interlocked.Increment(ref totalInspected);
            await writer.WriteLineAsync(response);
        }
    }
    catch (OperationCanceledException) { }
    catch (Exception ex) { 
        Log($"  [!] Client {endpoint} error: {ex.Message}"); 
    }
    finally { 
        Log($"[-] Client disconnected: {endpoint}"); tcp.Dispose(); 
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
/// Writes a timestamped message to the console with current inspection statistics.
/// </summary>
void Log(string message)
{
    lock (consoleLock)
    {
        var stats = totalInspected > 0
            ? $"  [inspected:{totalInspected} pass:{totalPassed} fail:{totalFailed} rate:{FailureRate:P0}]"
            : string.Empty;
        Console.WriteLine($"  {DateTime.Now:HH:mm:ss.fff}  {message}{stats}");
    }
}

/// <summary>
/// Prints the simulator banner header to the console.
/// </summary>
void PrintBanner()
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine();
    Console.WriteLine("  ╔══════════════════════════════════════════════════════╗");
    Console.WriteLine("  ║        VISION / CAMERA SIMULATOR  -  TCP :5002       ║");
    Console.WriteLine("  ║    Simulates a machine-vision inspection system      ║");
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