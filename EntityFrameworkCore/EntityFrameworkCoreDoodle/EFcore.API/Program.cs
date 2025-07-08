using EFcore.API.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add a DbContext here. This registers the dbcontext in scoped scenario
builder.Services.AddDbContext<MoviesContext>();

var app = builder.Build();

//DIRTY HACK , we will come back and fix this
var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<MoviesContext>();
context.Database.EnsureDeleted();
context.Database.EnsureCreated();

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