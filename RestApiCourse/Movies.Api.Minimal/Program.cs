using Microsoft.AspNetCore.Authentication.JwtBearer;
using Movies.Application;
using Movies.Application.Database;
using Asp.Versioning;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Movies.Api.Minimal.Auth;
using Movies.Api.Minimal.Endpoints;
using Movies.Api.Minimal.Health;
using Movies.Api.Minimal.Mapping;
using Movies.Api.Minimal.Swagger;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true, // who is issuing the token
        ValidateAudience = true, // who is the token intended for?
        ValidateLifetime = true, //validate the token expiration
        ValidateIssuerSigningKey = true, //for validating the signature
        ValidIssuer = config["Jwt:Issuer"],
        ValidAudience = config["Jwt:Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(config["Jwt:Key"]!))
    };
});
builder.Services.AddAuthorization(x=>
{
    x.AddPolicy(AuthConstants.AdminUserPolicyName
                , policy => policy.RequireClaim(AuthConstants.AdminUserClaimName, "true"));

    // x.AddPolicy(AuthConstants.AdminUserPolicyName
    //             , policy => policy.AddRequirements(new AdminAuthRequirement(config["ApiKey"]!)));
    x.AddPolicy(AuthConstants.TrustedUserPolicyName
               , policy => policy.RequireClaim(AuthConstants.TrustedUserClaimName, "true"));
    x.AddPolicy(AuthConstants.TrustedorAdminUserPolicyName
               , policy => policy.RequireAssertion(context =>
                   context.User.HasClaim(AuthConstants.AdminUserClaimName, "true") ||
                   context.User.HasClaim(AuthConstants.TrustedUserClaimName, "true")));
});

builder.Services.AddScoped<ApiKeyAuthFilter>();

builder.Services.AddApiVersioning(x =>
{
    x.DefaultApiVersion = new ApiVersion(1.0);
    x.AssumeDefaultVersionWhenUnspecified = true;
    x.ReportApiVersions = true; // add API supported versions to header (api-supported-versions)
    //x.ApiVersionReader = new HeaderApiVersionReader("api-version"); // read version from header (api-version)
    x.ApiVersionReader = new MediaTypeApiVersionReader("api-version"); // read version from accept header (api-version) eg: Accept: application/json;api-version=2.0
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddControllers();

//builder.Services.AddResponseCaching();
builder.Services.AddOutputCache(x=>
{
    x.AddBasePolicy(c => c.Cache());
    x.AddPolicy("MovieCache", c=>
    {
        c.Cache()
            .Expire(TimeSpan.FromMinutes(1))
            .SetVaryByQuery(new[] { "title", "year", "sortBy", "pageSize", "page" })
            .Tag("movies"); //imp: tagging allows to invalidate the cache for specific tags
    });
});
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>(DatabaseHealthCheck.Name);

//builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(x=>x.OperationFilter<SwaggerDefaultValues>());

builder.Services.AddApplication();
builder.Services.AddDatabase(config["Database:ConnectionString"]!);

builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

var app = builder.Build();

//for versioning in minimal API
app.CreateApiVersionSet();

//for minimal API
app.MapApiEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(x=>
    {
        foreach (var description in app.DescribeApiVersions())
        {
            x.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",description.GroupName);
        }
    });
}

app.MapHealthChecks("_health");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

//app.UseCors(); This should be used before response caching
//because response caching uses the request path to cache responses
//app.UseResponseCaching();
app.UseOutputCache();

app.UseMiddleware<ValidationMappingMiddleware>();
//app.MapControllers();

var dbInitializer = app.Services.GetRequiredService<DBInitializer>();
await dbInitializer.InitializeAsync(app.Lifetime.ApplicationStopping);
app.Run();

