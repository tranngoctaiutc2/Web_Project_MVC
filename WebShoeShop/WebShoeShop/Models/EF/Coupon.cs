using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebShoeShop.Models.EF
{
	[Table("tb_Coupon")]
	public class Coupon
	{
		[Key]
		public int Id { get; set; }

		[Required]
		[StringLength(50)]
		public string Code { get; set; }

		[StringLength(255)]
		[Display(Name = "Mô tả")]
		public string Description { get; set; }

		[Display(Name = "Số tiền giảm giá")]
		public decimal? DiscountAmount { get; set; }

		[Display(Name = "Phần trăm giảm giá")]
		public decimal? DiscountPercentage { get; set; }

		[Display(Name = "Số tiền giảm tối đa")]
		public decimal? MaxDiscountAmount { get; set; }

		[Display(Name = "Số tiền tối thiểu cho đơn hàng")]
		public decimal? MinimumOrderAmount { get; set; }

		[Required]
		[Display(Name = "Ngày bắt đầu")]
		public DateTime StartDate { get; set; }

		[Required]
		[Display(Name = "Ngày hết hạn")]
		public DateTime ExpirationDate { get; set; }

		public bool IsActive { get; set; }

		[Display(Name = "Số lần sử dụng")]
		public int? UsageLimit { get; set; }

		[Display(Name = "Số lần đã sử dụng")]
		public int UsageCount { get; set; }

	}

}