using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using UserManagementSystem.Services;

namespace UserManagementSystem.Middleware
{
    public class UserStatusMiddleware
    {
        private readonly RequestDelegate _next;

        public UserStatusMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IUserService userService)
        {
            var path = context.Request.Path.Value;
            if (path.StartsWith("/Account/Login") ||
                path.StartsWith("/Account/Register") ||
                path.StartsWith("/Account/Logout") ||
                path == "/")
            {
                await _next(context);
                return;
            }

            var userIdString = context.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) && !context.User.Identity.IsAuthenticated)
            {
                context.Response.Redirect("/Account/Login");
                return;
            }

            if (string.IsNullOrEmpty(userIdString) && context.User.Identity.IsAuthenticated)
            {
                var identityUserId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(identityUserId))
                {
                    context.Session.SetString("UserId", identityUserId);
                    userIdString = identityUserId;
                }
                else
                {
                    context.Response.Redirect("/Account/Login");
                    return;
                }
            }


            var userId = Guid.Parse(userIdString);
            var isBlocked = await userService.IsUserBlockedAsync(userId);

            if (isBlocked)
            {
                var signInManager = context.RequestServices.GetService<SignInManager<IdentityUser>>();
                if (signInManager != null && signInManager.IsSignedIn(context.User))
                {
                    await signInManager.SignOutAsync();
                }
                context.Session.Clear();
                context.Response.Redirect("/Account/Login?message=Your account has been blocked");
                return;
            }

            await _next(context);
        }
    }
}