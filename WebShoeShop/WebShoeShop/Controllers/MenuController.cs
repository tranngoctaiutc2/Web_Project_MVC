using System.Linq;
using System.Web.Mvc;
using WebShoeShop.Models;

namespace WebShoeShop.Controllers
{
	public class MenuController : Controller
	{
		private ApplicationDbContext db = new ApplicationDbContext();
		// GET: Menu
		public ActionResult Index()
		{
			return View();
		}

		public ActionResult MenuTop()
		{
			var items = db.Categories.OrderBy(x => x.Position).ToList();
			return PartialView("_MenuTop", items);
		}

		public ActionResult MenuProductCategory()
		{
			var items = db.ProductCategories.ToList();
			return PartialView("_MenuProductCategory", items);
		}
		public ActionResult MenuLeft(int? id)
		{
			if (id != null)
			{
				ViewBag.CateId = id;
			}
			var items = db.ProductCategories.ToList();
			return PartialView("_MenuLeft", items);
		}
		public ActionResult MenuBrand(int? id)
		{
			if (id != null)
			{
				ViewBag.BrandId = id;
			}
			var items = db.Brands.ToList();
			return PartialView("_MenuBrand", items);
		}
		public ActionResult MenuArrivals()
		{
			var items = db.ProductCategories.ToList();
			return PartialView("_MenuArrivals", items);
		}

	}
}