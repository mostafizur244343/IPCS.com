using IPCS_Model.Identity;
using IPCS_Repo.Data;
using IPCS_Service.Implementation;
using IPCS_Service.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/ipcs_log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 60)
    .CreateLogger();

builder.Host.UseSerilog();

//There is Database Connection Setup(Entity Framework)
var connectionstring = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<IPCSDBContext>(options => options.UseSqlServer(connectionstring));


builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // 2. Configure Lockout Settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // 5 minute block
    options.Lockout.MaxFailedAccessAttempts = 5; // 5 times fail limit
    options.Lockout.AllowedForNewUsers = true;
})
    .AddEntityFrameworkStores<IPCSDBContext>()
    .AddDefaultTokenProviders();

//This is JWT Registration Code
// ... rest of the code

//This is JWT Registration Code

var jwtSettings = builder.Configuration.GetSection("JwtSettings");

var secretKey = jwtSettings["SecurityKey"];
if (string.IsNullOrEmpty(secretKey) || secretKey == "YOUR_SECRET_KEY_MIN_32_CHARS_LONG")
{
    secretKey = "ThisIsASecretFallbackKeyForIPCSAPI2026SecureTokenGeneration!!";
}

// Share the resolved key with the rest of the app (AccountService signs tokens
// with JwtSettings:SecurityKey) so signing and validation always use the same key.
builder.Configuration["JwtSettings:SecurityKey"] = secretKey;

var key = Encoding.UTF8.GetBytes(secretKey);


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["ValidIssuer"],
            ValidAudience = jwtSettings["ValidAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });


// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
         // Apply CamelCase for JSON serialize 
 options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IRoleService, RoleService>();


builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUOMService, UOMService>();
builder.Services.AddScoped<IManufacturerService, ManufacturerService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IGenericInfoService, GenericInfoService>();
builder.Services.AddScoped<IStoreLocationService, StoreLocationService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<IPaymentMethodService, PaymentMethodService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ILotInfoService, LotInfoService>();
builder.Services.AddScoped<IBranchLotStockService, BranchLotStockService>();
builder.Services.AddScoped<IStockLedgerService, StockLedgerService>();
builder.Services.AddScoped<IDailySummaryService, DailySummaryService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<ITransferService, TransferService>();
builder.Services.AddScoped<IProductUnitConversionService, ProductUnitConversionService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddScoped<IPurchaseReturnService, PurchaseReturnService>();
builder.Services.AddScoped<ISalesReturnService, SalesReturnService>();
builder.Services.AddScoped<IGlobalUnitConversionService, GlobalUnitConversionService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



// 1. Add CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("VercelPolicy",
        policy =>
        {
            policy.WithOrigins("https://ipcs-com-lwmf.vercel.app") //  Give Real Vercel URL
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials(); //  if use coockies then required
        });
});
var app = builder.Build();

// 2. Use Global Exception Middleware
app.UseMiddleware<IPCS_API.Middlewares.ExceptionMiddleware>();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 3. Use CORS
app.UseCors("VercelPolicy");

app.UseAuthentication();
app.UseAuthorization();

// Serve static files from wwwroot
app.UseStaticFiles();

app.MapControllers();

// Apply pending EF Core migrations at startup so a fresh database gets its
// schema automatically (the app never ran Migrate() before). Guarded: if the
// database is unreachable, log and continue — the API still starts, and the
// migration runs again on the next restart.
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<IPCSDBContext>();
        await dbContext.Database.MigrateAsync();
    }
}
catch (Exception ex)
{
    Log.Error(ex, "Database migration failed at startup. The API will start without an up-to-date schema; check the database connection.");
}

// Automatic Permission Seeding on Startup
// Wrapped in try/catch: if the database is unreachable at startup, seeding must
// not crash the whole app (otherwise Render enters a crash loop and the API
// never answers any request). The API still starts; DB-backed calls fail
// individually until the database is back, and seeding re-runs on next restart.
try
{
    using (var scope = app.Services.CreateScope())
    {
        var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
        await permissionService.SeedPermissionsAsync();
    }
}
catch (Exception ex)
{
    Log.Error(ex, "Permission seeding failed at startup. The API will start without seeded permissions; check the database connection.");
}

// Automatic SuperAdmin Seeding on Startup
try
{
    using (var scope = app.Services.CreateScope())
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        const string superAdminRole = "SuperAdmin";
        const string superAdminEmail = "superadmin@ipcs.com";
        const string superAdminPassword = "Admin@1234";

        // 1. Create SuperAdmin role if it doesn't exist
        if (!await roleManager.RoleExistsAsync(superAdminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(superAdminRole));
        }

        // 2. Create SuperAdmin user if it doesn't exist
        var existingUser = await userManager.FindByEmailAsync(superAdminEmail);
        if (existingUser == null)
        {
            var superAdminUser = new User
            {
                UserName = "superadmin",
                Email = superAdminEmail,
                FullName = "Super Administrator",
                IsActive = true,
                EmailConfirmed = true,
                JoiningDate = DateTime.Now
            };

            var result = await userManager.CreateAsync(superAdminUser, superAdminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(superAdminUser, superAdminRole);
            }
        }
    }
}
catch (Exception ex)
{
    Log.Error(ex, "SuperAdmin seeding failed at startup. The API will start without the seeded SuperAdmin account; check the database connection.");
}

app.Run();
