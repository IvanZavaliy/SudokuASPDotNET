namespace SudokuApp.Models
{
    public class GameViewModel
    {
        public string PlayerName { get; set; }
        public string Level { get; set; }
        
        // Змінили коми на подвійні дужки [][]
        public int[][] PuzzleGrid { get; set; } 
        public int[][] SolutionGrid { get; set; } 
    }
}