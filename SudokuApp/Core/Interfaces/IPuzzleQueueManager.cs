using SudokuApp.Core.DTOs;
using SudokuApp.Core.Enums;

namespace SudokuApp.Core.Interfaces;

public interface IPuzzleQueueManager
{
    bool TryDequeue(DifficultyLevel level, out SudokuGameDto game);

    void Enqueue(DifficultyLevel level, SudokuGameDto game);

    int GetCount(DifficultyLevel level);
}