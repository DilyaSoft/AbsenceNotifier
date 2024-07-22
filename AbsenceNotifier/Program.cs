using AbsenceNotifier.Core;
using AbsenceNotifier.Core.Configurations;
using Serilog.Events;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var configuration = builder.Configuration;

builder.Services.AddControllers().AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<SmtpConfiguration>(configuration.GetSection("SmtpConfiguration"));
builder.Services.Configure<ApplicationCommonConfiguration>(configuration.GetSection("ApplicationCommonConfiguration"));
builder.Services.Configure<RocketChatConfiguration>(configuration.GetSection("RocketChatConfiguration"));
builder.Services.Configure<YandexChatConfiguration>(configuration.GetSection("YandexChatConfiguration"));
builder.Services.AddWebApiServices(configuration);
builder.Services.AddControllersWithViews();
builder.Services.AddSwaggerGen();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(25);
});

Log.Logger = new LoggerConfiguration()
                     .MinimumLevel.Debug()
                     .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                     .MinimumLevel.Override("System", LogEventLevel.Warning)
                     .WriteTo.Console()
                     .WriteTo.File("WebApiLogs/AbsenceNotifier.WebApi.Log")
                      .CreateLogger();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseSession();

app.MapControllers();

app.Run();

#pragma warning disable S1118 // Utility classes should not have public constructors
public partial class Program { }
#pragma warning restore S1118 // Utility classes should not have public constructors
