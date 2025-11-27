using Microsoft.EntityFrameworkCore;
using DatabaseMigrations.MigrationService;
using Backend.API.Infrastructure;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Initializer>();

// builder.AddServiceDefaults();

builder.Services.AddDbContextPool<DatabaseContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("data"), sqlOptions =>
        sqlOptions.MigrationsAssembly("Migrations")
    );

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
});

var app = builder.Build();

app.Run();
