using SudokuApp.Core.Enums;
using SudokuApp.Core.Interfaces;

namespace SudokuApp.Infrastructure.BackgroundServices;

public class PuzzleGeneratorWorker : BackgroundService
{
    private readonly IPuzzleQueueManager _queueManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PuzzleGeneratorWorker> _logger;

    private const int MIN_POOL_SIZE = 20; 
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

            foreach (DifficultyLevel level in Enum.GetValues(typeof(DifficultyLevel)))
            {
                int currentCount = _queueManager.GetCount(level);

                if (currentCount < MIN_POOL_SIZE)
                {
                    await GenerateBatchAsync(level, MAX_POOL_SIZE - currentCount, stoppingToken);
                    anyGenerated = true;
                }
            }

            int delay = anyGenerated ? 100 : 1000;
            await Task.Delay(delay, stoppingToken);
        }
    }

    private async Task GenerateBatchAsync(DifficultyLevel level, int countToGenerate, CancellationToken ct)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var sudokuService = scope.ServiceProvider.GetRequiredService<ISudokuService>();

            try 
            {
                var gameDto = sudokuService.CreateNewGame(level);
                _queueManager.Enqueue(level, gameDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating puzzle for level {level}");
            }
        }
        await Task.Yield(); 
    }
}