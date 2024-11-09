﻿using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using MMEmergencyCall.Shared;
using MMEmergencyCall.Domain.Client.Features.Signin;
using MMEmergencyCall.Domain.Admin.Features.Users;

namespace MMEmergencyCall.Api.Middlewares
{
    public class CustomAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly UserService _userService;

        public CustomAuthorizeAttribute(UserService userService)
        {
            _userService = userService;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!context.HttpContext!.Request.Headers["Token"].Any())
            {
                context.Result = new UnauthorizedObjectResult(string.Empty);
                return;
            }

            try
            {
                var signin = context.HttpContext!.Request.Headers["Token"]
                            .ToString()
                            .ToDecrypt()
                            .ToObject<SigninModel>();

                if (!await IsUserExist(signin.UserId))
                {
                    context.Result = new UnauthorizedObjectResult("User does not exist.");
                    return;
                }

                if (signin.SessionExpiredTime <= DateTime.Now)
                {
                    context.Result = new UnauthorizedObjectResult("Session has expired.");
                    return;
                }
            }
            catch (Exception)
            {
                context.Result = new UnauthorizedObjectResult("Invalid token.");
                return;
            }
        }

        private async Task<bool> IsUserExist(int userId)
        {
            var user = await _userService.GetByIdAsync(userId);
            return user != null;
        }
    }
}
