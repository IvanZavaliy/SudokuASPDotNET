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
        if (!Enum.TryParse<DifficultyLevel>(level, true, out var difficulty))
        {
            return BadRequest($"Invalid difficulty level. Available: {string.Join(", ", Enum.GetNames(typeof(DifficultyLevel)))}");
        }

        SudokuGameDto gameDto;

        if (_queueManager.TryDequeue(difficulty, out gameDto))
        {
            Response.Headers.Add("X-Source", "Pool");
            return Ok(gameDto);
        }

        gameDto = _sudokuService.CreateNewGame(difficulty);
        Response.Headers.Add("X-Source", "OnDemand");
            
        return Ok(gameDto);
    }
}