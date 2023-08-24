using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using serverMaestro.Data;
namespace serverMaestro
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<MaestroContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("MaestroContext") ?? throw new InvalidOperationException("Connection string 'MaestroContext' not found.")));

            // Add services to the container.

            var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(MyAllowSpecificOrigins,
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:3000")
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });

            builder.Services.AddControllers();

            //builder.Services
            //.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            //.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            //{
            //    options.SlidingExpiration = true;
            //    options.ExpireTimeSpan = new TimeSpan(0, 1, 0);
            //});


            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseCors(MyAllowSpecificOrigins);

            app.UseAuthorization();
            //app.UseAuthentication();

            app.MapControllers();

            app.Run();
        }
    }
}