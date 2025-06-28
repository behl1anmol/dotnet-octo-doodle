using System;
using Dapper;

namespace Movies.Application.Database;

public class DBInitializer
{
    private readonly IDBConnectionFactory _dbConnectionFactory;

    public DBInitializer(IDBConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        await connection.ExecuteAsync("""
            create table if not exists movies (
            id UUID primary key,
            slug TEXT not null,
            title TEXT not null,
            yearofrelease integer not null);
            """);

        await connection.ExecuteAsync("""
            create unique index concurrently if not exists movies_slug_idx
            on movies
            using btree(slug);
            """
        );

        await connection.ExecuteAsync("""
            create table if not exists genres (
            movieId UUID references movies(id),
            name TEXT not null);
            """
        );
    }
}
