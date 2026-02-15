namespace CST350_MinesweeperMilestone.Models
{
    public class MsCell
    {
        public int R { get; set; }
        public int C { get; set; }
        public bool Mine { get; set; }
        public bool Revealed { get; set; }
        public bool Flagged { get; set; }
        public int NeighborMines { get; set; }
    }
}