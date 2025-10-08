using Microsoft.Extensions.Hosting;

namespace TechWebSol.Services.MapManagement
{
	public class StartupCleanup : IHostedService
	{
		public Task StartAsync(CancellationToken cancellationToken)
		{
			try
			{
				var downloadsRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot", "downloads");
				if (Directory.Exists(downloadsRoot))
				{
					Directory.Delete(downloadsRoot, true);
				}
			}
			catch { /* best-effort cleanup */ }
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
	}
}

