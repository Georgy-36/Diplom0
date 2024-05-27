using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace Diplom0._1
{
    public class Publication
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Number { get; set; }
        public Guid AgentId { get; set; }
    }

    public class Connection
    {
        public Guid Id { get; set; }
        public Guid ProxyAgentId { get; set; }
        public Agent Agent { get; set; }
    }

    public class EndPoint
    {
        public Guid Id { get; set; }
        public string Address { get; set; }
        public string CommandName { get; set; }
        public Guid ProxyAgentId { get; set; }
    }

    public class ProxyAgent
    {
        public Guid Id { get; set; }
        public string ConnectionId { get; set; }
        public List<EndPoint> Endpoints { get; set; }
    }

    public class Agent
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<Connection> Connections { get; set; } = new List<Connection>();
        public List<Publication> Publications { get; set; } = new List<Publication>();
        public List<ProxyAgent> ProxyAgents { get; set; } = new List<ProxyAgent>();

        private static readonly HttpClient client = new HttpClient();

        public void AddConnection(Connection connection)
        {
            Connections.Add(connection);
        }

        public void AddPublication(Publication publication)
        {
            Publications.Add(publication);
        }

        public void AddProxyAgent(ProxyAgent proxyAgent)
        {
            ProxyAgents.Add(proxyAgent);
        }

        public async Task<string> CallProxy(string proxyName, string functionName, object data)
        {
            var proxyAgent = ProxyAgents.FirstOrDefault(pa => pa.ConnectionId == proxyName);
            if (proxyAgent != null)
            {
                var endpoint = proxyAgent.Endpoints.FirstOrDefault(ep => ep.CommandName == functionName);
                if (endpoint != null)
                {
                    var url = $"{endpoint.Address}/{functionName}";
                    var jsonContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(url, jsonContent);
                    return await response.Content.ReadAsStringAsync();
                }
            }
            throw new Exception($"Proxy {proxyName} or function {functionName} not found");
        }

        public async Task<string> SendFile(string proxyName, string fileName, byte[] fileContent)
        {
            var encodedFile = Convert.ToBase64String(fileContent);
            var data = new
            {
                FileName = fileName,
                FileContent = encodedFile
            };
            return await CallProxy(proxyName, "receive_file", data);
        }

        public string ReceiveFile(string fileName, string fileContent)
        {
            var decodedFile = Convert.FromBase64String(fileContent);
            File.WriteAllBytes(fileName, decodedFile);
            return $"File {fileName} received successfully";
        }
    }

    public class A1 : Agent
    {
        public A1()
        {
            Id = Guid.NewGuid();
            Name = "A1";
        }

        public string In() => "A1 in function called";
        public string Out() => "A1 out function called";
    }

    public class A2 : Agent
    {
        public A2()
        {
            Id = Guid.NewGuid();
            Name = "A2";
        }

        public string In() => "A2 in function called";
        public string Out() => "A2 out function called";
    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
