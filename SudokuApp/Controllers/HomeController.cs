using Microsoft.AspNetCore.Mvc;
using SudokuApp.Models;

namespace SudokuApp.Controllers
{
    public class HomeController : Controller
    {
        // Ніяких сервісів тут більше не потрібно.
        // Frontend сам постукає в API за даними.

        public IActionResult Index()
        {
            return View();
        }

        // Метод Game (POST) видаляємо, оскільки переходимо на API (GET /api/game)
        [HttpPost]
        public IActionResult Game(string playerName, string level)
        {
            // Ми не генеруємо тут поле! Це зробить JS через API.
            // Ми просто передаємо налаштування на сторінку гри.
            var model = new GameViewModel
            {
                PlayerName = playerName ?? "Player",
                Level = level,
                PuzzleGrid = null, // Поле буде пустим на цьому етапі
                SolutionGrid = null
            };

            return View(model);
        }
    }
}