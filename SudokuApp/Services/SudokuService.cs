namespace SudokuApp.Services
{
    public class SudokuService
    {
        private Random _rand = new Random();
        private const int SIZE = 9;

        // Генерує пару: (Загадка, Відповідь)
        public (int[,] Puzzle, int[,] Solution) GenerateSudoku(string levelName)
        {
            int[,] solution = new int[SIZE, SIZE];
            
            // Заповнюємо діагональні 3х3 квадрати (вони незалежні), щоб прискорити генерацію
            FillDiagonal(solution);
            
            // Вирішуємо решту (Backtracking)
            SolveSudoku(solution);

            // Копіюємо рішення в загадку
            int[,] puzzle = (int[,])solution.Clone();

            // Видаляємо клітинки залежно від рівня
            int holes = GetHolesCount(levelName);
            RemoveDigits(puzzle, holes);

            return (puzzle, solution);
        }

        private int GetHolesCount(string level)
        {
            return level switch
            {
                "Easy" => 29,
                "Medium" => 38,
                "Hard" => 47,
                "Very hard" => 56,
                "Insane" => 65,
                "Inhuman" => 74,
                _ => 29
            };
        }

        private void FillDiagonal(int[,] board)
        {
            for (int i = 0; i < SIZE; i += 3)
            {
                FillBox(board, i, i);
            }
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
            // Check Row & Col
            for (int x = 0; x < SIZE; x++)
                if (board[row, x] == num || board[x, col] == num) return false;

            // Check Box
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

            if (isEmpty) return true; // Done

            // Спробувати числа 1-9
            var numbers = Enumerable.Range(1, 9).OrderBy(x => _rand.Next()).ToList(); // Shuffle

            foreach (var num in numbers)
            {
                if (IsSafe(board, row, col, num))
                {
                    board[row, col] = num;
                    if (SolveSudoku(board)) return true;
                    board[row, col] = 0; // Backtrack
                }
            }
            return false;
        }

        private void RemoveDigits(int[,] board, int count)
        {
            while (count > 0)
            {
                int cellId = _rand.Next(0, 81);
                int i = cellId / SIZE;
                int j = cellId % SIZE;
                
                if (board[i, j] != 0)
                {
                    board[i, j] = 0;
                    count--;
                }
            }
        }
    }
}