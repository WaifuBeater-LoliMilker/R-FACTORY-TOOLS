using MySqlConnector;
using NModbus;
using R_Factory_Tools.DTO;
using R_Factory_Tools.Models;
using R_Factory_Tools.Repositories;
using R_Factory_Tools.Utilities;
using System.Net.Sockets;

namespace R_Factory_Tools
{
    public partial class FormMain : Form
    {
        private CancellationTokenSource _pollCts;
        private DateTime? _lastModified;
        private const int PollIntervalMs = 5000;
        private Task? _pollingTask;

        private List<Endpoints> _endpoints = [];
        private List<TcpClient> _tcpClient;
        private List<IModbusFactory> _modbusFactory;
        private List<IModbusMaster> modbus_Poll;

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
            _pollCts.Cancel();
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
                        }

                        DateTime? current = await FetchLatestTimestampAsync(ct).ConfigureAwait(false);

                        if (current != _lastModified)
                        {
                            _lastModified = current;

                            if (!IsDisposed && !ct.IsCancellationRequested)
                            {
                                BeginInvoke((Action)OnTableChanged);
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
                }
            }, ct);
        }

        private async Task<DateTime?> FetchLatestTimestampAsync(CancellationToken ct)
        {
            await using var conn = new MySqlConnection(ConnectionStringProvider.Default);
            await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText =
                "SELECT last_modified FROM audit_log where table_name = 'device_communication_param_config';";

            var result = await cmd.ExecuteScalarAsync(ct);
            return result == DBNull.Value ? null : (DateTime?)result;
        }

        private async void OnTableChanged()
        {
            var repo = new GenericRepo();
            var data = await repo.ProcedureToList<DeviceCommunicationParamConfigDTO>("spGetDeviceCommunicationParamConfig",
                ["DeviceParamId"], [-1]);
            _endpoints = [.. data
            .GroupBy(x => x.DeviceParameterId)
            .Select(g =>
            {
                var ipRow = g.First(d => d.ParamKey == "IP");
                var portRow = g.First(d => d.ParamKey == "PORT");

                if (!int.TryParse(portRow.ConfigValue, out var port))
                    throw new InvalidOperationException($"Invalid port for DeviceParameterId={g.Key}");

                return new Endpoints(ipRow.ConfigValue, port);
            }).Distinct()];
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
        }
    }

    public record Endpoints(string IP, int Port);
}