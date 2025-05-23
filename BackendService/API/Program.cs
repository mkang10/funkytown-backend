using API.AppStarts;
using Application.Interfaces;
using Application.SignalR;
using Infrastructure.Clients;
using Infrastructure.HelperServices.Models;
using Infrastructure.HelperServices;
using Microsoft.AspNetCore.SignalR;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using API.Chathub;


var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.

//Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000",
            "http://localhost:5000",
            "http://localhost:1212",
            "https://ftown-client-test.vercel.app",
            "https://ftown-admin.vercel.app",
            "https://ftown-admin-dhww.vercel.app",
            "http://127.0.0.1:5500")

            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // N?u dùng cookie ho?c auth header

    });
});
var redisConfig = builder.Configuration.GetSection("Redis");
var redisConnection = $"{redisConfig["Host"]}:{redisConfig["Port"]},password={redisConfig["Password"]}";

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = redisConfig["InstanceName"];
});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisConfig = builder.Configuration.GetSection("Redis");
    var redisConnection = $"{redisConfig["Host"]}:{redisConfig["Port"]},password={redisConfig["Password"]}";

    var configuration = ConfigurationOptions.Parse(redisConnection, true);
    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddHttpClient<IInventoryServiceClient, InventoryServiceClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7261/api/");
});
// Add depen
builder.Services.InstallService(builder.Configuration);
builder.Services.AddControllers();
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(80); // Nếu bạn map cổng 80
});
//mail

builder.Services.Configure<EmailServiceDTO>(builder.Configuration.GetSection("Mailtrap"));

builder.Services.AddTransient<EmailService>();

builder.Services.AddSignalR()
    .AddHubOptions<NotificationHub>(options =>
    {
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    });
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthService API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Please enter a valid token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
    options.DocInclusionPredicate((docName, apiDesc) =>
    {
        var excludedPaths = new[]
        {
        "api/inventoryimport/create-from-excel",
        "api/inventoryimport/createtransfer-from-excel"
    };

        return !excludedPaths.Contains(apiDesc.RelativePath, StringComparer.OrdinalIgnoreCase);
    });

    //options.OperationFilter<FileResponseOperationFilter>();
    //options.OperationFilter<FileUploadOperationFilter>();

    options.MapType<DateOnly>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "date",
        Example = new OpenApiString("2024-11-20")
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AuthService API v1");
    c.RoutePrefix = "swagger";
});
app.UseCors("AllowSpecificOrigins");


app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<ChatHub>("/chathub");
    endpoints.MapHub<BotHub>("/bothub");
    endpoints.MapHub<NotificationHub>("/notificationHub");

});

app.Run();
