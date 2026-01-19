using System.ComponentModel.DataAnnotations;

namespace CST350_MinesweeperMilestone.Models
{
    public class RegisterViewModel
    {
        [Required] public string FirstName { get; set; }
        [Required] public string LastName { get; set; }
        [Required] public string Sex { get; set; }

        [Range(1, 130)]
        public int Age { get; set; }

        [Required, StringLength(2, MinimumLength = 2)]
        public string State { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required] public string Username { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }
    }
}
