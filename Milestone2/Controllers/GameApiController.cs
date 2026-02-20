using CST350_MinesweeperMilestone.Data;
using Microsoft.AspNetCore.Mvc;

namespace CST350_MinesweeperMilestone.Controllers
{
    [ApiController]
    [Route("api")]
    public class GameApiController : ControllerBase
    {
        private readonly GameRepository _games;

        public GameApiController(IConfiguration config)
        {
            _games = new GameRepository(config);
        }

        [HttpGet("showSavedGames")]
        public IActionResult ShowSavedGames()
        {
            return Ok(_games.GetAllGames());
        }

        [HttpGet("showSavedGames/{id:int}")]
        public IActionResult ShowSavedGame(int id)
        {
            var game = _games.GetGameById(id);
            if (game == null) return NotFound();
            return Ok(game);
        }

        [HttpDelete("deleteOneGame/{id:int}")]
        public IActionResult DeleteOneGame(int id)
        {
            if (!_games.DeleteGame(id)) return NotFound();
            return Ok(new { deleted = true });
        }
    }
}
