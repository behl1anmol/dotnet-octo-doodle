using Microsoft.AspNetCore.Authentication.JwtBearer;
using Movies.API.Mapping;
using Movies.Application;
using Movies.Application.Database;
using Movies.API.Auth;
using Asp.Versioning;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Movies.Api.Swagger;

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
    x.AddPolicy(AuthConstants.TrustedUserPolicyName
               , policy => policy.RequireClaim(AuthConstants.TrustedUserClaimName, "true"));
    x.AddPolicy(AuthConstants.TrustedorAdminUserPolicyName
               , policy => policy.RequireAssertion(context =>
                   context.User.HasClaim(AuthConstants.AdminUserClaimName, "true") ||
                   context.User.HasClaim(AuthConstants.TrustedUserClaimName, "true")));
});

builder.Services.AddApiVersioning(x =>
{
    x.DefaultApiVersion = new ApiVersion(1.0);
    x.AssumeDefaultVersionWhenUnspecified = true;
    x.ReportApiVersions = true; // add API supported versions to header (api-supported-versions)
    //x.ApiVersionReader = new HeaderApiVersionReader("api-version"); // read version from header (api-version)
    x.ApiVersionReader = new MediaTypeApiVersionReader("api-version"); // read version from accept header (api-version) eg: Accept: application/json;api-version=2.0
}).AddMvc().AddApiExplorer();
builder.Services.AddControllers();

builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(x=>x.OperationFilter<SwaggerDefaultValues>());

builder.Services.AddApplication();
builder.Services.AddDatabase(config["Database:ConnectionString"]!);


var app = builder.Build();

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

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ValidationMappingMiddleware>();
app.MapControllers();

var dbInitializer = app.Services.GetRequiredService<DBInitializer>();
await dbInitializer.InitializeAsync(app.Lifetime.ApplicationStopping);
app.Run();

