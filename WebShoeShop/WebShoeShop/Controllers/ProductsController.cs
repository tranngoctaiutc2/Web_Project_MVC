using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebShoeShop.Models;
using WebShoeShop.Models.EF;

namespace WebShoeShop.Controllers
{
	public class ProductsController : Controller
	{
		private ApplicationDbContext db = new ApplicationDbContext();
		// GET: Products
		public ActionResult Index()
		{

			var items = db.Products.Where(x => x.IsActive).Take(100).ToList();
			return View(items);
		}

		public ActionResult Detail(string alias, int? id)
		{
			var item = db.Products.Find(id);
			if (item != null)
			{
				db.Products.Attach(item);
				item.ViewCount++;
				db.Entry(item).Property(x => x.ViewCount).IsModified = true;
				db.SaveChanges();
			}
			var countReView = db.Reviews.Where(x => x.ProductId == id).Count();
			ViewBag.CountReView = countReView;
			var relatedProducts = db.Products.Where(p => p.ProductCategoryId == item.ProductCategoryId && p.Id != item.Id).ToList();
			ViewBag.RelatedProducts = relatedProducts;
			return View(item);
		}
		public ActionResult ProductCategory(string alias, int id)
		{
			var items = db.Products.ToList();
			if (id > 0)
			{
				items = items.Where(x => x.ProductCategoryId == id).ToList();
			}
			var cate = db.ProductCategories.Find(id);
			if (cate != null)
			{
				ViewBag.CateName = cate.Title;
			}

			ViewBag.CateId = id;
			return View(items);
		}

		public ActionResult Partial_ItemsByCateId()
		{
			var items = db.Products.Where(x => x.IsHome && x.IsActive).Take(100).ToList();
			return PartialView(items);
		}

		public ActionResult Partial_ProductSales()
		{
			var items = db.Products.Where(x => x.IsSale && x.IsActive).Take(12).ToList();
			return PartialView(items);
		}
		public ActionResult Partial_ProductFeature()
		{
			var items = db.Products.Where(x => x.IsFeature && x.IsActive).Take(12).ToList();
			return PartialView(items);
		}
		public ActionResult _Review()
		{
			return PartialView();
		}
		[HttpGet]
		public ActionResult GetProductDetails(int id)
		{
			var product = db.Products
		   .Where(p => p.Id == id)
		   .Select(p => new ProductDetailsViewModel
		   {
			   Product = p,
			   Image = p.ProductImage.FirstOrDefault(x => x.IsDefault).Image,
			   ProductSizes = db.ProductSizes
				   .Where(ps => ps.ProductId == p.Id)
				   .Select(ps => new ProductSizeViewModel
				   {
					   Size = ps.Size,
					   Quantity = ps.Quantity ?? 0
				   })
				   .ToList()
		   })
		   .FirstOrDefault();

			if (product == null) return HttpNotFound();

			return PartialView("_ProductDetailsPartial", product);

		}
		public class ProductDetailsViewModel
		{
			public Product Product { get; set; }
			public List<ProductSizeViewModel> ProductSizes { get; set; }
			public string Image { get; set; }
		}

		public class ProductSizeViewModel
		{
			public int Size { get; set; }
			public int Quantity { get; set; }
		}
	}
}