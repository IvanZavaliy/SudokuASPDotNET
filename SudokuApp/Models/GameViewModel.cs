namespace SudokuApp.Models
{
    public class GameViewModel
    {
        public string PlayerName { get; set; }
        public string Level { get; set; }
        public int[][] PuzzleGrid { get; set; } 
        public int[][] SolutionGrid { get; set; } 
    }
}