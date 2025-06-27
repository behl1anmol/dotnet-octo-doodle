using System;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Movies.Application.Database;
using Movies.Application.Repositories;
using Movies.Application.Services;

namespace Movies.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IMovieRepository, MovieRepository>();
        services.AddSingleton<IMovieService, MovieService>();

        //we need to pass a class of interface that exists within this assembly
        //this is used to register all validators in the assembly
        //a better way  is to use a marker interface to identify the assembly
        //this is a common pattern in .NET applications
        services.AddValidatorsFromAssemblyContaining<IApplicationMarker>(ServiceLifetime.Singleton);
        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services
                                    , string connectionString)
    {
        services.AddSingleton<IDBConnectionFactory>(_ =>
                         new NpgsqlConnectionFactory(connectionString));
        services.AddSingleton<DBInitializer>();

        return services;     
    }
}
