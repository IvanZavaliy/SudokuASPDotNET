using System;
using System.Linq;
using SudokuApp.Core.DTOs;
using SudokuApp.Core.Enums;
using SudokuAppSudokuApp.Core.Factories;
using SudokuApp.Core.Interfaces;

namespace SudokuApp.Infrastructure.Services
{
    public class SudokuService : ISudokuService
    {
        private readonly DifficultyStrategyFactory _strategyFactory;
        private readonly Random _rand = new Random();
        private const int SIZE = 9;

        public SudokuService(DifficultyStrategyFactory strategyFactory)
        {
            _strategyFactory = strategyFactory;
        }

        public SudokuGameDto CreateNewGame(DifficultyLevel level)
        {
            int[,] solution = new int[SIZE, SIZE];
            GenerateFullBoard(solution);

            int[,] puzzle = (int[,])solution.Clone();

            var strategy = _strategyFactory.GetStrategy(level);
            strategy.RemoveNumbers(puzzle);

            return new SudokuGameDto
            {
                PuzzleGrid = ToJaggedArray(puzzle),
                SolutionGrid = ToJaggedArray(solution),
                LevelName = level.ToString(),
                GameId = Guid.NewGuid().ToString()
            };
        }

        public bool ValidateMove(int[][] currentBoard, int row, int col, int value)
        {
            for (int c = 0; c < SIZE; c++)
                if (c != col && currentBoard[row][c] == value) return false;

            for (int r = 0; r < SIZE; r++)
                if (r != row && currentBoard[r][col] == value) return false;

            int startRow = row - row % 3;
            int startCol = col - col % 3;
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    if ((startRow + r != row || startCol + c != col) && 
                        currentBoard[startRow + r][startCol + c] == value)
                        return false;
                }
            }

            return true;
        }

        private void GenerateFullBoard(int[,] board)
        {
            FillDiagonal(board);
            SolveSudoku(board);
        }

        private void FillDiagonal(int[,] board)
        {
            for (int i = 0; i < SIZE; i += 3)
                FillBox(board, i, i);
        }

        private void FillBox(int[,] board, int row, int col)
        {
            int num;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    do
                    {
                        num = _rand.Next(1, 10);
                    } while (!IsSafeInBox(board, row, col, num));
                    board[row + i, col + j] = num;
                }
            }
        }

        private bool IsSafeInBox(int[,] board, int rowStart, int colStart, int num)
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (board[rowStart + i, colStart + j] == num) return false;
            return true;
        }

        private bool IsSafe(int[,] board, int row, int col, int num)
        {
            for (int x = 0; x < SIZE; x++)
                if (board[row, x] == num || board[x, col] == num) return false;

            int startRow = row - row % 3;
            int startCol = col - col % 3;
            return IsSafeInBox(board, startRow, startCol, num);
        }

        private bool SolveSudoku(int[,] board)
        {
            int row = -1, col = -1;
            bool isEmpty = true;

            for (int i = 0; i < SIZE; i++)
            {
                for (int j = 0; j < SIZE; j++)
                {
                    if (board[i, j] == 0)
                    {
                        row = i;
                        col = j;
                        isEmpty = false;
                        break;
                    }
                }
                if (!isEmpty) break;
            }

            if (isEmpty) return true;

            var numbers = Enumerable.Range(1, 9).OrderBy(x => _rand.Next()).ToList();

            foreach (var num in numbers)
            {
                if (IsSafe(board, row, col, num))
                {
                    board[row, col] = num;
                    if (SolveSudoku(board)) return true;
                    board[row, col] = 0;
                }
            }
            return false;
        }
        
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