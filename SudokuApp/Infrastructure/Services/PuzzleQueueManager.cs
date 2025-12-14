using System.Collections.Concurrent;
using System.Collections.Generic;
using SudokuApp.Core.DTOs;
using SudokuApp.Core.Enums;
using SudokuApp.Core.Interfaces;

namespace SudokuApp.Infrastructure.Services;

public class PuzzleQueueManager : IPuzzleQueueManager
{
    private readonly Dictionary<DifficultyLevel, ConcurrentQueue<SudokuGameDto>> _queues;

    public PuzzleQueueManager()
    {
        _queues = new Dictionary<DifficultyLevel, ConcurrentQueue<SudokuGameDto>>();

        foreach (DifficultyLevel level in System.Enum.GetValues(typeof(DifficultyLevel)))
        {
            _queues[level] = new ConcurrentQueue<SudokuGameDto>();
        }
    }

    public bool TryDequeue(DifficultyLevel level, out SudokuGameDto game)
    {
        if (_queues.ContainsKey(level))
        {
            return _queues[level].TryDequeue(out game);
        }
        game = null;
        return false;
    }

    public void Enqueue(DifficultyLevel level, SudokuGameDto game)
    {
        if (_queues.ContainsKey(level))
        {
            _queues[level].Enqueue(game);
        }
    }

    public int GetCount(DifficultyLevel level)
    {
        return _queues.ContainsKey(level) ? _queues[level].Count : 0;
    }
}