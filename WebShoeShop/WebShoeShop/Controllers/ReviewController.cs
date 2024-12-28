using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Linq;
using System.Web.Mvc;
using WebShoeShop.Models;
using WebShoeShop.Models.EF;

namespace WebShoeShop.Controllers
{
	[Authorize]
	public class ReviewController : Controller
	{
		private ApplicationDbContext db = new ApplicationDbContext();
		// GET: Review
		public ActionResult Index()
		{
			return View();
		}
		[AllowAnonymous]
		public ActionResult _Review(int productId)
		{
			ViewBag.ProductId = productId;
			var item = new ReviewProduct();
			if (User.Identity.IsAuthenticated)
			{
				var userStore = new UserStore<ApplicationUser>(new ApplicationDbContext());
				var userManager = new UserManager<ApplicationUser>(userStore);
				var user = userManager.FindByName(User.Identity.Name);
				if (user != null)
				{
					var hasPurchased = db.Orders.Where(o => o.CustomerId == user.Id && o.OrderDetails.Any(
						od => od.ProductId == productId
						)).Any();
					if (!hasPurchased)
					{
						// Người dùng chưa mua sản phẩm -> Không được phép đánh giá
						return PartialView("_NotAuthorizedReview");
					}
					item.Email = user.Email;
					item.FullName = user.FullName;
					item.UserName = user.UserName;
				}
				return PartialView(item);
			}
			var hasPurchasedByEmail = db.Orders.Any(o =>
	   o.OrderDetails.Any(od => od.ProductId == productId));

			if (!hasPurchasedByEmail)
			{
				ViewBag.Message = "Rất tiếc, bạn chưa đủ điều kiện để đánh giá sản phẩm này.";
				return PartialView("_NotAuthorizedReview");
			}
			return PartialView();
		}
		[AllowAnonymous]
		public ActionResult _load_review(int productId)
		{
			var item = db.Reviews.Where(x => x.ProductId == productId).OrderByDescending(x => x.Id).ToList();
			ViewBag.Count = item.Count;
			return PartialView(item);
		}
		[AllowAnonymous]
		[HttpPost]
		public ActionResult PostReview(ReviewProduct req)
		{
			if (!ModelState.IsValid)
			{
				return Json(new
				{
					Success = false,
					Message = "Dữ liệu không hợp lệ.",
					ShowNotAuthorizedView = true
				});
			}
			bool hasPurchased = false;

			if (User.Identity.IsAuthenticated)
			{
				var userStore = new UserStore<ApplicationUser>(new ApplicationDbContext());
				var userManager = new UserManager<ApplicationUser>(userStore);
				var user = userManager.FindByName(User.Identity.Name);

				if (user != null)
				{
					hasPurchased = db.Orders.Any(o =>
						o.CustomerId == user.Id &&
						o.OrderDetails.Any(od => od.ProductId == req.ProductId));
				}
			}
			else
			{
				// Kiểm tra theo email cho người dùng chưa đăng ký
				hasPurchased = db.Orders.Any(o =>
					o.Email.ToLower() == req.Email.ToLower() &&
					o.OrderDetails.Any(od => od.ProductId == req.ProductId));
			}

			if (!hasPurchased)
			{
				return Json(new
				{
					Success = false,
					Message = "Bạn chưa mua sản phẩm này, không thể đánh giá.",
					ShowNotAuthorizedView = true
				});
			}
			req.CreateDate = DateTime.Now;
			db.Reviews.Add(req);
			db.SaveChanges();

			var reviews = db.Reviews
				.Where(x => x.ProductId == req.ProductId)
				.OrderByDescending(x => x.Id)
				.ToList();

			return PartialView("_load_review", reviews);
		}
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}
	}
}