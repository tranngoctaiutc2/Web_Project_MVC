using System;
using System.Linq;
using WebShoeShop.Models;
using WebShoeShop.Models.EF;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Ajax.Utilities;

namespace WebShoeShop.Helpers
{
    public static class TestSeeder
    {
        public static string username = "admin2";

        public static void SeedTestUsers()
        {

            using (var context = new ApplicationDbContext())
            {
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

                // Tạo role Admin nếu chưa tồn tại
                if (!roleManager.RoleExists("Admin"))
                {
                    roleManager.Create(new IdentityRole("Admin"));
                }

                // Tạo user Admin nếu chưa tồn tại
                if (userManager.FindByName("admin1") == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = "admin1",
                        Email = "admin1@example.com",
                        FullName = "Administrator",
                        Phone = "1234567890",
                        EmailConfirmed = true
                    };

                    // Tạo user với mật khẩu và thêm vào role Admin
                    var result = userManager.Create(user, "0985181215thanH@");

                    if (result.Succeeded)
                    {
                        userManager.AddToRole(user.Id, "Admin");
                    }
                    else
                    {
                        throw new Exception("Seed user failed: " + string.Join(", ", result.Errors));
                    }
                }
            }
        }

        public static void SeedTestProducts()
        { }

        public static void ClearTestUsers()
        {
            using (var context = new ApplicationDbContext())
            {
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

                var user = userManager.FindByName(username);
                if (user != null)
                {
                    userManager.Delete(user);
                }
            }
        }
    }
}
