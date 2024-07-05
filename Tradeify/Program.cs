using Core.Config;
using Core.DB;
using Core.Models;
using Hangfire;
using Hangfire.Common;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Hangfire.States;
using Hangfire.Storage;
using Logic.Helper;
using Logic.Helpers;
using Logic.IHelper;
using Logic.IHelpers;
using Logic.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IUserHelper, UserHelper>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IDropdownHelper, DropdownHelper>();
builder.Services.AddScoped<IEmailHelper, EmailHelper>();
builder.Services.AddScoped<IPackageHelper, PackageHelper>();
builder.Services.AddScoped<IAdminHelper, AdminHelper>();
builder.Services.AddScoped<IPaymentHelper, PaymentHelper>();
builder.Services.AddScoped<IBinaryHelper, BinaryHelper>();



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
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(43800);
});

builder.Services.AddSingleton<IEmailConfiguration>(builder.Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>());
builder.Services.AddSingleton<IGeneralConfiguration>(builder.Configuration.GetSection("GeneralConfiguration").Get<GeneralConfiguration>());
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddScoped<IRegistrationPackageHelper, RegistrationPackageHelper>();
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IUserHelper, UserHelper>();
builder.Services.AddScoped<IEmailHelper, EmailHelper>();
builder.Services.AddScoped<IUserHelper, UserHelper>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IDropdownHelper, DropdownHelper>();
builder.Services.AddScoped<IBonusHelper, BonusHelper>();
//builder.Services.AddScoped<IEmailHelper, EmailHelper>();


//Add support to logging with SERILOG
builder.Logging.AddSerilog();
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("GGC", Serilog.Events.LogEventLevel.Information)
     .ReadFrom.Configuration(builder.Configuration)
     .WriteTo.Console()
     .WriteTo.File("Logs/applog-.txt", rollingInterval: RollingInterval.Day)
     .Enrich.FromLogContext()
     .CreateLogger();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/AccessDenied");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/AccessDenied");
    app.UseHsts();
}

app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;

    if (response.StatusCode == (int)HttpStatusCode.Unauthorized)
        response.Redirect("/Account/Login");

    if (response.StatusCode == (int)HttpStatusCode.NotFound)
        response.Redirect("/Home/AccessDenied");

    if (response.StatusCode == (int)HttpStatusCode.Forbidden)
        response.Redirect("/Home/AccessDenied");

    if (response.StatusCode == (int)HttpStatusCode.BadGateway)
        response.Redirect("/Home/AccessDenied");

    if (response.StatusCode == (int)HttpStatusCode.InternalServerError)
        response.Redirect("/Home/AccessDenied");

    if (response.StatusCode == (int)HttpStatusCode.RequestTimeout)
        response.Redirect("/Home/AccessDenied");
});
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.Redirect("/Home/AccessDenied");
    });
});

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseStaticFiles();
//Add support to logging request with SERILOG
//app.UseSerilogRequestLogging();
app.UseRouting();
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.UseSession();
UpdateDatabase(app);

UpdateDatabase(app);
HangFireConfiguration(app);
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
pattern: "{controller=Home}/{action=Index}/{id?}");
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    try
//    {
//        var _binaryHelper = services.GetRequiredService<IBinaryHelper>();
//        _binaryHelper.AutoAssignOldUsers();
//    }
//    catch (Exception ex)
//    {
//        var logger = services.GetRequiredService<ILogger<Program>>();
//        logger.LogError(ex, "An error occurred while see the database.");
//    }
//}
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
app.UseHangfireDashboard("/TRADEIFYHangfire", dashboardOptions, severStorage);
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