using Microsoft.AspNetCore.Mvc;

namespace Diplom0._1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileController : ControllerBase
    {
        private readonly A2 _agentA2;

        public FileController()
        {
            _agentA2 = new A2();
        }

        [HttpPost("receive_file")]
        public IActionResult ReceiveFile([FromBody] FileData fileData)
        {
            var result = _agentA2.ReceiveFile(fileData.FileName, fileData.FileContent);
            return Ok(result);
        }
    }

    public class FileData
    {
        public string FileName { get; set; }
        public string FileContent { get; set; }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
