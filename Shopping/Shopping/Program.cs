using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shopping.Data;
using Shopping.Data.Entities;
using Shopping.Helpers;
using Vereyon.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<DataContext>(o =>
{
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//TODO: Make strongest password
//User configuration
builder.Services.AddIdentity<User, IdentityRole>(cfg =>
{
    cfg.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
    cfg.SignIn.RequireConfirmedEmail = true;
    cfg.User.RequireUniqueEmail = true;
    cfg.Password.RequireDigit = false;
    cfg.Password.RequiredUniqueChars = 0;
    cfg.Password.RequireLowercase = false;
    cfg.Password.RequireNonAlphanumeric = false;
    cfg.Password.RequireUppercase = false;
    cfg.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(5);
    cfg.Lockout.MaxFailedAccessAttempts = 3;
    cfg.Lockout.AllowedForNewUsers = true;
})
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<DataContext>();

//Not authorized actions configuration
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/NotAuthorized";
    options.AccessDeniedPath = "/Account/NotAuthorized";
});

//Inyections
builder.Services.AddTransient<SeedDb>(); //Database feeder
builder.Services.AddScoped<IUserHelper, UserHelper>(); //User helper
builder.Services.AddScoped<ICombosHelper, CombosHelper>(); //Combos helper
builder.Services.AddScoped<IBlobHelper, BlobHelper>(); //Blob helper
builder.Services.AddScoped<IMailHelper, MailHelper>(); //Mail helper
builder.Services.AddScoped<IOrdersHelper, OrdersHelper>(); //Orders helper

builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

builder.Services.AddFlashMessage();

var app = builder.Build();

//Seed database method
SeedData();
void SeedData()
{
    IServiceScopeFactory scopedFactory = app.Services.GetService<IServiceScopeFactory>();
    using IServiceScope scope = scopedFactory.CreateScope();
    SeedDb service = scope.ServiceProvider.GetService<SeedDb>();
    service.SeedAsync().Wait();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/error/{0}"); //Use this route for error pages

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); //Authentication for Users

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
