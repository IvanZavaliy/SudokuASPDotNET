namespace SudokuApp.Core.DTOs;

// Використовуємо цей клас для передачі даних між Сервісом та Контролером/API.
// Використання DTO розриває зв'язок з ViewModels.
public class SudokuGameDto
{
    // Використовуємо jagged array [][] замість [,], 
    // оскільки він нативно серіалізується в JSON без додаткових конвертерів.
    public int[][] PuzzleGrid { get; set; }
    public int[][] SolutionGrid { get; set; }
    public string LevelName { get; set; }
    public string GameId { get; set; } // Корисно для валідації ходів на сервері (Optimistic UI context)
}