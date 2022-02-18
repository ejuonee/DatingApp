using DatingApp.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace DatingApp.Data_Transfer_Object
{
    public class Seed

    {
        public static async Task SeedUsers(UserManager<AppUser> userManager,RoleManager<AppRole> roleManager)
        {
            //  RoleManager<AppRole> roleManager
            if (await userManager.Users.AnyAsync()) return;
            var userData = await System.IO.File.ReadAllTextAsync("DataTransferObject/UserSeedData.json");
            // var users = JsonSerializer.Deserialize<List<AppUser>>(userData);
            var users = JsonConvert.DeserializeObject<List<AppUser>>(userData);
            if (users == null) return;
            var roles = new List<AppRole>
            {
                new AppRole{Name = "Member"},
                new AppRole{Name = "Admin"},
                new AppRole{Name = "Moderator"},
                new AppRole{Name = "VIP"},
                new AppRole{Name = "Chairman"},
                new AppRole{Name = "Vice-Chairman"},
                new AppRole{Name = "Secretary"},
                new AppRole{Name = "Treasurer"},
                new AppRole{Name = "Auditor"},
                new AppRole{Name = "President"},
                new AppRole{Name = "Vice-President"},
                
            };

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(role);
            }
            foreach (var user in users)
            {
                // using var hmac = new HMACSHA512();
                user.UserName = user.UserName.ToLower();
                 await userManager.CreateAsync(user, "Pa$$w0rd");
                 await userManager.AddToRoleAsync(user, "Member");
                // user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("password"));
                // user.PasswordSalt = hmac.Key;

                
            }

            var admin = new AppUser
            {
                UserName = "admin"
            };

            await userManager.CreateAsync(admin, "Pa$$w0rd");
            await userManager.AddToRolesAsync(admin, new [] {"Admin", "Moderator"});
        }
            // await userManager.SaveChangesAsync();
    }
}
