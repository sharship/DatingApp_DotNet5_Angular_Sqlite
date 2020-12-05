
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedUsersAsync(DataContext context)
        {
            if(await context.Users.AnyAsync()) return;

            // get list of users to seed into DB
            var userRawData = await System.IO.File.ReadAllTextAsync("Data/UserSeedData.json");
            var users = JsonSerializer.Deserialize<List<AppUser>>(userRawData);

            if(users == null) return;
            // create password and salt for each user, and add them to DBContext one by one
            foreach (var user in users)
            {
                using var hmac = new HMACSHA512();

                user.UserName = user.UserName.ToLower();
                user.PasswordSalt = hmac.Key;
                user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("P@ssword"));

                await context.Users.AddAsync(user);
            }

            await context.SaveChangesAsync();
        }
    }
}