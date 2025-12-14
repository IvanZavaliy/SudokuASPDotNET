using Microsoft.AspNetCore.Mvc;
using SudokuApp.Models;

namespace SudokuApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Game(string playerName, string level)
        {
            var model = new GameViewModel
            {
                PlayerName = playerName ?? "Player",
                Level = level,
                PuzzleGrid = null,
                SolutionGrid = null
            };

            return View(model);
        }
    }
}