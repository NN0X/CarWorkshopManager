using QuestPDF.Infrastructure;
using QuestPDF;
using CarWorkshopManager.Data;
using CarWorkshopManager.Mappers;
using CarWorkshopManager.Models.Identity;
using CarWorkshopManager.Services.Implementations;
using CarWorkshopManager.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 10;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddSingleton<CustomerMapper>();
builder.Services.AddSingleton<ServiceOrderMapper>();
builder.Services.AddSingleton<PartMapper>();
builder.Services.AddSingleton<UsedPartMapper>();
builder.Services.AddSingleton<ServiceTaskMapper>();
builder.Services.AddSingleton<IEmailWithAttachmentSender, SendGridEmailWithAttachmentSender>();

builder.Services.AddScoped<IUsernameGeneratorService, UsernameGeneratorService>();
builder.Services.AddScoped<IUserRegistrationService, UserRegistrationService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IServiceOrderService, ServiceOrderService>();
builder.Services.AddScoped<IOrderCommentService, OrderCommentService>();
builder.Services.AddScoped<IVatRateService, VatRateService>();
builder.Services.AddScoped<IPartService, PartService>();
builder.Services.AddScoped<IWorkRateService, WorkRateService>();
builder.Services.AddScoped<IServiceTaskService, ServiceTaskService>();

builder.Services.AddHostedService<OpenOrdersReportBackgroundService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();

    await DomainSeeder.SeedAsync(dbContext);
    await IdentitySeeder.SeedAsync(services);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
