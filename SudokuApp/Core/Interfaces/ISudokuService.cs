using SudokuApp.Core.DTOs;
using SudokuApp.Core.Enums;

namespace SudokuApp.Core.Interfaces;

public interface ISudokuService
{
    /// <summary>
    /// Генерує нову гру на основі вибраного рівня складності.
    /// У майбутньому цей метод може звертатися до Background Pool замість прямої генерації[cite: 148].
    /// </summary>
    /// <param name="level">Рівень складності (Enum замість string для безпеки типів)</param>
    /// <returns>DTO з готовим пазлом та рішенням</returns>
    SudokuGameDto CreateNewGame(DifficultyLevel level);

    /// <summary>
    /// Метод для валідації ходу (для сценарію Server-Side Validation)[cite: 302].
    /// </summary>
    bool ValidateMove(int[][] currentBoard, int row, int col, int value);
}