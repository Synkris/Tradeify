using Core.DB;
using Core.Models;
using Hangfire;
using Hangfire.Common;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Hangfire.States;
using Hangfire.Storage;
using Logic.Helpers;
using Logic.IHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IUserHelper, UserHelper>();
builder.Services.AddScoped<IDropdownHelper, DropdownHelper>();

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("tradeifyProject")));
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("TRADEIFYProject")), ServiceLifetime.Scoped);
builder.Services.AddHangfire(x => x.UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection"))
.UseSerializerSettings(new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
GlobalJobFilters.Filters.Add(new ExpirationTimeAttribute());
JsonConvert.DefaultSettings = () =>
    new JsonSerializerSettings()
    {

        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };
builder.Services.AddHttpContextAccessor();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 3;
    options.Password.RequiredUniqueChars = 0;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
}).AddEntityFrameworkStores<AppDbContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IUserHelper, UserHelper>();
builder.Services.AddScoped<IDropdownHelper, DropdownHelper>();



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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();

static void UpdateDatabase(IApplicationBuilder app)
{
    using (var serviceScope = app.ApplicationServices
        .GetRequiredService<IServiceScopeFactory>()
        .CreateScope())
    {
        using (var context = serviceScope.ServiceProvider.GetService<AppDbContext>())
        {
            context.Database.Migrate();
        }
    }
}
void HangFireConfiguration(IApplicationBuilder app)
{
var dashboardOptions = new DashboardOptions { Authorization = new[] { new MyAuthorizationFilter() } };

var jobServerOptions = new BackgroundJobServerOptions
{
ServerName = String.Format(
    "{0}.{1}",
    Environment.MachineName,
    Guid.NewGuid().ToString())
};
app.UseHangfireServer(jobServerOptions);
var severStorage = new SqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection"));
JobStorage.Current = severStorage;
app.UseHangfireDashboard("/GGCHangfire", dashboardOptions, severStorage);
}
public class ExpirationTimeAttribute : JobFilterAttribute, IApplyStateFilter
{
    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        context.JobExpirationTimeout = TimeSpan.FromDays(20);

    }

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        context.JobExpirationTimeout = TimeSpan.FromDays(20);
    }
}

public class MyAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return true;
    }
}