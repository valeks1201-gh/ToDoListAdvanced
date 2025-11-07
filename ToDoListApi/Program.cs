using DAL;
using DAL.Core;
using DAL.Core.Interfaces;
using DAL.Models;
using ToDoListApi;

//using IdentityServer8.AccessTokenValidation;
using System.IdentityModel.Tokens.Jwt;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using ToDoListApi.Authorization;
using ToDoListApi.Helpers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using AppPermissions = DAL.Core.ApplicationPermissions;
using ToDoListApi.Interfaces;
using ToDoListApi.Services;

using Microsoft.AspNetCore.Mvc;
using Newtonsoft;
using Newtonsoft.Json;
using ToDoListApi.Middleware;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;

using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using static IdentityModel.ClaimComparer;
using ToDoListCore;

using Microsoft.Extensions.DependencyInjection;
using static Org.BouncyCastle.Math.EC.ECCurve;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Diagnostics;
using System.Runtime.InteropServices;



//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();


var builder = WebApplication.CreateBuilder(args);




var isRunningInProcessIIS = CoreUtilities.IsRunningInProcessIIS();
var aspNetCoreUrl = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
//builder.WebHost.UseUrls(urls: new string[] { "http://*:5061", "https://*:7061" }); // https://localhost:7061/ - open in browser
if (!(isRunningInProcessIIS) && (aspNetCoreUrl == null))
{
    var currentProjectUrl = CoreUtilities.GetCurrentProjectUrl(Configuration.AppSettings).TrimEnd('/');
    //builder.WebHost.UseUrls(urls: new string[] { "https://*:7124" }); // https://localhost:7124/ - open in browser 
    builder.WebHost.UseUrls(urls: new string[] { currentProjectUrl.Replace("localhost", "*") }); // https://localhost:7124/ - open in browser 
}


AddServices(builder);// Add services to the container.
var app = builder.Build();
ConfigureRequestPipeline(app); // Configure the HTTP request pipeline.
SeedDatabase(app); //Seed initial database
app.Run();


