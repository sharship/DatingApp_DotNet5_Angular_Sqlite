using System.Linq;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedUsersAsync(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            if(await userManager.Users.AnyAsync()) return;

            #region seed roles
            var roles = new List<AppRole>
            {
                new AppRole { Name = "Member" },
                new AppRole { Name = "Admin" },
                new AppRole { Name = "Moderator" },
            };

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(role);
            }

            #endregion


            #region seed users
            // get list of users to seed into DB
            var userRawData = await System.IO.File.ReadAllTextAsync("Data/UserSeedData.json");
            var users = JsonSerializer.Deserialize<List<AppUser>>(userRawData);

            if(users == null) return;

            
            foreach (var user in users)
            {
                user.UserName = user.UserName.ToLower();

                // add password for each user, and add them to DBContext one by one
                await userManager.CreateAsync(user, "P@ssw0rd");

                // add current user to named role
                await userManager.AddToRoleAsync(user, "Member");

                // set initial photo as approved for seeded users
                user.Photos.ToList().ForEach(p => p.IsApproved = true);
            }

            #endregion


            #region seed admin
            var admin = new AppUser
            {
                UserName = "admin",
            };

            await userManager.CreateAsync(admin, "P@ssw0rd");
            await userManager.AddToRolesAsync(admin, new List<string> { "Admin", "Moderator" });
            
            #endregion
            
        }
    }
}