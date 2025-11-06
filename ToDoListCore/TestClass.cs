using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grpc.Net.Client;
using Greet;
using System;
using System.Threading.Tasks;

namespace ToDoListCore
{
    public class TestClass
    {
        public async Task<string> GetDataAsync()
        {
            //await Task.Delay(1000); // Simulate an async operation
            //return "Data from async method";
            using var channel = GrpcChannel.ForAddress("https://localhost:7123");
            var clientGrpc = new Greeter.GreeterClient(channel);
            var reply = await clientGrpc.SayHelloAsync(new HelloRequest { Name = "GreeterClient" });
            //Console.WriteLine($"Greeting: {reply.Message}");
            return reply.Message;
        }
    }
}
