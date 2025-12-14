using System;
using SudokuApp.Core.Interfaces;

namespace SudokuApp.Core.Strategies
{
    public abstract class BaseDifficultyStrategy : IDifficultyStrategy
    {
        protected readonly Random _random = new Random();
        
        // Кожна конкретна стратегія має лише вказати кількість "дірок"
        protected abstract int HolesCount { get; }

        public void RemoveNumbers(int[,] board)
        {
            int count = HolesCount;
            int size = board.GetLength(0); // 9

            while (count > 0)
            {
                int cellId = _random.Next(0, size * size);
                int row = cellId / size;
                int col = cellId % size;

                if (board[row, col] != 0)
                {
                    board[row, col] = 0; // Видаляємо значення
                    count--;
                }
            }
        }
    }
}