using HslCommunication.ModBus;
using R_Factory_Tools.Models;
using R_Factory_Tools.Utilities;
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

        private readonly Dictionary<Endpoints, ModbusTcpNet> _connections = [];

        public FormMain()
        {
            InitializeComponent();
            _lastModified = null;
            _pollCts = new CancellationTokenSource();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
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
            var json = File.ReadAllText(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "appsettings.json"));
            using var doc = JsonDocument.Parse(json);
            var endpointsAPI = doc.RootElement
                .GetProperty("EndPointsAPI")
                .GetString()
                ?? throw new InvalidOperationException("ChangeAPI not found");
            HttpResponseMessage response = await _httpClient.GetAsync(endpointsAPI);
            response.EnsureSuccessStatusCode();
            string result = await response.Content.ReadAsStringAsync();
            (_endpoints, _modbusDetails, _startAddresses) = JsonSerializer.Deserialize<
                Tuple<List<Endpoints>, List<ModbusDetails>, List<StartAddresses>>>(result)!;
            ushort first = ushort.Parse(_startAddresses.First().StartAddress);
            ushort last = ushort.Parse(_startAddresses.Last().StartAddress);
            _startAddresses = _startAddresses.OrderBy(x => int.Parse(x.StartAddress)).ToList();

            for (int addr = first; addr <= last; addr++)
            {
                bool exists = _startAddresses.Any(x => int.Parse(x.StartAddress) == addr);
                if (!exists)
                {
                    _startAddresses.Add(new StartAddresses(0, addr.ToString()));
                }
            }
            _startAddresses = _startAddresses.OrderBy(x => int.Parse(x.StartAddress)).ToList();
        }

        private void CleanupStaleConnections()
        {
            var keep = new HashSet<Endpoints>(_endpoints);
            var toRemove = _connections.Keys
                .Where(ep => !keep.Contains(ep))
                .ToList();

            foreach (var ep in toRemove)
            {
                try
                {
                    var modbus = _connections[ep];
                    try { modbus.ConnectClose(); } catch { }
                }
                catch { }
                _connections.Remove(ep);
            }
        }

        private async Task PollModbusDevicesAsync()
        {
            var endpointList = _endpoints.ToList(); // clone

            foreach (var enpoint in endpointList)
            {
                try
                {
                    if (!_connections.TryGetValue(enpoint, out var modbus))
                    {
                        modbus = new ModbusTcpNet(enpoint.IP, Convert.ToInt32(enpoint.Port));
                        modbus.ReceiveTimeOut = 1000;
                        var connect = modbus.ConnectServer();
                        if (!connect.IsSuccess)
                        {
                            throw new InvalidOperationException($"Failed to connect to {enpoint.IP}:{enpoint.Port} - {connect.Message}");
                        }
                        _connections[enpoint] = modbus;
                    }

                    if (_modbusDetails == null || _modbusDetails.Count == 0 || _startAddresses == null || _startAddresses.Count == 0)
                        continue;

                    string functionName = _modbusDetails[0].FunctionRead.Replace(" ", "").Trim().ToLower();
                    byte unitId = Convert.ToByte(_modbusDetails[0].SlaveId);

                    modbus.Station = unitId;

                    ushort first = ushort.Parse(_startAddresses.First().StartAddress);
                    ushort last = ushort.Parse(_startAddresses.Last().StartAddress);
                    ushort numPoints = (ushort)Math.Max(0, last - first + 1);

                    if (numPoints == 0)
                        continue;

                    if (functionName.Contains("readcoils") ||
                        Regex.IsMatch(functionName, @"1(?!x)|0x", RegexOptions.IgnoreCase))
                    {
                        // Read coils (bit table, function 1)
                        // Hsl: ReadBool(address, length) returns OperateResult<bool[]>
                        var res = modbus.ReadBool(first.ToString(), numPoints);
                        if (res.IsSuccess)
                        {
                            bool[] coils = res.Content;
                            // TODO: use coils as needed (not saved to DB in your original code)
                        }
                        else
                        {
                            ErrorLogger.Write(new Exception($"Read coils failed: {res.Message}"));
                        }
                    }
                    else if (functionName.Contains("readinputs") ||
                        Regex.IsMatch(functionName, @"2(?!x)|1x", RegexOptions.IgnoreCase))
                    {
                        // Read discrete inputs (function 2). Hsl uses address prefix "x=2;" for discrete inputs in some examples,
                        // but since we set Station above you can also try ReadBool with an "x=2;" prefix:
                        var addrPrefix = $"x=2;{first}";
                        var res = modbus.ReadBool(addrPrefix, numPoints);
                        if (res.IsSuccess)
                        {
                            bool[] inputs = res.Content;
                            // TODO: use inputs as needed
                        }
                        else
                        {
                            ErrorLogger.Write(new Exception($"Read discrete inputs failed: {res.Message}"));
                        }
                    }
                    else if (functionName.Contains("readholdingregisters") ||
                        Regex.IsMatch(functionName, @"3(?!x)|4x", RegexOptions.IgnoreCase))
                    {
                        // Read holding registers (function 3)
                        // Use Read(address, length) to get raw bytes (each register is 2 bytes)
                        var res = modbus.ReadUInt16(first.ToString(), numPoints);
                        if (!res.IsSuccess)
                        {
                            ErrorLogger.Write(new Exception($"ReadHoldingRegisters failed: {res.Message}"));
                        }
                        else
                        {
                            // send to DB (your existing method expects ushort[])
                            await SendDataToDB(res.Content);
                        }
                    }
                    else if (functionName.Contains("readinputregisters") ||
                        Regex.IsMatch(functionName, @"4(?!x)|3x", RegexOptions.IgnoreCase))
                    {
                        // Read input registers (function 4).
                        // Hsl examples sometimes use "x=4;100" to indicate input register table
                        var addrPrefix = $"x=4;{first}";
                        var res = modbus.Read(addrPrefix, numPoints);
                        if (res.IsSuccess)
                        {
                            byte[] bytes = res.Content;
                            ushort[] inRegs = new ushort[numPoints];
                            for (int i = 0; i < numPoints; i++)
                            {
                                int baseIndex = i * 2;
                                inRegs[i] = (ushort)((bytes[baseIndex] << 8) | bytes[baseIndex + 1]);
                            }
                        }
                        else
                        {
                            ErrorLogger.Write(new Exception($"ReadInputRegisters failed: {res.Message}"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogger.Write(ex);
                    if (_connections.TryGetValue(enpoint, out var bad))
                    {
                        try { bad.ConnectClose(); } catch { }
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

    internal class EndPointsDTO
    {
        public List<Endpoints> Item1 { get; set; }
        public List<ModbusDetails> Item2 { get; set; }
        public List<StartAddresses> Item3 { get; set; }
    }

    public record Endpoints(string IP = "", string Port = "");
    public record ModbusDetails(string SlaveId = "", string FunctionRead = "");
    public record StartAddresses(int DeviceParameterId = 0, string StartAddress = "");
}