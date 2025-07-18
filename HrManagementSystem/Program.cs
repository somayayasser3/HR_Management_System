
using HrManagementSystem.Mapping;
using HrManagementSystem.Models;
using HrManagementSystem.UnitOfWorks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HrManagementSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            string corsText = "Allow Origin";
            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddScoped < UnitOfWork>();
            builder.Services.AddDbContext<HRContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("Con")));
            builder.Services.AddAutoMapper(typeof(MapConfig));
            //builder.Services.AddAutoMapper(typeof(MapConfig).Assembly);


            builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<HRContext>()
            .AddDefaultTokenProviders();

            //var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
            //builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddCors(options =>
            {

                options.AddPolicy(corsText, builder =>
                {
                    builder.AllowAnyOrigin();
                    builder.AllowAnyMethod();
                    builder.AllowAnyHeader();
                });
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUI(op => op.SwaggerEndpoint("/openapi/v1.json", "v1"));
            }

            app.UseHttpsRedirection();
            app.UseCors(corsText);
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}