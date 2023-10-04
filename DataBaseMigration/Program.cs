using DataBaseMigration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
if (args.Contains("--migrate", StringComparer.OrdinalIgnoreCase))
{
    var appSettingsPath = "appsettings.json"; // Assuming the file is in the same directory as the executable
    Console.WriteLine($"Loading appsettings from: {appSettingsPath}");

    var configuration = new ConfigurationBuilder()
        .AddJsonFile(appSettingsPath)
        .Build();

    var connectionString = configuration.GetConnectionString("SqlDb");
    Console.WriteLine($"Connection string: {connectionString}");

    DatabaseMigrationConfiguration.ConfigureAndMigrate(configuration);
}

else
{
    Console.WriteLine("Database migration skipped. To run the migration, use the --migrate command-line argument.");
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
