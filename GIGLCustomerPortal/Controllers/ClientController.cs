using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GIGLCustomerPortal.Controllers
{
    [Authorize]
    public class ClientController : Controller
    {
        [Route("~/client/dashboard")]
        public IActionResult Index()
        {
            return View();
        }
    }
}