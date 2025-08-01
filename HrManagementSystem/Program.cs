using HrManagementSystem.Mapping;
using HrManagementSystem.Models;
using HrManagementSystem.Services;

using HrManagementSystem.UnitOfWorks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace HrManagementSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services
            builder.Services.AddControllers();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<UnitOfWork>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<SalaryReportServiceEF>();

            //builder.Services.AddHttpClient();
            builder.Services.AddHttpClient<IFaceRecognitionService, FaceRecognitionService>();

            builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50MB limit
            });




            builder.Services.AddDbContext<HRContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("Con")));
            
            builder.Services.Configure<OpenAISettings>(
                builder.Configuration.GetSection("OpenAI"));

            builder.Services.AddAutoMapper(typeof(MapConfig));

            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
            var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

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

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            builder.Services.AddAuthorization();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            // Swagger + JWT support
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "HR API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer {your JWT token}'"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                // Force Swagger to use /openapi/v1.json (fix for 404)
                app.UseSwagger(c =>
                {
                    c.RouteTemplate = "openapi/{documentName}.json";
                });

                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/openapi/v1.json", "HR API v1");
                    c.RoutePrefix = "swagger"; // serve Swagger at root (https://localhost:7124/)
                });
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                await SeedData(scope.ServiceProvider);
            }

            app.Run();
        }

        private static async Task SeedData(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var unitOfWork = serviceProvider.GetRequiredService<UnitOfWork>();

            string[] roles = { "Admin", "HR", "Employee" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
                }
            }

            // Create default department
            var defaultDepartment = unitOfWork.DepartmentRepo.getAll().FirstOrDefault();
            if (defaultDepartment == null)
            {
                defaultDepartment = new Department
                {
                    DepartmentName = "Administration",
                    Description = "Default Administration Department",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                unitOfWork.DepartmentRepo.Add(defaultDepartment);
                unitOfWork.Save();
            }

            var adminEmail = "admin@company.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    IsActive = true,
                    EmailConfirmed = true,
                    Address = "Admin Office",
                    PhoneNumber = "01000000000",
                    Role = UserRole.Admin,
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");

                    // Create Employee record for Admin
                    var adminEmployee = new Employee
                    {
                        FullName = "System Administrator",
                        Address = "Admin Office",
                        PhoneNumber = "01236547896",
                        NationalId = "12345678912124",
                        Gender = "Male",
                        HireDate = DateTime.Now,
                        Salary = 500000,
                        WorkStartTime = new TimeSpan(8, 0, 0), // 8:00 AM ,
                        WorkEndTime = new TimeSpan(16, 0, 0), // 4:00 PM,
                        DepartmentId = defaultDepartment.DepartmentId,
                        UserId = adminUser.Id,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    unitOfWork.EmployeeRepo.Add(adminEmployee);
                    unitOfWork.Save();
                }
            }
        }
    }
}