using Microsoft.AspNetCore.Mvc;

namespace StockTwse.Controllers {
	public class StockTwseController : Controller {
		public IActionResult Index() {
			return View();
		}
	}
}
