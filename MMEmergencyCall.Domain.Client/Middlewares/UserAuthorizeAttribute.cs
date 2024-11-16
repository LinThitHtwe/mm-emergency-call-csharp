﻿using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using MMEmergencyCall.Domain.Client.Features.Signin;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MMEmergencyCall.Domain.Client.Middlewares;

public class UserAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var serviceProvider = context.HttpContext.RequestServices;
        var dbContext = serviceProvider.GetService<AppDbContext>();

        if (dbContext == null)
        {
            context.Result = new UnauthorizedObjectResult("Database context is not available.");
            return;
        }

        if (!context.HttpContext.Request.Headers["Token"].Any())
        {
            context.Result = new UnauthorizedObjectResult("Token is missing.");
            return;
        }

        try
        {
            var token = context.HttpContext.Request.Headers["Token"].ToString();
            var item = token.ToDecrypt().ToObject<SigninModel>();

            if (!await IsUserExist(dbContext, item.UserId))
            {
                context.Result = new UnauthorizedObjectResult("User does not exist.");
                return;
            }

            if (item.SessionExpiredTime <= DateTime.Now)
            {
                context.Result = new UnauthorizedObjectResult("Session has expired.");
                return;
            }
        }
        catch (Exception)
        {
            context.Result = new UnauthorizedObjectResult("Invalid token.");
        }
    }

    private async Task<bool> IsUserExist(AppDbContext dbContext, int userId)
    {
        return await dbContext.Users.AnyAsync(u => u.UserId == userId);
    }
}
