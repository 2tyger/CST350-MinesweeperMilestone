using CST350_MinesweeperMilestone.Data;
using CST350_MinesweeperMilestone.Filters;
using CST350_MinesweeperMilestone.Models;
using CST350_MinesweeperMilestone.Services;
using Microsoft.AspNetCore.Mvc;

namespace CST350_MinesweeperMilestone.Controllers
{
    public class UserController : Controller
    {
        private readonly UserRepository _repo;
        private readonly MinesweeperService _ms;

        public UserController(IConfiguration config, MinesweeperService ms)
        {
            _repo = new UserRepository(config);
            _ms = ms;
        }

        // Get Request for /User/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // Post Request for /User/Register
        [HttpPost]
        public IActionResult Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            if (_repo.UsernameExists(vm.Username))
            {
                ModelState.AddModelError("Username", "Username is already taken.");
                return View(vm);
            }

            var user = new UserModel
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Sex = vm.Sex,
                Age = vm.Age,
                State = vm.State,
                Email = vm.Email,
                Username = vm.Username
            };

            user.SetPassword(vm.Password);
            user.Id = _repo.CreateUser(user);

            return View("RegisterSuccess", user);
        }

        // Get Request for /User/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        // Post Request for /User/Login
        [HttpPost]
        public IActionResult ProcessLogin(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
                return View("Login", vm);

            var user = _repo.GetByUsername(vm.Username);

            if (user != null && user.VerifyPassword(vm.Password))
            {
                // store login status in session
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("Username", user.Username);

                return View("LoginSuccess", user);
            }

            return View("LoginFailure");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // Restricted game page
        [SessionCheckFilter]
        public IActionResult StartGame()
        {
            return View();
        }




        private const string GameSessionKey = "MS_GAME";

        [SessionCheckFilter]
        [HttpPost]
        public IActionResult NewGame(int rows, int cols, int mines)
        {
            var game = _ms.CreateNew(rows, cols, mines);
            HttpContext.Session.SetObject(GameSessionKey, game);

            // return initial state to render board
            return Json(new
            {
                rows = game.Rows,
                cols = game.Cols,
                flagsLeft = game.FlagsLeft,
                statusMsg = game.StatusMsg
            });
        }

        [SessionCheckFilter]
        [HttpPost]
        public IActionResult Reveal(int r, int c)
        {
            var game = HttpContext.Session.GetObject<MsGameState>(GameSessionKey);
            if (game == null) return BadRequest("No game in session. Start a new game.");

            var resp = _ms.Reveal(game, r, c);
            HttpContext.Session.SetObject(GameSessionKey, game);

            return Json(resp);
        }


        [SessionCheckFilter]
        [HttpPost]
        public IActionResult ToggleFlag(int r, int c)
        {
            var game = HttpContext.Session.GetObject<MsGameState>(GameSessionKey);
            if (game == null) return BadRequest("No game in session. Start a new game.");

            var resp = _ms.ToggleFlag(game, r, c);
            HttpContext.Session.SetObject(GameSessionKey, game);

            return Json(resp);
        }

        [SessionCheckFilter]
        [HttpGet]
        public IActionResult Timestamp()
        {
            return Content(DateTime.Now.ToString("hh:mm:ss tt"));
        }
    }
}
