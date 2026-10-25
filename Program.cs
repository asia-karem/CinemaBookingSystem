
using CinemaBookingSystem.Data;
using CinemaBookingSystem.Models;
using CinemaBookingSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CinemaBookingSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services
                .AddIdentity<IdentityUser, IdentityRole>(options =>
                {
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = false;
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });

            builder.Services.AddScoped<IAccountService, AccountService>();

            var app = builder.Build();

            // Seed database and Admin account
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var context =
                    services.GetRequiredService<ApplicationDbContext>();

                var roleManager =
                    services.GetRequiredService<RoleManager<IdentityRole>>();

                var userManager =
                    services.GetRequiredService<UserManager<IdentityUser>>();

                await context.Database.MigrateAsync();

                // Create Admin role
                const string adminRole = "Admin";

                if (!await roleManager.RoleExistsAsync(adminRole))
                {
                    var roleResult = await roleManager.CreateAsync(
                        new IdentityRole(adminRole));

                    if (!roleResult.Succeeded)
                    {
                        throw new Exception(
                            "Failed to create Admin role: " +
                            string.Join(", ",
                                roleResult.Errors.Select(e => e.Description)));
                    }
                }

                // Read Admin credentials from configuration/User Secrets
                var adminEmail = builder.Configuration["AdminSeed:Email"];
                var adminPassword = builder.Configuration["AdminSeed:Password"];

                if (string.IsNullOrWhiteSpace(adminEmail) ||
                    string.IsNullOrWhiteSpace(adminPassword))
                {
                    throw new InvalidOperationException(
                        "Configure AdminSeed:Email and AdminSeed:Password.");
                }

                // Find or create Admin user
                var adminUser = await userManager.FindByEmailAsync(adminEmail);

                if (adminUser == null)
                {
                    adminUser = new IdentityUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true
                    };

                    var createResult = await userManager.CreateAsync(
                        adminUser, adminPassword);

                    if (!createResult.Succeeded)
                    {
                        throw new Exception(
                            "Failed to create Admin user: " +
                            string.Join(", ",
                                createResult.Errors.Select(e => e.Description)));
                    }
                }

                // Ensure the Admin user has the Admin role
                var userRoles = await userManager.GetRolesAsync(adminUser);

                if (!userRoles.Contains(adminRole))
                {
                    var addRoleResult = await userManager.AddToRoleAsync(
                        adminUser, adminRole);

                    if (!addRoleResult.Succeeded)
                    {
                        throw new Exception(
                            "Failed to assign Admin role: " +
                            string.Join(", ",
                                addRoleResult.Errors.Select(e => e.Description)));
                    }
                }

                // Verify role assignment
                var verifiedRoles = await userManager.GetRolesAsync(adminUser);

                if (!verifiedRoles.Contains(adminRole))
                {
                    throw new InvalidOperationException(
                        $"Admin role was not assigned to {adminUser.Email}.");
                }

                // Seed Categories
                if (!await context.Categories.AnyAsync())
                {
                    context.Categories.AddRange(
                        new Category
                        {
                            Name = "Action",
                            Description = "Action movies"
                        },
                        new Category
                        {
                            Name = "Comedy",
                            Description = "Comedy movies"
                        },
                        new Category
                        {
                            Name = "Drama",
                            Description = "Drama movies"
                        },
                        new Category
                        {
                            Name = "Horror",
                            Description = "Horror movies"
                        },
                        new Category
                        {
                            Name = "Sci-Fi",
                            Description = "Science fiction movies"
                        },
                        new Category
                        {
                            Name = "Animation",
                            Description = "Animated movies"
                        }
                    );

                    await context.SaveChangesAsync();
                }

                // Seed Cinemas
                if (!await context.Cinemas.AnyAsync())
                {
                    context.Cinemas.AddRange(
                        new Cinema
                        {
                            Name = "Cinema 1",
                            Address = "Cairo, Egypt",
                            Description = "Main cinema hall"
                        },
                        new Cinema
                        {
                            Name = "Cinema 2",
                            Address = "Giza, Egypt",
                            Description = "Family cinema hall"
                        },
                        new Cinema
                        {
                            Name = "Cinema 3",
                            Address = "New Cairo, Egypt",
                            Description = "Premium cinema hall"
                        }
                    );

                    await context.SaveChangesAsync();
                }
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            await app.RunAsync();
        }
    }
}
