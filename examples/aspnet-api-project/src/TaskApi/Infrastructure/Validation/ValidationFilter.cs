using FluentValidation;

namespace TaskApi.Infrastructure.Validation;

/// <summary>
/// An endpoint filter that validates the <typeparamref name="TRequest"/> argument with its registered
/// FluentValidation validator before the handler runs, short-circuiting with an RFC 9457 validation
/// problem (HTTP 400) when validation fails. A validation failure is an expected outcome, so it returns
/// from here and never reaches the global exception handler.
/// </summary>
/// <typeparam name="TRequest">The request DTO to validate.</typeparam>
internal sealed class ValidationFilter<TRequest>(IValidator<TRequest> validator) : IEndpointFilter
{
    /// <inheritdoc />
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<TRequest>().FirstOrDefault();
        if (request is not null)
        {
            var result = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);
            if (!result.IsValid)
            {
                return TypedResults.ValidationProblem(result.ToDictionary());
            }
        }

        return await next(context);
    }
}
