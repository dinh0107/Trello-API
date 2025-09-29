using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Trello_API.Helper
{
    public class JwtCookieAuthHandler : DelegatingHandler 
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var cookie = HttpContext.Current.Request.Cookies["AccessToken"];
            if (cookie != null)
            {
                try
                {
                    var principal = JwtHelper.GetPrincipal(cookie.Value);
                    HttpContext.Current.User = principal;
                    request.GetRequestContext().Principal = principal;
                }
                catch
                {
                    // token không hợp lệ → không set principal
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}