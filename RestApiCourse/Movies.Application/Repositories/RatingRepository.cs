using System;
using Dapper;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public class RatingRepository : IRatingRepository
{
     private readonly IDBConnectionFactory _dbconnectionFactory;

    public RatingRepository(IDBConnectionFactory dBConnectionFactory)
    {
        _dbconnectionFactory = dBConnectionFactory;
    }

    public async Task<bool> RateMovieAsync(Guid movieId, int rating, Guid userId,
        CancellationToken cancellationToken = default)
    {
        using var connection = await _dbconnectionFactory.CreateConnectionAsync(cancellationToken);
        var rowsAffected = await connection.ExecuteAsync(
            new CommandDefinition("""
                INSERT INTO ratings (userid, movieid, rating)
                VALUES (@userId, @movieId, @rating)
                ON CONFLICT (userid, movieid) DO UPDATE
                SET rating = @rating
                """, new { userId, movieId, rating }, 
                cancellationToken: cancellationToken));
        
        return rowsAffected > 0;
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

    public async Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbconnectionFactory.CreateConnectionAsync(cancellationToken);
        var result = await connection.ExecuteAsync(new CommandDefinition("""
                                                                         delete from ratings where 
                                                                         movieid = @movieId and userid = @userId;
                                                                         """, new { movieId, userId }, cancellationToken: cancellationToken));
        return result > 0;
    }

    public async Task<IEnumerable<MovieRating>> GetRatingsForUserAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        using var connection = await _dbconnectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QueryAsync<MovieRating>(
            new CommandDefinition("""
                SELECT r.rating, r.movieid, m.slug
                FROM ratings r 
                join movies m on r.movieid = m.id
                WHERE userid = @userId
                """, new { userId }, cancellationToken: cancellationToken));
    }
}
