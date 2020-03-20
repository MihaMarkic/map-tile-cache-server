using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Map_Tile_Cache_Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MapTileController : ControllerBase
    {
        readonly ILogger<MapTileController> logger;
        readonly static HttpClient client = new HttpClient();
        readonly static string[] subdomains = new string[] { "a", "b", "c" };
        readonly static Random random = new Random();

        static MapTileController()
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("map-tile-cache-server");
        }
        public MapTileController(ILogger<MapTileController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        [Route("oss/{x:decimal}/{y:decimal}/{zoom:decimal}")]
        public async Task<IActionResult> Get(decimal x, decimal y, decimal zoom)
        {
            int startingSubDomain = random.Next(3);
            string subdomain = subdomains[startingSubDomain];
            var policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(5, 
                    sleepDurationProvider: retry => TimeSpan.FromSeconds(1), 
                    onRetry: (result, span, retry, context) => {
                        Console.WriteLine($"Failed because of status {result.Result?.StatusCode}");
                        subdomain = subdomains[(retry+ startingSubDomain) % subdomains.Length];
                    });
            var response = await policy.ExecuteAsync(() =>
            {
                string url = $"https://{subdomain}.tile.openstreetmap.org/{zoom}/{x}/{y}.png";
                Console.WriteLine($"Trying on {url}");
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                return client.SendAsync(request);

            });
            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync();
                return File(stream, "image/png");
            }
            else
            {
                return StatusCode(500);
            }
        }
    }
}
