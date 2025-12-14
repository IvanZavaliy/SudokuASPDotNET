namespace SudokuApp.Core.Strategies
{
    public class EasyStrategy : BaseDifficultyStrategy
    {
        protected override int HolesCount => 29;
    }

    public class MediumStrategy : BaseDifficultyStrategy
    {
        protected override int HolesCount => 38;
    }

    public class HardStrategy : BaseDifficultyStrategy
    {
        protected override int HolesCount => 47;
    }

    public class VeryHardStrategy : BaseDifficultyStrategy
    {
        protected override int HolesCount => 56;
    }

    public class InsaneStrategy : BaseDifficultyStrategy
    {
        protected override int HolesCount => 65;
    }
    
    public class InhumanStrategy : BaseDifficultyStrategy
    {
        protected override int HolesCount => 74;
    }
}