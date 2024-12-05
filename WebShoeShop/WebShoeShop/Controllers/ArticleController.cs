using System.Linq;
using System.Web.Mvc;
using WebShoeShop.Models;

namespace WebShoeShop.Controllers
{
	public class ArticleController : Controller
	{
		private ApplicationDbContext db = new ApplicationDbContext();
		// GET: Article
		public ActionResult Index(int? page)
		{
			var items = db.Products.Where(x => x.IsSale).Take(100).ToList();
			var coupon = db.Coupons.Take(3).ToList();
			ViewBag.Coupon = coupon;
			return View(items);
		}
		public ActionResult Details(int id)
		{
			var item = db.Posts.Find(id);
			return View(item);
		}
	}
}