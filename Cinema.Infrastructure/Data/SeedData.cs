using Cinema.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cinema.Infrastructure.Data;

// Ilk calistirmada rol, admin ve varsayilan salon/koltuk verilerini hazirlar.
public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration config)
    {
        using var scope = services.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // 1) Roller
        string[] roles = ["SuperAdmin", "Admin", "User"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // 2) SuperAdmin kullanicisi
        var email = config["Seed:SuperAdminEmail"];
        var password = config["Seed:SuperAdminPassword"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new Exception("Seed SuperAdmin bilgileri appsettings.json icinde eksik (Seed:SuperAdminEmail / Seed:SuperAdminPassword).");

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                throw new Exception("SuperAdmin olusturulamadi: " + errors);
            }
        }

        if (!await userManager.IsInRoleAsync(user, "SuperAdmin"))
            await userManager.AddToRoleAsync(user, "SuperAdmin");

        // 3) Varsayilan salon + koltuklar (seat map ekraninin calismasi icin temel veri)
        await SeedDefaultHallsAndSeatsAsync(dbContext);

        // 4) Rozet tanimlari
        await SeedBadgesAsync(dbContext);
    }

    private static async Task SeedDefaultHallsAndSeatsAsync(AppDbContext dbContext)
    {
        // VIP salon: az koltuk, yuksek konfor.
        var vipHall = await dbContext.Halls.FirstOrDefaultAsync(h => h.Name == "VIP Salon");
        if (vipHall == null)
        {
            vipHall = new Hall
            {
                Name = "VIP Salon",
                Description = "VIP salon - az koltuk, ozel hizmet"
            };
            dbContext.Halls.Add(vipHall);
            await dbContext.SaveChangesAsync();
        }

        var vipHasSeats = await dbContext.Seats.AnyAsync(s => s.HallId == vipHall.Id);
        if (!vipHasSeats)
        {
            var vipRows = new (string Row, int Count)[]
            {
                ("A", 6),
                ("B", 6),
                ("C", 6),
                ("D", 6)
            };

            foreach (var (row, count) in vipRows)
            {
                for (var seatNo = 1; seatNo <= count; seatNo++)
                {
                    dbContext.Seats.Add(new Seat
                    {
                        HallId = vipHall.Id,
                        RowLabel = row,
                        SeatNumber = seatNo,
                        IsActive = true,
                        IsAisleAfter = seatNo == 3
                    });
                }
            }

            await dbContext.SaveChangesAsync();
        }

        // Ekran goruntusune yakin bir koltuk duzeni.
        // Not: Bu sadece default seed; ileride admin tarafindan salon/koltuk editoru gelebilir.
        var rowConfigs = new (string Row, int Count)[]
        {
            ("A", 19),
            ("B", 20),
            ("C", 20),
            ("D", 20),
            ("E", 18),
            ("F", 18),
            ("G", 18),
            ("H", 18),
            ("I", 18),
            ("J", 24)
        };

        for (var hallNumber = 1; hallNumber <= 6; hallNumber++)
        {
            var hallName = $"Salon {hallNumber}";
            var hall = await dbContext.Halls.FirstOrDefaultAsync(h => h.Name == hallName);

            if (hall == null)
            {
                hall = new Hall
                {
                    Name = hallName,
                    Description = hallNumber == 1
                        ? "Ana salon - standart koltuk duzeni"
                        : $"{hallName} - standart koltuk duzeni"
                };

                dbContext.Halls.Add(hall);
                await dbContext.SaveChangesAsync();
            }

            var hasSeats = await dbContext.Seats.AnyAsync(s => s.HallId == hall.Id);
            if (hasSeats)
                continue;

            foreach (var (row, count) in rowConfigs)
            {
                for (var seatNo = 1; seatNo <= count; seatNo++)
                {
                    dbContext.Seats.Add(new Seat
                    {
                        HallId = hall.Id,
                        RowLabel = row,
                        SeatNumber = seatNo,
                        IsActive = true,
                        // Gorsel orta koridor hissi icin belirli koltuklardan sonra bosluk veriyoruz.
                        IsAisleAfter = row is "A" or "B" or "C" or "D" ? seatNo == 10 : seatNo == 9
                    });
                }
            }

            await dbContext.SaveChangesAsync();
        }
    }

    private static async Task SeedBadgesAsync(AppDbContext dbContext)
    {
        var definitions = new (string Code, string Title, string Description, string Requirement)[]
        {
            ("FIRST_TICKET", "Ilk Bilet", "Ilk biletini satin aldiginda kazanirsin.", "1 bilet"),
            ("REVIEWER", "Yorumcu", "5 yorum yaptiginda kazanirsin.", "5 yorum"),
            ("CRITIC", "Elestirmen", "20 yorum yaptiginda kazanirsin.", "20 yorum"),
            ("FAVORITER", "Favorici", "5 favori film eklediginde kazanirsin.", "5 favori"),
            ("RATER", "Puanlayici", "Ilk puan verdiginde kazanirsin.", "1 puan"),
            ("LOYAL", "Sadik Izleyici", "10 bilet satin aldiginda kazanirsin.", "10 bilet")
        };

        foreach (var def in definitions)
        {
            var exists = await dbContext.Badges.AnyAsync(b => b.Code == def.Code);
            if (exists)
                continue;

            dbContext.Badges.Add(new Badge
            {
                Code = def.Code,
                Title = def.Title,
                Description = def.Description,
                RequirementText = def.Requirement
            });
        }

        await dbContext.SaveChangesAsync();
    }
}