static void AddServices(WebApplicationBuilder builder)
{
    builder.Services.AddGrpc(); //GRPC

    string connectionString;
    var authServerUrl = CoreUtilities.GetCurrentProjectUrl(Configuration.AppSettings).TrimEnd('/');
   // authServerUrl = @"https://localhost:7124";

    string migrationsAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name; 
    var useMSSQL = builder.Configuration.GetSection("UseMSSQL").Value;

    if ((useMSSQL != null) && (bool.Parse(useMSSQL)))
    {
        connectionString = builder.Configuration.GetConnectionString("ApiDatabase_MSSQL") ??
                   throw new InvalidOperationException("Connection string 'ApiDatabase_MSSQL' not found.");

        //builder.Services.AddDbContext<ApplicationDbContext>(options =>
        //options.UseSqlServer(connectionString, b => b.MigrationsAssembly(migrationsAssembly)));

        //https://github.com/dotnet/efcore/issues/31323
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString, b => { b.UseCompatibilityLevel(120); b.MigrationsAssembly(migrationsAssembly); } ));
    }
    else
    {
        connectionString = builder.Configuration.GetConnectionString("ApiDatabase_PostgreSQL") ??
                   throw new InvalidOperationException("Connection string 'ApiDatabase_PostgreSQL' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString, b => b.MigrationsAssembly(migrationsAssembly)));

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    // add identity
    builder.Services.AddIdentity<Account, ApplicationRole>()
         .AddApiEndpoints()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

   

    // Configure Identity options and password complexity here
    builder.Services.Configure<IdentityOptions>(options =>
    {
        // User settings
        options.User.RequireUniqueEmail = true;

        //// Password settings
        //options.Password.RequireDigit = true;
        //options.Password.RequiredLength = 8;
        //options.Password.RequireNonAlphanumeric = false;
        //options.Password.RequireUppercase = true;
        //options.Password.RequireLowercase = false;

        //// Lockout settings
        //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
        //options.Lockout.MaxFailedAccessAttempts = 10;
    });

    // Adds IdentityServer
    X509Certificate2 rsaCertificate;
    rsaCertificate = new X509Certificate2(Path.Combine(builder.Environment.ContentRootPath, "Certificates", "todolistapi.pfx"), "1234");


  
    //builder.Services.AddIdentityServer(o =>
    //{
    //    o.IssuerUri = authServerUrl;
       
    //})
    //  // See http://docs.identityserver.io/en/release/topics/crypto.html#refcrypto for more information.
    //  .AddSigningCredential(rsaCertificate) // rsaCertificate

    //  .AddInMemoryPersistedGrants()
    //  // To configure IdentityServer to use EntityFramework (EF) as the storage mechanism for configuration data (rather than using the in-memory implementations),
    //  // see https://identityserver4.readthedocs.io/en/release/quickstarts/8_entity_framework.html
    //  .AddInMemoryIdentityResources(IdentityServerConfig.GetIdentityResources())
    //  .AddInMemoryApiScopes(IdentityServerConfig.GetApiScopes())
    //  .AddInMemoryApiResources(IdentityServerConfig.GetApiResources())
    //  .AddInMemoryClients(IdentityServerConfig.GetClients())
    //  .AddAspNetIdentity<Account>()
    //  .AddProfileService<ProfileService>();



    builder.Services.AddIdentityServer(options =>
    {
        options.IssuerUri = authServerUrl;

        options.Events.RaiseErrorEvents = true;
        options.Events.RaiseInformationEvents = true;
        options.Events.RaiseFailureEvents = true;
        options.Events.RaiseSuccessEvents = true;

        // see https://IdentityServer8.readthedocs.io/en/latest/topics/resources.html
        options.EmitStaticAudienceClaim = true;
    })
        //.AddInMemoryIdentityResources(IdentityServerConfig.GetIdentityResources())
        //.AddInMemoryApiScopes(IdentityServerConfig.GetApiScopes())
        //.AddInMemoryClients(IdentityServerConfig.GetClients())

        .AddConfigurationStore(options =>
        {
            options.ConfigureDbContext = builder =>
                builder.UseNpgsql(connectionString, b => b.MigrationsAssembly(migrationsAssembly));
        })
        .AddOperationalStore(options =>
        {
            options.ConfigureDbContext = builder =>
                builder.UseNpgsql(connectionString, b => b.MigrationsAssembly(migrationsAssembly));
            // this enables automatic token cleanup. this is optional. 
            options.EnableTokenCleanup = true;
            options.TokenCleanupInterval = 30;
        })


        .AddAspNetIdentity<Account>()
        .AddProfileService<ProfileService>()
        // not recommended for production - you need to store your key material somewhere secure
        .AddDeveloperSigningCredential();





    //  builder.Services.AddAuthentication(o =>
    //  {
    //      o.DefaultScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
    //      o.DefaultAuthenticateScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
    //      o.DefaultChallengeScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
    //  })
    //.AddIdentityServerAuthentication(options =>
    // {
    //     options.Authority = authServerUrl;
    //     options.RequireHttpsMetadata = false; // Note: Set to true in production
    //     options.ApiName = IdentityServerConfig.ApiName;
    // });




    //JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
    //builder.Services.AddAuthentication(options =>
    //{
    //    options.DefaultScheme = "Cookies";
    //    options.DefaultChallengeScheme = "oidc";
    //})
    //    .AddCookie("Cookies")
    //    .AddOpenIdConnect("oidc", options =>
    //    {
    //        options.Authority = "https://localhost:7123";
    //        options.ClientId = "swaggerui"; // "mvc";
    //        //options.ClientSecret = "secret";
    //        //options.ResponseType = "code";
    //        options.SaveTokens = true;
    //    });








    builder.Services
        .AddAuthorization(options =>
    {
        options.AddPolicy(ToDoListApi.Authorization.Policies.ViewAllUsersPolicy, policy => policy.RequireClaim(ClaimConstants.Permission, AppPermissions.ViewUsers));
        options.AddPolicy(ToDoListApi.Authorization.Policies.ManageAllUsersPolicy, policy => policy.RequireClaim(ClaimConstants.Permission, AppPermissions.ManageUsers));

        options.AddPolicy(ToDoListApi.Authorization.Policies.ViewAllRolesPolicy, policy => policy.RequireClaim(ClaimConstants.Permission, AppPermissions.ViewRoles));
        options.AddPolicy(ToDoListApi.Authorization.Policies.ViewRoleByRoleNamePolicy, policy => policy.Requirements.Add(new ViewRoleAuthorizationRequirement()));
        options.AddPolicy(ToDoListApi.Authorization.Policies.ManageAllRolesPolicy, policy => policy.RequireClaim(ClaimConstants.Permission, AppPermissions.ManageRoles));

        options.AddPolicy(ToDoListApi.Authorization.Policies.AssignAllowedRolesPolicy, policy => policy.Requirements.Add(new AssignRolesAuthorizationRequirement()));

        options.AddPolicy(ToDoListApi.Authorization.Policies.ViewOrgDataPolicy, policy => policy.RequireClaim(ClaimConstants.Permission, AppPermissions.ViewOrgData));
        options.AddPolicy(ToDoListApi.Authorization.Policies.ManageOrgDataPolicy, policy => policy.RequireClaim(ClaimConstants.Permission, AppPermissions.ManageOrgData));

        // accepts any access token issued by identity server
        // adds an authorization policy for scope 'todolist_api'
        options.AddPolicy("ApiScope", policy =>
        {
            policy
                .RequireAuthenticatedUser()
                .RequireClaim("scope", "todolist_api");
        });

        //options.AddPolicy("ApiScope", policy =>
        //{
        //    policy
        //        .RequireAuthenticatedUser()
        //        .RequireClaim("scope", "swaggerui");
        //});

        //options.AddPolicy("ApiScope", policy =>
        //{
        //    policy
        //        .RequireAuthenticatedUser()
        //        .RequireClaim("scope", "todolist_client");
        //});
    })
         .AddCors(options =>
         {
             // this defines a CORS policy called "default"
             options.AddPolicy("default", policy =>
             {
                 policy.WithOrigins(authServerUrl)
                // policy.WithOrigins("https://localhost:7123") //authServerUrl
                     //   policy.WithOrigins("https://localhost:5001")
                     .AllowAnyHeader()
                     .AllowAnyMethod();
             });
         })
        .AddControllers()
        .AddJsonOptions(x =>
          //x.JsonSerializerOptions.IgnoreNullValues = true
         x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())
        );



    // accepts any access token issued by identity server
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer("Bearer", options =>
        {
            options.Authority = authServerUrl;
           // options.Authority = "https://localhost:7123";
            options.TokenValidationParameters =
                new() { ValidateAudience = false };
        });

    //builder.Services.AddAuthentication()
    //        .AddJwtBearer(options =>
    //        {
    //            options.Authority = "https://localhost:7123";
    //           // options.Audience = "todolist_api";
    //        });

    //builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    //        .AddJwtBearer(options =>
    //        {
    //            // base-address of your identityserver
    //            options.Authority = "https://localhost:7123";

    //            // if you are using API resources, you can specify the name here
    //            //options.Audience = "resource1";

    //            // IdentityServer emits a typ header by default, recommended extra check
    //            options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
    //        });

    // builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);

    //builder.Services.AddAuthentication()
    //        .AddJwtBearer(options =>
    //        {
    //            options.Authority = "https://localhost:7123";
    //            options.Audience = "swaggerui";
    //        });

    //Add NewtonsoftJson
    builder.Services.AddMvcCore(options =>
    {
        options.Filters.Add(new ApiControllerAttribute());
        options.EnableEndpointRouting = false;
    })
      .AddNewtonsoftJson();

    //builder.Services.AddControllers().AddJsonOptions(x =>
    //   //x.JsonSerializerOptions.IgnoreNullValues = true
    //   x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())
    // );
    builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

    // Add CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("default", policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:3000"
                    )
                .AllowAnyHeader()
                .AllowAnyMethod()
                ;
        });
    });

    //builder.Services.AddControllersWithViews();
    builder.Services.AddMvc();

    builder.Services.AddSwaggerGen(c =>
    {
       c.SwaggerDoc("v2", new OpenApiInfo { Title = IdentityServerConfig.ApiFriendlyName, Version = "v2" });
        c.OperationFilter<AuthorizeCheckOperationFilter>();
        c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                Password = new OpenApiOAuthFlow
                {
                    TokenUrl = new Uri("/connect/token", UriKind.Relative),
                    Scopes = new Dictionary<string, string>()
                            {
                                { IdentityServerConfig.ApiName, IdentityServerConfig.ApiFriendlyName }
                            }
                }
            }
        });
        c.SchemaFilter<EnumSchemaFilter>();
        c.IncludeXmlComments(Path.Combine(System.AppContext.BaseDirectory, "ToDoListApi.xml"));
    });

    //builder.Services.AddAutoMapper(typeof(Program));

    // Configurations
    builder.Services.Configure<AppSettings>(builder.Configuration);

    // Business Services
    builder.Services.AddScoped<ToDoListApi.Helpers.IEmailSender, ToDoListApi.Helpers.EmailSender>();

    // Repositories
    builder.Services.AddScoped<IAccountManager, AccountManager>();

    // ToDoList
    builder.Services.AddScoped<IToDoListService, ToDoListService>();

    // Auth Handlers
    builder.Services.AddSingleton<IAuthorizationHandler, ViewUserAuthorizationHandler>();
    builder.Services.AddSingleton<IAuthorizationHandler, ManageUserAuthorizationHandler>();
    builder.Services.AddSingleton<IAuthorizationHandler, ViewRoleAuthorizationHandler>();
    builder.Services.AddSingleton<IAuthorizationHandler, AssignRolesAuthorizationHandler>();

    builder.Services.AddSingleton<IAuthorizationHandler, ViewOrgDataAuthorizationHandler>();
    builder.Services.AddSingleton<IAuthorizationHandler, ManageOrgDataAuthorizationHandler>();

    // DB Creation and Seeding
    builder.Services.AddTransient<IDatabaseInitializer, DatabaseInitializer>();

    //File Logger 
    builder.Logging.AddFile(builder.Configuration.GetSection("Logging"));

    //Email Templates
    EmailTemplates.Initialize(builder.Environment);

}

