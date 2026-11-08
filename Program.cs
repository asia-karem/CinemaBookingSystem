
using CinemaBookingSystem.Data;
using CinemaBookingSystem.Models;
using CinemaBookingSystem.Services;
using CinemaBookingSystem.Repositories;
using CinemaBookingSystem.Repositories.Interfaces;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CinemaBookingSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // MVC
            builder.Services.AddControllersWithViews();

            // Database
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")
                ));

            // Generic Repository
            builder.Services.AddScoped(
                typeof(IGenericRepository<>),
                typeof(GenericRepository<>)
            );

            // Movie Repository
            builder.Services.AddScoped<IMovieRepository, MovieRepository>();

            // Identity
            builder.Services
                .AddIdentity<IdentityUser, IdentityRole>(options =>
                {
                    // Password Settings
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = false;

                    // User Settings
                    options.User.RequireUniqueEmail = true;

                    // Email Confirmation
                    options.SignIn.RequireConfirmedEmail = true;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Cookie Configuration
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });

            // Application Services
            builder.Services.AddScoped<IAccountService, AccountService>();

            // Email Service
            builder.Services.AddTransient<IEmailSender, EmailSender>();

            var app = builder.Build();

            // =====================================
            // DATABASE SEEDING
            // =====================================

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

                // Create Admin Role
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
                                roleResult.Errors.Select(e => e.Description))
                        );
                    }
                }

                // Admin Credentials
                var adminEmail =
                    builder.Configuration["AdminSeed:Email"];

                var adminPassword =
                    builder.Configuration["AdminSeed:Password"];

                if (string.IsNullOrWhiteSpace(adminEmail) ||
                    string.IsNullOrWhiteSpace(adminPassword))
                {
                    throw new InvalidOperationException(
                        "Configure AdminSeed:Email and AdminSeed:Password."
                    );
                }

                // Find or Create Admin User
                var adminUser =
                    await userManager.FindByEmailAsync(adminEmail);

                if (adminUser == null)
                {
                    adminUser = new IdentityUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true
                    };

                    var createResult = await userManager.CreateAsync(
                        adminUser,
                        adminPassword
                    );

                    if (!createResult.Succeeded)
                    {
                        throw new Exception(
                            "Failed to create Admin user: " +
                            string.Join(", ",
                                createResult.Errors.Select(e => e.Description))
                        );
                    }
                }
                else if (!adminUser.EmailConfirmed)
                {
                    adminUser.EmailConfirmed = true;

                    var updateResult =
                        await userManager.UpdateAsync(adminUser);

                    if (!updateResult.Succeeded)
                    {
                        throw new Exception(
                            "Failed to confirm Admin email: " +
                            string.Join(", ",
                                updateResult.Errors.Select(e => e.Description))
                        );
                    }
                }

                // Assign Admin Role
                var userRoles =
                    await userManager.GetRolesAsync(adminUser);

                if (!userRoles.Contains(adminRole))
                {
                    var addRoleResult =
                        await userManager.AddToRoleAsync(
                            adminUser,
                            adminRole
                        );

                    if (!addRoleResult.Succeeded)
                    {
                        throw new Exception(
                            "Failed to assign Admin role: " +
                            string.Join(", ",
                                addRoleResult.Errors.Select(e => e.Description))
                        );
                    }
                }

                // Verify Admin Role
                var verifiedRoles =
                    await userManager.GetRolesAsync(adminUser);

                if (!verifiedRoles.Contains(adminRole))
                {
                    throw new InvalidOperationException(
                        $"Admin role was not assigned to {adminUser.Email}."
                    );
                }

                // Seed Categories
                if (!await context.Categories.AnyAsync())
                {
                    context.Categories.AddRange(
                        new Category { Name = "Action", Description = "Action movies" },
                        new Category { Name = "Comedy", Description = "Comedy movies" },
                        new Category { Name = "Drama", Description = "Drama movies" },
                        new Category { Name = "Horror", Description = "Horror movies" },
                        new Category { Name = "Sci-Fi", Description = "Science fiction movies" },
                        new Category { Name = "Animation", Description = "Animated movies" }
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

            // =====================================
            // MIDDLEWARE
            // =====================================

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();

            // =====================================
            // ADMIN AREA ROUTE
            // =====================================

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
            ).WithStaticAssets();

            // =====================================
            // PUBLIC WEBSITE ROUTE
            // =====================================

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            ).WithStaticAssets();

            await app.RunAsync();
        }
    }
}
