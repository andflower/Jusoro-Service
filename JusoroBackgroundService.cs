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
    /// JusoroService를 위한 윈도우 서비스 스케쥴러 및 리눅스 Clone화
    /// TODO : 로그 파일 기록, Thread.Sleep Cycle 최적화 필요, Exception 최적화 필요, Publish 단계 시작 전까지 구현, 리눅스 플랫폼 적용
    ///        로그 파일 기록 1) 지역변수로 인한 _loggerWriter.Close() 메소드 실행됨
    ///        로그 파일 기록 2) StartJusoro 메소드와 StopAsync 메소드에서 각각해야 될지 의문이며, Close() 호출 시기가 명확하지 않음
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
                    // 서비스 시작
                    if (jusoroStartProcess == null || !jusoroStartProcess.HasExited)
                    {
                        StartJusoro();
                    }
                    // TODO: 주기적인 작업 수행 (예: 서비스 상태 확인)
                    Thread.Sleep(TimeSpan.FromSeconds(5)); // 예: 5초마다 확인
                }
            }, stoppingToken);
        }

        private void StartJusoro()
        {
            // var localDirectory = Directory.GetCurrentDirectory();
            // var logFilePath = Path.Combine(localDirectory, "jusoro_service.log");
            // _loggerWriter = new StreamWriter(logFilePath, true);

            // CMD 창 내 로그
            _logger.LogInformation($"주소로 서비스를 초기화중입니다.");
            // 다시 시작할 때 필요한 초기화
            if (jusoroStartProcess != null)
            {
                jusoroStartProcess.Dispose();
            }

            // 서비스 시작 명령어
            jusoroBinPath = Path.Combine(jusoroWorkingPath, "jusoro", "bin");
            string jusoroStartCmd = Path.Combine(jusoroBinPath, "startup.cmd");

            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                // cmd.exe /C -> 한번 실행후 종료
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

            // jusoroStartProcess 종료 이벤트 처리
            jusoroStartProcess.EnableRaisingEvents = true;
            jusoroStartProcess.Exited += (sender, e) =>
            {
                //_loggerWriter.Close();
                //_loggerWriter = null;
                _logger.LogWarning($"[PID : {jusoroStartProcess.Id}] 주소로 서비스를 시작하였습니다.");
            };
        }

        // Ctrl + C로 키보드 인터럽트 발생시
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (jusoroStartProcess != null && jusoroStartProcess.HasExited && jusoroBinPath != null)
            {
                // CMD 창 내 로그
                _logger.LogWarning($"주소로 서비스를 종료합니다.");

                // 서비스 중지 명령어
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

                // jusoroStartProcess 종료 이벤트 처리
                jusoroStopProcess.EnableRaisingEvents = true;
                jusoroStopProcess.Exited += (sender, e) =>
                {
                    _logger.LogWarning($"[PID : {jusoroStopProcess.Id}] 주소로 서비스를 종료하였습니다.");
                };
            }

            await base.StopAsync(cancellationToken);
        }
    }
}
