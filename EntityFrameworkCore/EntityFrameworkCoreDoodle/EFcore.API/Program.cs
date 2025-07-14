using System.Text.Json.Serialization;
using EFcore.API.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add a DbContext here. This registers the dbcontext in scoped scenario
builder.Services.AddDbContext<MoviesContext>();

var app = builder.Build();

//DIRTY HACK , we will come back and fix this
var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<MoviesContext>();
var pendingMigrations = context.Database.GetPendingMigrations();
if (pendingMigrations.Any())
{
    throw new Exception("Database is not fully migrated. Please run the migrations manually. Pending migrations: " 
                        + string.Join(", ", pendingMigrations) + "");
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