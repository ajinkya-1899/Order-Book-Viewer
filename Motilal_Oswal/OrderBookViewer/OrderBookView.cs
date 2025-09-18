using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using System.Linq;

namespace Order_Book_Viewer
{
    using System.Diagnostics;
    using System.Net.Sockets;
    using System.Text;
    using System.Text.Json;
    using System.Windows.Forms;
    using System.Linq;

    public partial class OrderBookViewer : Form
    {
        private TcpClient? _client;
        private NetworkStream? _stream;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private Dictionary<string, (decimal price, long quantity)> _bids = new Dictionary<string, (decimal price, long quantity)>();
        private Dictionary<string, (decimal price, long quantity)> _asks = new Dictionary<string, (decimal price, long quantity)>();
        private System.Windows.Forms.Timer _updateTimer = new System.Windows.Forms.Timer();
        private Queue<OrderBookUpdate> _updateQueue = new Queue<OrderBookUpdate>();
        private Stopwatch _ingestStopwatch = new Stopwatch();
        private Stopwatch _renderStopwatch = new Stopwatch();
        private long _updatesReceived = 0;
        private long _updatesDropped = 0;
        private bool _isPaused = false;

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
            public string Action { get; set; } = "update";
        }

        public OrderBookViewer()
        {
            InitializeComponent();
            this.Load += OrderBookViewer_Load;
        }

        private void OrderBookViewer_Load(object? sender, EventArgs e)
        {
           
            ConnectToServer();
            _ingestStopwatch.Start();
            _updateTimer.Start();

            
            _updateTimer.Interval = 50; 
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();
        }

        private void _pauseResumeButton_Click(object? sender, EventArgs e)
        {
            _isPaused = !_isPaused;
            if (_isPaused)
            {
                _updateTimer.Stop();
                _pauseResumeButton.Text = "Resume";
                Log("Updates paused.");
            }
            else
            {
                _updateTimer.Start();
                _pauseResumeButton.Text = "Pause";
                Log("Updates resumed.");
            }
        }

        private void _speedSlider_Scroll(object? sender, EventArgs e)
        {
            _updateTimer.Interval = 1000 / _speedSlider.Value;
            Log($"Update speed set to {_speedSlider.Value} updates/sec.");
        }

        private void _symbolDropdown_SelectedIndexChanged(object? sender, EventArgs e)
        {
            string? selectedSymbol = _symbolDropdown.SelectedItem?.ToString();
            if (selectedSymbol != null)
            {
                Log($"Symbol changed to {selectedSymbol}.");
            }
        }

        private async void ConnectToServer()
        {
            try
            {
                Log("Attempting to connect to server...");
                _client = new TcpClient();
                await _client.ConnectAsync("localhost", 8080);
                Log("Connected to server!");
                _stream = _client.GetStream();
                _ = Task.Run(IngestData, _cts.Token);
            }
            catch (Exception ex)
            {
                Log($"Connection error: {ex.Message}");
            }
        }

