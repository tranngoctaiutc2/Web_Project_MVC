using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebShoeShop.Models.EF
{
	[Table("tb_RolePermission")]
	public class RolePermission
	{
		public int Id { get; set; }
		[Required]
		public string RoleId { get; set; }
		[Required]
		[StringLength(100)]
		public string Controller { get; set; }
		[StringLength(100)]
		public string Action { get; set; }
		public virtual IdentityRole Role { get; set; }
	}
}