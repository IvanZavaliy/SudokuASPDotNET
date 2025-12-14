namespace SudokuApp.Core.DTOs;

public class SudokuGameDto
{
    public int[][] PuzzleGrid { get; set; }
    public int[][] SolutionGrid { get; set; }
    public string LevelName { get; set; }
    public string GameId { get; set; }
}