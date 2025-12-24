#:package SSH.NET@*

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;


// USAGE: dotnet run <host> <port> <user> <password> <concurrent_requests>
// EXAMPLE: dotnet run localhost 2222 admin secret 50

// using var client2 = new SshClient("localhost", 23232, "backend", new PrivateKeyFile("./config/git-keys/backend_admin"));
// client2.Connect();


// if (args.Length < 4)
// {
//     Console.WriteLine("Usage: SshLoadTester <host> <port> <user> <pass> <concurrency>");
//     return;
// }

string host = "localhost";
int port = 23232;
string user = "backend";
int concurrency = 1000;

Console.WriteLine($"[INFO] Connecting to {host}:{port} as {user}...");

try
{
    // 1. Establish the main TCP connection (Simulates a user connecting)
    using var client = new SshClient("localhost", 23232, "backend", new PrivateKeyFile("./config/git-keys/backend_admin"));
    
    client.Connect();
    Console.WriteLine("[INFO] Connected. Starting bombardment...");

    var latencies = new ConcurrentBag<long>();
    var stopwatch = Stopwatch.StartNew();

    // 2. Bombard the server with multiplexed commands (Channels)
    // This tests how well your server handles concurrent channels over one socket.
    await Parallel.ForEachAsync(Enumerable.Range(0, concurrency), new ParallelOptions { MaxDegreeOfParallelism = concurrency }, async (i, token) =>
    {
        var sw = Stopwatch.StartNew();
        try
        {
            // CreateCommand opens a new 'exec' channel on the existing connection
            var cmd = client.CreateCommand("repo create icecream-" + Guid.NewGuid().ToString("N").Substring(0, 8)); 
            var result = await Task.Run(() => cmd.Execute(), token);
            
            sw.Stop();
            latencies.Add(sw.ElapsedMilliseconds);
            // Console.WriteLine(result.Trim());
            
            // Optional: Print progress for every 10th request
            if (i % 10 == 0) Console.Write(".");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERR] Request {i} failed: {ex.Message}");
        }
    });

    stopwatch.Stop();
    Console.WriteLine("\n\n------------------------------------------------");
    Console.WriteLine($"[REPORT] Bombardment Complete");
    Console.WriteLine($"Total Requests:   {latencies.Count}");
    Console.WriteLine($"Total Time:       {stopwatch.ElapsedMilliseconds} ms");
    Console.WriteLine($"Avg Latency:      {latencies.Average():F2} ms");
    Console.WriteLine($"Min Latency:      {latencies.Min()} ms");
    Console.WriteLine($"Max Latency:      {latencies.Max()} ms");
    Console.WriteLine($"Throughput:       {latencies.Count / stopwatch.Elapsed.TotalSeconds:F2} req/sec");
    Console.WriteLine("------------------------------------------------");

    client.Disconnect();
}
catch (Exception ex)
{
    Console.WriteLine($"[CRITICAL] Connection failed: {ex.Message}");
}