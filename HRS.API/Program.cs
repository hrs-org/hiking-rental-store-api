using System.Text;
using System.IdentityModel.Tokens.Jwt;
using FluentValidation;
using HRS.API.Configuration;
using HRS.API.Filters;
using HRS.API.Middleware;
using HRS.API.Services;
using HRS.API.Services.Interfaces;
using HRS.API.Validators.Store;
using HRS.API.Validators.Auth;
using HRS.API.Validators.Item;
using HRS.API.Validators.Maintenance;
using HRS.API.Validators.Payment;
using HRS.API.Validators.Rental;
using HRS.API.Validators.User;
using HRS.Domain.Interfaces;
using HRS.Infrastructure;
using HRS.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Hangfire;
using Hangfire.MySql;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailBuilderService, EmailBuilderService>();
builder.Services.AddScoped<IEmailSenderService, EmailSenderService>();
builder.Services.AddScoped<IUserVerificationService, UserVerificationService>();
builder.Services.AddScoped<IUserSessionService, UserSessionService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IPackageService, PackageService>();
builder.Services.AddScoped<IRentalOrderService, RentalOrderService>();
builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();
builder.Services.AddScoped<IItemMaintenanceService, ItemMaintenanceService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPendingPaymentCleanupService, PendingPaymentCleanupService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IStoreService, StoreService>();

builder.Services.AddScoped(typeof(ICrudRepository<>), typeof(CrudRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IStoreRepository, StoreRepository>();
builder.Services.AddScoped<IUserSessionRepository, UserSessionRepository>();
builder.Services.AddScoped<IUserVerificationRepository, UserVerificationRepository>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IItemRateRepository, ItemRateRepository>();
builder.Services.AddScoped<IPackageRepository, PackageRepository>();
builder.Services.AddScoped<IPackageRateRepository, PackageRateRepository>();
builder.Services.AddScoped<IRentalOrderRepository, RentalOrderRepository>();
builder.Services.AddScoped<IRentalOrderItemRepository, RentalOrderItemRepository>();
builder.Services.AddScoped<IRentalOrderPackageItemRepository, RentalOrderPackageItemRepository>();
builder.Services.AddScoped<IItemMaintenanceRepository, ItemMaintenanceRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IAppConfiguration, AppConfiguration>();
builder.Services.AddHttpContextAccessor();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers(options => { options.Filters.Add<ValidationFilter>(); });

builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AddItemRequestDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateItemRequestDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterEmployeeDetailDtoValidators>();
builder.Services.AddValidatorsFromAssemblyContaining<ChangePasswordRequestDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RentalOrderRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ReturnRentalOrderRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<PaymentRequestDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<VerifyPaymentRequestDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ItemMaintenanceRequestDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<StoreOnboardingRequestDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AssignCustomerRoleDtoValidator>();

builder.Services.Configure<Auth0Options>(builder.Configuration.GetSection("Auth0"));
builder.Services.AddHttpClient<IAuth0ManagementService, Auth0ManagementService>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HRS API", Version = "v1" });

    // 🔑 Enable JWT Bearer in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your JWT token.\n\nExample: **Bearer eyJhbGciOi...**"
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
            []
        }
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString,
        ServerVersion.AutoDetect(connectionString),
        b => b.MigrationsAssembly("HRS.Migrations")));

builder.Services.AddAutoMapper(cfg => { }, typeof(Program));

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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    })
    .AddJwtBearer("Auth0", options =>
    {
        var domain = builder.Configuration["Auth0:Domain"];
        var audience = builder.Configuration["Auth0:Audience"];

        options.Authority = string.IsNullOrWhiteSpace(domain) ? string.Empty : $"https://{domain}/";
        options.Audience = audience;
        options.IncludeErrorDetails = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            NameClaimType = "sub"
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("Auth0Jwt");

                var authHeader = context.Request.Headers.Authorization.ToString();
                var token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? authHeader["Bearer ".Length..].Trim()
                    : string.Empty;

                string? tokenIssuer = null;
                string? tokenAudience = null;
                if (!string.IsNullOrWhiteSpace(token))
                {
                    try
                    {
                        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
                        tokenIssuer = jwt.Issuer;
                        tokenAudience = string.Join(",", jwt.Audiences);
                    }
                    catch
                    {
                        // Ignore token parsing errors and log only validation exception below.
                    }
                }

                logger.LogError(
                    context.Exception,
                    "Auth0 JWT validation failed. ExpectedAudience={ExpectedAudience}; Authority={Authority}; TokenIssuer={TokenIssuer}; TokenAudience={TokenAudience}",
                    audience,
                    options.Authority,
                    tokenIssuer,
                    tokenAudience
                );

                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("Auth0Jwt");

                logger.LogWarning(
                    "Auth0 JWT challenge triggered. Error={Error}; ErrorDescription={ErrorDescription}; ErrorUri={ErrorUri}",
                    context.Error,
                    context.ErrorDescription,
                    context.ErrorUri
                );

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebClient", policy =>
        policy.WithOrigins(
                builder.Configuration["AllowedOrigins"]?.Split(',') ?? Array.Empty<string>()
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    );
});

builder.Services.AddHangfire(config =>
    config.UseStorage(new MySqlStorage(
        connectionString,
        new Hangfire.MySql.MySqlStorageOptions()
    ))
);
builder.Services.AddHangfireServer();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
if (app.Environment.IsDevelopment()) app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("AllowWebClient");
app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard();

app.MapControllers();

app.Run();
