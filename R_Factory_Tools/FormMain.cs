using MySqlConnector;
using NModbus;
using R_Factory_Tools.Models;
using R_Factory_Tools.Repositories;
using R_Factory_Tools.Utilities;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace R_Factory_Tools
{
    public partial class FormMain : Form
    {
        private readonly HttpClient _httpClient = new();
        private CancellationTokenSource _pollCts;
        private DateTime? _lastModified;
        private const int PollIntervalMs = 5000;
        private Task? _pollingTask;

        private List<Endpoints> _endpoints = [];
        private List<ModbusDetails> _modbusDetails = [];
        private List<StartAddresses> _startAddresses = [];
        private readonly ModbusFactory _factory = new();
        private readonly Dictionary<Endpoints, (TcpClient client, IModbusMaster master)> _connections = [];

        public FormMain()
        {
            InitializeComponent();
            _lastModified = null;
            _pollCts = new CancellationTokenSource();
            grvData.AutoGenerateColumns = false;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            LoadTableData();
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            try { _pollCts.Cancel(); }
            catch { }
        }

        private Task StartPollingLoopAsync(CancellationToken ct)
        {
            return Task.Run(async () =>
            {
                try
                {
                    while (!ct.IsCancellationRequested)
                    {
                        if (!IsDisposed && !ct.IsCancellationRequested)
                        {
                            BeginInvoke((Action)LoadTableData);
                            CleanupStaleConnections();
                            await PollModbusDevicesAsync();
                        }
                        btnStart.BeginInvoke(() =>
                        {
                            lblConnectionStatusValue.Text = "Connected";
                            lblConnectionStatusValue.BackColor = Color.Green;
                        });

                        DateTime? current = await FetchLatestTimestampAsync(ct).ConfigureAwait(false);

                        if (current != _lastModified)
                        {
                            _lastModified = current;

                            if (!IsDisposed && !ct.IsCancellationRequested)
                            {
                                try
                                {
                                    BeginInvoke((Action)OnTableChanged);
                                }
                                catch (Exception ex)
                                {
                                    ErrorLogger.Write(ex);
                                }
                            }
                        }

                        await Task.Delay(PollIntervalMs, ct).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    // expected on cancellation
                }
                catch (Exception ex)
                {
                    ErrorLogger.Write(ex);
                    btnStart.BeginInvoke(() =>
                    {
                        lblConnectionStatusValue.Text = "Disconnected";
                        lblConnectionStatusValue.BackColor = Color.OrangeRed;
                    });
                }
            }, ct);
        }

        private async Task<DateTime?> FetchLatestTimestampAsync(CancellationToken ct)
        {
            var json = File.ReadAllText(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "appsettings.json"));
            using var doc = JsonDocument.Parse(json);
            var changeAPI = doc.RootElement
                .GetProperty("ChangeAPI")
                .GetString()
                ?? throw new InvalidOperationException("ChangeAPI not found");
            HttpResponseMessage response = await _httpClient.GetAsync(changeAPI, ct);
            response.EnsureSuccessStatusCode();
            string result = await response.Content.ReadAsStringAsync(ct);
            DateTime date = JsonSerializer.Deserialize<DateTime>(result);
            return date;
        }

        private async void OnTableChanged()
        {
            var repo = new GenericRepo();
            (_endpoints, _modbusDetails, _startAddresses) =
                await repo.ProcedureToList<Endpoints, ModbusDetails, StartAddresses>(
                "spGetEnpoints", [], []);
            ushort first = ushort.Parse(_startAddresses.First().StartAddress);
            ushort last = ushort.Parse(_startAddresses.Last().StartAddress);
            _startAddresses = [.. _startAddresses.OrderBy(x => int.Parse(x.StartAddress))];
            for (int addr = first; addr <= last; addr++)
            {
                bool exists = _startAddresses.Any(x => int.Parse(x.StartAddress) == addr);
                if (!exists)
                {
                    _startAddresses.Add(new StartAddresses(0, addr.ToString()));
                }
            }
            _startAddresses = [.. _startAddresses.OrderBy(x => int.Parse(x.StartAddress))];
        }

        private async void LoadTableData()
        {
            var repo = new GenericRepo();
            var data = await repo.FindByExpression<DeviceParameterLogs>(
                l => true,
                q => q.OrderByDescending(l => l.Id),
                take: 10);
            grvData.DataSource = data;
        }

        private void CleanupStaleConnections()
        {
            var keep = new HashSet<Endpoints>(_endpoints);
            var toRemove = _connections.Keys
                .Where(ep => !keep.Contains(ep))
                .ToList();

            foreach (var ep in toRemove)
            {
                var (client, master) = _connections[ep];
                try { client.Close(); } catch { }
                _connections.Remove(ep);
            }
        }

        private async Task PollModbusDevicesAsync()
        {
            var endpointList = _endpoints.ToList();//clone

            foreach (var enpoint in endpointList)
            {
                try
                {
                    if (!_connections.TryGetValue(enpoint, out var tuple))
                    {
                        var tcp = new TcpClient();
                        await tcp.ConnectAsync(enpoint.IP, Convert.ToInt32(enpoint.Port));
                        var master = _factory.CreateMaster(tcp);
                        master.Transport.ReadTimeout = 1000;
                        master.Transport.Retries = 1;

                        tuple = (tcp, master);
                        _connections[enpoint] = tuple;
                    }

                    var masterClient = tuple.master;
                    string functionName = _modbusDetails[0].FunctionRead.Replace(" ", "").Trim().ToLower();
                    byte unitId = Convert.ToByte(_modbusDetails[0].SlaveId);
                    ushort first = ushort.Parse(_startAddresses.First().StartAddress);
                    ushort last = ushort.Parse(_startAddresses.Last().StartAddress);
                    ushort numPoints = (ushort)Math.Max(0, last - first + 1);
                    if (functionName.Contains("readcoils") ||
                        Regex.IsMatch(functionName, @"1(?!x)|0x", RegexOptions.IgnoreCase))
                    {
                        var coils = masterClient.ReadCoils(unitId, first, numPoints);
                    }
                    else if (functionName.Contains("readinputs") ||
                        Regex.IsMatch(functionName, @"2(?!x)|1x", RegexOptions.IgnoreCase))
                    {
                        var inputs = masterClient.ReadInputs(unitId, first, numPoints);
                    }
                    else if (functionName.Contains("readholdingregisters") ||
                        Regex.IsMatch(functionName, @"3(?!x)|4x", RegexOptions.IgnoreCase))
                    {
                        var regs = masterClient.ReadHoldingRegisters(unitId, first, numPoints);
                        await SendDataToDB(regs);
                        Console.WriteLine($"[{enpoint.IP}:{enpoint.Port}] → {string.Join(", ", regs)}");
                    }
                    else if (functionName.Contains("readinputregisters") ||
                        Regex.IsMatch(functionName, @"4(?!x)|3x", RegexOptions.IgnoreCase))
                    {
                        var inRegs = masterClient.ReadInputRegisters(unitId, first, numPoints);
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogger.Write(ex);
                    if (_connections.TryGetValue(enpoint, out var bad))
                    {
                        try { bad.client.Close(); } catch { }
                        _connections.Remove(enpoint);
                    }
                }
            }
        }

        private async Task SendDataToDB(ushort[] values)
        {
            var json = File.ReadAllText(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "appsettings.json"));
            using var doc = JsonDocument.Parse(json);
            var secret = doc.RootElement
                .GetProperty("Secret")
                .GetString()
                ?? throw new InvalidOperationException("Secret not found");
            var backendAPI = doc.RootElement
                .GetProperty("LogAPI")
                .GetString()
                ?? throw new InvalidOperationException("LogAPI not found");
            var data = new List<DeviceParameterLogs>();
            var currentTime = DateTime.Now;
            for (int i = 0; i < values.Length; i++)
            {
                data.Add(new DeviceParameterLogs
                {
                    Id = 0,
                    DeviceParameterId = _startAddresses[i].DeviceParameterId,
                    LogValue = values[i].ToString(),
                    YearValue = currentTime.Year,
                    MonthValue = currentTime.Month,
                    DayValue = currentTime.Day,
                    HourValue = currentTime.Hour,
                    MinuteValue = currentTime.Minute,
                    SecondValue = currentTime.Second
                });
            }
            grvData.BeginInvoke(() =>
            {
                var newSource = data.Where(d => d.DeviceParameterId != 0).ToList();
                grvData.DataSource = newSource;
            });
            string jsonData = JsonSerializer.Serialize(new { data, secret });
            using var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(backendAPI, content);
            response.EnsureSuccessStatusCode();
            string result = await response.Content.ReadAsStringAsync();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            _pollCts = new CancellationTokenSource();
            _pollingTask = StartPollingLoopAsync(_pollCts.Token);
        }

        private async void btnStop_Click(object sender, EventArgs e)
        {
            if (_pollCts != null)
            {
                _pollCts.Cancel();
                try { await _pollingTask!; }
                catch { }
                _pollCts.Dispose();
            }
            lblConnectionStatusValue.Text = "Disconnected";
            lblConnectionStatusValue.BackColor = Color.OrangeRed;
        }

        private void btnHide_Click(object sender, EventArgs e)
        {
            this.Hide();
            notifyIcon.Visible = true;
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            if (((MouseEventArgs)e).Button == MouseButtons.Left)
            {
                this.Show();
                notifyIcon.Visible = false;
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            if (Application.MessageLoop)
            {
                Application.Exit();
            }
            else
            {
                Environment.Exit(1);
            }
        }
    }

    public record Endpoints(string IP = "", string Port = "");
    public record ModbusDetails(string SlaveId = "", string FunctionRead = "");
    public record StartAddresses(int DeviceParameterId = 0, string StartAddress = "");
}