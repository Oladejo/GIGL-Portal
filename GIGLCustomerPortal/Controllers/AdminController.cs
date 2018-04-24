using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GIGLCustomerPortal.Controllers
{
    [Authorize(Roles = userRole)]
    [Authorize]
    public class AdminController : Controller
    {
        private const string userRole = "Admin";

        [Route("~/admin/dashboard")]
        public IActionResult Index()
        {
            return View();
        }
    }
}