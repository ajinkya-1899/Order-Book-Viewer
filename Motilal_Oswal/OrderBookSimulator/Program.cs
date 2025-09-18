using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

public class OrderBookSimulator
{
    private static TcpListener? _server;
    private static TcpClient? _client;
    private static CancellationTokenSource? _cts;
    private static Dictionary<string, (decimal price, long quantity)> _orderBook;
    private static decimal _midPrice;
    private static Random _random = new Random();

    
    public class OrderBookUpdate
    {
        public string Symbol { get; set; } = string.Empty;
        public string Side { get; set; } = string.Empty; // "Bids" or "Asks"
        public List<UpdateEntry> Updates { get; set; } = new List<UpdateEntry>();
    }

    public class UpdateEntry
    {
        public decimal Price { get; set; }
        public long Quantity { get; set; }
        [JsonPropertyName("action")]
        public string Action { get; set; } = "update"; 
    }

    public static async Task Main(string[] args)
    {
        // Parse command line arguments
        var argsDict = args.Select(arg => arg.Split('=')).ToDictionary(pair => pair[0], pair => pair.Length > 1 ? pair[1] : string.Empty);
        var symbol = argsDict.GetValueOrDefault("--symbol", "BTC/USD");
        if (!int.TryParse(argsDict.GetValueOrDefault("--bps", "10"), out var bps))
        {
            bps = 10;
        }

        Console.WriteLine($"Starting Order Book Simulator for {symbol} at {bps} updates/sec...");

        InitializeOrderBook();
        await StartTcpServer(symbol, bps);
    }

    private static void InitializeOrderBook()
    {
        _orderBook = new Dictionary<string, (decimal price, long quantity)>();
        _midPrice = 10000m; 

        for (int i = 0; i < 50; i++)
        {
            _orderBook[$"ask_{i}"] = (_midPrice + (i + 1) * 0.1m, _random.Next(1, 100));
            _orderBook[$"bid_{i}"] = (_midPrice - (i + 1) * 0.1m, _random.Next(1, 100));
        }
    }

    private static async Task StartTcpServer(string symbol, int bps)
    {
        try
        {
            _server = new TcpListener(IPAddress.Any, 8080);
            _server.Start();
            Console.WriteLine("TCP Server started. Waiting for a client to connect...");

            _client = await _server.AcceptTcpClientAsync();
            Console.WriteLine("Client connected!");

            _cts = new CancellationTokenSource();
            var networkStream = _client.GetStream();

            
            var fullBook = new List<OrderBookUpdate>
            {
                CreateFullUpdate("Bids"),
                CreateFullUpdate("Asks")
            };
            var fullBookJson = JsonSerializer.Serialize(fullBook);
            var fullBookBytes = Encoding.UTF8.GetBytes(fullBookJson + "\n");
            await networkStream.WriteAsync(fullBookBytes);

            await Task.Run(async () =>
            {
                var updateIntervalMs = 1000.0 / bps;
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var elapsed = stopwatch.ElapsedMilliseconds;
                        var updatesToSend = (int)(elapsed / updateIntervalMs);
                        stopwatch.Restart();

                        for (int i = 0; i < updatesToSend; i++)
                        {
                            var partialUpdate = GeneratePartialUpdate(symbol);
                            var jsonString = JsonSerializer.Serialize(partialUpdate);
                            var bytes = Encoding.UTF8.GetBytes(jsonString + "\n");
                            await networkStream.WriteAsync(bytes, 0, bytes.Length, _cts.Token);
                        }

                        // Wait for the next interval
                        if (updatesToSend == 0)
                        {
                            await Task.Delay((int)updateIntervalMs, _cts.Token);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error during update: {ex.Message}");
                        break;
                    }
                }
            }, _cts.Token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            _cts?.Cancel();
            _client?.Close();
            _server?.Stop();
        }
    }

    private static OrderBookUpdate CreateFullUpdate(string side)
    {
        var updates = new List<UpdateEntry>();
        var keys = _orderBook.Keys.Where(k => k.StartsWith(side.ToLowerInvariant().Substring(0, 3))).ToArray();
        foreach (var key in keys)
        {
            updates.Add(new UpdateEntry { Price = _orderBook[key].price, Quantity = _orderBook[key].quantity });
        }
        return new OrderBookUpdate { Symbol = "BTC/USD", Side = side, Updates = updates };
    }

    private static OrderBookUpdate GeneratePartialUpdate(string symbol)
    {
        var isBidSide = _random.NextDouble() < 0.5;
        var side = isBidSide ? "Bids" : "Asks";
        var updates = new List<UpdateEntry>();

        var keys = _orderBook.Keys.Where(k => k.StartsWith(side.ToLowerInvariant().Substring(0, 3))).ToList();
        var updateKey = keys[_random.Next(keys.Count)];

        var currentData = _orderBook[updateKey];
        var newPrice = currentData.price + (decimal)(_random.NextDouble() - 0.5) * 0.05m;
        var newQuantity = currentData.quantity + _random.Next(-5, 6);
        if (newQuantity < 1) newQuantity = 1;

        _orderBook[updateKey] = (newPrice, newQuantity);

        updates.Add(new UpdateEntry { Price = newPrice, Quantity = newQuantity, Action = "update" });

        return new OrderBookUpdate { Symbol = symbol, Side = side, Updates = updates };
    }
}
