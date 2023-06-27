using MyConversation.Helper;
using MyConversation.Middleware;
using MyConversation.SignalR;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

InitHandler.Init();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddCors(options => options.AddPolicy(MyAllowSpecificOrigins,
        builder =>
        {
            builder
            //.WithOrigins(new[] { "https://localhost:7192","*" })
            .SetIsOriginAllowed(origin => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        }));

#region serilog
bool isNotInfoLevel(LogEvent le)
{
    return le.Level == LogEventLevel.Error 
        || le.Level == LogEventLevel.Warning
        || le.Level == LogEventLevel.Information;
}
builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .WriteTo.File("Log/log-.txt", rollingInterval: RollingInterval.Day)
    .MinimumLevel.Debug()
    .Filter.ByIncludingOnly(isNotInfoLevel)
    );
#endregion

var app = builder.Build();
// Configure the HTTP request pipeline.
app.UseCors(MyAllowSpecificOrigins);
app.MapHub<ConversationHub>("/conversationHub");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseMiddleware<JwtMiddleware>();

app.MapControllers();
app.Run();
