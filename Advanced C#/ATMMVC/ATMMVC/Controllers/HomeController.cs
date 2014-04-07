using System.Web.Mvc;

namespace ATMMVC.Controllers {
    public class HomeController : Controller {
        public ActionResult Index() {
            ViewData["Message"] = "Welcome to Nenad's ATM!";
            return View();
        }
    }
}
