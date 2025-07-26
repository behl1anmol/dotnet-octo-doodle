using System.Text.Json.Serialization;
using EFcore.API.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

//configure Serilog
var serilog = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

//configure it for MS Extensions Loggging
builder.Services.AddSerilog(serilog);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add a DbContext here. This registers the dbcontext in scoped scenario
builder.Services.AddDbContext<MoviesContext>(optionsBuilder =>
{
    var connectionString = builder.Configuration.GetConnectionString("MoviesContext");
    optionsBuilder.UseSqlServer(connectionString, sqlBuilder => sqlBuilder.MaxBatchSize(50));
    //.UseLoggerFactory() for using ILogger for logging extensions
    //.EnableSensitiveDataLogging() // for logging SQL data and parameters
    //.LogTo(Console.WriteLine); //for logging to console
    // Enable lazy loading proxies requires virtual navigation properties and nuget package Microsoft.EntityFrameworkCore.Proxies
    //.UseLazyLoadingProxies(); 

},
    ServiceLifetime.Scoped,
    ServiceLifetime.Singleton);

// builder.Services.AddDbContextPool<MoviesContext>(optionsBuilder =>
//     {
//         var connectionString = builder.Configuration.GetConnectionString("MoviesContext");
//         optionsBuilder.UseSqlServer(connectionString);
//         //.UseLoggerFactory() for using ILogger for logging extensions
//         //.EnableSensitiveDataLogging() // for logging SQL data and parameters
//         //.LogTo(Console.WriteLine); //for logging to console
//     });

var app = builder.Build();

//DIRTY HACK , we will come back and fix this
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MoviesContext>();
    var pendingMigrations = context.Database.GetPendingMigrations();
    var migrations = pendingMigrations.ToList();
    if (migrations.Any())
    {
        throw new Exception("Database is not fully migrated. Please run the migrations manually. Pending migrations: "
                            + string.Join(", ", migrations) + "");
    }
}
// the below code is still a dirty hack as it gives
// right to the app to change the schema which is not provided to 
// the rest of the application.
//await context.Database.MigrateAsync();
// context.Database.EnsureDeleted();
// context.Database.EnsureCreated();

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