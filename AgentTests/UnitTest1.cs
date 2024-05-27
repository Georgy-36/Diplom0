using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Xunit;

namespace Diplom0._1
{
    public class FileTransferTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public FileTransferTest(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task TestFileTransfer()
        {
            var client = _factory.CreateClient();

            var fileName = "testfile.txt";
            var fileContent = Convert.ToBase64String(Encoding.UTF8.GetBytes("This is a test file content."));

            var content = new StringContent(JsonConvert.SerializeObject(new
            {
                FileName = fileName,
                FileContent = fileContent
            }), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/file/receive_file", content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            Assert.Equal($"File {fileName} received successfully", result);
        }
    }
    public class FileControllerE2ETest : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly IWebDriver _driver;
        private readonly string _baseUrl;

        public FileControllerE2ETest()
        {
            _driver = new ChromeDriver();
            _baseUrl = "http://localhost:5000";
        }

        [Fact]
        public void ReceiveFile_ReturnSuccessMessage()
        {

            var fileName = "test.txt";
            var fileContent = "Hello, World!";
            File.WriteAllText(fileName, fileContent);

            _driver.Navigate().GoToUrl($"{_baseUrl}/File/receive_file");
            var fileInput = _driver.FindElement(By.Name("file"));
            fileInput.SendKeys(fileName);
            _driver.FindElement(By.CssSelector("button[type=submit]")).Click();

            var result = _driver.FindElement(By.CssSelector("div.result")).Text;
            Assert.Equal($"File {fileName} received successfully", result);

            File.Delete(fileName);
        }

        [Fact]
        public void ReceiveFile_SaveFileToDisk()
        {
            var fileName = "test.txt";
            var fileContent = "Hello, World!";
            File.WriteAllText(fileName, fileContent);
            var expectedFileContent = File.ReadAllText(fileName);

            _driver.Navigate().GoToUrl($"{_baseUrl}/File/receive_file");
            var fileInput = _driver.FindElement(By.Name("file"));
            fileInput.SendKeys(fileName);
            _driver.FindElement(By.CssSelector("button[type=submit]")).Click();

            var savedFileName = _driver.FindElement(By.CssSelector("div.result span")).Text;
            var savedFileContent = File.ReadAllText(savedFileName);
            Assert.Equal(expectedFileContent, savedFileContent);

            File.Delete(fileName);
            File.Delete(savedFileName);
        }

        [Fact]
        public void ReceiveFile_ReturnErrorMessageForInvalidFile()
        {
            var fileName = "test.png";
            File.WriteAllBytes(fileName, new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });

            _driver.Navigate().GoToUrl($"{_baseUrl}/File/receive_file");
            var fileInput = _driver.FindElement(By.Name("file"));
            fileInput.SendKeys(fileName);
            _driver.FindElement(By.CssSelector("button[type=submit]")).Click();

            var result = _driver.FindElement(By.CssSelector("div.result")).Text;
            Assert.Equal("Error: Invalid file format", result);

            File.Delete(fileName);
        }

        ~FileControllerE2ETest()
        {
            _driver.Quit();
        }
    }
}
