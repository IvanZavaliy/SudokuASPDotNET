using Microsoft.AspNetCore.Mvc;
using SudokuApp.Models;
using SudokuApp.Services;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly SudokuService _sudokuService;

        public HomeController(SudokuService sudokuService)
        {
            _sudokuService = sudokuService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Game(string playerName, string level)
        {
            var (puzzle, solution) = _sudokuService.GenerateSudoku(level);

            var model = new GameViewModel
            {
                PlayerName = playerName,
                Level = level,
                // Використовуємо наш метод конвертації
                PuzzleGrid = ToJaggedArray(puzzle),
                SolutionGrid = ToJaggedArray(solution)
            };

            return View(model);
        }

        // --- Допоміжний метод для конвертації [,] -> [][] ---
        private int[][] ToJaggedArray(int[,] twoDArray)
        {
            int rows = twoDArray.GetLength(0);
            int cols = twoDArray.GetLength(1);
            var jagged = new int[rows][];
            for (int i = 0; i < rows; i++)
            {
                jagged[i] = new int[cols];
                for (int j = 0; j < cols; j++)
                {
                    jagged[i][j] = twoDArray[i, j];
                }
            }
            return jagged;
        }
    }
}