using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    // don't use BaseApiController this time because we need a View
    // effectively we go to index.html if API server doesn't know what to do with a particular route.
    public class FallbackController : Controller
    {
        public ActionResult Index()
        {
            // CurrentDirectory is API folder
            return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(), 
                "wwwroot", "index.html"), "text/HTML");
        }
    }
}
