using Greet;
using Grpc.Core;
using Grpc.Net.Client;
using IdentityModel.Client;
using IdentityServer8.Models;
using Newtonsoft.Json.Linq;
using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using System.Xml.Linq;
using Todolist;

namespace ToDoListCore
{
    public class GrcpClientHelper
    {
        string _webApiUrl = @"https://localhost:7123";
        string _jwtToken = String.Empty;

        //public GrcpClientHelper() 
        //{
        //    _jwtToken = Task.Run(() => GetJwtToken()).Result;
        //}

        public string WebApiUrl 
        {
            get { return _webApiUrl; } 
            set { _webApiUrl = value; } 
        }

        public string JwtToken
        {
            get { return _jwtToken; }
            set { _jwtToken = value; }
        }

        public async Task<string> GetDataAsync()
        {
            using var channel = GrpcChannel.ForAddress(this.WebApiUrl);
            var clientGrpc = new Greeter.GreeterClient(channel);
            var reply = await clientGrpc.SayHelloAsync(new HelloRequest { Name = "GreeterClient" });
            return reply.Message;
        }

        public async Task<string> TestAsync()
        {
            using var channel = GrpcChannel.ForAddress(this.WebApiUrl);
            var clientGrpc = new Todolist.ToDoListGRPC.ToDoListGRPCClient(channel);
            var reply = await clientGrpc.TestAsync(new Dummy { });
            return reply.Title;
        }

        public async Task<string> TestAuthAsync()
        {
            //var jwtToken = await GetJwtToken();
            var headers = new Metadata();
            headers.Add("Authorization", $"Bearer {this.JwtToken}");

            using var channel = GrpcChannel.ForAddress(this.WebApiUrl);
            var clientGrpc = new Todolist.ToDoListGRPC.ToDoListGRPCClient(channel);
            var reply = await clientGrpc.TestAuthAsync(new Dummy { }, headers);
            return reply.Title;
        }

        public async Task<string> GetJwtToken()
        {
            string jwtToken = String.Empty;
            using (var client = new HttpClient())
            {
                var identityServerResponse = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
                {
                    Address = this.WebApiUrl + @"/connect/token",
                    ClientId = "todolist_client",
                    UserName = "admin", // 
                    Password = "Password-123"  //.ToSha256()
                });

                jwtToken = identityServerResponse.AccessToken;
            }

            return jwtToken;
        }


        public async Task<ToDoItemsResponse> GetToDoItemsAsync()
        {
            //var jwtToken = await GetJwtToken();
            var headers = new Metadata();
            headers.Add("Authorization", $"Bearer {this.JwtToken}");

            using var channel = GrpcChannel.ForAddress(this.WebApiUrl);
            var clientGrpc = new Todolist.ToDoListGRPC.ToDoListGRPCClient(channel);
            var response = await clientGrpc.GetToDoListAsync(new Dummy { }, headers);
            return response;
        }
    }
}
