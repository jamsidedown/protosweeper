using Microsoft.EntityFrameworkCore;
using Protosweeper.Web.Controllers;
using Protosweeper.Web.Data;
using Protosweeper.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.WebHost.UseKestrel(option => option.AddServerHeader = false);
builder.Services.AddGrpc();
builder.Services.AddRazorPages();
builder.Services.AddLogging();
builder.Services.AddAntiforgery();

builder.Services.AddTransient<GameService>();
builder.Services.AddTransient<GameRepository>();

var app = builder.Build();

app.MapGrpcService<GreeterService>();
app.MapGrpcService<PracticeService>();
app.MapGrpcService<PvpService>();

app.UseRouting();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.MapControllers();
app.UseWebSockets();

var shutdownTask = Task.Run(async () =>
{
    app.Lifetime.ApplicationStopping.WaitHandle.WaitOne();
    await WebsocketController.Shutdown();
});

app.Run();

await shutdownTask;
