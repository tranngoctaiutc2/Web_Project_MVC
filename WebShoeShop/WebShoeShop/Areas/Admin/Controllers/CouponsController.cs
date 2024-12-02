using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using WebShoeShop.Common;
using WebShoeShop.Models;
using WebShoeShop.Models.EF;

namespace WebShoeShop.Areas.Admin.Controllers
{
	[CustomAuthorize]
	public class CouponsController : Controller
	{
		private ApplicationDbContext db = new ApplicationDbContext();

		// GET: Admin/Coupons
		public ActionResult Index()
		{
			var coupons = db.Coupons.ToList();
			return View(coupons);
		}
		public ActionResult Create()
		{
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(Coupon coupon)
		{
			if (ModelState.IsValid)
			{
				coupon.UsageCount = 0;
				db.Coupons.Add(coupon);
				db.SaveChanges();
				return RedirectToAction("Index");
			}
			return View(coupon);
		}
		public ActionResult Edit(int? id)
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
		public ActionResult Edit(Coupon coupon)
		{
			if (ModelState.IsValid)
			{
				var existingCoupon = db.Coupons.Find(coupon.Id);
				if (existingCoupon == null)
				{
					return HttpNotFound();
				}

				existingCoupon.Code = coupon.Code;
				existingCoupon.Description = coupon.Description;
				existingCoupon.DiscountAmount = coupon.DiscountAmount;
				existingCoupon.DiscountPercentage = coupon.DiscountPercentage;
				existingCoupon.MaxDiscountAmount = coupon.MaxDiscountAmount;
				existingCoupon.MinimumOrderAmount = coupon.MinimumOrderAmount;
				existingCoupon.StartDate = coupon.StartDate;
				existingCoupon.ExpirationDate = coupon.ExpirationDate;
				existingCoupon.IsActive = coupon.IsActive;
				existingCoupon.UsageLimit = coupon.UsageLimit;

				db.SaveChanges();
				return RedirectToAction("Index");
			}

			return View(coupon);
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
			var coupon = db.Coupons.Find(id);
			if (coupon != null)
			{
				db.Coupons.Remove(coupon);
				db.SaveChanges();
			}
			return RedirectToAction("Index");
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult ApplyCoupon(string code, decimal orderAmount)
		{
			var coupon = db.Coupons.FirstOrDefault(c => c.Code == code && c.IsActive && c.StartDate <= DateTime.Now && c.ExpirationDate >= DateTime.Now);
			if (coupon == null)
				return Json(new { success = false, message = "Mã giảm giá không hợp lệ hoặc đã hết hạn." });


			if (coupon.MinimumOrderAmount.HasValue && orderAmount < coupon.MinimumOrderAmount.Value)
				return Json(new { success = false, message = $"Đơn hàng phải có giá trị tối thiểu {coupon.MinimumOrderAmount} để áp dụng mã giảm giá." });

			if (coupon.UsageLimit.HasValue && coupon.UsageCount >= coupon.UsageLimit.Value)
				return Json(new { success = false, message = "Mã giảm giá đã đạt đến giới hạn sử dụng." });

			// Calculate discount
			decimal discount = coupon.DiscountAmount ?? (orderAmount * (decimal)(coupon.DiscountPercentage / 100));
			if (coupon.MaxDiscountAmount.HasValue)
				discount = Math.Min(discount, coupon.MaxDiscountAmount.Value);

			// Update usage count
			coupon.UsageCount++;
			db.Entry(coupon).State = EntityState.Modified;
			db.SaveChanges();

			return Json(new { success = true, discount, message = "Mã giảm giá đã được áp dụng thành công!" });
		}
	}
}