using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using WalletDiet.Data;
using WalletDiet.Models;
using WalletDiet.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using WalletDiet.Services;

namespace WalletDiet
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

            builder.Services.AddTransient<EmailService>();

            builder.Services.AddAuthentication(
                CookieAuthenticationDefaults.AuthenticationScheme
            )
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccesDenied";
            });

            var app = builder.Build();


            /* Seeding Account */
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<AppDbContext>();

                await context.Database.MigrateAsync();

                var existingUser = await context.Users
                    .Select(u => u.Username)
                    .ToListAsync();

                var hasher = new PasswordHasher<User>();

                var user = new User
                {
                    Username = "",
                    Role = "",
                    Email = ""
                };

                var admin = new User
                {
                    Username = "",
                    Role = "",
                    Email = ""
                };

                user.PasswordHash = hasher.HashPassword(user, "");
                admin.PasswordHash = hasher.HashPassword(admin, "");

                var users = new List<User>
                {
                    user,
                    admin
                };

                var newUsers = users
                    .Where(r => !existingUser.Contains(r.Username))
                    .ToList();

                if (newUsers.Count > 0)
                {
                    await context.Users.AddRangeAsync(newUsers);
                    await context.SaveChangesAsync();
                }
            }

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
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}")
                .WithStaticAssets();

            app.MapControllers();

            app.Run();
        }
    }
}
