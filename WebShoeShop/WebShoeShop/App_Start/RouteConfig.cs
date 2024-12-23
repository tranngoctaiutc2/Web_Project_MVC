using System.Web.Mvc;
using System.Web.Routing;

namespace WebShoeShop
{
	public class RouteConfig
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
			routes.MapRoute(
			 name: "Contact",
			 url: "lien-he",
			 defaults: new { controller = "Contact", action = "Index", alias = UrlParameter.Optional },
			 namespaces: new[] { "WebShoeShop.Controllers" }
		 );
			routes.MapRoute(
			name: "Instruct_SizeProducts",
			url: "huong-dan",
			defaults: new { controller = "Contact", action = "Instruct_SizeProducts" }
		);
			routes.MapRoute(
			name: "FAQs",
			url: "FAQs",
			defaults: new { controller = "Contact", action = "FAQs" }
		);
			routes.MapRoute(
			name: "Policy",
			url: "chinh-sach",
			defaults: new { controller = "Contact", action = "Policy" }
		);
			routes.MapRoute(
			name: "Security",
			url: "bao-mat",
			defaults: new { controller = "Contact", action = "Security" }
		);
			routes.MapRoute(
			name: "Career",
			url: "tuyen-dung",
			defaults: new { controller = "Contact", action = "Career" }
		);
			routes.MapRoute(
			name: "Franchisepolicy",
			url: "nhuong-quyen",
			defaults: new { controller = "Contact", action = "Franchisepolicy" }
		);
			routes.MapRoute(
			name: "BaiViet",
			url: "post/{alias}",
			defaults: new { controller = "Article", action = "Index", alias = UrlParameter.Optional },
			namespaces: new[] { "WebShoeShop.Controllers" }
		);
			routes.MapRoute(
			  name: "DetailNews",
			  url: "{alias}-n{id}",
			  defaults: new { controller = "News", action = "Detail", id = UrlParameter.Optional },
			  namespaces: new[] { "WebShoeShop.Controllers" }
		  );
			routes.MapRoute(
			   name: "NewsList",
			   url: "tin-tuc",
			   defaults: new { controller = "News", action = "Index", alias = UrlParameter.Optional },
			   namespaces: new[] { "WebShoeShop.Controllers" }
		   );
			routes.MapRoute(
			name: "CheckOut",
			url: "thanh-toan",
			defaults: new { controller = "ShoppingCart", action = "CheckOut", alias = UrlParameter.Optional },
			namespaces: new[] { "WebShoeShop.Controllers" }
			);
			routes.MapRoute(
			name: "vnpay_return",
			url: "vnpay_return",
			defaults: new { controller = "ShoppingCart", action = "VnpayReturn", alias = UrlParameter.Optional },
			namespaces: new[] { "WebShoeShop.Controllers" }
		);
			routes.MapRoute(
			name: "ShoppingCart",
			url: "gio-hang",
			defaults: new { controller = "ShoppingCart", action = "Index", alias = UrlParameter.Optional },
			namespaces: new[] { "WebShoeShop.Controllers" }
		);
			routes.MapRoute(
			name: "ProductsByBrand",
			url: "products/brand/{brandId}",
			defaults: new { controller = "Products", action = "ProductsByBrand", brandId = UrlParameter.Optional },
			namespaces: new[] { "WebShoeShop.Controllers" }
		);

			routes.MapRoute(
				name: "CategoryProduct",
			  url: "danh-muc-san-pham/{alias}-{id}",
			  defaults: new { controller = "Products", action = "ProductCategory", id = UrlParameter.Optional },
			  namespaces: new[] { "WebShoeShop.Controllers" }
				);
			routes.MapRoute(
			  name: "detailProduct",
			  url: "chi-tiet/{alias}-p{id}",
			  defaults: new { controller = "Products", action = "Detail", alias = UrlParameter.Optional },
			  namespaces: new[] { "WebShoeShop.Controllers" }
		  );
			routes.MapRoute(
			   name: "Products",
			   url: "san-pham",
			   defaults: new { controller = "Products", action = "Index", alias = UrlParameter.Optional },
			   namespaces: new[] { "WebShoeShop.Controllers" }
		   );
			routes.MapRoute(
				name: "Default",
				url: "{controller}/{action}/{id}",
				defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
				namespaces: new[] { "WebShoeShop.Controllers" }
			);
		}
	}
}
