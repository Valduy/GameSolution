using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Connectors.MatchmakerConnectors;
using Network;
using Network.Messages;

namespace FakeMatchmakerConnection
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var connector1 = new MatchmakerConnector();
            var httpClient1 = new HttpClient();
            var json1 = JsonSerializer.Serialize(new {Login = "Ivanson", Password = "12345678"});
            var result1 = await httpClient1.PostAsync(
                "http://localhost:5000/api/account/authorisation",
                new StringContent(json1, Encoding.UTF8, "application/json"));
            var token1 = JsonSerializer.Deserialize<TokenMessage>(await result1.Content.ReadAsStringAsync());
            var endPoint1 = new ClientEndPoint(NetworkHelper.GetLocalIPAddress(), 4321);
            var connectionTask1 = Task.Run(async ()
                => await connector1.ConnectAsync(endPoint1, "http://localhost:5000", token1.accessToken));

            var connector2 = new MatchmakerConnector();
            var httpClient2 = new HttpClient();
            var json2 = JsonSerializer.Serialize(new {Login = "Nosnavi", Password = "12345678"});
            var result2 = await httpClient2.PostAsync(
                "http://localhost:5000/api/account/authorisation",
                new StringContent(json2, Encoding.UTF8, "application/json"));
            var token2 = JsonSerializer.Deserialize<TokenMessage>(await result2.Content.ReadAsStringAsync());
            var endPoint2 = new ClientEndPoint(NetworkHelper.GetLocalIPAddress(), 4322);
            var connectionTask2 = Task.Run(async () 
                => await connector2.ConnectAsync(endPoint2, "http://localhost:5000", token2.accessToken));

            var port1 = await connectionTask1;
            var port2 = await connectionTask2;

            Console.ReadKey();
        }
    }
}