        private async Task IngestData()
        {
            var buffer = new byte[4096];
            var stringBuilder = new StringBuilder();

            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    var bytesRead = await _stream!.ReadAsync(buffer, _cts.Token);
                    if (bytesRead == 0)
                    {
                        Log("Server disconnected.");
                        break;
                    }

                    string chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    stringBuilder.Append(chunk);

                    while (true)
                    {
                        var newlineIndex = stringBuilder.ToString().IndexOf('\n');
                        if (newlineIndex == -1) break;

                        var message = stringBuilder.ToString(0, newlineIndex);
                        stringBuilder.Remove(0, newlineIndex + 1);

                        try
                        {
                            var update = JsonSerializer.Deserialize<OrderBookUpdate>(message);
                            if (update != null)
                            {
                                _updatesReceived++;
                                lock (_updateQueue)
                                {
                                    _updateQueue.Enqueue(update);
                                }
                            }
                        }
                        catch (JsonException)
                        {
                            // Log malformed message
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Log("Ingestion task cancelled.");
            }
            catch (Exception ex)
            {
                Log($"Ingestion error: {ex.Message}");
            }
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            this.BeginInvoke(() => ApplyUpdates());
        }

        private void ApplyUpdates()
        {
            _renderStopwatch.Restart();
            var processedCount = 0;

            lock (_updateQueue)
            {
                if (_updateQueue.Count == 0) return;

                while (_updateQueue.Count > 0)
                {
                    var update = _updateQueue.Dequeue();

                    if (_updateQueue.Count > 1000)
                    {
                        _updatesDropped++;
                    }

                    if (update.Side == "Bids")
                    {
                        UpdateOrderBook(bidsGridView, _bids, update);
                    }
                    else if (update.Side == "Asks")
                    {
                        UpdateOrderBook(asksGridView, _asks, update);
                    }

                    processedCount++;
                }
            }

            if (processedCount > 0)
            {
                bidsGridView.Refresh();
                asksGridView.Refresh();
            }

            _renderStopwatch.Stop();

            UpdateMetrics(processedCount);
        }

        private void UpdateOrderBook(DataGridView grid, Dictionary<string, (decimal price, long quantity)> book, OrderBookUpdate update)
        {
            foreach (var entry in update.Updates)
            {
                var key = $"{update.Side.ToLowerInvariant().Substring(0, 3)}_{book.Count}";
                if (!book.ContainsKey(key))
                {
                    key = book.FirstOrDefault(x => x.Value.price == entry.Price).Key;
                }

                if (entry.Action == "update")
                {
                    if (key != null)
                    {
                        book[key] = (entry.Price, entry.Quantity);
                        HighlightRow(grid, key);
                    }
                    else
                    {
                        key = $"{update.Side.ToLowerInvariant().Substring(0, 3)}_{book.Count}";
                        book[key] = (entry.Price, entry.Quantity);
                    }
                }
                else if (entry.Action == "delete" && key != null)
                {
                    book.Remove(key);
                }
                else if (entry.Action == "insert")
                {
                    key = $"{update.Side.ToLowerInvariant().Substring(0, 3)}_{book.Count}";
                    book[key] = (entry.Price, entry.Quantity);
                }
            }

            if (update.Side == "Bids")
            {
                _bids = _bids.OrderByDescending(x => x.Value.price).Take(50).ToDictionary(x => x.Key, x => x.Value);
                bidsGridView.RowCount = _bids.Count;
            }
            else
            {
                _asks = _asks.OrderBy(x => x.Value.price).Take(50).ToDictionary(x => x.Key, x => x.Value);
                asksGridView.RowCount = _asks.Count;
            }
        }

        private void HighlightRow(DataGridView grid, string key)
        {
            var rowIndex = -1;
            var book = grid == bidsGridView ? _bids.Values.OrderByDescending(b => b.price).ToArray() : _asks.Values.OrderBy(a => a.price).ToArray();
            for (int i = 0; i < book.Length; i++)
            {
                if (book[i].price == ((grid == bidsGridView ? _bids[key] : _asks[key]).price) && book[i].quantity == ((grid == bidsGridView ? _bids[key] : _asks[key]).quantity))
                {
                    rowIndex = i;
                    break;
                }
            }

            if (rowIndex >= 0 && rowIndex < grid.RowCount)
            {
                grid.Rows[rowIndex].DefaultCellStyle.BackColor = System.Drawing.Color.LightGreen;
                var timer = new System.Windows.Forms.Timer();
                timer.Interval = 300;
                timer.Tick += (s, e) =>
                {
                    grid.InvalidateRow(rowIndex);
                    timer.Stop();
                    timer.Dispose();
                };
                timer.Start();
            }
        }

        private void BidsGridView_CellValueNeeded(object? sender, DataGridViewCellValueEventArgs e)
        {
            var orderedBids = _bids.Values.OrderByDescending(b => b.price).ToArray();
            if (e.RowIndex < orderedBids.Length)
            {
                var bid = orderedBids[e.RowIndex];
                if (e.ColumnIndex == 0) e.Value = bid.price.ToString("F2");
                else if (e.ColumnIndex == 1) e.Value = bid.quantity.ToString();
            }
        }

        private void AsksGridView_CellValueNeeded(object? sender, DataGridViewCellValueEventArgs e)
        {
            var orderedAsks = _asks.Values.OrderBy(a => a.price).ToArray();
            if (e.RowIndex < orderedAsks.Length)
            {
                var ask = orderedAsks[e.RowIndex];
                if (e.ColumnIndex == 0) e.Value = ask.price.ToString("F2");
                else if (e.ColumnIndex == 1) e.Value = ask.quantity.ToString();
            }
        }

        private void UpdateMetrics(int processedCount)
        {
            this.BeginInvoke(() =>
            {
                logTextBox.Text = $"Ingest Speed: {_updatesReceived / (_ingestStopwatch.ElapsedMilliseconds / 1000.0):F2} updates/sec\n";
                logTextBox.AppendText($"UI Render Time: {_renderStopwatch.ElapsedMilliseconds}ms\n");
                logTextBox.AppendText($"Updates Processed/Batch: {processedCount}\n");
                logTextBox.AppendText($"Dropped Updates: {_updatesDropped}\n");
            });
        }

        private void Log(string message)
        {
            this.BeginInvoke(() =>
            {
                logTextBox.AppendText($"{DateTime.Now:HH:mm:ss} - {message}\n");
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cts.Cancel();
                _client?.Close();
                _updateTimer.Stop();
                _updateTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }

}
