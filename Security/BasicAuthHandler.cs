using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using GBEMiddlewareApi.Data;
using Microsoft.EntityFrameworkCore;

namespace GBEMiddlewareApi.Security
{
    public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly MiddlewareDbContext _middlewareContext;

        public BasicAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            System.Text.Encodings.Web.UrlEncoder encoder,
            ISystemClock clock,
            MiddlewareDbContext middlewareContext)
            : base(options, logger, encoder, clock)
        {
            _middlewareContext = middlewareContext;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // 1. Check if "Authorization" header is present
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                // No Auth header => No attempt
                return AuthenticateResult.NoResult();
            }

            // 2. Ensure it's "Basic" auth
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            if (!authHeader.Scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticateResult.NoResult();
            }

            // 3. Decode "username:password"
            var credentialBytes = Convert.FromBase64String(authHeader.Parameter ?? "");
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
            if (credentials.Length < 2)
            {
                return AuthenticateResult.Fail("Invalid Basic authentication header.");
            }

            var username = credentials[0];
            var password = credentials[1];

            // 4. Validate in DB (ApiCredentials)
            var apiCred = await _middlewareContext.ApiCredentials
                .Include(a => a.Partner)
                .Where(a => a.Status == "ACTIVE")
                .FirstOrDefaultAsync(a => a.Username == username && a.Password == password);

            if (apiCred == null)
            {
                return AuthenticateResult.Fail("Invalid username or password, or inactive credentials.");
            }

            // 5. Optional IP check (AllowedIp)
            var requestIp = Context.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrEmpty(apiCred.AllowedIp) && apiCred.AllowedIp != requestIp)
            {
                return AuthenticateResult.Fail("IP not allowed for these credentials.");
            }

            // 6. Ensure partner is active
            if (apiCred.Partner == null || apiCred.Partner.Status != "ACTIVE")
            {
                return AuthenticateResult.Fail("Associated partner is inactive or not found.");
            }

            // 7. Build claims
            var claims = new[]
            {
                new Claim("PartnerId", apiCred.PartnerId.ToString()),
                new Claim("ServiceId", apiCred.ServiceId.ToString()),
                new Claim("ApiCredId", apiCred.ApiCredId.ToString()),
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}
