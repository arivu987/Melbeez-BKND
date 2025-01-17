using Melbeez.Business;
using Melbeez.Business.Managers;
using Melbeez.Business.Managers.Abstractions;
using Melbeez.Common.Services;
using Melbeez.Common.Services.Abstraction;
using Melbeez.CustomFilters;
using Melbeez.Data;
using Melbeez.Data.Database;
using Melbeez.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

namespace Melbeez
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environments = environment;
        }

        public IConfiguration Configuration { get; }
        IWebHostEnvironment Environments { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add services to the container.

            //services.AddScoped<IHttpContextAccessor, HttpContextAccessor>();            
            services.AddTransient<IUserContextService, UserContextService>();
            services.AddTransient<IEmailSenderService, EmailSenderService>();
            services.AddTransient<ISMSSenderService, SMSSenderService>();
            services.ConfigureBusinessServices();
            services.ConfigureDataProject(Configuration);
            services.AddHostedService<AlertSMSCountService>();
            services.AddHostedService<PushNotificationService>();
            services.AddHostedService<TransferItemExpiredService>();

            services.AddMvc().AddApplicationPart(Assembly.Load(new AssemblyName("Melbeez.API.MediaUpload")));
            services.AddHttpContextAccessor();

            #region JWT Auth

            // configure jwt authentication
            string key = Configuration.GetValue<string>("JWTSecretKey");
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = true;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                };
            });

            #endregion

            #region Swagger            

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Melbeez", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });

            });
            #endregion

            services.AddControllers(options =>
            {
                if (!Environments.IsDevelopment())
                {
                    options.Filters.Add(typeof(CustomExceptionFilter));
                }
            })
           .AddJsonOptions(options =>
           {
               options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
           });

            var basePath = PlatformServices.Default.Application.ApplicationBasePath;
            var fileName = typeof(Program).GetTypeInfo().Assembly.GetName().Name + ".xml";
            string XmlCommentsFilePath = Path.Combine(basePath, fileName);

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.IncludeXmlComments(XmlCommentsFilePath, includeControllerXmlComments: true);
            });

            //services.AddCors(p => p.AddPolicy("melbeez-app-core", builder =>
            //{
            //    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader().AllowCredentials();
            //}));
            services.AddCors(options =>
            {
                options.AddPolicy("melbeez-app-core",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });
            services.Configure<Microsoft.AspNetCore.Identity.IdentityOptions>(options =>
             {
                 // Default Lockout settings.
                 options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                 options.Lockout.MaxFailedAccessAttempts = 6;
             });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseAuthentication();
            app.UseMiddleware<CheckAuthorizationMiddleware>();
            app.UseMiddleware<ApiDownMiddleware>();
            app.UseMiddleware<ApiActivityMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("./v1/swagger.json", "Melbeez v1"));

            app.UseCors("melbeez-app-core");
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthorization();

            #region Static File Server for the local files

            string uploadTo = Configuration.GetValue<string>("MediaUploadConfiguration:UploadTo");
            if (!string.IsNullOrWhiteSpace(uploadTo) && uploadTo.Equals("LOCAL"))
            {
                var basePath = Configuration.GetValue<string>("MediaUploadConfiguration:BasePath");
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }

                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(basePath),
                    RequestPath = "/MediaFiles"
                });
            }
            #endregion

            //Expose T&C File & PP
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(env.WebRootPath, "Documents")),
                RequestPath = "/policies"
            });

            // policies/PrivacyPolicy.html
            // policies/TermsAndConditions.html

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            using var loggerFactory = LoggerFactory.Create(loggingBuilder =>
            {
                loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                loggingBuilder.AddConsole();
            });

            ILogger<Program> logger = loggerFactory.CreateLogger<Program>();

            DatabaseInitialize(app, logger);
        }
        static void DatabaseInitialize(IApplicationBuilder app, ILogger<Program> logger)
        {
            logger.LogInformation("DatabaseInitialize - Start Migrate");
            Migrate(app, logger);
            logger.LogInformation("DatabaseInitialize - Complete Migrate");

            logger.LogInformation("DatabaseInitialize - Start Seed");
            Seed(app, logger);
            logger.LogInformation("DatabaseInitialize - Complete Seed");
        }
        static void Migrate(IApplicationBuilder app, ILogger<Program> logger)
        {
            logger.LogInformation("DatabaseInitialize - Migrate");
            try
            {
                var serviceScopeFactory = app.ApplicationServices.GetService<IServiceScopeFactory>();
                if (serviceScopeFactory != null)
                {
                    using var scope = serviceScopeFactory.CreateScope();
                    var services = scope.ServiceProvider;

                    var dbContext = services.GetRequiredService<ApplicationDbContext>();
                    if (dbContext.Database.GetPendingMigrations().Any())
                    {
                        logger.LogInformation("DatabaseInitialize - Migrate: Migration Pending executing migration.");
                        dbContext.Database.Migrate();
                    }

                    logger.LogInformation("DatabaseInitialize - Migrate: Completed.");
                }
                else
                {
                    logger.LogCritical("DatabaseInitialize - Migrate: Nullable ServiceScopeFactory");
                }
            }
            catch (Exception exception)
            {
                logger.LogCritical(exception, "DatabaseInitialize - Migrate: Critical: Database Migration failed.");
            }
            logger.LogInformation("DatabaseInitialize - Migrate - Exiting");
        }
        static void Seed(IApplicationBuilder app, ILogger<Program> logger)
        {
            logger.LogInformation("DatabaseInitialize - Seed: Start");
            try
            {
                var serviceScopeFactory = app.ApplicationServices.GetService<IServiceScopeFactory>();
                if (serviceScopeFactory != null)
                {
                    using var scope = serviceScopeFactory.CreateScope();
                    var services = scope.ServiceProvider;

                    logger.LogInformation("DatabaseInitialize - Seed: Getting Seed Manager.");
                    var seedManager = services.GetRequiredService<ISeedManager>();

                    logger.LogInformation("DatabaseInitialize - Seed: Wait for Seed to Complete.");
                    seedManager.SeedAsync().Wait();
                }
                else
                {
                    logger.LogCritical("DatabaseInitialize - Migrate: Nullable ServiceScopeFactory");
                }
            }
            catch (Exception exception)
            {
                logger.LogCritical(exception, "DatabaseInitialize - Seed: Critical: Database Seed failed.");
            }
            logger.LogInformation("DatabaseInitialize - Seed - Exiting");
        }
    }
}
