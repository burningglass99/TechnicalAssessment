using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TechnicalAssessment.Controllers;

namespace TechnicalAssessment
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseWebSockets();

            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
            };

            app.UseWebSockets(webSocketOptions);

            app.Use(async (context, next) =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync())
                    {
                        await Receive(context, webSocket);
                    }
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            });
            app.UseFileServer();
            
            app.UseRouting();

            app.UseAuthorization();
        }
        private async Task Receive(HttpContext context, WebSocket webSocket)
        {
            var buffer = new ArraySegment<byte>(new byte[2048]);
            WebSocketReceiveResult result;
            BoardController bc = new BoardController();
            do
            {                
                using (var ms = new MemoryStream())
                {
                    do
                    {
                        result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                        ms.Write(buffer.Array, buffer.Offset, result.Count);
                    } while (!result.EndOfMessage);

                    ms.Seek(0, SeekOrigin.Begin);
                    string jsonMessage, serverResponse;
                    using (var reader = new StreamReader(ms, Encoding.UTF8))
                        jsonMessage = await reader.ReadToEndAsync();
                    //Console.WriteLine(jsonMessage);
                    serverResponse = bc.RequestInterpretor(jsonMessage);
                    //Console.WriteLine(serverResponse);
                    if (serverResponse.Length > 0)
                    {
                        await Respond(context, webSocket, serverResponse);
                    }
                }               
            } while (!result.CloseStatus.HasValue);
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        private async Task Respond(HttpContext context, WebSocket webSocket, string data)
        {
            await webSocket.SendAsync(Encoding.UTF8.GetBytes(data), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
