
using CinemaBookingSystem.Data;
using CinemaBookingSystem.Models;
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

            var app = builder.Build();

            // Add default data to the database
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var context = services.GetRequiredService<ApplicationDbContext>();

                // Add Categories if the table is empty
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

                // Add Cinemas if the table is empty
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

            // Configure the HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
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
