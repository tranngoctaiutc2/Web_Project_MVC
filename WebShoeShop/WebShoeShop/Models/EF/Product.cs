﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web.Mvc;

namespace WebShoeShop.Models.EF
{
	[Table("tb_Product")]
	public class Product : CommonAbstract
	{
		public Product()
		{
			this.ProductImage = new HashSet<ProductImage>();
			this.ProductSize = new HashSet<ProductSize>();
			this.OrderDetails = new HashSet<OrderDetail>();
			this.Reviews = new HashSet<ReviewProduct>();
			this.Wishlists = new HashSet<Wishlist>();
		}
		[Key]
		[DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		[Required(ErrorMessage = "Tên sản phẩm không được để trống")]
		[StringLength(250)]
		public string Title { get; set; }


		[StringLength(250)]
		public string Alias { get; set; }

		[StringLength(50)]
		public string ProductCode { get; set; }
		public string Description { get; set; }

		[AllowHtml]
		public string Detail { get; set; }

		[StringLength(250)]
		public string Image { get; set; }
		[Required(ErrorMessage = "Giá nhập không được để trống")]
		public decimal OriginalPrice2 { get; set; }

		[Required(ErrorMessage = "Giá sản phẩm không được để trống")]
		public decimal Price { get; set; }
		public decimal? PriceSale { get; set; }
		[Required(ErrorMessage = "Số lượng không được để trống")]
		public int Quantity { get; set; }
		public int SoldQuantity { get; set; } = 0;
		public int ViewCount { get; set; }
		public bool IsHome { get; set; }
		public bool IsSale { get; set; }
		public bool IsFeature { get; set; }
		public bool IsHot { get; set; }
		public bool IsActive { get; set; }
		[Required(ErrorMessage = "Vui lòng chọn danh mục sản phẩm")]
		public int ProductCategoryId { get; set; }
		[StringLength(250)]
		public string SeoTitle { get; set; }
		[StringLength(500)]
		public string SeoDescription { get; set; }
		[StringLength(250)]
		public string SeoKeywords { get; set; }
		public int? BrandId { get; set; }
		public virtual Brand Brands { get; set; }
		public virtual ProductCategory ProductCategory { get; set; }
		public virtual ICollection<ProductImage> ProductImage { get; set; }
		public virtual ICollection<ProductSize> ProductSize { get; set; }
		public virtual ICollection<OrderDetail> OrderDetails { get; set; }
		public virtual ICollection<ReviewProduct> Reviews { get; set; }
		public virtual ICollection<Wishlist> Wishlists { get; set; }
		public int TotalQuantity
		{
			get
			{
				return ProductSize?.Sum(ps => ps.Quantity) ?? 0;
			}
		}
		public void ReduceQuantity(int quantity, int size)
		{
			var sizeWithStock = ProductSize.FirstOrDefault(ps => ps.Size == size);


			if (sizeWithStock == null)
			{
				throw new InvalidOperationException($"Size {size} không tồn tại trong kho.");
			}


			if (sizeWithStock.Quantity >= quantity)
			{
				sizeWithStock.Quantity -= quantity;
			}
			else
			{
				throw new InvalidOperationException("Không đủ hàng trong kho để đặt.");
			}
		}
		public void ReturnQuantity(int quantity, int size)
		{
			var sizeWithStock = ProductSize.FirstOrDefault(ps => ps.Size == size);

			if (sizeWithStock == null)
			{
				throw new InvalidOperationException($"Size {size} không tồn tại trong kho.");
			}
			sizeWithStock.Quantity += quantity;
		}
	}
}