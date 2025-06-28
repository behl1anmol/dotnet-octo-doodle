using System;
using Dapper;

namespace Movies.Application.Repositories;

public class RatingRepository : IRatingRepository
{
     private readonly IDBConnectionFactory _dbconnectionFactory;

    public RatingRepository(IDBConnectionFactory dBConnectionFactory)
    {
        _dbconnectionFactory = dBConnectionFactory;
    }

    public async Task<float?> GetRatingAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbconnectionFactory.CreateConnectionAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<float?>(
            new CommandDefinition("SELECT ROUND(AVG(rating), 1) FROM ratings WHERE movieId = @movieId",
                new { movieId }, cancellationToken: cancellationToken));
    }

    public async Task<(float? Rating, int? UserRating)> GetRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbconnectionFactory.CreateConnectionAsync(cancellationToken);
        var result = await connection.QuerySingleOrDefaultAsync<(float? Rating, int? UserRating)>(
            new CommandDefinition("""
                SELECT ROUND(AVG(rating), 1) AS Rating, 
                       MAX(CASE WHEN userid = @userId THEN rating END) AS UserRating
                FROM ratings
                WHERE movieId = @movieId
                GROUP BY movieId
                """, new { movieId, userId }, cancellationToken: cancellationToken));
            
        // return await connection.QuerySingleOrDefaultAsync<(float? Rating, int? UserRating)>(
        //     new CommandDefinition("""
        //         SELECT ROUND(AVG(rating), 1)
        //                (Select RATING FROM ratings WHERE movieId = @movieId
        //                AND userid = @userId limit 1)
        //         FROM ratings
        //         WHERE movieId = @movieId
        //         """, new { movieId, userId }, cancellationToken: cancellationToken));
            

        return result;
    }
}
