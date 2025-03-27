using Bogus;
using T3H.Poll.Domain.Entities;
using T3H.Poll.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using T3H.Poll.Domain.Enums;

namespace T3H.Poll.Migration.Data;

public static class DbInitializer
{
    private static readonly string DefaultPassword = "Abc@123";
    private static readonly PasswordHasher<User> PasswordHasher = new();
    private static readonly Faker Faker = new("vi");

    public static async Task Initialize(CrmDbContext context, ILogger<CrmDbContext> logger)
    {
        try
        {
            await context.Database.MigrateAsync();
            await SeedCommonSettings(context, logger);
            var roles = await SeedRoles(context, logger);
            var users = await SeedUsers(context, logger);
            await SeedUserRoles(context, users, roles, logger);
            var paymentTerms = await SeedPaymentTerms(context, logger);
            var paymentMethods = await SeedPaymentMethods(context, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private static async Task SeedCommonSettings(CrmDbContext context, ILogger<CrmDbContext> logger)
    {
        if (await context.CommonSettings.AnyAsync())
            return;

        logger.LogInformation("Seeding common settings...");
        var settings = new List<CommonSetting>
        {
            new() {
               
                Name = "Industry",
                Value = "Tài chính",
                Description = "Lĩnh vực tài chính",
                CreatedDateTime = DateTime.Now,
                UserNameCreated = "System",
                IsDeleted = false,
                SettingType = Domain.Enums.SettingType.Industry
            },
            new() {

                Name = "Industry",
                Value = "CNTT",
                Description = "Lĩnh vực CNTT",
                CreatedDateTime = DateTime.Now,
                UserNameCreated = "System",
                IsDeleted = false,
                SettingType = Domain.Enums.SettingType.Industry
            },
            new() {

                Name = "Industry",
                Value = "Giáo dục",
                Description = "Lĩnh vực giáo dục",
                CreatedDateTime = DateTime.Now,
                UserNameCreated = "System",
                IsDeleted = false,
                SettingType = Domain.Enums.SettingType.Industry
            },
            new() {

                Name = "Industry",
                Value = "Xây dưng dân dụng",
                Description = "Lĩnh vực Xây dưng dân dụng",
                CreatedDateTime = DateTime.Now,
                UserNameCreated = "System",
                IsDeleted = false,
                SettingType = Domain.Enums.SettingType.Industry
            },
             new() {

                Name = "Industry",
                Value = "Kỹ thuật cơ khí",
                Description = "Lĩnh vực  Kỹ thuật cơ khí",
                CreatedDateTime = DateTime.Now,
                UserNameCreated = "System",
                IsDeleted = false,
                SettingType = Domain.Enums.SettingType.Industry
            },
             new() {

                Name = "Industry",
                Value = "Kỹ thuật điện",
                Description = "Lĩnh vực  Kỹ thuật cơ khí",
                CreatedDateTime = DateTime.Now,
                UserNameCreated = "System",
                IsDeleted = false,
                SettingType = Domain.Enums.SettingType.Industry
            }
        };

        await context.CommonSettings.AddRangeAsync(settings);
        await context.SaveChangesAsync();
    }


    private static async Task<List<Role>> SeedRoles(CrmDbContext context, ILogger<CrmDbContext> logger)
    {
        if (await context.Roles.AnyAsync())
            return await context.Roles.ToListAsync();

        logger.LogInformation("Seeding roles...");
        var roles = new List<Role>
        {
            new Role { Name = "SuperAdmin", ConcurrencyStamp = Guid.NewGuid().ToString(), NormalizedName = "SuperAdmin", CreatedDateTime = DateTime.Now, UserNameCreated = "System", IsDeleted = false },
            new Role { Name = "Admin", ConcurrencyStamp = Guid.NewGuid().ToString(), NormalizedName = "Admin", CreatedDateTime = DateTime.Now, UserNameCreated = "System", IsDeleted = false },
            new Role { Name = "Manager",ConcurrencyStamp = Guid.NewGuid().ToString(), NormalizedName = "Manager", CreatedDateTime = DateTime.Now, UserNameCreated = "System", IsDeleted = false },
            new Role { Name = "Employee",ConcurrencyStamp = Guid.NewGuid().ToString(), NormalizedName = "Employee", CreatedDateTime = DateTime.Now, UserNameCreated = "System", IsDeleted = false },
            new Role { Name = "Sales", ConcurrencyStamp = Guid.NewGuid().ToString(), NormalizedName = "Sales", CreatedDateTime = DateTime.Now, UserNameCreated = "System", IsDeleted = false },
            new Role { Name = "Marketing",ConcurrencyStamp = Guid.NewGuid().ToString(), NormalizedName = "Marketing", CreatedDateTime = DateTime.Now, UserNameCreated = "System", IsDeleted = false }
        };

        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();
        return roles;
    }
    
    private static async Task<List<User>> SeedUsers(CrmDbContext context, ILogger<CrmDbContext> logger)
    {
        if (await context.Users.AnyAsync())
            return await context.Users.ToListAsync();

        logger.LogInformation("Seeding users...");
        // var users = new List<User> { CreateAdminUser(departments[0].Id) }; // Admin thuộc Ban giám đốc
        // users.AddRange(CreateManagerUsers(departments.Skip(1).ToList())); // Mỗi manager cho một phòng còn lại
        var users = new List<User>
        {
            CreateAdminUser(Guid.NewGuid())
        };
        for (int i = 0; i < 5; i++)
        {
            users.Add(CreateManagerUser(Guid.NewGuid()));
        }

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
        return users;
    }

    private static async Task SeedUserRoles(CrmDbContext context, List<User> users, List<Role> roles, ILogger<CrmDbContext> logger)
    {
        if (await context.UserRoles.AnyAsync())
            return;

        logger.LogInformation("Seeding user roles...");
        var userRoles = new List<UserRole>();
        {
            new UserRole { UserId = users[0].Id, RoleId = roles.First(r => r.Name == "SuperAdmin").Id };
        };

        for (int i = 1; i < users.Count; i++)
        {
            userRoles.Add(new UserRole{ UserId = users[i].Id, RoleId = roles.First(r => r.Name == "Manager").Id }); // Các manager là Manager
        }

        await context.UserRoles.AddRangeAsync(userRoles);
        await context.SaveChangesAsync();
    }

    private static async Task<List<PaymentTerm>> SeedPaymentTerms(CrmDbContext context, ILogger<CrmDbContext> logger)
    {
        if (await context.PaymentTerms.AnyAsync())
            return await context.PaymentTerms.ToListAsync();

        logger.LogInformation("Seeding payment terms...");
        var paymentTerms = new List<PaymentTerm>
        {
            PaymentTerm.Create("Thanh toán ngay", 1),
            PaymentTerm.Create("Thanh toán trong vòng 15 ngày", 2),
            PaymentTerm.Create("Thanh toán trong vòng 30 ngày", 3),
            PaymentTerm.Create("Thanh toán trong vòng 45 ngày", 4),
            PaymentTerm.Create("Thanh toán trong vòng 60 ngày", 5)
        };

        foreach (var term in paymentTerms)
        {
            term.CreatedDateTime = DateTime.Now;
            term.UserNameCreated = "System";
            term.IsDeleted = false;
        }

        await context.PaymentTerms.AddRangeAsync(paymentTerms);
        await context.SaveChangesAsync();
        return paymentTerms;
    }

    private static async Task<List<PaymentMethod>> SeedPaymentMethods(CrmDbContext context, ILogger<CrmDbContext> logger)
    {
        if (await context.PaymentMethods.AnyAsync())
            return await context.PaymentMethods.ToListAsync();

        logger.LogInformation("Seeding payment methods...");
        var paymentMethods = new List<PaymentMethod>
        {
            PaymentMethod.Create(
                "Tiền mặt",
                "Thanh toán trực tiếp bằng tiền mặt"
            ),
            PaymentMethod.Create(
                "Chuyển khoản ngân hàng",
                "Chuyển khoản qua tài khoản ngân hàng"
            ),
            PaymentMethod.Create(
                "Ví MoMo",
                "Thanh toán qua ví điện tử MoMo"
            ),
            PaymentMethod.Create(
                "VNPay",
                "Thanh toán qua cổng thanh toán VNPay"
            ),
            PaymentMethod.Create(
                "Thẻ tín dụng",
                "Thanh toán bằng thẻ Visa/Master/JCB"
            )
        };      

        await context.PaymentMethods.AddRangeAsync(paymentMethods);
        await context.SaveChangesAsync();
        return paymentMethods;
    }

    #region Helper Methods

    private static User CreateAdminUser(Guid departmentId)
    {
        var user = new User
        {           
            UserName = "admin",
            Email = "admin@system.com",
            FullName = "System Administrator",
            PhoneNumber = "0900000000",
            DepartmentId = departmentId,
            CreatedDateTime = DateTime.Now,
            UserNameCreated = "System",
            IsDeleted = false,
            SecurityStamp = Guid.NewGuid().ToString(),
            NormalizedEmail = "ADMIN@SYSTEM.COM",
            NormalizedUserName = "ADMIN",
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            AzureAdB2CUserId = "System",
            ColorAvatar = CrossCuttingConcerns.Helper.StringHelpers.GetRandomColor()
        };
        user.PasswordHash = PasswordHasher.HashPassword(user, DefaultPassword);
        return user;
    }

    private static User CreateManagerUser(Guid departmentId)
    {
        var user = new User
        {
            UserName = Faker.Internet.UserName(),
            Email = Faker.Internet.Email(),
            FullName = Faker.Name.FullName(),
            PhoneNumber = Faker.Phone.PhoneNumber("0#########"),
            DepartmentId = departmentId,
            CreatedDateTime = DateTime.Now,
            UserNameCreated = "System",
            IsDeleted = false,
            SecurityStamp = Guid.NewGuid().ToString(),
            NormalizedEmail = Faker.Internet.Email().ToUpper(),
            NormalizedUserName = Faker.Internet.UserName().ToUpper(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            AzureAdB2CUserId = "System",
            ColorAvatar = CrossCuttingConcerns.Helper.StringHelpers.GetRandomColor()
        };
        user.PasswordHash = PasswordHasher.HashPassword(user, DefaultPassword);
        return user;
    }

    #endregion
}
