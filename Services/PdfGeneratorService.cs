using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace TechWebSol.Services
{
    public interface IPdfGeneratorService
    {
        Task<byte[]> GeneratePdfFromHtmlAsync(string html, string? baseUrl = null);
    }

    public class PdfGeneratorService : IPdfGeneratorService
    {
        private static bool _browserDownloaded = false;
        private static readonly SemaphoreSlim _downloadSemaphore = new SemaphoreSlim(1, 1);

        public async Task<byte[]> GeneratePdfFromHtmlAsync(string html, string? baseUrl = null)
        {
            await EnsureBrowserAsync();

            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
            });

            await using var page = await browser.NewPageAsync();

            // Set base URL so relative assets resolve
            var contentOptions = new NavigationOptions
            {
                Timeout = 60000
            };

            await page.SetContentAsync(html, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle0 } });

            var pdfOptions = new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true,
                MarginOptions = new MarginOptions
                {
                    Top = "15mm",
                    Bottom = "15mm",
                    Left = "15mm",
                    Right = "15mm"
                }
            };

            var pdfBytes = await page.PdfDataAsync(pdfOptions);
            return pdfBytes;
        }

        private static async Task EnsureBrowserAsync()
        {
            if (_browserDownloaded) return;

            await _downloadSemaphore.WaitAsync();
            try
            {
                if (_browserDownloaded) return;
                // Download Chromium to local cache on first use
                using var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();
                _browserDownloaded = true;
            }
            finally
            {
                _downloadSemaphore.Release();
            }
        }
    }
}


