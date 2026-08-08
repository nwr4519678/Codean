using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Platform.Domain.Results;

namespace Platform.Application.Common.Behaviors;

/// <summary>
/// Runs all registered FluentValidation validators for a request and returns
/// a single Error.Validation result with all failures joined.
/// </summary>
public sealed class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any()) return await next();

        var ctx = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(ctx, cancellationToken)));
        var failures = results.SelectMany(r => r.Errors).Where(f => f is not null).ToList();
        if (failures.Count == 0) return await next();

        var data = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(g => g.Key, g => (object?)g.Select(x => x.ErrorMessage).ToArray());

        var error = Error.Validation("validation.failed", "One or more validation errors occurred.", data);
        return ResultHelper.CreateFailure<TResponse>(error);
    }
}
