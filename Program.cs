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

builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddSingleton<SettingsService>();
builder.Services.AddScoped<SecurityService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Ensure DB is initialized at startup
app.Services.GetRequiredService<DatabaseService>();

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

app.MapControllers();

app.Run();
