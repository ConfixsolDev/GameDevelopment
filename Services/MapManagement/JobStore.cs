using System.Collections.Concurrent;
using TechWebSol.Models.MapManagement;

namespace TechWebSol.Services.MapManagement
{
	public class JobStore
	{
        private readonly ConcurrentDictionary<string, DownloadJob> _jobs = new();

        public IReadOnlyDictionary<string, DownloadJob> Jobs => _jobs;

		public string Add(DownloadJob job)
		{
			var id = Guid.NewGuid().ToString("N");
			_jobs[id] = job;
			return id;
		}

		public bool TryGet(string jobId, out DownloadJob job) => _jobs.TryGetValue(jobId, out job!);
	}
}
