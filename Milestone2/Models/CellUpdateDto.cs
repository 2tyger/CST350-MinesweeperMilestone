namespace CST350_MinesweeperMilestone.Models
{
    public class CellUpdateDto
    {
        public string Id { get; set; } = "";
        public string Text { get; set; } = "";
        public bool Revealed { get; set; }
        public bool Flagged { get; set; }
        public bool Mine { get; set; }
        public int NeighborMines { get; set; }
    }
}