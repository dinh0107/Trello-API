using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Security.Jwt;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

[assembly: OwinStartup(typeof(Trello_API.Startup))]
namespace Trello_API
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var key = Encoding.ASCII.GetBytes("1f885887384444974576db63a778d063eb9e65937f4b0d02");

            app.Use(async (context, next) =>
            {
                if (!context.Request.Headers.ContainsKey("Authorization"))
                {
                    var cookieToken = context.Request.Cookies["AccessToken"];
                    if (!string.IsNullOrEmpty(cookieToken))
                    {
                        context.Request.Headers.Append("Authorization", "Bearer " + cookieToken);
                    }
                }
                await next.Invoke();
            });

            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Active,
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }
            });
        }

    }
}