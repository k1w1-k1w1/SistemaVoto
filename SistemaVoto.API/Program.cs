using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SistemaVoto.API.Data;
using SistemaVoto.API.Services;
using System.Text;

namespace SistemaVoto.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configurar DbContext con PostgreSQL
            builder.Services.AddDbContext<VotacionDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Registrar servicios
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<ISMSService, SMSService>();
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IPdfService, PdfService>();


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowWeb", policy =>
                    policy.WithOrigins("http://localhost:5268", "https://localhost:7023")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials()
                );
            });

            // Configurar JWT
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                    };
                });

            builder.Services.AddAuthorization();

            // Configurar Swagger con soporte JWT
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Sistema de Votación API",
                    Version = "v1"
                });

                // Agregar soporte de JWT en Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Ingresa el token JWT así: Bearer {tu_token}"
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

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            var app = builder.Build();

            // Configure the HTTP request pipeline
            //if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            

            app.UseCors("AllowWeb");


            app.UseAuthentication();  
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}