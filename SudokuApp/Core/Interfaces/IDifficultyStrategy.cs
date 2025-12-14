namespace SudokuApp.Core.Interfaces
{
    public interface IDifficultyStrategy
    {
        void RemoveNumbers(int[,] board);
    }
}