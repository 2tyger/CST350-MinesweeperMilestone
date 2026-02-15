namespace CST350_MinesweeperMilestone.Models
{
    public class GameUpdateResponse
    {
        public string StatusMsg { get; set; } = "";
        public int FlagsLeft { get; set; }
        public bool IsGameOver { get; set; }
        public bool IsWin { get; set; }

        public List<CellUpdateDto> Updates { get; set; } = new();
    }
}