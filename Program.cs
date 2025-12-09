//
// Kisse, a simple multi-user web application to record cat observations (with locations and uploaded photos).
// ASP.NET 10, with mostly server-side rendered frontend using Htmx.
//
using System.CommandLine;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Kisse.Data;
using Microsoft.Extensions.FileProviders;
using TupleAsJsonArray;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new TupleConverterFactory()))
    .AddMvcOptions(o => o.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
// Upload directory (separate from wwwroot)
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.GetFullPath(app.Configuration["Upload:Path"]!.TrimEnd('/'))),
    RequestPath = app.Configuration["Upload:URL"]!.TrimEnd('/')
});
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

// allow several actions on command line
RootCommand rootCommand = new RootCommand("Kisse, a simple web app for mapping out observations of outdoor cats.");

// migration command to use in production
Command migrateCommand = new Command("migrate", "Apply all database migrations.");
migrateCommand.SetAction(async _ =>
{
    var context = app.Services.CreateScope().ServiceProvider.GetService<ApplicationDbContext>();
    await context!.Database.MigrateAsync();
});
rootCommand.Subcommands.Add(migrateCommand);

// command to add a new user, registration through UI is disabled
var usernameOption = new Option<string>("--username")
{
    Description = "The username of the user.",
    Required = true
};
var emailOption = new Option<string>("--email")
{
    Description = "The email of the user.",
    Required = true
};
var passwordOption = new Option<string>("--password")
{
    Description = "The password (you might want to be careful to make sure it's not saved in shell history).",
    Required = true
};
Command addUserCommand = new Command("add-user", "Add a user to the database.")
{
    usernameOption,
    emailOption,
    passwordOption
};
addUserCommand.SetAction(async result =>
{
    var user = new IdentityUser
    {
        Email = result.GetValue(emailOption),
        UserName = result.GetValue(usernameOption),
        EmailConfirmed = true
    };
    var manager = app.Services.CreateScope().ServiceProvider.GetService<UserManager<IdentityUser>>()!;
    var createResult = await manager.CreateAsync(user, result.GetValue(passwordOption)!);
    if (!createResult.Succeeded)
    {
        Console.Error.WriteLine(createResult.Errors.FirstOrDefault()?.Description);
        return 1;
    }

    return 0;
});
rootCommand.Subcommands.Add(addUserCommand);

// command to change user's password, this also shouldn't be available in UI
Command setPasswordCommand = new Command("set-password", "Change user's password in the database.")
{
    usernameOption,
    passwordOption
};
setPasswordCommand.SetAction(async result =>
{
    var manager = app.Services.CreateScope().ServiceProvider.GetService<UserManager<IdentityUser>>()!;
    var user = await manager.FindByNameAsync(result.GetValue(usernameOption)!);
    if (user is null)
    {
        Console.Error.WriteLine("User not found.");
        return 1;
    }
    var token = await manager.GeneratePasswordResetTokenAsync(user);
    var resetResult = await manager.ResetPasswordAsync(user, token, result.GetValue(passwordOption)!);
    if (!resetResult.Succeeded)
    {
        Console.Error.WriteLine(resetResult.Errors.FirstOrDefault()?.Description);
        return 1;
    }
    return 0;
});
rootCommand.Subcommands.Add(setPasswordCommand);

// default is to run the app, go ahead
rootCommand.SetAction(async _ => await app.RunAsync());
return await rootCommand.Parse(args).InvokeAsync();