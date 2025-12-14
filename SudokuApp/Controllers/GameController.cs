using Microsoft.AspNetCore.Mvc;
using SudokuApp.Core.DTOs;
using SudokuApp.Core.Enums;
using SudokuApp.Core.Interfaces;
using System;

namespace SudokuApp.Controllers;

[ApiController]
[Route("api/game")]
public class GameController : ControllerBase
{
    private readonly ISudokuService _sudokuService;
    private readonly IPuzzleQueueManager _queueManager;

    public GameController(ISudokuService sudokuService, IPuzzleQueueManager queueManager)
    {
        _sudokuService = sudokuService;
        _queueManager = queueManager;
    }

    [HttpGet("new/{level}")]
    public IActionResult GetNewGame(string level)
    {
        // 1. Парсимо рівень складності з URL
        if (!Enum.TryParse<DifficultyLevel>(level, true, out var difficulty))
        {
            return BadRequest($"Invalid difficulty level. Available: {string.Join(", ", Enum.GetNames(typeof(DifficultyLevel)))}");
        }

        SudokuGameDto gameDto;

        // 2. Сценарій [P4]: Спробуємо взяти готову гру з пулу (це дуже швидко - 0ms)
        if (_queueManager.TryDequeue(difficulty, out gameDto))
        {
            // Додаємо хедер, щоб клієнт знав, що це з кешу (для дебагу)
            Response.Headers.Add("X-Source", "Pool");
            return Ok(gameDto);
        }

        // 3. Fallback: Якщо пул порожній (сценарій Pool Exhaustion), генеруємо "на льоту".
        // Це повільніше, але гарантує, що користувач отримає гру.
        gameDto = _sudokuService.CreateNewGame(difficulty);
        Response.Headers.Add("X-Source", "OnDemand");
            
        return Ok(gameDto);
    }

    // Тут у майбутньому буде метод POST /move для сценарію [P-D1] (Delta Updates)
    /*
    [HttpPost("move")]
    public IActionResult MakeMove([FromBody] MoveDto move) { ... }
    */
}