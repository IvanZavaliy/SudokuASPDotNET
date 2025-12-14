using SudokuAppSudokuApp.Core.Factories;
using SudokuApp.Core.Interfaces;
using SudokuApp.Infrastructure.BackgroundServices;
using SudokuApp.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<DifficultyStrategyFactory>();

builder.Services.AddSingleton<IPuzzleQueueManager, PuzzleQueueManager>();

builder.Services.AddScoped<ISudokuService, SudokuService>();

builder.Services.AddHostedService<PuzzleGeneratorWorker>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();