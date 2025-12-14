using SudokuApp.Core.DTOs;
using SudokuApp.Core.Enums;

namespace SudokuApp.Core.Interfaces;

public interface ISudokuService
{
    SudokuGameDto CreateNewGame(DifficultyLevel level);

    bool ValidateMove(int[][] currentBoard, int row, int col, int value);
}