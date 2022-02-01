﻿using DatingApp.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DatingApp.Data_Transfer_Object
{
    public class Seed

    {
        public static async Task SeedUser(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            if (await userManager.Users.AnyAsync()) return;
            var userData = await System.IO.File.ReadAllTextAsync("Data Transfer Object/UserSeedData.json");
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);
            if (users == null) return;
            var roles = new List<AppRole>
            {
                new AppRole{Name = "Member"},
                new AppRole{Name = "Admin"},
                new AppRole{Name = "Moderator"}
                // new AppRole{Name = "VIP"}
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

            var adminUser = new AppUser
            {
                UserName = "admin"
            };

            await userManager.CreateAsync(adminUser, "Pa$$w0rd");
            await userManager.AddToRolesAsync(adminUser, new [] {"Admin", "Moderator"});
        }
            // await userManager.SaveChangesAsync();
    }
}
