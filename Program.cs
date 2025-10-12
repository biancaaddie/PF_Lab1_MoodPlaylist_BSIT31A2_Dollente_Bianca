using Microsoft.EntityFrameworkCore;
using MoodPlaylistGenerator.Data;
using MoodPlaylistGenerator.Services.Interfaces;
using MoodPlaylistGenerator.Services.Implementations;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add SQLite database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=MoodPlaylist.db"));

// Add services
// OPTION 1: Use SQLite implementation (Code-First with Entity Framework)
builder.Services.AddScoped<MoodPlaylistGenerator.Services.Interfaces.IAuthService, MoodPlaylistGenerator.Services.Implementations.SQLiteAuthService>();

// OPTION 2: Use In-Memory implementation (List-based for learning/testing)
// Uncomment the line below and comment out the line above to switch
// builder.Services.AddSingleton<IAuthService, InMemoryAuthService>();

builder.Services.AddScoped<MoodPlaylistGenerator.Services.SongService>();
builder.Services.AddScoped<MoodPlaylistGenerator.Services.PlaylistService>();

// Register MediaUploadService for local media functionality
builder.Services.AddScoped<MoodPlaylistGenerator.Services.IMediaUploadService, MoodPlaylistGenerator.Services.MediaUploadService>();

// Add authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// Ensure database is created and migrated
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
