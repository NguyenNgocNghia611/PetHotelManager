namespace PetHotelManager.Data.Seed
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using PetHotelManager.Data;
    using PetHotelManager.Models;

    public static class DataSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            string[] roleNames = { "Admin", "Staff", "Doctor", "Veterinarian", "Customer" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // --- ADMIN ---
            var adminUser = await userManager.FindByEmailAsync("admin@pethotel.com");
            if (adminUser == null)
            {
                var newAdminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@pethotel.com",
                    FullName = "Quản trị viên",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newAdminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdminUser, "Admin");
                }
            }

            // --- STAFF ---
            var staffEmails = new[]
            {
                "staff1@pethotel.com",
                "staff2@pethotel.com",
                "staff3@pethotel.com",
                "staff4@pethotel.com",
                "staff5@pethotel.com"
            };

            foreach (var email in staffEmails)
            {
                if (!context.Users.Any(u => u.Email == email))
                {
                    var staff = new ApplicationUser
                    {
                        UserName = email.Split('@')[0],
                        Email = email,
                        FullName = $"Nhân viên {email.Split('@')[0]}",
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(staff, "Staff@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(staff, "Staff");
                    }
                }
            }

            // --- VETERINARIAN ---
            var vetEmails = new[]
            {
                "vet1@pethotel.com",
                "vet2@pethotel.com",
                "vet3@pethotel.com"
            };

            foreach (var email in vetEmails)
            {
                if (!context.Users.Any(u => u.Email == email))
                {
                    var vet = new ApplicationUser
                    {
                        UserName = email.Split('@')[0],
                        Email = email,
                        FullName = $"Bác sĩ thú y {email.Split('@')[0]}",
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(vet, "Veterinarian@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(vet, "Veterinarian");
                    }
                }
            }

            await context.SaveChangesAsync();

            // --- DOCTOR ---
            var doctorEmails = new[]
            {
                "doctor1@pethotel.com",
                "doctor2@pethotel.com",
                "doctor3@pethotel.com",
                "doctor4@pethotel.com",
                "doctor5@pethotel.com"
            };

            foreach (var email in doctorEmails)
            {
                if (!context.Users.Any(u => u.Email == email))
                {
                    var doctor = new ApplicationUser
                    {
                        UserName = email.Split('@')[0],
                        Email = email,
                        FullName = $"Bác sĩ {email.Split('@')[0]}",
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(doctor, "Doctor@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(doctor, "Doctor");
                    }
                }
            }

            await context.SaveChangesAsync();

            // --- PETS ---
            if (!await context.Pets.AnyAsync())
            {
                var staffUsers = await context.Users
                    .Where(u => staffEmails.Contains(u.Email))
                    .ToListAsync();

                if (staffUsers.Count > 0)
                {
                    context.Pets.AddRange(
                        new Pet { Name = "Cún Mít", Species = "Dog", Breed = "Poodle", Age = 2, Color = "Brown", HealthStatus = "Khỏe mạnh", UserId = staffUsers[0].Id },
                        new Pet { Name = "Mèo Bông", Species = "Cat", Breed = "Ba Tư", Age = 1, Color = "White", HealthStatus = "Đang điều trị da liễu", UserId = staffUsers[1].Id },
                        new Pet { Name = "Corgi Nâu", Species = "Dog", Breed = "Corgi", Age = 3, Color = "Golden", HealthStatus = "Khỏe mạnh", UserId = staffUsers[2].Id },
                        new Pet { Name = "Miu Miu", Species = "Cat", Breed = "Anh Lông Ngắn", Age = 2, Color = "Gray", HealthStatus = "Đang mang thai", UserId = staffUsers[3].Id },
                        new Pet { Name = "Chó Béo", Species = "Dog", Breed = "Husky", Age = 4, Color = "Black & White", HealthStatus = "Béo phì nhẹ", UserId = staffUsers[4].Id }
                    );
                    await context.SaveChangesAsync();
                }
            }

            // --- ROOM TYPES ---
            if (!await context.RoomTypes.AnyAsync())
            {
                context.RoomTypes.AddRange(
                    new RoomType { TypeName = "Standard", Description = "Phòng tiêu chuẩn cơ bản", PricePerDay = 150000 },
                    new RoomType { TypeName = "Deluxe", Description = "Phòng thoải mái hơn với đồ chơi", PricePerDay = 250000 },
                    new RoomType { TypeName = "VIP", Description = "Phòng cao cấp có điều hòa & camera", PricePerDay = 400000 },
                    new RoomType { TypeName = "Suite", Description = "Phòng siêu sang, không gian lớn", PricePerDay = 600000 },
                    new RoomType { TypeName = "Medical", Description = "Phòng y tế, chăm sóc đặc biệt", PricePerDay = 300000 }
                );
                await context.SaveChangesAsync();
            }

            // --- SERVICES ---
            if (!await context.Services.AnyAsync())
            {
                context.Services.AddRange(
                    new Service { Name = "Tắm gội", Category = "Chăm sóc", Price = 100000, Unit = "Lần" },
                    new Service { Name = "Cắt tỉa lông", Category = "Chăm sóc", Price = 150000, Unit = "Lần" },
                    new Service { Name = "Tiêm phòng", Category = "Y tế", Price = 200000, Unit = "Mũi" },
                    new Service { Name = "Khám tổng quát", Category = "Y tế", Price = 300000, Unit = "Lần" },
                    new Service { Name = "Lưu trú qua đêm", Category = "Lưu trú", Price = 400000, Unit = "Ngày" }
                );
                await context.SaveChangesAsync();
            }

            // --- ROOMS ---
            if (!await context.Rooms.AnyAsync())
            {
                var roomTypes = await context.RoomTypes.ToListAsync();

                context.Rooms.AddRange(
                    new Room { RoomNumber = "R101", RoomTypeId = roomTypes.First(t => t.TypeName == "Standard").Id, Status = "Trống" },
                    new Room { RoomNumber = "R102", RoomTypeId = roomTypes.First(t => t.TypeName == "Deluxe").Id, Status = "Đang dọn" },
                    new Room { RoomNumber = "R103", RoomTypeId = roomTypes.First(t => t.TypeName == "Medical").Id, Status = "Trống" },
                    new Room { RoomNumber = "R104", RoomTypeId = roomTypes.First(t => t.TypeName == "VIP").Id, Status = "Trống" },
                    new Room { RoomNumber = "R105", RoomTypeId = roomTypes.First(t => t.TypeName == "Suite").Id, Status = "Bảo trì" }
                );
                await context.SaveChangesAsync();
            }

            // --- APPOINTMENTS ---
            if (!await context.Appointments.AnyAsync())
            {
                var pets = await context.Pets.Take(5).ToListAsync();
                var services = await context.Services.Take(5).ToListAsync();
                var rooms = await context.Rooms.Take(5).ToListAsync();

                for (int i = 0; i < 5; i++)
                {
                    context.Appointments.Add(new Appointment
                    {
                        UserId = pets[i].UserId,
                        PetId = pets[i].Id,
                        ServiceId = services[i].Id,
                        RoomId = rooms[i].Id,
                        AppointmentDate = DateTime.UtcNow.AddDays(i + 1),
                        Status = i switch
                        {
                            0 => "Pending",
                            1 => "Accepted",
                            2 => "Rejected",
                            3 => "Cancelled",
                            _ => "Pending"
                        },
                        Notes = $"Lịch hẹn test #{i + 1}"
                    });
                }

                await context.SaveChangesAsync();
            }
        }
    }
}
