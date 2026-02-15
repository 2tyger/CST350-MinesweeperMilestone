using CST350_MinesweeperMilestone.Models;

namespace CST350_MinesweeperMilestone.Services
{
    public class MinesweeperService
    {
        private readonly Random _rng = new();

        public MsGameState CreateNew(int rows, int cols, int mines)
        {
            rows = Math.Clamp(rows, 5, 24);
            cols = Math.Clamp(cols, 5, 30);
            mines = Math.Clamp(mines, 1, rows * cols - 1);

            var g = new MsGameState
            {
                Rows = rows,
                Cols = cols,
                Mines = mines,
                FlagsLeft = mines,
                FirstClick = true,
                IsGameOver = false,
                IsWin = false,
                StatusMsg = "Game in progress"
            };

            g.Board = new List<List<MsCell>>();
            for (int r = 0; r < rows; r++)
            {
                var row = new List<MsCell>();
                for (int c = 0; c < cols; c++)
                {
                    row.Add(new MsCell { R = r, C = c });
                }
                g.Board.Add(row);
            }

            g.RevealedCount = 0;
            return g;
        }

        public GameUpdateResponse Reveal(MsGameState g, int r, int c)
        {
            var resp = BaseResponse(g);

            if (g.IsGameOver || g.IsWin) return resp;
            if (!InBounds(g, r, c)) return resp;

            var cell = g.Board[r][c];
            if (cell.Flagged || cell.Revealed) return resp;

            if (g.FirstClick)
            {
                PlaceMinesAvoiding(g, r, c);
                ComputeNeighbors(g);
                g.FirstClick = false;
            }

            // this fills resp.Updates
            RevealInternal(g, r, c, resp.Updates);

            CheckWin(g);

            return FinalizeResponse(g, resp);
        }

        public GameUpdateResponse ToggleFlag(MsGameState g, int r, int c)
        {
            var resp = BaseResponse(g);

            if (g.IsGameOver || g.IsWin) return resp;
            if (!InBounds(g, r, c)) return resp;

            var cell = g.Board[r][c];
            if (cell.Revealed) return resp;

            cell.Flagged = !cell.Flagged;
            g.FlagsLeft += cell.Flagged ? -1 : 1;

            resp.Updates.Add(ToUpdate(cell));
            return FinalizeResponse(g, resp);
        }

        private void RevealInternal(MsGameState g, int r, int c, List<CellUpdateDto> updates)
        {
            var cell = g.Board[r][c];
            if (cell.Revealed || cell.Flagged) return;

            cell.Revealed = true;
            g.RevealedCount++;

            // Mine hit
            if (cell.Mine)
            {
                g.IsGameOver = true;
                g.StatusMsg = "Game Over! You hit a mine.";

                // reveal all mines
                for (int i = 0; i < g.Rows; i++)
                {
                    for (int j = 0; j < g.Cols; j++)
                    {
                        if (g.Board[i][j].Mine)
                        {
                            g.Board[i][j].Revealed = true;
                            updates.Add(ToUpdate(g.Board[i][j]));
                        }
                    }
                }

                // include the clicked mine as well
                updates.Add(ToUpdate(cell));
                return;
            }

            updates.Add(ToUpdate(cell));

            // Flood fill for empty
            if (cell.NeighborMines == 0)
            {
                for (int dr = -1; dr <= 1; dr++)
                    for (int dc = -1; dc <= 1; dc++)
                    {
                        if (dr == 0 && dc == 0) continue;
                        int nr = r + dr, nc = c + dc;
                        if (!InBounds(g, nr, nc)) continue;
                        if (!g.Board[nr][nc].Revealed)
                            RevealInternal(g, nr, nc, updates);
                    }
            }
        }

        private void CheckWin(MsGameState g)
        {
            if (g.IsGameOver) return;

            int totalSafe = g.Rows * g.Cols - g.Mines;
            if (g.RevealedCount >= totalSafe)
            {
                g.IsWin = true;
                g.StatusMsg = "You Win!";

                // flag all mines
                for (int r = 0; r < g.Rows; r++)
                    for (int c = 0; c < g.Cols; c++)
                    {
                        if (g.Board[r][c].Mine)
                            g.Board[r][c].Flagged = true;
                    }
            }
        }

        private void PlaceMinesAvoiding(MsGameState g, int firstR, int firstC)
        {
            var forbidden = new HashSet<string>();
            forbidden.Add($"{firstR},{firstC}");

            for (int dr = -1; dr <= 1; dr++)
                for (int dc = -1; dc <= 1; dc++)
                {
                    int nr = firstR + dr, nc = firstC + dc;
                    if (InBounds(g, nr, nc))
                        forbidden.Add($"{nr},{nc}");
                }

            int toPlace = g.Mines;
            int total = g.Rows * g.Cols;

            while (toPlace > 0)
            {
                int idx = _rng.Next(total);
                int r = idx / g.Cols;
                int c = idx % g.Cols;
                string key = $"{r},{c}";
                if (forbidden.Contains(key)) continue;

                var cell = g.Board[r][c];
                if (!cell.Mine)
                {
                    cell.Mine = true;
                    toPlace--;
                }
            }
        }

        private void ComputeNeighbors(MsGameState g)
        {
            for (int r = 0; r < g.Rows; r++)
                for (int c = 0; c < g.Cols; c++)
                {
                    int count = 0;
                    for (int dr = -1; dr <= 1; dr++)
                        for (int dc = -1; dc <= 1; dc++)
                        {
                            if (dr == 0 && dc == 0) continue;
                            int nr = r + dr, nc = c + dc;
                            if (InBounds(g, nr, nc) && g.Board[nr][nc].Mine)
                                count++;
                        }
                    g.Board[r][c].NeighborMines = count;
                }
        }

        private static bool InBounds(MsGameState g, int r, int c)
            => r >= 0 && r < g.Rows && c >= 0 && c < g.Cols;

        private static CellUpdateDto ToUpdate(MsCell cell)
        {
            string text = "";
            if (cell.Revealed)
            {
                if (cell.Mine) text = "💣";
                else if (cell.NeighborMines > 0) text = cell.NeighborMines.ToString();
            }
            else if (cell.Flagged)
            {
                text = "🚩";
            }

            return new CellUpdateDto
            {
                Id = $"cell-{cell.R}-{cell.C}",
                Text = text,
                Revealed = cell.Revealed,
                Flagged = cell.Flagged,
                Mine = cell.Mine,
                NeighborMines = cell.NeighborMines
            };
        }

        private static GameUpdateResponse BaseResponse(MsGameState g) => new()
        {
            StatusMsg = g.StatusMsg,
            FlagsLeft = g.FlagsLeft,
            IsGameOver = g.IsGameOver,
            IsWin = g.IsWin,
            Updates = new List<CellUpdateDto>()
        };

        private static GameUpdateResponse FinalizeResponse(MsGameState g, GameUpdateResponse resp)
        {
            resp.StatusMsg = g.StatusMsg;
            resp.FlagsLeft = g.FlagsLeft;
            resp.IsGameOver = g.IsGameOver;
            resp.IsWin = g.IsWin;
            return resp;
        }
    }
}