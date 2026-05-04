using ShopDotNet.Services;
using Dapper;

DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".ShopSession";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(2);
});

builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
builder.Services.AddSingleton<ISettingsService, SettingsService>();
builder.Services.AddScoped<ISecurityService, SecurityService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IAttributeService, AttributeService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Ensure DB is initialized at startup
app.Services.GetRequiredService<IDatabaseService>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Serve product images from configurable path (e.g. sibling PHP shop's images)
var imagesPath = builder.Configuration["ImagesPath"];
if (!string.IsNullOrEmpty(imagesPath))
{
    var physicalPath = Path.IsPathRooted(imagesPath)
        ? imagesPath
        : Path.Combine(Directory.GetCurrentDirectory(), imagesPath);

    if (Directory.Exists(physicalPath))
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(physicalPath),
            RequestPath = "/images"
        });
    }
}

app.UseRouting();
app.UseSession();
app.UseMiddleware<ShopDotNet.Middleware.AdminMiddleware>();

app.MapControllers();

app.Run();
