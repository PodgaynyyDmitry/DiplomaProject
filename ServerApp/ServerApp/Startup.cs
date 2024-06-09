using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using ServerApp.Data;
using Microsoft.OpenApi.Models;

namespace ServerApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Настройка подключения к базе данных PostgreSQL
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            // Настройка аутентификации с использованием куки
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/api/auth/login";
                    options.LogoutPath = "/api/auth/logout";
                    options.AccessDeniedPath = "/api/auth/accessdenied";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Время жизни куки
                });

            // Добавление Swagger для документирования API
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "InformationUnitsApi", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // Включаем middleware для обслуживания сгенерированного JSON документа и интерфейса Swagger UI
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "InformationUnitsApi v1");
                    c.RoutePrefix = string.Empty;  // Это сделает Swagger доступным по URL-адресу корневой директории (например, https://localhost:5001/)
                });
            }

            app.UseRouting();

            app.UseAuthentication(); // Включаем аутентификацию
            app.UseAuthorization();  // Включаем авторизацию

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

    }
}
