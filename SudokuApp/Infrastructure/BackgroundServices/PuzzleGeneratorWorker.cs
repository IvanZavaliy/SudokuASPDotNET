using SudokuApp.Core.Enums;
using SudokuApp.Core.Interfaces;

namespace SudokuApp.Infrastructure.BackgroundServices;

public class PuzzleGeneratorWorker : BackgroundService
{
    private readonly IPuzzleQueueManager _queueManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PuzzleGeneratorWorker> _logger;

    // Поріг, нижче якого починаємо генерацію [cite: 39]
    private const int MIN_POOL_SIZE = 20; 
    // Максимальний розмір пулу, щоб не забити всю пам'ять [cite: 41]
    private const int MAX_POOL_SIZE = 50; 

    public PuzzleGeneratorWorker(
        IPuzzleQueueManager queueManager, 
        IServiceProvider serviceProvider,
        ILogger<PuzzleGeneratorWorker> logger)
    {
        _queueManager = queueManager;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Puzzle Generator Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            bool anyGenerated = false;

            // Перевіряємо кожен рівень складності
            foreach (DifficultyLevel level in Enum.GetValues(typeof(DifficultyLevel)))
            {
                int currentCount = _queueManager.GetCount(level);

                // Сценарій [R2] Mitigation: Якщо пул падає < Threshold, запускаємо генерацію
                if (currentCount < MIN_POOL_SIZE)
                {
                    await GenerateBatchAsync(level, MAX_POOL_SIZE - currentCount, stoppingToken);
                    anyGenerated = true;
                }
            }

            // Якщо пули повні, спимо довше (1 секунда), щоб не вантажити CPU [cite: 74]
            // Якщо генерували - спимо менше (100 мс), щоб швидше перевірити знову
            int delay = anyGenerated ? 100 : 1000;
            await Task.Delay(delay, stoppingToken);
        }
    }

    private async Task GenerateBatchAsync(DifficultyLevel level, int countToGenerate, CancellationToken ct)
    {
        // ISudokuService зазвичай Scoped, а Worker - Singleton.
        // Тому створюємо новий Scope для отримання сервісу.
        using (var scope = _serviceProvider.CreateScope())
        {
            var sudokuService = scope.ServiceProvider.GetRequiredService<ISudokuService>();

            // Генеруємо по одній грі за ітерацію, щоб не блокувати потік надовго
            // В реальності можна генерувати пачками, але тут обережно з CPU
            try 
            {
                var gameDto = sudokuService.CreateNewGame(level);
                _queueManager.Enqueue(level, gameDto);
                    
                // _logger.LogDebug($"Generated {level} puzzle. Pool size: {_queueManager.GetCount(level)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating puzzle for level {level}");
            }
        }
            
        // Даємо CPU "видихнути" між генераціями, якщо це потрібно
        await Task.Yield(); 
    }
}