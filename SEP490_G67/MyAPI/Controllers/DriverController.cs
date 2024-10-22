using Microsoft.AspNetCore.Mvc;

namespace MyAPI.Controllers
{
    public class DriverController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
