using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API.DTOs;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedUsers(AppDbContext context)
        {
            if (await context.Users.AnyAsync()) return;

            var memberData = await File.ReadAllTextAsync("Data/UserSeedData.json");

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var members = JsonSerializer.Deserialize<List<SeedUserDto>>(memberData, options);

            if (members == null) return;

            foreach (var memberDto in members)
            {
                using var hmac = new HMACSHA512();

                var user = new AppUser
                {
                    Id = memberDto.Id,
                    Email = memberDto.Email,
                    DisplayName = memberDto.DisplayName,
                    ImageUrl = memberDto.ImageUrl,
                    PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Pa$$w0rd")),
                    PasswordSalt = hmac.Key
                };

                var member = new Member
                {
                    Id = memberDto.Id,
                    DisplayName = memberDto.DisplayName,
                    DateOfBirth = memberDto.DateOfBirth,
                    Gender = memberDto.Gender,
                    Description = memberDto.Description,
                    City = memberDto.City,
                    Country = memberDto.Country,
                    Created = memberDto.Created,
                    LastActive = memberDto.LastActive,
                    ImageUrl = memberDto.ImageUrl,
                    User = user,
                    Photos =
                    [
                        new Photo
                        {
                            Url = memberDto.ImageUrl,
                            MemberId = memberDto.Id
                        }
                    ]
                };

                context.Members.Add(member);
            }

            await context.SaveChangesAsync();
        }
    }
}
