namespace CST350_MinesweeperMilestone.Models
{
    public class SavedGame
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime DateSaved { get; set; }
        public string GameData { get; set; } = string.Empty;
    }

    public class SavedGameSummary
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime DateSaved { get; set; }
    }
}
