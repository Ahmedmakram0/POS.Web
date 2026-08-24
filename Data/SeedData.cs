using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using POS.Web.Models.Entities;
using POS.Web.Models.Enums;
using POS.Web.Models.Identity;

namespace POS.Web.Data;

public static class SeedData
{
    public static readonly string[] Roles = { "SuperAdmin", "Admin", "Cashier" };

    public static async Task InitializeAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        const string superAdminEmail = "admin@pos.local";
        if (await userManager.FindByEmailAsync(superAdminEmail) is null)
        {
            var superAdmin = new ApplicationUser
            {
                UserName = superAdminEmail,
                Email = superAdminEmail,
                EmailConfirmed = true,
                FullName = "مدير النظام",
                CanOverrideMinimumPrice = true,
                CanDiscountToMinimumPrice = true,
                MaxDiscountPercent = 100
            };

            var result = await userManager.CreateAsync(superAdmin, "Admin@12345");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
        }

        if (!await db.FinancialAccounts.AnyAsync())
        {
            db.FinancialAccounts.AddRange(
                new FinancialAccount { Type = FinancialAccountType.CashSafe, Name = "الخزينة النقدية" },
                new FinancialAccount { Type = FinancialAccountType.InstaPay, Name = "إنستاباي" },
                new FinancialAccount { Type = FinancialAccountType.VodafoneCash, Name = "فودافون كاش" },
                new FinancialAccount { Type = FinancialAccountType.CustomerReceivables, Name = "مديونية العملاء" },
                new FinancialAccount { Type = FinancialAccountType.SupplierPayables, Name = "مستحقات الموردين" }
            );
        }

        if (!await db.SystemSettings.AnyAsync())
            db.SystemSettings.Add(new SystemSettings());

        if (!await db.ReceiptSettings.AnyAsync())
            db.ReceiptSettings.Add(new ReceiptSettings());

        if (!await db.DiscountSettings.AnyAsync())
            db.DiscountSettings.Add(new DiscountSettings());

        await db.SaveChangesAsync();
    }
}
