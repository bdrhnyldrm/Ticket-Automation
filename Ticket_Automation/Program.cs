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
                opts.Cookie.Name = ".TicketApp.auth"; // Buraya proje ad� gibi bir �ey yazabilirsin
                opts.ExpireTimeSpan = TimeSpan.FromDays(7); // 7 g�n boyunca oturum ge�erli
                opts.SlidingExpiration = true; // Kullan�c� aktifse s�re uzas�n
                opts.LoginPath = "/Account/Login"; // Giri� yap�lmazsa buraya y�nlendir
                opts.LogoutPath = "/Account/Logout"; // ��k�� i�in
                opts.AccessDeniedPath = "/Home/AccessDenied"; // Yetkisiz eri�im olursa buraya
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
