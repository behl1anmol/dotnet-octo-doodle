using System;

namespace Movies.API;

public static class ApiEndpoints
{
    private const string ApiBase = "api";
    public static class Movies
    {
        private const string Base = $"{ApiBase}/movies";
        public const string Create = Base;
        public const string Get = $"{Base}/{{idOrSlug}}"; //added a constraint to only accept GUID 
        public const string GetAll = Base;
        public const string Update = $"{Base}/{{id:guid}}"; //added a constraint to only accept GUID 
        public const string Delete = $"{Base}/{{id:guid}}"; //added a constraint to only accept GUID 
        public const string Rate = $"{Base}/{{id:guid}}/ratings"; //added a constraint to only accept GUID 
        public const string DeleteRating = $"{Base}/{{id:guid}}/ratings"; //added a constraint to only accept GUID 
    }

    public static class Ratings
    {
        private const string Base = $"{ApiBase}/ratings";
        public const string GetUserRatings = $"{Base}/me";
    }
}
