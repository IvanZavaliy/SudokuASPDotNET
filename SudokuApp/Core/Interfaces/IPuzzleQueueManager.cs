using SudokuApp.Core.DTOs;
using SudokuApp.Core.Enums;

namespace SudokuApp.Core.Interfaces;

public interface IPuzzleQueueManager
{
    // Метод для API контролера: миттєво взяти гру
    bool TryDequeue(DifficultyLevel level, out SudokuGameDto game);
        
    // Метод для воркера: покласти гру в чергу
    void Enqueue(DifficultyLevel level, SudokuGameDto game);
        
    // Перевірка, чи треба догенеровувати
    int GetCount(DifficultyLevel level);
}