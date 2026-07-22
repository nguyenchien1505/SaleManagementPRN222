using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebBanHang.Filters
{
    public class RoleAuthorize : ActionFilterAttribute
    {
        private readonly string[] _roles;

        public RoleAuthorize(params string[] roles)
        {
            _roles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var role = context.HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(role))
            {
                context.Result = new RedirectToActionResult("Login", "Account", new { area = "" });
                return;
            }

            if (!_roles.Contains(role))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", new { area = "" });
            }
        }
    }
}