using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace TechWebSol.Services.MapManagement
{
    /// <summary>
    /// Hosted service that automatically starts TileServer-GL when the application starts
    /// </summary>
    public class TileServerHostedService : IHostedService
    {
        private readonly ILogger<TileServerHostedService> _logger;
        private Process? _tileServerProcess;

        public TileServerHostedService(ILogger<TileServerHostedService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var contentRoot = Directory.GetCurrentDirectory();
                var scriptDir = Path.Combine(contentRoot, "tileserver");
                var batPath = Path.Combine(scriptDir, "start-tileserver.bat");

                if (!File.Exists(batPath))
                {
                    _logger.LogWarning("TileServer-GL startup script not found at: {Path}", batPath);
                    return Task.CompletedTask;
                }

                _logger.LogInformation("Starting TileServer-GL automatically...");

                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c cd /d \"{scriptDir}\" && start-tileserver.bat",
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    WorkingDirectory = scriptDir,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                _tileServerProcess = Process.Start(psi);

                _logger.LogInformation("TileServer-GL started successfully on port 8080");
                _logger.LogInformation("TileServer-GL UI: http://localhost:8080");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start TileServer-GL automatically");
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_tileServerProcess != null && !_tileServerProcess.HasExited)
                {
                    _logger.LogInformation("Stopping TileServer-GL...");
                    _tileServerProcess.Kill();
                    _tileServerProcess.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping TileServer-GL");
            }

            return Task.CompletedTask;
        }
    }
}

