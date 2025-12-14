using SudokuAppSudokuApp.Core.Factories;
using SudokuApp.Core.Interfaces;
using SudokuApp.Infrastructure.BackgroundServices;
using SudokuApp.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// --- SUDOKU SERVICES REGISTRATION (ВАЖЛИВО) ---

// 1. Фабрика стратегій (Single, бо вона не має стану)
builder.Services.AddSingleton<DifficultyStrategyFactory>();

// 2. Черга пазлів (Single, бо вона зберігає стан для всіх гравців)
builder.Services.AddSingleton<IPuzzleQueueManager, PuzzleQueueManager>();

// 3. Сервіс генерації (Scoped або Transient)
builder.Services.AddScoped<ISudokuService, SudokuService>();

// 4. Фоновий воркер (Hosted Service) - запускає генерацію у фоні
builder.Services.AddHostedService<PuzzleGeneratorWorker>();

// ----------------------------------------------

var app = builder.Build();

// Configure the HTTP request pipeline.
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