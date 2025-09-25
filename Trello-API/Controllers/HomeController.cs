using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Description;

namespace Trello_API.Controllers
{
    [RoutePrefix("")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : ApiController
    {
        [HttpGet, Route("")]
        public HttpResponseMessage Index()
        {
            var path = HostingEnvironment.MapPath("~/Content/swagger-ui/index.html");
            var html = File.ReadAllText(path);

            var resp = new HttpResponseMessage();
            resp.Content = new StringContent(html);
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return resp;
        }
    }
}
