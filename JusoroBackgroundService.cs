using System;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JusoroService
{
    /// <summary>
    /// JusoroService�� ���� ������ ���� �����췯 �� ������ Cloneȭ
    /// TODO : �α� ���� ���, Thread.Sleep Cycle ����ȭ �ʿ�, Exception ����ȭ �ʿ�, Publish �ܰ� ���� ������ ����, ������ �÷��� ����
    ///        �α� ���� ��� 1) ���������� ���� _loggerWriter.Close() �޼ҵ� �����
    ///        �α� ���� ��� 2) StartJusoro �޼ҵ�� StopAsync �޼ҵ忡�� �����ؾ� ���� �ǹ��̸�, Close() ȣ�� �ñⰡ ��Ȯ���� ����
    ///        
    /// ref1 : https://learn.microsoft.com/ko-kr/dotnet/core/extensions/windows-service?pivots=dotnet-7-0
    /// ref2 : https://learn.microsoft.com/ko-kr/dotnet/core/extensions/logging?tabs=command-line
    /// </summary>
    public class JusoroBackgroundService : BackgroundService
    {
        private readonly string jusoroWorkingPath = @"C:\jusoro-2.0.0-win64-internet";
        private string? jusoroBinPath;
        private Process? jusoroStartProcess;
        private Process? jusoroStopProcess;
        // private StreamWriter? _loggerWriter;

        public bool serviceStarted{ get; }

        private readonly ILogger<JusoroBackgroundService> _logger;

        public JusoroBackgroundService(ILogger<JusoroBackgroundService> logger)
        {
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    // ���� ����
                    if (jusoroStartProcess == null || !jusoroStartProcess.HasExited)
                    {
                        StartJusoro();
                    }
                    // TODO: �ֱ����� �۾� ���� (��: ���� ���� Ȯ��)
                    Thread.Sleep(TimeSpan.FromSeconds(5)); // ��: 5�ʸ��� Ȯ��
                }
            }, stoppingToken);
        }

        private void StartJusoro()
        {
            // var localDirectory = Directory.GetCurrentDirectory();
            // var logFilePath = Path.Combine(localDirectory, "jusoro_service.log");
            // _loggerWriter = new StreamWriter(logFilePath, true);

            // CMD â �� �α�
            _logger.LogInformation($"�ּҷ� ���񽺸� �ʱ�ȭ���Դϴ�.");
            // �ٽ� ������ �� �ʿ��� �ʱ�ȭ
            if (jusoroStartProcess != null)
            {
                jusoroStartProcess.Dispose();
            }

            // ���� ���� ��ɾ�
            jusoroBinPath = Path.Combine(jusoroWorkingPath, "jusoro", "bin");
            string jusoroStartCmd = Path.Combine(jusoroBinPath, "startup.cmd");

            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                // cmd.exe /C -> �ѹ� ������ ����
                Arguments = $"/C {jusoroStartCmd}",
                WorkingDirectory = jusoroBinPath,
                UseShellExecute = false,
                //RedirectStandardOutput = true,
                //RedirectStandardError = true,
            };

            jusoroStartProcess = new Process
            {
                StartInfo = startInfo
            };

            jusoroStartProcess.Start();

            //string result = process.StandardOutput.ReadToEnd();
            //_loggerWriter.WriteLine(result);

            jusoroStartProcess.WaitForExit();

            // jusoroStartProcess ���� �̺�Ʈ ó��
            jusoroStartProcess.EnableRaisingEvents = true;
            jusoroStartProcess.Exited += (sender, e) =>
            {
                //_loggerWriter.Close();
                //_loggerWriter = null;
                _logger.LogWarning($"[PID : {jusoroStartProcess.Id}] �ּҷ� ���񽺸� �����Ͽ����ϴ�.");
            };
        }

        // Ctrl + C�� Ű���� ���ͷ�Ʈ �߻���
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (jusoroStartProcess != null && jusoroStartProcess.HasExited && jusoroBinPath != null)
            {
                // CMD â �� �α�
                _logger.LogWarning($"�ּҷ� ���񽺸� �����մϴ�.");

                // ���� ���� ��ɾ�
                string jusoroStopCmd = Path.Combine(jusoroBinPath, "shutdown.cmd");

                var stopInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C {jusoroStopCmd}",
                    WorkingDirectory = jusoroBinPath,
                    UseShellExecute = false,
                };

                jusoroStopProcess = new Process
                {
                    StartInfo = stopInfo
                };

                jusoroStopProcess.Start();

                jusoroStopProcess.WaitForExit();

                // jusoroStartProcess ���� �̺�Ʈ ó��
                jusoroStopProcess.EnableRaisingEvents = true;
                jusoroStopProcess.Exited += (sender, e) =>
                {
                    _logger.LogWarning($"[PID : {jusoroStopProcess.Id}] �ּҷ� ���񽺸� �����Ͽ����ϴ�.");
                };
            }

            await base.StopAsync(cancellationToken);
        }
    }
}
