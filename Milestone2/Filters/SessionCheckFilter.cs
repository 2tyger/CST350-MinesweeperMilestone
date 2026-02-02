using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CST350_MinesweeperMilestone.Filters
{
    public class SessionCheckFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Session.GetString("UserId") == null)
            {
                context.Result = new RedirectToActionResult("Login", "User", null);
            }
        }
    }
}
