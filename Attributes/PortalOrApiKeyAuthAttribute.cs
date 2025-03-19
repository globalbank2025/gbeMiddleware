using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using GBEMiddlewareApi.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

namespace GBEMiddlewareApi.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class PortalOrApiKeyAuthAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            // 1) Check if user is already authenticated (portal user)
            if (user?.Identity?.IsAuthenticated == true)
            {
                // Portal user is authenticated => PASS
                return;
            }

            // 2) Otherwise, check if 3rd-party is using an API key
            var request = context.HttpContext.Request;
            var apiKey = request.Headers["X-Api-Key"].FirstOrDefault();

            if (string.IsNullOrEmpty(apiKey))
            {
                // No user, no API key => unauthorized
                context.Result = new UnauthorizedResult();
                return;
            }

            // Validate API key from DB
            var dbContext = (ApplicationDbContext)context.HttpContext
                .RequestServices.GetService(typeof(ApplicationDbContext));

            var creds = dbContext.ApiCredentials
                .AsNoTracking()
                .FirstOrDefault(c => c.ApiKey == apiKey && c.Status == "ACTIVE");

            if (creds == null)
            {
                // Invalid API key
                context.Result = new UnauthorizedResult();
                return;
            }

            // Optionally, set HttpContext.Items to hold client info
            context.HttpContext.Items["ApiClientName"] = $"Client-{creds.PartnerId}";
            // Let request pass
        }
    }
}
