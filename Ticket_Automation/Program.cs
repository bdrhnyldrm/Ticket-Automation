using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Ticket_Automation.Entity;

namespace Ticket_Automation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
            builder.Services.AddHttpClient<GeminiService>();

            builder.Services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });


            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(opts =>
            {
                opts.Cookie.Name = ".TicketApp.auth"; // Buraya proje adý gibi bir þey yazabilirsin
                opts.ExpireTimeSpan = TimeSpan.FromDays(7); // 7 gün boyunca oturum geçerli
                opts.SlidingExpiration = true; // Kullanýcý aktifse süre uzasýn
                opts.LoginPath = "/Account/Login"; // Giriþ yapýlmazsa buraya yönlendir
                opts.LogoutPath = "/Account/Logout"; // Çýkýþ için
                opts.AccessDeniedPath = "/Home/AccessDenied"; // Yetkisiz eriþim olursa buraya
            });



            var app = builder.Build();

            SeedData.TestVerileriniDoldur(app);

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
