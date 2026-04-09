using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TouristApp.API.Authentification
{
    public class JwtAuthenticationFilter : IAsyncActionFilter
    {
        private readonly JwtService _jwtService;

        public JwtAuthenticationFilter(JwtService jwtService)
        {
            _jwtService = jwtService;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var token =
                context.HttpContext.Request.Headers["Authorization"]
                    .ToString()
                    .Replace("Bearer ", "");

            if (!_jwtService.IsTokenValid(token))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            await next();
        }
    }
}
