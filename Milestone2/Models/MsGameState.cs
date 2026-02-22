namespace CST350_MinesweeperMilestone.Models
{
    public class MsGameState
    {
        public int Rows { get; set; }
        public int Cols { get; set; }
        public int Mines { get; set; }

        public int RevealedCount { get; set; }
        public int FlagsLeft { get; set; }
        public bool FirstClick { get; set; } = true;

        public bool IsGameOver { get; set; }
        public bool IsWin { get; set; }

        public string StatusMsg { get; set; } = "Click \"New Game\" to start.";

        public int ElapsedSeconds { get; set; }

        // stored as [r][c]
        public List<List<MsCell>> Board { get; set; } = new();
    }
}