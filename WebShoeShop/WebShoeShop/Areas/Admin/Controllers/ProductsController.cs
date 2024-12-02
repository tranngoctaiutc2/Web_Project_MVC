using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebShoeShop.Common;
using WebShoeShop.Models;
using WebShoeShop.Models.EF;

namespace WebShoeShop.Areas.Admin.Controllers
{
	[CustomAuthorize]
	public class ProductsController : Controller
	{
		private ApplicationDbContext db = new ApplicationDbContext();
		// GET: Admin/Products
		public ActionResult Index(string Searchtext, string SelectedOption, int? page)
		{
			IEnumerable<Product> items = db.Products.OrderByDescending(x => x.Id);
			var pageSize = 10;
			if (page == null)
			{
				page = 1;
			}
			if (!string.IsNullOrEmpty(Searchtext))
			{
				string searchTextLower = Searchtext.ToLowerInvariant();
				items = items.Where(x => x.Alias.IndexOf(searchTextLower, StringComparison.OrdinalIgnoreCase) != -1 || x.Title.IndexOf(searchTextLower, StringComparison.OrdinalIgnoreCase) != -1);
			}
			if (!string.IsNullOrEmpty(SelectedOption))
			{
				string searchTextLower = SelectedOption.ToLowerInvariant();
				items = items.Where(x => x.ProductCategory.Title.IndexOf(searchTextLower, StringComparison.OrdinalIgnoreCase) != -1);
			}
			var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
			items = items.ToPagedList(pageIndex, pageSize);
			ViewBag.PageSize = pageSize;
			ViewBag.Page = page;
			ViewBag.SelectedOption = SelectedOption; // 
			return View(items);

		}
		public ActionResult Add()
		{
			ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Add(Product model, List<string> Images, List<int> rDefault, List<int> Sizes, List<int> Quantities)
		{
			if (ModelState.IsValid)
			{
				var existingProduct = db.Products.FirstOrDefault(p => p.Id == model.Id);
				if (existingProduct != null)
				{
					// Cập nhật sản phẩm
					existingProduct.Title = model.Title;
					existingProduct.Description = model.Description;
					existingProduct.Price = model.Price;
					existingProduct.Quantity = model.Quantity;
					existingProduct.ProductCategoryId = model.ProductCategoryId;
					existingProduct.ModifiedDate = DateTime.Now;
					existingProduct.OriginalPrice2 = model.OriginalPrice2;
					existingProduct.Detail = model.Detail;
					existingProduct.PriceSale = model.PriceSale;
					existingProduct.IsActive = model.IsActive;
					existingProduct.IsSale = model.IsSale;
					existingProduct.IsHome = model.IsHome;
					existingProduct.IsFeature = model.IsFeature;

					existingProduct.ProductSize.Clear(); // Xóa size cũ
					if (Sizes != null && Sizes.Count > 0)
					{
						for (int i = 0; i < Sizes.Count; i++)
						{
							existingProduct.ProductSize.Add(new ProductSize
							{
								ProductId = existingProduct.Id,
								Size = Sizes[i],
								Quantity = Quantities[i]
							});
						}
					}

					// Quản lý hình ảnh
					if (Images != null && Images.Count > 0)
					{
						var currentImages = existingProduct.ProductImage.Select(i => i.Image).ToList();
						db.ProductImages.RemoveRange(existingProduct.ProductImage.Where(i => !Images.Contains(i.Image)).ToList());

						for (int i = 0; i < Images.Count; i++)
						{
							var isDefault = (i + 1 == rDefault[0]);
							if (!currentImages.Contains(Images[i]))
							{
								existingProduct.ProductImage.Add(new ProductImage
								{
									ProductId = existingProduct.Id,
									Image = Images[i],
									IsDefault = isDefault
								});
							}
						}
					}

					db.SaveChanges();
					return RedirectToAction("Index");
				}
				else
				{
					// Tạo mới sản phẩm
					model.CreatedDate = DateTime.Now;
					model.ModifiedDate = DateTime.Now;

					if (string.IsNullOrEmpty(model.SeoTitle))
					{
						model.SeoTitle = model.Title;
					}
					if (string.IsNullOrEmpty(model.Alias))
					{
						model.Alias = WebShoeShop.Models.Common.Filter.FilterChar(model.Title);
					}

					if (Images != null && Images.Count > 0)
					{
						for (int i = 0; i < Images.Count; i++)
						{
							var isDefault = (i + 1 == rDefault[0]);
							model.ProductImage.Add(new ProductImage
							{
								ProductId = model.Id,
								Image = Images[i],
								IsDefault = isDefault
							});
						}
					}

					if (Sizes != null && Sizes.Count > 0)
					{
						for (int i = 0; i < Sizes.Count; i++)
						{
							model.ProductSize.Add(new ProductSize
							{
								Size = Sizes[i],
								Quantity = Quantities[i]
							});
						}
					}

					db.Products.Add(model);
					db.SaveChanges();
					return RedirectToAction("Index");
				}
			}

			ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
			return View(model);
		}

		// Hiển thị view chỉnh sửa sản phẩm
		public ActionResult Edit(int id)
		{
			ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");

			// Lấy thông tin sản phẩm theo id
			var item = db.Products.Include("ProductSize").Include("ProductImage").FirstOrDefault(p => p.Id == id);

			if (item == null)
			{
				return HttpNotFound();
			}

			return View(item);
		}

		// Xử lý khi submit form chỉnh sửa sản phẩm
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(Product model, List<int> Sizes, List<int> Quantities, List<string> Images, List<int> rDefault)
		{
			if (ModelState.IsValid)
			{

				model.ModifiedDate = DateTime.Now;
				model.Alias = WebShoeShop.Models.Common.Filter.FilterChar(model.Title);


				var existingProduct = db.Products.Find(model.Id);
				if (existingProduct != null)
				{
					existingProduct.Title = model.Title;
					existingProduct.Description = model.Description;
					existingProduct.Price = model.Price;
					existingProduct.Quantity = model.Quantity;
					existingProduct.ProductCategoryId = model.ProductCategoryId;
					existingProduct.OriginalPrice2 = model.OriginalPrice2;
					existingProduct.Detail = model.Detail;
					existingProduct.PriceSale = model.PriceSale;
					existingProduct.IsActive = model.IsActive;
					existingProduct.IsSale = model.IsSale;
					existingProduct.IsHome = model.IsHome;
					existingProduct.IsFeature = model.IsFeature;

					db.ProductSizes.RemoveRange(existingProduct.ProductSize);
					existingProduct.ProductSize.Clear();

					if (Sizes != null && Sizes.Count > 0)
					{
						for (int i = 0; i < Sizes.Count; i++)
						{
							existingProduct.ProductSize.Add(new ProductSize
							{
								ProductId = existingProduct.Id,
								Size = Sizes[i],
								Quantity = Quantities[i]
							});
						}
					}

					db.ProductImages.RemoveRange(existingProduct.ProductImage);
					existingProduct.ProductImage.Clear();

					if (Images != null && Images.Count > 0)
					{
						for (int i = 0; i < Images.Count; i++)
						{
							var isDefault = (i + 1 == rDefault[0]);

							existingProduct.ProductImage.Add(new ProductImage
							{
								ProductId = existingProduct.Id,
								Image = Images[i],
								IsDefault = isDefault
							});
						}
					}
					db.SaveChanges();

					return RedirectToAction("Index");
				}
			}
			ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
			return View(model);
		}



		[HttpPost]
		public ActionResult Delete(int id)
		{
			var item = db.Products.Find(id);
			if (item != null)
			{
				var checkImg = item.ProductImage.Where(x => x.ProductId == item.Id);
				if (checkImg != null)
				{
					foreach (var img in checkImg)
					{
						db.ProductImages.Remove(img);
						db.SaveChanges();
					}
				}
				db.ProductSizes.RemoveRange(item.ProductSize);
				db.Products.Remove(item);
				db.SaveChanges();
				return Json(new { success = true });
			}

			return Json(new { success = false });
		}

		[HttpPost]
		public ActionResult IsActive(int id)
		{
			var item = db.Products.Find(id);
			if (item != null)
			{
				item.IsActive = !item.IsActive;
				db.Entry(item).State = System.Data.Entity.EntityState.Modified;
				db.SaveChanges();
				return Json(new { success = true, isAcive = item.IsActive });
			}

			return Json(new { success = false });
		}
		[HttpPost]
		public ActionResult IsHome(int id)
		{
			var item = db.Products.Find(id);
			if (item != null)
			{
				item.IsHome = !item.IsHome;
				db.Entry(item).State = System.Data.Entity.EntityState.Modified;
				db.SaveChanges();
				return Json(new { success = true, IsHome = item.IsHome });
			}

			return Json(new { success = false });
		}

		[HttpPost]
		public ActionResult IsSale(int id)
		{
			var item = db.Products.Find(id);
			if (item != null)
			{
				item.IsSale = !item.IsSale;
				db.Entry(item).State = System.Data.Entity.EntityState.Modified;
				db.SaveChanges();
				return Json(new { success = true, IsSale = item.IsSale });
			}

			return Json(new { success = false });
		}
		[HttpPost]
		public ActionResult IsFeature(int id)
		{
			var item = db.Products.Find(id);
			if (item != null)
			{
				item.IsFeature = !item.IsFeature;
				db.Entry(item).State = System.Data.Entity.EntityState.Modified;
				db.SaveChanges();
				return Json(new { success = true, IsFeature = item.IsFeature });
			}

			return Json(new { success = false });
		}
	}
}