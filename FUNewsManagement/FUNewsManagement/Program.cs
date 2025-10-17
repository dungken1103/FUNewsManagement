using Microsoft.EntityFrameworkCore;
using BusinessLayer.Interfaces;
using BusinessLayer.Services;
using DataAccessLayer.Repositories;
using DataAccessLayer.Repositories.Interfaces;

namespace FUNewsManagement;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        builder.Services.AddAuthentication("MyCookieAuth").AddCookie("MyCookieAuth", options =>
        {
            options.LoginPath = "/Account/Login";
            options.AccessDeniedPath = "/Account/AccessDenied";
        });

        builder.Services.AddScoped<IAccountService, AccountService>();
        builder.Services.AddScoped<ICategoryService, CategoryService>();
        builder.Services.AddScoped<INewsService, NewsService>();
        builder.Services.AddScoped<ITagService, TagService>();

        builder.Services.AddScoped<IAccountRepository, AccountRepository>();
        builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
        builder.Services.AddScoped<INewsRepository, NewsRepository>();
        builder.Services.AddScoped<ITagRepository, TagRepository>();

        builder.Services.AddDbContext<DataAccessLayer.Data.FUNewsManagementContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    
    app.UseAuthentication();
    app.UseAuthorization();
        app.UseSession();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}
