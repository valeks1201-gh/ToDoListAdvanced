using DAL.Models;
using IdentityModel.Client;
//using ToDoListApi.Models;
//using DAL.Models.NotMapped.API;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using ToDoListApi.ViewModels;
using ToDoListCore.Models;

namespace TestClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        async private void button1_Click(object sender, EventArgs e)
        {
            string jwtToken;
            using (var client = new HttpClient())
            {
                var identityServerResponse = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
                {
                    Address = "https://localhost:7123/connect/token",
                    ClientId = "todolist_client",
                   // ClientId = "todolist_client",
                    //  ClientId = "markSoft_blazorClient",

                    //UserName = "Admin",
                    //Password = "Password-123"  //.ToSha256()

                    //UserName = "managerAdvanced11", // 
                    //Password = "Password-123"  //.ToSha256()

                    UserName = "admin", // 
                    Password = "Password-123"  //.ToSha256()

                    //UserName = "managerCustomer1", // 
                    //Password = "Password-123"  //.ToSha256()

                    //UserName = "viewerCustomer1",
                    //Password = "Password-123"  //.ToSha256()
                });

                jwtToken = identityServerResponse.AccessToken;

                //Get authorized user's claims
                JwtSecurityToken token = new JwtSecurityToken(jwtToken);
                var claims = token.Claims;
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7123/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", textBox1.Text);
               // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                client.SetBearerToken(jwtToken);

                // New code:
                //HttpResponseMessage response = await client.GetAsync("api/Account/users");
                var response = await client.GetAsync("https://localhost:7123/api/Account/users");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    //var options = new JsonSerializerOptions
                    //{
                    //    PropertyNameCaseInsensitive = true,
                    //};
                    //options.Converters.Add(new JsonStringEnumConverter());
                    //var users = System.Text.Json.JsonSerializer.Deserialize<List<UserViewModel>>(json, options);
                    var users = JsonConvert.DeserializeObject<List<UserViewModel>>(json);

                }

                // 
                var response2 = await client.GetAsync("https://localhost:7123/api/Account/users/me");

                if (response2.IsSuccessStatusCode)
                {
                    var json = await response2.Content.ReadAsStringAsync();
                    var userMe = JsonConvert.DeserializeObject<UserViewModel>(json);

                }

                // 
                var response3 = await client.GetAsync("https://localhost:7123/api/ToDoList/ToDoItems"); 

                if (response3.IsSuccessStatusCode)
                {
                    var json = await response3.Content.ReadAsStringAsync();
                    var toDoItems = JsonConvert.DeserializeObject<List<ToDoItemResponse>>(json);
                }
            }
        }
    }
}
