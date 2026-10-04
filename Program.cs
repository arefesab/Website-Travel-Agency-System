using agancywebProject.Data;
using agancywebProject.Services;
using agancywebProject.Services.Payment;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using agancywebProject.Helpers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews().AddDataAnnotationsLocalization();

builder.Services.AddOptions<MvcDataAnnotationsLocalizationOptions>()
    .Configure<IHttpContextAccessor>((options, accessor) =>
    {
        options.DataAnnotationLocalizerProvider = (modelType, factory) => new CookieStringLocalizer(accessor);
    });

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnectionString")
));

builder.Services.AddHttpClient("zarinpal");
builder.Services.AddScoped<IPaymentGateway>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var provider = config["Payment:Provider"] ?? "Fake";
    if (string.Equals(provider, "Zarinpal", StringComparison.OrdinalIgnoreCase))
    {
        return new ZarinpalGateway(sp.GetRequiredService<IHttpClientFactory>(), config);
    }
    return new FakePaymentGateway();
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Login";
        options.AccessDeniedPath = "/Admin/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// First run on an empty database: create the admin user and load SeedData/seed.json
using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(scope.ServiceProvider, app.Environment, app.Configuration);
}

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
