using Microsoft.AspNetCore.Hosting;
using System.Diagnostics;
using ToDoListBlazor;
using ToDoListBlazor.Components;
using ToDoListCore;

var builder = WebApplication.CreateBuilder(args);

var currentProjectUrl = CoreUtilities.GetCurrentProjectUrl(Configuration.AppSettings).TrimEnd('/');
var webApiUrl = CoreUtilities.GetWebApiUrl(Configuration.AppSettings).TrimEnd('/');

var isRunningInProcessIIS = CoreUtilities.IsRunningInProcessIIS();
var aspNetCoreUrl = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");

//builder.WebHost.UseUrls(urls: new string[] { "http://*:5061", "https://*:7061" }); // https://localhost:7061/ - open in browser
if (!(isRunningInProcessIIS) && (aspNetCoreUrl == null))
{
    //currentProjectUrl = CoreUtilities.GetCurrentProjectUrl(Configuration.AppSettings).TrimEnd('/');
    //builder.WebHost.UseUrls(urls: new string[] { "https://*:7124" }); // https://localhost:7124/ - open in browser 
    currentProjectUrl = currentProjectUrl.Replace("localhost", "*");
  //  builder.WebHost.UseUrls(urls: new string[] { currentProjectUrl.Replace("localhost", "*") }); // https://localhost:7124/ - open in browser 
}

Trace.WriteLine("currentProjectUrl=" + currentProjectUrl);
Trace.WriteLine("webApiUrl=" + webApiUrl);
builder.WebHost.UseUrls(urls: new string[] { currentProjectUrl});
Configuration.CurrentProjectUrl = currentProjectUrl;
Configuration.WebApiUrl = webApiUrl;


//builder.WebHost.UseUrls(urls: new string[] { "http://*:5060", "https://*:7060" }); // https://localhost:7060/ - open in browser 

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();


