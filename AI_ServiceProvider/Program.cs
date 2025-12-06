using AI_ServiceProvider.Data;
using AI_ServiceProvider.Controllers.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AI_ServiceProvider
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // === 1. ADD SERVICES TO THE CONTAINER ===

            // Add DbContext
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Add Authentication (JWT)
            // IMPORTANT: In a real app, store this securely in User Secrets or Azure Key Vault
            var jwtKey = builder.Configuration.GetValue<string>("JwtSettings:Key") ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
            var jwtKeyBytes = Encoding.ASCII.GetBytes(jwtKey);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(jwtKeyBytes),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            
            builder.Services.AddScoped<IGoogleDriveService, GoogleDriveService>();
      
            // builder.Services.AddScoped<IImageParsingService, YourImageParsingServiceImplementation>();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

           

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication(); 

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}