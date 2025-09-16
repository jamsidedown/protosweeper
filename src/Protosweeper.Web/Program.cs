using Protosweeper.Web.Controllers;
using Protosweeper.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel(option => option.AddServerHeader = false);
builder.Services.AddGrpc();
builder.Services.AddRazorPages();
builder.Services.AddLogging();

builder.Services.AddTransient<GameService>();

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
