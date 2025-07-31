using MafiaServer.Pages;
using MafiaServer.Pages.GamePlayLogic;
using MafiaServer.Pages.StartUpServices;
using Microsoft.Extensions.Logging.AzureAppServices;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddSingleton<RoomManager>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(_ => true)
            .AllowCredentials(); // SignalR needs this
    });
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole(); // 💡 This is what Azure captures
builder.Logging.AddDebug(); // 💡 This is what Azure captures
builder.Logging.AddAzureWebAppDiagnostics(); // 💡 This is what Azure captures
builder.Logging.SetMinimumLevel(LogLevel.Information);

var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

app.MapHub<GameHub>("/GameHub");

// app.MapHub<MafiaGamePlayServer>("/MafiaLobby");GameHub


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();