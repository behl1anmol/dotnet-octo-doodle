using FluentValidation;

namespace Movies.Api.Minimal.Mapping;

public class ValidationMappingMiddleware
{
    private readonly RequestDelegate _next;

    public ValidationMappingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        //catch the validation exception and return with an appropriate contract
        catch (ValidationException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            var ValidationFailureResponse = new Movies.Contracts.Responses.ValidationFailureResponse
            {
                Errors = ex.Errors.Select(error => new Contracts.Responses.ValidationResponse
                {
                    PropertyName = error.PropertyName,
                    Message = error.ErrorMessage
                })
            };
            await context.Response.WriteAsJsonAsync(ValidationFailureResponse);
        }
    }
}