static void ConfigureRequestPipeline(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        IdentityModelEventSource.ShowPII = true;
    }
    else
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }



    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseCors(builder => builder
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());

    app.UseIdentityServer(); //Slava
    app.UseAuthorization();



    app.MapDefaultControllerRoute();

    // Map gRPC services.
    app.MapGrpcService<GreeterImpl>();
    app.MapGrpcService<ToDoListGRPCImpl>();

    //app.MapGet("/account", (HttpContext context) =>
    //       new JsonResult(context?.User?.Claims.Select(c => new { c.Type, c.Value }))
    //   ).RequireAuthorization("ApiScope");

    //app.MapGet("/todolist", (HttpContext context) =>
    //      new JsonResult(context?.User?.Claims.Select(c => new { c.Type, c.Value }))
    //  ).RequireAuthorization("ApiScope");



    /////////////////

    //app.UseSwagger();
    app.UseSwagger(options => options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0);
    app.UseSwaggerUI(c =>
    {
        c.DocumentTitle = "Swagger UI - ToDoListApi";
        c.SwaggerEndpoint("../swagger/v2/swagger.json", $"{IdentityServerConfig.ApiFriendlyName} V2");
        c.OAuthClientId(IdentityServerConfig.SwaggerClientID);
        //c.OAuthClientId(IdentityServerConfig.ToDoListClientID);
        c.OAuthClientSecret("no_password"); //Leaving it blank doesn't work

       
    });

   

    // global error handler
    app.UseMiddleware<ErrorHandlerMiddleware>();

    // custom jwt auth middleware 
    app.UseMiddleware<JwtMiddleware>();

    app.UseEndpoints(x => x.MapControllers());

    //app.UseEndpoints(endpoints =>
    //{
    //    endpoints.MapControllerRoute(
    //        name: "default",
    //        pattern: "{controller}/{action=Index}/{id?}");

    //    endpoints.Map("api/{**slug}", context =>
    //    {
    //        context.Response.StatusCode = StatusCodes.Status404NotFound;
    //        return Task.CompletedTask;
    //    });

    //    endpoints.MapFallbackToFile("index.html");
    //});


}

static void SeedDatabase(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;

        try
        {
            var databaseInitializer = services.GetRequiredService<IDatabaseInitializer>();
            databaseInitializer.SeedAsync().Wait();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogCritical(LoggingEvents.INIT_DATABASE, ex, LoggingEvents.INIT_DATABASE.Name);

            throw new Exception(LoggingEvents.INIT_DATABASE.Name, ex);
        }
    }
}




