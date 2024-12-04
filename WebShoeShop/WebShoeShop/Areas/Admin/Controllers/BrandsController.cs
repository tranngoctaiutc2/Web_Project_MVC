using System.Linq;
using System.Net;
using System.Web.Mvc;
using WebShoeShop.Models;
using WebShoeShop.Models.EF;

namespace WebShoeShop.Areas.Admin.Controllers
{
	public class BrandsController : Controller
	{
		private ApplicationDbContext db = new ApplicationDbContext();
		// GET: Admin/Brands
		public ActionResult Index()
		{
			var brands = db.Brands.ToList();
			return View(brands);
		}
		public ActionResult Create()
		{
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(Brand brand)
		{
			if (ModelState.IsValid)
			{
				db.Brands.Add(brand);
				db.SaveChanges();
				return RedirectToAction("Index");
			}
			return View(brand);
		}
		public ActionResult Edit(int? id)
		{
			if (id == null)
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			var brand = db.Brands.Find(id);
			if (brand == null)
				return HttpNotFound();
			return View(brand);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(Brand model)
		{
			if (ModelState.IsValid)
			{
				var existingBrand = db.Brands.Find(model.Id);
				if (existingBrand == null)
				{
					HttpNotFound();
				}
				existingBrand.Name = model.Name;
				existingBrand.Description = model.Description;
				existingBrand.IsActive = model.IsActive;
				db.SaveChanges();
				return RedirectToAction("Index");
			}
			return View(model);
		}
		public ActionResult Delete(int? id)
		{
			if (id == null)
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

			var coupon = db.Coupons.Find(id);
			if (coupon == null)
				return HttpNotFound();

			return View(coupon);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(int id)
		{
			var brand = db.Brands.Find(id);
			if (brand != null)
			{
				db.Brands.Remove(brand);
				db.SaveChanges();
			}
			return RedirectToAction("Index");
		}
	}
}