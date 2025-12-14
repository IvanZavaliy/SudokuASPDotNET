using System;
using System.Collections.Generic;
using SudokuApp.Core.Enums;
using SudokuApp.Core.Interfaces;
using SudokuApp.Core.Strategies;

namespace SudokuAppSudokuApp.Core.Factories;

public class DifficultyStrategyFactory
{
    private readonly Dictionary<DifficultyLevel, IDifficultyStrategy> _strategies;

    public DifficultyStrategyFactory()
    {
        _strategies = new Dictionary<DifficultyLevel, IDifficultyStrategy>
        {
            { DifficultyLevel.Easy, new EasyStrategy() },
            { DifficultyLevel.Medium, new MediumStrategy() },
            { DifficultyLevel.Hard, new HardStrategy() },
            { DifficultyLevel.VeryHard, new VeryHardStrategy() },
            { DifficultyLevel.Insane, new InsaneStrategy() },
            { DifficultyLevel.Inhuman, new InhumanStrategy() }
        };
    }

    public IDifficultyStrategy GetStrategy(DifficultyLevel level)
    {
        if (_strategies.TryGetValue(level, out var strategy))
        {
            return strategy;
        }
        return _strategies[DifficultyLevel.Easy];
    }
}