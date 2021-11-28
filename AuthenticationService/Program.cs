using AuthenticationServer.Logic;
using AuthenticationServer.Logic.DAL;
using AuthenticationServer.Logic.Enum;
using AuthenticationServer.Logic.Services;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

IConfiguration? configuration = builder.Services.BuildServiceProvider().GetRequiredService<IConfiguration>();
string connection = configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connection));


builder.Services.AddIdentity<IdentityUser, IdentityRole>(config =>
{
    config.Password.RequiredLength = 4;
    config.Password.RequireDigit = false;
    config.Password.RequireNonAlphanumeric = false;
    config.Password.RequireUppercase = false;
}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();


builder.Services.ConfigureApplicationCookie(config =>
{
    config.LoginPath = "/Account/Login";
});

var assembly = typeof(Program).Assembly.GetName().Name;

builder.Services.AddIdentityServer()
    .AddAspNetIdentity<IdentityUser>()
    .AddConfigurationStore(options =>
    {
        options.ConfigureDbContext = b => b.UseSqlServer(connection,
            sql => sql.MigrationsAssembly(assembly));
    })
    .AddOperationalStore(options =>
    {
        options.ConfigureDbContext = b => b.UseSqlServer(connection,
            sql => sql.MigrationsAssembly(assembly));
    }).
    AddDeveloperSigningCredential()
    .AddProfileService<ProfileService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
    scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>().Database.Migrate();
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
    await Configuration.CreateUsersAndRoles(scope.ServiceProvider);
    var context = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
    context.LoadMockData();


}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseIdentityServer();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
