namespace SudokuApp.Core.Interfaces
{
    public interface IDifficultyStrategy
    {
        // Метод приймає заповнену дошку і видаляє з неї цифри
        void RemoveNumbers(int[,] board);
    }
}